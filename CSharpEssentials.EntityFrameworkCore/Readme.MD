# CSharpEssentials.EntityFrameworkCore

`CSharpEssentials.EntityFrameworkCore` offers a set of extensions, interceptors, and base classes to enhance Entity Framework Core development. It includes robust pagination support, slow query logging, and simplified DbContext configuration.

## 🚀 Features

- **Advanced Pagination**: Extension methods for **Offset-based** and **Cursor-based** pagination on `IQueryable`.
- **Slow Query Logging**: `SlowQueryInterceptor` to automatically log queries exceeding a specified threshold.
- **BaseDbContext**: A base class (`BaseDbContext<T>`) with built-in logging and scope management.
- **Model Configuration**: Extensions to apply configurations easily.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

## 🛠 Usage

### 1. Pagination (Offset-based)

Easily paginate any `IQueryable`.

```csharp
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;

var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

var response = await dbContext.Users
    .PaginateAsync(request, cancellationToken: ct);

// Access data
var items = response.Data;
var total = response.TotalCount;
```

### 2. Cursor Pagination (High Performance)

Use cursor-based pagination for infinite scroll scenarios or large datasets where offset pagination is slow.

```csharp
using CSharpEssentials.EntityFrameworkCore.Pagination;

var request = new CursorPaginationRequest<DateTimeOffset> 
{ 
    Limit = 20, 
    Cursor = lastSeenDate 
};

var response = await dbContext.Logs
    .PaginateAsync(
        request, 
        cursorSelector: x => x.CreatedAt, // Order by CreatedAt
        isAscending: false // Newest first
    );

var nextCursor = response.NextCursor;
```

### 3. Slow Query Logging

Register the interceptor to find performance bottlenecks.

```csharp
using CSharpEssentials.EntityFrameworkCore.Interceptors;

services.AddDbContext<MyDbContext>((sp, options) =>
{
    var logger = sp.GetRequiredService<ILogger<SlowQueryInterceptor>>();
    options.AddInterceptors(new SlowQueryInterceptor(logger));
});
```

Queries taking longer than the threshold (default 1s) will be logged as Warnings.

### 4. Base DbContext

Inherit from `BaseDbContext` for automatic logger injection and instance tracking.

```csharp
using CSharpEssentials.EntityFrameworkCore;

public class MyDbContext : BaseDbContext<MyDbContext>
{
    public MyDbContext(DbContextOptions<MyDbContext> options, IServiceScopeFactory scopeFactory) 
        : base(options, scopeFactory)
    {
    }
}
```
