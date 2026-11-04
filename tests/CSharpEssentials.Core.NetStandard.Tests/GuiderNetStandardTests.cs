using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Core.NetStandard.Tests;

public class GuiderNetStandardTests
{
    private static long UnixMs(byte[] b) =>
        ((long)b[0] << 40) | ((long)b[1] << 32) | ((long)b[2] << 24) | ((long)b[3] << 16) | ((long)b[4] << 8) | b[5];

    [Fact]
    public void NewGuid_ShouldReturnVersion7WithTheCurrentTimestamp()
    {
        for (int i = 0; i < 10_000; i++)
        {
            long before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Guid guid = Guider.NewGuid();
            long after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            byte[] b = guid.ToByteArray(bigEndian: true);

            guid.Version.Should().Be(7);
            (b[8] >> 6).Should().Be(0b10);
            UnixMs(b).Should().BeInRange(before, after);
        }
    }

    [Fact]
    public void NewGuid_ShouldReturnUniqueIds()
    {
        Enumerable.Range(0, 100_000).Select(_ => Guider.NewGuid()).Distinct().Should().HaveCount(100_000);
    }

    [Fact]
    public void NewMonotonicGuid_ShouldStrictlyIncrease()
    {
        Guid previous = Guider.NewMonotonicGuid();
        for (int i = 0; i < 100_000; i++)
        {
            Guid current = Guider.NewMonotonicGuid();

            current.Version.Should().Be(7);
            current.CompareTo(previous).Should().BePositive();
            current.ToByteArray(bigEndian: true).AsSpan().SequenceCompareTo(previous.ToByteArray(bigEndian: true)).Should().BePositive();

            previous = current;
        }
    }
}
