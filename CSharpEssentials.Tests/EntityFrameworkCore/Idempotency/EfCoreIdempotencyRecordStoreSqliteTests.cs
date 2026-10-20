using System.Text;

using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Tests.Idempotency;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>The EF Core store against SQLite: <c>ON CONFLICT DO NOTHING</c>, the savepoint path and transactions.</summary>
public sealed class EfCoreIdempotencyRecordStoreSqliteTests : IDisposable
{
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan Retention = TimeSpan.FromHours(1);

    private static readonly IIdempotencyResultCodec<string> Codec = IdempotencyResultCodec.Create<string>(
        static value => Encoding.UTF8.GetBytes(value),
        static payload => Encoding.UTF8.GetString(payload.Span));

    private static readonly IdempotencyRecordKey Key = new("orders", IdempotencyScope.User("alice"), "order-1");

    private static readonly IdempotencyRecordKey PaddedKey = new("orders", IdempotencyScope.User("alice"), "order-1 ");

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<IdempotencyTestDbContext> _options;
    private readonly ServiceProvider _services;

    public EfCoreIdempotencyRecordStoreSqliteTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _connection.CreateCollation(
            CustomTableIdempotencyDbContext.Collation,
            static (left, right) => string.Compare(left.TrimEnd(' '), right.TrimEnd(' '), StringComparison.OrdinalIgnoreCase));
        _options = new DbContextOptionsBuilder<IdempotencyTestDbContext>().UseSqlite(_connection).Options;
        _services = new ServiceCollection().AddDbContext<IdempotencyTestDbContext>(options => options.UseSqlite(_connection)).BuildServiceProvider();

