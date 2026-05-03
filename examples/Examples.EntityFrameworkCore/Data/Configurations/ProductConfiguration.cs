using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Examples.EntityFrameworkCore.Data.Configurations;

/// <summary>
/// Entity configuration demonstrating EF Core fluent API.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Configure table name explicitly
        builder.ToTable("products");

        // Configure index
        builder.HasIndex(p => p.Name).HasDatabaseName("idx_products_name");

        // Configure decimal precision
        builder.Property(p => p.Price)
               .HasPrecision(18, 2);
    }
}
