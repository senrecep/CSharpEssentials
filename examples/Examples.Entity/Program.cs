using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;

Console.WriteLine("1. EntityBase<TId> and audit fields");
Order order = Order.Place("customer-1", 120m);
order.SetCreatedInfo(DateTimeOffset.UtcNow, "alice");
order.ChangeTotal(150m);
order.SetUpdatedInfo(DateTimeOffset.UtcNow, "bob");
Console.WriteLine($"   Order {order.Id}: created by {order.CreatedBy} at {order.CreatedAt:t}, updated by {order.UpdatedBy} at {order.UpdatedAt:t}");

Console.WriteLine("2. Domain events");
IDomainEvent[] events = [.. order.DomainEvents];
order.ClearDomainEvents();
foreach (IDomainEvent e in events)
{
    DomainEventTiming timing = e.GetType().GetCustomAttributes(typeof(DomainEventTimingAttribute), false)
        is [DomainEventTimingAttribute attribute] ? attribute.Timing : DomainEventTiming.AfterSave;
    Console.WriteLine($"   {e.GetType().Name} ({timing})");
}
Console.WriteLine($"   Left on the entity after ClearDomainEvents: {order.DomainEvents.Count}");

Console.WriteLine("3. DomainEvent record with a fixed clock");
OrderPlaced placed = new(order.Id, new FixedClock(new DateTimeOffset(2026, 1, 15, 9, 30, 0, TimeSpan.Zero)));
Console.WriteLine($"   OccurredAt {placed.OccurredAt:O}, EventId {placed.EventId} (version {placed.EventId.ToString()[14]})");

Console.WriteLine("4. Soft delete, restore, hard delete");
Product[] products = [new("Pen"), new("Notebook")];
products[0].MarkAsDeleted(DateTimeOffset.UtcNow, "alice");
Console.WriteLine($"   {products[0].Name}: IsDeleted={products[0].IsDeleted}, DeletedBy={products[0].DeletedBy}, DeletedAt={products[0].DeletedAt:t}");
products[0].Restore();
Console.WriteLine($"   {products[0].Name} after Restore: IsDeleted={products[0].IsDeleted}");
products.HardDelete();
Console.WriteLine($"   After HardDelete(): {string.Join(", ", products.Select(p => $"{p.Name} IsHardDeleted={p.IsHardDeleted}"))}");

Console.WriteLine("5. Code against the interfaces");
PrintAudit(order);
PrintAudit(products[1]);

static void PrintAudit(ICreationAudit entity) =>
    Console.WriteLine($"   {entity.GetType().Name}: CreatedBy={entity.CreatedBy ?? "(not set)"}");

public sealed record OrderPlaced : DomainEvent
{
    public OrderPlaced(Guid orderId, TimeProvider clock) : base(clock) => OrderId = orderId;

    public OrderPlaced(Guid orderId) => OrderId = orderId;

    public Guid OrderId { get; init; }
}

[DomainEventTiming(DomainEventTiming.BeforeSave)]
public sealed record OrderTotalChanged(Guid OrderId, decimal Total) : IDomainEvent;

public sealed class Order : EntityBase<Guid>
{
    private Order() { }

    public string CustomerId { get; private set; } = "";
    public decimal Total { get; private set; }

    public static Order Place(string customerId, decimal total)
    {
        Order order = new() { Id = Guid.NewGuid(), CustomerId = customerId, Total = total };
        order.Raise(new OrderPlaced(order.Id));
        return order;
    }

    public void ChangeTotal(decimal total)
    {
        Total = total;
        Raise(new OrderTotalChanged(Id, total));
    }
}

public sealed class Product : SoftDeletableEntityBase<int>
{
    private static int _nextId;

    public Product(string name)
    {
        Id = ++_nextId;
        Name = name;
    }

    public string Name { get; }
}

public sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
