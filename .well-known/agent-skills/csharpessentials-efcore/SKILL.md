---
name: csharpessentials-efcore
description: Use when writing EF Core code with CSharpEssentials. Covers AddAuditInterceptor, BaseDbContext (InterceptorsFromServices, DispatchDomainEventsOnSaveChanges), DomainEventInterceptor, soft delete filters and SoftDeleteAsync, PaginateAsync and KeysetPaginateAsync, *AsResultAsync, AddEfCoreTransactionRunner, the outbox (AddEfCoreOutbox, AddOutboxProcessor), DbErrorTranslation, Maybe<T>? and [StringEnum] columns, plus the HealthChecks (AddMigrationsCheck), Hosting (AddOutboxProcessorHostedService) and Locking (AddRelationalResourceLock) packages.
---

# CSharpEssentials.EntityFrameworkCore

EF Core helpers for entities built on `CSharpEssentials.Entity` (see the `csharpessentials-entity` skill): audit stamping, soft delete, domain event dispatch, pagination, a transaction runner, an outbox and error translation. Three add-on packages are covered at the end.

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

Targets `net10.0` (EF Core 10), `net9.0` (EF Core 9), `net8.0` (EF Core 8). Named query filters and CSE3001 need EF Core 10.

## Namespaces

```csharp
using CSharpEssentials.EntityFrameworkCore;                      // BaseDbContext<T>, DbContextInterceptors, mapping helpers, query filters,
                                                                 // SoftDeleteAsync, HardDelete, *AsResultAsync, MigrateDataAsync, ISqlSession,
                                                                 // ConfigureNullableMaybeConventions, ConfigureEnumConventions, UseOrderedGuidKeys
using CSharpEssentials.EntityFrameworkCore.Interceptors;         // AddAuditInterceptor, AuditInterceptor, DomainEventInterceptor,
                                                                 // AddSlowQueryInterceptor, IDomainEventPublisher, IAuditUserIdProvider
using CSharpEssentials.EntityFrameworkCore.Pagination;           // PaginateAsync, Paginate, KeysetPaginateAsync
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;  // PaginationRequest, KeysetPaginationRequest
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses; // PaginationResponse<T>, KeysetPaginationResponse<T>
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;    // KeysetOrdering<T>, KeysetPaginationOptions, ICursorProtector, KeysetCursorErrors
using CSharpEssentials.EntityFrameworkCore.Transactions;         // AddEfCoreTransactionRunner
using CSharpEssentials.Transactions;                             // ITransactionRunner, ITransactionCallbacks
using CSharpEssentials.EntityFrameworkCore.Outbox;               // AddEfCoreOutbox, AddOutboxMessages, AddOutboxProcessor, IOutboxMessageHandler
using CSharpEssentials.EntityFrameworkCore.DbErrors;             // AddDbErrorTranslation, DbErrorTranslation, IDbErrorTranslator
using CSharpEssentials.EntityFrameworkCore.Sql;                  // AddDbContextSqlSession
using CSharpEssentials.EntityFrameworkCore.Idempotency;          // AddIdempotencyRecords, AddEfCoreIdempotencyRecordStore
using CSharpEssentials.EntityFrameworkCore.Migrations;           // MigrateDatabaseAsync, IDatabaseMigrationLock
using CSharpEssentials.EntityFrameworkCore.Extensions;           // AddCqrsDbContexts, AddWriteDbContext, AddReadDbContext
using CSharpEssentials.ResultPattern;                            // Result, Result<T>
using CSharpEssentials.Errors;                                   // Error
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using CSharpEssentials.ResultPattern;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();

ServiceCollection services = new();
services.AddLogging();                                              // BaseDbContext needs ILogger<T>
services.AddAuditInterceptor(() => "alice");
services.AddSingleton<IDomainEventPublisher, ConsolePublisher>();
services.AddDbContext<ShopDb>(options => options.UseSqlite(connection));

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
ShopDb db = scope.ServiceProvider.GetRequiredService<ShopDb>();
await db.Database.EnsureCreatedAsync();

db.Products.AddRange(Product.Create("Pen"), Product.Create("Notebook"), Product.Create("Stapler"));
await db.SaveChangesAsync();                                        // stamps CreatedAt/CreatedBy, publishes ProductCreated x3

Product pen = await db.Products.SingleAsync(p => p.Name == "Pen");
db.Products.Remove(pen);
await db.SaveChangesAsync();                                        // soft delete: IsDeleted = true, DeletedBy = "alice"
Console.WriteLine(await db.Products.CountAsync());                  // 2 (the filter hides Pen)

PaginationResponse<Product> page = await db.Products.OrderBy(p => p.Name).PaginateAsync(pageNumber: 1, pageSize: 10);
Console.WriteLine($"{page.TotalCount} rows, {page.TotalPages} page");   // 2 rows, 1 page

Result<KeysetPaginationResponse<Product>> feed = await db.Products.KeysetPaginateAsync(
    new KeysetPaginationRequest { Limit = 1 },
    k => k.Ascending(p => p.Name).Ascending(p => p.Id));
Console.WriteLine($"{feed.Value.Items[0].Name}, next cursor: {feed.Value.HasNext}");   // Notebook, next cursor: True

public sealed record ProductCreated(string Name) : IDomainEvent;

public sealed class Product : SoftDeletableEntityBase<Guid>
{
    private Product() { }

    public string Name { get; private set; } = "";

    public static Product Create(string name)
    {
        Product product = new() { Id = Guid.NewGuid(), Name = name };
        product.Raise(new ProductCreated(name));
        return product;
    }
}

public sealed class ShopDb(DbContextOptions<ShopDb> options, IServiceScopeFactory scopes) : BaseDbContext<ShopDb>(options, scopes)
{
    public DbSet<Product> Products => Set<Product>();

    protected override DbContextInterceptors InterceptorsFromServices => DbContextInterceptors.Audit;

    protected override bool DispatchDomainEventsOnSaveChanges => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().SoftDeletableEntityBaseGuidIdMap();
        modelBuilder.ApplySoftDeleteQueryFilter();                  // last, after all entities
    }
}

public sealed class ConsolePublisher : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Published {domainEvent}");
        return Task.CompletedTask;
    }
}
```

