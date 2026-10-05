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

    /// <summary>
    /// Collects and clears domain events from all tracked entities, keeping each entity with its events
    /// so they can be put back with <see cref="Restore"/>.
    /// </summary>
    internal static List<(IDomainEventHolder Entity, IDomainEvent[] Events)> CollectByEntity(DbContext context)
    {
        List<(IDomainEventHolder Entity, IDomainEvent[] Events)> collected = [];

        foreach (IDomainEventHolder entity in context.ChangeTracker
            .Entries<IDomainEventHolder>()
            .Select(e => e.Entity))
        {
            IDomainEvent[] events = [.. entity.DomainEvents];
            if (events.Length == 0)
                continue;

            collected.Add((entity, events));
            entity.ClearDomainEvents();
        }

        return collected;
    }

    /// <summary>
    /// Puts the collected events that are in <paramref name="toRestore"/> back on their entities, ahead of any
    /// event raised since they were collected, keeping their original order.
    /// </summary>
    internal static void Restore(
        List<(IDomainEventHolder Entity, IDomainEvent[] Events)> collected, IDomainEvent[] toRestore)
    {
        if (toRestore.Length == 0)
            return;

        HashSet<IDomainEvent> restore = new(toRestore, ReferenceEqualityComparer.Instance);
        foreach ((IDomainEventHolder entity, IDomainEvent[] events) in collected)
        {
            IDomainEvent[] restored = [.. events.Where(restore.Contains)];
            if (restored.Length == 0)
                continue;

            IDomainEvent[] raisedSince = [.. entity.DomainEvents];
            entity.ClearDomainEvents();
            foreach (IDomainEvent domainEvent in restored.Concat(raisedSince))
                entity.Raise(domainEvent);
        }
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
