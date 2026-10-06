using CSharpEssentials.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Examples.Enums.EndToEnd;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrders(this RouteGroupBuilder group)
    {
        // Route: /orders/status/pending_approval, /orders/status/PendingApproval, /orders/status/1 and /orders/status/approval bind.
        group.MapGet("/orders/status/{status}", async (OrderStatus status, OrderService orders, CancellationToken ct) =>
            Results.Ok((await orders.List(status, Permissions.None, ct)).Select(OrderResponse.From)))
            .Produces<IEnumerable<OrderResponse>>();

        // Query: ?status=shipped&permissions=read&permissions=write (or permissions=read,write). An empty status binds null.
        group.MapGet("/orders", async (OrderStatus? status, Permissions? permissions, OrderService orders, CancellationToken ct) =>
            Results.Ok((await orders.List(status, permissions ?? Permissions.None, ct)).Select(OrderResponse.From)))
            .Produces<IEnumerable<OrderResponse>>();

        group.MapGet("/orders/{id:guid}", async (Guid id, OrderService orders, CancellationToken ct) =>
            (await orders.Get(id, ct)).Match(order => Results.Ok(OrderResponse.From(order)), errors => errors.ToProblemResult()))
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Body: { "customer": "ada", "status": "pending_approval", "permissions": ["read", "write"] }.
        // Header: X-Source-Status: approval.
        group.MapPost("/orders", async (
                CreateOrderRequest request,
                [FromHeader(Name = "X-Source-Status")] OrderStatus? source,
                OrderService orders,
                CancellationToken ct) =>
            (await orders.Create(request, source, ct)).Match(
                order => Results.Created($"/orders/{order.Id}", OrderResponse.From(order)),
                errors => errors.ToProblemResult()))
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return group;
    }
}
