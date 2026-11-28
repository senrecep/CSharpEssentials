using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

public sealed record Order(int Id, string Name);

public static class OrderErrors
{
    public static class Codes
    {
        public const string NameRequired = "Order.Name.Required";
        public const string NotFound = "Order.NotFound";
        public const string Taken = "Order.Taken";
    }

    public static readonly NotFoundError NotFound = new(Codes.NotFound, "Order not found.");
    public static readonly ConflictError Taken = new(Codes.Taken, "Order is taken.");
}

public sealed class OrderService
{
    private readonly Dictionary<int, Order> _orders = new() { [1] = new Order(1, "Book"), [2] = new Order(2, "Pen") };
    private readonly HashSet<int> _reserved = [2];

    public Result<Order> Create(int id, string name)
    {
        if (name.Length == 0)
            return Error.Validation(OrderErrors.Codes.NameRequired, "Name is required.");
        var order = new Order(id, name);
        _orders[id] = order;
        return order;
    }

    public Result Delete(int id) =>
        _orders.Remove(id) ? Result.Success() : OrderErrors.NotFound.ToError();

    public Maybe<Order> Find(int id) =>
        _orders.TryGetValue(id, out Order? order) ? Maybe.From(order) : Maybe<Order>.None;

    public Result<Order, NotFoundError, ConflictError> Reserve(int id)
    {
        if (!_orders.TryGetValue(id, out Order? order))
            return OrderErrors.NotFound;
        if (!_reserved.Add(id))
            return OrderErrors.Taken;
        return order;
    }
}
