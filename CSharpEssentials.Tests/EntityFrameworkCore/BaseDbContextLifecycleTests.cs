using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class BaseDbContextLifecycleTests
{
    // ── Test types ──────────────────────────────────────────────────

    private sealed class AfterEvent(string name) : IDomainEvent
    {
        public string Name { get; } = name;
    }

    [DomainEventTiming(DomainEventTiming.BeforeSave)]
    private sealed class BeforeEvent(string name) : IDomainEvent
    {
        public string Name { get; } = name;
    }

    private sealed class EventEntity : EntityBase<Guid>
    {
        public EventEntity() => Id = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }

    private sealed record DispatchCall(DomainEventTiming Timing, IReadOnlyList<IDomainEvent> Events, bool EntityPersisted);

    private sealed class LifecycleDbContext(
        DbContextOptions<LifecycleDbContext> options,
        IServiceScopeFactory serviceScopeFactory,
        DbContextInterceptors interceptors = DbContextInterceptors.None,
        bool dispatchDomainEvents = false,
        List<DispatchCall>? recordedCalls = null)
        : BaseDbContext<LifecycleDbContext>(options, serviceScopeFactory)
    {
        public DbSet<EventEntity> Entities => Set<EventEntity>();

        protected override DbContextInterceptors InterceptorsFromServices => interceptors;

        protected override bool DispatchDomainEventsOnSaveChanges => dispatchDomainEvents;

        protected override Task DispatchDomainEventsAsync(
            IReadOnlyList<IDomainEvent> domainEvents, DomainEventTiming timing, CancellationToken cancellationToken)
        {
            if (recordedCalls is null)
                return base.DispatchDomainEventsAsync(domainEvents, timing, cancellationToken);

            bool persisted = ChangeTracker.Entries<EventEntity>().All(e => e.State == EntityState.Unchanged);
            recordedCalls.Add(new DispatchCall(timing, domainEvents, persisted));
            return Task.CompletedTask;
        }
    }

    private sealed class TrackingPublisher : IDomainEventPublisher
    {
        public List<IDomainEvent> Published { get; } = [];

        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Published.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class TrackingOutbox : IDomainEventOutbox
    {
        public List<IDomainEvent> Stored { get; } = [];

        public Task StoreAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            Stored.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
    {
        ServiceCollection services = new();
        services.AddLogging();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    private static LifecycleDbContext CreateContext(
        ServiceProvider provider,
        DbContextInterceptors interceptors = DbContextInterceptors.None,
        bool dispatchDomainEvents = false,
        List<DispatchCall>? recordedCalls = null,
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        DbContextOptionsBuilder<LifecycleDbContext> builder = new DbContextOptionsBuilder<LifecycleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());
        configureOptions?.Invoke(builder);

        return new LifecycleDbContext(
            builder.Options,
            provider.GetRequiredService<IServiceScopeFactory>(),
            interceptors,
            dispatchDomainEvents,
            recordedCalls);
    }

    private static IInterceptor[] AttachedInterceptors(DbContext context) =>
        [.. context.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>()?.Interceptors ?? []];

    private static void RegisterAllInterceptors(IServiceCollection services)
    {
        services.AddAuditInterceptor(() => "auditor");
        services.AddSlowQueryInterceptor();
        services.AddSingleton<DomainEventInterceptor>();
    }

    // ── Interceptor registration ────────────────────────────────────

    [Fact]
    public void OnConfiguring_ShouldNotAttachInterceptors_ByDefault()
    {
        using ServiceProvider provider = BuildProvider(RegisterAllInterceptors);
        using LifecycleDbContext context = CreateContext(provider);

        AttachedInterceptors(context).Should().BeEmpty();
    }

    [Fact]
    public void OnConfiguring_ShouldAttachSelectedInterceptors_WhenRegistered()
    {
        using ServiceProvider provider = BuildProvider(RegisterAllInterceptors);
        using LifecycleDbContext context = CreateContext(
            provider, DbContextInterceptors.Audit | DbContextInterceptors.SlowQuery);

        IInterceptor[] attached = AttachedInterceptors(context);

        attached.Should().HaveCount(2);
        attached.Should().ContainSingle(i => i is AuditInterceptor);
        attached.Should().ContainSingle(i => i is SlowQueryInterceptor);
    }

    [Fact]
    public void OnConfiguring_ShouldAttachAllRegisteredInterceptors_WhenAllSelected()
    {
        using ServiceProvider provider = BuildProvider(RegisterAllInterceptors);
        using LifecycleDbContext context = CreateContext(provider, DbContextInterceptors.All);

        IInterceptor[] attached = AttachedInterceptors(context);

        attached.Should().HaveCount(3);
        attached.Should().ContainSingle(i => i is DomainEventInterceptor);
    }

    [Fact]
    public void OnConfiguring_ShouldSkipInterceptors_WhenNotRegistered()
    {
        using ServiceProvider provider = BuildProvider(services => services.AddSlowQueryInterceptor());
        using LifecycleDbContext context = CreateContext(provider, DbContextInterceptors.All);

        AttachedInterceptors(context).Should().ContainSingle().Which.Should().BeOfType<SlowQueryInterceptor>();
    }

    [Fact]
    public void OnConfiguring_ShouldNotDuplicate_WhenInterceptorAlreadyOnOptions()
    {
        using ServiceProvider provider = BuildProvider(RegisterAllInterceptors);
        AuditInterceptor audit = provider.GetRequiredService<AuditInterceptor>();
        using LifecycleDbContext context = CreateContext(
            provider, DbContextInterceptors.Audit, configureOptions: options => options.AddInterceptors(audit));

        AttachedInterceptors(context).Should().ContainSingle().Which.Should().BeSameAs(audit);
    }

    [Fact]
    public async Task OnConfiguring_AttachedAuditInterceptor_ShouldStampCreatedInfo()
    {
        using ServiceProvider provider = BuildProvider(RegisterAllInterceptors);
        using LifecycleDbContext context = CreateContext(provider, DbContextInterceptors.Audit);
        var entity = new EventEntity { Name = "audited" };

        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        entity.CreatedBy.Should().Be("auditor");
    }

    // ── Domain event dispatch ───────────────────────────────────────

    [Fact]
    public async Task SaveChangesAsync_ShouldNotCollectEvents_WhenDispatchDisabled()
    {
        using ServiceProvider provider = BuildProvider();
        List<DispatchCall> calls = [];
        using LifecycleDbContext context = CreateContext(provider, recordedCalls: calls);
        var entity = new EventEntity();
        entity.Raise(new AfterEvent("kept"));

        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        calls.Should().BeEmpty();
        entity.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchBeforeAndAfterSaveEvents_InLifecycleOrder()
    {
        using ServiceProvider provider = BuildProvider();
        List<DispatchCall> calls = [];
        using LifecycleDbContext context = CreateContext(provider, dispatchDomainEvents: true, recordedCalls: calls);
        var entity = new EventEntity();
        entity.Raise(new BeforeEvent("before"));
        entity.Raise(new AfterEvent("after"));

        context.Entities.Add(entity);
        int written = await context.SaveChangesAsync();

        written.Should().Be(1);
        calls.Should().HaveCount(2);
        calls[0].Timing.Should().Be(DomainEventTiming.BeforeSave);
        calls[0].EntityPersisted.Should().BeFalse();
        calls[0].Events.Should().ContainSingle().Which.Should().BeOfType<BeforeEvent>();
        calls[1].Timing.Should().Be(DomainEventTiming.AfterSave);
        calls[1].EntityPersisted.Should().BeTrue();
        calls[1].Events.Should().ContainSingle().Which.Should().BeOfType<AfterEvent>();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotDispatch_WhenNoEventsRaised()
    {
        using ServiceProvider provider = BuildProvider();
        List<DispatchCall> calls = [];
        using LifecycleDbContext context = CreateContext(provider, dispatchDomainEvents: true, recordedCalls: calls);

        context.Entities.Add(new EventEntity());
        await context.SaveChangesAsync();

        calls.Should().BeEmpty();
    }

    [Fact]
    public void SaveChanges_ShouldDispatchEvents_WhenDispatchEnabled()
    {
        using ServiceProvider provider = BuildProvider();
        List<DispatchCall> calls = [];
        using LifecycleDbContext context = CreateContext(provider, dispatchDomainEvents: true, recordedCalls: calls);
        var entity = new EventEntity();
        entity.Raise(new BeforeEvent("before"));
        entity.Raise(new AfterEvent("after"));

        context.Entities.Add(entity);
        context.SaveChanges();

        calls.Select(c => c.Timing).Should().Equal(DomainEventTiming.BeforeSave, DomainEventTiming.AfterSave);
        calls[1].EntityPersisted.Should().BeTrue();
    }

    [Fact]
    public void SaveChanges_ShouldNotCollectEvents_WhenDispatchDisabled()
    {
        using ServiceProvider provider = BuildProvider();
        List<DispatchCall> calls = [];
        using LifecycleDbContext context = CreateContext(provider, recordedCalls: calls);
        var entity = new EventEntity();
        entity.Raise(new AfterEvent("kept"));

        context.Entities.Add(entity);
        context.SaveChanges();

        calls.Should().BeEmpty();
        entity.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task SaveChangesAsync_DefaultDispatch_ShouldPublishThroughPublisher()
    {
        TrackingPublisher publisher = new();
        using ServiceProvider provider = BuildProvider(services => services.AddSingleton<IDomainEventPublisher>(publisher));
        using LifecycleDbContext context = CreateContext(provider, dispatchDomainEvents: true);
        var entity = new EventEntity();
        entity.Raise(new BeforeEvent("before"));
        entity.Raise(new AfterEvent("after"));

        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        publisher.Published.Select(e => e.GetType()).Should().Equal(typeof(BeforeEvent), typeof(AfterEvent));
    }

    [Fact]
    public async Task SaveChangesAsync_DefaultDispatch_ShouldRouteAfterSaveEventsToOutbox()
    {
        TrackingPublisher publisher = new();
        TrackingOutbox outbox = new();
        using ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IDomainEventPublisher>(publisher);
            services.AddSingleton<IDomainEventOutbox>(outbox);
        });
        using LifecycleDbContext context = CreateContext(provider, dispatchDomainEvents: true);
        var entity = new EventEntity();
        entity.Raise(new BeforeEvent("before"));
        entity.Raise(new AfterEvent("after"));

        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        publisher.Published.Should().ContainSingle().Which.Should().BeOfType<BeforeEvent>();
        outbox.Stored.Should().ContainSingle().Which.Should().BeOfType<AfterEvent>();
    }

    [Fact]
    public async Task SaveChangesAsync_DefaultDispatch_ShouldThrow_WhenPublisherMissing()
    {
        using ServiceProvider provider = BuildProvider();
        using LifecycleDbContext context = CreateContext(provider, dispatchDomainEvents: true);
        var entity = new EventEntity();
        entity.Raise(new BeforeEvent("before"));
        context.Entities.Add(entity);

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await context.Entities.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPreventInterceptorDoubleDispatch_WhenBothEnabled()
    {
        TrackingPublisher publisher = new();
        using ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IDomainEventPublisher>(publisher);
            services.AddSingleton<DomainEventInterceptor>();
        });
        using LifecycleDbContext context = CreateContext(
            provider, DbContextInterceptors.DomainEvents, dispatchDomainEvents: true);
        var entity = new EventEntity();
        entity.Raise(new AfterEvent("once"));

        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        publisher.Published.Should().ContainSingle();
    }

    [Fact]
    public async Task SaveChangesAsync_AttachedDomainEventInterceptor_ShouldDispatch_WhenContextDispatchDisabled()
    {
        TrackingPublisher publisher = new();
        using ServiceProvider provider = BuildProvider(services =>
        {
            services.AddSingleton<IDomainEventPublisher>(publisher);
            services.AddSingleton<DomainEventInterceptor>();
        });
        using LifecycleDbContext context = CreateContext(provider, DbContextInterceptors.DomainEvents);
        var entity = new EventEntity();
        entity.Raise(new AfterEvent("via-interceptor"));

        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        publisher.Published.Should().ContainSingle();
        entity.DomainEvents.Should().BeEmpty();
    }
}
