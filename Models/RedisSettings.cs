namespace MiniDpp.Models
{
    public class RedisSettings
    {
        public const string SectionName = "Redis";

        // false ise ya da Redis'e ulaşılamazsa önbellek bellekte tutulur.
        public bool Enabled { get; set; } = true;
        public string ConnectionString { get; set; } = "localhost:6379";
    }
}
