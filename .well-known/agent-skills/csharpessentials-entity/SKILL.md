---
name: csharpessentials-entity
description: Use when building DDD domain models — EntityBase<TId> for aggregate roots with audit fields and domain events, SoftDeletableEntityBase for soft deletion lifecycle, and IDomainEvent for defining and raising domain events.
---

# CSharpEssentials.Entity

DDD base classes for aggregate roots. Built-in audit tracking, soft deletion, and domain event support.

## Installation

```bash
dotnet add package CSharpEssentials.Entity
```

## Namespaces

```csharp
using CSharpEssentials.Entity;            // EntityBase, SoftDeletableEntityBase
using CSharpEssentials.Entity.Interfaces; // IDomainEvent, ISoftDeletable, IEntityBase
```

---

## EntityBase\<TId\>

```csharp
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
// DateTimeOffset CreatedAt          — set by AuditInterceptor
// string? CreatedBy                 — set by AuditInterceptor
// DateTimeOffset? UpdatedAt         — NOT ModifiedAt
// string? UpdatedBy                 — NOT ModifiedBy
// IReadOnlyList<IDomainEvent> DomainEvents  — property, NOT a method
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

// Soft delete lifecycle
product.MarkAsDeleted(DateTimeOffset.UtcNow, "admin"); // two params: (deletedAt, deletedBy)
product.Restore();                                      // undoes soft delete
product.MarkAsHardDeleted();                           // irreversible

// Additional members:
// DateTimeOffset? DeletedAt
// string? DeletedBy
// bool IsDeleted
// bool IsHardDeleted
```

---

## Domain Events

```csharp
// Define — implement IDomainEvent
public record OrderCreatedEvent(Guid OrderId, string CustomerId) : IDomainEvent;

// Control publish timing
[DomainEventTiming(DomainEventTiming.BeforeSave)]
public record InventoryReservedEvent(Guid ProductId, int Qty) : IDomainEvent;

// Raise inside the aggregate
order.Raise(new OrderCreatedEvent(order.Id, customerId));

// Read and clear (after publishing)
IReadOnlyList<IDomainEvent> events = order.DomainEvents;  // property
order.ClearDomainEvents();
```

---

## Best Practices

- Call `Raise()` only inside entity methods — keep domain events encapsulated in the aggregate
- `DomainEvents` is a **property** — do not call `GetDomainEvents()` (doesn't exist)
- Audit fields are `UpdatedAt`/`UpdatedBy` — **not** `ModifiedAt`/`ModifiedBy`
- `MarkAsDeleted()` takes **two parameters**: `(DateTimeOffset deletedAt, string deletedBy)`
- Use `DomainEventTiming.BeforeSave` for events that must be processed before the transaction commits
