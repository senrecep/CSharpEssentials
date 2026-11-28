---
name: csharpessentials-entity
description: Use when writing DDD entities or aggregate roots. Covers EntityBase/EntityBase<TId> (audit fields via SetCreatedInfo/SetUpdatedInfo, domain events via Raise/DomainEvents/ClearDomainEvents), SoftDeletableEntityBase<TId> (MarkAsDeleted/Restore/MarkAsHardDeleted, HardDelete()), the DomainEvent record (EventId, OccurredAt from a TimeProvider), IDomainEvent and [DomainEventTiming(BeforeSave|AfterSave)].
---

# CSharpEssentials.Entity

Base classes for entities with a typed id, audit fields, soft delete and domain events. The EF Core package (`csharpessentials-efcore` skill) fills the audit fields and dispatches the events on save.

```bash
dotnet add package CSharpEssentials.Entity
```

```csharp
using CSharpEssentials.Entity;            // EntityBase, EntityBase<TId>, SoftDeletableEntityBase<TId>, DomainEvent, DomainEventTiming, HardDelete()
using CSharpEssentials.Entity.Interfaces; // IDomainEvent, IIdentifiedDomainEvent, ISoftDeletable, IEntityBase, ICreationAudit, IModificationAudit
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Entity
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;

Order order = Order.Place("customer-1", 120m);
order.SetCreatedInfo(DateTimeOffset.UtcNow, "alice");
order.ChangeTotal(150m);

IDomainEvent[] events = [.. order.DomainEvents];   // copy, then clear
order.ClearDomainEvents();
Console.WriteLine($"{events.Length} events, {order.DomainEvents.Count} left");   // 2 events, 0 left

order.MarkAsDeleted(DateTimeOffset.UtcNow, "alice");
Console.WriteLine(order.IsDeleted);                 // True
order.Restore();
Console.WriteLine(order.IsDeleted);                 // False

public sealed record OrderPlaced(Guid OrderId) : DomainEvent;

[DomainEventTiming(DomainEventTiming.BeforeSave)]
public sealed record OrderTotalChanged(Guid OrderId, decimal Total) : IDomainEvent;

public sealed class Order : SoftDeletableEntityBase<Guid>
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
```

## Choosing a base class

| Need | Derive from |
|---|---|
| Typed id, audit, events | `EntityBase<TId>` (`TId : IEquatable<TId>`, for example `Guid`, `int`, `long`, `string`) |
| The same without an id property | `EntityBase` |
| Plus soft delete | `SoftDeletableEntityBase<TId>` or `SoftDeletableEntityBase` |

`Id` is `TId? Id { get; protected set; }`. Assign it inside the class (constructor or factory method).

## Audit fields

```csharp
entity.SetCreatedInfo(DateTimeOffset createdAt, string createdBy);   // CreatedAt, CreatedBy
entity.SetUpdatedInfo(DateTimeOffset updatedAt, string updatedBy);   // UpdatedAt, UpdatedBy (null until set)
```

Setters are private. `SetCreatedInfo` throws `ArgumentOutOfRangeException` for `DateTimeOffset.MinValue`. With EF Core, `AuditInterceptor` calls both on save.

## Soft delete

```csharp
entity.MarkAsDeleted(DateTimeOffset.UtcNow, "alice");   // IsDeleted, DeletedAt, DeletedBy
entity.Restore();                                      // clears all four soft delete fields
entity.MarkAsHardDeleted();                            // IsHardDeleted: EF Core removes the row instead of soft-deleting it
entities.HardDelete();                                 // extension on IEnumerable<T> where T : ISoftDeletable
```

## Domain events

```csharp
public sealed record PriceChanged(Guid ProductId, decimal Price) : IDomainEvent;   // plain event
public sealed record OrderPlaced(Guid OrderId) : DomainEvent;                      // with EventId + OccurredAt
public sealed record OrderShipped : DomainEvent                                    // with a test clock
{
    public OrderShipped(Guid orderId, TimeProvider clock) : base(clock) => OrderId = orderId;
    public Guid OrderId { get; init; }
}
```

- `DomainEvent` sets `OccurredAt = timeProvider.GetUtcNow()` (default `TimeProvider.System`) and `EventId = Guider.NewGuid(OccurredAt)` (version 7). Both are `init`, so JSON round trips keep them. Use it for events an outbox stores.
- `[DomainEventTiming(DomainEventTiming.BeforeSave)]`: dispatched before the write; a throwing handler aborts the save. No attribute means `AfterSave`. The attribute is not inherited.

## Rules

- `DomainEvents` is a property. There is no `GetDomainEvents()` method.
- `DomainEvents` is a live view. Copy it (`[.. entity.DomainEvents]`) before `ClearDomainEvents()` or before raising inside a loop over it; otherwise you get an empty list or `InvalidOperationException`.
- `Raise(null)` throws `ArgumentNullException`.
- Call `Raise` inside entity methods, not from services.
- Audit names are `UpdatedAt`/`UpdatedBy`, never `ModifiedAt`/`ModifiedBy`.
- `MarkAsDeleted` takes two arguments `(DateTimeOffset deletedAt, string deletedBy)`.
- Do not assign `CreatedAt`, `DeletedAt` and the other audit properties directly: the setters are private.
- A type of your own named `DomainEvent` conflicts with `CSharpEssentials.Entity.DomainEvent` (CS0104) when both namespaces are imported. Use an alias.
- The attribute only labels events. Publishing needs `BaseDbContext.DispatchDomainEventsOnSaveChanges` or `DomainEventInterceptor` from `CSharpEssentials.EntityFrameworkCore`.

## Interfaces

`ICreationAudit`, `IModificationAudit`, `IDomainEventHolder`, `IEntityBase`, `IEntityBase<TId>`, `ISoftDeletableBase` (`IsDeleted`), `ISoftDeletable`, `ISoftDeletableEntityBase(<TId>)`, `IDeletableEntityBase(<TId>)`, `IDomainEvent`, `IIdentifiedDomainEvent` (`EventId`, `OccurredAt`). The EF Core interceptors and query filters work against these interfaces, so your own classes can implement them instead of deriving from the base classes.

The package reports no diagnostics.
