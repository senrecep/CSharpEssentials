---
name: csharpessentials-efcore
description: Use when wiring EF Core with CSharpEssentials domain models — AuditInterceptor/DomainEventInterceptor/SlowQueryInterceptor registered via DI, BaseDbContext (InterceptorsFromServices, DispatchDomainEventsOnSaveChanges), PaginateAsync/Paginate, batch SoftDeleteAsync, Result queries (FirstOrDefaultAsResultAsync, SaveChangesAsResultAsync), ConfigureEnumConventions for [StringEnum] storage and AddCqrsDbContexts.
---

# CSharpEssentials.EntityFrameworkCore

EF Core interceptors, a base `DbContext`, pagination, soft delete and Result-returning queries that integrate with `EntityBase` and domain events.

## Installation

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

Targets `net10.0` (EF Core 10), `net9.0` (EF Core 9) and `net8.0` (EF Core 8). There is no `net11.0` target.

## Namespaces

```csharp
using CSharpEssentials.EntityFrameworkCore;                       // BaseDbContext, DbContextInterceptors, SoftDeleteAsync, *AsResultAsync
using CSharpEssentials.EntityFrameworkCore.Interceptors;          // interceptors, IAuditUserIdProvider, IDomainEventPublisher, ISlowQueryHandler
using CSharpEssentials.EntityFrameworkCore.Pagination;            // PaginateAsync, Paginate
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;   // PaginationRequest, CursorPaginationRequest<T>
using CSharpEssentials.EntityFrameworkCore.Extensions;            // AddCqrsDbContexts, AddWriteDbContext, AddReadDbContext
```

---

## Interceptors

Interceptors are resolved from DI. Register them, then attach them to the context:

```csharp
builder.Services.AddAuditInterceptor(sp =>
    sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User.Identity?.Name ?? "system");
builder.Services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));
builder.Services.AddSingleton<DomainEventInterceptor>();
builder.Services.AddScoped<IDomainEventPublisher, MediatorDomainEventPublisher>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlite("Data Source=app.db")
           .AddInterceptors(
               sp.GetRequiredService<AuditInterceptor>(),
               sp.GetRequiredService<DomainEventInterceptor>(),
               sp.GetRequiredService<SlowQueryInterceptor>()));
```

### AuditInterceptor

Sets `CreatedAt`/`CreatedBy` and `UpdatedAt`/`UpdatedBy` on `EntityBase` entries during `SaveChanges`. `AddAuditInterceptor` registers both the interceptor and an `IAuditUserIdProvider` from a factory; implement the interface yourself for anything more complex:

```csharp
public class HttpAuditUserIdProvider(IHttpContextAccessor accessor) : IAuditUserIdProvider
{
    public string GetCurrentUserId() =>
        accessor.HttpContext?.User.Identity?.Name ?? "system";
}
```

### DomainEventInterceptor

Publishes `IDomainEvent`s raised on entities through `IDomainEventPublisher` (or `IDomainEventOutbox` for after-save events, when registered):

```csharp
public class MediatorDomainEventPublisher(ILogger<MediatorDomainEventPublisher> logger) : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Publishing {Event}", domainEvent.GetType().Name);
        return Task.CompletedTask;
    }
}
```

### SlowQueryInterceptor

Logs commands slower than `SlowQueryOptions.Threshold` (default 1 second). Register an optional `ISlowQueryHandler` for metrics or alerts:

```csharp
public class SlowQueryMetrics(ILogger<SlowQueryMetrics> logger) : ISlowQueryHandler
{
    public void OnSlowQuery(SlowQueryContext context) =>
        logger.LogWarning("Slow query ({Elapsed} ms): {Sql}", context.ElapsedTime.TotalMilliseconds, context.CommandText);
}
```

---

## BaseDbContext

Opt-in interceptor attachment from DI and domain event dispatch around `SaveChanges`:

