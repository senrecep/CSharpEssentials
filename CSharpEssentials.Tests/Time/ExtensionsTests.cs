using CSharpEssentials.Time;
using FluentAssertions;

namespace CSharpEssentials.Tests.Time;

public class ExtensionsTests
{
    [Fact]
    public void ToTimeOnly_ShouldExtractTimeComponent()
    {
        DateTime dateTime = new(2024, 6, 15, 14, 30, 45, 123, DateTimeKind.Utc);

        TimeOnly result = dateTime.ToTimeOnly();

        result.Should().Be(new TimeOnly(14, 30, 45, 123));
    }

    [Fact]
    public void ToTimeOnly_Midnight_ShouldReturnMidnight()
    {
        DateTime dateTime = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        TimeOnly result = dateTime.ToTimeOnly();

        result.Should().Be(TimeOnly.MinValue);
    }

    [Fact]
    public void ToDateOnly_ShouldExtractDateComponent()
    {
        DateTime dateTime = new(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);

        DateOnly result = dateTime.ToDateOnly();

        result.Should().Be(new DateOnly(2024, 6, 15));
    }

    [Fact]
    public void ToDateOnly_MinValue_ShouldReturnMinDateOnly()
    {
        DateTime dateTime = DateTime.MinValue;

        DateOnly result = dateTime.ToDateOnly();

        result.Should().Be(DateOnly.MinValue);
    }

    [Fact]
    public void ToDateOnly_MaxValue_ShouldReturnMaxDateOnly()
    {
        DateTime dateTime = DateTime.MaxValue;

        DateOnly result = dateTime.ToDateOnly();

        result.Should().Be(DateOnly.MaxValue);
    }

    [Fact]
    public void ToTimeOnly_MaxValue_ShouldReturnMaxTimeOnly()
    {
        DateTime dateTime = DateTime.MaxValue;

        TimeOnly result = dateTime.ToTimeOnly();

        result.Should().Be(TimeOnly.MaxValue);
    }

    [Fact]
    public void ToDateOnly_And_ToTimeOnly_ShouldBeConsistentWithDateTime()
    {
        DateTime dateTime = new(2024, 12, 25, 8, 15, 30, DateTimeKind.Utc);

        DateOnly date = dateTime.ToDateOnly();
        TimeOnly time = dateTime.ToTimeOnly();

        date.ToDateTime(time).Should().Be(dateTime);
    }
}
