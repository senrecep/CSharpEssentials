using CSharpEssentials.Time;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.Time;

public class DateTimeProviderTests
{
    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeOffset expectedTime = new(2024, 6, 15, 12, 30, 45, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(expectedTime);
        
        DateTimeProvider provider = new(fakeTimeProvider);

        provider.UtcNow.Should().Be(expectedTime);
    }

    [Fact]
    public void UtcNowDateTime_ShouldReturnCurrentUtcDateTime()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeOffset expectedTime = new(2024, 6, 15, 12, 30, 45, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(expectedTime);
        
        DateTimeProvider provider = new(fakeTimeProvider);

        provider.UtcNowDateTime.Should().Be(expectedTime.DateTime);
    }

    [Fact]
    public void TimeZone_ShouldReturnLocalTimeZone()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeProvider provider = new(fakeTimeProvider);

        provider.TimeZone.Should().Be(TimeZoneInfo.Local);
    }

    [Fact]
    public void TimeZoneUtc_ShouldReturnUtcTimeZone()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeProvider provider = new(fakeTimeProvider);

        provider.TimeZoneUtc.Should().Be(TimeZoneInfo.Utc);
    }

    [Fact]
    public void Constructor_WithNullTimeProvider_ShouldThrowArgumentNullException()
    {
        Action act = () => _ = new DateTimeProvider(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("timeProvider");
    }

    [Fact]
    public void UtcNow_ShouldReflectTimeAdvancement()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeOffset startTime = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(startTime);
        
        DateTimeProvider provider = new(fakeTimeProvider);

        provider.UtcNow.Should().Be(startTime);

        fakeTimeProvider.Advance(TimeSpan.FromHours(5));

        provider.UtcNow.Should().Be(startTime.AddHours(5));
    }
}
