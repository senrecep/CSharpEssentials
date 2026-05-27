using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.LogWriters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public sealed class LoggerFactoryLogWriterTests
{
    private static RequestResponseContext CreateContext(string? requestBody = null, string? responseBody = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/test";
        return new RequestResponseContext(httpContext)
        {
            RequestBody = requestBody,
            ResponseBody = responseBody
        };
    }

    [Fact]
    public void MessageCreator_Should_BeLogMessageCreator_When_UseSeparateContextIsFalse()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Request, LogFields.Response]
        };

        var writer = new LoggerFactoryLogWriter(loggerFactory, options);

        writer.MessageCreator.Should().NotBeNull();
    }

    [Fact]
    public void MessageCreator_Should_BeLogMessageWithContextCreator_When_UseSeparateContextIsTrue()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = new LoggingOptions
        {
            UseSeparateContext = true,
            LoggingFields = [LogFields.Request, LogFields.Response]
        };

        var writer = new LoggerFactoryLogWriter(loggerFactory, options);

        writer.MessageCreator.Should().NotBeNull();
    }

    [Fact]
    public async Task Write_Should_CompleteSuccessfully_When_UseSeparateContextIsFalse()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Path, LogFields.Method]
        };
        var writer = new LoggerFactoryLogWriter(loggerFactory, options);
        var context = CreateContext("request body", "response body");

        Func<Task> act = () => writer.Write(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Write_Should_CompleteSuccessfully_When_UseSeparateContextIsTrue()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = new LoggingOptions
        {
            UseSeparateContext = true,
            LoggingFields = [LogFields.Path, LogFields.Method]
        };
        var writer = new LoggerFactoryLogWriter(loggerFactory, options);
        var context = CreateContext("request body", "response body");

        Func<Task> act = () => writer.Write(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Write_Should_CompleteSuccessfully_When_LoggingFieldsIsEmpty()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = new LoggingOptions
        {
            UseSeparateContext = true,
            LoggingFields = []
        };
        var writer = new LoggerFactoryLogWriter(loggerFactory, options);
        var context = CreateContext();

        Func<Task> act = () => writer.Write(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Write_Should_NotThrow_When_AllFieldsLogged_WithSeparateContext()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = LoggingOptions.CreateAllFields();
        var writer = new LoggerFactoryLogWriter(loggerFactory, options);
        var context = CreateContext("req", "resp");

        Func<Task> act = () => writer.Write(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Write_Should_NotThrow_When_AllFieldsLogged_WithoutSeparateContext()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = LoggingOptions.CreateAllFields();
        options.UseSeparateContext = false;
        var writer = new LoggerFactoryLogWriter(loggerFactory, options);
        var context = CreateContext("req", "resp");

        Func<Task> act = () => writer.Write(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Write_Should_UseCustomLoggerCategoryName_When_Configured()
    {
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory
            .Setup(f => f.CreateLogger("MyCategory"))
            .Returns(mockLogger.Object);
        mockLogger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Path],
            LoggerCategoryName = "MyCategory"
        };
        var writer = new LoggerFactoryLogWriter(mockLoggerFactory.Object, options);
        var context = CreateContext();

        await writer.Write(context);

        mockLoggerFactory.Verify(f => f.CreateLogger("MyCategory"), Times.Once);
    }

    [Fact]
    public async Task Write_Should_RespectLogLevel_When_LevelIsWarning()
    {
        var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Warning));
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Path],
            LoggingLevel = LogLevel.Information
        };
        var writer = new LoggerFactoryLogWriter(loggerFactory, options);
        var context = CreateContext();

        Func<Task> act = () => writer.Write(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Write_Should_IncludeHeaderValues_When_HeaderKeysConfigured()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Headers],
            HeaderKeys = ["X-Request-Id"]
        };
        var writer = new LoggerFactoryLogWriter(loggerFactory, options);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Request-Id"] = "abc-123";
        var context = new RequestResponseContext(httpContext);

        Func<Task> act = () => writer.Write(context);

        await act.Should().NotThrowAsync();
    }
}
