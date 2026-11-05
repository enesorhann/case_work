namespace MiniDpp.Models
{
    public class DppSettings
    {
        public const string SectionName = "Dpp";
        public const string DefaultBaseUrl = "http://localhost:8080";

        // QR kodun ve JSON-LD @id alanının kullandığı dış adres
        public string BaseUrl { get; set; } = DefaultBaseUrl;
        public string QrFolder { get; set; } = "Data/qr";
        public int CacheSeconds { get; set; } = 300;
        public long MaxUploadBytes { get; set; } = 5 * 1024 * 1024;
    }
}
