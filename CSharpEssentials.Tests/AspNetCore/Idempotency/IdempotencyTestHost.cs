using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
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
/// Test server with <c>UseIdempotency</c>. The <c>X-User</c> header becomes the authenticated user and
/// <c>X-Client</c> a client-credentials principal (a <c>client_id</c> claim only).
/// </summary>
internal sealed class IdempotencyTestHost : IAsyncDisposable
{
    public const string UserHeader = "X-User";

    public const string ClientHeader = "X-Client";

    private readonly WebApplication _app;

    private IdempotencyTestHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public IdempotencyProbe Probe => _app.Services.GetRequiredService<IdempotencyProbe>();

    public FakeTimeProvider Time => (FakeTimeProvider)_app.Services.GetRequiredService<TimeProvider>();

    public CapturingLoggerProvider Logs => _app.Services.GetRequiredService<CapturingLoggerProvider>();

    /// <param name="configure">Configures <see cref="IdempotencyOptions"/>.</param>
    /// <param name="configureServices">Adds services.</param>
    /// <param name="configurePipeline">Adds middleware after routing and before <c>UseIdempotency</c>.</param>
    /// <param name="configureInnerPipeline">Adds middleware after <c>UseIdempotency</c>, before the endpoint.</param>
    /// <param name="configureEndpoints">Maps extra endpoints.</param>
    public static async Task<IdempotencyTestHost> StartAsync(
        Action<IdempotencyOptions>? configure = null,
        Action<IServiceCollection>? configureServices = null,
        Action<WebApplication>? configurePipeline = null,
        Action<WebApplication>? configureInnerPipeline = null,
        Action<WebApplication>? configureEndpoints = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Services.AddSingleton<CapturingLoggerProvider>();
        builder.Services.AddSingleton<ILoggerProvider>(provider => provider.GetRequiredService<CapturingLoggerProvider>());
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
            manager.FeatureProviders.Add(new OnlyControllersFeatureProvider(typeof(IdempotentOrdersController), typeof(PlainOrdersController)));
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
            finally
            {
                if (context.Request.Headers[IdempotencyProbe.TraceHeader].ToString() is { Length: > 0 } trace)
                    context.RequestServices.GetRequiredService<IdempotencyProbe>().Finish(trace);
            }
        });
        app.UseRouting();
        app.Use((context, next) =>
        {
            string? user = context.Request.Headers[UserHeader];
            string? client = context.Request.Headers[ClientHeader];
            if (!string.IsNullOrEmpty(user))
                context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user)], "Test"));
            else if (!string.IsNullOrEmpty(client))
                context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(IdempotencyScopes.ClientIdClaimType, client)], "Test"));
            return next(context);
        });
        configurePipeline?.Invoke(app);
        app.UseIdempotency();
        app.Use(async (context, next) =>
        {
            if (context.Request.Headers[IdempotencyOutcomes.StageHeader] == "middleware")
            {
                context.RequestServices.GetRequiredService<IdempotencyProbe>().Execute();
                await IdempotencyOutcomes.RunAsync(context, context.Request.Query["outcome"].ToString());
                return;
            }

            await next(context);
        });
        configureInnerPipeline?.Invoke(app);

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
        app.MapPost("/write-then-throw", async (HttpContext context, IdempotencyProbe probe) =>
        {
            probe.Execute();
            await context.Response.WriteAsync("partial");
            await context.Response.Body.FlushAsync();
            throw new InvalidOperationException("connection lost while writing");
        }).WithIdempotency();
        app.MapPost("/matrix", async (HttpContext context, string outcome, IdempotencyProbe probe) =>
        {
            probe.Execute();
            await IdempotencyOutcomes.RunAsync(context, outcome);
        }).WithIdempotency();
        app.MapPost("/abort-ignored", (HttpContext context, IdempotencyProbe probe) =>
        {
            int execution = probe.Execute();
            context.Abort();
            context.Response.StatusCode = StatusCodes.Status201Created;
            context.Response.Headers.Location = $"/orders/{execution}";
        }).WithIdempotency();
        app.MapPost("/timeout", async (HttpContext context, IdempotencyProbe probe) =>
        {
            probe.Execute();
            await Task.Delay(Timeout.Infinite, context.RequestAborted);
        }).WithIdempotency().WithRequestTimeout(TimeSpan.FromMilliseconds(100));
        app.MapPost("/not-storable", async (HttpContext context, string kind, int status, IdempotencyProbe probe) =>
        {
            probe.Execute();
            context.Response.StatusCode = status;
            switch (kind)
            {
                case "encoded":
                    context.Response.Headers.ContentEncoding = "gzip";
                    await context.Response.Body.WriteAsync("not really gzip"u8.ToArray());
                    break;
                case "event-stream":
                    context.Response.ContentType = "text/event-stream";
                    await context.Response.WriteAsync("data: 1\n\n");
                    break;
                default:
                    await context.Response.WriteAsync(new string('x', 4096));
                    break;
            }
        }).WithIdempotency();
        app.MapPost("/start-first", async (HttpContext context, IdempotencyProbe probe) =>
        {
            int execution = probe.Execute();
            context.Response.StatusCode = StatusCodes.Status201Created;
            context.Response.ContentType = "application/json";
            await context.Response.StartAsync();
            await context.Response.WriteAsync($$"""{"execution":{{execution}}}""");
        }).WithIdempotency();
        app.MapPost("/short-circuit", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
            .AddEndpointFilter((context, _) =>
            {
                context.HttpContext.RequestServices.GetRequiredService<IdempotencyProbe>().Execute();
                return ValueTask.FromResult<object?>(HttpResults.StatusCode(StatusCodes.Status500InternalServerError));
            })
            .WithIdempotency();
        app.MapPost("/inner-short-circuit", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
            .WithIdempotency()
            .AddEndpointFilter((context, _) =>
            {
                context.HttpContext.RequestServices.GetRequiredService<IdempotencyProbe>().Execute();
                return ValueTask.FromResult<object?>(HttpResults.StatusCode(StatusCodes.Status500InternalServerError));
            });
        app.MapPost("/no-filter", async (HttpContext context, string outcome, IdempotencyProbe probe) =>
        {
            probe.Execute();
            await IdempotencyOutcomes.RunAsync(context, outcome);
        }).WithMetadata(new IdempotentAttribute());
        app.MapPost("/raw", (RequestDelegate)(context =>
        {
            context.RequestServices.GetRequiredService<IdempotencyProbe>().Execute();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Task.CompletedTask;
        })).WithIdempotency();
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
        app.MapPost("/conflict", (IdempotencyProbe probe) =>
        {
            probe.Execute();
            return HttpResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "Order already shipped");
        }).WithIdempotency();
        app.MapPost("/plain", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }));
        RouteGroupBuilder group = app.MapGroup("/group").WithIdempotency();
        group.MapPost("/items", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }));
        group.MapPost("/fail", (int status, IdempotencyProbe probe) =>
        {
            probe.Execute();
            return HttpResults.StatusCode(status);
        });
        configureEndpoints?.Invoke(app);
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

    /// <summary>
    /// Sends a request and waits until the server has finished it, so the next request sees its settled key.
    /// Returns <see langword="null"/> when the request was aborted.
    /// </summary>
    public async Task<IdempotencyResponse?> SendSettledAsync(HttpRequestMessage request)
    {
        string trace = Guid.NewGuid().ToString("N");
        request.Headers.TryAddWithoutValidation(IdempotencyProbe.TraceHeader, trace);
        IdempotencyResponse? result;
        try
        {
            using HttpResponseMessage response = await Client.SendAsync(request);
            result = await IdempotencyResponse.ReadAsync(response);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or OperationCanceledException)
        {
            result = null;
        }

        await Probe.Finished(trace);
        return result;
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
