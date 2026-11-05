namespace MiniDpp.Domain.Entities
{
    // Bonus: ürün güncellendiğinde önceki hali sürüm numarası ve tarihiyle saklanır.
    public class ProductVersion
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int VersionNumber { get; set; }
        public string SnapshotJson { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
