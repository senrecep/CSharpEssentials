using System.Text.Json;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Outbox;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Outbox;

public sealed class OutboxStoreTests : IDisposable
{
    private readonly SqliteConnection _connection = OutboxTestTypes.OpenConnection();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 5, 6, 7, 8, 9, TimeSpan.Zero));
    private readonly ServiceProvider _provider;

    public OutboxStoreTests()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(_clock);
        services.AddSingleton<DomainEventInterceptor>();
        services.AddEfCoreOutbox(OutboxTestTypes.Register);
        _provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }

    private DispatchingOutboxDbContext CreateDispatchingContext()
    {
        DispatchingOutboxDbContext context = new(
            new DbContextOptionsBuilder<DispatchingOutboxDbContext>().UseSqlite(_connection).Options,
            _provider.GetRequiredService<IServiceScopeFactory>());
        context.Database.EnsureCreated();
        return context;
    }

    private PlainOutboxDbContext CreateInterceptedContext()
    {
        PlainOutboxDbContext context = new(new DbContextOptionsBuilder<PlainOutboxDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(_provider.GetRequiredService<DomainEventInterceptor>())
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task SaveChangesAsync_Should_WriteOutboxRowWithTheEntity_When_BaseDbContextDispatches()
    {
        await using DispatchingOutboxDbContext context = CreateDispatchingContext();
        OutboxOrder order = new();
        OrderPlaced placed = new(order.Id);
        order.Raise(placed);
        context.Orders.Add(order);

        int written = await context.SaveChangesAsync();

        written.Should().Be(2);
        OutboxMessage message = await context.OutboxMessages.AsNoTracking().SingleAsync();
        message.Id.Should().Be(placed.EventId);
        message.Type.Should().Be("orders.placed");
        message.OccurredAt.Should().Be(placed.OccurredAt.UtcDateTime);
        message.NextAttemptAt.Should().Be(_clock.GetUtcNow().UtcDateTime);
        message.Attempts.Should().Be(0);
        message.ProcessedAt.Should().BeNull();
        JsonSerializer.Deserialize(message.Content, OutboxTestJsonContext.Default.OrderPlaced).Should().Be(placed);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_WriteOutboxRowWithTheEntity_When_InterceptorDispatches()
    {
        await using PlainOutboxDbContext context = CreateInterceptedContext();
        OutboxOrder order = new();
        order.Raise(new OrderPlaced(order.Id));
        order.Raise(new LegacyEvent("second"));
        context.Orders.Add(order);

        int written = await context.SaveChangesAsync();

        written.Should().Be(3);
        (await context.OutboxMessages.Select(m => m.Type).ToListAsync()).Should().BeEquivalentTo("orders.placed", "legacy");
    }

    [Fact]
    public async Task StoreAsync_Should_StampPlainEventsWithClockAndVersion7Id_When_EventHasNoIdentity()
    {
        await using DispatchingOutboxDbContext context = CreateDispatchingContext();
        OutboxOrder order = new();
        order.Raise(new LegacyEvent("plain"));
        context.Orders.Add(order);

        await context.SaveChangesAsync();

        OutboxMessage message = await context.OutboxMessages.AsNoTracking().SingleAsync();
        message.OccurredAt.Should().Be(_clock.GetUtcNow().UtcDateTime);
        message.Id.Version.Should().Be(7);
        message.Content.Should().Be("""{"Name":"plain"}""");
    }

    [Fact]
    public async Task SaveChangesAsync_Should_SaveNothingAndRestoreEvents_When_EventTypeIsNotRegistered()
    {
        await using DispatchingOutboxDbContext context = CreateDispatchingContext();
        OutboxOrder order = new();
        UnregisteredEvent unknown = new("x");
        order.Raise(unknown);
        context.Orders.Add(order);

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*UnregisteredEvent*not registered*");
        order.DomainEvents.Should().Equal(unknown);
        (await context.Orders.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Throw_When_ContextDoesNotMapOutboxMessage()
    {
        await using UnmappedOutboxDbContext context = new(
            new DbContextOptionsBuilder<UnmappedOutboxDbContext>().UseSqlite(_connection).Options,
            _provider.GetRequiredService<IServiceScopeFactory>());
        await context.Database.EnsureCreatedAsync();
        OutboxOrder order = new();
        order.Raise(new OrderPlaced(order.Id));
        context.Orders.Add(order);

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*AddOutboxMessages*");
    }

    [Fact]
    public async Task SaveChangesAsync_Should_DiscardOutboxRowWithTheEntity_When_TransactionRollsBack()
    {
        await using DispatchingOutboxDbContext context = CreateDispatchingContext();
        OutboxOrder order = new();
        order.Raise(new OrderPlaced(order.Id));
        context.Orders.Add(order);

        await using (IDbContextTransaction transaction = await context.Database.BeginTransactionAsync())
        {
            await context.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        context.ChangeTracker.Clear();
        (await context.Orders.CountAsync()).Should().Be(0);
        (await context.OutboxMessages.CountAsync()).Should().Be(0);
    }

    [Fact]
    public void AddOutboxMessages_Should_UseConfiguredTableAndSchema()
    {
        ModelBuilder builder = new();

        builder.AddOutboxMessages(options =>
        {
            options.TableName = "outbox";
            options.Schema = "messaging";
        });

        Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entity = builder.Model.FindEntityType(typeof(OutboxMessage))!;
        entity.GetTableName().Should().Be("outbox");
        entity.GetSchema().Should().Be("messaging");
        entity.GetIndexes().Should().ContainSingle();
    }

    [Fact]
    public void AddOutboxMessages_Should_Throw_When_TableNameIsEmpty()
    {
        Action act = () => new ModelBuilder().AddOutboxMessages(options => options.TableName = "");

        act.Should().Throw<ArgumentException>();
    }
}

public sealed class OutboxSerializerTests
{
    private static JsonOutboxMessageSerializer CreateSerializer()
    {
        OutboxEventTypeRegistry registry = new();
        OutboxTestTypes.Register(registry);
        return new JsonOutboxMessageSerializer(registry);
    }

    [Fact]
    public void Deserialize_Should_ReturnTheOriginalEvent_When_SerializedBySameRegistry()
    {
        JsonOutboxMessageSerializer serializer = CreateSerializer();
        OrderPlaced placed = new(Guid.NewGuid());

        OutboxPayload payload = serializer.Serialize(placed);
        IDomainEvent back = serializer.Deserialize(payload.Type, payload.Content);

        back.Should().Be(placed);
    }

    [Fact]
    public void Deserialize_Should_Throw_When_TypeNameIsUnknown()
    {
        Action act = () => CreateSerializer().Deserialize("nope", "{}");

        act.Should().Throw<InvalidOperationException>().WithMessage("*'nope'*");
    }

    [Fact]
    public void Add_Should_SerializeWithReflection_When_NoTypeInfoIsGiven()
    {
        OutboxEventTypeRegistry registry = new();
        registry.Add<LegacyEvent>("legacy");
        JsonOutboxMessageSerializer serializer = new(registry);

        OutboxPayload payload = serializer.Serialize(new LegacyEvent("r"));

        payload.Should().Be(new OutboxPayload("legacy", """{"Name":"r"}"""));
    }

    [Fact]
    public void Add_Should_Throw_When_NameOrTypeIsAlreadyRegistered()
    {
        OutboxEventTypeRegistry registry = new();
        registry.Add("a", OutboxTestJsonContext.Default.LegacyEvent);

        Action sameName = () => registry.Add("a", OutboxTestJsonContext.Default.OrderPlaced);
        Action sameType = () => registry.Add("b", OutboxTestJsonContext.Default.LegacyEvent);
        Action tooLong = () => registry.Add(new string('x', 257), OutboxTestJsonContext.Default.OrderPlaced);

        sameName.Should().Throw<ArgumentException>();
        sameType.Should().Throw<ArgumentException>();
        tooLong.Should().Throw<ArgumentException>();
    }
}
