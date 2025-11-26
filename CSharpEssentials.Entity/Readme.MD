# CSharpEssentials.Entity

`CSharpEssentials.Entity` provides a robust foundation for Domain-Driven Design (DDD) entities in .NET. It offers base classes for auditing, soft deletion, and domain event management, helping you build standard and maintainable domain models.

## 🚀 Features

- **Audit Logging**: Built-in tracking for `CreatedAt`, `CreatedBy`, `UpdatedAt`, and `UpdatedBy`.
- **Soft Delete**: Standardized soft deletion support via `SoftDeletableEntityBase`.
- **Domain Events**: Built-in container for raising and clearing domain events.
- **Flexible Generics**: Support for strongly-typed IDs (e.g., `Guid`, `int`, `long`).

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Entity
```

## 🛠 Usage

### 1. Basic Entity

Inherit from `EntityBase<TId>` to get standard audit fields and ID management.

```csharp
using CSharpEssentials.Entity;

public class Product : EntityBase<Guid>
{
    public string Name { get; set; }
    
    public Product(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}
```

### 2. Soft Deletable Entity

Use `SoftDeletableEntityBase<TId>` to add soft delete capabilities (IsDeleted, DeletedAt, DeletedBy).

```csharp
using CSharpEssentials.Entity;

public class User : SoftDeletableEntityBase<Guid>
{
    public string Username { get; set; }
}

// Usage
var user = new User();

// Mark as deleted (doesn't remove from DB, just flags it)
user.MarkAsDeleted(deletedAt: DateTimeOffset.UtcNow, deletedBy: "Admin");

// Restore if needed
user.Restore();
```

### 3. Domain Events

Entities can raise events that can be dispatched later (e.g., by your persistence layer).

```csharp
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;

public record ProductCreated(Guid ProductId) : IDomainEvent;

public class Product : EntityBase<Guid>
{
    public Product()
    {
        Id = Guid.NewGuid();
        // Raise an event
        Raise(new ProductCreated(Id));
    }
}

// Accessing events (e.g., in EF Core SaveChanges)
foreach (var domainEvent in product.DomainEvents)
{
    // Dispatch event...
}
product.ClearDomainEvents();
```

### 4. Auditing

The base classes expose methods to set audit info, typically called by your repository or DbContext.

```csharp
var product = new Product("Laptop");

// Set creation info
product.SetCreatedInfo(DateTimeOffset.UtcNow, "User123");

// Set update info
product.SetUpdatedInfo(DateTimeOffset.UtcNow, "User456");
```
