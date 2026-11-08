---
name: csharpessentials-entity
description: Use when building DDD domain models. Covers EntityBase<TId> for aggregate roots with audit fields and domain events (Raise/DomainEvents/ClearDomainEvents), SoftDeletableEntityBase<TId> for the MarkAsDeleted/Restore/MarkAsHardDeleted lifecycle, and IDomainEvent with [DomainEventTiming] for publish timing.
---

# CSharpEssentials.Entity

DDD base classes for aggregate roots with audit fields, soft deletion and domain events.

```bash
dotnet add package CSharpEssentials.Entity
```

```csharp
using CSharpEssentials.Entity;            // EntityBase, SoftDeletableEntityBase
using CSharpEssentials.Entity.Interfaces; // IDomainEvent, ISoftDeletable, IEntityBase
```

---

## EntityBase\<TId\>

```csharp
public record OrderCreatedEvent(Guid OrderId, string CustomerId) : IDomainEvent;

public class Order : EntityBase<Guid>
{
    public string CustomerId { get; private set; } = default!;
    public decimal Total { get; private set; }

    public static Order Create(string customerId, decimal total)
    {
        var order = new Order { Id = Guid.NewGuid(), CustomerId = customerId, Total = total };
        order.Raise(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }
}

// Provided members:
// TId? Id
// DateTimeOffset CreatedAt: set by AuditInterceptor (or SetCreatedInfo); MinValue throws
// string? CreatedBy: set by AuditInterceptor (or SetCreatedInfo)
// DateTimeOffset? UpdatedAt: NOT ModifiedAt
// string? UpdatedBy: NOT ModifiedBy
// IReadOnlyList<IDomainEvent> DomainEvents: property, a cached live read-only view (not a copy)
// void Raise(IDomainEvent)
// void ClearDomainEvents()
```

---

## SoftDeletableEntityBase\<TId\>

```csharp
public class Product : SoftDeletableEntityBase<int>
{
    public string Name { get; private set; } = default!;
}

var product = new Product();

// Soft delete lifecycle
product.MarkAsDeleted(DateTimeOffset.UtcNow, "admin"); // two params: (deletedAt, deletedBy)
product.MarkAsHardDeleted();                            // sets IsHardDeleted
product.Restore();                                      // clears IsDeleted, DeletedAt, DeletedBy and IsHardDeleted

// Hard-delete a batch: products.HardDelete() calls MarkAsHardDeleted() on each item
// IsHardDeleted makes the EF Core AuditInterceptor remove the row instead of turning the delete into a soft delete

// Additional members:
// DateTimeOffset? DeletedAt
// string? DeletedBy
// bool IsDeleted
// bool IsHardDeleted
```

---

## Domain events

```csharp
// Default timing is AfterSave; a BeforeSave event is published before the save and a throwing handler aborts it
[DomainEventTiming(DomainEventTiming.BeforeSave)]
public record InventoryReservedEvent(Guid ProductId, int Qty) : IDomainEvent;

// Raise inside the aggregate (see Order.Create above)
Order order = Order.Create("customer-1", 99m);

// Copy, then clear: DomainEvents is a live view, so an uncopied reference is empty after the clear
IDomainEvent[] events = [.. order.DomainEvents];
order.ClearDomainEvents();
```

---

## Guidance

- Call `Raise()` only inside entity methods to keep domain events encapsulated in the aggregate.
- `DomainEvents` is a property; there is no `GetDomainEvents()`. Raising or clearing while iterating it throws `InvalidOperationException`.
- Audit fields are `UpdatedAt`/`UpdatedBy`, not `ModifiedAt`/`ModifiedBy`.
- `MarkAsDeleted()` takes two parameters: `(DateTimeOffset deletedAt, string deletedBy)`.
- Use `DomainEventTiming.BeforeSave` for events that must be processed before the save; the attribute only labels the event, `CSharpEssentials.EntityFrameworkCore` publishes it (`BaseDbContext` dispatches `AfterSave` events after the save, `DomainEventInterceptor` inside `SavingChanges`, before the write).
