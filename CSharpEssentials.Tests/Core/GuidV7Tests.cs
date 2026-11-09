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

    private static long Packed(byte[] b) => (UnixMs(b) << 18) | ((long)(b[6] & 0x0F) << 14) | ((long)b[7] << 6) | (b[8] & 0x3FL);

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

    private static bool TookFreshNewGuidBits(long utcTicks, bool usePool)
    {
        byte[] bytes = new byte[16];
        GuidV7.FillRandom(bytes, utcTicks, usePool);
        return bytes.AsSpan(0, 6).IndexOfAnyExcept((byte)0) >= 0;
    }

    private const int _poolSize = 1024;

    private static byte[] Salt() => GuidV7.CurrentPool()!.AsSpan(_poolSize, 8).ToArray();

    private static byte[] Xor(byte[] a, byte[] b) => a.Zip(b, (x, y) => (byte)(x ^ y)).ToArray();

    private static (byte[] Taken, byte[] Leftover) TakeAndPeekLeftover(long utcTicks)
    {
        byte[] leftover = GuidV7.CurrentPool()!.AsSpan(GuidV7.CurrentPoolOffset(), 8).ToArray();
        byte[] bytes = new byte[16];
        GuidV7.FillRandom(bytes, utcTicks, usePool: true);
        return (bytes.AsSpan(6, 8).ToArray(), leftover);
    }

    private static void DrainPoolUntil(long utcTicks, Func<int, bool> stop)
    {
        while (!stop(GuidV7.CurrentPoolOffset()))
            GuidV7.FillRandom(new byte[16], utcTicks, usePool: true);
    }

    private static void DrainPoolToASafeOffset(long utcTicks) => DrainPoolUntil(utcTicks, offset => offset <= _poolSize - 20);

    [Fact]
    public void FillRandom_WhenCalledAgainWithinOneMillisecond_ShouldTakeTheNextPoolBytesUnderTheSameSalt()
    {
        long now = DateTime.UtcNow.Ticks;
        GuidV7.FillRandom(new byte[16], now, usePool: true);
        GuidV7.FillRandom(new byte[16], now, usePool: true);
        DrainPoolToASafeOffset(now);
        byte[] salt = Salt();

        (byte[] taken, byte[] leftover) = TakeAndPeekLeftover(now + _ticksPerMs);

        Salt().Should().Equal(salt);
        taken.Should().Equal(Xor(leftover, salt));
        TookFreshNewGuidBits(now + _ticksPerMs, usePool: true).Should().BeFalse();
    }

    [Theory]
    [InlineData(_ticksPerMs + 1)]
    [InlineData(-1)]
    public void FillRandom_AfterAPauseOrAClockStepBack_ShouldTakeAFreshGuidAndSaltTheLeftoverPoolWithIt(long step)
    {
        long now = DateTime.UtcNow.Ticks;
        GuidV7.FillRandom(new byte[16], now, usePool: true);
        GuidV7.FillRandom(new byte[16], now, usePool: true);
        DrainPoolToASafeOffset(now);
        byte[] oldSalt = Salt();

        TookFreshNewGuidBits(now + step, usePool: true).Should().BeTrue();
        (byte[] taken, byte[] leftover) = TakeAndPeekLeftover(now + step);

        Salt().Should().NotEqual(oldSalt);
        taken.Should().Equal(Xor(leftover, Salt()));
    }

    [Fact]
    public void FillRandom_WhenThePoolRunsOut_ShouldRefillItAndTakeANewGuidAsTheSalt()
    {
        long now = DateTime.UtcNow.Ticks;
        GuidV7.FillRandom(new byte[16], now, usePool: true);
        GuidV7.FillRandom(new byte[16], now, usePool: true);
        for (int refill = 0; refill < 3; refill++)
        {
            DrainPoolUntil(now, offset => offset == _poolSize - 4);
            byte[] oldSalt = Salt();

            byte[] bytes = new byte[16];
            GuidV7.FillRandom(bytes, now, usePool: true);

            GuidV7.CurrentPoolOffset().Should().Be(10);
            Salt().Should().NotEqual(oldSalt);
            new Guid(GuidV7.CurrentPool()!.AsSpan(_poolSize, 16)).Version.Should().Be(4);
            bytes.AsSpan(6, 8).ToArray().Should().Equal(Xor(GuidV7.CurrentPool()!.AsSpan(0, 8).ToArray(), Salt()));
        }
    }

    [Fact]
    public void FillRandom_WhenThePoolIsDisabled_ShouldAlwaysTakeFreshBitsFromGuidNewGuid()
    {
        long now = DateTime.UtcNow.Ticks;
        for (int i = 0; i < 100; i++)
            TookFreshNewGuidBits(now + i, usePool: false).Should().BeTrue();
    }

    [Fact]
    public void Pack_ShouldEncodeTheClockAndSubMillisecondFractionWithACounterAtZero()
    {
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        long packed = GuidV7.Pack(0, Ticks(unixMs) + (_ticksPerMs / 2));

        (packed >> 18).Should().Be(unixMs);
        ((packed >> 6) & 0xFFF).Should().Be(2048);
        (packed & 0x3F).Should().Be(0);
    }

    [Fact]
    public void Pack_WhenTheFractionRepeats_ShouldIncrementTheCounterInRandB()
    {
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long ticks = Ticks(unixMs) + (_ticksPerMs / 2);
        long first = GuidV7.Pack(0, ticks);

        long second = GuidV7.Pack(first, ticks);
        byte[] b = BigEndian(GuidV7.ComposePacked(second, new byte[16]));

        second.Should().Be(first + 1);
        (b[8] & 0x3F).Should().Be(1);
        (b[8] >> 6).Should().Be(0b10);
        Packed(b).Should().Be(second);
    }

    [Fact]
    public void Pack_WhenTheCounterRollsOver_ShouldCarryIntoTheFraction()
    {
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long last = (unixMs << 18) | (5 << 6) | 0x3F;

        long next = GuidV7.Pack(last, Ticks(unixMs));

        next.Should().Be((unixMs << 18) | (6 << 6));
    }

    [Fact]
    public void NewMonotonicGuid_WhenCalledInATightLoop_ShouldKeepTheTimestampWithinOneMillisecondOfTheClock()
    {
        Span<byte> b = stackalloc byte[16];
        Guid last = Guid.Empty;
        for (int i = 0; i < 1_000_000; i++)
            last = Guider.NewMonotonicGuid();
        long clockMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        last.TryWriteBytes(b, bigEndian: true, out _).Should().BeTrue();
        UnixMs(b.ToArray()).Should().BeLessThanOrEqualTo(clockMs + 1);
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
    public void NewMonotonicGuid_WhenCalledFromManyThreads_ShouldReturnUniqueIdsThatIncreaseOnEachThread()
    {
        const int threads = 64, perThread = 5_000;
        ConcurrentBag<byte[]> all = [];
        int outOfOrder = 0;
        using Barrier barrier = new(threads);

        Thread[] workers = Enumerable.Range(0, threads).Select(_ => new Thread(() =>
        {
            byte[][] local = new byte[perThread][];
            barrier.SignalAndWait();
            for (int i = 0; i < perThread; i++)
                local[i] = BigEndian(Guider.NewMonotonicGuid());
            for (int i = 1; i < perThread; i++)
                if (local[i].AsSpan().SequenceCompareTo(local[i - 1]) <= 0)
                    Interlocked.Increment(ref outOfOrder);
            foreach (byte[] id in local)
                all.Add(id);
        })).ToArray();
        foreach (Thread worker in workers)
            worker.Start();
        foreach (Thread worker in workers)
            worker.Join();

        outOfOrder.Should().Be(0);
        all.Should().HaveCount(threads * perThread);
        all.Select(b => new Guid(b, bigEndian: true)).Distinct().Should().HaveCount(threads * perThread);
        all.Select(Packed).Distinct().Should().HaveCount(threads * perThread);
    }

    [Fact]
    public void Pack_WhenCounterOverflows_ShouldCarryIntoTimestamp()
    {
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long last = (unixMs << 18) | 0x3FFFF;
        Guid lastGuid = GuidV7.ComposePacked(last, new byte[16]);

        long next = GuidV7.Pack(last, Ticks(unixMs));
        Guid nextGuid = GuidV7.ComposePacked(next, new byte[16]);

        next.Should().Be((unixMs + 1) << 18);
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

        allocated.Should().BeLessThan(10_000, "one allocating call per Generate would add at least 10,000 bytes");
    }
}
