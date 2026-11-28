# CSharpEssentials.EntityFrameworkCore

EF Core support for the `CSharpEssentials.Entity` base classes and a set of data access helpers. It fills audit fields, turns deletes into soft deletes, dispatches domain events, pages queries, runs units of work in a transaction and stores events in an outbox.

Benchmarks against plain EF Core, MR.EntityFrameworkCore.KeysetPagination, X.PagedList.EF and EntityFrameworkCore.Exceptions are in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md). The design-time cost of the bundled CSE3001 analyzer is in [the analyzer BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore.Generators/BENCHMARKS.md).

## Install

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

Targets `net10.0` (EF Core 10), `net9.0` (EF Core 9) and `net8.0` (EF Core 8). There is no `net11.0` target. Use the EF Core major that matches your target framework.

The package is not Native AOT compatible: EF Core's Native AOT support is experimental, needs precompiled queries and is not available on net8.0 ([EF Core NativeAOT and precompiled queries](https://learn.microsoft.com/ef/core/performance/nativeaot-and-precompiled-queries)).

Add-on packages:

- [CSharpEssentials.EntityFrameworkCore.HealthChecks](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore.HealthChecks/Readme.MD): a readiness check that fails while migrations are pending.
- [CSharpEssentials.EntityFrameworkCore.Hosting](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore.Hosting/Readme.MD): a hosted service that relays the outbox.
- [CSharpEssentials.EntityFrameworkCore.Locking](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore.Locking/Readme.MD): a database-session lock for the Mediator `LockBehavior`.

## Quick start

Audit fields, soft delete, domain events and paging on an in-memory SQLite database:

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();

ServiceCollection services = new();
services.AddLogging();
services.AddAuditInterceptor(() => "alice");                       // who is saving
services.AddSingleton<IDomainEventPublisher, ConsolePublisher>();   // where events go
services.AddDbContext<ShopDb>(options => options.UseSqlite(connection));

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
ShopDb db = scope.ServiceProvider.GetRequiredService<ShopDb>();
await db.Database.EnsureCreatedAsync();

db.Products.AddRange(Product.Create("Pen"), Product.Create("Notebook"), Product.Create("Stapler"));
await db.SaveChangesAsync();
// Published ProductCreated { Name = Pen } (three lines)

Product pen = await db.Products.SingleAsync(p => p.Name == "Pen");
Console.WriteLine($"{pen.Name} created by {pen.CreatedBy}");          // Pen created by alice

db.Products.Remove(pen);
await db.SaveChangesAsync();                                         // UPDATE ... SET IsDeleted = 1, not DELETE
int all = await db.Products.IgnoreQueryFilters().CountAsync();
Console.WriteLine($"Visible: {await db.Products.CountAsync()}, stored: {all}, deleted by {pen.DeletedBy}");
// Visible: 2, stored: 3, deleted by alice

PaginationResponse<Product> page = await db.Products.OrderBy(p => p.Name).PaginateAsync(pageNumber: 1, pageSize: 1);
Console.WriteLine($"Page 1 of {page.TotalPages}: {page.Items[0].Name}");   // Page 1 of 2: Notebook

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
        modelBuilder.ApplySoftDeleteQueryFilter();
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

## Contents

