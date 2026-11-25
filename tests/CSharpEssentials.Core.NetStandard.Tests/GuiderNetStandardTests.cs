using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Core.NetStandard.Tests;

public sealed class GuiderNetStandardTests
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
    public void NewGuid_WithTimestamp_ShouldEncodeThatTime()
    {
        var timestamp = new DateTimeOffset(2026, 3, 4, 5, 6, 7, 891, TimeSpan.Zero);
        Guid guid = Guider.NewGuid(timestamp);
        byte[] b = guid.ToByteArray(bigEndian: true);

        guid.Version.Should().Be(7);
        (b[8] >> 6).Should().Be(0b10);
        UnixMs(b).Should().Be(timestamp.ToUnixTimeMilliseconds());
    }

    [Theory]
    [InlineData("www.example.com", "2ed6657d-e927-568b-95e1-2665a8aea6a2")]
    public void NewGuidV5_ShouldMatchTheRfc9562Example(string name, string expected)
    {
        Guider.NewGuidV5(GuidNamespaces.Dns, name).Should().Be(new Guid(expected));
        Guider.NewGuidV5(GuidNamespaces.Dns, System.Text.Encoding.UTF8.GetBytes(name)).Should().Be(new Guid(expected));
    }

    [Fact]
    public void NewGuidV5_ShouldHashLongNamesLikeTheirBytes()
    {
        string name = new('x', 10_000);
        Guid guid = Guider.NewGuidV5(GuidNamespaces.Url, name);

        guid.Version.Should().Be(5);
        guid.Should().Be(Guider.NewGuidV5(GuidNamespaces.Url, System.Text.Encoding.UTF8.GetBytes(name)));
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
