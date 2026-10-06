using AlertsAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace AlertsAPI.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertRepository _alertRepository;

        public AlertsController(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        [HttpGet("ByCommand")]
        public async Task<IActionResult> GetAllAlerts(string command)
        {
            var alerts = await _alertRepository.GetAllAlertsByCommandAsync(command);
            var jsonList = alerts.ConvertAll(doc => doc.ToJson());
            return Ok(jsonList);
        }
    }
}
