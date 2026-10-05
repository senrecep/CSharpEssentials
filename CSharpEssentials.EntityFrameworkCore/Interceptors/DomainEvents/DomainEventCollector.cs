using System.Collections.Concurrent;
using System.Reflection;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Shared domain event collection logic used by <see cref="DomainEventInterceptor"/>
/// and <see cref="BaseDbContext{TContext}"/>.
/// </summary>
internal static class DomainEventCollector
{
    private static readonly ConcurrentDictionary<Type, DomainEventTiming> TimingCache = new();

    /// <summary>
    /// Collects and clears domain events from all tracked entities, preserving per-entity list order.
    /// Events are returned entity-by-entity in ChangeTracker order; within each entity
    /// they appear in the order they were raised (list index).
    /// </summary>
    internal static IDomainEvent[] Collect(DbContext context)
    {
        List<IDomainEvent> collected = [];

        foreach (IDomainEventHolder entity in context.ChangeTracker
            .Entries<IDomainEventHolder>()
            .Select(e => e.Entity))
        {
            collected.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        return [.. collected];
    }

    internal static (IDomainEvent[] BeforeSave, IDomainEvent[] AfterSave) SplitByTiming(IDomainEvent[] events)
    {
        ILookup<bool, IDomainEvent> grouped = events
            .ToLookup(e => ResolveTiming(e) == DomainEventTiming.BeforeSave);

        return ([.. grouped[true]], [.. grouped[false]]);
    }

    private static DomainEventTiming ResolveTiming(IDomainEvent domainEvent)
    {
        return TimingCache.GetOrAdd(domainEvent.GetType(), static type =>
            type.GetCustomAttribute<DomainEventTimingAttribute>()?.Timing ?? DomainEventTiming.AfterSave);
    }
}