## Wiring the interceptors

```csharp
services.AddAuditInterceptor(sp =>                                   // also: Func<string>, Func<TUserId>, Func<IServiceProvider, TUserId>
    sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User.FindFirst("sub")?.Value ?? "anonymous");
services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));    // default threshold 1 s
services.AddSingleton<ISlowQueryHandler, MySlowQueryHandler>();      // optional
services.AddScoped<IDomainEventPublisher, MyPublisher>();
services.AddSingleton<DomainEventInterceptor>();                     // only when not using BaseDbContext dispatch
```

Attach them in one of two ways:

```csharp
// A: on the options, works for any DbContext
services.AddDbContext<AppDb>((sp, o) => o.UseNpgsql(cs).AddInterceptors(
    sp.GetRequiredService<AuditInterceptor>(),
    sp.GetRequiredService<DomainEventInterceptor>(),
    sp.GetRequiredService<SlowQueryInterceptor>()));

// B: AppDb : BaseDbContext<AppDb>, constructor (DbContextOptions<AppDb>, IServiceScopeFactory)
protected override DbContextInterceptors InterceptorsFromServices => DbContextInterceptors.Audit | DbContextInterceptors.SlowQuery;
protected override bool DispatchDomainEventsOnSaveChanges => true;   // instead of DomainEventInterceptor
```

Audit: `Added` + `ICreationAudit` gets `SetCreatedInfo`, `Modified` + `IModificationAudit` gets `SetUpdatedInfo`, `Deleted` + `ISoftDeletable` (not `IsHardDeleted`) becomes `MarkAsDeleted` and state `Modified`. Cascade-deleted dependents of a soft-deleted root are kept (soft-deleted when `ISoftDeletable`, otherwise `Unchanged`).

Domain events: `BeforeSave` events are published before the write; a throwing handler aborts the save. `AfterSave` events: `BaseDbContext` publishes after a successful save; `DomainEventInterceptor` publishes before the write. With `AddEfCoreOutbox`, `AfterSave` events are stored as outbox rows in the same save instead.

## Mapping entities

```csharp
modelBuilder.Entity<Order>().SoftDeletableEntityBaseGuidIdMap();     // Order : SoftDeletableEntityBase<Guid>
modelBuilder.Entity<Invoice>().EntityBaseGuidIdMap();                // Invoice : EntityBase<Guid>
modelBuilder.Entity<Line>().EntityBaseMap<Line, long>(userIdMaxLength: 64);
modelBuilder.Entity<Order>().OptimisticConcurrencyVersionMap();      // shadow byte[] "RowVersion"
modelBuilder.Entity<Order>().Property(o => o.Details).HasJsonConversion("jsonb");
```

The map helpers make `CreatedBy` required (max 40 chars by default), so register the audit interceptor or inserts fail.

## Soft delete and filters

