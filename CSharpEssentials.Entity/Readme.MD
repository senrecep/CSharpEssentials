# CSharpEssentials.Entity

DDD foundation for .NET entities with auditing, soft deletion, and domain events.

## Features

- **EntityBase<TId>** — Typed IDs with creation and modification audit fields.
- **SoftDeletableEntityBase** — Soft delete with `MarkAsDeleted`, `Restore`, and `MarkAsHardDeleted`.
- **Domain Events** — Raise and clear events for outbox or dispatcher patterns.
- **Event Timing** — Control when events publish with `[DomainEventTiming]`.

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
    public string Name { get; set; } = string.Empty;
}

var product = new Product { Id = Guid.NewGuid(), Name = "Laptop" };
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
user.MarkAsDeleted(DateTimeOffset.UtcNow, "Admin");
user.Restore();
```

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

// Later (e.g., in EF Core SaveChanges)
foreach (var evt in order.DomainEvents)
{
    // dispatch...
}
order.ClearDomainEvents();
```

### Event Timing

```csharp
// Published after transaction commits (default)
public record OrderCreatedEvent(Guid OrderId) : IDomainEvent;

// Published before transaction commits
[DomainEventTiming(DomainEventTiming.BeforeSave)]
public record OrderValidationEvent(Guid OrderId) : IDomainEvent;
```
