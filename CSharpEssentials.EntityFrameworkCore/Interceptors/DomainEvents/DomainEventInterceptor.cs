using System.Collections.Concurrent;
using System.Reflection;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Intercepts SaveChanges to collect and dispatch domain events raised by entities.
/// <para>
/// <b>Timing:</b> Events are split by <see cref="DomainEventTimingAttribute"/>:
/// <list type="bullet">
///   <item><see cref="DomainEventTiming.BeforeSave"/> — published before the DB transaction commits (can abort save on failure).</item>
///   <item><see cref="DomainEventTiming.AfterSave"/> — published after the DB transaction commits (default, safe).</item>
/// </list>
/// </para>
/// <para>
/// <b>Outbox support:</b> When an <see cref="IDomainEventOutbox"/> is registered in DI,
/// after-save events are routed to the outbox for reliable delivery instead of direct publish.
/// Before-save events are always published directly via <see cref="IDomainEventPublisher"/>.
/// </para>
/// </summary>
public sealed class DomainEventInterceptor(
    ILogger<DomainEventInterceptor> logger,
    IServiceScopeFactory serviceScopeFactory) : SaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<Type, DomainEventTiming> TimingCache = new();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        IDomainEvent[] allEvents = CollectDomainEvents(eventData.Context);
        if (allEvents.Length == 0)
            return base.SavingChanges(eventData, result);

        (IDomainEvent[] beforeSave, IDomainEvent[] afterSave) = SplitByTiming(allEvents);

        if (beforeSave.Length > 0)
            PublishEventsAsync(beforeSave, CancellationToken.None).GetAwaiter().GetResult();

        InterceptionResult<int> returnValue = base.SavingChanges(eventData, result);

        if (afterSave.Length > 0)
            DispatchAfterSaveEventsAsync(afterSave, CancellationToken.None).GetAwaiter().GetResult();

        return returnValue;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        IDomainEvent[] allEvents = CollectDomainEvents(eventData.Context);
        if (allEvents.Length == 0)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        (IDomainEvent[] beforeSave, IDomainEvent[] afterSave) = SplitByTiming(allEvents);

        if (beforeSave.Length > 0)
            await PublishEventsAsync(beforeSave, cancellationToken);

        InterceptionResult<int> returnValue = await base.SavingChangesAsync(eventData, result, cancellationToken);

        if (afterSave.Length > 0)
            await DispatchAfterSaveEventsAsync(afterSave, cancellationToken);

        return returnValue;
    }

    /// <summary>
    /// Collects domain events from all tracked entities, preserving per-entity list order.
    /// Events are returned entity-by-entity in ChangeTracker order; within each entity
    /// they appear in the order they were raised (list index).
    /// </summary>
    private static IDomainEvent[] CollectDomainEvents(DbContext context)
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

    private static (IDomainEvent[] BeforeSave, IDomainEvent[] AfterSave) SplitByTiming(IDomainEvent[] events)
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

    private async Task DispatchAfterSaveEventsAsync(IDomainEvent[] events, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IDomainEventOutbox? outbox = scope.ServiceProvider.GetService<IDomainEventOutbox>();

        if (outbox is not null)
        {
            logger.LogDebug("Storing {Count} domain events in outbox", events.Length);
            await outbox.StoreAsync(events, cancellationToken);
        }
        else
        {
            await PublishEventsDirectAsync(scope.ServiceProvider, events, cancellationToken);
        }
    }

    private async Task PublishEventsAsync(IDomainEvent[] events, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        await PublishEventsDirectAsync(scope.ServiceProvider, events, cancellationToken);
    }

    private async Task PublishEventsDirectAsync(IServiceProvider provider, IDomainEvent[] events, CancellationToken cancellationToken)
    {
        IDomainEventPublisher publisher = provider.GetRequiredService<IDomainEventPublisher>();

        logger.LogDebug("Publishing {Count} domain events", events.Length);

        foreach (IDomainEvent domainEvent in events)
            await publisher.PublishAsync(domainEvent, cancellationToken);
    }
}
