using System.Globalization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>Fails unless an app maps exactly the expected <c>GET /items{i}/{id:int}</c> routes, so no row measures a different app.</summary>
public static class StartupSelfCheck
{
    /// <summary>
    /// The extra endpoint FastEndpoints always maps (<c>MapFastEndpoints</c> adds it for its test URL cache). It is
    /// real FastEndpoints startup cost, so it stays in the measured unit and is the only extra endpoint allowed.
    /// </summary>
    public const string FastEndpointsTestUrlCacheRoute = "_test_url_cache_";

    /// <summary>Throws when <paramref name="endpoints"/> is not the expected app for <paramref name="variant"/>.</summary>
    /// <param name="variant">The variant that built the app.</param>
    /// <param name="n">The endpoint count the app should map.</param>
    /// <param name="endpoints">The endpoints the app exposes.</param>
    public static void Verify(EndpointVariant variant, int n, IReadOnlyList<Endpoint> endpoints)
    {
        int expected = variant == EndpointVariant.Baseline ? 0 : n;
        HashSet<string> want = [.. Enumerable.Range(0, expected).Select(static i => "/items" + i.ToString(CultureInfo.InvariantCulture) + "/{id:int}")];
        List<RouteEndpoint> routeEndpoints = [.. endpoints.OfType<RouteEndpoint>()];
        List<string> routes = [.. routeEndpoints.Select(static e => e.RoutePattern.RawText ?? string.Empty)];
        bool isFastEndpoints = variant is EndpointVariant.FastEndpoints or EndpointVariant.FastEndpointsSourceGen;
        List<RouteEndpoint> itemEndpoints = [.. routeEndpoints.Where(e => !(isFastEndpoints && (e.RoutePattern.RawText ?? string.Empty).Contains(FastEndpointsTestUrlCacheRoute, StringComparison.Ordinal)))];
        List<string> items = [.. itemEndpoints.Select(static e => e.RoutePattern.RawText ?? string.Empty)];
        bool allGet = itemEndpoints.TrueForAll(static e => e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods is [var method] && method == HttpMethods.Get);

        if (endpoints.Count != routes.Count || items.Count != expected || !allGet || !want.SetEquals(items.Select(Normalize)))
        {
            throw new InvalidOperationException(
                $"{variant} with N={n} mapped {endpoints.Count} endpoints ({items.Count} item routes, all GET: {allGet}) instead of {expected} GET routes: "
                + string.Join(", ", routes.Take(5)));
        }
    }

    /// <summary>Throws unless <c>GET /items{N-1}/7</c> answers 200 with the shared <c>ItemResponse</c> body.</summary>
    /// <param name="variant">The variant to start.</param>
    /// <param name="n">The endpoint count.</param>
    /// <returns>A task that completes when the check passes.</returns>
    public static async Task VerifyResponseAsync(EndpointVariant variant, int n)
    {
        string last = (n - 1).ToString(CultureInfo.InvariantCulture);
        string path = "/items" + last + "/7";
        string expected = "{\"id\":7,\"name\":\"item" + last + "\"}";
        (int status, string body) = await StartupApp.GetAsync(variant, n, path).ConfigureAwait(false);
        if (status != 200 || !string.Equals(body, expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"{variant} with N={n}: GET {path} returned {status} '{body}' instead of 200 '{expected}'.");
    }

    private static string Normalize(string route) => route.StartsWith('/') ? route : "/" + route;
}
