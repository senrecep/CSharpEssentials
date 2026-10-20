using System.Text;
using System.Transactions;

using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Tests.Idempotency;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using Npgsql;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>The EF Core store against PostgreSQL: transactions, concurrency, Observe mode and keyed registration.</summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyRecordStorePostgresTests(IdempotencyPostgresFixture postgres) : IDisposable
{
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan Retention = TimeSpan.FromHours(1);
    private static readonly TimeSpan Blocked = TimeSpan.FromMilliseconds(500);

    private static readonly IIdempotencyResultCodec<string> Codec = IdempotencyResultCodec.Create<string>(
        static value => Encoding.UTF8.GetBytes(value),
        static payload => Encoding.UTF8.GetString(payload.Span));

    private readonly string _namespace = "pg-" + Guid.NewGuid().ToString("N");
    private readonly ServiceProvider _services = postgres.CreateServices();

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task AddIdempotencyRecords_Should_Give_The_Key_Columns_The_C_Collation()
    {
        await using NpgsqlConnection connection = new(postgres.ConnectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(
            "SELECT column_name || ' ' || collation_name FROM information_schema.columns WHERE table_name = 'IdempotencyRecords' AND collation_name IS NOT NULL ORDER BY column_name",
            connection);
        List<string> columns = [];
        await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                columns.Add(reader.GetString(0));
        }

        columns.Should().Equal("Key C", "Namespace C", "Scope C");
    }

    [Fact]
    public async Task Keys_Scopes_And_Namespaces_That_Differ_By_A_Trailing_Space_Should_Be_Distinct_Records()
    {
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> store = Store(context);
        IdempotencyRecordReservation first = await store.TryReserveAsync(new(_namespace, IdempotencyScope.Tenant("abc"), "key-a"), "fp", Lease);

        IdempotencyRecordReservation key = await store.TryReserveAsync(new(_namespace, IdempotencyScope.Tenant("abc"), "key-a "), "other", Lease);
        IdempotencyRecordReservation scope = await store.TryReserveAsync(new(_namespace, IdempotencyScope.Tenant("abc "), "key-a"), "other", Lease);
        IdempotencyRecordReservation @namespace = await store.TryReserveAsync(new(_namespace + " ", IdempotencyScope.Tenant("abc"), "key-a"), "other", Lease);

        new[] { first, key, scope, @namespace }.Should().OnlyContain(reservation => reservation.Status == IdempotencyReservationStatus.Reserved);
        (await context.IdempotencyRecords.CountAsync(record => record.Namespace.StartsWith(_namespace))).Should().Be(4);
    }

    [Fact]
    public async Task A_Rolled_Back_Transaction_Should_Leave_Neither_The_Record_Nor_The_Business_Row()
    {
        IdempotencyRecordKey key = Key("order-1");

        string rolledBack = await RunInTransactionAsync(key, EventId("evt-1"), commit: false);
        int recordsAfterRollback = await CountRecordsAsync(key);
        int paymentsAfterRollback = await CountPaymentsAsync(EventId("evt-1"));
        string committed = await RunInTransactionAsync(key, EventId("evt-1"), commit: true);
        string replayed = await RunInTransactionAsync(key, EventId("evt-1"), commit: true);

        rolledBack.Should().Be("Executed");
        recordsAfterRollback.Should().Be(0);
        paymentsAfterRollback.Should().Be(0);
        committed.Should().Be("Executed");
        replayed.Should().Be("Replayed");
        (await CountRecordsAsync(key)).Should().Be(1);
        (await CountPaymentsAsync(EventId("evt-1"))).Should().Be(1);
    }

    [Fact]
    public async Task A_Concurrent_Reservation_Should_Wait_For_The_First_Transaction_And_Replay_What_It_Committed()
    {
        IdempotencyRecordKey key = Key("order-1");
        await using IdempotencyTestDbContext first = postgres.CreateContext();
        await using IDbContextTransaction transaction = await first.Database.BeginTransactionAsync();
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> firstStore = Store(first);
        IdempotencyRecordReservation reserved = await firstStore.TryReserveAsync(key, "fp", Lease);

        await using IdempotencyTestDbContext second = postgres.CreateContext();
        Task<IdempotencyRecordReservation> waiting = Store(second).TryReserveAsync(key, "fp", Lease).AsTask();
        await Task.Delay(Blocked);
        bool completedWhileFirstWasOpen = waiting.IsCompleted;
        (await firstStore.CompleteAsync(key, reserved.Token!, "fp", "response"u8.ToArray(), Retention)).Should().BeTrue();
        await transaction.CommitAsync();
        IdempotencyRecordReservation replay = await waiting.WaitAsync(TimeSpan.FromSeconds(10));

        reserved.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        completedWhileFirstWasOpen.Should().BeFalse();
        replay.Status.Should().Be(IdempotencyReservationStatus.Completed);
        replay.Payload!.Value.ToArray().Should().Equal("response"u8.ToArray());
    }

    [Fact]
    public async Task A_Concurrent_Reservation_Should_Win_When_The_First_Transaction_Rolls_Back()
    {
        IdempotencyRecordKey key = Key("order-1");
        await using IdempotencyTestDbContext first = postgres.CreateContext();
        await using IDbContextTransaction transaction = await first.Database.BeginTransactionAsync();
        (await Store(first).TryReserveAsync(key, "fp", Lease)).Status.Should().Be(IdempotencyReservationStatus.Reserved);

        await using IdempotencyTestDbContext second = postgres.CreateContext();
        Task<IdempotencyRecordReservation> waiting = Store(second).TryReserveAsync(key, "fp", Lease).AsTask();
        await Task.Delay(Blocked);
        bool completedWhileFirstWasOpen = waiting.IsCompleted;
        await transaction.RollbackAsync();
        IdempotencyRecordReservation reservation = await waiting.WaitAsync(TimeSpan.FromSeconds(10));

        completedWhileFirstWasOpen.Should().BeFalse();
        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Concurrent_Transactions_Should_Reserve_A_Key_Exactly_Once()
    {
        IdempotencyRecordKey key = Key("order-1");
        using Barrier barrier = new(16);

        IdempotencyReservationStatus[] statuses = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(async () =>
        {
            await using IdempotencyTestDbContext context = postgres.CreateContext();
            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            barrier.SignalAndWait();
            IdempotencyRecordReservation reservation = await Store(context).TryReserveAsync(key, "fp", Lease);
            await Task.Delay(50);
            await transaction.CommitAsync();
            return reservation.Status;
        })));

        statuses.Count(status => status == IdempotencyReservationStatus.Reserved).Should().Be(1);
        statuses.Where(status => status != IdempotencyReservationStatus.Reserved).Should().OnlyContain(status => status == IdempotencyReservationStatus.InFlight);
    }

    [Fact]
    public async Task A_Conflict_On_The_Plain_Insert_Should_Keep_The_Transaction_Usable()
    {
        IdempotencyRecordKey key = Key("order-1");
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> store = new(context, ScopeFactory, TimeProvider.System, plainInsert: true);

        IdempotencyRecordReservation first = await store.TryReserveAsync(key, "fp", Lease);
        IdempotencyRecordReservation second = await store.TryReserveAsync(key, "fp", Lease);
        context.Payments.Add(new IdempotencyPayment { EventId = EventId("plain"), Amount = 10 });
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        first.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        second.Status.Should().Be(IdempotencyReservationStatus.InFlight);
        (await CountRecordsAsync(key)).Should().Be(1);
        (await CountPaymentsAsync(EventId("plain"))).Should().Be(1);
    }

    [Fact]
    public async Task Observe_Should_Not_Wait_For_The_Business_Transaction_Of_A_Concurrent_Request()
    {
        IdempotencyRecordKey key = Key("order-1");
        TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<string> slow = ObserveInTransactionAsync(key, EventId("evt-slow"), async () =>
        {
            entered.SetResult();
            await gate.Task;
        });
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        string concurrent = await ObserveInTransactionAsync(key, EventId("evt-fast"), () => Task.CompletedTask).WaitAsync(TimeSpan.FromSeconds(10));
        gate.SetResult();
        string first = await slow.WaitAsync(TimeSpan.FromSeconds(10));

        first.Should().Be("Executed");
        concurrent.Should().Be("Executed");
        (await CountPaymentsAsync(EventId("evt-slow"))).Should().Be(1);
        (await CountPaymentsAsync(EventId("evt-fast"))).Should().Be(1);
        (await CountRecordsAsync(key)).Should().Be(1);
    }

    [Fact]
    public async Task Observe_Should_Commit_The_Business_Transaction_When_The_Bookkeeping_Fails()
    {
        IdempotencyRecordKey key = Key("order-1");
        await using ServiceProvider unreachable = postgres.CreateServices(
            connectionString: "Host=127.0.0.1;Port=1;Database=idempotency;Username=nobody;Timeout=2");
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        EfCoreTransactionRunner<IdempotencyTestDbContext> runner = new(context);
        IdempotencyExecutor executor = new(
            new EfCoreIdempotencyRecordStore<IdempotencyTestDbContext>(context, unreachable.GetRequiredService<IServiceScopeFactory>()),
            Monitor(IdempotencyMode.Observe));

        IdempotencyExecution<string> execution = await runner.ExecuteAsync(
            token => executor.ExecuteAsync(key, "fp", null, ct => AddPaymentAsync(context, EventId("evt-1"), ct), Codec, token),
            _ => true);

        execution.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        (await CountPaymentsAsync(EventId("evt-1"))).Should().Be(1);
        (await CountRecordsAsync(key)).Should().Be(0);
    }

    [Fact]
    public async Task Observe_Should_Leave_No_Record_When_The_Ambient_Transaction_Rolls_Back()
    {
        IdempotencyRecordKey key = Key("order-1");

        using (TransactionScope ambient = new(TransactionScopeAsyncFlowOption.Enabled))
        {
            await using IdempotencyTestDbContext context = postgres.CreateContext();
            IdempotencyExecutor executor = new(Store(context), Monitor(IdempotencyMode.Observe));
            IdempotencyExecution<string> execution = await executor.ExecuteAsync(
                key, "fp", null, ct => AddPaymentAsync(context, EventId("evt-1"), ct), Codec);
            execution.Status.Should().Be(IdempotencyExecutionStatus.Executed);
        }

        (await CountPaymentsAsync(EventId("evt-1"))).Should().Be(0);
        (await CountRecordsAsync(key)).Should().Be(0);
    }

    [Fact]
    public async Task CompleteAsync_Should_Write_Without_A_Savepoint_When_The_Connection_Is_Not_Enlisted_In_The_Ambient_Transaction()
    {
        IdempotencyRecordKey key = Key("order-1");
        string connectionString = new NpgsqlConnectionStringBuilder(postgres.ConnectionString) { Enlist = false }.ConnectionString;

        using (TransactionScope ambient = new(TransactionScopeAsyncFlowOption.Enabled))
        {
            await using IdempotencyTestDbContext context = new(new DbContextOptionsBuilder<IdempotencyTestDbContext>().UseNpgsql(connectionString).Options);
            bool completed = await Store(context).CompleteAsync(key, "detached-token", "fp", "done"u8.ToArray(), TimeSpan.FromHours(1));
            completed.Should().BeTrue();
        }

        (await CountRecordsAsync(key)).Should().Be(1);
    }

    [Fact]
    public async Task Profiles_Should_Write_To_The_Store_Their_StoreKey_Names()
    {
        await using ServiceProvider services = postgres.CreateServices(collection => collection
            .AddIdempotencyExecutor(options => options.AddProfile("payments", profile => profile.StoreKey = "payments"))
            .AddEfCoreIdempotencyRecordStore<IdempotencyTestDbContext>("payments"));
        IdempotencyRecordKey payment = Key("payment-1");
        IdempotencyRecordKey order = Key("order-1");

        await using (AsyncServiceScope scope = services.CreateAsyncScope())
        {
            IIdempotencyExecutor executor = scope.ServiceProvider.GetRequiredService<IIdempotencyExecutor>();
            await executor.ExecuteAsync(payment, "fp", new IdempotencyExecutionOptions { Profile = "payments" }, _ => ValueTask.FromResult("paid"), Codec);
            await executor.ExecuteAsync(order, "fp", null, _ => ValueTask.FromResult("ordered"), Codec);
        }

        IIdempotencyRecordStore defaultStore = services.GetRequiredService<IIdempotencyRecordStore>();
        defaultStore.Should().BeOfType<InMemoryIdempotencyRecordStore>();
        (await CountRecordsAsync(payment)).Should().Be(1);
        (await CountRecordsAsync(order)).Should().Be(0);
        (await defaultStore.TryReserveAsync(order, "fp", Lease)).Status.Should().Be(IdempotencyReservationStatus.Completed);
        (await defaultStore.TryReserveAsync(payment, "fp", Lease)).Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task PurgeExpiredAsync_Should_Delete_Only_The_Expired_Records()
    {
        FakeTimeProvider clock = new(new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.Zero));
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> store = new(context, ScopeFactory, clock);
        await store.TryReserveAsync(Key("in-flight"), "fp", TimeSpan.FromSeconds(30));
        IdempotencyRecordReservation completed = await store.TryReserveAsync(Key("completed"), "fp", TimeSpan.FromSeconds(30));
        await store.CompleteAsync(Key("completed"), completed.Token!, "fp", "done"u8.ToArray(), TimeSpan.FromHours(1));

        clock.Advance(TimeSpan.FromSeconds(31));
        int afterLease = await store.PurgeExpiredAsync();
        clock.Advance(TimeSpan.FromHours(1));
        int afterRetention = await store.PurgeExpiredAsync();

        afterLease.Should().Be(1);
        afterRetention.Should().Be(1);
        (await CountRecordsAsync(Key("completed"))).Should().Be(0);
    }

    private IServiceScopeFactory ScopeFactory => _services.GetRequiredService<IServiceScopeFactory>();

    private string EventId(string id) => _namespace + ":" + id;

    private IdempotencyRecordKey Key(string key) => new(_namespace, IdempotencyScope.User("alice"), key);

    private EfCoreIdempotencyRecordStore<IdempotencyTestDbContext> Store(IdempotencyTestDbContext context) => new(context, ScopeFactory);

    private static TestOptionsMonitor<IdempotencyExecutorOptions> Monitor(IdempotencyMode mode) => new(new IdempotencyExecutorOptions { Mode = mode });

    private static async ValueTask<string> AddPaymentAsync(IdempotencyTestDbContext context, string eventId, CancellationToken cancellationToken)
    {
        context.Payments.Add(new IdempotencyPayment { EventId = eventId, Amount = 10 });
        await context.SaveChangesAsync(cancellationToken);
        return eventId;
    }

    private async Task<string> RunInTransactionAsync(IdempotencyRecordKey key, string eventId, bool commit)
    {
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        EfCoreTransactionRunner<IdempotencyTestDbContext> runner = new(context);
        IdempotencyExecutor executor = new(Store(context), Monitor(IdempotencyMode.Enforce));
        IdempotencyExecution<string> execution = await runner.ExecuteAsync(
            token => executor.ExecuteAsync(key, "fp", null, ct => AddPaymentAsync(context, eventId, ct), Codec, token),
            _ => commit);
        return execution.Status.ToString();
    }

    private async Task<string> ObserveInTransactionAsync(IdempotencyRecordKey key, string eventId, Func<Task> afterSave)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IdempotencyTestDbContext context = scope.ServiceProvider.GetRequiredService<IdempotencyTestDbContext>();
        EfCoreTransactionRunner<IdempotencyTestDbContext> runner = new(context);
        IdempotencyExecutor executor = new(Store(context), Monitor(IdempotencyMode.Observe));
        IdempotencyExecution<string> execution = await runner.ExecuteAsync(
            token => executor.ExecuteAsync(key, "fp", null, async ct =>
            {
                string result = await AddPaymentAsync(context, eventId, ct);
                await afterSave();
                return result;
            }, Codec, token),
            _ => true);
        return execution.Status.ToString();
    }

    private async Task<int> CountRecordsAsync(IdempotencyRecordKey key)
    {
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        return await context.IdempotencyRecords.CountAsync(record => record.Namespace == key.Namespace && record.Key == key.Key);
    }

    private async Task<int> CountPaymentsAsync(string eventId)
    {
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        return await context.Payments.CountAsync(payment => payment.EventId == eventId);
    }
}
