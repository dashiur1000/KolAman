using Elastic.Clients.Elasticsearch;
using NotificationGate.Models;

namespace NotificationGate.Services
{
    public class ElasticService : IElasticService
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "exam-logs";

        public ElasticService()
        {
            string elasticUrl = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL") ?? "http://localhost:9200";

            var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
                .DefaultIndex(IndexName);

            _client = new ElasticsearchClient(settings);

            EnsureIndexExistsAsync().Wait();
        }

        private async Task EnsureIndexExistsAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(IndexName);
            if (!existsResponse.Exists)
            {
                var createResponse = await _client.Indices.CreateAsync(IndexName);
                if (createResponse.IsValidResponse)
                {
                    Console.WriteLine($"Elasticsearch index '{IndexName}' created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create Elasticsearch index: {createResponse.DebugInformation}");
                }
            }
        }

        public async Task SendLogAsync(string message, string level)
        {
            var log = new AppLog
            {
                Message = message,
                Level = level
            };

            var response = await _client.IndexAsync(log, IndexName);

            if (!response.IsValidResponse)
            {
                Console.WriteLine($"Error sending log to Elasticsearch: {response.DebugInformation}");
            }
        }
    }
}
