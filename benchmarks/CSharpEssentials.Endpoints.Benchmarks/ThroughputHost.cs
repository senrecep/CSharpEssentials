using System.Net;
using System.Text;

#if NET10_0_OR_GREATER
using Carter;
#endif
using CSharpEssentials.Endpoints.Benchmarks.Throughput;

using FastEndpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using FeHost = CSharpEssentials.Endpoints.Benchmarks.Throughput.FastEndpointsHost;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// An in-process TestServer app with the three throughput routes (GET and DELETE <c>/users/{id:int}</c>, POST
/// <c>/users</c>) mapped by one variant. Plain and CSE validate with the .NET 10 DataAnnotations engine
/// (<c>AddValidation</c>), Carter and FastEndpoints with FluentValidation, so the POST rows compare different engines.
/// Invalid input answers 400 in every variant (the Carter sample answers 422; this uses the <c>ValidationProblem</c>
/// default). FastEndpointsSourceGen uses both generator features, discovered types and generated reflection data for
/// the request DTOs. FastEndpoints applies the <c>UseFastEndpoints</c> configuration lambda once per process, which
/// holds because each variant's host is built alone in its own benchmark process. Carter gets an
/// <c>Assembly[]</c>, because a single <c>Assembly</c> binds to the catalog constructor that walks deps.json.
/// </summary>
public sealed class ThroughputHost : IAsyncDisposable
{
    private const string UsersPath = "/users";
    private const string UserPath = "/users/1";
    private const string ExpectedUser = """{"id":1,"name":"user1"}""";
#if NET10_0_OR_GREATER
    private const string ExpectedCreated = """{"id":1,"name":"Ada"}""";
#endif

    private static readonly byte[] ValidBody = Encoding.UTF8.GetBytes("""{"name":"Ada","email":"ada@example.com","age":30}""");
    private static readonly byte[] InvalidBody = Encoding.UTF8.GetBytes("""{"name":"","email":"not-an-email","age":10}""");

    private readonly WebApplication _app;

    private ThroughputHost(WebApplication app, HttpClient client)
    {
        _app = app;
        Client = client;
    }

    /// <summary>The client that sends requests to the in-process server.</summary>
    public HttpClient Client { get; }

    /// <summary>Starts the app for <paramref name="variant"/>.</summary>
    /// <param name="variant">The mapping variant; <see cref="EndpointVariant.Baseline"/> is not supported.</param>
    /// <returns>The started host.</returns>
    public static async Task<ThroughputHost> StartAsync(EndpointVariant variant)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        AddServices(builder.Services, variant);

        WebApplication app = builder.Build();
        Map(app, variant);

