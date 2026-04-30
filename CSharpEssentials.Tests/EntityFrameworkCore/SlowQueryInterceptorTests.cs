using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class SlowQueryInterceptorTests
{
    [Fact]
    public void SlowQueryInterceptor_Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        Func<SlowQueryInterceptor> action = () => new SlowQueryInterceptor(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SlowQueryInterceptor_Constructor_ShouldNotThrow_WhenLoggerIsProvided()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();

        Func<SlowQueryInterceptor> action = () => new SlowQueryInterceptor(loggerMock.Object);

        action.Should().NotThrow();
    }

    // Note: Testing the actual interceptor methods would require setting up DbCommand and EventData,
    // which is complex. The interceptor is primarily tested through integration tests.
    // These unit tests verify the constructor behavior.
}

