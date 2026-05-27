using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.Infrastructure.MessageCreators;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public sealed class LogMessageCreatorTests
{
    private static RequestResponseContext CreateContext(
        string? requestBody = null,
        string? responseBody = null,
        string path = "/test",
        string method = "GET",
        string queryString = "",
        string host = "localhost")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Path = path;
        if (!string.IsNullOrEmpty(queryString))
            httpContext.Request.QueryString = new QueryString(queryString);
        httpContext.Request.Host = new HostString(host);

        return new RequestResponseContext(httpContext)
        {
            RequestBody = requestBody,
            ResponseBody = responseBody,
            ResponseCreationTime = TimeSpan.FromMilliseconds(123)
        };
    }

    [Fact]
    public void Create_Should_ReturnNonEmptyLogString_When_FieldsConfigured()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Path]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext();

        (string logString, List<string?>? values) = creator.Create(context);

        logString.Should().NotBeNullOrEmpty();
        values.Should().BeNull();
    }

    [Fact]
    public void Create_Should_ContainRequestBody_When_RequestFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Request]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext(requestBody: "my request payload");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("my request payload");
    }

    [Fact]
    public void Create_Should_ContainResponseBody_When_ResponseFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Response]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext(responseBody: "my response payload");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("my response payload");
    }

    [Fact]
    public void Create_Should_ContainPath_When_PathFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Path]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext(path: "/api/users");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("/api/users");
    }

    [Fact]
    public void Create_Should_ContainMethod_When_MethodFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Method]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext(method: "POST");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("POST");
    }

    [Fact]
    public void Create_Should_ContainQueryString_When_QueryStringFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.QueryString]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext(queryString: "?id=42");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("id=42");
    }

    [Fact]
    public void Create_Should_ContainHostName_When_HostNameFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.HostName]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext(host: "example.com");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("example.com");
    }

    [Fact]
    public void Create_Should_ContainResponseTiming_When_ResponseTimingFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.ResponseTiming]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext();

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("ResponseTiming");
    }

    [Fact]
    public void Create_Should_ContainRequestLength_When_RequestLengthFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.RequestLength]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext(requestBody: "hello");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("5");
    }

    [Fact]
    public void Create_Should_ContainResponseLength_When_ResponseLengthFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.ResponseLength]
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext(responseBody: "hello world");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("11");
    }

    [Fact]
    public void Create_Should_ContainHeaders_When_HeadersFieldIncluded()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = [LogFields.Headers],
            HeaderKeys = ["X-Correlation-Id"]
        };
        var creator = new LogMessageCreator(options);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-Id"] = "corr-999";
        var context = new RequestResponseContext(httpContext);

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("corr-999");
    }

    [Fact]
    public void Create_Should_ReturnEmptyString_When_NoFieldsConfigured()
    {
        var options = new LoggingOptions
        {
            UseSeparateContext = false,
            LoggingFields = []
        };
        var creator = new LogMessageCreator(options);
        var context = CreateContext();

        (string logString, List<string?>? values) = creator.Create(context);

        logString.Should().BeEmpty();
        values.Should().BeNull();
    }

    [Fact]
    public void Create_Should_ContainAllFields_When_AllFieldsConfigured()
    {
        var options = LoggingOptions.CreateAllFields();
        options.UseSeparateContext = false;
        var creator = new LogMessageCreator(options);
        var context = CreateContext(requestBody: "req", responseBody: "resp", path: "/all", method: "PUT");

        (string logString, _) = creator.Create(context);

        logString.Should().Contain("req");
        logString.Should().Contain("resp");
        logString.Should().Contain("/all");
        logString.Should().Contain("PUT");
    }
}
