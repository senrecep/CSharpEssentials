using System.Text;
using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.Infrastructure.Interfaces;
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

    [Fact]
    public async Task InvokeAsync_Should_NotReadTheBody_When_ContentLengthExceedsTenMegabytes()
    {
        RequestResponseContext? captured = null;
        DefaultRequestResponseWithHandlerMiddleware middleware = CreateHandlerMiddleware(
            _ => Task.CompletedTask, ctx => captured = ctx, new NullLogWriter());
        DefaultHttpContext httpContext = CreateContext("/upload", "small");
        httpContext.Request.ContentLength = (10 * 1024 * 1024) + 1;

        await middleware.InvokeAsync(httpContext);

        captured!.RequestBody.Should().Be("Request body too large: 10485761 bytes");
        httpContext.Request.Body.Position.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_Should_ReportEmptyBodies_When_RequestAndResponseHaveNoContent()
    {
        RequestResponseContext? captured = null;
        DefaultRequestResponseWithHandlerMiddleware middleware = CreateHandlerMiddleware(
            _ => Task.CompletedTask, ctx => captured = ctx, new NullLogWriter());

        await middleware.InvokeAsync(CreateContext("/orders", ""));

        captured!.RequestBody.Should().Be("Empty request body");
        captured.ResponseBody.Should().Be("Empty response body");
    }

    [Fact]
    public async Task InvokeAsync_Should_LogErrorContextAndRethrow_When_PipelineThrows()
    {
        var writer = new RecordingLogWriter();
        var middleware = new DefaultRequestResponseMiddleware(
            _ => throw new InvalidOperationException("boom"), writer, []);
        DefaultHttpContext httpContext = CreateContext("/orders", "payload");
        Stream originalBody = httpContext.Response.Body;

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        RequestResponseContext logged = writer.Written.Should().ContainSingle().Subject;
        logged.RequestBody.Should().Be("payload");
        logged.ResponseBody.Should().Be("Exception: InvalidOperationException, Message: boom");
        httpContext.Response.Body.Should().BeSameAs(originalBody);
    }

    [Fact]
    public async Task InvokeAsync_Should_ReplaceResponseBodyWithWriterError_When_LogWriterThrows()
    {
        RequestResponseContext? captured = null;
        DefaultRequestResponseWithHandlerMiddleware middleware = CreateHandlerMiddleware(
            context => context.Response.WriteAsync("visible"), ctx => captured = ctx, new ThrowingLogWriter());
        DefaultHttpContext httpContext = CreateContext("/orders", "payload");

        await middleware.InvokeAsync(httpContext);

        captured!.RequestBody.Should().Be("payload");
        captured.ResponseBody.Should().Be("Error writing log: writer down");
    }

    private static DefaultRequestResponseWithHandlerMiddleware CreateHandlerMiddleware(
        RequestDelegate next, Action<RequestResponseContext> capture, ILogWriter writer)
        => new(next, ctx =>
        {
            capture(ctx);
            return Task.CompletedTask;
        }, writer, []);

    private sealed class RecordingLogWriter : ILogWriter
    {
        public List<RequestResponseContext> Written { get; } = [];
        public IMessageCreator? MessageCreator => null;

        public Task Write(RequestResponseContext requestResponseContext)
        {
            Written.Add(requestResponseContext);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingLogWriter : ILogWriter
    {
        public IMessageCreator? MessageCreator => null;
        public Task Write(RequestResponseContext requestResponseContext) => throw new InvalidOperationException("writer down");
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