```csharp
modelBuilder.ApplySoftDeleteQueryFilter();               // anonymous !IsDeleted on root ISoftDeletableBase types (all targets)
modelBuilder.ApplyNamedSoftDeleteQueryFilter();          // EF Core 10: named QueryFilterNames.SoftDelete
modelBuilder.Entity<Order>()
    .HasSoftDeleteQueryFilter()                          // EF Core 10, one type
    .HasQueryFilter(QueryFilterNames.Tenant, o => o.TenantId == TenantId);

List<Order> withDeleted = await db.Orders.IgnoreSoftDeleteQueryFilter().ToListAsync(ct);   // tenant filter still applies

int rows = await db.Orders.Where(o => o.CustomerId == id).SoftDeleteAsync(deletedBy: "admin", cancellationToken: ct);
db.HardDelete(order);                                    // MarkAsHardDeleted + Remove; then SaveChangesAsync
```

- Never mix anonymous and named filters on one entity type: the named helpers throw `InvalidOperationException`.
- Pass filter names as a `static readonly string[]` or `new[] { ... }`, not an inline collection expression or `List<string>` (EF Core 10.0.x recompiles the query).
- `SoftDeleteAsync` is one `ExecuteUpdate`: no change tracker, no interceptors, no domain events, relational providers only.

## Paging

```csharp
PaginationResponse<Order> page = await db.Orders.OrderBy(o => o.Id)
    .PaginateAsync(new PaginationRequest { PageNumber = 2, PageSize = 20, Search = term },
                   search: s => o => o.Number.Contains(s), includeTotalCount: true, maxPageSize: 100, cancellationToken: ct);
// Items, PageNumber, PageSize, TotalCount (-1 without count), TotalPages, HasPreviousPage, HasNextPage

KeysetOrdering<Order> ordering = new KeysetOrdering<Order>().Descending(o => o.CreatedOn).Descending(o => o.Id);   // build once
Result<KeysetPaginationResponse<Order>> result = await db.Orders.KeysetPaginateAsync(
    new KeysetPaginationRequest { Limit = 20, After = after, Before = before }, ordering, ct);
if (result.IsFailure) return Results.BadRequest(result.FirstError.Code);   // KeysetCursorErrors codes
// result.Value: Items, NextCursor (send as After), PreviousCursor (send as Before), HasNext, HasPrevious
```

- Offset `PageSize` is capped at 100 unless you pass `maxPageSize` (`int.MaxValue` removes the cap).
- Keyset: end the ordering with a unique key. Keys must be non-nullable members (no `int?`, no `x => x.Nav!.Prop`, no computed values). On SQLite do not use `decimal`, `DateTimeOffset` or `TimeSpan` keys. Enum keys must be stored as numbers.
- Bad cursors and `After` + `Before` together return `Error.Validation`; they never throw.
- `PaginateAsync(ICursorPaginationRequest<TCursor>, selector)` is obsolete. Use `KeysetPaginateAsync`.

## Result helpers and seeding

```csharp
Result<Order> one = await db.Orders.Where(o => o.Id == id).FirstOrDefaultAsResultAsync(Error.NotFound("Order.NotFound", "Not found"), ct);
Result<Order> single = await db.Orders.SingleOrDefaultAsResultAsync(cancellationToken: ct);
Result<Order> byKey = await db.Orders.FindAsResultAsync([id], cancellationToken: ct);
Result saved = await db.SaveChangesAsResultAsync(ct);                       // any exception -> ErrorType.Unknown

await db.MigrateDataAsync<Country, CountrySeed>(seed, (set, data) => set.Any(), s => new Country { Code = s.Code }, ct);
await db.MigrateDataAsync(seed, new MigrateDataOptions<Country, CountrySeed, string>
{
    Query = set => set, EntityKeyProperty = c => c.Code, DataKeyProperty = s => s.Code,
    IsUpdatedFunc = (c, s) => c.Name != s.Name, UpdateFunc = (c, s) => { c.Name = s.Name; return c; },
    Converter = s => new Country { Code = s.Code, Name = s.Name },
}, ct);                                                                     // add, update, remove; calls SaveChangesAsync
```

## Running a unit of work

```csharp
services.AddEfCoreTransactionRunner<AppDb>();   // scoped ITransactionRunner + ITransactionCallbacks
services.AddDbContextSqlSession<AppDb>();       // scoped ISqlSession for Dapper/ADO.NET on the same transaction

Result result = await runner.ExecuteAsync(async ct =>
{
    db.Orders.Add(order);
    await db.SaveChangesAsync(ct);
    await session.Connection.ExecuteAsync(sql, args, transaction: session.Transaction);   // Dapper; always pass Transaction
    await callbacks.OnCommittedAsync(async token => await cache.RemoveAsync(key, token), ct);   // Func<CancellationToken, ValueTask>; runs after commit
    return Result.Success();
}, outcome => outcome.IsSuccess, cancellationToken);                                       // false -> rollback
```

