using MiniDpp.Dtos.Passport;

namespace MiniDpp.Dtos.Product
{
    // Ürünün belirli bir andaki hali. Sürüm karşılaştırması ve sürüm geçmişi için kullanılır.
    public class ProductSnapshotDto
    {
        public string Gtin { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public double CottonPercent { get; set; }
        public double ElastanePercent { get; set; }
        public double CarbonKgCo2e { get; set; }
        public List<BatchDto> Batches { get; set; } = new();
    }
}
