namespace Examples.Enums.EndToEnd;

public sealed class Order
{
    public Guid Id { get; init; }
    public required string Customer { get; set; }
    public OrderStatus Status { get; set; }
    public Permissions Permissions { get; set; }
}

public sealed record CreateOrderRequest(string Customer, OrderStatus Status, Permissions Permissions);

public sealed record OrderResponse(Guid Id, string Customer, OrderStatus Status, Permissions Permissions)
{
    public static OrderResponse From(Order order) => new(order.Id, order.Customer, order.Status, order.Permissions);
}
