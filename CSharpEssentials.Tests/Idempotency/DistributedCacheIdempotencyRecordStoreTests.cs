using System.Text;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Idempotency.Testing;
using CSharpEssentials.Tests.AspNetCore.Idempotency;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class DistributedCacheIdempotencyRecordStoreTests : IdempotencyRecordStoreContractTests
{
    private FakeDistributedCache? _cache;

    protected override bool IsReservationAtomic => false;

    private FakeDistributedCache Cache
    {
        get
        {
            _ = Store;
            return _cache!;
        }
    }

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider)
    {
        _cache = new FakeDistributedCache(timeProvider);
        return new DistributedCacheIdempotencyRecordStore(_cache);
    }

    [Fact]
    public async Task Entries_Should_Use_The_Default_Storage_Key()
    {
        await ReserveAsync(Key("k1"));

        Cache.Keys.Should().Equal(Key("k1").ToStorageKey());
        Cache.Keys.Single().Should().StartWith(IdempotencyRecordKey.DefaultPrefix);
    }

    [Fact]
    public async Task Entries_Should_Use_A_Custom_Prefix()
    {
        DistributedCacheIdempotencyRecordStore store = new(Cache, "app:");

        await store.TryReserveAsync(Key("k1"), "fp", Lease);

        store.KeyPrefix.Should().Be("app:");
        Cache.Keys.Should().Equal(Key("k1").ToStorageKey("app:"));
    }

    [Fact]
    public async Task Unreadable_Entry_Should_Be_Treated_As_Absent()
    {
        await Cache.SetAsync(Key("k1").ToStorageKey(), [0xFF, 0x01], new DistributedCacheEntryOptions());

        IdempotencyRecordReservation reservation = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Truncated_Entry_Should_Be_Treated_As_Absent()
    {
        byte[] stored = await CompleteAndReadAsync("k1");
        await Cache.SetAsync(Key("k1").ToStorageKey(), stored[..^3], new DistributedCacheEntryOptions());

        IdempotencyRecordReservation reservation = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Entry_With_Trailing_Bytes_Should_Be_Treated_As_Absent()
    {
        byte[] stored = await CompleteAndReadAsync("k1");
        await Cache.SetAsync(Key("k1").ToStorageKey(), [.. stored, 0x00], new DistributedCacheEntryOptions());

        IdempotencyRecordReservation reservation = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Corrupt_Length_Should_Be_Treated_As_Absent()
    {
        byte[] payload = Payload();
        byte[] stored = await CompleteAndReadAsync("k1");
        BitConverter.GetBytes(int.MaxValue).CopyTo(stored, stored.Length - payload.Length - sizeof(int));
        await Cache.SetAsync(Key("k1").ToStorageKey(), stored, new DistributedCacheEntryOptions());

        IdempotencyRecordReservation reservation = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Version_2_Entry_Should_Not_Be_Read()
    {
        await Cache.SetAsync(Key("k1").ToStorageKey(), Version2Entry("token", "fp"), new DistributedCacheEntryOptions());

        IdempotencyRecordReservation reservation = await Store.TryReserveAsync(Key("k1"), "fp", Lease);
        bool released = await Store.ReleaseAsync(Key("k1"), "token");

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        released.Should().BeFalse();
    }

    [Fact]
    public async Task Complete_Should_Overwrite_An_Unreadable_Entry()
    {
        string token = await ReserveAsync(Key("k1"));
        await Cache.SetAsync(Key("k1").ToStorageKey(), [0xFF], new DistributedCacheEntryOptions());

        bool completed = await Store.CompleteAsync(Key("k1"), token, "fp", Payload(), Retention);
        IdempotencyRecordReservation replay = await Store.TryReserveAsync(Key("k1"), "fp", Lease);

        completed.Should().BeTrue();
        replay.Status.Should().Be(IdempotencyReservationStatus.Completed);
    }

    [Fact]
    public void Constructor_Should_Reject_Null_Arguments()
    {
        Action nullCache = () => _ = new DistributedCacheIdempotencyRecordStore(null!);
        Action nullPrefix = () => _ = new DistributedCacheIdempotencyRecordStore(Cache, null!);

        nullCache.Should().Throw<ArgumentNullException>();
        nullPrefix.Should().Throw<ArgumentNullException>();
    }

    private async Task<byte[]> CompleteAndReadAsync(string key)
    {
        string token = await ReserveAsync(Key(key));
        await Store.CompleteAsync(Key(key), token, "fp", Payload(), Retention);
        return (await Cache.GetAsync(Key(key).ToStorageKey()))!;
    }

    /// <summary>A completed entry in the format written before version 3: the HTTP response inside the record.</summary>
    private static byte[] Version2Entry(string token, string fingerprint)
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write((byte)2);
            writer.Write(token);
            writer.Write(fingerprint);
            writer.Write(true);
            writer.Write(201);
            writer.Write(0);
            writer.Write(2);
            writer.Write("{}"u8);
        }

        return stream.ToArray();
    }
}
