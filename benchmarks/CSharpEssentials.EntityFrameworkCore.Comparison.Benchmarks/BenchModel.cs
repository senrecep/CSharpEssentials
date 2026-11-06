namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The one model every context of the suite maps, so all rows read and write the same tables.</summary>
internal static class BenchModel
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditedOrder>(order =>
        {
            order.ToTable("orders");
            order.Ignore(x => x.DomainEvents);
        });
        modelBuilder.Entity<FeedItem>(item =>
        {
            item.ToTable("feed");
            item.Property(x => x.Id).ValueGeneratedNever();
            item.HasIndex(x => new { x.PublishedAt, x.Id });
        });
        modelBuilder.Entity<Country>(country =>
        {
            country.ToTable("countries");
            country.HasKey(x => x.Code);
        });
        modelBuilder.Entity<UniqueItem>(item =>
        {
            item.ToTable("unique_items");
            item.HasIndex(x => x.Name).IsUnique();
        });
    }
}
