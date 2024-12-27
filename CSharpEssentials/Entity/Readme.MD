# Entity Module

## Core Interfaces

### `IEntityBase`

Base interface for all entities, providing essential functionality:

- Domain event handling
- Creation and modification audit

### `IDomainEvent` & `IDomainEventHolder`

Support for domain events:

- `IDomainEvent`: Marker interface for domain events
- `IDomainEventHolder`: Manages domain events collection and operations

### Audit Interfaces

- `ICreationAudit`: Tracks entity creation (CreatedAt, CreatedBy)
- `IModificationAudit`: Tracks entity modifications (UpdatedAt, UpdatedBy)
- `ISoftDeletable`: Manages soft deletion state and operations

## Abstract Classes

### `EntityBase`

```csharp
public class MyEntity : EntityBase
{
    public string Name { get; private set; }

    public void UpdateName(string newName)
    {
        Name = newName;
        Raise(new NameUpdatedEvent(newName));
    }
}

// Usage
var entity = new MyEntity();
entity.SetCreatedInfo(DateTimeOffset.UtcNow, "system");
entity.UpdateName("New Name");
var events = entity.DomainEvents; // Access domain events
```

Features:

- Domain event management
- Audit trail (creation and modification)
- Immutable properties with controlled setters

### `SoftDeletableEntityBase`

```csharp
public class MySoftDeletableEntity : SoftDeletableEntityBase
{
    public string Title { get; private set; }

    public void Archive(string deletedBy)
    {
        MarkAsDeleted(DateTimeOffset.UtcNow, deletedBy);
        Raise(new EntityArchivedEvent(Id));
    }
}

// Usage
var entity = new MySoftDeletableEntity();
entity.Archive("admin");
if (entity.IsDeleted)
{
    // Entity is soft deleted
}
entity.Restore(); // Bring it back
```

Features:

- Inherits all `EntityBase` functionality
- Soft deletion support
- Hard deletion flag
- Restoration capability

## Generic Variants

### `EntityBaseTId<TId>`

```csharp
public class CustomEntity : EntityBaseTId<Guid>
{
    public CustomEntity(Guid id)
    {
        Id = id;
    }
}
```

### `SoftDeletableEntityBaseTId<TId>`

```csharp
public class CustomSoftDeletableEntity : SoftDeletableEntityBaseTId<int>
{
    public CustomSoftDeletableEntity(int id)
    {
        Id = id;
    }
}
```

## Best Practices

1. Always use private setters for properties
2. Raise domain events for significant state changes
3. Use the audit trail capabilities for tracking changes
4. Prefer soft deletion over hard deletion when possible
5. Implement custom domain logic through methods, not property setters

## Functional Programming Features

1. Immutable state through private setters
2. Event sourcing support via domain events
3. Pure functions for state modifications
4. Side-effect isolation in domain event handlers

## Example: Complete Entity Implementation

```csharp
public class Product : SoftDeletableEntityBase
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    private Product(string name, decimal price)
    {
        Name = name;
        Price = price;
        Stock = 0;
    }

    public static Product Create(string name, decimal price)
    {
        var product = new Product(name, price);
        product.Raise(new ProductCreatedEvent(product));
        return product;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new ArgumentException("Price must be positive");

        var oldPrice = Price;
        Price = newPrice;

        Raise(new ProductPriceUpdatedEvent(Id, oldPrice, newPrice));
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");

        Stock += quantity;
        Raise(new StockAddedEvent(Id, quantity));
    }
}

// Usage
var product = Product.Create("Laptop", 999.99m);
product.SetCreatedInfo(DateTimeOffset.UtcNow, "system");
product.UpdatePrice(899.99m);
product.AddStock(10);
```

This implementation showcases:

- Factory method pattern
- Domain event usage
- Immutable state management
- Business rule enforcement
- Audit trail tracking
