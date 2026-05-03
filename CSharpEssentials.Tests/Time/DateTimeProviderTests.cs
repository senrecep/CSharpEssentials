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

    [Fact]
    public void UtcNowDate_ShouldReturnDatePartOfUtcNow()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeOffset expectedTime = new(2024, 6, 15, 12, 30, 45, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(expectedTime);

        DateTimeProvider provider = new(fakeTimeProvider);

        provider.UtcNowDate.Should().Be(new DateOnly(2024, 6, 15));
    }

    [Fact]
    public void UtcNowTime_ShouldReturnTimePartOfUtcNow()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeOffset expectedTime = new(2024, 6, 15, 12, 30, 45, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(expectedTime);

        DateTimeProvider provider = new(fakeTimeProvider);

        provider.UtcNowTime.Should().Be(new TimeOnly(12, 30, 45));
    }

    [Fact]
    public void UtcNowDate_ShouldChangeWhenUtcNowAdvances()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeOffset startTime = new(2024, 1, 1, 23, 59, 59, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(startTime);

        DateTimeProvider provider = new(fakeTimeProvider);
        provider.UtcNowDate.Should().Be(new DateOnly(2024, 1, 1));

        fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));

        provider.UtcNowDate.Should().Be(new DateOnly(2024, 1, 2));
    }

    [Fact]
    public void UtcNowTime_ShouldChangeWhenUtcNowAdvances()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeOffset startTime = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(startTime);

        DateTimeProvider provider = new(fakeTimeProvider);
        provider.UtcNowTime.Should().Be(new TimeOnly(0, 0, 0));

        fakeTimeProvider.Advance(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));

        provider.UtcNowTime.Should().Be(new TimeOnly(1, 30, 0));
    }

    [Fact]
    public void IDateTimeProvider_Interface_ShouldBeImplementedCorrectly()
    {
        FakeTimeProvider fakeTimeProvider = new();
        DateTimeProvider provider = new DateTimeProvider(fakeTimeProvider);

        provider.TimeZone.Should().Be(TimeZoneInfo.Local);
        provider.TimeZoneUtc.Should().Be(TimeZoneInfo.Utc);
        provider.UtcNow.Should().Be(fakeTimeProvider.GetUtcNow());
        provider.UtcNowDateTime.Should().Be(fakeTimeProvider.GetUtcNow().DateTime);
        provider.UtcNowDate.Should().Be(DateOnly.FromDateTime(fakeTimeProvider.GetUtcNow().DateTime));
        provider.UtcNowTime.Should().Be(TimeOnly.FromDateTime(fakeTimeProvider.GetUtcNow().DateTime));
    }
}
