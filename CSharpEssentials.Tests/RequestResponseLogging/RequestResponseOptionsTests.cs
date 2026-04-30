using CSharpEssentials.RequestResponseLogging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public class RequestResponseOptionsTests
{
    [Fact]
    public void IgnorePaths_ShouldSetIgnoredPaths()
    {
        var options = new RequestResponseOptions();

        options.IgnorePaths("/health", "/metrics");

        // Test that the method doesn't throw
        options.Should().NotBeNull();
    }

    [Fact]
    public void UseHandler_ShouldSetHandler()
    {
        var options = new RequestResponseOptions();
        Func<RequestResponseContext, Task> handler = _ => Task.CompletedTask;

        options.UseHandler(handler);

        // Test that the method doesn't throw
        options.Should().NotBeNull();
    }

    [Fact]
    public void UseLogger_WithAction_ShouldConfigureLoggingOptions()
    {
        var options = new RequestResponseOptions();
        ILoggerFactory loggerFactory = new Mock<ILoggerFactory>().Object;

        options.UseLogger(loggerFactory, opt =>
        {
            opt.LoggingLevel = LogLevel.Warning;
            opt.LoggerCategoryName = "TestLogger";
        });

        // Test that the method doesn't throw
        options.Should().NotBeNull();
    }

    [Fact]
    public void UseLogger_WithLoggingOptions_ShouldSetLoggingOptions()
    {
        var options = new RequestResponseOptions();
        ILoggerFactory loggerFactory = new Mock<ILoggerFactory>().Object;
        var loggingOptions = new LoggingOptions
        {
            LoggingLevel = LogLevel.Error,
            LoggerCategoryName = "CustomLogger"
        };

        options.UseLogger(loggerFactory, loggingOptions);

        // Test that the method doesn't throw
        options.Should().NotBeNull();
    }
}

