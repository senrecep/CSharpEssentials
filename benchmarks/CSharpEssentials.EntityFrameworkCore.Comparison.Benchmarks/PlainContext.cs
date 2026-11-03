namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>A plain EF Core context with no interceptors and no overrides.</summary>
public sealed class PlainContext(DbContextOptions<PlainContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => BenchModel.Configure(modelBuilder);
}
