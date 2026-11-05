namespace MiniDpp.Domain.Entities
{
    // Üretim partisi (batch/lot). Bir ürünün birden fazla partisi olabilir; üretim yeri partiye aittir.
    public class Batch
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string BatchNumber { get; set; } = string.Empty;
        public string ProductionPlace { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
