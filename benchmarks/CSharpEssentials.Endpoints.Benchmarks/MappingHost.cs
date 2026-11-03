using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// The measured unit of the naming and group benchmarks, the same as <see cref="StartupApp"/> without services: slim builder,
/// Build, the mapping call, a full enumeration of every endpoint data source, then dispose. No server is started.
/// </summary>
public static class MappingHost
{
    /// <summary>Builds an app, maps it with <paramref name="map"/> and returns its endpoints and data-source count.</summary>
    /// <param name="map">The mapping call.</param>
    /// <returns>Every endpoint the app exposes and the number of endpoint data sources.</returns>
    public static async Task<(IReadOnlyList<Endpoint> Endpoints, int DataSources)> BuildAsync(Action<WebApplication> map)
    {
        WebApplication app = Create(map, testServer: false);
        await using (app.ConfigureAwait(false))
        {
            return Enumerate(app);
        }
    }

    /// <summary>Builds and maps an app that the caller keeps and disposes.</summary>
    /// <param name="map">The mapping call.</param>
    /// <returns>The app and its endpoints.</returns>
    public static (WebApplication App, IReadOnlyList<Endpoint> Endpoints) BuildKept(Action<WebApplication> map)
    {
        WebApplication app = Create(map, testServer: false);
        return (app, Enumerate(app).Endpoints);
    }

    /// <summary>
    /// Reads the endpoint name of every endpoint and counts the named ones. Reading is part of the unit, because
    /// <c>OperationNaming.TypeName</c> resolves names on the first read.
    /// </summary>
    /// <param name="endpoints">The endpoints.</param>
    /// <returns>The number of endpoints with a name.</returns>
    public static int CountNames(IReadOnlyList<Endpoint> endpoints)
    {
        int named = 0;
        for (int i = 0; i < endpoints.Count; i++)
        {
            if (endpoints[i].Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName is not null)
                named++;
        }

        return named;
    }

    /// <summary>Starts the same app on a TestServer and sends <c>GET</c> to <paramref name="path"/>. Self-check only, never measured.</summary>
    /// <param name="map">The mapping call.</param>
    /// <param name="path">The request path.</param>
    /// <returns>The status code and body.</returns>
    public static async Task<(int Status, string Body)> GetAsync(Action<WebApplication> map, string path)
    {
        WebApplication app = Create(map, testServer: true);
        await using (app.ConfigureAwait(false))
        {
            await app.StartAsync().ConfigureAwait(false);
            using HttpClient client = app.GetTestClient();
            using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative)).ConfigureAwait(false);
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            await app.StopAsync().ConfigureAwait(false);
            return ((int)response.StatusCode, body);
        }
    }

    private static WebApplication Create(Action<WebApplication> map, bool testServer)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        if (testServer)
            builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        WebApplication app = builder.Build();
        map(app);
        return app;
    }

    private static (IReadOnlyList<Endpoint> Endpoints, int DataSources) Enumerate(WebApplication app)
    {
        ICollection<EndpointDataSource> sources = ((IEndpointRouteBuilder)app).DataSources;
        return ([.. sources.SelectMany(static source => source.Endpoints)], sources.Count);
    }
}
