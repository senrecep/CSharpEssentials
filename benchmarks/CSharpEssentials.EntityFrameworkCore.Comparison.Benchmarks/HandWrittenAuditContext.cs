using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The hand-written audit: a <c>SaveChangesAsync</c> override that stamps the same fields CSE's interceptor stamps.</summary>
public sealed class HandWrittenAuditContext(DbContextOptions<HandWrittenAuditContext> options, TimeProvider clock, string userId)
    : DbContext(options)
{
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = clock.GetUtcNow();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is ICreationAudit created)
                created.SetCreatedInfo(now, userId);
            else if (entry.State == EntityState.Modified && entry.Entity is IModificationAudit modified)
                modified.SetUpdatedInfo(now, userId);
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => BenchModel.Configure(modelBuilder);
}
