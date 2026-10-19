using System.Text;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CSharpEssentials.Idempotency.Testing;

/// <summary>
/// Behavior every <see cref="IIdempotencyRecordStore"/> must have. Derive from it in an xUnit test project and implement
/// <see cref="CreateStore"/>; every scenario then runs against your store.
/// <para>
/// By default the store gets a <see cref="FakeTimeProvider"/> and expiry is tested by advancing it. A store whose
/// expiry runs on its own clock (Redis TTL, a database <c>now()</c>) cannot follow a fake clock: override
/// <see cref="UseRealClock"/> to pass <see cref="TimeProvider.System"/> and wait instead, with short
/// <see cref="Lease"/> and <see cref="Retention"/> values. Every test instance uses its own <see cref="Namespace"/>, so
/// the scenarios can share one database or cache.
/// </para>
/// </summary>
public abstract class IdempotencyRecordStoreContractTests
{
    private const int ParallelReservations = 32;

    /// <summary>Namespace of every key created by <see cref="Key(string, IdempotencyScope?)"/>; unique per test instance.</summary>
    protected string Namespace { get; } = "contract-" + Guid.NewGuid().ToString("N");

    /// <summary>
    /// <see langword="false"/> (the default) runs expiry against a <see cref="FakeTimeProvider"/>.
    /// <see langword="true"/> runs it against <see cref="TimeProvider.System"/> and real waits.
    /// </summary>
    protected virtual bool UseRealClock => false;

    /// <summary>
    /// <see langword="true"/> (the default) when <see cref="IIdempotencyRecordStore.TryReserveAsync"/> is atomic: of
    /// concurrent reservations for one key exactly one is <see cref="IdempotencyReservationStatus.Reserved"/>. Override
    /// it with <see langword="false"/> for a best-effort store; the concurrency tests then only check that every result
    /// is <see cref="IdempotencyReservationStatus.Reserved"/> or <see cref="IdempotencyReservationStatus.InFlight"/>.
    /// </summary>
    protected virtual bool IsReservationAtomic => true;

    /// <summary>Lease of reservations: 30 seconds, or 1 second with <see cref="UseRealClock"/>.</summary>
    protected virtual TimeSpan Lease => UseRealClock ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(30);

    /// <summary>Retention of completed entries: 1 hour, or 3 seconds with <see cref="UseRealClock"/>.</summary>
    protected virtual TimeSpan Retention => UseRealClock ? TimeSpan.FromSeconds(3) : TimeSpan.FromHours(1);

    /// <summary>Time added past a lease or retention to be sure it expired: 1 second, or 500 ms with <see cref="UseRealClock"/>.</summary>
    protected virtual TimeSpan ExpirySlack => UseRealClock ? TimeSpan.FromMilliseconds(500) : TimeSpan.FromSeconds(1);

