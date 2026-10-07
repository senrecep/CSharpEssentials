using CSharpEssentials.AspNetCore;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class InMemoryIdempotencyStoreTests : IdempotencyStoreContractTests
{
    private readonly InMemoryIdempotencyStore _store;

    public InMemoryIdempotencyStoreTests() => _store = new InMemoryIdempotencyStore(Time);

    protected override IIdempotencyStore Store => _store;

    [Fact]
    public async Task TryReserve_Should_Reserve_Only_Once_Under_Concurrency()
    {
        IdempotencyReservation[] results = await Task.WhenAll(Enumerable.Range(0, 64)
            .Select(_ => Task.Run(async () => await _store.TryReserveAsync("k1", "fp", InFlight))));

        results.Count(r => r.Status == IdempotencyReservationStatus.Reserved).Should().Be(1);
        results.Count(r => r.Status == IdempotencyReservationStatus.InFlight).Should().Be(63);
    }

    [Fact]
    public async Task TryReserve_Should_Reserve_Expired_Key_Only_Once_Under_Concurrency()
    {
        await _store.TryReserveAsync("k1", "fp", InFlight);
        Time.Advance(InFlight);

        IdempotencyReservation[] results = await Task.WhenAll(Enumerable.Range(0, 64)
            .Select(_ => Task.Run(async () => await _store.TryReserveAsync("k1", "fp", InFlight))));

        results.Count(r => r.Status == IdempotencyReservationStatus.Reserved).Should().Be(1);
    }

    [Fact]
    public async Task Expired_Entries_Should_Not_Block_New_Reservations_After_Sweep()
    {
        await ReserveAsync("k1");
        string token = await ReserveAsync("k2");
        await _store.CompleteAsync("k2", token, "fp", Response(), Retention);

        Time.Advance(TimeSpan.FromMinutes(2));
        await _store.TryReserveAsync("k3", "fp", InFlight);
        IdempotencyReservation k1 = await _store.TryReserveAsync("k1", "other", InFlight);
        IdempotencyReservation k2 = await _store.TryReserveAsync("k2", "fp", InFlight);

        k1.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        k2.Status.Should().Be(IdempotencyReservationStatus.Completed);
    }

    [Fact]
    public async Task Methods_Should_Validate_Arguments()
    {
        Func<Task> emptyKey = async () => await _store.TryReserveAsync("", "fp", InFlight);
        Func<Task> emptyFingerprint = async () => await _store.TryReserveAsync("k", "", InFlight);
        Func<Task> zeroTimeout = async () => await _store.TryReserveAsync("k", "fp", TimeSpan.Zero);
        Func<Task> emptyToken = async () => await _store.CompleteAsync("k", "", "fp", Response(), Retention);
        Func<Task> nullResponse = async () => await _store.CompleteAsync("k", "t", "fp", null!, Retention);
        Func<Task> zeroRetention = async () => await _store.CompleteAsync("k", "t", "fp", Response(), TimeSpan.Zero);
        Func<Task> releaseEmptyToken = async () => await _store.ReleaseAsync("k", "");

        await emptyKey.Should().ThrowAsync<ArgumentException>();
        await emptyFingerprint.Should().ThrowAsync<ArgumentException>();
        await zeroTimeout.Should().ThrowAsync<ArgumentOutOfRangeException>();
        await emptyToken.Should().ThrowAsync<ArgumentException>();
        await releaseEmptyToken.Should().ThrowAsync<ArgumentException>();
        await nullResponse.Should().ThrowAsync<ArgumentNullException>();
        await zeroRetention.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
