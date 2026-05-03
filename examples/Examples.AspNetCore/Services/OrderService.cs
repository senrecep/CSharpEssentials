using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace Examples.AspNetCore.Services;

/// <summary>
/// Demonstrates Result composition with Then, Match, and Else.
/// </summary>
public sealed class OrderService : IOrderService
{
    private readonly IProductService _productService;
    private static readonly List<Order> _orders = new();

    public OrderService(IProductService productService)
    {
        _productService = productService;
    }

    public Result<Order> PlaceOrder(Guid productId, int quantity)
    {
        // -------------------------------------------------------------------
        // RESULT CHAINING with Then
        // Each step only executes if the previous one succeeded.
        // No nested if-blocks, no exceptions for business rules.
        // -------------------------------------------------------------------

        return _productService.GetById(productId)
            .Then(product => ValidateQuantity(product, quantity))
            .Then(product => ReserveStock(product, quantity))
            .Then(product => CreateOrder(product, quantity));
    }

    private static Result<Product> ValidateQuantity(Product product, int quantity)
    {
        if (quantity <= 0)
            return Error.Validation("Order.Quantity", "Quantity must be greater than zero.");

        if (quantity > product.StockQuantity)
        {
            return Error.Conflict(
                "Order.Stock",
                $"Only {product.StockQuantity} units available, but {quantity} requested."
            );
        }

        return Result.Success(product);
    }

    private static Result<Product> ReserveStock(Product product, int quantity)
    {
        // In a real app this would update the database.
        // Here we just validate the reservation step succeeded.
        if (product.Price * quantity > 10_000)
        {
            return Error.Conflict(
                "Order.Limit",
                "Order total exceeds the $10,000 limit."
            );
        }

        return Result.Success(product);
    }

    private static Result<Order> CreateOrder(Product product, int quantity)
    {
        var order = new Order(
            Guid.NewGuid(),
            product.Id,
            quantity,
            product.Price * quantity,
            OrderStatus.Pending,
            DateTime.UtcNow
        );

        _orders.Add(order);
        return Result.Success(order);
    }
}
