using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.EntityFrameworkCore.Data;

public sealed class ShopDbContext(DbContextOptions<ShopDbContext> options, IServiceScopeFactory scopes)
    : BaseDbContext<ShopDbContext>(options, scopes)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override DbContextInterceptors InterceptorsFromServices => DbContextInterceptors.All;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureEnumConventions(EnumConventions.Default);
        configurationBuilder.ConfigureNullableMaybeConventions();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopDbContext).Assembly);
        modelBuilder.ApplyNamedSoftDeleteQueryFilter();
    }
}