[Audit interceptor](#audit-interceptor) · [Domain events](#domain-events) · [BaseDbContext](#basedbcontext) · [Entity mapping](#entity-mapping) · [Soft delete and query filters](#soft-delete-and-query-filters) · [Offset pagination](#offset-pagination) · [Keyset pagination](#keyset-pagination) · [Result helpers and seed data](#result-helpers-and-seed-data) · [Transaction runner](#transaction-runner) · [Outbox](#outbox) · [Idempotency records](#idempotency-records) · [Database error translation](#database-error-translation) · [Slow query logging](#slow-query-logging) · [Maybe columns](#maybe-columns) · [Enum storage](#enum-storage) · [Ordered GUID keys](#ordered-guid-keys) · [Registration helpers](#registration-helpers) · [Migrate on startup](#migrate-on-startup) · [Diagnostics](#diagnostics)

## Namespaces

| Namespace | Contents |
|---|---|
| `CSharpEssentials.EntityFrameworkCore` | `BaseDbContext<T>`, `DbContextInterceptors`, entity mapping, query filters, `SoftDeleteAsync`, `HardDelete`, `*AsResultAsync`, `MigrateDataAsync`, Maybe and enum conventions, `UseOrderedGuidKeys`, `ISqlSession` |
| `.Interceptors` | `AuditInterceptor`, `DomainEventInterceptor`, `SlowQueryInterceptor`, `AddAuditInterceptor`, `AddSlowQueryInterceptor`, `IAuditUserIdProvider`, `IDomainEventPublisher`, `IDomainEventOutbox`, `ISlowQueryHandler` |
| `.Pagination` | `PaginateAsync`, `Paginate`, `KeysetPaginateAsync`, `PaginationDefaults` |
| `.Pagination.Requests` / `.Responses` / `.Keyset` | request and response records; `KeysetOrdering<T>`, `KeysetPaginationOptions`, `ICursorProtector`, `KeysetCursorErrors` |
| `.Extensions` | `AddCqrsDbContexts`, `AddWriteDbContext`, `AddReadDbContext`, `AddPooledDbContext`, `RegisterDbContextFactory`, `UseAsReadContext` |
| `.Transactions` | `AddEfCoreTransactionRunner`, `EfCoreTransactionRunner<T>` (the interfaces are in `CSharpEssentials.Transactions`) |
| `.Outbox` | `AddEfCoreOutbox`, `AddOutboxMessages`, `AddOutboxProcessor`, `OutboxMessage`, `IOutboxMessageHandler` |
| `.Idempotency` | `AddIdempotencyRecords`, `AddEfCoreIdempotencyRecordStore`, `EfCoreIdempotencyRecordStore<T>` |
| `.DbErrors` | `DbErrorTranslation`, `AddDbErrorTranslation`, `IDbErrorTranslator`, `SqlStateErrorTranslator` |
| `.Sql` | `AddDbContextSqlSession`, `AddDbDataSourceReadOnlyConnectionFactory` |
| `.Migrations` | `MigrateDatabaseAsync`, `IDatabaseMigrationLock` |
| `.Converters` | `NullableMaybeConverter<T>`, enum value converters, `EnumJsonValueReaderWriter<TEnum>` |

## Audit interceptor

`AuditInterceptor` runs in `SaveChanges` and `SaveChangesAsync`:

| Entry state | Interface | Action |
|---|---|---|
| `Added` | `ICreationAudit` | `SetCreatedInfo(now, user)` |
| `Modified` | `IModificationAudit` | `SetUpdatedInfo(now, user)` |
| `Deleted` | `ISoftDeletable`, not marked with `MarkAsHardDeleted()` | `MarkAsDeleted(now, user)` and the state becomes `Modified` (a soft delete) |

Register it, then attach it, either with `BaseDbContext` (as in the quick start) or on the options:

```csharp
services.AddAuditInterceptor(() => "system");
services.AddDbContext<AppDb>((sp, options) => options
    .UseSqlite(connectionString)
    .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
```

`AddAuditInterceptor` takes the user in one of four forms: `Func<string>`, `Func<IServiceProvider, string>`, `Func<TUserId>` or `Func<IServiceProvider, TUserId>` (the id is stored with `ToString()`). Use the `IServiceProvider` form to read the current request:

```csharp
services.AddAuditInterceptor(sp =>
    sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User.FindFirst("sub")?.Value ?? "anonymous");
```

- It registers a scoped `IAuditUserIdProvider`, `TimeProvider.System` and the singleton `AuditInterceptor`. The last two are added only when not registered yet, so your own `TimeProvider` wins and a second call adds no second interceptor.
- The user provider and the clock are read once per save, and only when the save has an entry to stamp.
- `AddAuditUserIdProvider` (same four overloads) registers only the provider. You can also implement `IAuditUserIdProvider` or `IAuditUserIdProvider<TUserId>` yourself and register the interceptor with `services.AddSingleton(TimeProvider.System); services.AddSingleton<AuditInterceptor>();`.
- `SoftDeletableEntityBaseGuidIdMap()` and the other mapping helpers make `CreatedBy` required. Without the interceptor the insert fails with a `NOT NULL` error.

### Soft delete and cascades

EF Core marks tracked owned types and `Cascade`/`ClientCascade` dependents as deleted as soon as you call `Remove` (the default `CascadeDeleteTiming.Immediate`). When the root becomes a soft delete, the interceptor keeps them:

- Owned types stay.
- `ISoftDeletable` dependents are soft-deleted too.
- Other dependents go back to `Unchanged`.
- A dependent marked with `MarkAsHardDeleted()` is still deleted, and so is one that another principal deleted in the same save cascades to.
- A dependent you removed yourself in the same save looks like a cascaded one and is kept. Remove it in its own `SaveChanges`.
- Optional dependents with `SetNull`/`ClientSetNull` still get their foreign key set to null by EF.

Pending edits on kept dependents are saved with the default snapshot change tracking. With a notification-based `ChangeTrackingStrategy` they can be lost, so save those edits before you remove the root.

## Domain events

There are two ways to dispatch the events that entities `Raise`. Use one of them, not both.

### With BaseDbContext

Set `DispatchDomainEventsOnSaveChanges => true`, as in the quick start:

- `BeforeSave` events go to `IDomainEventPublisher` before the write.
- `AfterSave` events go to `IDomainEventPublisher` after the save succeeds. When an `IDomainEventOutbox` is registered they are stored in it instead, before the write, in the same `SaveChanges`.
- When the save (or the outbox) fails, the `AfterSave` events are put back on their entities so a retry dispatches them, and the rows the outbox added are detached. When the publisher throws while publishing `AfterSave` events, they stay cleared, because a retry would publish the earlier ones twice.
- Override `DispatchDomainEventsAsync` to change how events are published.

### With DomainEventInterceptor

Works on any `DbContext`:

```csharp
services.AddSingleton<IDomainEventPublisher, MyPublisher>();
services.AddSingleton<DomainEventInterceptor>();
services.AddDbContext<AppDb>((sp, options) => options
    .UseSqlite(connectionString)
    .AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>()));
```

- In `SavingChanges` it takes the events of every tracked `IDomainEventHolder` and clears them.
- It publishes the `BeforeSave` events, then the `AfterSave` events, both before the write. With a registered `IDomainEventOutbox` the `AfterSave` events go to the outbox instead.
- When a `BeforeSave` publish or the outbox throws, the `AfterSave` events are put back and the outbox rows detached. `BeforeSave` events are not put back.
- When the publisher throws while publishing `AfterSave` events, or the database write fails, the events stay cleared.

### Notes

The publisher and the outbox are resolved from a DI scope, so a scoped `IDomainEventPublisher` works. Event timing is set with `[DomainEventTiming]` from `CSharpEssentials.Entity`.

## BaseDbContext

`BaseDbContext<TContext>` is an optional base class. Its constructor takes `DbContextOptions<TContext>` and `IServiceScopeFactory` and creates a service scope that lives as long as the context (the protected `ServiceProvider`). It resolves `ILogger<TContext>` from that scope with `GetRequiredService`, so call `services.AddLogging()` or the constructor throws.

| Override | Default | Effect |
|---|---|---|
| `InterceptorsFromServices` | `None` | Attaches the interceptors registered in DI: `DbContextInterceptors.Audit`, `DomainEvents`, `SlowQuery` or `All`. Unregistered ones and ones already on the options are skipped. `DomainEvents` is ignored when `DispatchDomainEventsOnSaveChanges` is `true`. |
| `DispatchDomainEventsOnSaveChanges` | `false` | Dispatches domain events around `SaveChanges`; see [Domain events](#domain-events). |
| `DeferDomainEventPublishingUntilCommit` | `false` | With dispatch on, publishes `AfterSave` events after the commit of an `EfCoreTransactionRunner` unit and drops them on rollback. Outbox storage is never deferred. |
| `DispatchDomainEventsAsync(...)` | publisher | How events are published. |

If you override `OnConfiguring`, call `base.OnConfiguring(optionsBuilder)`.

## Entity mapping

Mapping helpers for the `CSharpEssentials.Entity` base classes, for `IEntityTypeConfiguration<T>` or `OnModelCreating`:

```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>   // Order : SoftDeletableEntityBase<Guid>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.SoftDeletableEntityBaseGuidIdMap();                    // key, audit and soft delete columns
        builder.OptimisticConcurrencyVersionMap();                     // shadow byte[] "RowVersion" row version
        builder.Property(o => o.Details).HasJsonConversion("jsonb");   // JSON column
    }
}
```

| Method | Maps |
|---|---|
| `EntityBaseGuidIdMap()` | `Id` key plus audit columns, for `EntityBase<Guid>` |
| `SoftDeletableEntityBaseGuidIdMap()` | the same plus `IsDeleted`, `DeletedAt`, `DeletedBy`; `IsHardDeleted` is ignored |
| `EntityBaseMap<TEntity, TId>()`, `SoftDeletableEntityBaseMap<TEntity, TId>()` | the same for any key type |
| `EntityBaseMap<TEntity>()`, `SoftDeletableEntityBaseMap<TEntity>()` | audit (and soft delete) columns without a key |
| `OptimisticConcurrencyVersionMap(propertyName = "RowVersion")` | a shadow `byte[]` row version |
| `HasJsonConversion(columnType = "jsonb", options = null)` | stores the property as JSON with the CSharpEssentials JSON options |

`CreatedAt`/`CreatedBy` are required, `UpdatedAt`/`UpdatedBy` optional, and the user columns are at most 40 characters unless you pass `userIdMaxLength`. In a `HasJsonConversion` column, an undefined enum value is saved as its number; on load it becomes the `[EnumFallback]` member, or the read throws when the enum has none.

## Soft delete and query filters

### Filters

| Method | Targets | Filter |
|---|---|---|
| `modelBuilder.ApplySoftDeleteQueryFilter()` | every root entity type that implements `ISoftDeletableBase` | anonymous `!IsDeleted` |
| `entity.AddQueryFilter<T>(expression)` | one entity type | your filter `AND` the existing anonymous filter |
| `modelBuilder.ApplyNamedSoftDeleteQueryFilter()` (EF Core 10) | every root `ISoftDeletableBase` type | `!IsDeleted` named `QueryFilterNames.SoftDelete` |
| `entity.HasSoftDeleteQueryFilter()` (EF Core 10) | one entity type | the same, for one type |

Call the `Apply*` methods at the end of `OnModelCreating`. They put the filter on root types only; derived types inherit it, and owned types are skipped because EF Core does not allow filters on them.

On EF Core 10, named filters let a query switch off soft delete and keep the others, such as a tenant filter:

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.Entity;
using CSharpEssentials.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();
using AppDb db = new(new DbContextOptionsBuilder<AppDb>().UseSqlite(connection).Options) { TenantId = "a" };
db.Database.EnsureCreated();

db.Orders.AddRange(new Order("a", "A-1"), new Order("a", "A-2"), new Order("b", "B-1"));
db.SaveChanges();

// One UPDATE statement for every matching row; the change tracker and interceptors are bypassed.
int deleted = await db.Orders.Where(o => o.Number == "A-1").SoftDeleteAsync(deletedBy: "bob");
Console.WriteLine($"Soft-deleted rows: {deleted}");                                                   // 1

Console.WriteLine($"Default: {db.Orders.Count()}");                                                   // 1 (tenant a, not deleted)
Console.WriteLine($"Without soft delete: {db.Orders.IgnoreSoftDeleteQueryFilter().Count()}");         // 2 (tenant a)
Console.WriteLine($"Without tenant: {db.Orders.IgnoreQueryFilters(TenantOnly).Count()}");             // 2 (not deleted)

public partial class Program
{
    private static readonly string[] TenantOnly = [QueryFilterNames.Tenant];
}

public sealed class Order : SoftDeletableEntityBase<int>
{
    public Order(string tenantId, string number) => (TenantId, Number) = (tenantId, number);

    public string TenantId { get; private set; }
    public string Number { get; private set; }
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public string TenantId { get; init; } = "";

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Order>()
            .HasSoftDeleteQueryFilter()
            .HasQueryFilter(QueryFilterNames.Tenant, o => o.TenantId == TenantId);
}
```

- `IgnoreSoftDeleteQueryFilter()` is `IgnoreQueryFilters([QueryFilterNames.SoftDelete])` with a cached array.
- `QueryFilterNames.SoftDelete` (`"SoftDelete"`) and `QueryFilterNames.Tenant` (`"Tenant"`) are plain constants. Use them for your own filters so callers can ignore them by name.
- Pass filter keys in a `static readonly string[]` field or `new[] { ... }`. With a collection expression written inline at the call or a `List<string>`, EF Core 10.0.x compiles the query again on every execution.
- EF Core does not allow an anonymous filter and named filters on the same entity type. The named helpers throw `InvalidOperationException` when the type already has an anonymous filter. On net8.0 and net9.0 only the anonymous helpers exist.
- A soft-deletable type whose root type is not soft-deletable gets no filter from the `Apply*` methods. Make the root implement `ISoftDeletableBase`.

Moving from `ApplySoftDeleteQueryFilter()`: named keys only switch off named filters, so `IgnoreSoftDeleteQueryFilter()` ignores nothing while the model still has the anonymous filter. Change the model first (`ApplyNamedSoftDeleteQueryFilter()`, and register your other filters by name), then the queries.

### Analyzer CSE3001

The package ships an analyzer for the parameterless `IgnoreQueryFilters()`, which turns off every filter, the tenant filter included. It runs only when the referenced EF Core has the named overload (EF Core 10 or later).

| ID | Severity | Message |
|---|---|---|
| CSE3001 | Info | `IgnoreQueryFilters()` ignores every query filter of the entity type. Pass the names of the filters to ignore. |

It is Info so that apps still on the anonymous filter do not break. Raise it once your filters are named:

```ini
[*.cs]
dotnet_diagnostic.CSE3001.severity = warning
```

For entities that implement `ISoftDeletableBase`, the code fix "Ignore only the named 'SoftDelete' filter" rewrites the call to `IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete })` and adds the `using`. Apply it only after the model registers the named filter. Other entities get no fix, because only you know which filters the query should skip. To keep a call that must see every row, suppress it with `[SuppressMessage("CSharpEssentials.EntityFrameworkCore", "CSE3001", Justification = "...")]`.

### Batch soft delete and hard delete

```csharp
int rows = await db.Orders.Where(o => o.CustomerId == id).SoftDeleteAsync(deletedBy: user, timeProvider, ct);
int same = await db.Orders.Where(o => o.CustomerId == id).SoftDeleteAsync(deletedAt: DateTimeOffset.UtcNow, deletedBy: user, ct);

db.HardDelete(order);                  // MarkAsHardDeleted() + Remove: the audit interceptor keeps it a real DELETE
db.HardDelete(oldOrders);              // the same for a sequence
db.Orders.HardDelete(order);           // on a DbSet
db.Orders.Delete(oldOrders);           // also a hard delete, despite the name
await db.SaveChangesAsync(ct);
```

- `SoftDeleteAsync` works for any `ISoftDeletable` entity. It runs one `ExecuteUpdate` with `AND NOT IsDeleted`, so rows already deleted keep their original `DeletedAt` and `DeletedBy`. Like every `ExecuteUpdate`, it skips the change tracker, interceptors and domain events, and needs a relational provider. Use `MarkAsDeleted` and `SaveChanges` when audit or events must run.
- `HardDelete` and `Delete` need an `ISoftDeletableEntityBase` entity.

## Offset pagination

```csharp
PaginationResponse<Product> page = await db.Products
    .OrderBy(p => p.Name)
    .PaginateAsync(pageNumber: 2, pageSize: 20, cancellationToken: ct);

