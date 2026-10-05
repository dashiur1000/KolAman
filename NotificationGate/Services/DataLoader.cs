using NotificationGate.Models;
using System.Text.Json;

namespace NotificationGate.Services
{
    public class DataLoader
    {
        public Alert? LoadFromJson(string json)
        {
            var str = File.ReadAllText(json);
            try
            {
                var alert = JsonSerializer.Deserialize<Alert>(str);
                return alert;
            }
            catch
            {
                return null;
            }
        }
    }
}
