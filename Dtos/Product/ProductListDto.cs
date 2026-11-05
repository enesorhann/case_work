using MiniDpp.Dtos.Passport;

namespace MiniDpp.Dtos.Product
{
    public class ProductListDto
    {
        public string Gtin { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public double CottonPercent { get; set; }
        public double ElastanePercent { get; set; }
        public List<BatchDto> Batches { get; set; } = new();
        public int Version { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
