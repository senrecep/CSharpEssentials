using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Marks a type whose static <see cref="Map"/> method maps one or more Minimal API routes.
/// Endpoint types are discovered at compile time and are never instantiated.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint's routes on <paramref name="app"/>.
    /// </summary>
    /// <param name="app">
    /// The builder to map on: the endpoint's group when the type has an endpoint group attribute,
    /// otherwise an empty-prefix group under the root builder.
    /// </param>
    static abstract void Map(IEndpointRouteBuilder app);
}
