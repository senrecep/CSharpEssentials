using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.Tests.Endpoints;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class DomainEventInterceptorDispatchTests : IDisposable
{
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

    private sealed class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
    {
        public DbSet<EventEntity> Entities => Set<EventEntity>();
    }

    private sealed class RecordingPublisher(bool throwOnPublish = false) : IDomainEventPublisher
    {
        public List<IDomainEvent> Published { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];

        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (throwOnPublish)
                throw new InvalidOperationException("Publisher failed.");
            Published.Add(domainEvent);
            Tokens.Add(cancellationToken);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingOutbox : IDomainEventOutbox
    {
        public List<IDomainEvent> Stored { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];

        public Task StoreAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            Stored.AddRange(domainEvents);
            Tokens.Add(cancellationToken);
            return Task.CompletedTask;
        }
    }

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly CapturingLoggerProvider _logs = new();
    private ServiceProvider? _provider;

    public void Dispose()
    {
        _provider?.Dispose();
        _logs.Dispose();
    }

    private EventDbContext CreateContext(RecordingPublisher publisher, RecordingOutbox? outbox = null)
    {
        ServiceCollection services = new();
        services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Debug).AddProvider(_logs));
        services.AddSingleton<IDomainEventPublisher>(publisher);
        if (outbox is not null)
            services.AddSingleton<IDomainEventOutbox>(outbox);
        services.AddSingleton<DomainEventInterceptor>();
        _provider = services.BuildServiceProvider();

        return new EventDbContext(new DbContextOptionsBuilder<EventDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .AddInterceptors(_provider.GetRequiredService<DomainEventInterceptor>())
            .Options);
    }

    private int PersistedCount()
    {
        using EventDbContext reader = new(new DbContextOptionsBuilder<EventDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);
        return reader.Entities.Count();
    }

    private IEnumerable<string> InterceptorMessages() =>
        _logs.Entries
            .Where(entry => entry.Category == typeof(DomainEventInterceptor).FullName && entry.Level == LogLevel.Debug)
            .Select(entry => entry.Message);

    [Fact]
    public async Task SavingChangesAsync_Should_PublishBeforeSaveEvents_And_StoreAfterSaveEventsInOutbox()
    {
        RecordingPublisher publisher = new();
        RecordingOutbox outbox = new();
        await using EventDbContext db = CreateContext(publisher, outbox);
        EventEntity entity = new() { Name = "order" };
        entity.Raise(new BeforeEvent("before"));
        entity.Raise(new AfterEvent("after"));
        db.Entities.Add(entity);

        await db.SaveChangesAsync();

        publisher.Published.Should().ContainSingle().Which.Should().BeOfType<BeforeEvent>();
        outbox.Stored.Should().ContainSingle().Which.Should().BeOfType<AfterEvent>()
            .Which.Name.Should().Be("after");
        entity.DomainEvents.Should().BeEmpty();
        PersistedCount().Should().Be(1);
    }

    [Fact]
    public async Task SavingChangesAsync_Should_PassCallerToken_To_PublisherAndOutbox()
    {
        RecordingPublisher publisher = new();
        RecordingOutbox outbox = new();
        await using EventDbContext db = CreateContext(publisher, outbox);
        using CancellationTokenSource cts = new();
        EventEntity entity = new();
        entity.Raise(new BeforeEvent("before"));
        entity.Raise(new AfterEvent("after"));
        db.Entities.Add(entity);

        await db.SaveChangesAsync(cts.Token);

        publisher.Tokens.Should().Equal(cts.Token);
        outbox.Tokens.Should().Equal(cts.Token);
    }

    [Fact]
    public async Task SavingChangesAsync_Should_PublishAfterSaveEventsInRaiseOrder_When_NoOutbox()
    {
        RecordingPublisher publisher = new();
        await using EventDbContext db = CreateContext(publisher);
        EventEntity entity = new();
        entity.Raise(new AfterEvent("first"));
        entity.Raise(new AfterEvent("second"));
        db.Entities.Add(entity);

        await db.SaveChangesAsync();

        publisher.Published.Cast<AfterEvent>().Select(e => e.Name).Should().Equal("first", "second");
    }

    [Fact]
    public async Task SavingChangesAsync_Should_SaveWithoutPublishing_When_NoEventsRaised()
    {
        RecordingPublisher publisher = new();
        RecordingOutbox outbox = new();
        await using EventDbContext db = CreateContext(publisher, outbox);
        db.Entities.Add(new EventEntity { Name = "quiet" });

        int written = await db.SaveChangesAsync();

        written.Should().Be(1);
        publisher.Published.Should().BeEmpty();
        outbox.Stored.Should().BeEmpty();
        InterceptorMessages().Should().BeEmpty();
    }

    [Fact]
    public async Task SavingChangesAsync_Should_AbortSave_When_BeforeSavePublisherThrows()
    {
        RecordingPublisher publisher = new(throwOnPublish: true);
        await using EventDbContext db = CreateContext(publisher);
        EventEntity entity = new();
        entity.Raise(new BeforeEvent("before"));
        db.Entities.Add(entity);

        Func<Task> act = () => db.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Publisher failed.");
        PersistedCount().Should().Be(0);
    }

    [Fact]
    public void SavingChanges_Should_AbortSave_When_BeforeSavePublisherThrows()
    {
        RecordingPublisher publisher = new(throwOnPublish: true);
        using EventDbContext db = CreateContext(publisher);
        EventEntity entity = new();
        entity.Raise(new BeforeEvent("before"));
        db.Entities.Add(entity);

        Action act = () => db.SaveChanges();

        act.Should().Throw<InvalidOperationException>().WithMessage("Publisher failed.");
        PersistedCount().Should().Be(0);
    }

    [Fact]
    public void SavingChanges_Should_LogPublishAndOutboxCounts_When_DebugEnabled()
    {
        RecordingPublisher publisher = new();
        RecordingOutbox outbox = new();
        using EventDbContext db = CreateContext(publisher, outbox);
        EventEntity entity = new();
        entity.Raise(new BeforeEvent("before"));
        entity.Raise(new AfterEvent("after-1"));
        entity.Raise(new AfterEvent("after-2"));
        db.Entities.Add(entity);

        db.SaveChanges();

        InterceptorMessages().Should().Equal(
            "Publishing 1 domain events",
            "Storing 2 domain events in outbox");
    }
}
