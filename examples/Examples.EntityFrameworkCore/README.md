# CSharpEssentials.EntityFrameworkCore Example

This console application demonstrates the complete feature set of `CSharpEssentials.EntityFrameworkCore` including soft delete, audit interceptors, pagination, enum conversion, and naming conventions.

## Features Demonstrated

| Feature | File | Description |
|---------|------|-------------|
| **BaseDbContext** | `Data/ShopDbContext.cs` | Automatic configuration discovery, soft-delete filters |
| **Soft Delete** | `Data/Entities/Product.cs` | Entities are flagged, not removed; filtered from queries |
| **Audit Interceptor** | Built into BaseDbContext | Auto-sets `CreatedAt` and `UpdatedAt` |
| **Domain Event Interceptor** | Built into BaseDbContext | Dispatches entity domain events BeforeSave / AfterSave |
| **Pagination** | `Services/ProductCatalogService.cs` | Offset-based `ToPaginatedList()` and cursor-based `PaginateAsync()` |
| **Enum to String** | `Data/ShopDbContext.cs` | Stores enums as snake_case strings in the database |
| **Snake Case Naming** | `Data/ShopDbContext.cs` | Automatic `PascalCase` -> `snake_case` conversion |

## Running the Project

```bash
cd examples/Examples.EntityFrameworkCore
dotnet run
```

The demo performs the following steps automatically:

1. **Database Creation** — Creates a local SQLite database (`shop.db`).
2. **Seeding** — Inserts 3 initial products.
3. **Soft Delete Demo** — Deletes the "Wireless Mouse" product, then shows:
   - Visible products (filtered): 2
   - Total products (with deleted): 3
4. **Pagination Demo** — Adds 25 more products and demonstrates page navigation.
5. **Enum Conversion Demo** — Shows how `ProductCategory` is stored as strings.
6. **Audit Interceptor Demo** — Modifies a product and observes `UpdatedAt` being set automatically.
7. **Cursor Pagination Demo** — Efficient pagination for large datasets using `CursorPaginationRequest<T>`.
8. **Domain Event Interceptor Demo** — Dispatches events raised by entities during `SaveChanges`.

## BaseDbContext

`ShopDbContext` inherits from `BaseDbContext`:

```csharp
public class ShopDbContext : BaseDbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Convert table/column names to snake_case automatically
        modelBuilder.UseSnakeCaseNamingConvention();

        // Store enums as snake_case strings (e.g. "electronics")
        modelBuilder.UseEnumToStringConversion(StringCase.SnakeCase);
    }
}
```

## Soft Delete Entity

```csharp
public class Product : SoftDeletableEntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
}
```

Deleting a product:

```csharp
_dbContext.Products.Remove(product);
_dbContext.SaveChanges();
// Product.IsDeleted = true, row remains in database
```

Querying without soft-delete filter:

```csharp
var all = db.Products.IgnoreQueryFilters().ToList();
```

## Pagination

### Offset-based

```csharp
var page = _dbContext.Products
    .OrderBy(p => p.Name)
    .ToPaginatedList(pageNumber: 1, pageSize: 10);

// page.Items        -> List<Product> for current page
// page.TotalCount   -> Total items across all pages
// page.TotalPages   -> Total number of pages
// page.HasNextPage  -> bool
```

### Cursor-based (High Performance)

Ideal for infinite scroll or very large datasets.

```csharp
var request = new CursorPaginationRequest<DateTimeOffset>
{
    Limit = 20,
    Cursor = lastSeenDate
};

var response = await _dbContext.Logs
    .PaginateAsync(request, cursorSelector: x => x.CreatedAt);

// response.NextCursor -> Continue from here
```

## Enum String Conversion

The `EnumToStringConverter` stores enum values as human-readable strings:

| Enum Value | Stored in DB |
|------------|--------------|
| `ProductCategory.Electronics` | `"electronics"` |
| `ProductCategory.Clothing` | `"clothing"` |
| `ProductCategory.Food` | `"food"` |

This makes the database self-documenting and avoids magic numbers.
