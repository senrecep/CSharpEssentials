using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

public enum HostKind
{
    Minimal,
    ApiController,
    PlainController,
}

/// <summary>
/// Shared inputs for every host, so Minimal API and MVC endpoints return the exact same errors.
/// </summary>
internal static class ProblemScenarios
{
    public const string TraceId = "0af7651916cd43dd8448eb211c80319c";
    public const string ParentSpanId = "b7ad6b7169203331";
    public const string Traceparent = "00-" + TraceId + "-" + ParentSpanId + "-01";
    public const string UserName = "alice";

    public static Error[] Get(string? scenario) => scenario switch
    {
        "validation" =>
        [
            Error.Validation("name.required", "Name is required"),
            Error.Validation("name.required", "Name is too short"),
            Error.Validation("email.invalid", "Email is invalid"),
        ],
        "notfound" => [Error.NotFound("user.notFound", "User not found")],
        "unauthorized" => [Error.Failure("auth.unauthorized", "Login required")],
        "forbidden" => [Error.Failure("auth.forbidden", "Not allowed")],
        _ =>
        [
            Error.Validation("name.required", "Name is required"),
            Error.Validation("name.required", "Name is too short"),
            Error.Validation("email.invalid", "Email is invalid"),
            Error.NotFound("user.notFound", "User not found"),
            Error.Conflict("user.conflict", "User already exists", new ErrorMetadata("existingId", 42)),
        ],
    };

    public static ErrorMetadata? Extensions(bool ext) => ext ? new ErrorMetadata("tenant", "t1") : null;

    public static Exception CreateException(string? kind) => kind switch
    {
        "canceled" => new OperationCanceledException("canceled"),
        "badrequest" => new BadHttpRequestException("Request body too large", StatusCodes.Status413PayloadTooLarge),
        "domain" => new DomainException(Error.Conflict("order.conflict", "Order already shipped")),
        "validation" => new EnhancedValidationException([Error.Validation("email.invalid", "Email is invalid")]),
        "timeout" => new TimeoutException("upstream secret"),
        "notsupported" => new NotSupportedException("secret notsupported"),
        _ => new InvalidOperationException("secret"),
    };
}

[ApiController]
[Route("problem")]
public sealed class ApiProblemController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromQuery] string? s, [FromQuery] bool ext = false, [FromQuery] int? status = null) =>
        ProblemScenarios.Get(s).ToActionResult(HttpContext, ProblemScenarios.Extensions(ext), status);

    [HttpGet("/throw")]
    public IActionResult Throw([FromQuery] string? e) => throw ProblemScenarios.CreateException(e);
}

[Route("problem")]
public sealed class PlainProblemController : Controller
{
    [HttpGet]
    public IActionResult Get([FromQuery] string? s, [FromQuery] bool ext = false, [FromQuery] int? status = null) =>
        ProblemScenarios.Get(s).ToActionResult(HttpContext, ProblemScenarios.Extensions(ext), status);

    [HttpGet("/throw")]
    public IActionResult Throw([FromQuery] string? e) => throw ProblemScenarios.CreateException(e);
}

/// <summary>
/// Only exposes the given controllers, so other types in the test assembly never become endpoints.
/// </summary>
internal sealed class OnlyControllersFeatureProvider(params Type[] controllers) : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        feature.Controllers.Clear();
        foreach (Type controller in controllers)
            feature.Controllers.Add(controller.GetTypeInfo());
    }
}

/// <summary>
/// Makes ASP.NET Core hosting create a recorded <see cref="Activity"/> for every request, so trace ids come from the
/// <c>traceparent</c> header and are deterministic.
/// </summary>
internal static class HostingActivityListener
{
    private static readonly ActivityListener Listener = Create();

    public static void Ensure() => _ = Listener;

    private static ActivityListener Create()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Microsoft.AspNetCore",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }
}

internal sealed record ProblemResponse(int StatusCode, string? MediaType, string? RawContentType, string Body)
{
    public JsonObject Json
    {
        get
        {
            try
            {
                return JsonNode.Parse(Body)!.AsObject();
            }
            catch (System.Text.Json.JsonException exception)
            {
                throw new InvalidOperationException($"Response {StatusCode} ({RawContentType}) is not a JSON object: {Body}", exception);
            }
        }
    }
}

