using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MongoDB.Bson;

namespace DbConsumer.Services
{
    public class RabbitMQConsumerService
    {
        IElasticService elasticService = new ElasticService();
        private readonly string _hostName;
        private readonly MongoRepository _mongoRepository;

        public RabbitMQConsumerService(string hostName, MongoRepository mongoRepository)
        {
            _hostName = hostName;
            _mongoRepository = mongoRepository;
        }

        public void StartConsuming()
        {
            var factory = new ConnectionFactory() { HostName = _hostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "commands_exchange", type: ExchangeType.Direct);

            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                exchange: "commands_exchange",
                routingKey: "region.NORTH");
            channel.QueueBind(queue: queueName,
                exchange: "commands_exchange",
                routingKey: "region.CENTER");
            channel.QueueBind(queue: queueName,
                exchange: "commands_exchange",
                routingKey: "region.SOUTH");
            channel.QueueBind(queue: queueName,
                exchange: "commands_exchange",
                routingKey: "region.OVERSEAS");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                string logLevel2 = "Info";
                string logMessage2 = $"Received alert from RabbitMQ (RoutingKey: {routingKey})";
                elasticService.SendLogAsync(logMessage2, logLevel2);

                try
                {
                    var alertDoc = BsonDocument.Parse(messageJson);

                    if (!ValidateAlert(alertDoc))
                    {
                        string logLevel3 = "Warning";
                        string logMessage3 = "Validation failed for alert. Skipping.";
                        elasticService.SendLogAsync(logMessage3, logLevel3);
                        return;
                    }

                    string region = routingKey.Replace("region.", "");
                    _mongoRepository.SaveAlert(region, alertDoc);

                    string logLevel4 = "Warning";
                    string logMessage4 = $"Successfully saved alert to MongoDB collection for: {region}";
                    elasticService.SendLogAsync(logMessage4, logLevel4);
                }
                catch (Exception ex)
                {
                    string logLevel3 = "Error";
                    string logMessage3 = $"Processing message failed: {ex.Message}";
                    elasticService.SendLogAsync(logMessage3, logLevel3);
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Listening for Rabbit messages");
            Console.ReadLine();
        }

        private bool ValidateAlert(BsonDocument doc)
        {
            return doc.Contains("title") && doc.Contains("content");
        }
    }
}

