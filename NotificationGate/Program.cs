using System;
using System.IO;
using System.Threading.Tasks;
using NotificationGate.Services;

namespace NotificationGate
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            string kafkaBootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP") ?? "localhost:9092";

            IElasticService elasticService = new ElasticService();

            string alertsPath = @"C:\Users\dzs10\Desktop\IDF\KolAman\NotificationGate\alert-simulator\alerts\";

            string logLevel1 = "Info";
            string logMessage1 = alertsPath;
            await elasticService.SendLogAsync(logMessage1, logLevel1);

            using var kafka = new KafkaProducerService(kafkaBootstrap);

            string logLevel2 = "Info";
            string logMessage2 = "Connect to Elastic";
            await elasticService.SendLogAsync(logMessage2, logLevel2);

            var watcher = new FileSystemWatche(alertsPath, kafka);
            watcher.Start();

            string logLevel3 = "Info";
            string logMessage3 = "Send to Kafka";
            await elasticService.SendLogAsync(logMessage3, logLevel2);

            Console.WriteLine("NotificationGate running.");
        }
    }
}

