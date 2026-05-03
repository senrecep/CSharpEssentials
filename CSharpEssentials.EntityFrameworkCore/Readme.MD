# CSharpEssentials.EntityFrameworkCore

`CSharpEssentials.EntityFrameworkCore` offers a set of extensions, interceptors, and base classes to enhance Entity Framework Core development. It includes robust pagination support, audit trails, domain event dispatching, slow query logging, and simplified DbContext configuration.

## Features

- **Audit Interceptor**: Automatically populates `CreatedAt`/`CreatedBy`/`UpdatedAt`/`UpdatedBy` fields and handles soft deletes.
- **Domain Event Interceptor**: Collects and dispatches domain events raised by entities, with before/after save timing and optional outbox pattern support.
- **Slow Query Logging**: `SlowQueryInterceptor` to automatically log queries exceeding a configurable threshold.
- **Advanced Pagination**: Extension methods for **Offset-based** and **Cursor-based** pagination on `IQueryable`.
- **BaseDbContext**: A base class (`BaseDbContext<T>`) with built-in logging and scope management.
- **Model Configuration**: Extensions to apply configurations easily.

## Installation

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

## Usage

### 1. Audit Interceptor

Automatically tracks who created, modified, or deleted entities and when.

```csharp
using CSharpEssentials.EntityFrameworkCore.Interceptors;

// Simple registration — no need to create a separate class
services.AddAuditInterceptor(() => "system-user");

// Or resolve user ID from DI (e.g. from HttpContext)
services.AddAuditInterceptor(sp =>
    sp.GetRequiredService<IHttpContextAccessor>()
      .HttpContext?.User?.FindFirst("sub")?.Value ?? "anonymous");

// Generic variant for typed user IDs
services.AddAuditInterceptor<Guid>(sp =>
    sp.GetRequiredService<ICurrentUserService>().UserId);

// Register with DbContext
services.AddDbContext<MyDbContext>((sp, options) =>
    options.UseSqlServer(connectionString)
           .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
```

The interceptor handles:
- **Added** entities (`ICreationAudit`) — sets `CreatedAt` and `CreatedBy`
- **Modified** entities (`IModificationAudit`) — sets `UpdatedAt` and `UpdatedBy`
- **Deleted** entities (`ISoftDeletable`) — converts to soft delete with `DeletedAt`, `DeletedBy`, and `IsDeleted`

### 2. Domain Event Interceptor

Collects domain events raised by entities and dispatches them during `SaveChanges`.

```csharp
using CSharpEssentials.EntityFrameworkCore.Interceptors;

// Register publisher and interceptor
services.AddSingleton<IDomainEventPublisher, MediatRDomainEventPublisher>();
services.AddSingleton<DomainEventInterceptor>();

services.AddDbContext<MyDbContext>((sp, options) =>
    options.UseSqlServer(connectionString)
           .AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>()));
```

#### Event Timing

Control when events are published relative to the database save:

```csharp
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;

// AfterSave (default) — published after commit, safe for notifications
public sealed class OrderCreatedEvent(Guid orderId) : IDomainEvent
{
    public Guid OrderId { get; } = orderId;
}

// BeforeSave — published before commit, can abort the save on failure
[DomainEventTiming(DomainEventTiming.BeforeSave)]
public sealed class OrderValidationEvent(Guid orderId) : IDomainEvent
{
    public Guid OrderId { get; } = orderId;
}
```

#### Raising Events

```csharp
// In your entity (inherits from EntityBase which implements IDomainEventHolder)
public void UpdatePrice(decimal newPrice)
{
    decimal oldPrice = Price;
    Price = newPrice;
    Raise(new PriceChangedEvent(Id, oldPrice, newPrice));
}
```

Events are collected entity-by-entity in ChangeTracker order, preserving the list index within each entity.

#### Outbox Pattern

Register an `IDomainEventOutbox` for reliable delivery. AfterSave events are routed to the outbox; BeforeSave events are always published directly.

```csharp
services.AddSingleton<IDomainEventOutbox, EfCoreOutbox>();
```

### 3. Slow Query Logging

Register the interceptor to find performance bottlenecks.

```csharp
using CSharpEssentials.EntityFrameworkCore.Interceptors;

// Default threshold: 1 second
services.AddSlowQueryInterceptor();

// Custom threshold
services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));

// With configure action
services.AddSlowQueryInterceptor(options =>
    options.Threshold = TimeSpan.FromSeconds(2));

// Register with DbContext
services.AddDbContext<MyDbContext>((sp, options) =>
    options.UseSqlServer(connectionString)
           .AddInterceptors(sp.GetRequiredService<SlowQueryInterceptor>()));
```

Queries taking longer than the threshold will be logged as Warnings.

#### Custom Slow Query Handler

Implement `ISlowQueryHandler` for custom metrics or alerting:

```csharp
public sealed class PrometheusSlowQueryHandler : ISlowQueryHandler
{
    public void OnSlowQuery(SlowQueryContext context)
    {
        // Push to Prometheus, OpenTelemetry, etc.
        SlowQueryCounter.Inc();
    }
}

services.AddSingleton<ISlowQueryHandler, PrometheusSlowQueryHandler>();
```

### 4. Pagination (Offset-based)

Easily paginate any `IQueryable`.

```csharp
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;

var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

var response = await dbContext.Users
    .PaginateAsync(request, cancellationToken: ct);

var items = response.Data;
var total = response.TotalCount;
```

### 5. Cursor Pagination (High Performance)

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
        cursorSelector: x => x.CreatedAt,
        isAscending: false
    );

var nextCursor = response.NextCursor;
```

Supports any `IComparable<T>` cursor type: `string`, `int`, `Guid`, `DateTimeOffset`, etc.

### 6. Base DbContext

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

## Full Setup Example

```csharp
var services = new ServiceCollection();

services.AddLogging(b => b.AddConsole());

// Interceptors — one line each
services.AddAuditInterceptor(() => "demo-user");
services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));
services.AddSingleton<IDomainEventPublisher, MyPublisher>();
services.AddSingleton<DomainEventInterceptor>();

// DbContext with all interceptors
services.AddDbContext<ShopDbContext>((sp, options) =>
    options.UseSqlServer(connectionString)
           .AddInterceptors(
               sp.GetRequiredService<AuditInterceptor>(),
               sp.GetRequiredService<DomainEventInterceptor>(),
               sp.GetRequiredService<SlowQueryInterceptor>()));
```
