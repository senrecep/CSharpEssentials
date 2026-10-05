using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public class ComparableExtensionsTests
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(0, false)]
    [InlineData(11, false)]
    public void IsBetween_ShouldIncludeBounds(int value, bool expected)
    {
        value.IsBetween(1, 10).Should().Be(expected);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(1, false)]
    [InlineData(10, false)]
    [InlineData(0, false)]
    [InlineData(11, false)]
    public void IsBetweenExclusive_ShouldExcludeBounds(int value, bool expected)
    {
        value.IsBetweenExclusive(1, 10).Should().Be(expected);
    }

    [Fact]
    public void IsBetween_WhenMinGreaterThanMax_ShouldReturnFalse()
    {
        5.IsBetween(10, 1).Should().BeFalse();
    }

    [Fact]
    public void IsBetween_WithDates_ShouldCompareChronologically()
    {
        DateTime start = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc).IsBetween(start, end).Should().BeTrue();
        new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc).IsBetween(start, end).Should().BeFalse();
    }

    [Fact]
    public void IsBetween_WithStrings_ShouldUseDefaultComparer()
    {
        "m".IsBetween("a", "z").Should().BeTrue();
        "zz".IsBetween("a", "z").Should().BeFalse();
    }

    [Fact]
    public void IsBetween_WithNullString_ShouldTreatNullAsSmallest()
    {
        string? value = null;

        value!.IsBetween("a", "z").Should().BeFalse();
        "a".IsBetween(null!, "z").Should().BeTrue();
    }

    [Fact]
    public void IsBetween_WithDecimals_ShouldRespectPrecision()
    {
        10.0001m.IsBetween(0m, 10m).Should().BeFalse();
        9.9999m.IsBetweenExclusive(0m, 10m).Should().BeTrue();
    }
}
