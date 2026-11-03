using System.Text;
using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;
using CSharpEssentials.RequestResponseLogging.LogWriters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public sealed class BaseMiddlewareRoutingTests
{
    [Fact]
    public void Constructor_WithNullLogWriter_ShouldThrowArgumentNullException()
    {
        Action act = () => _ = new DefaultRequestResponseMiddleware(_ => Task.CompletedTask, null!, []);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logWriter");
    }

    [Fact]
    public void Constructor_WithNullIgnoredPaths_ShouldThrowArgumentNullException()
    {
        Action act = () => _ = new DefaultRequestResponseMiddleware(_ => Task.CompletedTask, new NullLogWriter(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("ignoredPaths");
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_ShouldThrowArgumentNullException()
    {
        var middleware = new DefaultRequestResponseMiddleware(_ => Task.CompletedTask, new NullLogWriter(), []);

        Func<Task> act = () => middleware.InvokeAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("context");
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/HEALTHZ")]
    [InlineData("/health/")]
    public async Task InvokeAsync_Should_BypassLogging_When_PathMatchesIgnoredPrefixIgnoringCaseAndTrailingSlash(string path)
    {
        bool nextCalled = false;
        RequestResponseContext? captured = null;
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            ctx =>
            {
                captured = ctx;
                return Task.CompletedTask;
            },
            new NullLogWriter(),
            ["/Health/"]);
        DefaultHttpContext httpContext = CreateContext(path, "ignored");

        await middleware.InvokeAsync(httpContext);

        nextCalled.Should().BeTrue();
        captured.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_Should_SkipOnlyTheRequestBody_When_SkipRequestLoggingAttributeIsOnTheEndpoint()
    {
        RequestResponseContext? captured = null;
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            context => context.Response.WriteAsync("visible"),
            ctx =>
            {
                captured = ctx;
                return Task.CompletedTask;
            },
            new NullLogWriter(),
            []);
        DefaultHttpContext httpContext = CreateContext("/orders", "secret payload");
        httpContext.SetEndpoint(new Endpoint(null, new EndpointMetadataCollection(new SkipRequestLoggingAttribute()), "orders"));

        await middleware.InvokeAsync(httpContext);

        captured!.RequestBody.Should().Be("Skipped logging request body");
        captured.ResponseBody.Should().Be("visible");
    }

    private static DefaultHttpContext CreateContext(string path, string body)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = path;
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        httpContext.Request.Body = new MemoryStream(bytes);
        httpContext.Request.ContentLength = bytes.Length;
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }
}
