using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class BaseDbContextDefaultsTests
{
    private sealed class RaisedEvent : IDomainEvent;

    private sealed class EventEntity : EntityBase<Guid>
    {
        public EventEntity() => Id = Guid.NewGuid();
    }

    private sealed class PlainDbContext(DbContextOptions<PlainDbContext> options, IServiceScopeFactory serviceScopeFactory)
        : BaseDbContext<PlainDbContext>(options, serviceScopeFactory)
    {
        public DbSet<EventEntity> Entities => Set<EventEntity>();
    }

    private sealed class SyncOnlyScope(IServiceProvider serviceProvider) : IServiceScope
    {
        public bool Disposed { get; private set; }

        public IServiceProvider ServiceProvider { get; } = serviceProvider;

        public void Dispose() => Disposed = true;
    }

    private sealed class SyncOnlyScopeFactory(IServiceProvider serviceProvider) : IServiceScopeFactory
    {
        public SyncOnlyScope? Created { get; private set; }

        public IServiceScope CreateScope() => Created = new SyncOnlyScope(serviceProvider);
    }

    private static ServiceProvider BuildProvider(bool withInterceptors)
    {
        ServiceCollection services = new();
        services.AddLogging();
        if (withInterceptors)
        {
            services.AddAuditInterceptor(() => "auditor");
            services.AddSingleton<DomainEventInterceptor>();
        }

        return services.BuildServiceProvider();
    }

    private static PlainDbContext CreateContext(IServiceScopeFactory scopeFactory) =>
        new(new DbContextOptionsBuilder<PlainDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, scopeFactory);

    [Fact]
    public void SaveChanges_Should_LeaveDomainEventsOnEntity_When_DefaultsAreKept()
    {
        using ServiceProvider provider = BuildProvider(withInterceptors: true);
        using PlainDbContext db = CreateContext(provider.GetRequiredService<IServiceScopeFactory>());
        EventEntity entity = new();
        entity.Raise(new RaisedEvent());
        db.Entities.Add(entity);

        int written = db.SaveChanges();

        written.Should().Be(1);
        entity.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RaisedEvent>();
        entity.CreatedBy.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_AttachNoInterceptors_When_DefaultsAreKept()
    {
        await using ServiceProvider provider = BuildProvider(withInterceptors: true);
        await using PlainDbContext db = CreateContext(provider.GetRequiredService<IServiceScopeFactory>());
        EventEntity entity = new();
        entity.Raise(new RaisedEvent());
        db.Entities.Add(entity);

        await db.SaveChangesAsync();

        db.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>()!.Interceptors.Should().BeNullOrEmpty();
        entity.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task DisposeAsync_Should_DisposeScope_When_ScopeIsNotAsyncDisposable()
    {
        await using ServiceProvider provider = BuildProvider(withInterceptors: false);
        SyncOnlyScopeFactory scopeFactory = new(provider);
        PlainDbContext db = CreateContext(scopeFactory);

        await db.DisposeAsync();

        scopeFactory.Created!.Disposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_Should_DisposeScope_When_ScopeIsNotAsyncDisposable()
    {
        using ServiceProvider provider = BuildProvider(withInterceptors: false);
        SyncOnlyScopeFactory scopeFactory = new(provider);
        PlainDbContext db = CreateContext(scopeFactory);

        db.Dispose();

        scopeFactory.Created!.Disposed.Should().BeTrue();
    }
}
