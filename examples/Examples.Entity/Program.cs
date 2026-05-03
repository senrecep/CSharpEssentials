using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Entity Example");
Console.WriteLine("========================================\n");

// ============================================================================
// ENTITY BASE
// ============================================================================
Console.WriteLine("--- EntityBase ---");

User user = new()
{
    FirstName = "Alice",
    LastName = "Smith",
    Email = "alice@example.com"
};

user.SetCreatedInfo(DateTimeOffset.UtcNow, "system");
user.SetUpdatedInfo(DateTimeOffset.UtcNow, "system");

Console.WriteLine($"User: {user.FirstName} {user.LastName}");
Console.WriteLine($"CreatedAt: {user.CreatedAt}");
Console.WriteLine($"CreatedBy: {user.CreatedBy}");
Console.WriteLine($"UpdatedAt: {user.UpdatedAt}");
Console.WriteLine($"UpdatedBy: {user.UpdatedBy}");
Console.WriteLine($"DomainEvents: {user.DomainEvents.Count}");
Console.WriteLine();

// ============================================================================
// SOFT DELETABLE ENTITY
// ============================================================================
Console.WriteLine("--- SoftDeletableEntityBase ---");

Product product = new()
{
    Name = "Laptop",
    Price = 1499.99m
};

product.SetCreatedInfo(DateTimeOffset.UtcNow, "system");
Console.WriteLine($"Product: {product.Name}, IsDeleted={product.IsDeleted}");

product.MarkAsDeleted(DateTimeOffset.UtcNow, "system");
Console.WriteLine($"After soft delete: IsDeleted={product.IsDeleted}, DeletedAt={product.DeletedAt}, DeletedBy={product.DeletedBy}");

product.Restore();
Console.WriteLine($"After restore: IsDeleted={product.IsDeleted}");
Console.WriteLine();

// ============================================================================
// HARD DELETE
// ============================================================================
Console.WriteLine("--- Hard Delete ---");

Product hardDeleteProduct = new()
{
    Name = "Temporary Item",
    Price = 9.99m
};

hardDeleteProduct.SetCreatedInfo(DateTimeOffset.UtcNow, "system");
Console.WriteLine($"Before hard delete: IsHardDeleted={hardDeleteProduct.IsHardDeleted}");

hardDeleteProduct.MarkAsHardDeleted();
Console.WriteLine($"After mark as hard deleted: IsHardDeleted={hardDeleteProduct.IsHardDeleted}");
Console.WriteLine();

// ============================================================================
// ENTITY WITH TYPED ID (Guid)
// ============================================================================
Console.WriteLine("--- EntityBase<Guid> ---");

Guid categoryId = Guid.NewGuid();
Category category = new(categoryId)
{
    Name = "Electronics"
};

Console.WriteLine($"Category Id: {category.Id}");
Console.WriteLine($"Category Name: {category.Name}");
Console.WriteLine();

// ============================================================================
// DOMAIN EVENTS
// ============================================================================
Console.WriteLine("--- Domain Events ---");

Order order = new() { OrderNumber = "ORD-001", Total = 250.00m };
order.Raise(new OrderCreatedEvent(order.OrderNumber));

Console.WriteLine($"Order: {order.OrderNumber}");
Console.WriteLine($"Events: {order.DomainEvents.Count}");

foreach (IDomainEvent evt in order.DomainEvents)
{
    Console.WriteLine($"  - {evt.GetType().Name}");
}

order.ClearDomainEvents();
Console.WriteLine($"After clear: {order.DomainEvents.Count}");
Console.WriteLine();

// ============================================================================
// INTERFACES
// ============================================================================
Console.WriteLine("--- Interfaces ---");

static void PrintAuditInfo(ICreationAudit entity)
{
    Console.WriteLine($"  CreatedAt: {entity.CreatedAt}");
    Console.WriteLine($"  CreatedBy: {entity.CreatedBy}");
}

static void PrintModificationInfo(IModificationAudit entity)
{
    Console.WriteLine($"  UpdatedAt: {entity.UpdatedAt}");
    Console.WriteLine($"  UpdatedBy: {entity.UpdatedBy}");
}

static void PrintSoftDeleteInfo(ISoftDeletable entity)
{
    Console.WriteLine($"  IsDeleted: {entity.IsDeleted}");
    Console.WriteLine($"  DeletedAt: {entity.DeletedAt}");
    Console.WriteLine($"  DeletedBy: {entity.DeletedBy}");
}

User auditUser = new() { FirstName = "Bob", LastName = "Jones", Email = "bob@example.com" };
auditUser.SetCreatedInfo(DateTimeOffset.UtcNow, "admin");
Console.WriteLine("ICreationAudit demo:");
PrintAuditInfo(auditUser);

Product softProduct = new() { Name = "Mouse", Price = 29.99m };
softProduct.MarkAsDeleted(DateTimeOffset.UtcNow, "admin");
Console.WriteLine("\nISoftDeletable demo:");
PrintSoftDeleteInfo(softProduct);

User modUser = new() { FirstName = "Charlie", LastName = "Brown", Email = "charlie@example.com" };
modUser.SetCreatedInfo(DateTimeOffset.UtcNow, "system");
modUser.SetUpdatedInfo(DateTimeOffset.UtcNow, "admin");
Console.WriteLine("\nIModificationAudit demo:");
PrintModificationInfo(modUser);
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");

// ============================================================================
// MODELS
// ============================================================================

public class User : EntityBase
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class Product : SoftDeletableEntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class Category : EntityBase<Guid>
{
    public Category(Guid id)
    {
        Id = id;
    }

    public string Name { get; set; } = string.Empty;
}

public class Order : EntityBase
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public record OrderCreatedEvent(string OrderNumber) : IDomainEvent;
