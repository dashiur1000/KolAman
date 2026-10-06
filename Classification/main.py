import json
import logging
from os.path import exists
from confluent_kafka import Consumer
from shapely.geometry import Point
import redis
import pika
import geopandas as gpd
from elasticsearch import Elasticsearch, helpers
import configparser

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

REDIS_HOST = "localhost"
REDIS_PORT = 6379
REDIS_TTL_SECONDS = 3600

KAFKA_BOOTSTRAP_SERVERS = ["localhost:9092"]
KAFKA_TOPIC = "alerts-topic"
KAFKA_GROUP_ID = "classification-service-group"

RABBITMQ_HOST = "localhost"

# ["elasticsearch"]
# cloud_id = "discovery.type=single-node"
# user = "pack.security.enabled=false"
# password = "ES_JAVA_OPTS=-Xms512m -Xmx512m"

def get_region_with_geopandas(lon: float, lat: float) -> str:
    geojson_path = "regions.geojson"
    if not exists(geojson_path):
        logger.error(f"{geojson_path} does not exist")
        return "OVERSEAS"

    gdf = gpd.read_file(geojson_path)

    pt = Point(lon, lat)

    matched = gdf[gdf.geometry.contains(pt)]

    if not matched.empty:
        return matched.iloc[0]["region"]
    return "OVERSEAS"


class RedisDuplicateChecker:
    def __init__(self, host, port, ttl):
        self.client = redis.Redis(host=host, port=port, decode_responses=True)
        self.ttl = ttl

    def is_duplicate(self, alert_id):
        exists_flag = self.client.get(alert_id)
        if exists_flag:
            return True
        self.client.set(alert_id, self.ttl, "processed")
        return False


class RabbitMQPublisher:
    def __init__(self, host):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters(host=host))
        self.channel = self.connection.channel()
        self.channel.exchange_declare(exchange="commands_exchange", exchange_type="direct")


    def publish(self, region, message):
        routing_key = f"region.{region}"

        queue_name = f"queue_{region}"
        self.channel.queue_declare(queue=queue_name, durable=True)
        self.channel.queue_bind(exchange="commands_exchange", queue=queue_name, routing_key=routing_key)

        self.channel.basic_publish(
            exchange="commands_exchange",
            routing_key=routing_key,
            body=json.dumps(message)
        )
        logger.info(f"Alert successfully sent to RabbitMQ for region: {region}")

    def close(self):
        if self.connection and self.connection.is_open:
            self.connection.close()


# class Elasticsearch_Config:
#
#     config = configparser.ConfigParser()
#
#     def config_to_es(self):
#         es = Elasticsearch(
#             cloud_id=self.config['elasticsearch'][cloud_id],
#             http_auth=(self.config['elasticsearch'][user], self.config['elasticsearch'][password])
#         )
#         print(es.info())

class AlertValidator:
    def validate(alert_data):
        if not isinstance(alert_data, dict):
            return False
        if not alert_data.get("content") or not alert_data.get("title"):
            return False
        if "alert_id" not in alert_data:
            return False
        if alert_data["lon"] <= 0 or alert_data["lat"] <= 0:
            return False
        return True


def main():
    redis_checker = RedisDuplicateChecker(REDIS_HOST, REDIS_PORT, REDIS_TTL_SECONDS)
    rabbit_publisher = RabbitMQPublisher(RABBITMQ_HOST)
    # elasticsearch_config = Elasticsearch_Config()


    config = {
        'bootstrap.servers': KAFKA_BOOTSTRAP_SERVERS[0],
        'group.id': KAFKA_GROUP_ID,
        'auto.offset.reset': 'earliest'
    }

    consumer = Consumer(config)
    consumer.subscribe([KAFKA_TOPIC])

    logger.info("Starting classification service...")
    try:
        while True:
            msg = consumer.poll(1.0)
            if msg is None:
                continue
            if msg.error():
                logger.error(f"Consumer error: {msg.error()}")
                continue

            try:
                alert_data = json.loads(msg.value().decode('utf-8'))
            except Exception as e:
                logger.error(f"Failed to parse message value as JSON: {e}")
                continue

            if not AlertValidator.validate(alert_data):
                logger.warning(f"Validation failed for alert: {alert_data}")
                continue

            alert_id = alert_data.get("alert_id", alert_data.get("id"))

            if redis_checker.is_duplicate(alert_id):
                logger.info(f"Duplicate alert detected: {alert_id}. Skipping.")
                continue

            lon = float(alert_data["lon"])
            lat = float(alert_data["lat"])
            region = get_region_with_geopandas(lon, lat)

            # elasticsearch_config.config_to_es()

            rabbit_publisher.publish(f"{region}", alert_data)

    except KeyboardInterrupt:
        logger.info("Stopping classification service...")
    finally:
        consumer.close()
        rabbit_publisher.close()


if __name__ == "__main__":
    main()



