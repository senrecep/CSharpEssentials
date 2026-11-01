using System.Reflection;

#if NET10_0_OR_GREATER
using Carter;
#endif
using FastEndpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// The measured startup unit: the same slim builder for every variant, the variant's services, Build, the variant's
/// mapping call, then a full enumeration of every endpoint data source (which runs RequestDelegateFactory), then dispose.
/// No server is started.
/// <para>
/// The unit is not the same work for every variant. Plain, CSE and Carter map <c>(int id) =&gt; ...</c> handlers, so the
/// enumeration pays RequestDelegateFactory's full binding cost per endpoint. FastEndpoints v8.3 maps every endpoint to a
/// parameterless <c>() =&gt; FeRequestHandler.Instance</c>
/// (https://github.com/FastEndpoints/FastEndpoints/blob/v8.3/Src/Library/Main/EndpointRouteMapper.cs#L47-L63) and builds
/// its request binder, validator, object factory and property setters on each endpoint's first request. That work is
/// outside <see cref="BuildAsync"/> and inside <see cref="ServeAllAsync"/>, which <see cref="ColdStartupBenchmarks"/> measures.
/// </para>
/// <para>
/// Carter gets an <c>Assembly[]</c>, because a single <c>Assembly</c> binds to the catalog constructor that walks deps.json.
/// </para>
/// </summary>
public static class StartupApp
{
    /// <summary>Builds an app for <paramref name="variant"/> with <paramref name="n"/> endpoints and returns its endpoints.</summary>
    /// <param name="variant">The mapping variant.</param>
    /// <param name="n">The endpoint count: 10, 100 or 500.</param>
    /// <returns>Every endpoint the app exposes.</returns>
    public static async Task<IReadOnlyList<Endpoint>> BuildAsync(EndpointVariant variant, int n)
    {
        WebApplication app = Create(variant, n, testServer: false);
        await using (app.ConfigureAwait(false))
        {
            Map(app, variant, n);
            return [.. ((IEndpointRouteBuilder)app).DataSources.SelectMany(static source => source.Endpoints)];
        }
    }

