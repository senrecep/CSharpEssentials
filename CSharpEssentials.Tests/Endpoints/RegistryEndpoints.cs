using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Endpoints;

internal static class RegistryEndpoints
{
    public const string FilterHeader = "X-Endpoint-Filter";

    public sealed class OrdersGroup : IEndpointGroup
    {
        public static string Prefix => "orders";

        public static void Configure(RouteGroupBuilder group) => group.WithTags("Orders").WithMetadata(new OrderMarker("outer"));
    }

    [EndpointGroup<OrdersGroup>]
    public sealed class OrderItemsGroup : IEndpointGroup
    {
        public static string Prefix => "{orderId:int}/items";

        public static void Configure(RouteGroupBuilder group) => group.WithMetadata(new OrderMarker("inner"));
    }

    [EndpointGroup<OrderItemsGroup>]
    public sealed class ListOrderItems : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/", (int orderId) => TypedResults.Ok(orderId)).WithName("ListOrderItems").WithGroupName("v1");
    }

    public sealed class Documented : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/documented/{id:int}", (int id) => TypedResults.Ok(id)).WithName("GetDocumented").WithTags("Docs").WithGroupName("v1");
    }

    public sealed class Filtered : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/filtered", () => "filtered").AddEndpointFilter(async (context, next) =>
            {
                context.HttpContext.Response.Headers[FilterHeader] = "applied";
                return await next(context);
            });
    }

    public sealed class Secured : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app) => app.MapGet("/secured", () => "secret").RequireAuthorization();
    }
}
