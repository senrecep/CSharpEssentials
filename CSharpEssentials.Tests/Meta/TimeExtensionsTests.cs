using CSharpEssentials.Maybe;
using CSharpEssentials.Time;
using FluentAssertions;

namespace CSharpEssentials.Tests.Meta;

public class TimeExtensionsTests
{
    [Fact]
    public void MsToDateTime_WithValue_ShouldReturnSome()
    {
        long? value = 1609459200000; // 2021-01-01 00:00:00 UTC

        Maybe<DateTime> result = value.MsToDateTime();

        result.HasValue.Should().BeTrue();
        result.Value.Should().BeCloseTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MsToDateTime_WithNull_ShouldReturnNone()
    {
        long? value = null;

        Maybe<DateTime> result = value.MsToDateTime();

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void MsToDateTime_WithNull_WithDefaultValue_ShouldReturnDefault()
    {
        long? value = null;
        DateTime defaultValue = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Maybe<DateTime> result = value.MsToDateTime(defaultValue);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(defaultValue);
    }

    [Fact]
    public void MsToDateTime_WithValue_ShouldIgnoreDefaultValue()
    {
        long? value = 1609459200000;
        DateTime defaultValue = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Maybe<DateTime> result = value.MsToDateTime(defaultValue);

        result.HasValue.Should().BeTrue();
        result.Value.Should().NotBe(defaultValue);
    }
}
