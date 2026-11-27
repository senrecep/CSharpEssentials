using CSharpEssentials.Core;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.Core;

public sealed class GuiderTimestampTests
{
    private static readonly DateTimeOffset Fixed = new(2026, 3, 4, 5, 6, 7, 891, TimeSpan.FromHours(3));

    private static long UnixMs(Guid guid)
    {
        byte[] b = guid.ToByteArray(bigEndian: true);
        return ((long)b[0] << 40) | ((long)b[1] << 32) | ((long)b[2] << 24) | ((long)b[3] << 16) | ((long)b[4] << 8) | b[5];
    }

    [Fact]
    public void NewGuid_WithTimestamp_Should_Encode_That_Time_As_Version7()
    {
        Guid guid = Guider.NewGuid(Fixed);

        UnixMs(guid).Should().Be(Fixed.ToUnixTimeMilliseconds());
        guid.Version.Should().Be(7);
        (guid.Variant & 0b1100).Should().Be(0b1000);
    }

    [Fact]
    public void NewGuid_WithTimestamp_Should_Match_The_Bcl_Layout()
    {
        Guid ours = Guider.NewGuid(Fixed);
        Guid bcl = Guid.CreateVersion7(Fixed);

        ours.ToByteArray(bigEndian: true)[..6].Should().Equal(bcl.ToByteArray(bigEndian: true)[..6]);
    }

    [Fact]
    public void NewGuid_WithTimestamp_Should_Return_Unique_Ids_For_The_Same_Time()
    {
        Enumerable.Range(0, 10_000).Select(_ => Guider.NewGuid(Fixed)).Distinct().Should().HaveCount(10_000);
    }

    [Fact]
    public void NewGuid_WithTimestamp_Should_Accept_The_Unix_Epoch_And_Reject_Earlier_Times()
    {
        UnixMs(Guider.NewGuid(DateTimeOffset.UnixEpoch)).Should().Be(0);

        Action before = () => Guider.NewGuid(DateTimeOffset.UnixEpoch.AddTicks(-1));
        before.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("timestamp");
    }

    [Fact]
    public void NewGuid_WithTimeProvider_Should_Follow_The_Fake_Clock()
    {
        var clock = new FakeTimeProvider(Fixed);

        UnixMs(Guider.NewGuid(clock)).Should().Be(Fixed.ToUnixTimeMilliseconds());
        clock.Advance(TimeSpan.FromMinutes(5));
        UnixMs(Guider.NewGuid(clock)).Should().Be(Fixed.AddMinutes(5).ToUnixTimeMilliseconds());
    }

    [Fact]
    public void NewGuid_WithTimeProvider_Should_Reject_Null()
    {
        Action act = () => Guider.NewGuid((TimeProvider)null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("timeProvider");
    }

    [Fact]
    public void NewGuid_WithTimestamp_Should_Not_Break_The_Monotonic_Order()
    {
        Guid previous = Guider.NewMonotonicGuid();
        for (int i = 0; i < 1_000; i++)
        {
            _ = Guider.NewGuid(DateTimeOffset.UnixEpoch);
            Guid current = Guider.NewMonotonicGuid();
            current.CompareTo(previous).Should().BePositive();
            previous = current;
        }
    }
}
