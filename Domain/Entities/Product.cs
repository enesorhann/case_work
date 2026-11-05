namespace MiniDpp.Domain.Entities
{
    // GTIN ile tanımlanan ürün. GTIN baştaki sıfırlar kaybolmasın diye metin (string) tutulur.
    public class Product
    {
        public int Id { get; set; }
        public string Gtin { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int ModelId { get; set; }
        public ProductModel Model { get; set; } = null!;
        public double CottonPercent { get; set; }
        public double ElastanePercent { get; set; }
        public double CarbonKgCo2e { get; set; }
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Batch> Batches { get; set; } = new List<Batch>();
        public ICollection<ProductVersion> Versions { get; set; } = new List<ProductVersion>();
    }
}
