namespace MiniDpp.Models
{
    // Doğrulanmış, kaydedilmeye hazır CSV satırı
    public class ProductRow
    {
        public int Line { get; set; }
        public string Gtin { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public string ProductionPlace { get; set; } = string.Empty;
        public double CottonPercent { get; set; }
        public double ElastanePercent { get; set; }
        public double CarbonKgCo2e { get; set; }
    }
}