internal sealed class ProblemTestHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private ProblemTestHost(WebApplication app, HttpClient client)
    {
        _app = app;
        Client = client;
    }

    public HttpClient Client { get; }

    public IServiceProvider Services => _app.Services;

    /// <param name="kind">Which endpoint style serves <c>/problem</c> and <c>/throw</c>.</param>
    /// <param name="configureServices">Problem details registration. Runs after the JSON configuration.</param>
    /// <param name="configurePipeline">Middleware before the endpoints, e.g. <c>UseEnhancedProblemDetails</c>.</param>
    /// <param name="mapExtra">Extra Minimal API endpoints.</param>
    /// <param name="useLibraryJson">Whether <c>ConfigureSystemTextJson()</c> is applied (the library's recommended setup).</param>
    public static async Task<ProblemTestHost> StartAsync(
        HostKind kind,
        Action<IServiceCollection>? configureServices = null,
        Action<WebApplication>? configurePipeline = null,
        Action<WebApplication>? mapExtra = null,
        bool useLibraryJson = true)
    {
        HostingActivityListener.Ensure();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.WebHost.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        if (useLibraryJson)
            builder.Services.ConfigureSystemTextJson();
        Type[] controllers = kind switch
        {
            HostKind.ApiController => [typeof(ApiProblemController)],
            HostKind.PlainController => [typeof(PlainProblemController)],
            _ => [],
        };
        builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
        {
            foreach (IApplicationFeatureProvider<ControllerFeature> provider in
                     manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().ToList())
                manager.FeatureProviders.Remove(provider);
            manager.FeatureProviders.Add(new OnlyControllersFeatureProvider(controllers));
        });
        configureServices?.Invoke(builder.Services);

        WebApplication app = builder.Build();
        configurePipeline?.Invoke(app);
        app.Use((context, next) =>
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, ProblemScenarios.UserName)], authenticationType: "Test"));
            return next(context);
        });

        if (kind == HostKind.Minimal)
        {
            app.MapGet("/problem", (string? s, bool? ext, int? status) =>
                ProblemScenarios.Get(s).ToProblemResult(ProblemScenarios.Extensions(ext ?? false), status));
            app.MapGet("/throw", IResult (string? e) => throw ProblemScenarios.CreateException(e));
            app.MapGet("/result", (string? s) => Result.Failure(ProblemScenarios.Get(s)))
                .AddEndpointFilter<ResultEndpointFilter>();
            app.MapGet("/result-of-t", (string? s) => Result<int>.Failure(ProblemScenarios.Get(s)))
                .AddEndpointFilter<ResultEndpointFilter>();
        }
        else
        {
            app.MapControllers();
        }

        mapExtra?.Invoke(app);

        await app.StartAsync();
        return new ProblemTestHost(app, app.GetTestClient());
    }

    public async Task<ProblemResponse> SendAsync(string path, HttpMethod? method = null, string? accept = "application/json")
    {
        using var request = new HttpRequestMessage(method ?? HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("traceparent", ProblemScenarios.Traceparent);
        if (accept is not null)
            request.Headers.TryAddWithoutValidation("Accept", accept);
        using HttpResponseMessage response = await Client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();
        return new ProblemResponse(
            (int)response.StatusCode,
            response.Content.Headers.ContentType?.MediaType,
            response.Content.Headers.ContentType?.ToString(),
            body);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}

internal static partial class ProblemJson
{
    [GeneratedRegex("^00-(?<trace>[0-9a-f]{32})-(?<span>[0-9a-f]{16})-0[0-9a-f]$")]
    private static partial Regex TraceparentRegex();

    [GeneratedRegex("^[0-9a-f]{16}$")]
    private static partial Regex SpanIdRegex();

    public static bool IsTraceparent(string value, out string traceId)
    {
        Match match = TraceparentRegex().Match(value);
        traceId = match.Success ? match.Groups["trace"].Value : string.Empty;
        return match.Success;
    }

    /// <summary>
    /// Replaces the values that legitimately differ per request (this request's span id, the request id)
    /// after checking their format. Everything else must match exactly.
    /// </summary>
    public static JsonObject Normalize(JsonObject source)
    {
        var json = source.DeepClone().AsObject();

        if (json["traceId"] is JsonValue traceValue)
        {
            string trace = traceValue.GetValue<string>();
            if (IsTraceparent(trace, out string traceId))
            {
                traceId.Should().Be(ProblemScenarios.TraceId);
                json["traceId"] = $"00-{traceId}-<span>-01";
            }
        }

        if (json["spanId"] is JsonValue spanValue)
        {
            SpanIdRegex().IsMatch(spanValue.GetValue<string>()).Should().BeTrue();
            json["spanId"] = "<span>";
        }

        if (json["requestId"] is JsonValue requestValue)
        {
            requestValue.GetValue<string>().Should().NotBeNullOrWhiteSpace();
            json["requestId"] = "<requestId>";
        }

        return json;
    }

    /// <summary>
    /// Normalized JSON with object keys sorted, because member order carries no meaning in a problem document.
    /// </summary>
    public static string Canonical(JsonObject json) => Sort(Normalize(json))!.ToJsonString();

    private static JsonNode? Sort(JsonNode? node) => node switch
    {
        JsonObject obj => new JsonObject(obj
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => KeyValuePair.Create(pair.Key, Sort(pair.Value)))),
        JsonArray array => new JsonArray(array.Select(Sort).ToArray()),
        JsonNode other => other.DeepClone(),
        _ => null,
    };
}
