using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace NotificationGate.Services
{
    public class KafkaProducerService : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic = "alerts-topic";

        public KafkaProducerService(string bootstrapServers)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task SendAlertAsync(string key, string messageValue)
        {
            try
            {
                var dr = await _producer.ProduceAsync(_topic, new Message<string, string>
                {
                    Key = key,
                    Value = messageValue
                });
                //Console.WriteLine($"Sent '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<string, string> e)
            {
                Console.WriteLine($"Kafka produce error: {e.Error.Reason}");
            }
        }

        public void Dispose()
        {
            try
            {
                _producer.Flush(TimeSpan.FromSeconds(10));
            }
            catch { }
            _producer.Dispose();
        }
    }
}
