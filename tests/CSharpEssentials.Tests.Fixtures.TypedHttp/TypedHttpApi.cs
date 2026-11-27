using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

#if TYPED_HTTP_RDG
namespace CSharpEssentials.Tests.Fixtures.TypedHttpRdg;
#else
namespace CSharpEssentials.Tests.Fixtures.TypedHttp;
#endif

/// <summary>An order.</summary>
public sealed record TypedOrder(int Id, decimal Total);

/// <summary>The request body of <c>POST /typed/orders</c>.</summary>
public sealed record CreateTypedOrder(int Id);

/// <summary>A nested leaf error: the order is locked.</summary>
public sealed record TypedOrderLocked(int Id) : ConflictError("Order.Locked", $"Order {Id} is locked.");

/// <summary>
/// Typed result endpoints in the <see cref="Document"/> document. Order 0 is not found, order 7 is locked, any other order exists.
/// </summary>
public static class TypedHttpApi
{
    public const string Document = "typed";

    private static readonly NotFoundError NotFound = new("Order.NotFound", "The order was not found.");

    public static Result<TypedOrder, NotFoundError, TypedOrderLocked> Get(int id) => id switch
    {
        0 => NotFound,
        7 => new TypedOrderLocked(id),
        _ => new TypedOrder(id, id * 10m),
    };

    public static IEndpointRouteBuilder MapTypedHttpApi(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/typed").WithGroupName(Document);
        group.MapGet("/orders/{id:int}", static (int id) => Get(id).ToHttpResult());
        group.MapGet("/async/{id:int}", static async (int id) => await Task.FromResult(Get(id)).ToHttpResult());
        group.MapPost("/orders", static (CreateTypedOrder order) => Get(order.Id).ToCreated(static o => $"/typed/orders/{o.Id}"));
        group.MapDelete("/orders/{id:int}", static (int id) => Get(id).ToNoContent());
        group.MapGet("/filtered/{id:int}", static (int id) => Get(id)).AddEndpointFilter<ResultEndpointFilter>();
        group.MapGet("/raw/{id:int}", static (int id) => Get(id)).ExcludeFromDescription();
        return app;
    }
}