        using IdempotencyTestDbContext context = new(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task A_Rolled_Back_Transaction_Should_Leave_Neither_The_Record_Nor_The_Business_Row()
    {
        IdempotencyExecutionStatus rolledBack = await RunInTransactionAsync(commit: false);
        int recordsAfterRollback = await CountAsync(context => context.IdempotencyRecords);
        int paymentsAfterRollback = await CountAsync(context => context.Payments);
        IdempotencyExecutionStatus committed = await RunInTransactionAsync(commit: true);
        IdempotencyExecutionStatus replayed = await RunInTransactionAsync(commit: true);

        rolledBack.Should().Be(IdempotencyExecutionStatus.Executed);
        recordsAfterRollback.Should().Be(0);
        paymentsAfterRollback.Should().Be(0);
        committed.Should().Be(IdempotencyExecutionStatus.Executed);
        replayed.Should().Be(IdempotencyExecutionStatus.Replayed);
        (await CountAsync(context => context.IdempotencyRecords)).Should().Be(1);
        (await CountAsync(context => context.Payments)).Should().Be(1);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task A_Second_Reservation_Should_Be_InFlight_And_Keep_The_Transaction_Usable(bool plainInsert, bool inTransaction)
    {
        await using IdempotencyTestDbContext context = new(_options);
        IDbContextTransaction? transaction = inTransaction ? await context.Database.BeginTransactionAsync() : null;
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> store = new(context, ScopeFactory, TimeProvider.System, plainInsert);

        IdempotencyRecordReservation first = await store.TryReserveAsync(Key, "fp", Lease);
        IdempotencyRecordReservation second = await store.TryReserveAsync(Key, "fp", Lease);
        IdempotencyRecordReservation mismatch = await store.TryReserveAsync(Key, "other", Lease);
        context.Payments.Add(new IdempotencyPayment { EventId = "evt-1", Amount = 10 });
        await context.SaveChangesAsync();
        if (transaction is not null)
        {
            await transaction.CommitAsync();
            await transaction.DisposeAsync();
        }

        first.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        second.Status.Should().Be(IdempotencyReservationStatus.InFlight);
        mismatch.Status.Should().Be(IdempotencyReservationStatus.FingerprintMismatch);
        (await CountAsync(context => context.IdempotencyRecords)).Should().Be(1);
        (await CountAsync(context => context.Payments)).Should().Be(1);
    }

    [Fact]
    public async Task CompleteAsync_Should_Insert_The_Record_When_It_Was_Purged()
    {
        FakeTimeProvider clock = new();
        await using IdempotencyTestDbContext context = new(_options);
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> store = new(context, ScopeFactory, clock);
        IdempotencyRecordReservation reservation = await store.TryReserveAsync(Key, "fp", Lease);
        clock.Advance(Lease + TimeSpan.FromSeconds(1));
        await store.PurgeExpiredAsync();

        bool completed = await store.CompleteAsync(Key, reservation.Token!, "fp", "done"u8.ToArray(), TimeSpan.FromHours(1));
        IdempotencyRecordReservation replay = await store.TryReserveAsync(Key, "fp", Lease);

        completed.Should().BeTrue();
        replay.Status.Should().Be(IdempotencyReservationStatus.Completed);
        replay.Payload!.Value.ToArray().Should().Equal("done"u8.ToArray());
    }

    [Fact]
    public async Task PurgeExpiredAsync_Should_Delete_Only_The_Expired_Records()
    {
        FakeTimeProvider clock = new();
        await using IdempotencyTestDbContext context = new(_options);
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> store = new(context, ScopeFactory, clock);
        await store.TryReserveAsync(Key, "fp", TimeSpan.FromSeconds(30));
        await store.TryReserveAsync(Key with { }, "fp", TimeSpan.FromSeconds(30));
        await store.TryReserveAsync(new IdempotencyRecordKey("orders", null, "global"), "fp", TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromSeconds(31));
        int purged = await store.PurgeExpiredAsync();

        purged.Should().Be(1);
        (await CountAsync(records => records.IdempotencyRecords)).Should().Be(1);
    }

    [Fact]
    public async Task Keys_Should_Stay_Distinct_Under_A_Case_Insensitive_Padding_Collation()
    {
        await using CustomTableIdempotencyDbContext context = await CreateCustomTableContextAsync();
        EfCoreIdempotencyRecordStore<CustomTableIdempotencyDbContext> store = new(context, ScopeFactory);
        IdempotencyRecordReservation reserved = await store.TryReserveAsync(Key, "fp", Lease);

        IdempotencyRecordReservation otherCase = await store.TryReserveAsync(new IdempotencyRecordKey("orders", IdempotencyScope.User("alice"), "ORDER-1"), "other", Lease);
        IdempotencyRecordReservation trailingSpace = await store.TryReserveAsync(PaddedKey, "other", Lease);
        IdempotencyRecordReservation paddedScope = await store.TryReserveAsync(new IdempotencyRecordKey("orders", IdempotencyScope.User("alice "), "order-1"), "other", Lease);

        new[] { reserved, otherCase, trailingSpace, paddedScope }.Should().OnlyContain(reservation => reservation.Status == IdempotencyReservationStatus.Reserved);
        (await context.Set<IdempotencyRecord>().CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task CompleteAsync_Should_Not_Complete_A_Key_That_Only_Matches_Under_The_Collation()
    {
        await using CustomTableIdempotencyDbContext context = await CreateCustomTableContextAsync();
        EfCoreIdempotencyRecordStore<CustomTableIdempotencyDbContext> store = new(context, ScopeFactory);
        IdempotencyRecordReservation padded = await store.TryReserveAsync(PaddedKey, "fp", Lease);
        IdempotencyRecordReservation exact = await store.TryReserveAsync(Key, "fp", Lease);

        bool completed = await store.CompleteAsync(Key, exact.Token!, "fp", "exact"u8.ToArray(), Retention);
        IdempotencyRecordReservation paddedAfter = await store.TryReserveAsync(PaddedKey, "fp", Lease);

        completed.Should().BeTrue();
        padded.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        paddedAfter.Status.Should().Be(IdempotencyReservationStatus.InFlight);
    }

    [Fact]
    public async Task The_Store_Should_Neither_Return_Nor_Change_A_Row_Of_Its_Hash_That_Holds_Another_Key()
    {
        await using IdempotencyTestDbContext context = new(_options);
        context.IdempotencyRecords.Add(new IdempotencyRecord
        {
            KeyHash = IdempotencyRecord.HashKey(Key),
            Namespace = "orders",
            Scope = "user:mallory",
            Key = "order-1",
            Token = "token",
            Fingerprint = "fp",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        });
        await context.SaveChangesAsync();
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> store = new(context, ScopeFactory);

        Func<Task> reserve = () => store.TryReserveAsync(Key, "fp", Lease).AsTask();
        bool completed = await store.CompleteAsync(Key, "token", "fp", "replayed"u8.ToArray(), Retention);
        bool released = await store.ReleaseAsync(Key, "token");

        await reserve.Should().ThrowAsync<InvalidOperationException>().WithMessage("*another key*");
        completed.Should().BeFalse();
        released.Should().BeFalse();
        (await context.IdempotencyRecords.AsNoTracking().SingleAsync()).Payload.Should().BeNull();
    }

    [Fact]
    public void HashKey_Should_Hash_The_Length_Prefixed_Storage_Key()
    {
        byte[] hash = IdempotencyRecord.HashKey(Key);

        Convert.ToHexStringLower(hash).Should().Be("c7a35ec1f21705c6264e444789a8190892426b3b2849089322cf8ebd7bc3fe70", "idem:efcore:v1:6:orders|10:user:alice|7:order-1");
    }

    [Fact]
    public async Task The_Store_Should_Throw_When_The_Context_Does_Not_Map_The_Records()
    {
        await using NotesDbContext context = new(new DbContextOptionsBuilder<NotesDbContext>().UseSqlite(_connection).Options);
        EfCoreIdempotencyRecordStore<NotesDbContext> store = new(context, ScopeFactory);

        Func<Task> reserve = () => store.TryReserveAsync(Key, "fp", Lease).AsTask();

        await reserve.Should().ThrowAsync<InvalidOperationException>().WithMessage("*AddIdempotencyRecords*");
    }

    private IServiceScopeFactory ScopeFactory => _services.GetRequiredService<IServiceScopeFactory>();

    private async Task<CustomTableIdempotencyDbContext> CreateCustomTableContextAsync()
    {
        CustomTableIdempotencyDbContext context = new(new DbContextOptionsBuilder<CustomTableIdempotencyDbContext>().UseSqlite(_connection).Options);
        await context.Database.ExecuteSqlRawAsync(context.Database.GenerateCreateScript());
        return context;
    }

    private async Task<IdempotencyExecutionStatus> RunInTransactionAsync(bool commit)
    {
        await using IdempotencyTestDbContext context = new(_options);
        EfCoreTransactionRunner<IdempotencyTestDbContext> runner = new(context);
        IdempotencyExecutor executor = new(
            new EfCoreIdempotencyRecordStore<IdempotencyTestDbContext>(context, ScopeFactory),
            new TestOptionsMonitor<IdempotencyExecutorOptions>(new IdempotencyExecutorOptions()));
        IdempotencyExecution<string> execution = await runner.ExecuteAsync(
            token => executor.ExecuteAsync(Key, "fp", null, async ct =>
            {
                context.Payments.Add(new IdempotencyPayment { EventId = "evt-1", Amount = 10 });
                await context.SaveChangesAsync(ct);
                return "paid";
            }, Codec, token),
            _ => commit);
        return execution.Status;
    }

    private async Task<int> CountAsync<TEntity>(Func<IdempotencyTestDbContext, IQueryable<TEntity>> query)
    {
        await using IdempotencyTestDbContext context = new(_options);
        return await query(context).CountAsync();
    }
}
