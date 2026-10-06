using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace Examples.Enums.EndToEnd;

public sealed class OrderService(ShopDbContext db)
{
    public async Task<Result<Order>> Get(Guid id, CancellationToken cancellationToken)
    {
        Order? order = await db.Orders.FindAsync([id], cancellationToken);
        if (order is null)
            return Error.NotFound("Order.NotFound", $"Order '{id}' was not found.");

        return order;
    }

    public Task<List<Order>> List(OrderStatus? status, Permissions permissions, CancellationToken cancellationToken) =>
        db.Orders
            .Where(o => status == null || o.Status == status)
            .Where(o => (o.Permissions & permissions) == permissions)
            .ToListAsync(cancellationToken);

    public async Task<Result<Order>> Create(CreateOrderRequest request, OrderStatus? source, CancellationToken cancellationToken)
    {
        if (request.Status == OrderStatus.Shipped && source != OrderStatus.PendingApproval)
            return Error.Validation("Order.Status", "An order is shipped only after approval.");

        Order order = new()
        {
            Id = Guid.CreateVersion7(),
            Customer = request.Customer,
            Status = request.Status,
            Permissions = request.Permissions,
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return order;
    }
}
