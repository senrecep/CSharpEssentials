using System.Text;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Tests.AspNetCore.Idempotency;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class DistributedCacheRecordEntryFormatTests
{
    private static readonly IdempotencyRecordKey RecordKey = new("ns", IdempotencyScope.User("user-1"), "k1");

    private readonly FakeDistributedCache _cache = new(new FakeTimeProvider());

    private DistributedCacheIdempotencyRecordStore Store => new(_cache);

    [Fact]
    public async Task Reserved_Entry_Should_Keep_The_Version_3_Layout()
    {
        IdempotencyRecordReservation reservation = await Store.TryReserveAsync(RecordKey, "fp", TimeSpan.FromMinutes(1));

        byte[] entry = (await _cache.GetAsync(RecordKey.ToStorageKey()))!;
        entry.Should().Equal(Entry(reservation.Token!, "fp", null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(300_000)]
    public async Task Completed_Entry_Should_Keep_The_Version_3_Layout_And_Return_The_Payload(int size)
    {
        byte[] payload = new byte[size];
        for (int index = 0; index < size; index++)
            payload[index] = (byte)(index * 31);
        string fingerprint = new('f', 200);
        IdempotencyRecordReservation reserved = await Store.TryReserveAsync(RecordKey, fingerprint, TimeSpan.FromMinutes(1));

        bool completed = await Store.CompleteAsync(RecordKey, reserved.Token!, fingerprint, payload, TimeSpan.FromHours(1));
        IdempotencyRecordReservation replay = await Store.TryReserveAsync(RecordKey, fingerprint, TimeSpan.FromMinutes(1));

        completed.Should().BeTrue();
        byte[] entry = (await _cache.GetAsync(RecordKey.ToStorageKey()))!;
        entry.Should().Equal(Entry(reserved.Token!, fingerprint, payload));
        replay.Status.Should().Be(IdempotencyReservationStatus.Completed);
        replay.Payload!.Value.ToArray().Should().Equal(payload);
    }

    private static byte[] Entry(string token, string fingerprint, byte[]? payload)
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write((byte)3);
            writer.Write(token);
            writer.Write(fingerprint);
            writer.Write(payload is not null);
            if (payload is not null)
            {
                writer.Write(payload.Length);
                writer.Write(payload);
            }
        }

        return stream.ToArray();
    }
}
