using System.Text;
using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.Infrastructure.Interfaces;
using CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;
using CSharpEssentials.RequestResponseLogging.LogWriters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public sealed class BaseMiddlewareCaptureTests
{
    [Fact]
    public async Task InvokeAsync_Should_CaptureRequestBody_And_LeaveItReadableForTheHandler()
    {
        string? handlerBody = null;
        RequestResponseContext? captured = null;
        DefaultHttpContext httpContext = CreateContext("""{"item":"book"}""");
        Stream originalResponse = httpContext.Response.Body;
        var middleware = CreateMiddleware(async context =>
        {
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            handlerBody = await reader.ReadToEndAsync();
            await context.Response.WriteAsync("created");
        }, ctx => captured = ctx);

        await middleware.InvokeAsync(httpContext);

        handlerBody.Should().Be("""{"item":"book"}""");
        captured!.RequestBody.Should().Be("""{"item":"book"}""");
        captured.ResponseBody.Should().Be("created");
        httpContext.Response.Body.Should().BeSameAs(originalResponse);
        ReadAll(originalResponse).Should().Be("created");
    }

    [Fact]
    public async Task InvokeAsync_Should_NotReadRequestBody_When_ContentLengthExceedsTheLimit()
    {
        RequestResponseContext? captured = null;
        DefaultHttpContext httpContext = CreateContext("small");
        httpContext.Request.ContentLength = 10 * 1024 * 1024 + 1;
        var middleware = CreateMiddleware(context => context.Response.WriteAsync("ok"), ctx => captured = ctx);

        await middleware.InvokeAsync(httpContext);

        captured!.RequestBody.Should().Be("Request body too large: 10485761 bytes");
        httpContext.Request.Body.Position.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_Should_ReportCaptureError_When_RequestBodyCannotBeRead()
    {
        RequestResponseContext? captured = null;
        DefaultHttpContext httpContext = CreateContext();
        httpContext.Request.Body = new FailingStream("request stream broken");
        var middleware = CreateMiddleware(context => context.Response.WriteAsync("ok"), ctx => captured = ctx);

        await middleware.InvokeAsync(httpContext);

        captured!.RequestBody.Should().Be("Error capturing request body: request stream broken");
        captured.ResponseBody.Should().Be("ok");
    }

    [Fact]
    public async Task InvokeAsync_Should_ReportEmptyResponseBody_When_HandlerWritesNothing()
    {
        RequestResponseContext? captured = null;
        DefaultHttpContext httpContext = CreateContext();
        var middleware = CreateMiddleware(context =>
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        }, ctx => captured = ctx);

        await middleware.InvokeAsync(httpContext);

        captured!.RequestBody.Should().Be("Empty request body");
        captured.ResponseBody.Should().Be("Empty response body");
    }

    [Fact]
    public async Task InvokeAsync_Should_KeepTheRequest_And_RecordTheError_When_LogWriterThrows()
    {
        RequestResponseContext? captured = null;
        DefaultHttpContext httpContext = CreateContext();
        var writer = new RecordingLogWriter(new InvalidOperationException("writer down"));
        var middleware = CreateMiddleware(context => context.Response.WriteAsync("ok"), ctx => captured = ctx, writer);

        await middleware.InvokeAsync(httpContext);

        writer.Contexts.Should().ContainSingle();
        captured!.ResponseBody.Should().Be("Error writing log: writer down");
    }

    [Fact]
    public async Task InvokeAsync_Should_LogTheErrorAndRethrow_When_HandlerThrows()
    {
        DefaultHttpContext httpContext = CreateContext("body");
        Stream originalResponse = httpContext.Response.Body;
        var writer = new RecordingLogWriter();
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"), _ => { }, writer);

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        httpContext.Response.Body.Should().BeSameAs(originalResponse);
        writer.Contexts.Should().ContainSingle().Which.Should().Match<RequestResponseContext>(ctx =>
            ctx.ResponseBody == "Exception: InvalidOperationException, Message: boom" &&
            ctx.ResponseCreationTime == TimeSpan.Zero);
    }

    [Fact]
    public async Task InvokeAsync_Should_RethrowTheHandlerException_When_LoggingTheErrorAlsoFails()
    {
        DefaultHttpContext httpContext = CreateContext();
        var writer = new RecordingLogWriter(new IOException("writer down"));
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"), _ => { }, writer);

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        writer.Contexts.Should().ContainSingle();
    }

    [Fact]
    public async Task InvokeAsync_Should_RethrowTheHandlerException_When_NoLogWriterIsConfigured()
    {
        DefaultHttpContext httpContext = CreateContext();
        var middleware = new DefaultRequestResponseMiddleware(_ => throw new InvalidOperationException("boom"), new NullLogWriter(), []);

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    private static DefaultRequestResponseWithHandlerMiddleware CreateMiddleware(
        RequestDelegate next,
        Action<RequestResponseContext> onContext,
        ILogWriter? writer = null) =>
        new(next, ctx =>
        {
            onContext(ctx);
            return Task.CompletedTask;
        }, writer ?? new NullLogWriter(), []);

    private static DefaultHttpContext CreateContext(string? body = null)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = "/orders";
        byte[] bytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
        httpContext.Request.Body = new MemoryStream(bytes);
        httpContext.Request.ContentLength = bytes.Length;
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static string ReadAll(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, leaveOpen: true);
        return reader.ReadToEnd();
    }

    private sealed class RecordingLogWriter(Exception? failure = null) : ILogWriter
    {
        public List<RequestResponseContext> Contexts { get; } = [];

        public IMessageCreator? MessageCreator => null;

        public Task Write(RequestResponseContext requestResponseContext)
        {
            lock (Contexts)
                Contexts.Add(requestResponseContext);
            if (failure is not null)
                throw failure;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingStream(string message) : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new IOException(message);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            throw new IOException(message);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new IOException(message);

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
            throw new IOException(message);
    }
}
