using AlertsAPI.Models;
using MongoDB.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlertsAPI.Repositories
{
    public interface IAlertRepository
    {
        Task<List<AlertModel>> GetAllAlertsByCommandAsync(string command);
    }
}
