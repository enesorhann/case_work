namespace MiniDpp.Dtos.Product
{
    public class ProductVersionDto
    {
        public int VersionNumber { get; set; }
        public DateTime SavedAt { get; set; }
        public ProductSnapshotDto Data { get; set; } = new();
    }
}
