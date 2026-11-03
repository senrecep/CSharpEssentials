using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The hand-written dispatch: collect and clear the events of tracked holders, save, then publish.</summary>
public sealed class HandWrittenDispatchContext(DbContextOptions<HandWrittenDispatchContext> options, IDomainEventPublisher publisher)
    : DbContext(options)
{
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        List<IDomainEvent> events = [];
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<IDomainEventHolder> entry in ChangeTracker.Entries<IDomainEventHolder>())
        {
            events.AddRange(entry.Entity.DomainEvents);
            entry.Entity.ClearDomainEvents();
        }

        int written = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        foreach (IDomainEvent domainEvent in events)
            await publisher.PublishAsync(domainEvent, cancellationToken);

        return written;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => BenchModel.Configure(modelBuilder);
}
