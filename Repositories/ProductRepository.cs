using Microsoft.EntityFrameworkCore;
using MiniDpp.Data;
using MiniDpp.Domain.Entities;
using MiniDpp.Interfaces.Product;

namespace MiniDpp.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly DataContext _context;

        public ProductRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByGtinAsync(string gtin, bool tracking = false)
        {
            var query = _context.Products
                .Include(p => p.Model)
                .Include(p => p.Batches)
                .Include(p => p.Versions)
                .AsSplitQuery();

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(p => p.Gtin == gtin);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Model)
                .Include(p => p.Batches)
                .AsSplitQuery()
                .OrderBy(p => p.Gtin)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string gtin)
        {
            return await _context.Products.AnyAsync(p => p.Gtin == gtin);
        }

        public async Task<ProductModel> GetOrCreateModelAsync(string name)
        {
            // Aynı içe aktarmada az önce eklenmiş (henüz kaydedilmemiş) model de bulunur.
            var model = _context.ProductModels.Local.FirstOrDefault(m => m.Name == name)
                ?? await _context.ProductModels.FirstOrDefaultAsync(m => m.Name == name);

            if (model == null)
            {
                model = new ProductModel { Name = name };
                await _context.ProductModels.AddAsync(model);
            }
            return model;
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task AddVersionAsync(ProductVersion version)
        {
            await _context.ProductVersions.AddAsync(version);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
