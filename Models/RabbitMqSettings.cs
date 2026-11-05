namespace MiniDpp.Models
{
    public class RabbitMqSettings
    {
        public const string SectionName = "RabbitMq";

        // false ise ya da RabbitMQ'ya ulaşılamazsa arka plan işleri aynı süreçte çalışır.
        public bool Enabled { get; set; } = true;
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";

        public int RetryDelaySeconds { get; set; } = 5;
    }
}
