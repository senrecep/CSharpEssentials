using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Endpoints;

internal static class NamingEndpoints
{
    [ExcludeFromMapping]
    public sealed class Items : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/naming/items", () => "list");
            app.MapPost("/naming/items", () => "created");
        }
    }

    [ExcludeFromMapping]
    public sealed class Reports : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/naming/reports/daily", () => "daily");
            app.MapGet("/naming/reports/weekly", () => "weekly");
        }
    }

    [ExcludeFromMapping]
    public sealed class Echo : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) => app.MapGet("/naming/echo", () => "echo");
    }

    [ExcludeFromMapping]
    public sealed class EchoClash : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/naming/echo-clash", () => "clash").WithName("NamingEndpoints_Echo");
    }

    public static class Orders
    {
        [ExcludeFromMapping]
        public sealed class Endpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/naming/orders/{id:int}", (int id) => TypedResults.Ok(id));
        }
    }

    public static class Users
    {
        [ExcludeFromMapping]
        public sealed class Endpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/naming/users/{id:int}", (int id) => TypedResults.Ok(id));
        }
    }
}
