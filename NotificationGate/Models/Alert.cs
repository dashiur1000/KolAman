namespace NotificationGate.Models
{
    public class Alert
    {
        public string alert_id { get; set; }
        public string source { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string priority { get; set; }
        public string classification { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string? timestamp { get; set; }
        public string status { get; set; }
    }
}
