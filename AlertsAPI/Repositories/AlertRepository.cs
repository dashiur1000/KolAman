using AlertsAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlertsAPI.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly IMongoDatabase _database;

        public AlertRepository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<List<AlertModel>> GetAllAlertsByCommandAsync(string command)
        {
            var collectionName = command;
            var allAlerts = new List<AlertModel>();
            var collection = _database.GetCollection<AlertModel>(collectionName);
            var alerts = await collection.Find(_ => true).ToListAsync();
            allAlerts.AddRange(alerts);
            return allAlerts;
        }
    }
}

