using CSharpEssentials.Enums;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// The Readme's enum storage setup: <c>ConfigureEnumConventions(EnumConventions.Default)</c> for every column and
/// <c>HasLegacyEnumStorage(EnumStoredAs.LegacySnakeCase)</c> for the column that keeps its 3.x text.
/// </summary>
public sealed class EnumStorageContext(DbContextOptions<EnumStorageContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ConfigureEnumConventions(EnumConventions.Default);

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Shipment>().Property(x => x.LegacyStatus).HasLegacyEnumStorage(EnumStoredAs.LegacySnakeCase);
}
