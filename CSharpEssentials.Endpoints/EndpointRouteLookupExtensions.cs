using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Builds request paths for mapped endpoint types through <see cref="EndpointTypeMetadata"/>, mainly for tests.
/// </summary>
public static class EndpointRouteLookupExtensions
{
    private const string UnreferencedCodeMessage = "Reads route values from the public properties of the values object, which trimming can remove.";

    /// <summary>
    /// Returns the path of the single route mapped by <typeparamref name="TEndpoint"/>, with route values filled in.
    /// </summary>
    /// <typeparam name="TEndpoint">The endpoint type.</typeparam>
    /// <param name="app">The route builder the endpoint was mapped on.</param>
    /// <param name="values">Route values as an object or dictionary, for example <c>new { id = 42 }</c>. Values without a route parameter become query string entries.</param>
    /// <returns>The path, for example <c>/apps/42</c>.</returns>
    /// <exception cref="InvalidOperationException">No route, more than one route, or a required route value is missing.</exception>
    [RequiresUnreferencedCode(UnreferencedCodeMessage)]
    public static string RouteOf<TEndpoint>(
        this IEndpointRouteBuilder app,
        object? values = null)
        where TEndpoint : IEndpoint =>
        Resolve(app, typeof(TEndpoint), null, values);

    /// <summary>
    /// Returns the path of the route mapped by <typeparamref name="TEndpoint"/> whose endpoint name or HTTP method equals
    /// <paramref name="nameOrMethod"/>, with route values filled in.
    /// </summary>
    /// <typeparam name="TEndpoint">The endpoint type.</typeparam>
    /// <param name="app">The route builder the endpoint was mapped on.</param>
    /// <param name="nameOrMethod">An endpoint name set with <c>WithName</c> (ordinal), or an HTTP method (case-insensitive).</param>
    /// <param name="values">Route values as an object or dictionary, for example <c>new { id = 42 }</c>. Values without a route parameter become query string entries.</param>
    /// <returns>The path, for example <c>/apps/42</c>.</returns>
    /// <exception cref="InvalidOperationException">No route, more than one route, or a required route value is missing.</exception>
    [RequiresUnreferencedCode(UnreferencedCodeMessage)]
    public static string RouteOf<TEndpoint>(
        this IEndpointRouteBuilder app,
        string nameOrMethod,
        object? values = null)
        where TEndpoint : IEndpoint
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrMethod);
        return Resolve(app, typeof(TEndpoint), nameOrMethod, values);
    }

    [RequiresUnreferencedCode(UnreferencedCodeMessage)]
    private static string Resolve(
        IEndpointRouteBuilder app,
        Type endpointType,
        string? nameOrMethod,
        object? values)
    {
        ArgumentNullException.ThrowIfNull(app);

        string selection = nameOrMethod is null ? string.Empty : $" matching '{nameOrMethod}'";
        RouteEndpoint[] routes = [.. app.DataSources
            .SelectMany(static source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<EndpointTypeMetadata>()?.EndpointType == endpointType)
            .Where(endpoint => nameOrMethod is null || Matches(endpoint, nameOrMethod))
            .DistinctBy(static endpoint => endpoint.RoutePattern.RawText, StringComparer.OrdinalIgnoreCase)];
        if (routes.Length == 0)
        {
            throw new InvalidOperationException(
                $"No route{selection} is mapped for endpoint '{endpointType.FullName}'. Map the endpoints on this route builder before calling RouteOf.");
        }

        if (routes.Length > 1)
        {
            string patterns = string.Join(", ", routes.Select(static endpoint => "'" + endpoint.RoutePattern.RawText + "'"));
            throw new InvalidOperationException(
                $"Endpoint '{endpointType.FullName}' maps {routes.Length} routes{selection} ({patterns}). Pass an endpoint name or HTTP method to select one.");
        }

        RouteEndpoint route = routes[0];
        TemplateBinder binder = app.ServiceProvider.GetRequiredService<TemplateBinderFactory>().Create(route.RoutePattern);
        RouteValueDictionary routeValues = [with(values)];
        TemplateValuesResult? accepted = binder.GetValues(null, routeValues);
        string? path = accepted is not null && binder.TryProcessConstraints(null, accepted.CombinedValues, out _, out _)
            ? binder.BindValues(accepted.AcceptedValues)
            : null;
        return path ?? throw new InvalidOperationException(
            $"Route '{route.RoutePattern.RawText}' of endpoint '{endpointType.FullName}' needs route values that were not supplied or do not match its constraints.");
    }

    private static bool Matches(RouteEndpoint endpoint, string nameOrMethod) =>
        string.Equals(endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName, nameOrMethod, StringComparison.Ordinal) ||
        (endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(nameOrMethod, StringComparer.OrdinalIgnoreCase) ?? false);
}
