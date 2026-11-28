using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Examples.Endpoints;

public sealed class ListEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/endpoints", Ok<string[]> () =>
            TypedResults.Ok(ExamplesEndpointsEndpointRegistry.EndpointTypes.Select(type => type.Name).ToArray()));
}
