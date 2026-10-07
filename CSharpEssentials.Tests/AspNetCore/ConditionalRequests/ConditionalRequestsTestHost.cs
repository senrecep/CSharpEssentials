using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace CSharpEssentials.Tests.AspNetCore.ConditionalRequests;

internal sealed record VersionedItem(int Id, string Version) : IVersioned;

internal record Document(int Id, long Revision, DateTimeOffset UpdatedAt);

internal sealed record DerivedDocument(int Id, long Revision, DateTimeOffset UpdatedAt) : Document(Id, Revision, UpdatedAt);

/// <summary>Versioned, but a registered source must win over <see cref="IVersioned"/>.</summary>
internal sealed record VersionedDocument(int Id, string Version) : IVersioned;

internal sealed record TenantDocument(int Id, long Revision);

internal sealed record PlainItem(string Name);

internal static class ConditionalData
{
    /// <summary>Has milliseconds, so <c>Last-Modified</c> must be truncated to the second.</summary>
    public static readonly DateTimeOffset UpdatedAt = new(2026, 1, 2, 3, 4, 5, 678, TimeSpan.Zero);

    public const string LastModified = "Fri, 02 Jan 2026 03:04:05 GMT";

    public static readonly VersionedItem Item = new(1, "v1");

    public static readonly Document Doc = new(1, 7, UpdatedAt);
}

internal sealed class DocumentETagSource : IETagSource<Document>
{
    public ResourceValidators? GetValidators(Document value) =>
        new(new EntityTagHeaderValue($"\"doc-{value.Revision}\""), value.UpdatedAt);
}

internal sealed class VersionedDocumentETagSource : IETagSource<VersionedDocument>
{
    public ResourceValidators? GetValidators(VersionedDocument value) =>
        new(new EntityTagHeaderValue($"\"custom-{value.Id}\""), null);
}

/// <summary>A scoped dependency, so the source must be resolved from the request services.</summary>
internal sealed class TenantContext
{
    public string Tenant => "t1";
}

internal sealed class TenantDocumentETagSource(TenantContext tenant) : IETagSource<TenantDocument>
{
    public ResourceValidators? GetValidators(TenantDocument value) =>
        new(new EntityTagHeaderValue($"\"{tenant.Tenant}-{value.Revision}\""), null);
}

/// <summary>Registering a source for an interface is rejected.</summary>
internal sealed class VersionedInterfaceETagSource : IETagSource<IVersioned>
{
    public ResourceValidators? GetValidators(IVersioned value) => null;
}

/// <summary>Counts how often <see cref="Items"/> is enumerated.</summary>
internal sealed class EnumerationCounter
{
    private int _count;

    public int Count => _count;

    public async IAsyncEnumerable<PlainItem> Items()
    {
        Interlocked.Increment(ref _count);
        await Task.Yield();
        yield return new PlainItem("a");
        yield return new PlainItem("b");
    }
}

/// <summary>Maps every failure to 409, so a 412 is up to the mapper.</summary>
internal sealed class ConflictErrorMapper : IResultErrorMapper
{
    public IResult Map(Error[] errors) => HttpResults.StatusCode(StatusCodes.Status409Conflict);
}

internal sealed record VersionCheck(bool Matches, string? IfMatchVersion);

internal sealed class FixedETagGenerator : IETagGenerator
{
    public ResourceValidators? GetValidators(object value) => new(new EntityTagHeaderValue("\"fixed\""), null);
}

[ApiController]
[Route("mvc")]
internal sealed class ConditionalGetController : ControllerBase
{
    [HttpGet("items/{id:int}")]
    [HttpHead("items/{id:int}")]
    [ConditionalGet]
    public VersionedItem GetItem(int id) => ConditionalData.Item with { Id = id };

    [HttpGet("docs")]
    [ConditionalGet]
    public Document GetDocument()
    {
        Response.Headers.CacheControl = "private, max-age=60";
        Response.Headers.Vary = "Accept-Language";
        Response.Headers.ContentLocation = "/docs/1";
        return ConditionalData.Doc;
    }

    [HttpGet("result")]
    [ConditionalGet]
    public Result<VersionedItem> GetResult() => ConditionalData.Item;

    [HttpGet("result-string")]
    [ConditionalGet]
    public Result<string> GetResultString() => "v1";

    [HttpGet("problem")]
    [ConditionalGet]
    public IActionResult GetProblem() => new ObjectResult(new ProblemDetails { Title = "Not a resource" });

    [HttpGet("missing")]
    [ConditionalGet]
    public IActionResult GetMissing() => NotFound(ConditionalData.Item);

    [HttpGet("off")]
    public VersionedItem GetWithoutAttribute() => ConditionalData.Item;

    [HttpPost("items")]
    [ConditionalGet]
    public VersionedItem Post() => ConditionalData.Item;
}

