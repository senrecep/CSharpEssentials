using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore;

namespace Examples.Enums.EndToEnd;

public sealed class ShopDbContext(DbContextOptions<ShopDbContext> options, EnumConventions conventions) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    // Status: text column with wire names and ck_orders_status_enum.
    // Permissions ([Flags]): integer bitmask with a mask check constraint.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ConfigureEnumConventions(conventions);

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Order>(order =>
        {
            order.ToTable("orders");
            order.Property(o => o.Id).HasColumnName("id");
            order.Property(o => o.Customer).HasColumnName("customer");
            order.Property(o => o.Status).HasColumnName("status");
            order.Property(o => o.Permissions).HasColumnName("permissions");
        });
}
