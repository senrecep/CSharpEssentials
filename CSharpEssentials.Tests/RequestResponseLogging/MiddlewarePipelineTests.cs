using CSharpEssentials.RequestResponseLogging;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
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
        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

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
        capturedContext!.ResponseBody.Should().Be("hello world");
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
        capturedContext!.RequestBody.Should().Contain("Skipped");
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
        capturedContext!.ResponseBody.Should().Contain("Skipped");
    }

    [Fact]
    public async Task InvokeAsync_Should_UseNullLogWriter_When_NoLoggerFactoryConfigured()
    {
        _host = await CreateHostBuilder(_ => { }).StartAsync();
        var client = _host.GetTestClient();

        Func<Task> act = () => client.GetAsync("/test");

        await act.Should().NotThrowAsync();
    }
}