        await app.StartAsync().ConfigureAwait(false);
        return new ThroughputHost(app, app.GetTestClient());
    }

    /// <summary>GET <c>/users/1</c>.</summary>
    /// <returns>The response status code.</returns>
    public Task<HttpStatusCode> GetByIdAsync() => SendAsync(HttpMethod.Get, UserPath, null);

    /// <summary>POST <c>/users</c> with a valid body.</summary>
    /// <returns>The response status code.</returns>
    public Task<HttpStatusCode> PostValidAsync() => SendAsync(HttpMethod.Post, UsersPath, ValidBody);

    /// <summary>POST <c>/users</c> with a body that breaks every rule.</summary>
    /// <returns>The response status code.</returns>
    public Task<HttpStatusCode> PostInvalidAsync() => SendAsync(HttpMethod.Post, UsersPath, InvalidBody);

    /// <summary>DELETE <c>/users/1</c>.</summary>
    /// <returns>The response status code.</returns>
    public Task<HttpStatusCode> DeleteAsync() => SendAsync(HttpMethod.Delete, UserPath, null);

    /// <summary>Fails unless every route answers like every other variant, so no row measures a rejected or different request.</summary>
    /// <returns>A task that completes when the check passes.</returns>
    public async Task VerifyAsync()
    {
        await ExpectAsync(HttpMethod.Get, UserPath, null, HttpStatusCode.OK, ExpectedUser).ConfigureAwait(false);
        await ExpectAsync(HttpMethod.Delete, UserPath, null, HttpStatusCode.NoContent, string.Empty).ConfigureAwait(false);
#if NET10_0_OR_GREATER
        await ExpectAsync(HttpMethod.Post, UsersPath, ValidBody, HttpStatusCode.OK, ExpectedCreated).ConfigureAwait(false);
        await ExpectAsync(HttpMethod.Post, UsersPath, InvalidBody, HttpStatusCode.BadRequest, null).ConfigureAwait(false);
#endif
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync().ConfigureAwait(false);
        await _app.DisposeAsync().ConfigureAwait(false);
    }

    private static void AddServices(IServiceCollection services, EndpointVariant variant)
    {
        switch (variant)
        {
            case EndpointVariant.Baseline:
                throw NeedsMapping(variant);
            case EndpointVariant.Plain:
            case EndpointVariant.CseGenerated:
            case EndpointVariant.CseReflection:
#if NET10_0_OR_GREATER
                services.AddValidation();
#endif
                break;
#if NET10_0_OR_GREATER
            // https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
            // https://github.com/CarterCommunity/Carter/blob/10.0.0/src/Carter/DependencyContextAssemblyCatalog.cs#L48
            case EndpointVariant.Carter:
                System.Reflection.Assembly[] carterAssemblies = [typeof(Throughput.CarterHost.GetUserModule).Assembly];
                services.AddCarter(new DependencyContextAssemblyCatalog(carterAssemblies));
                break;
#else
            case EndpointVariant.Carter:
                throw NeedsMapping(variant);
#endif
            // https://fast-endpoints.com/docs/get-started
            case EndpointVariant.FastEndpoints:
                services.AddFastEndpoints(options =>
                {
                    options.DisableAutoDiscovery = true;
                    options.Assemblies = [typeof(FeHost.GetUserEndpoint).Assembly];
                });
                break;
            // https://fast-endpoints.com/docs/configuration-settings#source-generator-based-startup
            case EndpointVariant.FastEndpointsSourceGen:
                services.AddFastEndpoints(FeHost.DiscoveredTypes.All);
                break;
            default:
                throw NeedsMapping(variant);
        }
    }

    private static void Map(WebApplication app, EndpointVariant variant)
    {
        switch (variant)
        {
            case EndpointVariant.Baseline:
                throw NeedsMapping(variant);
            case EndpointVariant.Plain:
                PlainUserEndpoints.Map(app);
                break;
            case EndpointVariant.CseGenerated:
                app.MapThroughputEndpoints();
                break;
            case EndpointVariant.CseReflection:
                app.MapEndpointsFromAssemblies(typeof(ThroughputHost).Assembly);
                break;
#if NET10_0_OR_GREATER
            // https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
            case EndpointVariant.Carter:
                app.MapCarter();
                break;
#else
            case EndpointVariant.Carter:
                throw NeedsMapping(variant);
#endif
            // https://fast-endpoints.com/docs/get-started
            case EndpointVariant.FastEndpoints:
                app.UseFastEndpoints();
                break;
            // https://fast-endpoints.com/docs/configuration-settings#source-generated-reflection
            case EndpointVariant.FastEndpointsSourceGen:
                app.UseFastEndpoints(c => FeHost.GeneratedReflection.AddFromCSharpEssentialsEndpointsBenchmarksThroughputFastEndpointsHost(c.Binding.ReflectionCache));
                break;
            default:
                throw NeedsMapping(variant);
        }
    }

    private static ArgumentOutOfRangeException NeedsMapping(EndpointVariant variant) =>
        new(nameof(variant), variant, "The throughput app needs a mapping variant from EndpointVariants.Mapping that runs on this target framework.");

    private async Task ExpectAsync(HttpMethod method, string path, byte[]? body, HttpStatusCode status, string? content)
    {
        using HttpResponseMessage response = await SendCoreAsync(method, path, body).ConfigureAwait(false);
        string actual = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (response.StatusCode != status || content is not null && !string.Equals(actual, content, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{method} {path} returned {(int)response.StatusCode} '{actual}' instead of {(int)status} '{content}'.");
        }
    }

    private async Task<HttpStatusCode> SendAsync(HttpMethod method, string path, byte[]? body)
    {
        using HttpResponseMessage response = await SendCoreAsync(method, path, body).ConfigureAwait(false);
        return response.StatusCode;
    }

    private async Task<HttpResponseMessage> SendCoreAsync(HttpMethod method, string path, byte[]? body)
    {
        using HttpRequestMessage request = new(method, new Uri(path, UriKind.Relative));
        if (body is not null)
        {
            request.Content = new ByteArrayContent(body);
            request.Content.Headers.ContentType = new("application/json");
        }

        HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false);
        await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        return response;
    }
}
