using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.DbErrors;

public sealed class TranslatorDbContext(DbContextOptions<TranslatorDbContext> options) : DbContext(options)
{
    public DbSet<TranslatorItem> Items => Set<TranslatorItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<TranslatorItem>(item =>
        {
            item.ToTable("translator_items");
            item.HasIndex(x => x.Name).IsUnique();
        });
}
