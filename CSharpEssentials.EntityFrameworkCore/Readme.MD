# CSharpEssentials.EntityFrameworkCore

Extensions, interceptors, and utilities for Entity Framework Core. Audit trails, domain events, pagination, and CQRS support.

## Features

- **Audit Interceptor** — Auto-populates `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` and handles soft deletes.
- **Domain Event Interceptor** — Dispatches domain events with before/after save timing and outbox support.
- **Slow Query Logging** — Logs queries exceeding a configurable threshold.
- **Pagination** — Offset-based and cursor-based pagination on `IQueryable`.
- **CQRS Support** — Register separate write and read DbContexts.
- **SQL Connection Factories** — Abstractions for raw SQL/Dapper scenarios.

## Installation

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

## Usage

### Audit Interceptor

```csharp
services.AddAuditInterceptor(() => "system-user");

services.AddDbContext<MyDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
```

### Domain Event Interceptor

```csharp
services.AddSingleton<IDomainEventPublisher, MyPublisher>();
services.AddSingleton<DomainEventInterceptor>();

// In your entity
public void UpdatePrice(decimal newPrice)
{
    Price = newPrice;
    Raise(new PriceChangedEvent(Id, newPrice));
}
```

### Pagination

```csharp
using CSharpEssentials.EntityFrameworkCore.Pagination;

// Offset-based
var response = await dbContext.Users.PaginateAsync(
    new PaginationRequest { PageNumber = 1, PageSize = 10 }, ct);

// Cursor-based
var cursorResponse = await dbContext.Logs.PaginateAsync(
    new CursorPaginationRequest<DateTimeOffset> { Limit = 20, Cursor = lastDate },
    cursorSelector: x => x.CreatedAt);
```

### CQRS Write/Read

```csharp
services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(
    configureWrite: (sp, options) => options.UseNpgsql(connectionString),
    configureRead: (sp, options) => options.UseNpgsql(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

### Slow Query Logging

```csharp
services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));
```
