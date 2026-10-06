using MongoDB.Driver;
using MongoDB.Bson;

namespace DbConsumer.Services
{
    public class MongoRepository
    {
        private readonly IMongoDatabase _database;

        public MongoRepository(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }
        public void SaveAlert(string region, BsonDocument alertDocument)
        {
            string collectionName = region switch
            {
                "NORTH" => "NorthAlerts",
                "CENTER" => "CentralAlerts",
                "SOUTH" => "SouthernAlerts",
                "OVERSEAS" => "OverseasAlerts",
                _ => "OverseasAlerts"
            };

            var collection = _database.GetCollection<BsonDocument>(collectionName);
            collection.InsertOne(alertDocument);
        }
    }
}
