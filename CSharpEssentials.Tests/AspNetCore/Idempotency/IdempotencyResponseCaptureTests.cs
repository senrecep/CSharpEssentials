using System.Text;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotencyResponseCaptureTests
{
    [Fact]
    public async Task Synchronous_Writes_And_Flush_Should_Be_Captured_And_Replayed()
    {
        await using IdempotencyTestHost host = await StartAsync();

        IdempotencyResponse? first = await SettleAsync(host, "/sync-write");
        IdempotencyResponse second = await host.PostAsync("/sync-write");

        first!.StatusCode.Should().Be(StatusCodes.Status201Created);
        first.Body.Should().Be("sync-1|tail");
        second.Replayed.Should().BeTrue();
        second.Body.Should().Be(first.Body);
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Array_WriteAsync_Should_Be_Captured_And_Replayed()
    {
        await using IdempotencyTestHost host = await StartAsync();

        IdempotencyResponse? first = await SettleAsync(host, "/array-write");
        IdempotencyResponse second = await host.PostAsync("/array-write");

        first!.Body.Should().Be("array-1");
        second.Replayed.Should().BeTrue();
        second.Body.Should().Be("array-1");
    }

    [Fact]
    public async Task Response_Body_Should_Be_A_Write_Only_Stream_While_Captured()
    {
        await using IdempotencyTestHost host = await StartAsync();

        IdempotencyResponse response = await host.PostAsync("/stream-shape");

        response.Body.Should().Be("read:False|seek:False|write:True|unsupported:5");
    }

    [Fact]
    public async Task Writes_After_The_Body_Exceeds_The_Limit_Should_Reach_The_Client_But_Not_Be_Stored()
    {
        await using IdempotencyTestHost host = await StartAsync(maxResponseBodySize: 8);

        IdempotencyResponse? first = await SettleAsync(host, "/two-writes");
        IdempotencyResponse second = await host.PostAsync("/two-writes");

        first!.Body.Should().Be("0123456789abc");
        second.Replayed.Should().BeFalse();
        second.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        host.Logs.Named("Idempotency.KeptInFlight").Should().ContainSingle()
            .Which.Message.Should().Contain("MaxResponseBodySize");
    }

    [Theory]
    [InlineData("identity")]
    [InlineData("IDENTITY, identity")]
    [InlineData("")]
    public async Task Identity_Or_Empty_Content_Encoding_Should_Still_Be_Stored(string encoding)
    {
        await using IdempotencyTestHost host = await StartAsync();
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest($"/encoding?value={Uri.EscapeDataString(encoding)}");

        IdempotencyResponse? first = await host.SendSettledAsync(request);
        IdempotencyResponse second = await host.PostAsync($"/encoding?value={Uri.EscapeDataString(encoding)}");

        first!.Body.Should().Be("plain-1");
        second.Replayed.Should().BeTrue();
        second.Body.Should().Be("plain-1");
    }

    [Fact]
    public async Task A_Real_Coding_After_An_Identity_Entry_Should_Not_Be_Stored()
    {
        await using IdempotencyTestHost host = await StartAsync();

        IdempotencyResponse? first = await SettleAsync(host, "/encoding?value=identity,%20gzip");
        IdempotencyResponse second = await host.PostAsync("/encoding?value=identity,%20gzip");

        first!.StatusCode.Should().Be(StatusCodes.Status201Created);
        second.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        host.Logs.Named("Idempotency.KeptInFlight").Should().ContainSingle()
            .Which.Message.Should().Contain("Content-Encoding");
    }

    private static Task<IdempotencyResponse?> SettleAsync(IdempotencyTestHost host, string path)
    {
        HttpRequestMessage request = IdempotencyTestHost.CreateRequest(path);
        return SendAndDisposeAsync(host, request);
    }

    private static async Task<IdempotencyResponse?> SendAndDisposeAsync(IdempotencyTestHost host, HttpRequestMessage request)
    {
        using (request)
            return await host.SendSettledAsync(request);
    }

    private static Task<IdempotencyTestHost> StartAsync(long? maxResponseBodySize = null) =>
        IdempotencyTestHost.StartAsync(
            options =>
            {
                if (maxResponseBodySize is { } limit)
                    options.MaxResponseBodySize = limit;
            },
            configureEndpoints: app =>
            {
                app.MapPost("/sync-write", (HttpContext context, IdempotencyProbe probe) =>
                {
                    AllowSynchronousIO(context);
                    int execution = probe.Execute();
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    byte[] head = Encoding.UTF8.GetBytes($"sync-{execution}");
                    context.Response.Body.Write(head, 0, head.Length);
                    context.Response.Body.Flush();
                    context.Response.Body.Write("|tail"u8);
                    return Task.CompletedTask;
                }).WithIdempotency();
                app.MapPost("/array-write", async (HttpContext context, IdempotencyProbe probe) =>
                {
                    byte[] bytes = Encoding.UTF8.GetBytes($"array-{probe.Execute()}");
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length, context.RequestAborted);
                }).WithIdempotency();
                app.MapPost("/stream-shape", async (HttpContext context, IdempotencyProbe probe) =>
                {
                    probe.Execute();
                    Stream body = context.Response.Body;
                    Action[] unsupported =
                    [
                        () => _ = body.Length,
                        () => _ = body.Position,
                        () => body.Position = 0,
                        () => body.Seek(0, SeekOrigin.Begin),
                        () => body.SetLength(0),
                    ];
                    int count = unsupported.Count(Throws);
                    bool readThrows = Throws(() => _ = body.Read(new byte[1], 0, 1));
                    await context.Response.WriteAsync(
                        $"read:{body.CanRead && !readThrows}|seek:{body.CanSeek}|write:{body.CanWrite}|unsupported:{count}");
                }).WithIdempotency();
                app.MapPost("/two-writes", async (HttpContext context, IdempotencyProbe probe) =>
                {
                    probe.Execute();
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    await context.Response.Body.WriteAsync("0123456789"u8.ToArray());
                    await context.Response.Body.WriteAsync("abc"u8.ToArray());
                }).WithIdempotency();
                app.MapPost("/encoding", async (HttpContext context, string value, IdempotencyProbe probe) =>
                {
                    int execution = probe.Execute();
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    context.Response.Headers.ContentEncoding = new StringValues([value, " "]);
                    await context.Response.WriteAsync($"plain-{execution}");
                }).WithIdempotency();
            });

    private static void AllowSynchronousIO(HttpContext context)
    {
        IHttpBodyControlFeature? feature = context.Features.Get<IHttpBodyControlFeature>();
        feature.Should().NotBeNull();
        feature!.AllowSynchronousIO = true;
    }

    private static bool Throws(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (NotSupportedException)
        {
            return true;
        }
    }
}
