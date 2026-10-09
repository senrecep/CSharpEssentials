# CSharpEssentials.EntityFrameworkCore Example

This console application demonstrates `CSharpEssentials.EntityFrameworkCore` on SQLite: soft delete, the audit, domain event and slow query interceptors, offset and keyset pagination, and enum storage.

## Features Demonstrated

| Feature | File | Description |
|---------|------|-------------|
| **BaseDbContext** | `Data/ShopDbContext.cs` | `ShopDbContext : BaseDbContext<ShopDbContext>` logs the context lifecycle and keeps a service scope; it also applies the enum conventions and the soft-delete filter |
| **Soft Delete** | `Data/Entities/Product.cs`, `Services/ProductCatalogService.cs` | `Product : SoftDeletableEntityBase<Guid>`; `MarkAsDeleted` hides the row through a query filter (`HasQueryFilter(e => !e.IsDeleted)`, added by reflection in `ShopDbContext`) |
| **Audit Interceptor** | `Program.cs` | `AddAuditInterceptor(() => "demo-user")`, attached with `AddInterceptors`; sets `CreatedAt`/`CreatedBy` and `UpdatedAt`/`UpdatedBy` on `SaveChanges` |
| **Domain Event Interceptor** | `Program.cs`, `Services/ConsoleDomainEventPublisher.cs`, `Data/Events/` | `DomainEventInterceptor` publishes entity events through `IDomainEventPublisher`; `ProductCreatedEvent` has `[DomainEventTiming(BeforeSave)]`, `ProductPriceChangedEvent` uses the `AfterSave` default |
| **Slow Query Interceptor** | `Program.cs` | `AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500))` |
| **Pagination** | `Services/ProductCatalogService.cs`, `Program.cs` | Offset-based `PaginateAsync()` and keyset `KeysetPaginateAsync()` |
| **Enum Storage** | `Data/ShopDbContext.cs`, `Data/Entities/Product.cs` | `ConfigureEnumConventions(EnumConventions.Default)` stores the `[StringEnum]` enum `ProductCategory` by its wire name and adds a check constraint |

The interceptors are registered in DI and attached to the context in `Program.cs`. `ShopDbContext` does not override `InterceptorsFromServices` or `DispatchDomainEventsOnSaveChanges`, so `BaseDbContext` does not attach them itself. The example does not use a naming convention: the `Product` table is named `products` explicitly and the columns keep their property names.

## Running the Project

```bash
cd examples/Examples.EntityFrameworkCore
dotnet run
```

The demo performs the following steps automatically:

1. **Database Creation**: Deletes and creates a local SQLite database (`shop.db`).
2. **Seeding**: Inserts 3 initial products.
3. **Soft Delete Demo**: Soft-deletes the "Wireless Mouse" product, then shows:
   - Visible products (filtered): 2
   - Total products (with deleted): 3
4. **Offset Pagination Demo**: Adds 25 more products and prints pages 1 and 2 of 5 items.
5. **Cursor Pagination Demo**: Reads two pages of 3 products with `KeysetPaginateAsync` and `KeysetPaginationRequest`.
6. **Audit Interceptor Demo**: Modifies a product and shows `UpdatedAt` and `UpdatedBy` being set automatically.
7. **Domain Event Interceptor Demo**: Saves products that raise events; the console publisher prints `[BeforeSave]` and `[AfterSave]` lines. The `AfterSave` events are handled inside `SavingChanges`, before the database write.
8. **Enum Demo**: Reads a product by its `ProductCategory`; the column holds the wire name (see below).

## BaseDbContext

`ShopDbContext` inherits from `BaseDbContext`:

```csharp
public class ShopDbContext : BaseDbContext<ShopDbContext>
{
    public ShopDbContext(
        DbContextOptions<ShopDbContext> options,
        IServiceScopeFactory serviceScopeFactory) : base(options, serviceScopeFactory) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.ConfigureEnumConventions(EnumConventions.Default);
    }
}
```

## Soft Delete Entity

```csharp
public class Product : SoftDeletableEntityBase<Guid>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
}
```

Deleting a product (`ProductCatalogService.DeleteProduct`):

```csharp
product.MarkAsDeleted(DateTimeOffset.UtcNow, "system");
_dbContext.SaveChanges();
// Product.IsDeleted = true, row remains in database
```

`Products.Remove(product)` has the same effect when the `AuditInterceptor` is attached: it turns a delete of an `ISoftDeletable` entity into a soft delete. The package also ships `modelBuilder.ApplySoftDeleteQueryFilter()`, which this example replaces with its own reflection loop.

Querying without soft-delete filter:

```csharp
var all = db.Products.IgnoreQueryFilters().ToList();
```

## Pagination

### Offset-based

```csharp
var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

var page = await _dbContext.Products
    .OrderBy(p => p.Name)
    .PaginateAsync(request);

// page.Items        -> IReadOnlyList<Product> for current page
// page.TotalCount   -> Total items across all pages
// page.TotalPages   -> Total number of pages
// page.HasNextPage  -> bool
```

### Keyset (cursor) pagination

Ideal for infinite scroll or very large datasets. The key must be unique, so add the id as a tie-breaker.

```csharp
var page = await _dbContext.Products.KeysetPaginateAsync(
    new KeysetPaginationRequest { Limit = 20, After = nextCursor },
    keys => keys.Ascending(p => p.Name).Ascending(p => p.Id));

// page.Value.Items      -> rows of this page
// page.Value.NextCursor -> pass as After to get the next page
// page.Value.HasNext    -> bool
```

## Enum String Conversion

`ConfigureEnumConventions` stores `[StringEnum]` values by their wire name through `EnumWireNameConverter<TEnum>`:

| Enum Value | Stored in DB |
|------------|--------------|
| `ProductCategory.Electronics` | `"electronics"` |
| `ProductCategory.Clothing` | `"clothing"` |
| `ProductCategory.Food` | `"food"` |

This makes the database self-documenting and avoids magic numbers. The convention also adds a check constraint, so the database rejects unknown values:

```sql
CONSTRAINT "ck_products_Category_enum" CHECK ("Category" IN ('electronics', 'clothing', 'food', 'books', 'home'))
```

The stored value is the wire name, the same spelling as JSON and query binding. For an existing database whose enum columns hold integers, keep them and opt in per column:

```csharp
configurationBuilder.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer);
```
