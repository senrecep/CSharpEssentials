# CSharpEssentials.EntityFrameworkCore

Extensions, interceptors, and utilities for Entity Framework Core. Audit trails, domain events, pagination, and CQRS support.

## Features

- **Audit Interceptor** — Auto-populates `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` and handles soft deletes.
- **Domain Event Interceptor** — Dispatches domain events with before/after save timing and outbox support.
- **Slow Query Logging** — Logs queries exceeding a configurable threshold.
- **Pagination** — Offset-based and cursor-based pagination on `IQueryable`.
- **Batch Soft Delete** — Soft-delete many rows with a single `UPDATE` statement.
- **BaseDbContext** — Opt-in interceptor attachment from DI and domain event dispatch around `SaveChanges`.
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
public sealed record PriceChangedEvent(Guid ProductId, decimal NewPrice) : IDomainEvent;

public class Product : EntityBase<Guid>
{
    public decimal Price { get; private set; }

    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        Raise(new PriceChangedEvent(Id, newPrice));
    }
}
```

### Pagination

```csharp
using CSharpEssentials.EntityFrameworkCore.Pagination;

// Offset-based
var response = await dbContext.Users.PaginateAsync(
    new PaginationRequest { PageNumber = 1, PageSize = 10 }, cancellationToken: ct);

// Offset-based with page number and size
var page = await dbContext.Users.OrderBy(u => u.Id).PaginateAsync(pageNumber: 2, pageSize: 20, cancellationToken: ct);

// Synchronous, also works on in-memory IQueryable
var memoryPage = users.AsQueryable().Paginate(pageNumber: 1, pageSize: 10);

// Cursor-based
var cursorResponse = await dbContext.Logs.PaginateAsync(
    new CursorPaginationRequest<DateTimeOffset> { Limit = 20, Cursor = lastDate },
    cursorSelector: x => x.CreatedAt);
```

### Batch Soft Delete

```csharp
// UPDATE ... SET IsDeleted = 1, DeletedAt = @now, DeletedBy = @user WHERE ... AND NOT IsDeleted
int affected = await dbContext.Orders
    .Where(o => o.CustomerId == customerId)
    .SoftDeleteAsync(deletedBy: userId, timeProvider, ct);
```

Works for any entity implementing `ISoftDeletable` (e.g. `SoftDeletableEntityBase`). Rows already soft-deleted keep their original `DeletedAt`/`DeletedBy`. Like all `ExecuteUpdate` calls, it bypasses the change tracker and `SaveChanges` interceptors and needs a relational provider.

### BaseDbContext

```csharp
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, IServiceScopeFactory scopeFactory)
    : BaseDbContext<AppDbContext>(options, scopeFactory)
{
    // Attach interceptors registered in DI (AddAuditInterceptor, AddSlowQueryInterceptor, ...).
    protected override DbContextInterceptors InterceptorsFromServices =>
        DbContextInterceptors.Audit | DbContextInterceptors.SlowQuery;

    // Dispatch domain events around SaveChanges/SaveChangesAsync.
    protected override bool DispatchDomainEventsOnSaveChanges => true;
}
```

Both hooks are off by default. Interceptors that are not registered in DI, or are already on the options, are skipped. When dispatch is on, `BeforeSave` events go to `IDomainEventPublisher` before the save, and `AfterSave` events go to `IDomainEventOutbox` (if registered) or `IDomainEventPublisher` after the save succeeds. Override `DispatchDomainEventsAsync` to change this. If you override `OnConfiguring`, call `base.OnConfiguring(optionsBuilder)`.

The two domain event paths differ in when `AfterSave` events are handled:

| | `DispatchDomainEventsOnSaveChanges` | `DomainEventInterceptor` |
|---|---|---|
| `AfterSave` events | after the save completes | in `SavingChanges`, before the database write |
| Outbox write | not atomic with the save: the save is already committed when `StoreAsync` runs; inside an explicit transaction it runs before that transaction commits | before the save, in the same `SaveChanges` call |
| Save fails | `AfterSave` events are put back on their entities, so a retry dispatches them | events are already dispatched |

Use one of them, not both. When `DispatchDomainEventsOnSaveChanges` is `true`, `DbContextInterceptors.DomainEvents` in `InterceptorsFromServices` is ignored and the `DomainEventInterceptor` is not attached. The context's service scope (`ServiceProvider`) is disposed with the context.

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