PaginationResponse<Product> search = await db.Products
    .OrderBy(p => p.Name)
    .PaginateAsync(
        new PaginationRequest { PageNumber = 1, PageSize = 10, Search = " pen " },
        search: term => p => p.Name.Contains(term),
        cancellationToken: ct);

PaginationResponse<Product> inMemory = products.AsQueryable().Paginate(pageNumber: 1, pageSize: 10);
```

- `PaginationResponse<T>` has `Items`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage` and `HasNextPage`.
- It runs a `COUNT` and a data query. With `includeTotalCount: false` no `COUNT` runs and `TotalCount` is `-1`.
- The request is normalized first: page number and size are raised to 1, the size is lowered to `maxPageSize` (default `PaginationDefaults.MaxPageSize`, 100), and `Search` is trimmed. Pass `maxPageSize: 500` for larger pages, or `int.MaxValue` to remove the cap. A cap below 1 throws `ArgumentOutOfRangeException`.
- The `search` delegate runs only when `Search` is not empty, and before the count.
- `IPaginationRequest.Normalize()` and `Normalize(int maxPageSize)` are default interface methods; call them through the interface type.
- Add an `OrderBy` yourself; paging without an order gives unstable pages.

## Keyset pagination

`KeysetPaginateAsync` pages by a composite key, for example "by title, then by id". It sends one query that reads `limit + 1` rows, never runs a `COUNT`, and returns opaque cursors for the next and previous page. Its cost does not grow with the page number, and rows inserted between requests do not shift the pages.

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using CSharpEssentials.ResultPattern;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();
using AppDb db = new(new DbContextOptionsBuilder<AppDb>().UseSqlite(connection).Options);
db.Database.EnsureCreated();
db.Articles.AddRange(new[] { "Delta", "Alpha", "Echo", "Bravo", "Golf", "Charlie", "Foxtrot" }.Select(t => new Article { Title = t }));
db.SaveChanges();

// Build the ordering once; it is immutable. End with a unique key.
KeysetOrdering<Article> byTitle = new KeysetOrdering<Article>().Ascending(a => a.Title).Ascending(a => a.Id);

string? after = null;
KeysetPaginationResponse<Article> page;
do
{
    Result<KeysetPaginationResponse<Article>> result =
        await db.Articles.KeysetPaginateAsync(new KeysetPaginationRequest { Limit = 3, After = after }, byTitle);
    page = result.Value;
    Console.WriteLine(string.Join(", ", page.Items.Select(a => a.Title)));
    after = page.NextCursor;
}
while (after is not null);
// Alpha, Bravo, Charlie / Delta, Echo, Foxtrot / Golf

Result<KeysetPaginationResponse<Article>> back =
    await db.Articles.KeysetPaginateAsync(new KeysetPaginationRequest { Limit = 3, Before = page.PreviousCursor }, byTitle);
Console.WriteLine(string.Join(", ", back.Value.Items.Select(a => a.Title)));   // Delta, Echo, Foxtrot

Result<KeysetPaginationResponse<Article>> bad =
    await db.Articles.KeysetPaginateAsync(new KeysetPaginationRequest { After = "not-a-cursor" }, byTitle);
Console.WriteLine($"{bad.FirstError.Type}: {bad.FirstError.Code}");             // Validation: KeysetPagination.InvalidCursor

public sealed class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();
}
```

The inline form `KeysetPaginateAsync(request, k => k.Descending(x => x.CreatedAt).Descending(x => x.Id), ct)` builds a new ordering on each call. Every overload also takes a `KeysetPaginationOptions`.

### Requests and responses

- `KeysetPaginationRequest` has `Limit` (default 10), `After` and `Before`. Send `NextCursor` as `After` for the next page and `PreviousCursor` as `Before` for the previous one. Neither gives the first page.
- `KeysetPaginationResponse<T>` has `Items`, `NextCursor`, `PreviousCursor`, `HasNext` and `HasPrevious`. `Items` is always in the configured order, also for a `Before` page.
- `HasNext` and `HasPrevious` come from the extra row, not from a count. A page reached with `After` always reports a previous page, and a page reached with `Before` always reports a next page. An empty page has no cursors.
- The keys replace any `OrderBy` already on the query. Directions can be mixed.

### Errors

A bad request never throws. It returns `Error.Validation` with one of these codes from `KeysetCursorErrors`, and the metadata key `parameter` names the field (`after` or `before`). Map it to HTTP 400.

| Code | Cause |
|---|---|
| `KeysetPagination.InvalidCursor` | not a cursor, tampered, longer than `MaxCursorLength`, or rejected by the protector |
| `KeysetPagination.UnsupportedCursorVersion` | a cursor format this version cannot read |
| `KeysetPagination.CursorDirectionMismatch` | a `NextCursor` sent as `Before`, or the reverse |
| `KeysetPagination.CursorKeyMismatch` | a cursor from another ordering or entity |
| `KeysetPagination.AfterAndBefore` | both `After` and `Before` set |

### Options

| `KeysetPaginationOptions` | Default | |
|---|---|---|
| `MaxLimit` | 100 | `Limit` is clamped to `1..MaxLimit` (at most `int.MaxValue - 1`) |
| `MaxCursorLength` | 2048 | longer cursors are rejected before decoding; raise it if a protector makes long cursors |
| `Protector` | `NoOpCursorProtector` | signs or encrypts cursors |
| `PredicateBuilder` | `KeysetPredicateBuilder.Instance` | writes the `WHERE` condition |

### Cursor format and protection

A cursor is base64url (no padding) of a small JSON payload: `{"v":1,"d":"a","k":"<key fingerprint>","p":[<key values>]}`. `d` is the direction, `k` a hash of the entity type and the key paths, types and directions, and `p` the typed key values. `DateTime` keeps its `Kind`, `DateTimeOffset` its offset, enums are stored as numbers, and non-finite `double`/`float` values as `"NaN"`, `"Infinity"` and `"-Infinity"`.

The cursor is encoded, not encrypted: a client can read the key values and build a cursor. That is safe for the query, because every value is a typed SQL parameter. To hide the values or reject client-built cursors, implement `ICursorProtector`. The package does not reference ASP.NET Core, so a Data Protection wrapper is yours to write:

```csharp
public sealed class DataProtectionCursorProtector(IDataProtectionProvider provider) : ICursorProtector
{
    private readonly IDataProtector _protector = provider.CreateProtector("KeysetCursor.v1");

    public string Protect(string cursor) => _protector.Protect(cursor);

    public bool TryUnprotect(string protectedCursor, out string cursor)
    {
        try { cursor = _protector.Unprotect(protectedCursor); return true; }
        catch (Exception ex) when (ex is CryptographicException or FormatException) { cursor = ""; return false; }
    }
}

KeysetPaginationOptions options = new() { Protector = protector, MaxLimit = 50 };
Result<KeysetPaginationResponse<Order>> page = await db.Orders.KeysetPaginateAsync(request, ordering, options, ct);
```

`TryUnprotect` must not throw. Return `false` and the request gets `InvalidCursor`.

### Predicate

The default `KeysetPredicateBuilder` writes the expanded form with a redundant condition on the first key, so the database can use an index on it:

```sql
-- Descending(CreatedAt).Ascending(Name).Ascending(Id), After cursor
WHERE "CreatedAt" <= @p0 AND ("CreatedAt" < @p0 OR ("CreatedAt" = @p0 AND ("Name" > @p1 OR ("Name" = @p1 AND "Id" > @p2))))
ORDER BY "CreatedAt" DESC, "Name", "Id"
LIMIT @p3
```

Cursor values are always parameters, so the SQL text and the cached plan are the same for every page. Strings compare with `string.Compare`, `Guid` with `CompareTo` and enums by their numeric value, which EF Core translates to `<`/`>`. The comparison therefore follows the database's own ordering, the same as the `ORDER BY`. To write a different predicate (for example a row-value comparison on PostgreSQL), implement `IKeysetPredicateBuilder` and set `PredicateBuilder`. Each `KeysetColumn` carries the column expression, the cursor value expression, the CLR type and the direction (already reversed for `Before` pages).

### Limitations

- Supported key types: `int`, `long`, `short`, `byte`, `decimal`, `double`, `float`, `string`, `Guid`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `DateOnly`, `TimeOnly` and enums (except `ulong`-based ones).
- Nullable keys (`int?`, a `string?` with nullable annotations) throw `ArgumentException` when the ordering is built, because SQL `NULL` breaks `<`/`>`.
- Keys must be property or field accesses (`x => x.CreatedAt`, `x => x.Audit.CreatedAt`), not computed expressions. A path through a nullable navigation (`x => x.Audit!.CreatedAt`) throws `ArgumentException`.
- The provider must order and compare the key type. SQLite cannot for `decimal`, `DateTimeOffset` and `TimeSpan` and throws `NotSupportedException`; SQL Server and PostgreSQL can.
- Enum keys must be stored as numbers. Enums stored as strings (`ConfigureEnumConventions`, `[StringEnum]`, `HasConversion<string>()`) cannot be keys.
- Rows whose key values change between requests can move between pages.
- There is no total count. Run `CountAsync` yourself if you need one.

### The old cursor overload

`PaginateAsync(ICursorPaginationRequest<TCursor>, cursorSelector, ...)` is `[Obsolete]` (CS0618) and will be removed in a later major version. It filters by one column, so a non-unique column such as a timestamp skips rows at page borders, and it has no upper bound on `Limit`. Move to `KeysetPaginateAsync` with a tie-breaker key:

```csharp
// Before
var old = await db.Logs.PaginateAsync(
    new CursorPaginationRequest<DateTime> { Limit = 20, Cursor = lastCreatedAt }, x => x.CreatedAt, isAscending: false);

