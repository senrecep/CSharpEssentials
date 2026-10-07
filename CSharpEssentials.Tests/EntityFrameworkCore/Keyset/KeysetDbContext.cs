using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Keyset;

public sealed class KeysetDbContext(DbContextOptions<KeysetDbContext> options) : DbContext(options)
{
    public DbSet<KeysetRow> Rows => Set<KeysetRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<KeysetRow>(row =>
        {
            row.ToTable("keyset_rows");
            row.Property(x => x.Id).ValueGeneratedNever();
        });
}
