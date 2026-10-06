using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AlertsAPI.Models
{
        public class AlertModel
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string? Id { get; set; }
            public string? alert_id { get; set; }
            public string? source { get; set; }
            public string title { get; set; } = string.Empty;
            public string? priority { get; set; }
            public string content { get; set; } = string.Empty;
            public string? classification { get; set; }
            public double lon { get; set; }
            public double lat { get; set; }
            public string? timestamp { get; set; }
            public string? status { get; set; }
        }
}