```csharp
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, IServiceScopeFactory scopeFactory)
    : BaseDbContext<AppDbContext>(options, scopeFactory)
{
    protected override DbContextInterceptors InterceptorsFromServices =>
        DbContextInterceptors.Audit | DbContextInterceptors.SlowQuery;

    protected override bool DispatchDomainEventsOnSaveChanges => true;
}
```

- Both hooks are off by default. Interceptors not registered in DI are skipped.
- With `DispatchDomainEventsOnSaveChanges`, `BeforeSave` events are published before the save and `AfterSave` events after it succeeds; on a failed save they are put back on their entities. Use it or `DomainEventInterceptor`, not both.
- If you override `OnConfiguring`, call `base.OnConfiguring(optionsBuilder)`.

---

## Enum Storage (4.0 breaking change)

`ConfigureEnumConventions` stores every `[StringEnum]` enum as a string using the same naming as JSON (`StringEnumNaming`, snake_case by default). 3.x stored names with Core `ToSnakeCase`; set `UseLegacySnakeCase` to keep reading existing data:

```csharp
public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureEnumConventions(typeof(ShopDbContext).Assembly);

        // 3.x storage format for existing data:
        // configurationBuilder.ConfigureEnumConventions(o => o.UseLegacySnakeCase = true, typeof(ShopDbContext).Assembly);
    }
}
```

---

## Pagination

```csharp
// Offset-based
PaginationResponse<Order> page = await db.Orders
    .OrderByDescending(o => o.CreatedAt)
    .PaginateAsync(pageNumber: 1, pageSize: 20, cancellationToken: ct);

IReadOnlyList<Order> items = page.Items;
int total = page.TotalCount;   // -1 when includeTotalCount: false
bool more = page.HasNextPage;  // also PageNumber, PageSize, TotalPages, HasPreviousPage

// Request object with optional search
var request = new PaginationRequest { PageNumber = 2, PageSize = 10, Search = "pen" };
PaginationResponse<Product> products = await db.Products
    .PaginateAsync(request, search: term => p => p.Name.Contains(term), cancellationToken: ct);

// Cursor-based
CursorPaginationResponse<Order, DateTimeOffset> feed = await db.Orders.PaginateAsync(
    new CursorPaginationRequest<DateTimeOffset> { Limit = 20 },
    cursorSelector: o => o.CreatedAt);
```

`Paginate` is the synchronous variant and also works on in-memory `IQueryable<T>`.

---

## Soft Delete and Result Queries

```csharp
// One UPDATE statement; bypasses the change tracker and SaveChanges interceptors
int affected = await db.Set<Article>()
    .Where(a => a.Title.StartsWith("draft"))
    .SoftDeleteAsync(deletedBy: "admin", cancellationToken: ct);

// Result-returning queries: Error.NotFound() (or your error) instead of null
Result<Order> order = await db.Orders.Where(o => o.Total > 100).FirstOrDefaultAsResultAsync(cancellationToken: ct);
Result<Order> byKey = await db.Orders.FindAsResultAsync([orderId], cancellationToken: ct);
Result saved = await db.SaveChangesAsResultAsync(ct);
```

Add a global filter for soft-deleted rows with `modelBuilder.ApplySoftDeleteQueryFilter()` in `OnModelCreating`.

---

## CQRS Registration

```csharp
builder.Services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(
    configureWrite: (sp, options) => options.UseSqlite("Data Source=app.db"));
```

The write context is pooled with change tracking; the read context is pooled with `NoTracking`. `AddWriteDbContext<T>` and `AddReadDbContext<T>` register them one at a time.

---

## Best Practices

- Pick one domain event path: `DomainEventInterceptor` or `BaseDbContext.DispatchDomainEventsOnSaveChanges`
- `SoftDeleteAsync` skips audit and domain events — use `MarkAsDeleted` + `SaveChanges` when those must run
- `PaginateAsync` issues a COUNT and a data query; pass `includeTotalCount: false` to skip the COUNT
- Upgrading from 3.x: enum columns change format unless you set `UseLegacySnakeCase = true`
