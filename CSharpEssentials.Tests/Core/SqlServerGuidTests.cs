using System.Data.SqlTypes;
using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public sealed class SqlServerGuidTests
{
    private static long UnixMs(byte[] b) =>
        ((long)b[10] << 40) | ((long)b[11] << 32) | ((long)b[12] << 24) | ((long)b[13] << 16) | ((long)b[14] << 8) | b[15];

    [Fact]
    public void NewSqlServerGuid_ShouldSetVersion8Variant10AndTimestampInLastSixBytes()
    {
        long before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Guid guid = Guider.NewSqlServerGuid();
        long after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        byte[] b = guid.ToByteArray(bigEndian: true);

        guid.Version.Should().Be(8);
        (b[8] >> 6).Should().Be(2);
        UnixMs(b).Should().BeInRange(before, after + 1);
    }

    [Fact]
    public void ComposeSqlServer_ShouldPlaceTimestampSequenceAndRandomBits()
    {
        long packed = (0x0123_4567_89ABL << 18) | 0x2_ABCD;
        byte[] random = Enumerable.Range(0, 16).Select(i => (byte)(0xF0 | i)).ToArray();

        Guid guid = GuidV7.ComposeSqlServer(packed, random);

        guid.ToString().Should().Be("fafbfcfd-feff-86d7-aabc-0123456789ab");
    }

    [Fact]
    public void ComposeSqlServer_ShouldFollowPackedOrderAcrossCounterFractionAndMillisecondCarries()
    {
        long start = (1_700_000_000_000L << 18) - 70;
        byte[] random = new byte[16];
        var rng = new Random(7);
        rng.NextBytes(random);
        SqlGuid previous = new(GuidV7.ComposeSqlServer(start, (byte[])random.Clone()));

        for (long packed = start + 1; packed < start + 600_000; packed += 1 + (packed % 3))
        {
            rng.NextBytes(random);
            SqlGuid current = new(GuidV7.ComposeSqlServer(packed, random));
            current.CompareTo(previous).Should().BePositive();
            previous = current;
        }
    }

    [Fact]
    public void NewSqlServerGuid_ShouldStrictlyIncreaseInSqlServerOrder()
    {
        SqlGuid previous = new(Guider.NewSqlServerGuid());
        for (int i = 0; i < 100_000; i++)
        {
            SqlGuid current = new(Guider.NewSqlServerGuid());
            current.CompareTo(previous).Should().BePositive();
            previous = current;
        }
    }

    [Fact]
    public void NewSqlServerGuid_ShouldVaryEveryRandomBit()
    {
        byte set = 0, clear = 0;
        byte[] setBytes = new byte[8], clearBytes = new byte[8];
        for (int i = 0; i < 1_000; i++)
        {
            byte[] b = Guider.NewSqlServerGuid().ToByteArray(bigEndian: true);
            for (int j = 0; j < 6; j++)
            {
                setBytes[j] |= b[j];
                clearBytes[j] |= (byte)~b[j];
            }
            set |= (byte)(((b[6] & 0x0F) << 4) | (b[7] & 0x0F));
            clear |= (byte)~(((b[6] & 0x0F) << 4) | (b[7] & 0x0F));
        }

        setBytes.Take(6).Should().AllSatisfy(x => x.Should().Be(0xFF));
        clearBytes.Take(6).Should().AllSatisfy(x => x.Should().Be(0xFF));
        set.Should().Be(0xFF);
        clear.Should().Be(0xFF);
    }

    [Fact]
    public void NewSqlServerGuid_ShouldNotAllocate()
    {
        for (int i = 0; i < 1_000; i++)
            _ = Guider.NewSqlServerGuid();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10_000; i++)
            _ = Guider.NewSqlServerGuid();
        (GC.GetAllocatedBytesForCurrentThread() - before).Should().Be(0);
    }
}