// After
Result<KeysetPaginationResponse<Log>> page = await db.Logs.KeysetPaginateAsync(
    new KeysetPaginationRequest { Limit = 20, After = nextCursor },
    k => k.Descending(x => x.CreatedAt).Descending(x => x.Id), ct);
```

Old raw cursors are not accepted by `KeysetPaginateAsync`; clients start again from the first page once.

## Result helpers and seed data

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();
using AppDb db = new(new DbContextOptionsBuilder<AppDb>().UseSqlite(connection).Options);
db.Database.EnsureCreated();

CountrySeed[] seed = [new("TR", "Turkey"), new("DE", "Germany")];

// Adds the rows only when the precondition is false, then saves.
await db.MigrateDataAsync<Country, CountrySeed>(seed, (set, data) => set.Any(), s => new Country { Code = s.Code, Name = s.Name });
Console.WriteLine($"Countries: {db.Countries.Count()}");                                    // 2

// Key-based sync: updates changed rows, adds new ones, removes rows missing from the seed.
CountrySeed[] next = [new("TR", "Türkiye"), new("FR", "France")];
await db.MigrateDataAsync(next, new MigrateDataOptions<Country, CountrySeed, string>
{
    Query = set => set,
    EntityKeyProperty = c => c.Code,
    DataKeyProperty = s => s.Code,
    IsUpdatedFunc = (c, s) => c.Name != s.Name,
    UpdateFunc = (c, s) => { c.Name = s.Name; return c; },
    Converter = s => new Country { Code = s.Code, Name = s.Name },
});
Console.WriteLine(string.Join(", ", db.Countries.OrderBy(c => c.Code).Select(c => c.Name)));  // France, Türkiye

Result<Country> tr = await db.Countries.Where(c => c.Code == "TR").FirstOrDefaultAsResultAsync();
Result<Country> xx = await db.Countries.FindAsResultAsync(["XX"], Error.NotFound("Country.NotFound", "No such country"));
Console.WriteLine($"{tr.Value.Name}; {xx.FirstError.Code}");                                // Türkiye; Country.NotFound

db.ChangeTracker.Clear();
db.Countries.Add(new Country { Code = "TR", Name = "Duplicate" });
Result saved = await db.SaveChangesAsResultAsync();
Console.WriteLine($"{saved.IsFailure}: {saved.FirstError.Type}");                           // True: Unknown

public sealed record CountrySeed(string Code, string Name);

public sealed class Country
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Country> Countries => Set<Country>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Country>().HasKey(c => c.Code);
}
```

| Method | Returns |
|---|---|
| `FirstOrDefaultAsResultAsync(notFoundError?, ct)` | the first row, or `notFoundError` (default `Error.NotFound()`) |
| `SingleOrDefaultAsResultAsync(notFoundError?, ct)` | the single row, or the error; more than one row still throws |
| `FindAsResultAsync(keyValues, notFoundError?, ct)` | `DbSet.FindAsync`, or the error |
| `SaveChangesAsResultAsync(ct)` | `Result`; every exception becomes an `ErrorType.Unknown` error |
| `SaveChangesAsResultAsync(acceptAllChangesOnSuccess, ct)` | `Result<int>` with the row count |
| `AsNoTracking(bool isDisabled)` | `AsNoTracking()` unless `isDisabled` is `true` |

`SaveChangesAsResultAsync` lets an `OperationCanceledException` from your own token through. Use `DbErrorTranslation` when you want specific errors for constraint violations.

`MigrateDataAsync` with `MigrateDataOptions` also has `PreConditionFunc` (skip everything when it returns `true`) and `HardDeleteMode`. With `HardDeleteMode = false` a soft-deletable row missing from the seed goes through the normal delete (a soft delete with the audit interceptor); with `true` it is marked with `MarkAsHardDeleted()` first. Both overloads call `SaveChangesAsync`.

## Transaction runner

`EfCoreTransactionRunner<TDbContext>` implements `ITransactionRunner` and `ITransactionCallbacks` from `CSharpEssentials.Core` (namespace `CSharpEssentials.Transactions`). The Mediator `TransactionBehavior` uses it, and so can your own code. `ISqlSession` lets raw ADO.NET or Dapper code run on the same connection and transaction.

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using System.Data.Common;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Sql;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Transactions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();

ServiceCollection services = new();
services.AddDbContext<AppDb>(options => options.UseSqlite(connection));
services.AddEfCoreTransactionRunner<AppDb>();   // scoped ITransactionRunner and ITransactionCallbacks
services.AddDbContextSqlSession<AppDb>();       // scoped ISqlSession: the context's connection and transaction

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
AppDb db = scope.ServiceProvider.GetRequiredService<AppDb>();
ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
ITransactionCallbacks callbacks = scope.ServiceProvider.GetRequiredService<ITransactionCallbacks>();
ISqlSession session = scope.ServiceProvider.GetRequiredService<ISqlSession>();
db.Database.EnsureCreated();

Result placed = await runner.ExecuteAsync(async ct =>
{
    db.Orders.Add(new Order { Number = "A-1" });
    await db.SaveChangesAsync(ct);

    await using DbCommand command = session.Connection.CreateCommand();
    command.Transaction = session.Transaction;   // pass it to every command
    command.CommandText = "UPDATE Orders SET Number = Number || '-checked'";
    await command.ExecuteNonQueryAsync(ct);

    await callbacks.OnCommittedAsync(_ => { Console.WriteLine("after commit: A-1"); return ValueTask.CompletedTask; }, ct);
    return Result.Success();
}, outcome => outcome.IsSuccess);
// after commit: A-1

Result rejected = await runner.ExecuteAsync(async ct =>
{
    db.Orders.Add(new Order { Number = "B-1" });
    await db.SaveChangesAsync(ct);
    await callbacks.OnCommittedAsync(_ => { Console.WriteLine("never printed"); return ValueTask.CompletedTask; }, ct);
    return Result.Failure(Error.Validation("Order.Rejected", "Rejected"));
}, outcome => outcome.IsSuccess);   // false: roll back

db.ChangeTracker.Clear();
Console.WriteLine(string.Join(", ", db.Orders.Select(o => o.Number)));   // A-1-checked

public sealed class Order
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
```

`ExecuteAsync(work, shouldCommit, ct)`: `work` returns a `ValueTask<T>`, `shouldCommit` decides whether to commit, and a `work` that throws rolls back.

- The outermost call runs inside `Database.CreateExecutionStrategy()` and opens a transaction, so retrying strategies such as `EnableRetryOnFailure` work. A retry runs the whole unit again, nested calls included, on a cleared change tracker. With a retrying strategy the context must have no pending changes when the call starts; the runner throws `InvalidOperationException` otherwise.
- A nested call on the same context instance joins the outer transaction. When the provider supports savepoints, the nested work runs behind one and a failure rolls back to it, so the outer call still decides. Entities the nested work saved stay tracked after that rollback; reload or detach them if the outer work continues. Without savepoints (SQL Server with MARS), a failed nested call keeps its writes.
- Only the same `DbContext` instance is joined. Another context or connection has its own transaction.
- It needs a relational provider. The InMemory provider throws on `BeginTransactionAsync`. Retrying strategies refuse to run inside an ambient `TransactionScope`, so do not combine it with `TransactionScopeBehavior`.
- If a commit fails with a transient error after the database applied it, the retry runs the work again. Make the work idempotent when you retry.

### After-commit callbacks

`OnCommittedAsync(callback, ct)` queues a side effect, such as evicting a cache entry, that runs only after the data is committed.

- Callbacks run in order after the outermost commit, after the execution strategy has returned, so a retried attempt never runs them twice.
- A callback gets `CancellationToken.None`. The data is already committed, so a request cancelled right after the commit still runs the side effect. Add your own timeout inside if needed.
- A unit that does not commit, throws or is retried drops its callbacks. A nested unit that rolls back to its savepoint drops only its own.
- With no transaction open, the callback runs at once. Inside a transaction the runner did not start (`BeginTransactionAsync` called by hand, or a `TransactionScope`), it throws `InvalidOperationException`.
- Callbacks are kept per `DbContext` instance, so any runner over that context sees them.
- When callbacks throw, the rest still run and the runner throws an `AggregateException`. The transaction is committed at that point.

### SQL sessions

| Registration | Service | Use for |
|---|---|---|
| `AddDbContextSqlSession<TContext>()` | scoped `ISqlSession` and `ISqlConnectionFactory` (`DbContextSqlSession<TContext>`) | SQL that must commit with the `DbContext` work |
| `AddDbDataSourceReadOnlyConnectionFactory(dataSource?)` | singleton `ISqlReadOnlyConnectionFactory` over the registered `DbDataSource` | reads outside any transaction |

- `ISqlSession.Transaction` is `null` when no transaction is open. Pass it to every command; most providers reject a command without it while one is open.
- `ISqlSession.Connection` and `ISqlConnectionFactory.Create()` return the context's own connection. Do not dispose or close it.
- `AddDbContextSqlSession` replaces any `ISqlConnectionFactory` registered before it. Code that wraps `factory.Create()` in `using` must move to `ISqlSession`.
- A connection from `DbDataSourceSqlConnectionFactory` is new and closed. The caller opens and disposes it.

## Outbox

`EfCoreDomainEventOutbox` stores `AfterSave` domain events as `OutboxMessage` rows in the context that raised them, in the same `SaveChanges` as the entities. `IOutboxProcessor` later reads the rows and passes each event to your `IOutboxMessageHandler`.

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using System.Text.Json.Serialization;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();

ServiceCollection services = new();
services.AddLogging();
services.AddAuditInterceptor(() => "system");
services.AddDbContext<AppDb>(options => options.UseSqlite(connection));
services.AddEfCoreOutbox(types => types.Add("orders.placed", AppJson.Default.OrderPlaced));
services.AddScoped<IOutboxMessageHandler, PrintHandler>();
services.AddOutboxProcessor<AppDb>(options => options.BatchSize = 100);

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
AppDb db = scope.ServiceProvider.GetRequiredService<AppDb>();
db.Database.EnsureCreated();

db.Orders.Add(Order.Place());
await db.SaveChangesAsync();   // the order row and its outbox row, in one transaction
Console.WriteLine($"Pending: {db.Set<OutboxMessage>().Count(m => m.ProcessedAt == null)}");   // Pending: 1

int claimed = await provider.GetRequiredService<IOutboxProcessor>().ProcessBatchAsync();
// handled orders.placed: OrderPlaced { EventId = ..., OccurredAt = ..., OrderId = ... }
Console.WriteLine($"Claimed: {claimed}, pending: {db.Set<OutboxMessage>().Count(m => m.ProcessedAt == null)}");   // Claimed: 1, pending: 0

int purged = await db.PurgeProcessedOutboxMessagesAsync(DateTime.UtcNow.AddMinutes(1));
Console.WriteLine($"Purged: {purged}");   // Purged: 1

public sealed record OrderPlaced(Guid OrderId) : DomainEvent;

[JsonSerializable(typeof(OrderPlaced))]
internal sealed partial class AppJson : JsonSerializerContext;

public sealed class Order : EntityBase<Guid>
{
    public static Order Place()
    {
        Order order = new() { Id = Guid.NewGuid() };
        order.Raise(new OrderPlaced(order.Id));
        return order;
    }
}

public sealed class AppDb(DbContextOptions<AppDb> options, IServiceScopeFactory scopes) : BaseDbContext<AppDb>(options, scopes)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override DbContextInterceptors InterceptorsFromServices => DbContextInterceptors.Audit;

    protected override bool DispatchDomainEventsOnSaveChanges => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().EntityBaseGuidIdMap();
        modelBuilder.AddOutboxMessages();
    }
}

public sealed class PrintHandler : IOutboxMessageHandler
{
    public Task HandleAsync(OutboxMessage message, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Console.WriteLine($"handled {message.Type}: {domainEvent}");
        return Task.CompletedTask;
    }
}
```