- The outermost call uses `CreateExecutionStrategy()`, so `EnableRetryOnFailure` works; a retry reruns the whole unit on a cleared change tracker. Make no changes before the call.
- Nested calls on the same context join the transaction behind a savepoint.
- Needs a relational provider (not InMemory). Do not combine with `TransactionScope`.
- Never dispose `ISqlSession.Connection`.

## Storing and relaying events with the outbox

```csharp
public sealed record OrderPlaced(Guid OrderId) : DomainEvent;   // EventId becomes the message id

[JsonSerializable(typeof(OrderPlaced))]
internal sealed partial class AppJson : JsonSerializerContext;

modelBuilder.AddOutboxMessages();                                          // OnModelCreating; then add a migration
services.AddEfCoreOutbox(types => types.Add("orders.placed", AppJson.Default.OrderPlaced));
services.AddScoped<IOutboxMessageHandler, BrokerHandler>();                // HandleAsync(OutboxMessage, IDomainEvent, ct)
services.AddOutboxProcessor<AppDb>(o => { o.BatchSize = 100; o.MaxAttempts = 8; });
// o.Claimer = new SqlOutboxMessageClaimer(OutboxClaimDialect.PostgreSql);  // SKIP LOCKED for many processors

int claimed = await provider.GetRequiredService<IOutboxProcessor>().ProcessBatchAsync(ct);
await db.PurgeProcessedOutboxMessagesAsync(DateTime.UtcNow.AddDays(-7), ct);
```

- Requires `DispatchDomainEventsOnSaveChanges` or `DomainEventInterceptor`; only `AfterSave` events are stored.
- Every stored event type must be registered by name. An unregistered type throws and nothing is saved.
- Delivery is at least once: dedupe on `OutboxMessage.Id`. Failures retry with backoff and dead-letter after `MaxAttempts` (`DeadLetteredAt`).
- A custom `IDomainEventOutbox.StoreAsync(DbContext context, events, ct)` adds rows to `context` and never calls `SaveChanges`.

## Translating database errors

```csharp
services.AddDbErrorTranslation();                                    // SQLSTATE: 23505/23503 -> Conflict, 23514/23502 -> Validation, 40001/40P01 -> Conflict (retryable)
services.AddDbErrorTranslator<SqlServerTranslator>();                // runs first; needed for SQL Server and SQLite (no SqlState)

Result<int> saved = await translation.SaveChangesAsync(db, ct);       // translated error, or rethrows unknown exceptions
bool known = translation.TryTranslate(exception, out Error error);
```

Codes: `Database.UniqueViolation`, `Database.ForeignKeyViolation`, `Database.CheckViolation`, `Database.NotNullViolation`, `Database.SerializationFailure`, `Database.Deadlock`. Translators must be stateless.

## Maybe and enum columns

```csharp
public Maybe<string>? Nickname { get; set; }                         // Maybe<T>? -> NULL column; plain Maybe<T> is NOT NULL

protected override void ConfigureConventions(ModelConfigurationBuilder b)
{
    b.ConfigureNullableMaybeConventions();                           // or .Property(x => x.Nickname).HasNullableMaybeConversion()
    b.ConfigureEnumConventions(EnumConventions.Default);             // [StringEnum] enums stored by wire name + check constraint
    // existing DB: b.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer);
    b.UseOrderedGuidKeys();                                          // opt-in version 7 / SQL Server ordered Guid keys
}

db.Customers.Where(c => c.Nickname == null);                         // absence; never == Maybe<string>.None
Maybe<string> nick = customer.Nickname.Flatten();                    // null -> None
modelBuilder.Entity<Order>().Property(o => o.Priority).HasEnumStorage(EnumStorage.Integer);
migrationBuilder.ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);
string auditSql = EnumDataAudit.Sql<OrderStatus>("orders", "Status", storedAs: EnumStoredAs.Integer, provider: db.Database.ProviderName);
```

## Registration and startup

