using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class DistributedCacheIdempotencyStoreTests : IdempotencyStoreContractTests
{
    private readonly FakeDistributedCache _cache;
    private readonly DistributedCacheIdempotencyStore _store;

    public DistributedCacheIdempotencyStoreTests()
    {
        _cache = new FakeDistributedCache(Time);
        _store = new DistributedCacheIdempotencyStore(_cache);
    }

    protected override IIdempotencyStore Store => _store;

    [Fact]
    public async Task Entries_Should_Use_The_Key_Prefix()
    {
        await _store.TryReserveAsync("k1", "fp", InFlight);

        _cache.Keys.Should().Equal("idempotency:k1");
    }

    [Fact]
    public async Task Unreadable_Entry_Should_Be_Treated_As_Absent()
    {
        await _cache.SetAsync("idempotency:k1", [0xFF, 0x01], new DistributedCacheEntryOptions());

        IdempotencyReservation reservation = await _store.TryReserveAsync("k1", "fp", InFlight);

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Truncated_Entry_Should_Be_Treated_As_Absent()
    {
        string token = await ReserveAsync("k1");
        await _store.CompleteAsync("k1", token, "fp", Response(), Retention);
        byte[] stored = (await _cache.GetAsync("idempotency:k1"))!;
        await _cache.SetAsync("idempotency:k1", stored[..^3], new DistributedCacheEntryOptions());

        IdempotencyReservation reservation = await _store.TryReserveAsync("k1", "fp", InFlight);

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Corrupt_Length_Should_Be_Treated_As_Absent()
    {
        string token = await ReserveAsync("k1");
        await _store.CompleteAsync("k1", token, "fp", Response(), Retention);
        byte[] stored = (await _cache.GetAsync("idempotency:k1"))!;
        // The body length is the int before the last 8 body bytes ("{"id":1}").
        BitConverter.GetBytes(int.MaxValue).CopyTo(stored, stored.Length - 8 - sizeof(int));
        await _cache.SetAsync("idempotency:k1", stored, new DistributedCacheEntryOptions());

        IdempotencyReservation reservation = await _store.TryReserveAsync("k1", "fp", InFlight);

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }
}
