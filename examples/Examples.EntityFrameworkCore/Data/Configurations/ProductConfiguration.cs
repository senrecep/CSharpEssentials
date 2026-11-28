using CSharpEssentials.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Examples.EntityFrameworkCore.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.SoftDeletableEntityBaseGuidIdMap();
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);
    }
}

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.EntityBaseGuidIdMap();
        builder.HasIndex(c => c.Email).IsUnique();
    }
}
