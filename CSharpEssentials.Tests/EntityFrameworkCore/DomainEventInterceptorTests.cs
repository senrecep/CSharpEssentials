using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class DomainEventInterceptorTests
{
    // ── Test domain events ──────────────────────────────────────────

    private sealed class TestEvent(string name) : IDomainEvent
    {
        public string Name { get; } = name;
    }

    [DomainEventTiming(DomainEventTiming.BeforeSave)]
    private sealed class BeforeSaveEvent(string name) : IDomainEvent
    {
        public string Name { get; } = name;
    }

    // ── Test entity and DbContext ───────────────────────────────────

    private sealed class EventEntity : EntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<EventEntity> Entities => Set<EventEntity>();
    }

    // ── Test publisher ──────────────────────────────────────────────

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

    // ── Setup ───────────────────────────────────────────────────────

    private static (TestDbContext Db, TrackingPublisher Publisher, ServiceProvider Provider) CreateContext(
        IDomainEventOutbox? outbox = null)
    {
        ServiceCollection services = new();
        services.AddLogging();

        TrackingPublisher publisher = new();
        services.AddSingleton<IDomainEventPublisher>(publisher);

        if (outbox is not null)
            services.AddSingleton(outbox);

        ServiceProvider provider = services.BuildServiceProvider();
        IServiceScopeFactory scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        ILogger<DomainEventInterceptor> logger = provider.GetRequiredService<ILogger<DomainEventInterceptor>>();

        DomainEventInterceptor interceptor = new(logger, scopeFactory);

        // Also register AuditInterceptor so EntityBase fields are set
        services.AddSingleton(TimeProvider.System);
        services.AddAuditUserIdProvider(() => "test");

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return (new TestDbContext(options), publisher, provider);
    }

    [Fact]
    public void SavingChanges_ShouldPublishDomainEvents()
    {
        (TestDbContext db, TrackingPublisher publisher, ServiceProvider provider) = CreateContext();

        EventEntity entity = new() { Name = "Test" };
        entity.Raise(new TestEvent("Event1"));
        db.Entities.Add(entity);
        db.SaveChanges();

        publisher.Published.Should().HaveCount(1);
        publisher.Published[0].Should().BeOfType<TestEvent>()
            .Which.Name.Should().Be("Event1");

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public void SavingChanges_ShouldClearDomainEventsAfterPublish()
    {
        (TestDbContext db, TrackingPublisher _, ServiceProvider provider) = CreateContext();

        EventEntity entity = new() { Name = "Test" };
        entity.Raise(new TestEvent("Event1"));
        db.Entities.Add(entity);
        db.SaveChanges();

        entity.DomainEvents.Should().BeEmpty();

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public void SavingChanges_ShouldPreservePerEntityEventOrder()
    {
        (TestDbContext db, TrackingPublisher publisher, ServiceProvider provider) = CreateContext();

        EventEntity entity1 = new() { Name = "Entity1" };
        entity1.Raise(new TestEvent("E1-First"));
        entity1.Raise(new TestEvent("E1-Second"));
        entity1.Raise(new TestEvent("E1-Third"));

        EventEntity entity2 = new() { Name = "Entity2" };
        entity2.Raise(new TestEvent("E2-First"));
        entity2.Raise(new TestEvent("E2-Second"));

        db.Entities.AddRange(entity1, entity2);
        db.SaveChanges();

        publisher.Published.Should().HaveCount(5);

        // Entity1's events should maintain their relative order
        List<string> names = publisher.Published.Cast<TestEvent>().Select(e => e.Name).ToList();
        int e1First = names.IndexOf("E1-First");
        int e1Second = names.IndexOf("E1-Second");
        int e1Third = names.IndexOf("E1-Third");
        int e2First = names.IndexOf("E2-First");
        int e2Second = names.IndexOf("E2-Second");

        e1First.Should().BeLessThan(e1Second);
        e1Second.Should().BeLessThan(e1Third);
        e2First.Should().BeLessThan(e2Second);

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public void SavingChanges_BeforeSaveEvents_ShouldPublishBeforeAfterSaveEvents()
    {
        (TestDbContext db, TrackingPublisher publisher, ServiceProvider provider) = CreateContext();

        EventEntity entity = new() { Name = "Test" };
        entity.Raise(new BeforeSaveEvent("Before"));
        entity.Raise(new TestEvent("After"));
        db.Entities.Add(entity);
        db.SaveChanges();

        publisher.Published.Should().HaveCount(2);
        publisher.Published[0].Should().BeOfType<BeforeSaveEvent>();
        publisher.Published[1].Should().BeOfType<TestEvent>();

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public void SavingChanges_WithOutbox_ShouldRouteAfterSaveEventsToOutbox()
    {
        TrackingOutbox outbox = new();
        (TestDbContext db, TrackingPublisher publisher, ServiceProvider provider) = CreateContext(outbox);

        EventEntity entity = new() { Name = "Test" };
        entity.Raise(new BeforeSaveEvent("Before"));
        entity.Raise(new TestEvent("AfterViaOutbox"));
        db.Entities.Add(entity);
        db.SaveChanges();

        // BeforeSave events always go directly to publisher
        publisher.Published.Should().HaveCount(1);
        publisher.Published[0].Should().BeOfType<BeforeSaveEvent>();

        // AfterSave events go to outbox
        outbox.Stored.Should().HaveCount(1);
        outbox.Stored[0].Should().BeOfType<TestEvent>()
            .Which.Name.Should().Be("AfterViaOutbox");

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public void SavingChanges_WithNoEvents_ShouldNotCallPublisher()
    {
        (TestDbContext db, TrackingPublisher publisher, ServiceProvider provider) = CreateContext();

        EventEntity entity = new() { Name = "NoEvents" };
        db.Entities.Add(entity);
        db.SaveChanges();

        publisher.Published.Should().BeEmpty();

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public async Task SavingChangesAsync_ShouldPublishDomainEvents()
    {
        (TestDbContext db, TrackingPublisher publisher, ServiceProvider provider) = CreateContext();

        EventEntity entity = new() { Name = "AsyncTest" };
        entity.Raise(new TestEvent("AsyncEvent"));
        db.Entities.Add(entity);
        await db.SaveChangesAsync();

        publisher.Published.Should().HaveCount(1);
        publisher.Published[0].Should().BeOfType<TestEvent>()
            .Which.Name.Should().Be("AsyncEvent");

        await db.DisposeAsync();
        await provider.DisposeAsync();
    }
}
