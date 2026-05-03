using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace Examples.AspNetCore.Services;

/// <summary>
/// Order service demonstrating chaining Results with Then and Else.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Places an order for a product.
    /// Uses Result chaining: validate -> reserve stock -> create order.
    /// </summary>
    Result<Order> PlaceOrder(Guid productId, int quantity);
}

public sealed record Order(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal TotalPrice,
    OrderStatus Status,
    DateTime PlacedAt
);

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
