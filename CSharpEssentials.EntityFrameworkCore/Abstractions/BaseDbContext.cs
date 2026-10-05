using CSharpEssentials.Core;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore;

public abstract partial class BaseDbContext<TContext> : DbContext
    where TContext : DbContext
{
    private readonly Guid _instanceId = Guider.NewGuid();
    protected readonly ILogger<TContext> Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseDbContext(
        DbContextOptions<TContext> options, IServiceScopeFactory serviceScopeFactory) : base(options)
    {
        IServiceProvider serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<TContext>>();
        ServiceProvider = serviceProvider;
        LogContextCreated(_instanceId);
    }

    /// <summary>
    /// Interceptors resolved from DI and attached in <see cref="OnConfiguring"/>.
    /// Interceptors that are not registered in DI, or that are already present on the options, are skipped.
    /// Defaults to <see cref="DbContextInterceptors.None"/>.
    /// </summary>
    protected virtual DbContextInterceptors InterceptorsFromServices => DbContextInterceptors.None;

    /// <summary>
    /// When <c>true</c>, <c>SaveChanges</c>/<c>SaveChangesAsync</c> collect domain events from tracked
    /// <see cref="IDomainEventHolder"/> entities and pass them to <see cref="DispatchDomainEventsAsync"/>:
    /// <see cref="DomainEventTiming.BeforeSave"/> events before the save, <see cref="DomainEventTiming.AfterSave"/>
    /// events after it succeeds. Defaults to <c>false</c>.
    /// <para>
    /// Do not combine with <see cref="DomainEventInterceptor"/>; once collected here, events are cleared
    /// from the entities and the interceptor will not see them.
    /// </para>
    /// </summary>
    protected virtual bool DispatchDomainEventsOnSaveChanges => false;

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

        if (interceptors.HasFlag(DbContextInterceptors.DomainEvents))
            TryAddInterceptor<DomainEventInterceptor>(optionsBuilder, attached);

        if (interceptors.HasFlag(DbContextInterceptors.SlowQuery))
            TryAddInterceptor<SlowQueryInterceptor>(optionsBuilder, attached);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (!DispatchDomainEventsOnSaveChanges)
            return base.SaveChanges(acceptAllChangesOnSuccess);

        (IDomainEvent[] beforeSave, IDomainEvent[] afterSave) =
            DomainEventCollector.SplitByTiming(DomainEventCollector.Collect(this));

        if (beforeSave.Length > 0)
            DispatchDomainEventsAsync(beforeSave, DomainEventTiming.BeforeSave, CancellationToken.None)
                .GetAwaiter().GetResult();

        int result = base.SaveChanges(acceptAllChangesOnSuccess);

        if (afterSave.Length > 0)
            DispatchDomainEventsAsync(afterSave, DomainEventTiming.AfterSave, CancellationToken.None)
                .GetAwaiter().GetResult();

        return result;
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (!DispatchDomainEventsOnSaveChanges)
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        (IDomainEvent[] beforeSave, IDomainEvent[] afterSave) =
            DomainEventCollector.SplitByTiming(DomainEventCollector.Collect(this));

        if (beforeSave.Length > 0)
            await DispatchDomainEventsAsync(beforeSave, DomainEventTiming.BeforeSave, cancellationToken);

        int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        if (afterSave.Length > 0)
            await DispatchDomainEventsAsync(afterSave, DomainEventTiming.AfterSave, cancellationToken);

        return result;
    }

    /// <summary>
    /// Dispatches domain events collected during <c>SaveChanges</c> when
    /// <see cref="DispatchDomainEventsOnSaveChanges"/> is enabled. Called only with a non-empty list.
    /// <para>
    /// Default behavior: <see cref="DomainEventTiming.AfterSave"/> events go to <see cref="IDomainEventOutbox"/>
    /// when one is registered, otherwise to <see cref="IDomainEventPublisher"/>;
    /// <see cref="DomainEventTiming.BeforeSave"/> events always go to <see cref="IDomainEventPublisher"/>.
    /// Throws <see cref="InvalidOperationException"/> when the required publisher is not registered.
    /// </para>
    /// </summary>
    protected virtual async Task DispatchDomainEventsAsync(
        IReadOnlyList<IDomainEvent> domainEvents,
        DomainEventTiming timing,
        CancellationToken cancellationToken)
    {
        if (timing == DomainEventTiming.AfterSave)
        {
            IDomainEventOutbox? outbox = ServiceProvider.GetService<IDomainEventOutbox>();
            if (outbox is not null)
            {
                await outbox.StoreAsync(domainEvents, cancellationToken);
                return;
            }
        }

        IDomainEventPublisher publisher = ServiceProvider.GetRequiredService<IDomainEventPublisher>();
        foreach (IDomainEvent domainEvent in domainEvents)
            await publisher.PublishAsync(domainEvent, cancellationToken);
    }

    ~BaseDbContext()
    {
        LogContextDestructed(_instanceId);
    }

    public override void Dispose()
    {
        LogContextDisposed(_instanceId);
        base.Dispose();
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
