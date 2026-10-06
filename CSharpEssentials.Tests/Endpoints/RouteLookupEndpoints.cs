using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Endpoints;

internal static class RouteLookupEndpoints
{
    public sealed class LookupGroup : IEndpointGroup
    {
        public static string Prefix => "lookup/{tenant}";
    }

    [EndpointGroup<LookupGroup>]
    public sealed class GetLookupItem : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/items/{id:int}", (string tenant, int id) => TypedResults.Ok($"{tenant}:{id}"));
    }

    [ExcludeFromMapping]
    public sealed class LookupItemCommands : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/commands/{id:int}", (int id) => TypedResults.Ok($"updated:{id}")).WithName("UpdateLookupItem");
            app.MapDelete("/commands/{id:int}/remove", (int id) => TypedResults.Ok($"removed:{id}")).WithName("RemoveLookupItem");
        }
    }
}