### Storing

- `AddEfCoreOutbox(types => ...)` registers the scoped `IDomainEventOutbox`, the `OutboxEventTypeRegistry` and `JsonOutboxMessageSerializer` (unless an `IOutboxMessageSerializer` is already registered). It works with `DispatchDomainEventsOnSaveChanges` and with `DomainEventInterceptor`.
- `AddOutboxMessages(options => ...)` maps the table (`OutboxMessages` by default; `TableName`, `Schema`), the key and an index on (`ProcessedAt`, `DeadLetteredAt`, `NextAttemptAt`). Add a migration after calling it.
- A row keeps the event's `EventId` as `Id` and its `OccurredAt` when the event implements `IIdentifiedDomainEvent` (the `DomainEvent` record does). Other events get a new version 7 id and the current time of the registered `TimeProvider`.
- `Type` is the name you registered, not the CLR name, so you can rename or move event classes. Saving an event type that is not registered throws `InvalidOperationException`, writes nothing and puts the events back on their entities.
- `Add<TEvent>(name, JsonTypeInfo<TEvent>)` uses source-generated metadata and is safe for trimming and Native AOT. `Add<TEvent>(name, JsonSerializerOptions?)` uses reflection (`RequiresUnreferencedCode`).
- A custom `IDomainEventOutbox` gets the saving context in `StoreAsync(DbContext context, IReadOnlyList<IDomainEvent> events, ct)`. It may add rows to that context but must not call `SaveChanges`.

### Relaying

`AddOutboxProcessor<TContext>(options => ...)` registers the singleton `IOutboxProcessor`. Call `ProcessBatchAsync()` from a loop or a job, or use `AddOutboxProcessorHostedService()` from [CSharpEssentials.EntityFrameworkCore.Hosting](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore.Hosting/Readme.MD).

| `OutboxProcessorOptions` | Default | |
|---|---|---|
| `BatchSize` | 50 | rows claimed per batch |
| `MaxAttempts` | 10 | a failure on this attempt dead-letters the row |
| `LeaseDuration` | 5 min | how long a claim lasts |
| `RetryBaseDelay`, `RetryMaxDelay`, `UseJitter` | 5 s, 10 min, `true` | the backoff of `OutboxRetryBackoff.Compute` |
| `RetryDelay` | `null` | your own `Func<int, TimeSpan>` instead of the backoff |
| `MaxErrorLength` | 2000 | length of the stored `Error` text |
| `PollingInterval` | 5 s | used by the hosted service |
| `Claimer` | `ConditionalUpdateOutboxMessageClaimer.Instance` | how rows are claimed |

- Delivery is at least once. A crash or a lost completion delivers a message again, so dedupe downstream on `OutboxMessage.Id`.
- A claim sets `ClaimToken`, adds one to `Attempts` and moves `NextAttemptAt` to the end of the lease. Rows of a processor that stopped mid-batch are claimed again after the lease. Completion, retry and dead-letter updates check the claim token, so an expired processor cannot overwrite a newer claim.
- A handler that throws, or a type name that is not registered, stores `Error` (exception type and message) and schedules a retry. The delay is `RetryBaseDelay` doubled per attempt, capped at `RetryMaxDelay`, with jitter in the upper half when `UseJitter` is on.
- On attempt `MaxAttempts` a failure sets `DeadLetteredAt`, and the row is never claimed again. To replay it, clear `DeadLetteredAt` and reset `Attempts`.
- The lease is checked before each message. Messages left when it has ended are released without counting the attempt. Keep `LeaseDuration` longer than a batch takes.
- Handlers are resolved in a new scope per message. `ProcessBatchAsync` returns the number of claimed rows. A database error or cancellation is thrown from it; a handler error never is.
- The processor never deletes rows. `context.PurgeProcessedOutboxMessagesAsync(processedBefore, ct)` deletes rows processed before a UTC time in one statement and returns the count. Pending and dead-lettered rows are kept.

### Claimers

The default claimer works on any relational provider: it reads the ids of the oldest due rows, then claims them with one conditional `UPDATE`, so two processors never claim the same row. With many processors on a busy table, switch to a statement that skips locked rows:

```csharp
services.AddOutboxProcessor<AppDb>(options => options.Claimer = new SqlOutboxMessageClaimer(OutboxClaimDialect.PostgreSql));
```

`OutboxClaimSql.Build(context, dialect)` writes the statement from the mapped names. For the default mapping:

```sql
-- OutboxClaimDialect.PostgreSql
UPDATE "OutboxMessages" SET "ClaimToken" = {0}, "NextAttemptAt" = {1}, "Attempts" = "Attempts" + 1
WHERE "Id" IN (SELECT "Id" FROM "OutboxMessages"
               WHERE "ProcessedAt" IS NULL AND "DeadLetteredAt" IS NULL AND "NextAttemptAt" <= {2}
               ORDER BY "OccurredAt", "Id" LIMIT {3} FOR UPDATE SKIP LOCKED)
RETURNING *

-- OutboxClaimDialect.SqlServer
WITH claimed AS (SELECT TOP ({3}) * FROM [OutboxMessages] WITH (UPDLOCK, READPAST, ROWLOCK)
                 WHERE [ProcessedAt] IS NULL AND [DeadLetteredAt] IS NULL AND [NextAttemptAt] <= {2}
                 ORDER BY [OccurredAt], [Id])
UPDATE claimed SET [ClaimToken] = {0}, [NextAttemptAt] = {1}, [Attempts] = [Attempts] + 1 OUTPUT inserted.*
```

`OutboxClaimDialect.Sqlite` is the PostgreSQL form without `FOR UPDATE SKIP LOCKED`. For another database pass your own statement to `new SqlOutboxMessageClaimer(sql)`, with the placeholders `{0}` claim token, `{1}` lease end, `{2}` now and `{3}` batch size; it must return every column of the claimed rows. You can also implement `IOutboxMessageClaimer.ClaimAsync(DbContext context, OutboxClaimRequest request, CancellationToken cancellationToken)`; `OutboxClaimRequest` carries `ClaimToken`, `BatchSize`, `Now` and `LeaseUntil`. The SQLite statement and the default claimer are tested against SQLite; the PostgreSQL and SQL Server statements are checked as text only.

## Idempotency records

`EfCoreIdempotencyRecordStore<TContext>` is an `IIdempotencyRecordStore` (from `CSharpEssentials.Idempotency`) that keeps its records in your `DbContext` and writes them in the context's current transaction. A record commits and rolls back with the business rows of the same unit of work, so a request whose transaction rolls back leaves no record and its retry runs again.

```csharp
public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.AddIdempotencyRecords(Database.ProviderName);   // then add a migration
}

services.AddDbContext<AppDb>(options => options.UseNpgsql(connectionString));
services.AddEfCoreTransactionRunner<AppDb>();
services.AddMediatorTransactionRunnerBehavior();
services.AddMediatorIdempotencyBehavior(options => options.Placement = IdempotencyPlacement.InsideTransaction);
services.AddEfCoreIdempotencyRecordStore<AppDb>();             // replaces the in-memory store, in any order
services.AddEfCoreIdempotencyRecordStore<AppDb>("payments");   // or a keyed store for profiles with StoreKey = "payments"
```

### Table

