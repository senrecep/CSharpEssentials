using System.Collections.Concurrent;
using System.Reflection;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Shared domain event collection logic used by <see cref="DomainEventInterceptor"/>
/// and <see cref="BaseDbContext{TContext}"/>.
/// </summary>
internal static class DomainEventCollector
{
    private static readonly ConcurrentDictionary<Type, DomainEventTiming> TimingCache = new();

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

    /// <summary>
    /// Calls <see cref="IDomainEventOutbox.StoreAsync"/> and returns the entries it added to <paramref name="context"/>.
    /// When the outbox throws, the entries it added are detached so a retry does not write them twice.
    /// </summary>
    internal static async Task<EntityEntry[]> StoreInOutboxAsync(
        IDomainEventOutbox outbox, DbContext context, IDomainEvent[] events, CancellationToken cancellationToken)
    {
        HashSet<object> before = new(context.ChangeTracker.Entries().Select(static e => e.Entity), ReferenceEqualityComparer.Instance);
        try
        {
            await outbox.StoreAsync(context, events, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            Detach(AddedSince(context, before));
            throw;
        }

        return AddedSince(context, before);
    }

    /// <summary>Detaches entries that are still tracked, such as outbox rows whose save failed.</summary>
    internal static void Detach(EntityEntry[] entries)
    {
        foreach (EntityEntry entry in entries)
        {
            if (entry.State != EntityState.Detached)
                entry.State = EntityState.Detached;
        }
    }

    private static EntityEntry[] AddedSince(DbContext context, HashSet<object> before) =>
        [.. context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added && !before.Contains(e.Entity))];

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
