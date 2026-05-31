using CSharpEssentials.Time;
using FluentAssertions;

namespace CSharpEssentials.Tests.Time;

public class FakeDateTimeProviderTests
{
    private static readonly DateTimeOffset FixedTime = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void UtcNow_Should_ReturnFixedTime()
    {
        var provider = new FakeDateTimeProvider(FixedTime);
        provider.UtcNow.Should().Be(FixedTime);
    }

    [Fact]
    public void UtcNowDateTime_Should_ReturnFixedDateTime()
    {
        var provider = new FakeDateTimeProvider(FixedTime);
        provider.UtcNowDateTime.Should().Be(FixedTime.UtcDateTime);
    }

    [Fact]
    public void Advance_Should_MoveTimeForward()
    {
        var provider = new FakeDateTimeProvider(FixedTime);
        provider.Advance(TimeSpan.FromHours(1));
        provider.UtcNow.Should().Be(FixedTime.AddHours(1));
    }

    [Fact]
    public void SetTime_Should_ReplaceCurrentTime()
    {
        var provider = new FakeDateTimeProvider(FixedTime);
        var newTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        provider.SetTime(newTime);
        provider.UtcNow.Should().Be(newTime);
    }

    [Fact]
    public void Advance_Multiple_Should_Accumulate()
    {
        var provider = new FakeDateTimeProvider(FixedTime);
        provider.Advance(TimeSpan.FromMinutes(30));
        provider.Advance(TimeSpan.FromMinutes(30));
        provider.UtcNow.Should().Be(FixedTime.AddHours(1));
    }

    [Fact]
    public void TimeZone_Should_BeUtc()
    {
        var provider = new FakeDateTimeProvider(FixedTime);
        provider.TimeZone.Should().Be(TimeZoneInfo.Utc);
        provider.TimeZoneUtc.Should().Be(TimeZoneInfo.Utc);
    }
}
