using System.Collections.Concurrent;
using System.Security.Cryptography;
using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public class GuidV7Tests
{
    private const int _rfcSampleSize = 30_000;
    private const long _ticksPerMs = TimeSpan.TicksPerMillisecond;

    public static TheoryData<string> Generators => ["NewGuid", "portable", "monotonic"];

    private static Guid Generate(string generator) => generator switch
    {
        "NewGuid" => Guider.NewGuid(),
        "portable" => GuidV7.NewGuid(),
        _ => Guider.NewMonotonicGuid()
    };

    private static byte[] BigEndian(Guid guid) => guid.ToByteArray(bigEndian: true);

    private static long UnixMs(byte[] b) =>
        ((long)b[0] << 40) | ((long)b[1] << 32) | ((long)b[2] << 24) | ((long)b[3] << 16) | ((long)b[4] << 8) | b[5];

    private static long Packed(byte[] b) => (UnixMs(b) << 12) | ((long)(b[6] & 0x0F) << 8) | b[7];

    private static long Ticks(long unixMs) => DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcTicks;

    [Theory]
    [MemberData(nameof(Generators))]
    public void Generated_ids_carry_version_7_and_variant_10(string generator)
    {
        for (int i = 0; i < _rfcSampleSize; i++)
        {
            Guid guid = Generate(generator);
            byte[] b = BigEndian(guid);

            (b[6] >> 4).Should().Be(7);
            (b[8] >> 6).Should().Be(0b10);
            guid.Version.Should().Be(7);
            (guid.Variant & 0b1100).Should().Be(0b1000);
        }
    }

    [Theory]
    [InlineData("NewGuid")]
    [InlineData("portable")]
    public void Stateless_timestamp_matches_the_clock(string generator)
    {
        long before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Guid guid = Generate(generator);
        long after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        UnixMs(BigEndian(guid)).Should().BeInRange(before, after);
    }

    [Fact]
    public void Pack_timestamp_matches_the_given_clock_and_seeds_the_counter_from_sub_millisecond_time()
    {
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        long packed = GuidV7.Pack(0, Ticks(unixMs) + (_ticksPerMs / 2));

        (packed >> 12).Should().Be(unixMs);
        (packed & 0xFFF).Should().Be(2048);
    }

    [Fact]
    public void Compose_matches_the_bit_layout_of_Guid_CreateVersion7()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow;
        for (int i = 0; i < 1_000; i++)
        {
            DateTimeOffset timestamp = start.AddMilliseconds(i * 7919);
            Guid expected = Guid.CreateVersion7(timestamp);
            byte[] bytes = BigEndian(expected);

            Guid actual = GuidV7.Compose(timestamp.ToUnixTimeMilliseconds(), bytes);

            actual.Should().Be(expected);
        }
    }

    [Fact]
    public void Compose_writes_version_and_variant_over_any_random_bits()
    {
        byte[] ones = Enumerable.Repeat((byte)0xFF, 16).ToArray();
        byte[] zeros = new byte[16];

        Guid fromOnes = GuidV7.Compose(0x0123_4567_89AB, ones);
        Guid fromZeros = GuidV7.Compose(0x0123_4567_89AB, zeros);

        fromOnes.Should().Be(Guid.Parse("01234567-89ab-7fff-bfff-ffffffffffff"));
        fromZeros.Should().Be(Guid.Parse("01234567-89ab-7000-8000-000000000000"));
    }

    [Fact]
    public void NewMonotonicGuid_is_strictly_increasing_on_one_thread()
    {
        Guid previous = Guider.NewMonotonicGuid();
        byte[] previousBytes = BigEndian(previous);
        for (int i = 0; i < 100_000; i++)
        {
            Guid current = Guider.NewMonotonicGuid();
            byte[] currentBytes = BigEndian(current);

            currentBytes.AsSpan().SequenceCompareTo(previousBytes).Should().BePositive();
            current.CompareTo(previous).Should().BePositive();
            string.CompareOrdinal(current.ToString(), previous.ToString()).Should().BePositive();

            previous = current;
            previousBytes = currentBytes;
        }
    }

    [Fact]
    public void NewMonotonicGuid_is_unique_across_threads()
    {
        const int threads = 64, perThread = 5_000;
        ConcurrentBag<byte[]> all = [];
        using Barrier barrier = new(threads);

        Thread[] workers = Enumerable.Range(0, threads).Select(_ => new Thread(() =>
        {
            byte[][] local = new byte[perThread][];
            barrier.SignalAndWait();
            for (int i = 0; i < perThread; i++)
                local[i] = BigEndian(Guider.NewMonotonicGuid());
            foreach (byte[] id in local)
                all.Add(id);
        })).ToArray();
        foreach (Thread worker in workers)
            worker.Start();
        foreach (Thread worker in workers)
            worker.Join();

        all.Should().HaveCount(threads * perThread);
        all.Select(b => new Guid(b, bigEndian: true)).Distinct().Should().HaveCount(threads * perThread);
        all.Select(Packed).Distinct().Should().HaveCount(threads * perThread);
    }

    [Fact]
    public void Pack_carries_counter_overflow_into_the_timestamp()
    {
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long last = (unixMs << 12) | 0xFFF;
        Guid lastGuid = GuidV7.ComposePacked(last, new byte[16]);

        long next = GuidV7.Pack(last, Ticks(unixMs));
        Guid nextGuid = GuidV7.ComposePacked(next, new byte[16]);

        next.Should().Be((unixMs + 1) << 12);
        UnixMs(BigEndian(nextGuid)).Should().Be(unixMs + 1);
        Packed(BigEndian(nextGuid)).Should().Be(next);
        BigEndian(nextGuid).AsSpan().SequenceCompareTo(BigEndian(lastGuid)).Should().BePositive();
        nextGuid.Version.Should().Be(7);
    }

    [Fact]
    public void Pack_keeps_ids_increasing_when_the_clock_goes_backwards_or_stalls()
    {
        long now = DateTime.UtcNow.Ticks;
        long[] clock = [now, now - TimeSpan.TicksPerSecond, now - TimeSpan.TicksPerDay, now, now, now - 1, now + _ticksPerMs];
        long last = 0;
        byte[] previous = new byte[16];
        Span<byte> random = stackalloc byte[16];

        foreach (long ticks in clock)
        {
            long packed = GuidV7.Pack(last, ticks);
            RandomNumberGenerator.Fill(random);
            byte[] current = BigEndian(GuidV7.ComposePacked(packed, random.ToArray()));

            packed.Should().BeGreaterThan(last);
            current.AsSpan().SequenceCompareTo(previous).Should().BePositive();
            Packed(current).Should().Be(packed);

            last = packed;
            previous = current;
        }
    }

    [Theory]
    [MemberData(nameof(Generators))]
    public void Generators_do_not_allocate(string generator)
    {
        for (int i = 0; i < 1_000; i++)
            Generate(generator);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10_000; i++)
            Generate(generator);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        allocated.Should().Be(0);
    }
}
