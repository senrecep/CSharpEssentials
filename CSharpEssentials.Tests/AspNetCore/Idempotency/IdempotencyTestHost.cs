using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// Test server with <c>UseIdempotency</c>. The <c>X-User</c> header becomes the authenticated user.
/// </summary>
internal sealed class IdempotencyTestHost : IAsyncDisposable
{
    public const string UserHeader = "X-User";

    private readonly WebApplication _app;

    private IdempotencyTestHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public IdempotencyProbe Probe => _app.Services.GetRequiredService<IdempotencyProbe>();

    public FakeTimeProvider Time => (FakeTimeProvider)_app.Services.GetRequiredService<TimeProvider>();

    public static async Task<IdempotencyTestHost> StartAsync(
        Action<IdempotencyOptions>? configure = null,
        Action<IServiceCollection>? configureServices = null,
        Action<WebApplication>? configurePipeline = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.WebHost.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        builder.Services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero)));
        builder.Services.TryAddSingleton<IdempotencyProbe>();
        builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
        {
            foreach (IApplicationFeatureProvider<ControllerFeature> provider in
                     manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().ToList())
                manager.FeatureProviders.Remove(provider);
            manager.FeatureProviders.Add(new OnlyControllersFeatureProvider(typeof(IdempotentOrdersController)));
        });
        builder.Services.AddIdempotency(configure);
        configureServices?.Invoke(builder.Services);

        WebApplication app = builder.Build();
        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (InvalidOperationException) when (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        });
        app.UseRouting();
        app.Use((context, next) =>
        {
            string? user = context.Request.Headers[UserHeader];
            if (!string.IsNullOrEmpty(user))
                context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user)], "Test"));
            return next(context);
        });
        configurePipeline?.Invoke(app);
        app.UseIdempotency();

        app.MapPost("/orders", async (HttpContext context, IdempotencyProbe probe) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            string body = await reader.ReadToEndAsync();
            int execution = probe.Execute();
            context.Response.Headers.Append("Set-Cookie", "session=secret");
            context.Response.Headers.Append("X-Not-Replayed", "1");
            return HttpResults.Created($"/orders/{execution}", new { execution, body });
        }).WithIdempotency();
        app.MapMethods("/orders", [HttpMethods.Put], (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
            .WithIdempotency();
        app.MapPost("/fail", (int status, IdempotencyProbe probe) =>
        {
            probe.Execute();
            return HttpResults.StatusCode(status);
        }).WithIdempotency();
        app.MapPost("/throw", IResult (IdempotencyProbe probe) =>
        {
            probe.Execute();
            throw new InvalidOperationException("boom");
        }).WithIdempotency();
        app.MapPost("/slow", async (IdempotencyProbe probe) =>
        {
            int execution = probe.Execute();
            probe.Started.TrySetResult();
            await probe.Gate.Task;
            return HttpResults.Ok(new { execution });
        }).WithIdempotency();
        app.MapPost("/large", async (HttpContext context, IdempotencyProbe probe) =>
        {
            probe.Execute();
            await context.Response.WriteAsync(new string('x', 4096));
        }).WithIdempotency();
        app.MapPost("/abort", async (HttpContext context, IdempotencyProbe probe) =>
        {
            probe.Execute();
            await context.Response.WriteAsync("partial");
            await context.Response.Body.FlushAsync();
            throw new InvalidOperationException("connection lost while writing");
        }).WithIdempotency();
        app.MapPost("/pipe", (HttpContext context, IdempotencyProbe probe) =>
        {
            probe.Execute();
            context.Response.ContentType = "text/plain";
            Span<byte> span = context.Response.BodyWriter.GetSpan(5);
            "piped"u8.CopyTo(span);
            context.Response.BodyWriter.Advance(5);
            return Task.CompletedTask;
        }).WithIdempotency();
        app.MapPost("/echo-length", async (HttpContext context, IdempotencyProbe probe) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            string body = await reader.ReadToEndAsync();
            return HttpResults.Ok(new { execution = probe.Execute(), length = body.Length });
        }).WithIdempotency();
        app.MapPost("/no-content", (IdempotencyProbe probe) =>
        {
            probe.Execute();
            return HttpResults.NoContent();
        }).WithIdempotency();
        app.MapPost("/plain", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }));
        RouteGroupBuilder group = app.MapGroup("/group").WithIdempotency();
        group.MapPost("/items", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }));
        app.MapControllers();

        await app.StartAsync();
        return new IdempotencyTestHost(app);
    }

    public async Task<IdempotencyResponse> PostAsync(
        string path,
        string? key = "key-1",
        string body = """{"item":"book"}""",
        string? user = "alice",
        HttpMethod? method = null)
    {
        using HttpRequestMessage request = CreateRequest(path, key, body, user, method);
        using HttpResponseMessage response = await Client.SendAsync(request);
        return await IdempotencyResponse.ReadAsync(response);
    }

    public static HttpRequestMessage CreateRequest(
        string path,
        string? key = "key-1",
        string body = """{"item":"book"}""",
        string? user = "alice",
        HttpMethod? method = null)
    {
        var request = new HttpRequestMessage(method ?? HttpMethod.Post, path)
        {
            Content = new StringContent(body, Encoding.UTF8, new MediaTypeHeaderValue("application/json")),
        };
        if (key is not null)
            request.Headers.TryAddWithoutValidation("Idempotency-Key", key);
        if (user is not null)
            request.Headers.TryAddWithoutValidation(UserHeader, user);
        return request;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
