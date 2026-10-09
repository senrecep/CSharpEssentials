# CSharpEssentials.Entity

DDD foundation for .NET entities with auditing, soft deletion, and domain events.

## Features

- **`EntityBase<TId>`**: Typed IDs with creation and modification audit fields. `EntityBase` is the same without an ID.
- **`SoftDeletableEntityBase<TId>`**: Soft delete with `MarkAsDeleted`, `Restore`, and `MarkAsHardDeleted`. `SoftDeletableEntityBase` is the same without an ID.
- **Domain Events**: Raise and clear events for outbox or dispatcher patterns.
- **Event Timing**: Control when events publish with `[DomainEventTiming]`.
- **Interfaces**: `IEntityBase`, `IEntityBase<TId>`, `ISoftDeletable`, `ICreationAudit`, `IModificationAudit` and `IDomainEventHolder` (namespace `CSharpEssentials.Entity.Interfaces`) for code that works with any entity.

## Installation

```bash
dotnet add package CSharpEssentials.Entity
```

## Usage

### Basic Entity

```csharp
using CSharpEssentials.Entity;

public class Product : EntityBase<Guid>
{
    public Product(Guid id) => Id = id; // Id has a protected setter

    public string Name { get; set; } = string.Empty;
}

var product = new Product(Guid.NewGuid()) { Name = "Laptop" };
product.SetCreatedInfo(DateTimeOffset.UtcNow, "system");
product.SetUpdatedInfo(DateTimeOffset.UtcNow, "admin");
```

### Soft Deletable Entity

```csharp
public class User : SoftDeletableEntityBase<Guid>
{
    public string Name { get; set; } = string.Empty;
}

var user = new User();
user.MarkAsDeleted(DateTimeOffset.UtcNow, "Admin"); // IsDeleted, DeletedAt, DeletedBy
user.Restore();                                     // clears IsDeleted, DeletedAt, DeletedBy and IsHardDeleted
```

`MarkAsHardDeleted()` sets `IsHardDeleted`, which tells the EF Core `AuditInterceptor` to remove the row instead of converting the delete into a soft delete. The `HardDelete()` extension (on `IEnumerable<T>` where `T : ISoftDeletable`) marks every item. The audit properties (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `DeletedAt`, `DeletedBy`) have private setters; set them with `SetCreatedInfo`, `SetUpdatedInfo` and `MarkAsDeleted`. `SetCreatedInfo` throws `ArgumentOutOfRangeException` for `DateTimeOffset.MinValue`.

### Domain Events

```csharp
public record OrderCreated(Guid OrderId) : IDomainEvent;

public class Order : EntityBase<Guid>
{
    public void Create()
    {
        Id = Guid.NewGuid();
        Raise(new OrderCreated(Id));
    }
}

var order = new Order();
order.Create();

// Later (e.g., in EF Core SaveChanges)
foreach (var evt in order.DomainEvents)
{
    // dispatch...
}
order.ClearDomainEvents();
```

`DomainEvents` returns a snapshot, so raising or clearing events while you loop over it is safe. The EF Core package collects the events of tracked entities and dispatches them; see `CSharpEssentials.EntityFrameworkCore`.

### Event Timing

```csharp
// AfterSave (default): published after the save
public record OrderCreatedEvent(Guid OrderId) : IDomainEvent;

// Published before the save; a handler that throws aborts it
[DomainEventTiming(DomainEventTiming.BeforeSave)]
public record OrderValidationEvent(Guid OrderId) : IDomainEvent;
```

The attribute only labels the event; `CSharpEssentials.EntityFrameworkCore` reads it. `BaseDbContext` with `DispatchDomainEventsOnSaveChanges` publishes `AfterSave` events once the save has succeeded. `DomainEventInterceptor` dispatches them inside `SavingChanges`, still before the database write, so that an outbox can store them in the same save.
