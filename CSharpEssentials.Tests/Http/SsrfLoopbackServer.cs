using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Http;

/// <summary>
/// A real Kestrel server on 127.0.0.1 with a random port, so the SSRF guard's ConnectCallback opens real sockets.
/// </summary>
internal sealed class SsrfLoopbackServer : IAsyncDisposable
{
    private readonly WebApplication _app;

    private SsrfLoopbackServer(WebApplication app, int port)
    {
        _app = app;
        Port = port;
    }

    public int Port { get; }

    public static async Task<SsrfLoopbackServer> StartAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        WebApplication app = builder.Build();

        app.MapGet("/ok", () => "ok");
        app.MapGet("/json", () => Microsoft.AspNetCore.Http.Results.Json("ok"));
        app.Map("/redirect", (HttpContext context) =>
        {
            context.Response.StatusCode = int.TryParse(context.Request.Query["status"], out int status) ? status : 302;
            context.Response.Headers.Location = context.Request.Query["to"].ToString();
            return Task.CompletedTask;
        });
        app.MapGet("/chain/{remaining:int}", (HttpContext context, int remaining) =>
        {
            if (remaining == 0)
                return context.Response.WriteAsync("done");

            context.Response.StatusCode = 302;
            context.Response.Headers.Location = $"/chain/{remaining - 1}";
            return Task.CompletedTask;
        });
        app.MapGet("/auth", (HttpContext context) => context.Request.Headers.Authorization.ToString() is { Length: > 0 } value ? value : "none");
        app.MapGet("/headers", (HttpContext context) =>
            $"{context.Request.Headers.Authorization}|{context.Request.Headers.Cookie}|{context.Request.Headers.ProxyAuthorization}");
        app.MapGet("/version", (HttpContext context) => context.Request.Protocol);
        app.MapGet("/set-cookie", (HttpContext context) =>
        {
            context.Response.Headers.SetCookie = "session=secret; Path=/";
            context.Response.StatusCode = 302;
            context.Response.Headers.Location = "/headers";
            return Task.CompletedTask;
        });
        app.Map("/echo", async (HttpContext context) =>
        {
            string body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            await context.Response.WriteAsync($"{context.Request.Method}:{body}");
        });
        app.MapGet("/big", async (HttpContext context, int length, bool declare) =>
        {
            if (declare)
                context.Response.ContentLength = length;

            byte[] chunk = new byte[1024];
            for (int written = 0; written < length; written += chunk.Length)
            {
                await context.Response.Body.WriteAsync(chunk.AsMemory(0, Math.Min(chunk.Length, length - written)));
                await context.Response.Body.FlushAsync();
            }
        });
        app.MapGet("/slow", async (HttpContext context) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), context.RequestAborted);
            return "late";
        });
        app.MapGet("/slow-body", async (HttpContext context) =>
        {
            await context.Response.WriteAsync("start");
            await context.Response.Body.FlushAsync();
            await Task.Delay(TimeSpan.FromSeconds(10), context.RequestAborted);
            await context.Response.WriteAsync("end");
        });

        await app.StartAsync();
        string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        return new SsrfLoopbackServer(app, new Uri(address).Port);
    }

    public Uri Url(string host, string pathAndQuery) => new($"http://{host}:{Port}{pathAndQuery}");

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
