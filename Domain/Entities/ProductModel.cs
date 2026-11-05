namespace MiniDpp.Domain.Entities
{
    // Denim modeli (ör. "Slim Denim 501"). Bir modelin birden çok GTIN'li ürünü olabilir.
    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
