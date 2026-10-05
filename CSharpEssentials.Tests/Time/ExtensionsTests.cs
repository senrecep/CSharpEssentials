using CSharpEssentials.Time;
using FluentAssertions;

namespace CSharpEssentials.Tests.Time;

public class ExtensionsTests
{
    [Fact]
    public void ToTimeOnly_ShouldExtractTimeComponent()
    {
        DateTime dateTime = new(2024, 6, 15, 14, 30, 45, 123, DateTimeKind.Utc);

        var result = dateTime.ToTimeOnly();

        result.Should().Be(new TimeOnly(14, 30, 45, 123));
    }

    [Fact]
    public void ToTimeOnly_Midnight_ShouldReturnMidnight()
    {
        DateTime dateTime = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = dateTime.ToTimeOnly();

        result.Should().Be(TimeOnly.MinValue);
    }

    [Fact]
    public void ToDateOnly_ShouldExtractDateComponent()
    {
        DateTime dateTime = new(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);

        var result = dateTime.ToDateOnly();

        result.Should().Be(new DateOnly(2024, 6, 15));
    }

    [Fact]
    public void ToDateOnly_MinValue_ShouldReturnMinDateOnly()
    {
        DateTime dateTime = DateTime.MinValue;

        var result = dateTime.ToDateOnly();

        result.Should().Be(DateOnly.MinValue);
    }

    [Fact]
    public void ToDateOnly_MaxValue_ShouldReturnMaxDateOnly()
    {
        DateTime dateTime = DateTime.MaxValue;

        var result = dateTime.ToDateOnly();

        result.Should().Be(DateOnly.MaxValue);
    }

    [Fact]
    public void ToTimeOnly_MaxValue_ShouldReturnMaxTimeOnly()
    {
        DateTime dateTime = DateTime.MaxValue;

        var result = dateTime.ToTimeOnly();

        result.Should().Be(TimeOnly.MaxValue);
    }

    [Fact]
    public void ToDateOnly_And_ToTimeOnly_ShouldBeConsistentWithDateTime()
    {
        DateTime dateTime = new(2024, 12, 25, 8, 15, 30, DateTimeKind.Utc);

        var date = dateTime.ToDateOnly();
        var time = dateTime.ToTimeOnly();

        date.ToDateTime(time).Should().Be(dateTime);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, false, 2024, 6, 17)]
    [InlineData(DayOfWeek.Saturday, false, 2024, 6, 22)]
    [InlineData(DayOfWeek.Saturday, true, 2024, 6, 15)]
    [InlineData(DayOfWeek.Sunday, false, 2024, 6, 16)]
    public void NextDayOfWeek_DateOnly_ShouldReturnExpectedDate(DayOfWeek dayOfWeek, bool includeCurrent, int year, int month, int day)
    {
        DateOnly saturday = new(2024, 6, 15);

        saturday.NextDayOfWeek(dayOfWeek, includeCurrent).Should().Be(new DateOnly(year, month, day));
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, false, 2024, 6, 10)]
    [InlineData(DayOfWeek.Friday, false, 2024, 6, 14)]
    [InlineData(DayOfWeek.Saturday, false, 2024, 6, 8)]
    [InlineData(DayOfWeek.Saturday, true, 2024, 6, 15)]
    [InlineData(DayOfWeek.Sunday, false, 2024, 6, 9)]
    public void PreviousDayOfWeek_DateOnly_ShouldReturnExpectedDate(DayOfWeek dayOfWeek, bool includeCurrent, int year, int month, int day)
    {
        DateOnly saturday = new(2024, 6, 15);

        saturday.PreviousDayOfWeek(dayOfWeek, includeCurrent).Should().Be(new DateOnly(year, month, day));
    }

    [Fact]
    public void NextDayOfWeek_DateTime_ShouldKeepTimeComponent()
    {
        DateTime saturday = new(2024, 12, 28, 9, 30, 0, DateTimeKind.Utc);

        saturday.NextDayOfWeek(DayOfWeek.Wednesday).Should().Be(new DateTime(2025, 1, 1, 9, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void PreviousDayOfWeek_DateTime_ShouldCrossMonthBoundary()
    {
        DateTime monday = new(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        monday.PreviousDayOfWeek(DayOfWeek.Friday).Should().Be(new DateTime(2024, 6, 28, 0, 0, 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData(2000, 6, 15, 2024, 6, 15, 24)]
    [InlineData(2000, 6, 15, 2024, 6, 14, 23)]
    [InlineData(2000, 6, 15, 2024, 6, 16, 24)]
    [InlineData(2000, 12, 31, 2024, 1, 1, 23)]
    [InlineData(2024, 6, 15, 2024, 6, 15, 0)]
    [InlineData(2000, 2, 29, 2023, 2, 28, 22)]
    [InlineData(2000, 2, 29, 2023, 3, 1, 23)]
    [InlineData(2000, 2, 29, 2024, 2, 29, 24)]
    public void GetAge_ShouldCountCompletedYears(int birthYear, int birthMonth, int birthDay, int year, int month, int day, int expected)
    {
        new DateOnly(birthYear, birthMonth, birthDay).GetAge(new DateOnly(year, month, day)).Should().Be(expected);
    }

    [Fact]
    public void GetAge_WhenBirthDateIsInFuture_ShouldThrow()
    {
        Action act = () => new DateOnly(2025, 1, 1).GetAge(new DateOnly(2024, 1, 1));

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("birthDate");
    }

    [Fact]
    public void GetAge_WithDateTimeProvider_ShouldUseProviderDate()
    {
        var provider = new FakeDateTimeProvider(new DateTimeOffset(2024, 6, 15, 10, 0, 0, TimeSpan.Zero));

        new DateOnly(2000, 6, 15).GetAge(provider).Should().Be(24);
        new DateOnly(2000, 6, 16).GetAge(provider).Should().Be(23);
    }

    [Fact]
    public void GetAge_WithNullProvider_ShouldThrow()
    {
        Action act = () => new DateOnly(2000, 1, 1).GetAge((IDateTimeProvider)null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
