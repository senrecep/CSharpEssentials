using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>A publisher that only counts, so the domain event rows measure collection and dispatch, not a handler.</summary>
public sealed class CountingPublisher : IDomainEventPublisher
{
    public int Count { get; private set; }

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Count++;
        return Task.CompletedTask;
    }

    public void Reset() => Count = 0;
}
