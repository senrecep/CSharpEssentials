using System.IO.Pipelines;
using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.RequestResponseLogging.Infrastructure.Interfaces;
using CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;
using CSharpEssentials.RequestResponseLogging.LogWriters;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public sealed class MiddlewarePipelineTests : IAsyncLifetime
{
    private IHost? _host;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    private static IHostBuilder CreateHostBuilder(Action<RequestResponseOptions> configureOptions, RequestDelegate? appHandler = null) =>
        new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services => services.AddRouting());
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.AddRequestResponseLogging(configureOptions);
                    app.Run(appHandler ?? (ctx => ctx.Response.WriteAsync("OK")));
                });
            });

    [Fact]
    public async Task InvokeAsync_Should_PassThrough_When_RequestIsNormal()
    {
        _host = await CreateHostBuilder(_ => { }).StartAsync();
        var client = _host.GetTestClient();

        var response = await client.GetAsync("/test");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("OK");
    }

    [Fact]
    public async Task InvokeAsync_Should_SkipMiddleware_When_PathIsIgnored()
    {
        var handlerInvoked = false;

        _host = await CreateHostBuilder(
            opts => opts.IgnorePaths("/health"),
            ctx =>
            {
                handlerInvoked = true;
                return ctx.Response.WriteAsync("OK");
            }).StartAsync();

        var client = _host.GetTestClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        handlerInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Should_LogRequest_When_LoggerFactoryConfigured()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

        _host = await CreateHostBuilder(opts =>
            opts.UseLogger(loggerFactory, loggingOpts =>
            {
                loggingOpts.LoggingFields = [LogFields.Request, LogFields.Response, LogFields.Path];
            })).StartAsync();

        var client = _host.GetTestClient();

        var response = await client.GetAsync("/api/values");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvokeAsync_Should_InvokeHandler_When_HandlerConfigured()
    {
        RequestResponseContext? capturedContext = null;

        _host = await CreateHostBuilder(opts =>
            opts.UseHandler(ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            })).StartAsync();

        var client = _host.GetTestClient();

        var response = await client.GetAsync("/api/data");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        capturedContext.Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_Should_CaptureResponseBody_When_HandlerConfigured()
    {
        RequestResponseContext? capturedContext = null;

        _host = await CreateHostBuilder(
            opts => opts.UseHandler(ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            }),
            ctx => ctx.Response.WriteAsync("hello world")).StartAsync();

        var client = _host.GetTestClient();
        await client.GetAsync("/test");

        capturedContext.Should().NotBeNull();
        capturedContext.ResponseBody.Should().Be("hello world");
    }

    [Fact]
    public async Task InvokeAsync_Should_IgnorePath_When_PathStartsWithIgnoredPrefix()
    {
        RequestResponseContext? capturedContext = null;

        _host = await CreateHostBuilder(opts =>
        {
            opts.IgnorePaths("/swagger");
            opts.UseHandler(ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            });
        }).StartAsync();

        var client = _host.GetTestClient();
        await client.GetAsync("/swagger/index.html");

        capturedContext.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_Should_ProcessPath_When_PathDoesNotMatchIgnored()
    {
        RequestResponseContext? capturedContext = null;

        _host = await CreateHostBuilder(opts =>
        {
            opts.IgnorePaths("/swagger");
            opts.UseHandler(ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            });
        }).StartAsync();

        var client = _host.GetTestClient();
        await client.GetAsync("/api/test");

        capturedContext.Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_Should_IgnoreOnlyRootPath_When_RootIsIgnored()
    {
        var loggedPaths = new List<string>();

        _host = await CreateHostBuilder(opts =>
        {
            opts.IgnorePaths("/");
            opts.UseHandler(ctx =>
            {
                loggedPaths.Add(ctx.Context.Request.Path.Value!);
                return Task.CompletedTask;
            });
        }).StartAsync();

        var client = _host.GetTestClient();
        await client.GetAsync("/");
        await client.GetAsync("/api/test");

        loggedPaths.Should().Equal("/api/test");
    }

    [Fact]
    public async Task InvokeAsync_Should_SkipNullAndEmptyEntries_When_IgnoredPathsContainThem()
    {
        var loggedPaths = new List<string>();

        _host = await CreateHostBuilder(opts =>
        {
            opts.IgnorePaths(null!, "", "/health");
            opts.UseHandler(ctx =>
            {
                loggedPaths.Add(ctx.Context.Request.Path.Value!);
                return Task.CompletedTask;
            });
        }).StartAsync();

        var client = _host.GetTestClient();
        await client.GetAsync("/health");
        await client.GetAsync("/api/test");

        loggedPaths.Should().Equal("/api/test");
    }

    [Fact]
    public async Task InvokeAsync_Should_SkipRequestLogging_When_SkipRequestResponseLoggingAttributePresent()
    {
        RequestResponseContext? capturedContext = null;

        _host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services => services.AddRouting());
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.AddRequestResponseLogging(opts =>
                        opts.UseHandler(ctx =>
                        {
                            capturedContext = ctx;
                            return Task.CompletedTask;
                        }));
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/skip-all", [SkipRequestResponseLogging] () => "skipped")
                                 .WithMetadata(new SkipRequestResponseLoggingAttribute());
                    });
                });
            })
            .Build();

        await _host.StartAsync();
        var client = _host.GetTestClient();
        await client.GetAsync("/skip-all");

        capturedContext.Should().NotBeNull();
        capturedContext.RequestBody.Should().Contain("Skipped");
        capturedContext.ResponseBody.Should().Contain("Skipped");
    }

    [Fact]
    public async Task InvokeAsync_Should_SkipResponseLogging_When_SkipResponseLoggingAttributePresent()
    {
        RequestResponseContext? capturedContext = null;

        _host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services => services.AddRouting());
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.AddRequestResponseLogging(opts =>
                        opts.UseHandler(ctx =>
                        {
                            capturedContext = ctx;
                            return Task.CompletedTask;
                        }));
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/skip-response", () => "response body")
                                 .WithMetadata(new SkipResponseLoggingAttribute());
                    });
                });
            })
            .Build();

        await _host.StartAsync();
        var client = _host.GetTestClient();
        await client.GetAsync("/skip-response");

        capturedContext.Should().NotBeNull();
        capturedContext.ResponseBody.Should().Contain("Skipped");
    }

    [Fact]
    public async Task InvokeAsync_Should_UseNullLogWriter_When_NoLoggerFactoryConfigured()
    {
        _host = await CreateHostBuilder(_ => { }).StartAsync();
        var client = _host.GetTestClient();

        Func<Task> act = () => client.GetAsync("/test");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvokeAsync_Should_RewindRequestBody_When_BodyReadThrowsPartway()
    {
        long? positionSeenByEndpoint = null;
        RequestResponseContext? capturedContext = null;
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            ctx =>
            {
                positionSeenByEndpoint = ctx.Request.Body.Position;
                return Task.CompletedTask;
            },
            ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            },
            new NullLogWriter(),
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Body = new ThrowOnceStream(new byte[10_000], throwAtPosition: 4096);

        await middleware.InvokeAsync(httpContext);

        capturedContext!.RequestBody.Should().StartWith("Error capturing request body");
        positionSeenByEndpoint.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_Should_ReportCaptureError_When_RewindAfterFailedReadThrows()
    {
        RequestResponseContext? capturedContext = null;
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            _ => Task.CompletedTask,
            ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            },
            new NullLogWriter(),
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Body = new DisposedOnFailureStream(new byte[100]);

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().NotThrowAsync();
        capturedContext!.RequestBody.Should().StartWith("Error capturing request body");
    }

    [Fact]
    public async Task InvokeAsync_Should_StopCapturing_When_ChunkedBodyExceedsLimit()
    {
        const int bodySize = 11 * 1024 * 1024;
        long bytesSeenByEndpoint = 0;
        RequestResponseContext? capturedContext = null;

        _host = await CreateHostBuilder(
            opts => opts.UseHandler(ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            }),
            async ctx =>
            {
                byte[] chunk = new byte[81920];
                int read;
                while ((read = await ctx.Request.Body.ReadAsync(chunk)) > 0)
                {
                    bytesSeenByEndpoint += read;
                }
            }).StartAsync();
        var client = _host.GetTestClient();

        var pipe = new Pipe();
        Task writer = Task.Run(async () =>
        {
            byte[] chunk = new byte[1024 * 1024];
            Array.Fill(chunk, (byte)'a');
            for (int i = 0; i < bodySize / chunk.Length; i++)
            {
                await pipe.Writer.WriteAsync(chunk);
            }

            await pipe.Writer.CompleteAsync();
        });
        using var content = new StreamContent(pipe.Reader.AsStream());

        await client.PostAsync("/upload", content);
        await writer;

        content.Headers.ContentLength.Should().BeNull();
        capturedContext!.RequestBody.Should().StartWith("Request body too large");
        bytesSeenByEndpoint.Should().Be(bodySize);
    }

    [Fact]
    public async Task InvokeAsync_Should_ReportWriteError_When_LogWriterFailsAsynchronously()
    {
        RequestResponseContext? capturedContext = null;
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            _ => Task.CompletedTask,
            ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            },
            new AsyncThrowingLogWriter(),
            []);

        await middleware.InvokeAsync(new DefaultHttpContext());

        capturedContext!.ResponseBody.Should().Be("Error writing log: writer failed");
    }

    [Fact]
    public async Task InvokeAsync_Should_InvokeHandlerWithRequestBody_When_PipelineThrows()
    {
        RequestResponseContext? capturedContext = null;
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            _ => throw new InvalidOperationException("endpoint failed"),
            ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            },
            new NullLogWriter(),
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Body = new MemoryStream("payload"u8.ToArray());

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("endpoint failed");
        capturedContext.Should().NotBeNull();
        capturedContext!.RequestBody.Should().Be("payload");
        capturedContext.ResponseBody.Should().Be("Exception: InvalidOperationException, Message: endpoint failed");
    }

    [Fact]
    public async Task InvokeAsync_Should_WriteLogWithRequestBody_When_PipelineThrows()
    {
        var writer = new CapturingLogWriter();
        var middleware = new DefaultRequestResponseMiddleware(
            _ => throw new InvalidOperationException("endpoint failed"),
            writer,
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Body = new MemoryStream("payload"u8.ToArray());

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<InvalidOperationException>();
        RequestResponseContext logged = writer.Contexts.Should().ContainSingle().Subject;
        logged.RequestBody.Should().Be("payload");
        logged.ResponseBody.Should().Be("Exception: InvalidOperationException, Message: endpoint failed");
    }

    [Fact]
    public async Task InvokeAsync_Should_LogHandlerFailureAndRethrowOriginal_When_PipelineAndHandlerThrow()
    {
        var writer = new CapturingLogWriter();
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            _ => throw new InvalidOperationException("endpoint failed"),
            _ => throw new ArgumentException("handler failed"),
            writer,
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Body = new MemoryStream("payload"u8.ToArray());

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("endpoint failed");
        writer.Contexts.Select(c => c.ResponseBody).Should().Equal(
            "Exception: InvalidOperationException, Message: endpoint failed",
            "Error writing log: handler failed");
        writer.Contexts[1].RequestBody.Should().Be("payload");
    }

    [Fact]
    public async Task InvokeAsync_Should_LogCapturedBodyAndRethrow_When_CopyToClientFails()
    {
        var writer = new CapturingLogWriter();
        var handled = new List<string?>();
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            ctx => ctx.Response.WriteAsync("hello"),
            ctx =>
            {
                handled.Add(ctx.ResponseBody);
                return Task.CompletedTask;
            },
            writer,
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new ThrowingWriteStream();

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<IOException>().WithMessage("client gone");
        string[] expected = ["hello", "Exception: IOException, Message: client gone"];
        writer.Contexts.Select(c => c.ResponseBody).Should().Equal(expected);
        handled.Should().Equal(expected);
        httpContext.Response.Body.Should().BeOfType<ThrowingWriteStream>();
    }

    [Fact]
    public async Task InvokeAsync_Should_LogCapturedBodyAndRethrow_When_CopyFailsWithoutHandler()
    {
        var writer = new CapturingLogWriter();
        var middleware = new DefaultRequestResponseMiddleware(
            ctx => ctx.Response.WriteAsync("hello"),
            writer,
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new ThrowingWriteStream();

        Func<Task> act = () => middleware.InvokeAsync(httpContext);

        await act.Should().ThrowAsync<IOException>().WithMessage("client gone");
        writer.Contexts.Select(c => c.ResponseBody).Should().Equal(
            "hello",
            "Exception: IOException, Message: client gone");
    }

    [Fact]
    public async Task InvokeAsync_Should_LeaveBodiesUntouched_When_NoBodyFieldIsLogged()
    {
        var logger = new RecordingLogger(enabled: true);
        var writer = new LoggerFactoryLogWriter(logger.CreateFactory(), new LoggingOptions { LoggingFields = [LogFields.Path] });
        var requestBody = new MemoryStream("payload"u8.ToArray());
        var responseBody = new MemoryStream();
        Stream? requestSeen = null;
        Stream? responseSeen = null;
        var middleware = new DefaultRequestResponseMiddleware(
            ctx =>
            {
                requestSeen = ctx.Request.Body;
                responseSeen = ctx.Response.Body;
                return Task.CompletedTask;
            },
            writer,
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/orders";
        httpContext.Request.Body = requestBody;
        httpContext.Response.Body = responseBody;

        await middleware.InvokeAsync(httpContext);

        requestSeen.Should().BeSameAs(requestBody);
        requestBody.Position.Should().Be(0);
        responseSeen.Should().BeSameAs(responseBody);
        logger.Entries.Should().ContainSingle().Which.Message.Should().Be($"Path: /orders{Environment.NewLine}");
    }

    [Fact]
    public async Task InvokeAsync_Should_CaptureBodies_When_OnlyLengthFieldsAreLogged()
    {
        var logger = new RecordingLogger(enabled: true);
        var writer = new LoggerFactoryLogWriter(logger.CreateFactory(), new LoggingOptions { LoggingFields = [LogFields.RequestLength, LogFields.ResponseLength] });
        var middleware = new DefaultRequestResponseMiddleware(ctx => ctx.Response.WriteAsync("hello"), writer, []);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Body = new MemoryStream("payload"u8.ToArray());
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext);

        string nl = Environment.NewLine;
        logger.Entries.Should().ContainSingle().Which.Message.Should().Be($"RequestLength: 7{nl}ResponseLength: 5{nl}");
    }

    [Fact]
    public async Task InvokeAsync_Should_KeepRequestBodyInMemory_When_ContentLengthIsLargerThanDefaultBufferThreshold()
    {
        string payload = new('a', 64 * 1024);
        bool? bufferedInMemory = null;
        string? bodySeenByEndpoint = null;
        RequestResponseContext? capturedContext = null;
        var middleware = new DefaultRequestResponseWithHandlerMiddleware(
            async ctx =>
            {
                bufferedInMemory = ((FileBufferingReadStream)ctx.Request.Body).InMemory;
                using var reader = new StreamReader(ctx.Request.Body);
                bodySeenByEndpoint = await reader.ReadToEndAsync();
            },
            ctx =>
            {
                capturedContext = ctx;
                return Task.CompletedTask;
            },
            new NullLogWriter(),
            []);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Body = new NonSeekableStream(System.Text.Encoding.UTF8.GetBytes(payload));
        httpContext.Request.ContentLength = payload.Length;

        await middleware.InvokeAsync(httpContext);

        bufferedInMemory.Should().BeTrue();
        bodySeenByEndpoint.Should().Be(payload);
        capturedContext!.RequestBody.Should().Be(payload);
    }

    private sealed class ThrowingWriteStream : MemoryStream
    {
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
            throw new IOException("client gone");
    }

    private sealed class CapturingLogWriter : ILogWriter
    {
        public IMessageCreator? MessageCreator => null;

        public List<RequestResponseContext> Contexts { get; } = [];

        public Task Write(RequestResponseContext requestResponseContext)
        {
            Contexts.Add(requestResponseContext);
            return Task.CompletedTask;
        }
    }

    private sealed class AsyncThrowingLogWriter : ILogWriter
    {
        public IMessageCreator? MessageCreator => null;

        public async Task Write(RequestResponseContext requestResponseContext)
        {
            await Task.Yield();
            throw new InvalidOperationException("writer failed");
        }
    }

    private sealed class NonSeekableStream(byte[] content) : MemoryStream(content)
    {
        public override bool CanSeek => false;
    }

    private sealed class ThrowOnceStream(byte[] content, long throwAtPosition) : MemoryStream(content)
    {
        private bool _thrown;

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!_thrown && Position >= throwAtPosition)
            {
                _thrown = true;
                throw new IOException("connection reset");
            }

            return base.ReadAsync(buffer, cancellationToken);
        }
    }

    private sealed class DisposedOnFailureStream(byte[] content) : MemoryStream(content)
    {
        private bool _failed;

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _failed = true;
            throw new IOException("connection reset");
        }

        public override long Seek(long offset, SeekOrigin loc) =>
            _failed ? throw new ObjectDisposedException(nameof(DisposedOnFailureStream)) : base.Seek(offset, loc);
    }
}