    /// <summary>The clock passed to <see cref="CreateStore"/>.</summary>
    protected TimeProvider Clock => field ??= UseRealClock
        ? TimeProvider.System
        : new FakeTimeProvider(new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero));

    /// <summary>The store under test, created on first use.</summary>
    protected IIdempotencyRecordStore Store => field ??= CreateStore(Clock);

    /// <summary>Creates the store under test. Its leases and retention must follow <paramref name="timeProvider"/>.</summary>
    protected abstract IIdempotencyRecordStore CreateStore(TimeProvider timeProvider);

    /// <summary>A key in <see cref="Namespace"/> owned by the user <c>user-1</c>.</summary>
    protected IdempotencyRecordKey Key(string key) => Key(key, IdempotencyScope.User("user-1"));

    /// <summary>A key in <see cref="Namespace"/>; <paramref name="scope"/> <see langword="null"/> gives a global key.</summary>
    protected IdempotencyRecordKey Key(string key, IdempotencyScope? scope) => new(Namespace, scope, key);

    /// <summary>Moves the fake clock forward, or waits with <see cref="UseRealClock"/>.</summary>
    protected Task AdvanceAsync(TimeSpan duration)
    {
        if (Clock is FakeTimeProvider fake)
        {
            fake.Advance(duration);
            return Task.CompletedTask;
        }

        return Task.Delay(duration);
    }

    /// <summary>Reserves <paramref name="key"/>, asserts it was reserved and returns the token.</summary>
    protected async Task<string> ReserveAsync(IdempotencyRecordKey key, string fingerprint = "fp")
    {
        IdempotencyRecordReservation reservation = await Store.TryReserveAsync(key, fingerprint, Lease);
        Assert.Equal(IdempotencyReservationStatus.Reserved, reservation.Status);
        Assert.NotNull(reservation.Token);
        return reservation.Token;
    }

    /// <summary>UTF-8 bytes of <paramref name="text"/>.</summary>
    protected static byte[] Payload(string text = """{"id":1}""") => Encoding.UTF8.GetBytes(text);

    private static string Text(IdempotencyRecordReservation reservation)
    {
        Assert.Equal(IdempotencyReservationStatus.Completed, reservation.Status);
        Assert.NotNull(reservation.Payload);
        return Encoding.UTF8.GetString(reservation.Payload.Value.ToArray());
    }

    [Fact]
    public async Task TryReserve_Should_Reserve_A_Free_Key()
    {
        IdempotencyRecordReservation reservation = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, reservation.Status);
        Assert.False(string.IsNullOrEmpty(reservation.Token));
        Assert.Null(reservation.Payload);
    }

    [Fact]
    public async Task TryReserve_Should_Report_InFlight_For_Same_Fingerprint()
    {
        await ReserveAsync(Key("k1"));

        IdempotencyRecordReservation second = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.InFlight, second.Status);
        Assert.Null(second.Token);
        Assert.Null(second.Payload);
    }

    [Fact]
    public async Task TryReserve_Should_Report_Mismatch_For_Different_Fingerprint_While_InFlight()
    {
        await ReserveAsync(Key("k1"));

        IdempotencyRecordReservation second = await Store.TryReserveAsync(Key("k1"), "other", Lease);

        Assert.Equal(IdempotencyReservationStatus.FingerprintMismatch, second.Status);
    }

    [Fact]
    public async Task TryReserve_Should_Return_Completed_Payload()
    {
        string token = await ReserveAsync(Key("k1"));
        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", Payload(), Retention));

        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal("""{"id":1}""", Text(replay));
        Assert.Null(replay.Token);
    }

    [Fact]
    public async Task TryReserve_Should_Report_Mismatch_For_Different_Fingerprint_After_Completion()
    {
        string token = await ReserveAsync(Key("k1"));
        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", Payload(), Retention));

        IdempotencyRecordReservation second = await Store.TryReserveAsync(Key("k1"), "other", Lease);

        Assert.Equal(IdempotencyReservationStatus.FingerprintMismatch, second.Status);
    }

    [Fact]
    public async Task Release_Should_Free_An_InFlight_Key()
    {
        string token = await ReserveAsync(Key("k1"));

        Assert.True(await Store.ReleaseAsync(Key("k1"), token));
        IdempotencyRecordReservation retry = await Store.TryReserveAsync(Key("k1"), "other", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, retry.Status);
    }

    [Fact]
    public async Task Release_Should_Keep_A_Completed_Key()
    {
        string token = await ReserveAsync(Key("k1"));
        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", Payload(), Retention));

        Assert.False(await Store.ReleaseAsync(Key("k1"), token));
        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Completed, replay.Status);
    }

    [Fact]
    public async Task Release_Of_Unknown_Key_Should_Do_Nothing()
    {
        Assert.False(await Store.ReleaseAsync(Key("missing"), "token"));
    }

    [Fact]
    public async Task Release_With_Another_Token_Should_Keep_The_Reservation()
    {
        await ReserveAsync(Key("k1"));

        Assert.False(await Store.ReleaseAsync(Key("k1"), "other-token"));
        IdempotencyRecordReservation retry = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.InFlight, retry.Status);
    }

    [Fact]
    public async Task InFlight_Reservation_Should_Expire_After_Lease()
    {
        await ReserveAsync(Key("k1"));

        await AdvanceAsync(Lease + ExpirySlack);
        IdempotencyRecordReservation retry = await Store.TryReserveAsync(Key("k1"), "other", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, retry.Status);
    }

    [Fact]
    public async Task Completed_Payload_Should_Live_For_Retention_Not_Lease()
    {
        string token = await ReserveAsync(Key("k1"));
        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", Payload(), Retention));

        await AdvanceAsync(Lease + ExpirySlack);
        IdempotencyRecordReservation beforeExpiry = await Store.TryReserveAsync(Key("k1"), "fp", Lease);
        await AdvanceAsync(Retention);
        IdempotencyRecordReservation afterExpiry = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Completed, beforeExpiry.Status);
        Assert.Equal(IdempotencyReservationStatus.Reserved, afterExpiry.Status);
    }

    [Fact]
    public async Task Keys_Should_Be_Independent()
    {
        await ReserveAsync(Key("k1"));

        IdempotencyRecordReservation other = await Store.TryReserveAsync(Key("k2"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, other.Status);
    }

    [Fact]
    public async Task Namespaces_Should_Be_Independent()
    {
        await ReserveAsync(Key("k1"));

        IdempotencyRecordReservation other =
            await Store.TryReserveAsync(new IdempotencyRecordKey(Namespace + "-other", IdempotencyScope.User("user-1"), "k1"), "other", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, other.Status);
    }

    [Fact]
    public async Task Scopes_Should_Be_Independent()
    {
        await ReserveAsync(Key("k1", IdempotencyScope.User("user-1")));

        IdempotencyRecordReservation other = await Store.TryReserveAsync(Key("k1", IdempotencyScope.User("user-2")), "other", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, other.Status);
    }

    [Fact]
    public async Task Global_Key_Should_Not_Collide_With_Scoped_Keys()
    {
        string token = await ReserveAsync(Key("k1", scope: null));
        Assert.True(await Store.CompleteAsync(Key("k1", scope: null), token, "fp", Payload("global"), Retention));

        IdempotencyRecordReservation marker = await Store.TryReserveAsync(Key("k1", IdempotencyScope.User("-")), "other", Lease);
        IdempotencyRecordReservation blank = await Store.TryReserveAsync(Key("k1", IdempotencyScope.User(" ")), "other", Lease);
        IdempotencyRecordReservation global = await Store.TryReserveAsync(Key("k1", scope: null), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, marker.Status);
        Assert.Equal(IdempotencyReservationStatus.Reserved, blank.Status);
        Assert.Equal("global", Text(global));
    }

    [Fact]
    public async Task Parts_Containing_Separators_Should_Not_Collide()
    {
        await ReserveAsync(new IdempotencyRecordKey(Namespace, IdempotencyScope.User("a|1:b"), "c"));

        IdempotencyRecordReservation shifted =
            await Store.TryReserveAsync(new IdempotencyRecordKey(Namespace, IdempotencyScope.User("a"), "b|1:c"), "other", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, shifted.Status);
    }

    [Fact]
    public async Task Empty_Payload_Should_Round_Trip()
    {
        string token = await ReserveAsync(Key("k1"));
        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", ReadOnlyMemory<byte>.Empty, Retention));

        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Completed, replay.Status);
        Assert.NotNull(replay.Payload);
        Assert.Equal(0, replay.Payload.Value.Length);
    }

    [Fact]
    public async Task Binary_Payload_Should_Round_Trip_Exactly()
    {
        byte[] payload = [.. Enumerable.Range(0, 256).Select(value => (byte)value), 0, 0xFF];
        string token = await ReserveAsync(Key("k1"));
        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", payload, Retention));

        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Completed, replay.Status);
        Assert.NotNull(replay.Payload);
        Assert.Equal(payload, replay.Payload.Value.ToArray());
    }

    [Fact]
    public async Task Stored_Payload_Should_Not_Change_With_The_Callers_Buffer()
    {
        byte[] payload = Payload("first");
        string token = await ReserveAsync(Key("k1"));
        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", payload, Retention));

        payload[0] = (byte)'X';
        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal("first", Text(replay));
    }

    [Fact]
    public async Task Reservations_Should_Get_Distinct_Tokens()
    {
        string first = await ReserveAsync(Key("k1"));
        string second = await ReserveAsync(Key("k2"));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task Expired_Token_Should_Be_Fenced_Out_By_The_Next_Reservation()
    {
        string late = await ReserveAsync(Key("k1"));
        await AdvanceAsync(Lease + ExpirySlack);
        string retry = await ReserveAsync(Key("k1"), "other");

        Assert.NotEqual(late, retry);
        Assert.False(await Store.ReleaseAsync(Key("k1"), late));
        Assert.False(await Store.CompleteAsync(Key("k1"), late, "fp", Payload("late"), Retention));
        IdempotencyRecordReservation stillHeld = await Store.TryReserveAsync(Key("k1"), "other", Lease);
        Assert.True(await Store.CompleteAsync(Key("k1"), retry, "other", Payload("retry"), Retention));
        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "other", Lease);

        Assert.Equal(IdempotencyReservationStatus.InFlight, stillHeld.Status);
        Assert.Equal("retry", Text(replay));
    }

    [Fact]
    public async Task Expired_Reservation_That_Nobody_Took_Should_Still_Complete()
    {
        string token = await ReserveAsync(Key("k1"));
        await AdvanceAsync(Lease + ExpirySlack);

        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", Payload(), Retention));
        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Completed, replay.Status);
    }

    [Fact]
    public async Task Complete_Twice_Should_Keep_The_First_Payload()
    {
        string token = await ReserveAsync(Key("k1"));

        Assert.True(await Store.CompleteAsync(Key("k1"), token, "fp", Payload("first"), Retention));
        Assert.False(await Store.CompleteAsync(Key("k1"), token, "fp", Payload("second"), Retention));
        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        Assert.Equal("first", Text(replay));
    }

    [Fact]
    public async Task Parallel_Reservations_Of_A_Free_Key_Should_Reserve_Once()
    {
        IdempotencyRecordReservation[] results = await ReserveInParallelAsync(Key("k1"));

        AssertSingleReservation(results);
    }

    [Fact]
    public async Task Parallel_Reservations_Of_An_Expired_Key_Should_Reserve_Once()
    {
        await ReserveAsync(Key("k1"));
        await AdvanceAsync(Lease + ExpirySlack);

        IdempotencyRecordReservation[] results = await ReserveInParallelAsync(Key("k1"));

        AssertSingleReservation(results);
    }

    [Fact]
    public async Task Methods_Should_Validate_Arguments()
    {
        IdempotencyRecordKey key = Key("k1");

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Store.TryReserveAsync(null!, "fp", Lease));
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await Store.TryReserveAsync(key, "", Lease));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Store.TryReserveAsync(key, "fp", TimeSpan.Zero));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Store.CompleteAsync(null!, "t", "fp", Payload(), Retention));
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await Store.CompleteAsync(key, "", "fp", Payload(), Retention));
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await Store.CompleteAsync(key, "t", "", Payload(), Retention));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Store.CompleteAsync(key, "t", "fp", Payload(), TimeSpan.Zero));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await Store.ReleaseAsync(null!, "t"));
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await Store.ReleaseAsync(key, ""));
    }

    private async Task<IdempotencyRecordReservation[]> ReserveInParallelAsync(IdempotencyRecordKey key)
    {
        IIdempotencyRecordStore store = Store;
        TimeSpan lease = Lease;
        TaskCompletionSource<bool> start = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<IdempotencyRecordReservation>[] reservations = [.. Enumerable.Range(0, ParallelReservations).Select(_ => Task.Run(async () =>
        {
            await start.Task;
            return await store.TryReserveAsync(key, "fp", lease);
        }))];

        start.SetResult(true);
        return await Task.WhenAll(reservations);
    }

    private void AssertSingleReservation(IdempotencyRecordReservation[] results)
    {
        int reserved = results.Count(result => result.Status == IdempotencyReservationStatus.Reserved);
        int inFlight = results.Count(result => result.Status == IdempotencyReservationStatus.InFlight);

        Assert.Equal(ParallelReservations, reserved + inFlight);
        if (IsReservationAtomic)
            Assert.Equal(1, reserved);
        else
            Assert.InRange(reserved, 1, ParallelReservations);
    }
}
