using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Intercepts SaveChanges to collect and dispatch domain events raised by entities.
/// <para>
/// <b>Timing:</b> Events are split by <see cref="DomainEventTimingAttribute"/>:
/// <list type="bullet">
///   <item><see cref="DomainEventTiming.BeforeSave"/> — published first (a failure aborts the save).</item>
///   <item><see cref="DomainEventTiming.AfterSave"/> — dispatched next, still inside <c>SavingChanges</c>, i.e. before
///   the changes are written. With an <see cref="IDomainEventOutbox"/> that adds rows to the same context, the outbox
///   entries are saved atomically with the entities. For dispatch after a successful save use
///   <c>BaseDbContext.DispatchDomainEventsOnSaveChanges</c>.</item>
/// </list>
/// </para>
/// <para>
/// <b>Outbox support:</b> When an <see cref="IDomainEventOutbox"/> is registered in DI,
/// after-save events are routed to the outbox for reliable delivery instead of direct publish.
/// Before-save events are always published directly via <see cref="IDomainEventPublisher"/>.
/// </para>
/// </summary>
public sealed partial class DomainEventInterceptor(
    ILogger<DomainEventInterceptor> logger,
    IServiceScopeFactory serviceScopeFactory) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        IDomainEvent[] allEvents = DomainEventCollector.Collect(eventData.Context);
        if (allEvents.Length == 0)
            return base.SavingChanges(eventData, result);

        (IDomainEvent[] beforeSave, IDomainEvent[] afterSave) = DomainEventCollector.SplitByTiming(allEvents);

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
            return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);

        IDomainEvent[] allEvents = DomainEventCollector.Collect(eventData.Context);
        if (allEvents.Length == 0)
            return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);

        (IDomainEvent[] beforeSave, IDomainEvent[] afterSave) = DomainEventCollector.SplitByTiming(allEvents);

        if (beforeSave.Length > 0)
            await PublishEventsAsync(beforeSave, cancellationToken).ConfigureAwait(false);

        InterceptionResult<int> returnValue = await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);

        if (afterSave.Length > 0)
            await DispatchAfterSaveEventsAsync(afterSave, cancellationToken).ConfigureAwait(false);

        return returnValue;
    }

    private async Task DispatchAfterSaveEventsAsync(IDomainEvent[] events, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IDomainEventOutbox? outbox = scope.ServiceProvider.GetService<IDomainEventOutbox>();

        if (outbox is not null)
        {
            LogStoringDomainEvents(events.Length);
            await outbox.StoreAsync(events, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await PublishEventsDirectAsync(scope.ServiceProvider, events, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task PublishEventsAsync(IDomainEvent[] events, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        await PublishEventsDirectAsync(scope.ServiceProvider, events, cancellationToken).ConfigureAwait(false);
    }

    private async Task PublishEventsDirectAsync(IServiceProvider provider, IDomainEvent[] events, CancellationToken cancellationToken)
    {
        IDomainEventPublisher publisher = provider.GetRequiredService<IDomainEventPublisher>();

        LogPublishingDomainEvents(events.Length);

        foreach (IDomainEvent domainEvent in events)
            await publisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Storing {Count} domain events in outbox")]
    private partial void LogStoringDomainEvents(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Publishing {Count} domain events")]
    private partial void LogPublishingDomainEvents(int count);
}
