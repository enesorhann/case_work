using Microsoft.EntityFrameworkCore;
using MiniDpp.Domain.Entities;

namespace MiniDpp.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<ProductModel> ProductModels { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Batch> Batches { get; set; } = null!;
        public DbSet<ProductVersion> ProductVersions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ProductModel
            modelBuilder.Entity<ProductModel>()
                .Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<ProductModel>()
                .HasIndex(m => m.Name)
                .IsUnique();

            // Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Gtin)
                .IsRequired()
                .HasMaxLength(14);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Gtin)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasMaxLength(200);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Model)
                .WithMany(m => m.Products)
                .HasForeignKey(p => p.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Batch
            modelBuilder.Entity<Batch>()
                .Property(b => b.BatchNumber)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Batch>()
                .Property(b => b.ProductionPlace)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Batch>()
                .HasIndex(b => new { b.ProductId, b.BatchNumber })
                .IsUnique();

            modelBuilder.Entity<Batch>()
                .HasOne(b => b.Product)
                .WithMany(p => p.Batches)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductVersion
            modelBuilder.Entity<ProductVersion>()
                .Property(v => v.SnapshotJson)
                .IsRequired();

            modelBuilder.Entity<ProductVersion>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Versions)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // SQLite tarih bilgisini saat dilimi olmadan saklar; okurken UTC olarak işaretlenir.
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties().Where(p => p.ClrType == typeof(DateTime)))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        value => value,
                        value => DateTime.SpecifyKind(value, DateTimeKind.Utc)));
                }
            }
        }
    }
}
