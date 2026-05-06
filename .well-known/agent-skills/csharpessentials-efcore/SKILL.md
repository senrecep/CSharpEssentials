---
name: csharpessentials-efcore
description: Use when wiring EF Core with CSharpEssentials domain models — AuditInterceptor for automatic CreatedAt/UpdatedAt, DomainEventInterceptor for post-save event dispatch, SlowQueryInterceptor for query monitoring, and ToPagedListAsync for offset pagination.
---

# CSharpEssentials.EntityFrameworkCore

EF Core interceptors and pagination utilities that integrate with EntityBase and domain events automatically.

## Installation

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

## Namespace

```csharp
using CSharpEssentials.EntityFrameworkCore;
```

---

## Interceptors

Register in `DbContext.OnConfiguring`:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder options)
{
    options
        .AddInterceptors(new AuditInterceptor(auditUserIdProvider))
        .AddInterceptors(new DomainEventInterceptor(eventPublisher))
        .AddInterceptors(new SlowQueryInterceptor(slowQueryHandler, TimeSpan.FromSeconds(5)));
}
```

Register `AuditInterceptor` before `DomainEventInterceptor` in the chain.

### AuditInterceptor

Auto-sets `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` on `EntityBase<TId>` entries during `SaveChanges`.

```csharp
public class MyAuditProvider : IAuditUserIdProvider
{
    private readonly IHttpContextAccessor _accessor;
    public MyAuditProvider(IHttpContextAccessor accessor) => _accessor = accessor;

    public string GetUserId() =>
        _accessor.HttpContext?.User?.Identity?.Name ?? "system";
}

builder.Services.AddScoped<IAuditUserIdProvider, MyAuditProvider>();
```

### DomainEventInterceptor

Dispatches `IDomainEvent`s raised on entities after `SaveChanges` completes.

```csharp
public class MyEventPublisher : IDomainEventPublisher
{
    private readonly IMediator _mediator;
    public MyEventPublisher(IMediator mediator) => _mediator = mediator;

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct) =>
        _mediator.Publish(domainEvent, ct);
}

builder.Services.AddScoped<IDomainEventPublisher, MyEventPublisher>();
```

### SlowQueryInterceptor

Invokes a handler when a query exceeds the configured threshold.

```csharp
public class MySlowQueryHandler : ISlowQueryHandler
{
    public Task HandleAsync(string sql, TimeSpan elapsed, CancellationToken ct)
    {
        _logger.LogWarning("Slow query ({Elapsed}ms): {Sql}", elapsed.TotalMilliseconds, sql);
        return Task.CompletedTask;
    }
}
```

---

## Pagination

```csharp
// Offset-based paging
var page = await _db.Orders
    .OrderByDescending(o => o.CreatedAt)
    .ToPagedListAsync(pageNumber: 1, pageSize: 20);

page.Items           // IReadOnlyList<Order>
page.TotalCount      // int — total records (ignoring pagination)
page.PageNumber      // int
page.TotalPages      // int
page.HasNextPage     // bool
page.HasPreviousPage // bool
```

---

## Best Practices

- Register `AuditInterceptor` before `DomainEventInterceptor` — audit fields must be set before events fire
- Add a global EF query filter for `IsDeleted = false` to exclude soft-deleted records automatically
- Use `[DomainEventTiming(BeforeSave)]` on events that must validate before the transaction commits
- `ToPagedListAsync` issues two SQL queries (COUNT + data) — add appropriate indexes on sort columns
