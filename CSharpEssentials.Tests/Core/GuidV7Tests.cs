using System.Collections.Concurrent;
using System.Security.Cryptography;
using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public sealed class GuidV7Tests
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
    public void Generate_ShouldSetVersion7AndVariant10(string generator)
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
    public void NewGuid_ShouldEncodeTheCurrentUnixMilliseconds(string generator)
    {
        long before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Guid guid = Generate(generator);
        long after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        UnixMs(BigEndian(guid)).Should().BeInRange(before, after);
    }

    [Theory]
    [InlineData("NewGuid")]
    [InlineData("portable")]
    public void NewGuid_ShouldFillEveryRandomBit(string generator)
    {
        byte[] randomMask = [0, 0, 0, 0, 0, 0, 0x0F, 0xFF, 0x3F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];
        byte[] seenOne = new byte[16];
        byte[] seenZero = new byte[16];
        for (int i = 0; i < 1_000; i++)
        {
            byte[] b = BigEndian(Generate(generator));
            for (int j = 0; j < 16; j++)
            {
                seenOne[j] |= b[j];
                seenZero[j] |= (byte)~b[j];
            }
        }

        for (int j = 0; j < 16; j++)
        {
            (seenOne[j] & randomMask[j]).Should().Be(randomMask[j]);
            (seenZero[j] & randomMask[j]).Should().Be(randomMask[j]);
        }
    }

    [Fact]
    public void Pack_ShouldEncodeTheClockAndSeedTheCounterFromSubMillisecondTime()
    {
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        long packed = GuidV7.Pack(0, Ticks(unixMs) + (_ticksPerMs / 2));

        (packed >> 12).Should().Be(unixMs);
        (packed & 0xFFF).Should().Be(2048);
    }

    [Fact]
    public void Compose_ShouldMatchTheLayoutOfGuidCreateVersion7()
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
    public void Compose_ShouldOverwriteVersionAndVariantBits()
    {
        byte[] ones = Enumerable.Repeat((byte)0xFF, 16).ToArray();
        byte[] zeros = new byte[16];

        Guid fromOnes = GuidV7.Compose(0x0123_4567_89AB, ones);
        Guid fromZeros = GuidV7.Compose(0x0123_4567_89AB, zeros);

        fromOnes.Should().Be(Guid.Parse("01234567-89ab-7fff-bfff-ffffffffffff"));
        fromZeros.Should().Be(Guid.Parse("01234567-89ab-7000-8000-000000000000"));
    }

    [Fact]
    public void NewMonotonicGuid_WhenCalledOnOneThread_ShouldStrictlyIncrease()
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
    public void NewMonotonicGuid_WhenCalledFromManyThreads_ShouldReturnUniqueIds()
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
    public void Pack_WhenCounterOverflows_ShouldCarryIntoTimestamp()
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
        nextGuid.CompareTo(lastGuid).Should().BePositive();
        nextGuid.Version.Should().Be(7);
    }

    [Fact]
    public void Pack_WhenClockGoesBackwardsOrStalls_ShouldKeepIncreasing()
    {
        long now = DateTime.UtcNow.Ticks;
        long[] clock = [now, now - TimeSpan.TicksPerSecond, now - TimeSpan.TicksPerDay, now, now, now - 1, now + _ticksPerMs];
        long last = 0;
        byte[] previous = new byte[16];
        Guid previousGuid = Guid.Empty;
        Span<byte> random = stackalloc byte[16];

        foreach (long ticks in clock)
        {
            long packed = GuidV7.Pack(last, ticks);
            RandomNumberGenerator.Fill(random);
            Guid currentGuid = GuidV7.ComposePacked(packed, random.ToArray());
            byte[] current = BigEndian(currentGuid);

            packed.Should().BeGreaterThan(last);
            current.AsSpan().SequenceCompareTo(previous).Should().BePositive();
            currentGuid.CompareTo(previousGuid).Should().BePositive();
            Packed(current).Should().Be(packed);

            last = packed;
            previous = current;
            previousGuid = currentGuid;
        }
    }

    [Theory]
    [MemberData(nameof(Generators))]
    public void Generate_ShouldNotAllocate(string generator)
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
