import json
import logging
from logging import root
from os.path import exists
from confluent_kafka import Consumer
from shapely.geometry import Point, Polygon
import redis
import pika
from kafka import KafkaConsumer
import geopandas as gpd
import os

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

REDIS_HOST = "localhost"
REDIS_PORT = 6379
REDIS_TTL_SECONDS = 3600

KAFKA_BOOTSTRAP_SERVERS = ["localhost:9092"]
KAFKA_TOPIC = "alerts-topic"
KAFKA_GROUP_ID = "classification-service-group"

RABBITMQ_HOST = "localhost"

def get_region_with_geopandas(lon: float, lat: float) -> str:
    # 1. טעינת קובץ ה-GeoJSON ל-GeoDataFrame
    gdf = gpd.read_file("regions.geojson")
    if gdf is not exists:
        print(f"{gdf} is not exists")

    # 2. יצירת נקודה מתאימה
    pt = Point(lon, lat)

    # 3. סינון השורות שהפוליגון שלהן מכיל את הנקודה
    matched = gdf[gdf.geometry.contains(pt)]

    # 4. החזרת שם האזור אם נמצאה התאמה, אחרת OVERSEAS
    if not matched.empty:
        return matched.iloc[0]["region"]
    return "OVERSEAS"

class RedisDuplicateChecker:
    def __init__(self, host, port, ttl):
        self.client = redis.Redis(host=host, port=port, decode_responses=True)
        self.ttl = ttl

    def is_duplicate(self, alert_id):
        exists = self.client.get(alert_id)
        if exists:
            return True
        self.client.setex(alert_id, self.ttl, "processed")
        return False

class RabbitMQPublisher:
    def __init__(self, host):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters(host=host))
        self.channel = self.connection.channel()
        self.channel.exchange_declare(exchange="commands_exchange", exchange_type="direct")

    def publish(self, region, message):
        routing_key = f"region.{region}"
        self.channel.basic_publish(
            exchange="commands_exchange",
            routing_key=routing_key,
            body=json.dumps(message)
        )
        logger.info(f"Alert {message.get('id')} successfully sent to RabbitMQ for region: {region}")

class AlertValidator:
    def validate(alert_data):
        if not isinstance(alert_data, dict):
            return False
        if not alert_data.get("content") or not alert_data.get("title"):
            return False
        if "lon" not in alert_data or "lat" not in alert_data:
            return False
        return True

def main():
    redis_checker = RedisDuplicateChecker(REDIS_HOST, REDIS_PORT, REDIS_TTL_SECONDS)
    rabbit_publisher = RabbitMQPublisher(RABBITMQ_HOST)

    config= {
            'bootstrap.servers': "localhost:9092",
            'group.id': "classification-service-group",
            'auto.offset.reset': 'earliest'
    }
    consumer = Consumer(config)
    try:
        for message in consumer:
            alert_data = message.value
            if not AlertValidator.validate(alert_data):
                logger.warning(f"Validation failed for alert: {alert_data}")
                continue
            alert_id = alert_data["alert_id"]
            if redis_checker.is_duplicate(alert_id):
                logger.info(f"Duplicate alert detected: {alert_id}. Skipping.")
                continue
            lon = alert_data["lon"]
            lat = alert_data["lat"]
            region = get_region_with_geopandas(lon, lat)
            rabbit_publisher.publish(region, alert_data)


    except KeyboardInterrupt:
        logger.info("Stopping classification service...")
    finally:
        consumer.close()
        rabbit_publisher.close()

if __name__ == "__main__":
    main()