    /// <summary>
    /// Starts the same app on a TestServer and sends <c>GET</c> to <paramref name="path"/>. Self-check only, never measured.
    /// </summary>
    /// <param name="variant">The mapping variant.</param>
    /// <param name="n">The endpoint count: 10, 100 or 500.</param>
    /// <param name="path">The request path.</param>
    /// <returns>The status code and body.</returns>
    public static async Task<(int Status, string Body)> GetAsync(EndpointVariant variant, int n, string path)
    {
        WebApplication app = Create(variant, n, testServer: true);
        await using (app.ConfigureAwait(false))
        {
            Map(app, variant, n);
            await app.StartAsync().ConfigureAwait(false);
            using HttpClient client = app.GetTestClient();
            using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative)).ConfigureAwait(false);
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            await app.StopAsync().ConfigureAwait(false);
            return ((int)response.StatusCode, body);
        }
    }

    /// <summary>
    /// Starts the app on a TestServer, sends <c>GET /items{i}/1</c> to every endpoint once, stops and disposes it.
    /// Throws unless every request answers 200.
    /// </summary>
    /// <param name="variant">The mapping variant.</param>
    /// <param name="n">The endpoint count: 10, 100 or 500.</param>
    /// <returns>The number of 200 responses, which equals <paramref name="n"/>.</returns>
    public static async Task<int> ServeAllAsync(EndpointVariant variant, int n)
    {
        WebApplication app = Create(variant, n, testServer: true);
        await using (app.ConfigureAwait(false))
        {
            Map(app, variant, n);
            await app.StartAsync().ConfigureAwait(false);
            using HttpClient client = app.GetTestClient();
            int ok = 0;
            for (int i = 0; i < n; i++)
            {
                using HttpResponseMessage response = await client
                    .GetAsync(new Uri("/items" + i.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/1", UriKind.Relative))
                    .ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    ok++;
            }

            await app.StopAsync().ConfigureAwait(false);
            if (ok != n)
                throw new InvalidOperationException($"{variant} with N={n}: {ok} of {n} endpoints answered 200.");
            return ok;
        }
    }

    private static WebApplication Create(EndpointVariant variant, int n, bool testServer)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        if (testServer)
            builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        AddServices(builder.Services, variant, n);
        return builder.Build();
    }

    private static void AddServices(IServiceCollection services, EndpointVariant variant, int n)
    {
        switch (variant)
        {
            case EndpointVariant.Baseline:
            case EndpointVariant.Plain:
            case EndpointVariant.CseGenerated:
            case EndpointVariant.CseReflection:
                break;
#if NET10_0_OR_GREATER
            // https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
            // https://github.com/CarterCommunity/Carter/blob/10.0.0/src/Carter/DependencyContextAssemblyCatalog.cs#L48
            case EndpointVariant.Carter:
                Assembly[] carterAssemblies = [CarterAssembly(n)];
                services.AddCarter(new DependencyContextAssemblyCatalog(carterAssemblies));
                break;
#else
            case EndpointVariant.Carter:
                throw CarterUnsupported();
#endif
            // https://fast-endpoints.com/docs/get-started
            // https://github.com/FastEndpoints/FastEndpoints/blob/v8.3/Src/Library/Main/EndpointDiscoveryOptions.cs
            case EndpointVariant.FastEndpoints:
                services.AddFastEndpoints(options =>
                {
                    options.DisableAutoDiscovery = true;
                    options.Assemblies = [FastEndpointsAssembly(n)];
                });
                break;
            // https://fast-endpoints.com/docs/configuration-settings#source-generator-based-startup
            case EndpointVariant.FastEndpointsSourceGen:
                services.AddFastEndpoints(FastEndpointsDiscoveredTypes(n));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(variant), variant, null);
        }
    }

    private static void Map(WebApplication app, EndpointVariant variant, int n)
    {
        switch (variant)
        {
            case EndpointVariant.Baseline:
                break;
            case EndpointVariant.Plain:
                MapPlain(app, n);
                break;
            case EndpointVariant.CseGenerated:
                MapCseGenerated(app, n);
                break;
            case EndpointVariant.CseReflection:
                app.MapEndpointsFromAssemblies(CseAssembly(n));
                break;
#if NET10_0_OR_GREATER
            // https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
            case EndpointVariant.Carter:
                app.MapCarter();
                break;
#else
            case EndpointVariant.Carter:
                throw CarterUnsupported();
#endif
            // https://fast-endpoints.com/docs/get-started
            case EndpointVariant.FastEndpoints:
            case EndpointVariant.FastEndpointsSourceGen:
                app.UseFastEndpoints();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(variant), variant, null);
        }
    }

    private static void MapPlain(WebApplication app, int n)
    {
        switch (n)
        {
            case 10:
                Startup.Plain10.PlainEndpoints.Map(app);
                break;
            case 100:
                Startup.Plain100.PlainEndpoints.Map(app);
                break;
            case 500:
                Startup.Plain500.PlainEndpoints.Map(app);
                break;
            default:
                throw Unsupported(n);
        }
    }

    private static void MapCseGenerated(WebApplication app, int n)
    {
        switch (n)
        {
            case 10:
                app.MapCse10Endpoints();
                break;
            case 100:
                app.MapCse100Endpoints();
                break;
            case 500:
                app.MapCse500Endpoints();
                break;
            default:
                throw Unsupported(n);
        }
    }

    private static Assembly CseAssembly(int n) => n switch
    {
        10 => typeof(Startup.Cse10.ItemResponse).Assembly,
        100 => typeof(Startup.Cse100.ItemResponse).Assembly,
        500 => typeof(Startup.Cse500.ItemResponse).Assembly,
        _ => throw Unsupported(n),
    };

#if NET10_0_OR_GREATER
    private static Assembly CarterAssembly(int n) => n switch
    {
        10 => typeof(Startup.Carter10.ItemResponse).Assembly,
        100 => typeof(Startup.Carter100.ItemResponse).Assembly,
        500 => typeof(Startup.Carter500.ItemResponse).Assembly,
        _ => throw Unsupported(n),
    };
#endif

    private static Assembly FastEndpointsAssembly(int n) => n switch
    {
        10 => typeof(Startup.Fe10.ItemResponse).Assembly,
        100 => typeof(Startup.Fe100.ItemResponse).Assembly,
        500 => typeof(Startup.Fe500.ItemResponse).Assembly,
        _ => throw Unsupported(n),
    };

    private static List<Type> FastEndpointsDiscoveredTypes(int n) => n switch
    {
        10 => Startup.Fe10.DiscoveredTypes.All,
        100 => Startup.Fe100.DiscoveredTypes.All,
        500 => Startup.Fe500.DiscoveredTypes.All,
        _ => throw Unsupported(n),
    };

#if !NET10_0_OR_GREATER
    private static PlatformNotSupportedException CarterUnsupported() => new("Carter 10 targets net10.0 and later.");

#endif
    private static ArgumentOutOfRangeException Unsupported(int n) =>
        new(nameof(n), n, "The startup libraries exist for 10, 100 and 500 endpoints.");
}