```csharp
services.AddCqrsDbContexts<WriteDb, ReadDb>((sp, o) => o.UseNpgsql(cs));   // pooled; read context NoTracking
services.AddWriteDbContext<WriteDb>(o => o.EnableAuditInterceptor = true, (sp, b) => b.UseNpgsql(cs));

await app.Services.MigrateDatabaseAsync<AppDb>(app.Lifetime.ApplicationStopping);   // before app.Run(); optional IDatabaseMigrationLock

modelBuilder.AddIdempotencyRecords(Database.ProviderName);                          // transactional idempotency store
services.AddEfCoreIdempotencyRecordStore<AppDb>();
```

`DbContextRegistrationOptions.MigrationsAssembly`, `RetryOptions` and `QuerySplittingBehavior` are not applied; set them in the provider callback.

## Add-on packages

### CSharpEssentials.EntityFrameworkCore.HealthChecks

```csharp
using CSharpEssentials.EntityFrameworkCore.HealthChecks;

services.AddHealthChecks().AddMigrationsCheck<AppDb>();   // name "AppDb-migrations", tag "ready"
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains(MigrationsHealthCheckBuilderExtensions.ReadyTag) });
```

Pending migrations or an unreachable database: the failure status (`Unhealthy` by default), names under data key `pendingMigrations`. All applied but `HasPendingModelChanges()`: `Degraded`. Otherwise `Healthy`.

### CSharpEssentials.EntityFrameworkCore.Hosting

```csharp
using CSharpEssentials.EntityFrameworkCore.Hosting;

services.AddOutboxProcessor<AppDb>(o => o.PollingInterval = TimeSpan.FromSeconds(2));   // required first
services.AddOutboxProcessorHostedService();
```

Runs `ProcessBatchAsync` in a loop: again at once after a full batch, otherwise waits `PollingInterval` (5 s). Failed batches are logged and retried.

### CSharpEssentials.EntityFrameworkCore.Locking

```csharp
using CSharpEssentials.EntityFrameworkCore.Locking;

services.AddMediatorLockBehavior();                                        // CSharpEssentials.Mediator
services.AddRelationalResourceLock<AppDb>(ResourceLockSqlDialect.PostgreSql, o => o.PollInterval = TimeSpan.FromMilliseconds(100));
// ResourceLockSqlDialect.SqlServer, or new ResourceLockSqlDialect(tryAcquireSql, releaseSql) with {0} as the key
```

Registers `IResourceLock` (namespace `CSharpEssentials.Locking`): `AcquireAsync(key, timeout, ct)` throws `TimeoutException`; `TryAcquireAsync(key, ct)` returns `Maybe<IAsyncDisposable>`. Dispose the handle to release. Each held lock keeps one connection open; keep `LockPlacement.OutsideTransaction`. Not reentrant.

## Rules

- Register `services.AddLogging()` when using `BaseDbContext`; its constructor resolves `ILogger<TContext>`.
- Use one domain event path: `DispatchDomainEventsOnSaveChanges` or `DomainEventInterceptor`, never both.
- Call `ApplySoftDeleteQueryFilter()` / `ApplyNamedSoftDeleteQueryFilter()` at the end of `OnModelCreating`.
- `IgnoreQueryFilters()` without names removes the tenant filter too; on EF Core 10 use `IgnoreSoftDeleteQueryFilter()`.
- Async methods end in `Async` (`PaginateAsync`, `KeysetPaginateAsync`, `SoftDeleteAsync`, `FirstOrDefaultAsResultAsync`, `SaveChangesAsResultAsync`, `MigrateDataAsync`, `ProcessBatchAsync`). The only sync pager is `Paginate`.
- `KeysetPaginateAsync` returns `Result<KeysetPaginationResponse<T>>`; check `IsFailure` before `.Value`.
- `ExecuteAsync` takes `work` and `shouldCommit`; there is no overload without `shouldCommit`.
- `SoftDeleteAsync`, `PurgeProcessedOutboxMessagesAsync` and the transaction runner need a relational provider (SQLite works, InMemory does not).
- For SQLite in-memory demos keep one `SqliteConnection` open; services that open their own scope (outbox processor, lock, health check) need a shared-cache connection string such as `Data Source=demo;Mode=Memory;Cache=Shared`.
- Use `Microsoft.EntityFrameworkCore.Sqlite` 10.x with `net10.0`.

## Diagnostics

| ID | Severity | Meaning | Fix |
|---|---|---|---|
| CSE3001 | Info | parameterless `IgnoreQueryFilters()` (EF Core 10) | pass names, e.g. `IgnoreSoftDeleteQueryFilter()`; the code fix writes `IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete })` |
| CSE0014 | Error (from `CSharpEssentials.Enums`) | a generated `AlterColumn` left next to `ConvertEnumColumn` | delete the `AlterColumn` |
