using System.Collections.Concurrent;
using System.Globalization;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

namespace Examples.AspNetCore;

[StringEnum]
public enum OrderStatus
{
    Pending,
    PendingApproval,
    Shipped,
}

public sealed record Order(Guid Id, string Customer, int Quantity, OrderStatus Status, int Revision) : IVersioned
{
    public string Version => Revision.ToString(CultureInfo.InvariantCulture);
}

public sealed record CreateOrder(string? Customer, int Quantity);

public sealed record UpdateOrder(OrderStatus Status);

public sealed class CreateOrderValidator : Validator<CreateOrder>
{
    protected override ValueTask Configure(CreateOrder model, RuleContext<CreateOrder> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Customer).NotEmpty().MaxLength(50);
        rules.For(() => model.Quantity).GreaterThan(0);
        return ValueTask.CompletedTask;
    }
}

public sealed class OrderStore
{
    public static readonly Guid SeedId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly ConcurrentDictionary<Guid, Order> _orders = new()
    {
        [SeedId] = new Order(SeedId, "ada", 2, OrderStatus.PendingApproval, 1),
    };

    public IEnumerable<Order> List(OrderStatus? status) =>
        _orders.Values.Where(o => status is null || o.Status == status);

    public Result<Order, NotFoundError> Find(Guid id) =>
        _orders.TryGetValue(id, out Order? order)
            ? order
            : new NotFoundError("order.not_found", $"Order {id} was not found.");

    public Order Add(CreateOrder command)
    {
        var order = new Order(Guid.NewGuid(), command.Customer!, command.Quantity, OrderStatus.Pending, 1);
        _orders[order.Id] = order;
        return order;
    }

    public Result<Order> Update(Guid id, UpdateOrder body, Preconditions preconditions)
    {
        if (!_orders.TryGetValue(id, out Order? current))
            return Error.NotFound("order.not_found", $"Order {id} was not found.");
        if (!preconditions.Matches(current))
            return ConditionalRequestErrors.PreconditionFailed;
        Order updated = current with { Status = body.Status, Revision = current.Revision + 1 };
        _orders[id] = updated;
        return updated;
    }
}
