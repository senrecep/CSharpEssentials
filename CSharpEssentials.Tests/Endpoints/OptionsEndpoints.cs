using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Endpoints;

internal static class OptionsEndpoints
{
    public sealed class UsersGroup : IEndpointGroup
    {
        public static string Prefix => "users";

        public static void Configure(RouteGroupBuilder group) => group.WithMetadata(new OrderMarker("group"));
    }

    public sealed class Reports : IEndpointGroup
    {
        public static string Prefix => "reports";
    }

    [EndpointGroup(typeof(UsersGroup))]
    public sealed class ListUsers : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => TypedResults.Ok());
    }

    [EndpointGroup(typeof(UsersGroup))]
    public sealed class NamedUser : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) => app.MapGet("/{id:int}", (int id) => TypedResults.Ok(id)).WithName("ExplicitName");
    }

    [EndpointGroup(typeof(UsersGroup))]
    public sealed class TaggedUser : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) => app.MapGet("/tagged", () => TypedResults.Ok()).WithTags("Explicit");
    }

    [EndpointGroup<Reports>]
    public sealed class DailyReport : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) => app.MapGet("/daily", () => TypedResults.Ok());
    }

    public sealed class Ping : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) => app.MapGet("/ping", () => "pong");
    }
}