[ApiController]
[Route("mvc-class")]
[ConditionalGet]
internal sealed class ConditionalGetClassController : ControllerBase
{
    [HttpGet]
    public VersionedItem Get() => ConditionalData.Item;
}

[ApiController]
[Route("mvc-ifmatch")]
internal sealed class IfMatchController : ControllerBase
{
    private static readonly ResourceValidators Current = new(new EntityTagHeaderValue("\"v2\""), null);

    [HttpPut("items")]
    [HttpPost("items")]
    [HttpPatch("items")]
    [HttpDelete("items")]
    [IfMatch]
    public IActionResult Put() =>
        HttpContext.GetPreconditions().Matches(Current)
            ? Ok("updated")
            : ConditionalRequestErrors.PreconditionFailed.ToActionResult(HttpContext);

    [HttpPut("required")]
    [HttpPost("required")]
    [HttpPatch("required")]
    [HttpDelete("required")]
    [IfMatch(Required = true)]
    public IActionResult PutRequired() => Ok(HttpContext.GetPreconditions().TryGetIfMatchVersion(out string? version) ? version : "<none>");

    [HttpPut("convention")]
    public IActionResult PutByConvention() => Ok("handled");
}

internal sealed class ConditionalRequestsTestHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private ConditionalRequestsTestHost(WebApplication app, HttpClient client)
    {
        _app = app;
        Client = client;
    }

    public HttpClient Client { get; }

    public IServiceProvider Services => _app.Services;

    /// <param name="configure">Options of <c>AddConditionalRequests</c>.</param>
    /// <param name="configureServices">Runs before <c>AddConditionalRequests</c>.</param>
    /// <param name="addConditionalRequests">Whether <c>AddConditionalRequests</c> is called.</param>
    /// <param name="controllerConventions">Conventions applied to <c>MapControllers()</c>.</param>
    public static async Task<ConditionalRequestsTestHost> StartAsync(
        Action<ConditionalRequestOptions>? configure = null,
        Action<IServiceCollection>? configureServices = null,
        bool addConditionalRequests = true,
        Action<ControllerActionEndpointConventionBuilder>? controllerConventions = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.WebHost.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
        {
            foreach (IApplicationFeatureProvider<ControllerFeature> provider in
                     manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().ToList())
                manager.FeatureProviders.Remove(provider);
            manager.FeatureProviders.Add(new OnlyControllersFeatureProvider(
                typeof(ConditionalGetController), typeof(ConditionalGetClassController), typeof(IfMatchController)));
        });
        builder.Services.AddScoped<TenantContext>();
        builder.Services.AddSingleton<EnumerationCounter>();
        configureServices?.Invoke(builder.Services);
        if (addConditionalRequests)
        {
            builder.Services.AddConditionalRequests(configure);
            builder.Services.AddETagSource<Document, DocumentETagSource>();
            builder.Services.AddETagSource<VersionedDocument, VersionedDocumentETagSource>();
            builder.Services.AddETagSource<TenantDocument, TenantDocumentETagSource>();
        }

        WebApplication app = builder.Build();
        MapConditionalGet(app);
        MapIfMatch(app);
        ControllerActionEndpointConventionBuilder controllers = app.MapControllers();
        controllerConventions?.Invoke(controllers);

        await app.StartAsync();
        return new ConditionalRequestsTestHost(app, app.GetTestClient());
    }

    private static void MapConditionalGet(WebApplication app)
    {
        app.MapGet("/items/{id:int}", (int id) => ConditionalData.Item with { Id = id }).WithConditionalGet();
        app.MapMethods("/docs", [HttpMethods.Get, HttpMethods.Head], (HttpContext httpContext) =>
        {
            httpContext.Response.Headers.CacheControl = "private, max-age=60";
            httpContext.Response.Headers.Vary = "Accept-Language";
            httpContext.Response.Headers.ContentLocation = "/docs/1";
            return ConditionalData.Doc;
        }).WithConditionalGet();
        app.MapGet("/derived", () => new DerivedDocument(2, 9, ConditionalData.UpdatedAt)).WithConditionalGet();
        app.MapPost("/items", () => ConditionalData.Item).WithConditionalGet();
        app.MapGet("/typed", () => TypedResults.Ok(ConditionalData.Item)).WithConditionalGet();
        app.MapGet("/not-found", () => TypedResults.NotFound(ConditionalData.Item)).WithConditionalGet();
        app.MapGet("/string", () => "v1").WithConditionalGet();
        app.MapGet("/off", () => ConditionalData.Item);
        app.MapGet("/versioned-doc", () => new VersionedDocument(3, "ignored")).WithConditionalGet();
        app.MapGet("/tenant-doc", () => new TenantDocument(4, 5)).WithConditionalGet();
        app.MapGet("/plain/{name}", (string name) => new PlainItem(name)).WithConditionalGet();
        app.MapGet("/version", (string version) => new VersionedItem(1, version)).WithConditionalGet();
        app.MapGet("/async-items", (EnumerationCounter counter) => counter.Items()).WithConditionalGet();
        app.MapGet("/existing-etag", (HttpContext httpContext) =>
        {
            httpContext.Response.Headers.ETag = "\"handler\"";
            return ConditionalData.Item;
        }).WithConditionalGet();

        // Result<T> with ResultEndpointFilter in both orders.
        app.MapGet("/result-outer", GetResult).AddEndpointFilter<ResultEndpointFilter>().WithConditionalGet();
        app.MapGet("/result-inner", GetResult).WithConditionalGet().AddEndpointFilter<ResultEndpointFilter>();
        app.MapGet("/result-string-outer", GetResultString).AddEndpointFilter<ResultEndpointFilter>().WithConditionalGet();
        app.MapGet("/result-string-inner", GetResultString).WithConditionalGet().AddEndpointFilter<ResultEndpointFilter>();

        RouteGroupBuilder group = app.MapGroup("/group").WithConditionalGet();
        group.MapGet("/item", () => ConditionalData.Item);
        group.MapGet("/twice", () => ConditionalData.Item).WithConditionalGet();
    }

    private static Result<VersionedItem> GetResult(bool fail = false) =>
        fail ? Error.NotFound("item.notFound", "Item not found") : ConditionalData.Item;

    private static Result<string> GetResultString() => "v1";

    private static void MapIfMatch(WebApplication app)
    {
        var current = new ResourceValidators(new EntityTagHeaderValue("\"v2\""), null);

        app.MapPut("/if-match/items", Result<string> (HttpContext httpContext) =>
                httpContext.GetPreconditions().Matches(current) ? "updated" : ConditionalRequestErrors.PreconditionFailed)
            .AddEndpointFilter<ResultEndpointFilter>()
            .WithIfMatch();
        app.MapPut("/if-match/required", () => "handled").WithIfMatch(required: true);
        app.MapMethods("/if-match/methods", [HttpMethods.Post, HttpMethods.Patch, HttpMethods.Delete], Result<string> (HttpContext httpContext) =>
                httpContext.GetPreconditions().Matches(current) ? "updated" : ConditionalRequestErrors.PreconditionFailed)
            .AddEndpointFilter<ResultEndpointFilter>()
            .WithIfMatch(required: true);
        app.MapPut("/if-match/versioned", (string version, HttpContext httpContext) =>
        {
            Preconditions preconditions = httpContext.GetPreconditions();
            return new VersionCheck(
                preconditions.Matches(new VersionedItem(1, version)),
                preconditions.TryGetIfMatchVersion(out string? ifMatchVersion) ? ifMatchVersion : null);
        }).WithIfMatch();
        app.MapPut("/if-match/plain-loader", () => "handled")
            .WithIfMatch((_, _) => ValueTask.FromResult<PlainItem?>(new PlainItem("a")));
        app.MapMethods("/if-match/inspect", [HttpMethods.Get, HttpMethods.Put, HttpMethods.Delete], (HttpContext httpContext) =>
        {
            Preconditions preconditions = httpContext.GetPreconditions();
            bool hasVersion = preconditions.TryGetIfMatchVersion(out string? version);
            return new PreconditionsView(
                preconditions.HasIfMatch,
                preconditions.IsWildcard,
                preconditions.IfMatch.Count,
                hasVersion ? version : null,
                preconditions.Matches(current));
        }).WithIfMatch();
        app.MapPut("/if-match/loader/{id:int}", (int id) => $"handled {id}")
            .WithIfMatch((httpContext, _) => ValueTask.FromResult(
                httpContext.Request.RouteValues["id"] is "0" ? null : (VersionedItem?)new VersionedItem(1, "v2")));
        app.MapPut("/if-match/no-filter", (HttpContext httpContext) => httpContext.GetPreconditions().HasIfMatch);

        RouteGroupBuilder group = app.MapGroup("/if-match-group").WithIfMatch(required: true);
        group.MapPut("/required", () => "handled");
        group.MapPut("/optional", () => "handled").WithIfMatch();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, params (string Name, string Value)[] headers)
    {
        using var request = new HttpRequestMessage(method, path);
        foreach ((string name, string value) in headers)
            request.Headers.TryAddWithoutValidation(name, value);
        return await Client.SendAsync(request);
    }

    public Task<HttpResponseMessage> GetAsync(string path, params (string Name, string Value)[] headers) =>
        SendAsync(HttpMethod.Get, path, headers);

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}

internal sealed record PreconditionsView(bool HasIfMatch, bool IsWildcard, int Count, string? Version, bool Matches);

internal static class ResponseExtensions
{
    public static string? ETag(this HttpResponseMessage response) => response.Headers.ETag?.ToString();

    public static string? LastModified(this HttpResponseMessage response) =>
        response.Content.Headers.TryGetValues(HeaderNames.LastModified, out IEnumerable<string>? values)
            ? values.Single()
            : null;
}
