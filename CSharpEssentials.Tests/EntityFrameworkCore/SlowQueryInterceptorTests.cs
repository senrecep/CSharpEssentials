using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class SlowQueryInterceptorTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        Func<SlowQueryInterceptor> action = () => new SlowQueryInterceptor(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WhenLoggerIsProvided()
    {
        Mock<ILogger<SlowQueryInterceptor>> loggerMock = new();

        Func<SlowQueryInterceptor> action = () => new SlowQueryInterceptor(loggerMock.Object);

        action.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ShouldAcceptSlowQueryOptions()
    {
        Mock<ILogger<SlowQueryInterceptor>> loggerMock = new();
        SlowQueryOptions options = new() { Threshold = TimeSpan.FromMilliseconds(500) };

        Func<SlowQueryInterceptor> action = () => new SlowQueryInterceptor(loggerMock.Object, options);

        action.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ShouldAcceptSlowQueryHandler()
    {
        Mock<ILogger<SlowQueryInterceptor>> loggerMock = new();
        Mock<ISlowQueryHandler> handlerMock = new();

        Func<SlowQueryInterceptor> action = () =>
            new SlowQueryInterceptor(loggerMock.Object, slowQueryHandler: handlerMock.Object);

        action.Should().NotThrow();
    }

    // ── SlowQueryOptions Tests ──────────────────────────────────────

    [Fact]
    public void SlowQueryOptions_DefaultThreshold_ShouldBeOneSecond()
    {
        SlowQueryOptions options = new();

        options.Threshold.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SlowQueryOptions_ShouldAllowCustomThreshold()
    {
        SlowQueryOptions options = new() { Threshold = TimeSpan.FromMilliseconds(250) };

        options.Threshold.Should().Be(TimeSpan.FromMilliseconds(250));
    }

    // ── DI Extension Tests ──────────────────────────────────────────

    [Fact]
    public void AddSlowQueryInterceptor_WithDefaults_ShouldRegisterInterceptor()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSlowQueryInterceptor();
        ServiceProvider provider = services.BuildServiceProvider();

        SlowQueryInterceptor interceptor = provider.GetRequiredService<SlowQueryInterceptor>();

        interceptor.Should().NotBeNull();

        provider.Dispose();
    }

    [Fact]
    public void AddSlowQueryInterceptor_WithThreshold_ShouldRegisterWithOptions()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(200));
        ServiceProvider provider = services.BuildServiceProvider();

        SlowQueryOptions options = provider.GetRequiredService<SlowQueryOptions>();

        options.Threshold.Should().Be(TimeSpan.FromMilliseconds(200));

        provider.Dispose();
    }

    [Fact]
    public void AddSlowQueryInterceptor_WithConfigure_ShouldApplyOptions()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSlowQueryInterceptor(opts => opts.Threshold = TimeSpan.FromSeconds(5));
        ServiceProvider provider = services.BuildServiceProvider();

        SlowQueryOptions options = provider.GetRequiredService<SlowQueryOptions>();

        options.Threshold.Should().Be(TimeSpan.FromSeconds(5));

        provider.Dispose();
    }
}
