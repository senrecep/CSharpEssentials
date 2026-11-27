using CSharpEssentials.Time;
using FluentAssertions;

namespace CSharpEssentials.Tests.Time;

public class TimeZonesTests
{
    private static readonly DateTimeOffset LateEvening = new(2024, 6, 15, 22, 30, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("Europe/Istanbul")]
    [InlineData("Turkey Standard Time")]
    public void Find_ShouldResolveIanaAndWindowsIds(string id)
    {
        TimeZoneInfo zone = TimeZones.Find(id);

        zone.GetUtcOffset(LateEvening).Should().Be(TimeSpan.FromHours(3));
    }

    [Fact]
    public void Find_ShouldResolveUtc()
    {
        TimeZones.Find("UTC").GetUtcOffset(LateEvening).Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Find_WithUnknownId_ShouldThrowWithTheIdInTheMessage()
    {
        Action act = () => TimeZones.Find("Mars/Olympus_Mons");

        act.Should().Throw<TimeZoneNotFoundException>().WithMessage("*'Mars/Olympus_Mons'*");
    }

    [Fact]
    public void Find_WithNull_ShouldThrowArgumentNullException()
    {
        Action act = () => TimeZones.Find(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("id");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Mars/Olympus_Mons")]
    public void TryFind_WithUnknownOrBlankId_ShouldReturnFalse(string? id)
    {
        bool found = TimeZones.TryFind(id, out TimeZoneInfo? zone);

        found.Should().BeFalse();
        zone.Should().BeNull();
    }

    [Fact]
    public void TryFind_WithKnownId_ShouldReturnTheZone()
    {
        bool found = TimeZones.TryFind("America/New_York", out TimeZoneInfo? zone);

        found.Should().BeTrue();
        zone!.GetUtcOffset(LateEvening).Should().Be(TimeSpan.FromHours(-4));
    }

    [Fact]
    public void NowIn_ShouldConvertTheProviderTimeToTheZone()
    {
        var clock = new FakeDateTimeProvider(LateEvening);

        DateTimeOffset now = clock.NowIn(TimeZones.Find("Europe/Istanbul"));

        now.Should().Be(LateEvening);
        now.Offset.Should().Be(TimeSpan.FromHours(3));
        now.DateTime.Should().Be(new DateTime(2024, 6, 16, 1, 30, 0));
    }

    [Fact]
    public void TodayIn_ShouldReturnTheDateInTheZone()
    {
        var clock = new FakeDateTimeProvider(LateEvening);

        clock.TodayIn(TimeZones.Find("Europe/Istanbul")).Should().Be(new DateOnly(2024, 6, 16));
        clock.TodayIn(TimeZones.Find("America/New_York")).Should().Be(new DateOnly(2024, 6, 15));
    }

    [Fact]
    public void NowIn_WithNullArguments_ShouldThrow()
    {
        var clock = new FakeDateTimeProvider(LateEvening);

        Action nullZone = () => clock.NowIn(null!);
        Action nullClock = () => ((IDateTimeProvider)null!).NowIn(TimeZoneInfo.Utc);

        nullZone.Should().Throw<ArgumentNullException>().WithParameterName("zone");
        nullClock.Should().Throw<ArgumentNullException>().WithParameterName("dateTimeProvider");
    }

    [Fact]
    public void GetAge_WithZone_ShouldTurnAtMidnightInThatZone()
    {
        var clock = new FakeDateTimeProvider(LateEvening);
        var birthDate = new DateOnly(2000, 6, 16);

        birthDate.GetAge(clock, TimeZones.Find("Europe/Istanbul")).Should().Be(24);
        birthDate.GetAge(clock, TimeZones.Find("America/New_York")).Should().Be(23);
    }
}
