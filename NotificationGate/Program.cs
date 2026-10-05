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

            string alertsPath = @"C:\Users\dzs10\Desktop\IDF\KolAman\NotificationGate\alert-simulator\alerts\";
            Console.WriteLine(alertsPath);

            using var kafka = new KafkaProducerService(kafkaBootstrap);
            var watcher = new FileSystemWatche(alertsPath, kafka);
            watcher.Start();

            Console.WriteLine("NotificationGate running. Press Enter to exit...");
            Console.ReadLine();
        }
    }
}

