using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.PurchasePrice).HasPrecision(18, 2);
        builder.Property(p => p.SalePrice).HasPrecision(18, 2);
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}