`AddIdempotencyRecords(providerName, configure?)` maps `IdempotencyRecord` to `IdempotencyRecords` (`TableName`, `Schema`, `KeyCollation` options) with the primary key `KeyHash` and an index on `ExpiresAt`. `KeyHash` is a `binary(32)` SHA-256 of the length-prefixed storage key (namespace, scope and key). The readable `Namespace`, `Scope` and `Key` columns sit next to it. A global key has the scope `global`; `ExpiresAt` is UTC.

### Behavior

- Uniqueness never depends on collation: two keys share a row only when their hashes match, so `KEY-A` and `key-a`, or `order-1` and `order-1 `, stay separate. A row whose readable columns belong to another key makes the reservation throw `InvalidOperationException` instead of replaying a foreign response.
- `AddIdempotencyRecords(Database.ProviderName)` gives the readable columns a binary collation on PostgreSQL (`C`), SQL Server (`Latin1_General_100_BIN2`), MySQL (`utf8mb4_0900_bin`) and SQLite (`BINARY`), so your own queries on them are case-sensitive. `KeyCollation` overrides it, for example `utf8mb4_nopad_bin` on MariaDB. On PostgreSQL keep those columns `text` or `varchar`, not `char(n)`. SQL Server ignores trailing spaces in comparisons under every collation, so there only `KeyHash` keeps `order-1` and `order-1 ` apart.
- On PostgreSQL and SQLite a reservation is one `INSERT ... ON CONFLICT DO NOTHING`. On other providers it is an `INSERT` behind a savepoint, rolled back to the savepoint on a duplicate key so the transaction stays usable.
- A concurrent request for the same key waits on the primary key until the first transaction ends, then replays the stored response, or takes the key when the first one rolled back. Complete and release change the row only while it carries the reservation's token. An expired row counts as absent.
- The store needs `READ COMMITTED` (the default on PostgreSQL, SQL Server and MySQL). Under `REPEATABLE READ` or `SERIALIZABLE` a concurrent reservation fails with a serialization error.
- Records are written with SQL, not the change tracker, so `SaveChanges`, interceptors and query filters do not see them.
- Expired rows stay until `PurgeExpiredAsync()` deletes them in one statement. Run it from a job, for example a `BackgroundService` that resolves `EfCoreIdempotencyRecordStore<AppDb>` in a scope every hour.

### Observe mode

In `Observe` mode the executor and the `Idempotency-Key` middleware reserve and release through `store.Detached`, which uses a new `TContext` in its own scope, so a reservation never waits on or fails the business transaction. The result itself is still written in the business transaction. A failed write rolls back to a savepoint and the business data commits without the record; on SQL Server a transaction the server has doomed (`XACT_ABORT ON`, error 3960) is lost with the business data. The context must not depend on state that only the request scope has. The [Idempotency README](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD) covers the modes.

## Database error translation

`DbErrorTranslation` turns persistence exceptions into `Error` values, which the AspNetCore package renders as ProblemDetails. The built-in `SqlStateErrorTranslator` reads `DbException.SqlState` from the exception or an inner exception, so it needs no provider reference:

| SQLSTATE | Meaning | Error | Code |
|---|---|---|---|
| `23505` | unique violation | `Conflict` (409) | `Database.UniqueViolation` |
| `23503` | foreign key violation | `Conflict` (409) | `Database.ForeignKeyViolation` |
| `23514` | check violation | `Validation` (400) | `Database.CheckViolation` |
| `23502` | not null violation | `Validation` (400) | `Database.NotNullViolation` |
| `40001` | serialization failure | `Conflict` (409), `retryable: true` | `Database.SerializationFailure` |
| `40P01` | deadlock | `Conflict` (409), `retryable: true` | `Database.Deadlock` |

The metadata has `sqlState` and, when the exception is a `DbUpdateException` with entries, `entities` (their entity type names). The database message is not copied, because it can contain row values.

PostgreSQL (Npgsql) sets `SqlState`. SQL Server and SQLite do not, so they need an `IDbErrorTranslator` of your own. This program translates SQLite's unique constraint error:

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();

ServiceCollection services = new();
services.AddDbContext<AppDb>(options => options.UseSqlite(connection));
services.AddDbErrorTranslation();                              // the SQLSTATE translator
services.AddDbErrorTranslator<SqliteUniqueTranslator>();       // runs first

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
AppDb db = scope.ServiceProvider.GetRequiredService<AppDb>();
DbErrorTranslation translation = scope.ServiceProvider.GetRequiredService<DbErrorTranslation>();
db.Database.EnsureCreated();

db.Users.Add(new User { Email = "a@example.com" });
await translation.SaveChangesAsync(db);
db.Users.Add(new User { Email = "a@example.com" });
Result<int> saved = await translation.SaveChangesAsync(db);

Console.WriteLine($"{saved.FirstError.Type}: {saved.FirstError.Code}");   // Conflict: Database.UniqueViolation

public sealed class SqliteUniqueTranslator : IDbErrorTranslator
{
    private const int SqliteConstraintUnique = 2067;

    public bool TryTranslate(Exception exception, out Error error)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException { SqliteExtendedErrorCode: SqliteConstraintUnique })
            {
                error = Error.Conflict("Database.UniqueViolation", "A record with the same unique value already exists.");
                return true;
            }
        }

        error = default;
        return false;
    }
}

public sealed class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
}
```

- `translation.SaveChangesAsync(context, ct)` returns `Result<int>`: the row count, or the translated error. Exceptions no translator recognizes are rethrown, so unexpected failures still reach your exception handler. After a failure the change tracker keeps the unsaved changes.
- `translation.TryTranslate(exception, out Error error)` translates an exception you caught yourself.
- `AddDbErrorTranslator<T>()` translators run in registration order before the SQLSTATE default, and the first match wins. `DbErrorTranslation` is a singleton, so translators must be stateless.
- A translator can wrap `SqlStateErrorTranslator` to add details, for example the constraint name of `PostgresException`: call `new SqlStateErrorTranslator().TryTranslate(exception, out error)`, then `error.Metadata!.AddMetadata("constraint", postgres.ConstraintName)`.
- The default ProblemDetails fields of the AspNetCore package do not include metadata. With `ProblemErrorFields.AllErrors` the client also sees `sqlState`, `entities` and what your translators add.

## Slow query logging

`SlowQueryInterceptor` logs every command (reader, scalar or non-query) that runs for at least the threshold, as a warning with its text and parameters, and passes it to an `ISlowQueryHandler` when one is registered.

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();

ServiceCollection services = new();
services.AddLogging();
services.AddSlowQueryInterceptor(TimeSpan.Zero);   // log everything, to show the output; the default is 1 second
services.AddSingleton<ISlowQueryHandler, PrintHandler>();
services.AddDbContext<AppDb>((sp, options) => options
    .UseSqlite(connection)
    .AddInterceptors(sp.GetRequiredService<SlowQueryInterceptor>()));

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
AppDb db = scope.ServiceProvider.GetRequiredService<AppDb>();
db.Database.EnsureCreated();

string name = "Pen";
_ = await db.Items.Where(i => i.Name == name).ToListAsync();
// ReaderExecutedAsync ... ms, parameters: @name=?

public sealed class PrintHandler : ISlowQueryHandler
{
    public void OnSlowQuery(SlowQueryContext context)
    {
        if (context.CommandText.Contains("FROM \"Items\"", StringComparison.Ordinal))   // skip the schema queries
            Console.WriteLine($"{context.MethodName} {context.ElapsedTime.TotalMilliseconds:F1} ms, parameters: {context.Parameters}");
    }
}

public sealed class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
}
```

- `AddSlowQueryInterceptor(TimeSpan threshold)` or `AddSlowQueryInterceptor(options => options.Threshold = ...)` registers `SlowQueryOptions` and the singleton interceptor. Attach it with `AddInterceptors`, `DbContextInterceptors.SlowQuery` or `DbContextRegistrationOptions.EnableSlowQueryInterceptor`.
- Parameter values appear only when the context has `EnableSensitiveDataLogging()`. Otherwise each parameter reads `@name=?`, in the log and in `SlowQueryContext.Parameters`.
- `SlowQueryContext` has `CommandText`, `Parameters`, `ElapsedTime`, `Database`, `HasTransaction` and `MethodName`.
- If the handler throws, the exception is logged as a warning and the command still succeeds.

## Maybe columns

`Maybe<T>` is a struct, and EF Core cannot make a non-nullable struct property optional, so a plain `Maybe<T>` property maps to a `NOT NULL` column. Declare the property as `Maybe<T>?` to store absence as `NULL`:

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Maybe;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();
using AppDb db = new(new DbContextOptionsBuilder<AppDb>().UseSqlite(connection).Options);
db.Database.EnsureCreated();

db.Customers.AddRange(
    new Customer { Name = "Ada", Nickname = Maybe<string>.From("ada"), LoyaltyPoints = Maybe<int>.From(5) },
    new Customer { Name = "Bob" });   // both columns NULL
db.SaveChanges();
db.ChangeTracker.Clear();

List<string> withoutNickname = db.Customers.Where(c => c.Nickname == null).Select(c => c.Name).ToList();
List<string> fivePoints = db.Customers.Where(c => c.LoyaltyPoints == Maybe<int>.From(5)).Select(c => c.Name).ToList();
Console.WriteLine($"No nickname: {string.Join(",", withoutNickname)}; 5 points: {string.Join(",", fivePoints)}");   // No nickname: Bob; 5 points: Ada

Customer bob = db.Customers.Single(c => c.Name == "Bob");
Maybe<int> points = bob.LoyaltyPoints.Flatten();   // null becomes None
Console.WriteLine($"Bob has points: {points.HasValue}");   // Bob has points: False

public sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Maybe<string>? Nickname { get; set; }   // TEXT NULL
    public Maybe<int>? LoyaltyPoints { get; set; } // INTEGER NULL
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ConfigureNullableMaybeConventions();
}
```

- `ConfigureNullableMaybeConventions()` covers every public read-write `Maybe<T>?` property of entity, owned, derived (TPH) and complex types. It skips ignored and `[NotMapped]` properties and properties with your own `HasConversion`. It reads the CLR properties once while the model is built.
- Per property: `modelBuilder.Entity<Customer>().Property(c => c.Nickname).HasNullableMaybeConversion();`. It needs a concrete `T`; from unconstrained generic code the compiler reports CS0452. Use the convention there, or `HasConversion(new NullableMaybeConverter<T>())` / `new NullableMaybeReferenceConverter<T>()` (namespace `.Converters`).
- `null` and `None` are both written as `NULL`. `NULL` is read back as `null`, not `None`; call `Flatten()` to get a `Maybe<T>`.
- Query absence with `c.Nickname == null` (or `!c.Nickname.HasValue`, the `Nullable<T>.HasValue`), presence with `!= null`, and a value with `== Maybe<int>.From(5)`. Never compare with `Maybe<T>.None`: it is sent as a non-null parameter and matches no `NULL` row. Members of `Maybe<T>` itself (`.Value.HasValue`, `.Value.Value`) do not translate; the query throws `InvalidOperationException`.
- `Maybe<T?>?` (for example `Maybe<int?>?`) is not supported: EF Core fails the model build.
- A mutable value such as `Maybe<byte[]>?` changed in place is not saved. Assign a new value.
- NativeAOT: the convention closes generic types with `MakeGenericType` (`[RequiresDynamicCode]`). Use a compiled model (`dotnet ef dbcontext optimize`) or the per-property API.
- Changing a `Maybe<T>` property to `Maybe<T>?` turns the column from `NOT NULL` into `NULL`. Old `default(T)` values written for `None` stay; update them to `NULL` in the migration if they meant absence.
- `MaybeConversion<T>()` for a plain `Maybe<T>` is kept for compatibility but cannot store `None`: for a value type it writes `default(T)` and reads back `Some(default(T))`, for a reference type the insert fails. Prefer `Maybe<T>?`.
- Background: EF Core issues [#24685](https://github.com/dotnet/efcore/issues/24685), [#34943](https://github.com/dotnet/efcore/issues/34943) and [#13850](https://github.com/dotnet/efcore/issues/13850).

## Enum storage

`ConfigureEnumConventions` stores `[StringEnum]` enums (from `CSharpEssentials.Enums`) by their wire name, the same spelling as JSON and binding, and adds a check constraint per column:

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package CSharpEssentials.Enums
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.Enums;
using CSharpEssentials.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();
using AppDb db = new(new DbContextOptionsBuilder<AppDb>().UseSqlite(connection).Options);
db.Database.EnsureCreated();

db.Orders.Add(new Order { Status = OrderStatus.PendingApproval });
db.SaveChanges();

string stored = db.Database.SqlQueryRaw<string>("SELECT Status AS Value FROM Orders").Single();
Console.WriteLine(stored);   // pending_approval

try
{
    db.Database.ExecuteSqlRaw("UPDATE Orders SET Status = 'lost'");
}
catch (SqliteException ex)
{
    Console.WriteLine(ex.Message);   // SQLite Error 19: 'CHECK constraint failed: ck_Orders_Status_enum'.
}

// Lists stored values a conversion or the constraint would reject, with their counts.
string audit = EnumDataAudit.Sql<OrderStatus>("Orders", "Status", provider: db.Database.ProviderName);
Console.WriteLine(audit);

[StringEnum]
public enum OrderStatus
{
    Pending,
    PendingApproval,
    Shipped,
}

public sealed class Order
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ConfigureEnumConventions(EnumConventions.Default);
}
```

| Shape | Column | Check constraint `ck_{table}_{column}_enum` |
|---|---|---|
| `String` (default) | `text` on PostgreSQL, `TEXT` on SQLite, otherwise `n` = longest wire name rounded up to 16 | `"Status" IN ('pending', 'shipped')` |
| `Integer` (`[StringEnum(Storage = EnumStorage.Integer)]`, `HasEnumStorage`, `EnumConventions.Storage`) | underlying integer type | `"Priority" IN (0, 1, 2)` |
| `[Flags]` (integer by default) | underlying integer type | `("Permissions" & ~7) = 0` |
| `[Flags]` with `String` storage | `text[]` on PostgreSQL, JSON array text elsewhere | `<@ ARRAY[...]::text[]` on PostgreSQL |
| `List<TEnum>` | `text[]`/`integer[]` on PostgreSQL, JSON text elsewhere | `<@ ARRAY[...]` on PostgreSQL |
| `ToJson()` owned types | JSON column | none; `EnumJsonValueReaderWriter<TEnum>` writes wire names |

Per property:

```csharp
modelBuilder.Entity<Order>().Property(o => o.Priority).HasEnumStorage(EnumStorage.Integer);
modelBuilder.Entity<Order>().Property(o => o.Kind).HasEnumCheckConstraint(false);
modelBuilder.Entity<Order>().Property(o => o.Legacy).HasLegacyEnumStorage(EnumStoredAs.MemberName);
```

The same three methods exist for complex type properties.

- Precedence: `HasEnumStorage` on the property, then `[StringEnum(Storage = ...)]`, then `EnumConventions.Storage` (`FlagsStorage` for flags).
- Reads are tolerant (wire name, member name, aliases, any case); writes are canonical. An unknown stored value maps to the `[EnumFallback]` member, otherwise the query throws and the message names the column.
- Enums without `[StringEnum]` keep EF's integer column. `ConfigureEnumConventionsWithReflection` opts them in through reflection (not trim or AOT safe).
- Properties with your own `HasConversion` are skipped.
- Compiled models (`dotnet ef dbcontext optimize`) are not supported for properties these conventions configure.
- Adding a member changes only the constraint, so the migration is `DropCheckConstraint` and `AddCheckConstraint`. Turn constraints off per property with `HasEnumCheckConstraint(false)` or for all with `EnumConventions.CheckConstraints = false`.
- Renaming a member changes the stored value and the constraint; see [Renaming a member safely](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#renaming-a-member-safely).
- The converters are public in `.Converters` for use with `HasConversion`: `EnumWireNameConverter<TEnum>`, `EnumIntegerConverter<TEnum, TNumber>`, `EnumToFormattedStringConverter<TEnum>` and `LegacySnakeCaseEnumConverter<TEnum>`.

### An existing database

Existing enum columns hold integers (EF's default) or an older text format. Keep them as they are, so the first migration after you add the conventions is empty:

```csharp
configurationBuilder.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer);

modelBuilder.Entity<Order>().Property(o => o.Status).HasEnumStorage(EnumStorage.String);          // opt in one column
modelBuilder.Entity<Order>().Property(o => o.Kind).HasLegacyEnumStorage(EnumStoredAs.MemberName); // keep an old text format
```

`existingStorage` and `HasLegacyEnumStorage` keep writing the old format, read every spelling and add no check constraint. `EnumStoredAs` has `Integer`, `MemberName`, `CamelCase`, `LegacySnakeCase`, `Text` (any known spelling, a conversion source only) and `FlagsText`. The 4.x overloads (`ConfigureEnumConventions(params Assembly[])`, `EnumConventionOptions`) are obsolete; `UseLegacySnakeCase = true` maps to `existingStorage: EnumStoredAs.LegacySnakeCase`.

### Converting the data

When you remove `HasLegacyEnumStorage` (or `existingStorage`) from a column, `dotnet ef migrations add` generates an `AlterColumn` that changes the type without converting the data. Replace it with `ConvertEnumColumn` and keep the constraint operations around it:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropCheckConstraint(name: "ck_orders_Status_enum", table: "orders");   // when one exists
    migrationBuilder.ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);
    migrationBuilder.AddCheckConstraint(name: "ck_orders_Status_enum", table: "orders", sql: "...");   // as generated
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropCheckConstraint(name: "ck_orders_Status_enum", table: "orders");
    migrationBuilder.ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Text, to: EnumStorage.Integer);
}
```

| From | To | Unknown values |
|---|---|---|
| `Integer` | `EnumStorage.String` or a text format | kept as their number text; they fail the new constraint |
| `Text`, `MemberName`, `CamelCase`, `LegacySnakeCase` | `EnumStorage.String` or another text format | kept; they fail the new constraint |
| any text | `EnumStorage.Integer` | integer text is kept; other text aborts the migration with `cannot convert orders.Status to OrderStatus, unknown value: ...` |
| `FlagsText` (`"Read, Write"`) | `EnumStorage.Integer` | tokens are OR'ed; on PostgreSQL through a temporary column |
| flags `Integer` | `EnumStoredAs.FlagsText` | values with undefined bits stay numbers |

- `to` is an `EnumStorage` (`String` or `Integer`) or an `EnumStoredAs` text format. `schema:` sets the table schema and `type:` a non-default target store type such as `varchar(32)`.
- No conversion writes `NULL`. Text conversions skip canonical values, so running one twice changes nothing.
- SQL is generated for PostgreSQL and SQLite (from `MigrationBuilder.ActiveProvider`); other providers throw. On SQLite EF rebuilds the table.
- `ConvertEnumJsonPath<TEnum>("orders", "payload", ["status"])` rewrites one path of a PostgreSQL `jsonb` column (`to: EnumStorage.Integer` for `Down`). Unknown values are left alone.
- Diagnostic CSE0014 (a build error, reported by the `CSharpEssentials.Enums` generator) flags a generated `AlterColumn` left next to `ConvertEnumColumn` for the same column.

### Auditing the data

`EnumDataAudit.Sql<TEnum>(table, column, schema?, storedAs = EnumStoredAs.Text, provider?)` only builds a read-only query (`SELECT "Status" AS "Value", count(*) AS "Count" ... GROUP BY ...`) that lists the stored values a conversion or the constraint would reject. Run it in production before you deploy the migration, with your SQL client or through EF Core:

```csharp
string sql = EnumDataAudit.Sql<OrderStatus>("orders", "Status", storedAs: EnumStoredAs.Integer, provider: db.Database.ProviderName);
List<EnumAuditRow> rejected = await db.Database.SqlQueryRaw<EnumAuditRow>(sql).ToListAsync(ct);   // empty: every row converts

