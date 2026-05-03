using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.EntityFrameworkCore.Data;

/// <summary>
/// ShopDbContext inherits from BaseDbContext provided by CSharpEssentials.
/// BaseDbContext automatically logs context lifecycle events and provides
/// a service provider scope for dependency resolution.
/// </summary>
public class ShopDbContext : BaseDbContext<ShopDbContext>
{
    public ShopDbContext(
        DbContextOptions<ShopDbContext> options,
        IServiceScopeFactory serviceScopeFactory) : base(options, serviceScopeFactory) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// ConfigureConventions is called before OnModelCreating and applies
    /// global conventions. ConfigureEnumConventions scans the assembly for
    /// enums decorated with [StringEnum] and automatically registers
    /// EnumToFormattedStringConverter + MaxLength for each.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.ConfigureEnumConventions(typeof(ShopDbContext).Assembly);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> implementations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopDbContext).Assembly);

        // Configure soft-delete query filter for all SoftDeletableEntityBase entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletableEntityBase).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ShopDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder });
            }
        }
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : class, ISoftDeletableEntityBase
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
