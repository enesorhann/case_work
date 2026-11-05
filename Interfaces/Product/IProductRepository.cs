namespace MiniDpp.Interfaces.Product
{
    using MiniDpp.Domain.Entities;

    public interface IProductRepository
    {
        // Model, partiler ve sürümlerle birlikte getirir. tracking=false ise salt okunur.
        Task<Product?> GetByGtinAsync(string gtin, bool tracking = false);
        // Modeller ve partilerle birlikte, GTIN sırasına göre
        Task<List<Product>> GetAllAsync();
        Task<bool> ExistsAsync(string gtin);
        Task<ProductModel> GetOrCreateModelAsync(string name);
        Task AddAsync(Product product);
        Task AddVersionAsync(ProductVersion version);
        // Tüm değişiklikler tek seferde (tek transaction) kaydedilir.
        Task SaveChangesAsync();
    }
}