public sealed class EnumAuditRow
{
    public int Value { get; init; }    // the column's type: int for Integer, string for text columns
    public long Count { get; init; }
}
```

## Ordered GUID keys

EF Core and the provider choose how `Guid` keys are generated, and this package leaves that alone unless you opt in:

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using SqliteConnection connection = new("DataSource=:memory:");
connection.Open();
using AppDb db = new(new DbContextOptionsBuilder<AppDb>().UseSqlite(connection).Options);
db.Database.EnsureCreated();

foreach (string name in new[] { "first", "second", "third" })
{
    db.Events.Add(new Event { Name = name });
    db.SaveChanges();
}

foreach (Event e in db.Events.OrderBy(e => e.Id))
    Console.WriteLine($"{e.Id} {e.Name}");   // version 7 ids; ordering by Id gives first, second, third

public sealed class Event
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.UseOrderedGuidKeys();   // layout picked from the provider; or UseOrderedGuidKeys(OrderedGuidLayout.SqlServer)
}
```

For one property without the convention: `modelBuilder.Entity<Order>().Property(x => x.Id).HasValueGeneratorFactory<Version7GuidValueGeneratorFactory>();` (or `SqlServerGuidValueGeneratorFactory`). Compiled models support both factories.

The convention covers every non-nullable `Guid` primary key that EF Core generates on add. It skips foreign keys, keys with their own value generator, default value, default value SQL or computed SQL, and keys with a value conversion (such as `HasConversion<string>()`), whose stored form no longer sorts like the provider's GUID column. Anything configured explicitly wins.

| Layout | Values | Sorts by creation time in |
|---|---|---|
| `Version7` | `Guider.NewMonotonicGuid()`: RFC 9562 version 7, strictly increasing within the process | big-endian bytes, the string form and `Guid.CompareTo` |
| `SqlServer` | `Guider.NewSqlServerGuid()`: RFC 9562 version 8, milliseconds in the last 6 bytes, strictly increasing within the process | SQL Server `uniqueidentifier` order (`SqlGuid`) only |

| Provider | Column and comparison | Default generator without this package | Picked layout |
|---|---|---|---|
| SQL Server | `uniqueidentifier`; compares bytes 10 to 15 first, then 8 to 9, 6 to 7, 4 to 5 and 0 to 3 | `SequentialGuidValueGenerator`: ordered within one generator instance | `SqlServer` |
| PostgreSQL (Npgsql) | `uuid`; compares the 16 bytes in RFC order | version 4 on Npgsql 8; version 7 on Npgsql 9 and later | `Version7` |
| SQLite | `TEXT`, uppercase with hyphens; compares ordinally | version 4 | `Version7` |
| Pomelo MySQL | `GuidFormat` `Char36` (the default), `Char32` or `Binary16` keep RFC order; `TimeSwapBinary16` and `LittleEndianBinary16` reorder the bytes | ticks first, laid out per `GuidFormat`; sorts in every format except `TimeSwapBinary16` | none, unless you pass a layout |
| Any other provider (MySql.EntityFrameworkCore, in-memory, Cosmos, Oracle, MongoDB) | not checked by this package | the provider's | none, unless you pass a layout |

- On SQL Server the default generator is already sequential inside one app instance. It seeds a counter from the clock when the generator is created, so several instances, or one restarted instance, write into separate ranges of the index. `SqlServer` values follow the clock in every instance, so concurrent instances all append near the end of the index. In a single test run against SQL Server 2022 in a container (20,000 single-row inserts into a clustered key, `char(200)` rows), one process left 0.5% fragmentation with either generator. Four processes started 1 second apart left 96.9% fragmentation with the default generator and 11.2% with `SqlServer`, with pages 96% and 92% full. Version 4 and `NewMonotonicGuid()` left 99% fragmentation and pages 67% full with one and with four processes, and `NewGuid()` the same with one process.
- Npgsql 9 and later already generate version 7 values. Opting in there only makes values from one process strictly increasing inside a millisecond.
- On Pomelo, pass `Version7` only for `Char36`, `Char32` or `Binary16`. This package cannot see the `GuidFormat` of the connection.
- A `Version7` value sorts randomly in SQL Server, and a `SqlServer` value sorts randomly everywhere else. Pass a layout only when the default pick is wrong for your columns.
- The order holds within one process. Values from different processes interleave by their millisecond.

Security: both layouts show the creation time of a row, to the millisecond, to anyone who sees the key, and carry 56 random bits instead of the 122 of `Guid.NewGuid()`. Do not use them for keys that must stay secret or hard to guess, such as tokens, invitation links or password reset ids.

## Registration helpers

Namespace `CSharpEssentials.EntityFrameworkCore.Extensions`:

```csharp
services.AddCqrsDbContexts<WriteDb, ReadDb>(
    configureWrite: (sp, options) => options.UseNpgsql(connectionString),
    configureRead: (sp, options) => options.UseNpgsql(readReplicaConnectionString));

services.AddWriteDbContext<WriteDb>(
    options =>
    {
        options.EnableAuditInterceptor = true;       // attached when registered in DI
        options.EnableSlowQueryInterceptor = true;
        options.EnableDetailedErrors = true;
    },
    (sp, builder) => builder.UseNpgsql(connectionString));
```

| Method | Registers |
|---|---|
| `AddCqrsDbContexts<TWrite, TRead>(configureWrite, configureRead?)` | both as pooled contexts: the write context with `TrackAll`, the read context with `NoTracking`. Without `configureRead` the read context reuses `configureWrite`. |
| `AddWriteDbContext<T>`, `AddReadDbContext<T>` | one pooled context with `UseAsWriteContext()` or `UseAsReadContext()` |
| `AddPooledDbContext<T>` | a pooled context (`AddDbContextPool`) |
| `RegisterDbContextFactory<T>` | an `IDbContextFactory<T>` |
| `UseAsWriteContext()`, `UseAsReadContext()`, `UseAsReadContextWithIdentityResolution()` | on `DbContextOptionsBuilder`: `TrackAll`, `NoTracking`, `NoTrackingWithIdentityResolution` |

Each registration method has an overload that takes `Action<DbContextRegistrationOptions>` before the provider callback. It applies `EnableDetailedErrors`, `EnableSensitiveDataLogging`, `QueryTrackingBehavior` and the three `Enable*Interceptor` flags. The properties `MigrationsAssembly`, `RetryOptions` and `QuerySplittingBehavior` exist on the class but are not applied by this version; set them in the provider callback instead, for example `UseNpgsql(cs, npgsql => npgsql.MigrationsAssembly("MyApp.Migrations").EnableRetryOnFailure())`.

## Migrate on startup

`MigrateDatabaseAsync<TContext>()` (namespace `.Migrations`) applies pending migrations from a new service scope. It is opt-in; call it before the host starts serving:

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using CSharpEssentials.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(options => options.UseSqlite("Data Source=app.db"));
builder.Services.AddSingleton<IDatabaseMigrationLock, FileMigrationLock>();   // optional

WebApplication app = builder.Build();
await app.Services.MigrateDatabaseAsync<AppDb>(app.Lifetime.ApplicationStopping);
app.Run();

public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options);

// Serializes migrations across processes on one machine. Use a database lock across machines.
public sealed class FileMigrationLock : IDatabaseMigrationLock
{
    public async ValueTask<IAsyncDisposable> AcquireAsync(DbContext context, CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                return new FileStream("migrate.lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);
            }
            catch (IOException)
            {
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}
```

- When several instances start together, register an `IDatabaseMigrationLock` over your infrastructure (a PostgreSQL advisory lock, `sp_getapplock`, a Redis lock). The migration runs while the lock is held, and disposing the returned handle releases it. The package ships no implementation, so it takes no driver dependency.
- If `AcquireAsync` returns `null`, `MigrateDatabaseAsync` throws `InvalidOperationException` before migrating.
- EF Core 9 and later take a database lock inside `MigrateAsync` on most providers. The hook matters for EF Core 8 and for work you want serialized with the migration.
- There is no hosted-service registration for this: a hosted service would add a dependency on `Microsoft.Extensions.Hosting.Abstractions`, and the awaited call above runs before the host serves, which a hosted service does not guarantee.
- For a readiness probe that fails while migrations are pending, use [CSharpEssentials.EntityFrameworkCore.HealthChecks](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore.HealthChecks/Readme.MD).

## Diagnostics

| ID | Severity | Reported by | Meaning |
|---|---|---|---|
| CSE3001 | Info | this package | parameterless `IgnoreQueryFilters()` on EF Core 10; see [Analyzer CSE3001](#analyzer-cse3001) |
| CSE0014 | Error | `CSharpEssentials.Enums` | a generated `AlterColumn` left next to `ConvertEnumColumn` for the same column |

## Links

- Agent skill: [csharpessentials-efcore](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-efcore/SKILL.md)
- Example projects: [examples/Examples.EntityFrameworkCore](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.EntityFrameworkCore) and [examples/Examples.EntityFrameworkCore.Operations](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.EntityFrameworkCore.Operations)
- Entity base classes: [CSharpEssentials.Entity](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Entity/Readme.MD)
- Enum metadata: [CSharpEssentials.Enums](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD)
- Transaction and idempotency behaviors: [CSharpEssentials.Mediator](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Mediator/Readme.MD), [CSharpEssentials.Idempotency](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD)
