using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore;

public abstract partial class BaseDbContext<TContext> : DbContext
    where TContext : DbContext
{
    private readonly IServiceScope _serviceScope;
    protected readonly ILogger<TContext> Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseDbContext(
        DbContextOptions<TContext> options, IServiceScopeFactory serviceScopeFactory) : base(options)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
        ServiceProvider = _serviceScope.ServiceProvider;
        Logger = ServiceProvider.GetRequiredService<ILogger<TContext>>();
        LogContextCreated(ContextId.InstanceId);
    }

    /// <summary>
    /// Interceptors resolved from DI and attached in <see cref="OnConfiguring"/>.
    /// Interceptors that are not registered in DI, or that are already present on the options, are skipped.
    /// <see cref="DbContextInterceptors.DomainEvents"/> is ignored when <see cref="DispatchDomainEventsOnSaveChanges"/>
    /// is <c>true</c>, so domain events are dispatched by one path only.
    /// Defaults to <see cref="DbContextInterceptors.None"/>.
    /// </summary>
    protected virtual DbContextInterceptors InterceptorsFromServices => DbContextInterceptors.None;

    /// <summary>
    /// When <c>true</c>, <c>SaveChanges</c>/<c>SaveChangesAsync</c> collect domain events from tracked
    /// <see cref="IDomainEventHolder"/> entities and pass them to <see cref="DispatchDomainEventsAsync"/>:
    /// <see cref="DomainEventTiming.BeforeSave"/> events before the save, <see cref="DomainEventTiming.AfterSave"/>
    /// events after it succeeds. Defaults to <c>false</c>.
    /// <para>
    /// When an <see cref="IDomainEventOutbox"/> is registered, after-save events are stored in it before the changes
    /// are written, with this context, so the rows it adds are saved by the same <c>SaveChanges</c> as the entities, in
    /// one transaction. When the outbox or the save throws, the rows the outbox added are detached and the after-save
    /// events are put back on their entities, so a retry stores them once.
    /// </para>
    /// <para>
    /// Without an outbox, after-save events are published once the save has completed. Inside an explicit transaction
    /// they are published before that transaction commits; <see cref="DeferDomainEventPublishingUntilCommit"/> moves
    /// publishing after the commit of an <c>EfCoreTransactionRunner</c> unit. When the save fails, the after-save events
    /// are put back on their entities so a retry dispatches them. When <see cref="IDomainEventPublisher"/> throws while
    /// publishing them, they stay cleared, because a retry would publish the earlier events twice.
    /// </para>
    /// <para>
    /// Do not combine with <see cref="DomainEventInterceptor"/>; when this is <c>true</c>,
    /// <see cref="DbContextInterceptors.DomainEvents"/> in <see cref="InterceptorsFromServices"/> is ignored.
    /// An interceptor added to the options directly still runs but sees no events, because they are
    /// collected here first.
    /// </para>
    /// </summary>
    protected virtual bool DispatchDomainEventsOnSaveChanges => false;

    /// <summary>
    /// When <c>true</c> and <see cref="DispatchDomainEventsOnSaveChanges"/> is enabled, the default
    /// <see cref="DispatchDomainEventsAsync"/> publishes <see cref="DomainEventTiming.AfterSave"/> events through
    /// <see cref="IDomainEventPublisher"/> only after the outermost commit of the
    /// <c>EfCoreTransactionRunner</c> unit that saved them, and drops them when that unit rolls back or retries.
    /// Outside a runner unit they are published right after the save, as before. Events stored in an
    /// <see cref="IDomainEventOutbox"/> are not deferred: the outbox write must be part of the transaction.
    /// A publisher that throws after the commit surfaces as an <see cref="AggregateException"/> from the runner.
    /// Defaults to <c>false</c>.
    /// </summary>
    protected virtual bool DeferDomainEventPublishingUntilCommit => false;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        DbContextInterceptors interceptors = InterceptorsFromServices;
        if (interceptors == DbContextInterceptors.None)
            return;

        IEnumerable<IInterceptor> existing =
            optionsBuilder.Options.FindExtension<CoreOptionsExtension>()?.Interceptors ?? [];
        HashSet<IInterceptor> attached = [.. existing];

        if (interceptors.HasFlag(DbContextInterceptors.Audit))
            TryAddInterceptor<AuditInterceptor>(optionsBuilder, attached);

        if (interceptors.HasFlag(DbContextInterceptors.DomainEvents) && !DispatchDomainEventsOnSaveChanges)
            TryAddInterceptor<DomainEventInterceptor>(optionsBuilder, attached);

        if (interceptors.HasFlag(DbContextInterceptors.SlowQuery))
            TryAddInterceptor<SlowQueryInterceptor>(optionsBuilder, attached);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (!DispatchDomainEventsOnSaveChanges)
            return base.SaveChanges(acceptAllChangesOnSuccess);

        List<(IDomainEventHolder Entity, IDomainEvent[] Events)> collected = DomainEventCollector.CollectByEntity(this);
        (IDomainEvent[] beforeSave, IDomainEvent[] afterSave) =
            DomainEventCollector.SplitByTiming([.. collected.SelectMany(static c => c.Events)]);
        IDomainEventOutbox? outbox = afterSave.Length > 0 ? ServiceProvider.GetService<IDomainEventOutbox>() : null;
        EntityEntry[] outboxEntries = [];

        int result;
        try
        {
            if (beforeSave.Length > 0)
                DispatchDomainEventsAsync(beforeSave, DomainEventTiming.BeforeSave, CancellationToken.None)
                    .GetAwaiter().GetResult();

            if (outbox is not null)
                outboxEntries = DomainEventCollector.StoreInOutboxAsync(outbox, this, afterSave, CancellationToken.None)
                    .GetAwaiter().GetResult();

            result = base.SaveChanges(acceptAllChangesOnSuccess);
        }
        catch
        {
            DomainEventCollector.Detach(outboxEntries);
            DomainEventCollector.Restore(collected, afterSave);
            throw;
        }

        if (outbox is null && afterSave.Length > 0)
            DispatchDomainEventsAsync(afterSave, DomainEventTiming.AfterSave, CancellationToken.None)
                .GetAwaiter().GetResult();

        return result;
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (!DispatchDomainEventsOnSaveChanges)
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

        List<(IDomainEventHolder Entity, IDomainEvent[] Events)> collected = DomainEventCollector.CollectByEntity(this);
        (IDomainEvent[] beforeSave, IDomainEvent[] afterSave) =
            DomainEventCollector.SplitByTiming([.. collected.SelectMany(static c => c.Events)]);
        IDomainEventOutbox? outbox = afterSave.Length > 0 ? ServiceProvider.GetService<IDomainEventOutbox>() : null;
        EntityEntry[] outboxEntries = [];

        int result;
        try
        {
            if (beforeSave.Length > 0)
                await DispatchDomainEventsAsync(beforeSave, DomainEventTiming.BeforeSave, cancellationToken).ConfigureAwait(false);

            if (outbox is not null)
                outboxEntries = await DomainEventCollector.StoreInOutboxAsync(outbox, this, afterSave, cancellationToken).ConfigureAwait(false);

            result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            DomainEventCollector.Detach(outboxEntries);
            DomainEventCollector.Restore(collected, afterSave);
            throw;
        }

        if (outbox is null && afterSave.Length > 0)
            await DispatchDomainEventsAsync(afterSave, DomainEventTiming.AfterSave, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Dispatches domain events collected during <c>SaveChanges</c> when
    /// <see cref="DispatchDomainEventsOnSaveChanges"/> is enabled. Called only with a non-empty list.
    /// <para>
    /// Default behavior: both timings go to <see cref="IDomainEventPublisher"/>. When an
    /// <see cref="IDomainEventOutbox"/> is registered, <see cref="DomainEventTiming.AfterSave"/> events are stored in it
    /// before the save instead and never reach this method.
    /// Throws <see cref="InvalidOperationException"/> when the publisher is not registered.
    /// </para>
    /// </summary>
    protected virtual async Task DispatchDomainEventsAsync(
        IReadOnlyList<IDomainEvent> domainEvents,
        DomainEventTiming timing,
        CancellationToken cancellationToken)
    {
        if (timing == DomainEventTiming.AfterSave && DeferDomainEventPublishingUntilCommit)
        {
            IDomainEvent[] deferred = [.. domainEvents];
            if (AfterCommitCallbacks.TryDefer(this, token => PublishDomainEventsAsync(deferred, token)))
                return;
        }

        await PublishDomainEventsAsync(domainEvents, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask PublishDomainEventsAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        IDomainEventPublisher publisher = ServiceProvider.GetRequiredService<IDomainEventPublisher>();
        foreach (IDomainEvent domainEvent in domainEvents)
            await publisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
    }

    ~BaseDbContext()
    {
        if (Logger is not null)
            LogContextDestructed(ContextId.InstanceId);
    }

    public override void Dispose()
    {
        LogContextDisposed(ContextId.InstanceId);
        base.Dispose();
        _serviceScope.Dispose();
        GC.SuppressFinalize(this);
    }

    public override async ValueTask DisposeAsync()
    {
        LogContextDisposed(ContextId.InstanceId);
        await base.DisposeAsync().ConfigureAwait(false);
        if (_serviceScope is IAsyncDisposable asyncScope)
            await asyncScope.DisposeAsync().ConfigureAwait(false);
        else
            _serviceScope.Dispose();
        GC.SuppressFinalize(this);
    }

    private void TryAddInterceptor<TInterceptor>(
        DbContextOptionsBuilder optionsBuilder, HashSet<IInterceptor> attached)
        where TInterceptor : class, IInterceptor
    {
        IInterceptor? interceptor = ServiceProvider.GetService<TInterceptor>();
        if (interceptor is not null && attached.Add(interceptor))
            optionsBuilder.AddInterceptors(interceptor);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Context {DbContextInstanceId} created")]
    private partial void LogContextCreated(Guid dbContextInstanceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Context {DbContextInstanceId} destructed")]
    private partial void LogContextDestructed(Guid dbContextInstanceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Context {DbContextInstanceId} disposed")]
    private partial void LogContextDisposed(Guid dbContextInstanceId);
}
