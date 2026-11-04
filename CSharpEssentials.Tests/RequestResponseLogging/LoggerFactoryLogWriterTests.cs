using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.LogWriters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
        using var loggerFactory = LoggerFactory.Create(_ => { });
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
        using var loggerFactory = LoggerFactory.Create(_ => { });
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
        using var loggerFactory = LoggerFactory.Create(_ => { });
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
        using var loggerFactory = LoggerFactory.Create(_ => { });
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
        using var loggerFactory = LoggerFactory.Create(_ => { });
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
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var options = LoggingOptions.CreateAllFields();
        var writer = new LoggerFactoryLogWriter(loggerFactory, options);
        var context = CreateContext("req", "resp");

        Func<Task> act = () => writer.Write(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Write_Should_NotThrow_When_AllFieldsLogged_WithoutSeparateContext()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
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
        using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Warning));
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
        using var loggerFactory = LoggerFactory.Create(_ => { });
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

    [Fact]
    public async Task Write_Should_NotBuildMessage_When_LevelIsDisabled()
    {
        var logger = new RecordingLogger(enabled: false);
        var options = new LoggingOptions
        {
            UseSeparateContext = true,
            LoggingFields = [LogFields.Path]
        };
        var writer = new LoggerFactoryLogWriter(logger.CreateFactory(), options);
        var requestFeature = new Mock<IHttpRequestFeature>();
        requestFeature.SetupProperty(f => f.Path, "/test");
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set(requestFeature.Object);

        await writer.Write(new RequestResponseContext(httpContext));

        requestFeature.VerifyGet(f => f.Path, Times.Never);
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task Write_Should_LogTemplateWithValues_When_UseSeparateContextIsTrue()
    {
        var logger = new RecordingLogger(enabled: true);
        var options = new LoggingOptions
        {
            UseSeparateContext = true,
            LoggingFields = [LogFields.Path, LogFields.Request]
        };
        var writer = new LoggerFactoryLogWriter(logger.CreateFactory(), options);

        await writer.Write(CreateContext(requestBody: "{\"id\":1}"));
        await writer.Write(CreateContext(requestBody: "second"));

        string nl = Environment.NewLine;
        logger.Entries.Select(e => e.Message).Should().Equal(
            $"Path: /test{nl}Request: {{\"id\":1}}{nl}",
            $"Path: /test{nl}Request: second{nl}");
        logger.Entries[1].State.Should().Equal(
            new KeyValuePair<string, object?>("Path", "/test"),
            new KeyValuePair<string, object?>("Request", "second"),
            new KeyValuePair<string, object?>("{OriginalFormat}", $"Path: {{Path}}{nl}Request: {{Request}}{nl}"));
    }
}
