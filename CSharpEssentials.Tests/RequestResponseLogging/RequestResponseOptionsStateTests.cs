using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.LogWriters;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public sealed class RequestResponseOptionsStateTests
{
    [Fact]
    public void NewOptions_ShouldHaveNoWriterHandlerOrIgnoredPaths()
    {
        var options = new RequestResponseOptions();

        options.LogWriter.Should().BeNull();
        options.LogWriterUsing.Should().BeFalse();
        options.ReqResHandler.Should().BeNull();
        options.HandlerUsing.Should().BeFalse();
        options.IgnoredPaths.Should().BeEmpty();
        options.LoggerFactory.Should().BeNull();
    }

    [Fact]
    public void UseLogWriter_ShouldStoreWriter_And_ReportItIsUsed()
    {
        var options = new RequestResponseOptions();
        var writer = new NullLogWriter();

        options.UseLogWriter(writer);

        options.LogWriter.Should().BeSameAs(writer);
        options.LogWriterUsing.Should().BeTrue();
    }

    [Fact]
    public void UseHandler_ShouldStoreHandler_And_ReportItIsUsed()
    {
        var options = new RequestResponseOptions();
        Func<RequestResponseContext, Task> handler = _ => Task.CompletedTask;

        options.UseHandler(handler);

        options.ReqResHandler.Should().BeSameAs(handler);
        options.HandlerUsing.Should().BeTrue();
    }

    [Fact]
    public void IgnorePaths_ShouldReplacePreviouslyIgnoredPaths()
    {
        var options = new RequestResponseOptions();

        options.IgnorePaths("/health");
        options.IgnorePaths("/metrics", "/swagger");

        options.IgnoredPaths.Should().Equal("/metrics", "/swagger");
    }

    [Fact]
    public void UseLogger_WithAction_ShouldApplyConfigurationToFreshLoggingOptions()
    {
        var options = new RequestResponseOptions();
        ILoggerFactory loggerFactory = Mock.Of<ILoggerFactory>();
        LoggingOptions? configured = null;

        options.UseLogger(loggerFactory, opt =>
        {
            configured = opt;
            opt.LoggingLevel = LogLevel.Warning;
            opt.LoggerCategoryName = "Audit";
        });

        options.LoggerFactory.Should().BeSameAs(loggerFactory);
        options.LoggingOptions.Should().BeSameAs(configured);
        options.LoggingOptions.LoggingLevel.Should().Be(LogLevel.Warning);
        options.LoggingOptions.LoggerCategoryName.Should().Be("Audit");
    }

    [Fact]
    public void UseLogger_WithLoggingOptions_ShouldStoreGivenInstance()
    {
        var options = new RequestResponseOptions();
        ILoggerFactory loggerFactory = Mock.Of<ILoggerFactory>();
        var loggingOptions = new LoggingOptions { LoggingLevel = LogLevel.Error };

        options.UseLogger(loggerFactory, loggingOptions);

        options.LoggerFactory.Should().BeSameAs(loggerFactory);
        options.LoggingOptions.Should().BeSameAs(loggingOptions);
    }
}
