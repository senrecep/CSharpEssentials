using System.Text.Json.Serialization;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Outbox;

public sealed record OrderPlaced(Guid OrderId) : DomainEvent;

public sealed record LegacyEvent(string Name) : IDomainEvent;

public sealed record UnregisteredEvent(string Name) : IDomainEvent;

[JsonSerializable(typeof(OrderPlaced))]
[JsonSerializable(typeof(LegacyEvent))]
internal sealed partial class OutboxTestJsonContext : JsonSerializerContext;

internal sealed class OutboxOrder : EntityBase<Guid>
{
    public OutboxOrder() => Id = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
}

internal sealed class PlainOutboxDbContext(DbContextOptions<PlainOutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxOrder> Orders => Set<OutboxOrder>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.AddOutboxMessages();
}

internal sealed class DispatchingOutboxDbContext(DbContextOptions<DispatchingOutboxDbContext> options, IServiceScopeFactory scopeFactory)
    : BaseDbContext<DispatchingOutboxDbContext>(options, scopeFactory)
{
    public DbSet<OutboxOrder> Orders => Set<OutboxOrder>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override bool DispatchDomainEventsOnSaveChanges => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.AddOutboxMessages();
}

internal sealed class UnmappedOutboxDbContext(DbContextOptions<UnmappedOutboxDbContext> options, IServiceScopeFactory scopeFactory)
    : BaseDbContext<UnmappedOutboxDbContext>(options, scopeFactory)
{
    public DbSet<OutboxOrder> Orders => Set<OutboxOrder>();

    protected override bool DispatchDomainEventsOnSaveChanges => true;
}

internal static class OutboxTestTypes
{
    public static void Register(OutboxEventTypeRegistry registry) =>
        registry
            .Add("orders.placed", OutboxTestJsonContext.Default.OrderPlaced)
            .Add("legacy", OutboxTestJsonContext.Default.LegacyEvent);

    public static SqliteConnection OpenConnection()
    {
        SqliteConnection connection = new("DataSource=:memory:");
        connection.Open();
        return connection;
    }
}
