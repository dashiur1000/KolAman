namespace NotificationGate.Services
{
    public interface IElasticService
    {
        Task SendLogAsync(string message, string level);
    }
}
