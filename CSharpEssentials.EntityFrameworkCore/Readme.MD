# CSharpEssentials.EntityFrameworkCore

Extensions, interceptors, and utilities for Entity Framework Core. Audit trails, domain events, pagination, and CQRS support.

## Features

- **Audit Interceptor**: Auto-populates `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` and handles soft deletes.
- **Domain Event Interceptor**: Dispatches domain events with before/after save timing and outbox support.
- **Slow Query Logging**: Logs queries exceeding a configurable threshold.
- **Pagination**: Offset-based pagination on `IQueryable` and composite keyset (cursor) pagination with opaque, versioned cursors and `Result` errors for bad cursors.
- **Batch Soft Delete**: Soft-delete many rows with a single `UPDATE` statement.
- **Named Query Filters** (EF Core 10): Register the soft-delete filter under a name and ignore only that filter, with the CSE3001 analyzer and code fix for parameterless `IgnoreQueryFilters()`.
- **BaseDbContext**: Opt-in interceptor attachment from DI and domain event dispatch around `SaveChanges`.
- **Maybe Columns**: Store `Maybe<T>?` properties in nullable columns, per property or by convention.
- **CQRS Support**: Register separate write and read DbContexts, plus pooled and factory registrations with shared options.
- **Entity Mapping**: `EntityBaseMap`, `SoftDeletableEntityBaseMap`, `OptimisticConcurrencyVersionMap` and a JSON column conversion.
- **Result Helpers**: `FirstOrDefaultAsResultAsync`, `SingleOrDefaultAsResultAsync`, `FindAsResultAsync` and `SaveChangesAsResultAsync` return `Result<T>` instead of `null` or an exception.
- **Hard Delete and Data Seeding**: `HardDelete` on a context or set, and `MigrateDataAsync` to sync seed data.
- **SQL Connection Factories**: Abstractions for raw SQL/Dapper scenarios.
- **Database Error Translation**: Turns unique, foreign key, check and not-null violations, serialization failures and deadlocks into `Error` values through SQLSTATE, with pluggable provider translators.

## Installation

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

Targets `net10.0` (EF Core 10), `net9.0` (EF Core 9) and `net8.0` (EF Core 8); there is no `net11.0` target. Use the EF Core major that matches your target framework.

## Usage

### Audit Interceptor

```csharp
using CSharpEssentials.EntityFrameworkCore.Interceptors;

services.AddAuditInterceptor(() => "system-user");

services.AddDbContext<MyDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
```

On `SaveChanges`, an `Added` entity that implements `ICreationAudit` gets `SetCreatedInfo`, a `Modified` one that implements `IModificationAudit` gets `SetUpdatedInfo`, and a `Deleted` `ISoftDeletable` that is not marked with `MarkAsHardDeleted()` is turned into a soft delete (`MarkAsDeleted`, state `Modified`). The user name comes from a scoped `IAuditUserIdProvider`, resolved on every save, and the time from `TimeProvider`. `AddAuditInterceptor` also registers `TimeProvider.System` and `AuditInterceptor` as singletons.

`AddAuditInterceptor` takes a `Func<string>`, a `Func<IServiceProvider, string>`, or the typed `Func<TUserId>` / `Func<IServiceProvider, TUserId>` forms (the user id is stored with `ToString()`). To resolve the user from the request, use the `IServiceProvider` form:

```csharp
services.AddAuditInterceptor(sp =>
    sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User.FindFirst("sub")?.Value ?? "anonymous");
```

`AddAuditUserIdProvider` (same overloads) registers only the scoped `IAuditUserIdProvider`, not the interceptor. Use it, or your own `IAuditUserIdProvider` / `IAuditUserIdProvider<TUserId>` class, when you register the interceptor yourself: `services.AddSingleton(TimeProvider.System); services.AddSingleton<AuditInterceptor>();` (`AddAuditInterceptor` registers `TimeProvider.System` unconditionally, so it replaces a `TimeProvider` registered before it; register yours after it).

### Domain Event Interceptor

```csharp
services.AddSingleton<IDomainEventPublisher, MyPublisher>();
services.AddSingleton<DomainEventInterceptor>();

services.AddDbContext<MyDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>()));

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

public sealed class MyPublisher : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        Task.CompletedTask; // hand the event to your bus or handlers
}
```

`DomainEventInterceptor` takes the events of every tracked `IDomainEventHolder` in `SavingChanges`, clears them from the entities, publishes the `BeforeSave` events through `IDomainEventPublisher` and then handles the `AfterSave` events (see the comparison table under BaseDbContext). `IDomainEventOutbox.StoreAsync` receives the `AfterSave` events instead of the publisher when an outbox is registered.

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

// Cursor-based: see Keyset Pagination below
Result<KeysetPaginationResponse<Log>> feed = await dbContext.Logs.KeysetPaginateAsync(
    new KeysetPaginationRequest { Limit = 20, After = nextCursor },
    k => k.Descending(x => x.CreatedAt).Descending(x => x.Id), ct);
```

Offset pagination caps the page size at `PaginationDefaults.MaxPageSize` (100) by default. `IPaginationRequest.Normalize()` (a default interface method, so call it through the interface type) raises the page number and page size to 1 and lowers a larger page size to 100. `Normalize(int maxPageSize)` uses your own cap instead; pass `int.MaxValue` to turn the cap off. A cap below 1 throws `ArgumentOutOfRangeException`. `PaginateAsync` and `Paginate` take the same cap through an optional `maxPageSize` parameter, the same way `KeysetPaginateAsync` clamps `Limit` to `KeysetPaginationOptions.MaxLimit`.

```csharp
IPaginationRequest request = new PaginationRequest { PageNumber = 1, PageSize = 5000 };
await dbContext.Users.OrderBy(u => u.Id).PaginateAsync(request, cancellationToken: ct);                    // 100 rows
await dbContext.Users.OrderBy(u => u.Id).PaginateAsync(request, maxPageSize: 500, cancellationToken: ct);  // 500 rows
```

`ICursorPaginationRequest<TCursor>.Normalize()` only raises `Limit` to 1; `Normalize(int maxLimit)` also lowers it to `maxLimit`. It is used only by the obsolete single-column cursor `PaginateAsync`, so it keeps its 5.x behavior; `KeysetPaginateAsync` caps `Limit` on its own.

`PaginationRequest.Search` is trimmed by `Normalize` and, when it is not empty and you pass a `search` delegate (`Func<string, Expression<Func<T, bool>>>`), `PaginateAsync` and `Paginate` apply that filter before counting. `PaginationResponse<T>` carries `Items`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage` and `HasNextPage`; with `includeTotalCount: false` no `COUNT` runs and `TotalCount` is `-1`.

### Keyset Pagination

`KeysetPaginateAsync` pages through a query by a composite key, such as "newest first, then by id". It returns opaque cursors for the next and previous page, sends one query that reads `limit + 1` rows, and never runs a `COUNT`. Unlike offset paging, its cost does not grow with the page number, and rows inserted or deleted between requests do not shift the pages.

```csharp
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;

// GET /orders?limit=20&after=eyJ2IjoxLCJkIjoiYSIs...
Result<KeysetPaginationResponse<Order>> page = await db.Orders
    .Where(o => o.CustomerId == customerId)
    .KeysetPaginateAsync(
        new KeysetPaginationRequest { Limit = 20, After = after, Before = before },
        k => k.Descending(o => o.CreatedAt).Descending(o => o.Id),
        ct);

// page.Value.Items, NextCursor, PreviousCursor, HasNext, HasPrevious
```

- **Keys.** The keys replace any `OrderBy` already on the query. Together they must be unique, so end with a unique tie-breaker such as the primary key. Directions can be mixed (`Descending(x => x.CreatedAt).Ascending(x => x.Id)`). A `KeysetOrdering<T>` is immutable; build it once and reuse it with the `KeysetOrdering<T>` overloads.
- **Request.** `After` takes `NextCursor` and returns the following page; `Before` takes `PreviousCursor` and returns the page before it. Setting both returns a `KeysetPagination.AfterAndBefore` validation error. A request with neither returns the first page.
- **Limit.** `Limit` is clamped to `1..KeysetPaginationOptions.MaxLimit` (default `100`, at most `int.MaxValue - 1`).
- **Cursor length.** A cursor longer than `KeysetPaginationOptions.MaxCursorLength` (default `2048` characters) is rejected with `InvalidCursor` before it is decoded. Raise it if a protector produces longer cursors.
- **Before pages.** For a `Before` request the ordering is reversed in SQL, `limit + 1` rows are read, and the page is reversed in memory, so `Items` is always in the configured order.
- **Has next/previous.** `HasNext` and `HasPrevious` come from the extra row, not from a count. A page reached with `After` always reports a previous page, and a page reached with `Before` always reports a next page, since the cursor came from a real neighbouring page. An empty page has no cursors.
- **Errors.** A malformed, tampered, outdated or mismatched cursor never throws. It returns `Error.Validation` with one of the `KeysetCursorErrors` codes (`InvalidCursor`, `UnsupportedCursorVersion`, `CursorDirectionMismatch`, `CursorKeyMismatch`), and the metadata key `parameter` names the bad request field (`after` or `before`). Map it to HTTP 400.

**Cursor format.** A cursor is base64url (no padding) of a small versioned JSON payload: `{"v":1,"d":"a","k":"<key fingerprint>","p":[<key values>]}`. `d` is the direction (`a` after, `b` before), `k` is a hash of the entity type and the key paths, types and directions, so a cursor issued for one ordering or entity is rejected by another, and `p` holds the typed key values. `DateTime` keeps its `Kind`, `DateTimeOffset` its offset, enums are stored as numbers, and non-finite `double`/`float` values as the strings `"NaN"`, `"Infinity"` and `"-Infinity"`. Supported key types: `int`, `long`, `short`, `byte`, `decimal`, `double`, `float`, `string`, `Guid`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `DateOnly`, `TimeOnly` and enums (except `ulong`-based ones).

**Cursor protection.** The default cursor is encoded, not encrypted or signed: a client can read the key values and craft a cursor. That is safe for the query (every value is a typed SQL parameter), but if the key values are sensitive or you want to reject client-built cursors, plug in an `ICursorProtector`. The package does not reference ASP.NET Core; a Data Protection wrapper takes a few lines:

```csharp
public sealed class DataProtectionCursorProtector(IDataProtectionProvider provider) : ICursorProtector
{
    private readonly IDataProtector _protector = provider.CreateProtector("KeysetCursor.v1");

    public string Protect(string cursor) => _protector.Protect(cursor);

    public bool TryUnprotect(string protectedCursor, out string cursor)
    {
        try { cursor = _protector.Unprotect(protectedCursor); return true; }
        catch (Exception ex) when (ex is CryptographicException or FormatException) { cursor = string.Empty; return false; }
    }
}

var options = new KeysetPaginationOptions { Protector = protector, MaxLimit = 50 };
Result<KeysetPaginationResponse<Order>> page = await db.Orders.KeysetPaginateAsync(request, ordering, options, ct);
```

`TryUnprotect` must not throw; return `false` and the request gets an `InvalidCursor` error.

**Predicate.** The default `KeysetPredicateBuilder` writes the expanded form with a redundant leading-column condition, which lets the database use an index on the first key:

```sql
-- Descending(CreatedAt).Ascending(Name).Ascending(Id), After cursor
WHERE "CreatedAt" <= @p0 AND ("CreatedAt" < @p0 OR ("CreatedAt" = @p0 AND ("Name" > @p1 OR ("Name" = @p1 AND "Id" > @p2))))
ORDER BY "CreatedAt" DESC, "Name", "Id"
LIMIT @p3
```

Cursor values always become SQL parameters, so the SQL text, and the cached query plan, are the same for every page. Strings are compared with `string.Compare`, `Guid` with `CompareTo`, and enums through their numeric value, which EF Core translates to plain `<`/`>` comparisons. The comparison therefore follows the database's own ordering (collation for strings, the stored form for `Guid`), the same ordering as the generated `ORDER BY`.

To emit a different predicate, implement `IKeysetPredicateBuilder` and set `KeysetPaginationOptions.PredicateBuilder`. Each `KeysetColumn` carries the column expression, the parameter expression for the cursor value, the CLR type and the effective direction (already reversed for `Before` pages).

**Untested sketch: PostgreSQL row values.** This is not shipped or tested by this package; verify it against your Npgsql version before use. Npgsql translates `EF.Functions.GreaterThan`/`LessThan` over value tuples to a row-value comparison, which PostgreSQL can serve from one composite index. A custom builder would have to emit an expression equivalent to this LINQ (shown for `Descending(x => x.CreatedAt).Descending(x => x.Id)` and an `After` cursor):

```csharp
x => EF.Functions.LessThan(ValueTuple.Create(x.CreatedAt, x.Id), ValueTuple.Create(cursorCreatedAt, cursorId))
// WHERE ("CreatedAt", "Id") < (@p0, @p1)
```

A row-value comparison only fits orderings whose keys all share one direction. For mixed directions, return `KeysetPredicateBuilder.Instance.BuildPredicate(columns)` from your builder.

**Limitations.**

- Nullable key columns (`int?`, `string?` with nullable annotations) are rejected with `ArgumentException` when the ordering is built, because SQL `NULL` breaks `<`/`>` comparisons. Use a non-nullable column, or a computed non-null column.
- Keys must be property or field accesses on the entity (`x => x.CreatedAt`, `x => x.Audit.CreatedAt`), not computed expressions. Every member on the path must be non-nullable: `x => x.Audit!.CreatedAt` through a nullable `Audit?` navigation is rejected with `ArgumentException`.
- The provider must translate ordering and comparison of each key type. SQLite, for example, cannot order or compare `decimal`, `DateTimeOffset` or `TimeSpan` columns, and EF Core throws `NotSupportedException` for them; SQL Server and PostgreSQL support them. Enum keys are compared through their numeric value, so they must be stored as numbers (the default). Enums stored as strings, through `ConfigureEnumConventions`, `[StringEnum]` or `HasConversion<string>()`, cannot be keys: the database would order and compare the names, not the numeric values the cursor holds.
- The ordering is by the key values alone. Rows that change their key values between requests can move between pages.
- There is no total count. Run a separate `CountAsync` if you need it.

**Moving from `PaginateAsync<T, TCursor>`.** The single-column cursor overload is `[Obsolete]` since 6.0 (warning CS0618) and will be removed in a later major version. It still works and sends the cursor as a SQL parameter, but it filters by one column only, so that column must be unique: with a non-unique column such as a timestamp, a page that ends inside a group of equal values skips the rest of that group. It also has no upper bound on `Limit`. To migrate:

```csharp
// Before
var response = await db.Logs.PaginateAsync(
    new CursorPaginationRequest<DateTime> { Limit = 20, Cursor = lastCreatedAt }, x => x.CreatedAt, isAscending: false);

// After: the tie-breaker makes the key unique, and the cursor is opaque
Result<KeysetPaginationResponse<Log>> page = await db.Logs.KeysetPaginateAsync(
    new KeysetPaginationRequest { Limit = 20, After = nextCursor },
    k => k.Descending(x => x.CreatedAt).Descending(x => x.Id), ct);
```

Cursors from `PaginateAsync` (raw values) are not accepted by `KeysetPaginateAsync`; clients restart from the first page once.

### Batch Soft Delete

```csharp
// UPDATE ... SET IsDeleted = 1, DeletedAt = @now, DeletedBy = @user WHERE ... AND NOT IsDeleted
int affected = await dbContext.Orders
    .Where(o => o.CustomerId == customerId)
    .SoftDeleteAsync(deletedBy: userId, timeProvider, ct);
```

Works for any entity implementing `ISoftDeletable` (e.g. `SoftDeletableEntityBase`). Rows already soft-deleted keep their original `DeletedAt`/`DeletedBy`. Like all `ExecuteUpdate` calls, it bypasses the change tracker and `SaveChanges` interceptors and needs a relational provider.

### Named Query Filters (EF Core 10)

EF Core 10 can name query filters, so a query can switch off one filter and keep the others. On `net10.0` the package registers the soft-delete filter (`!IsDeleted`) under `QueryFilterNames.SoftDelete`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>()
        .HasSoftDeleteQueryFilter()                                                   // one entity
        .HasQueryFilter(QueryFilterNames.Tenant, o => o.TenantId == _tenant.Id);

    // or, at the end of OnModelCreating, every root entity type that implements ISoftDeletableBase:
    // modelBuilder.ApplyNamedSoftDeleteQueryFilter();
}

// Soft-deleted orders of the current tenant: the Tenant filter still applies.
List<Order> orders = await db.Orders.IgnoreSoftDeleteQueryFilter().ToListAsync(ct);
// same as db.Orders.IgnoreQueryFilters(SoftDeleteOnly), with
// private static readonly string[] SoftDeleteOnly = [QueryFilterNames.SoftDelete];
```

- `QueryFilterNames.SoftDelete` (`"SoftDelete"`) and `QueryFilterNames.Tenant` (`"Tenant"`) are plain constants; use them for your own filters so callers can ignore them by name.
- Pass filter keys in a `static readonly` array or `new[] { ... }`. EF Core 10.0.x compiles the query again on every execution when the keys come from a collection expression (`[...]`) or a `List<string>`. `IgnoreSoftDeleteQueryFilter()` already uses a cached array.
- EF Core does not allow an anonymous filter and named filters on the same entity type. Use these helpers instead of `ApplySoftDeleteQueryFilter`/`AddQueryFilter`, not next to them; they throw `InvalidOperationException` when the entity type already has an anonymous filter. Those anonymous helpers keep working unchanged, and on `net8.0`/`net9.0` they are the only option.
- `ApplyNamedSoftDeleteQueryFilter` puts the filter on root entity types only; derived types inherit it. A soft-deletable type whose root is not soft-deletable gets no filter, so make the root implement `ISoftDeletableBase`. Owned types are skipped, because EF Core does not allow query filters on them.

**Moving from `ApplySoftDeleteQueryFilter()`.** Named keys only switch off named filters. While the model still uses the anonymous `ApplySoftDeleteQueryFilter()`, `IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete })` and `IgnoreSoftDeleteQueryFilter()` ignore nothing and soft-deleted rows stay hidden. Switch the model to `ApplyNamedSoftDeleteQueryFilter()` (and register your other filters by name, for example with `HasQueryFilter(QueryFilterNames.Tenant, ...)`) before you change the queries.

The package ships an analyzer for the parameterless `IgnoreQueryFilters()`, which turns off every filter, the tenant filter included. It runs only when the referenced EF Core has the named overload (EF Core 10 or later):

| ID | Severity | Rule |
|---|---|---|
| CSE3001 | Info | `IgnoreQueryFilters()` ignores every query filter of the entity type. Pass the names of the filters to ignore. |

CSE3001 is Info by default so that apps still on the anonymous filter are not broken. Once your filters are named, raise it in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.CSE3001.severity = warning
```

When the entity implements `ISoftDeletableBase`, the code fix "Ignore only the named 'SoftDelete' filter (requires HasSoftDeleteQueryFilter/ApplyNamedSoftDeleteQueryFilter)" rewrites the call to `IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete })`, adding the `using` when needed (the `"SoftDelete"` literal when `QueryFilterNames` is not available). It uses `new[] { ... }` because collection expressions are not allowed in expression trees and make EF Core 10.0.x recompile the query; move the array to a `static readonly` field if the call is on a hot path. Apply it only after the model registers the named filter (see above). For other entities there is no fix, because only you know which filters the query should skip. If the query really must see every row, suppress the diagnostic at that call with `[SuppressMessage("CSharpEssentials.EntityFrameworkCore", "CSE3001", Justification = "...")]`, or set `dotnet_diagnostic.CSE3001.severity = none` in `.editorconfig`.

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

### Maybe Columns

`Maybe<T>` is a struct, and EF Core cannot make a non-nullable struct property optional, so a plain `Maybe<T>` property always maps to a `NOT NULL` column. Declare the property as `Maybe<T>?` to store absence as `NULL`:

```csharp
using CSharpEssentials.Maybe;               // Maybe<T>, Flatten
using CSharpEssentials.EntityFrameworkCore; // HasNullableMaybeConversion, ConfigureNullableMaybeConventions

public sealed class Customer
{
    public int Id { get; set; }
    public Maybe<int>? LoyaltyPoints { get; set; }   // INTEGER NULL
    public Maybe<string>? Nickname { get; set; }     // TEXT NULL
}

// Every public read-write Maybe<T>? property of every entity type
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
    configurationBuilder.ConfigureNullableMaybeConventions();

// Or per property
modelBuilder.Entity<Customer>().Property(c => c.LoyaltyPoints).HasNullableMaybeConversion();
modelBuilder.Entity<Customer>().Property(c => c.Nickname).HasNullableMaybeConversion();

Maybe<int> points = customer.LoyaltyPoints.Flatten(); // null becomes None
```

- `null` and `None` are both written as `NULL`; `NULL` is read back as `null`, not `None`. Call `Flatten()` to get a `Maybe<T>`.
- The convention covers entity types, owned types, derived types (TPH) and complex types (`ComplexProperty`). It skips ignored and `[NotMapped]` properties and properties with your own `HasConversion` (yours wins). It reads the CLR properties once while the model is built; nothing runs per query.
- `Maybe<T?>?` (for example `Maybe<int?>?`) is not supported: EF Core fails the model build. Ignore the property or configure your own converter.
- `HasNullableMaybeConversion` needs a concrete `T`: from unconstrained generic code (or for `Maybe<int?>?`) the compiler reports CS0452 against the `class` overload. Use the convention there, or `HasConversion(new NullableMaybeConverter<T>())` / `new NullableMaybeReferenceConverter<T>()` (namespace `CSharpEssentials.EntityFrameworkCore.Converters`).
- Mutable reference values (`Maybe<byte[]>?`): change tracking compares `Maybe<T>` values, so changing the array in place is not saved. Assign a new value instead.
- NativeAOT: the convention closes the converter types with `MakeGenericType` while the model is built (`[RequiresDynamicCode]`). Under NativeAOT use a compiled model (`dotnet ef dbcontext optimize`), or the per-property API. This applies to `Maybe<T>?` columns only: properties configured by `ConfigureEnumConventions` cannot be compiled (see Enum Storage).
- Query rules:
  - absence: `c.Nickname == null` or `!c.Nickname.HasValue` (the `Nullable<T>.HasValue`);
  - presence: `c.Nickname != null`;
  - a value: `c.LoyaltyPoints == Maybe<int>.From(5)` (or a `Maybe<int>` variable).
  - Never compare with `None`: `c.LoyaltyPoints == Maybe<int>.None` is sent as a non-null parameter, so it matches no `NULL` row.
  - Members of `Maybe<T>` itself (`.Value.HasValue`, `.Value.Value`) do not translate to SQL; the query throws `InvalidOperationException`.
- Background: EF Core issues [#24685](https://github.com/dotnet/efcore/issues/24685), [#34943](https://github.com/dotnet/efcore/issues/34943) and [#13850](https://github.com/dotnet/efcore/issues/13850).
- Migration: changing a `Maybe<T>` property to `Maybe<T>?` changes the column from `NOT NULL` to `NULL` (an `AlterColumn`). Existing `default(T)` values written for `None` stay as they are; update them to `NULL` in the migration if they meant absence.

`MaybeConversion<T>()` (for `Maybe<T>` without `?`) is kept for compatibility but cannot store `None`: for a value type `T` it writes `default(T)` and reads back `Some(default(T))`; for a reference type `T` it writes `NULL` into a `NOT NULL` column, so the insert fails. Prefer `Maybe<T>?` with the API above.

If you would rather keep the entity free of nullable `Maybe`, map a `T?` backing field and expose `Maybe<T>` from the domain side:

```csharp
public sealed class Customer
{
    private string? _nickname;                                   // TEXT NULL, mapped by EF
    public Maybe<string> Nickname
    {
        get => Maybe<string>.From(_nickname);
        set => _nickname = value.GetValueOrDefault();
    }
}

modelBuilder.Entity<Customer>(b =>
{
    b.Ignore(c => c.Nickname);
    b.Property<string?>("_nickname").HasColumnName("Nickname");
});
// Query through the field: db.Customers.Where(c => EF.Property<string?>(c, "_nickname") == null)
```

### Enum Storage

`ConfigureEnumConventions` stores `[StringEnum]` enums by their wire name (the same spelling as JSON and binding) and adds a check constraint per column:

```csharp
using CSharpEssentials.Enums;               // EnumConventions, EnumStorage, [StringEnum]
using CSharpEssentials.EntityFrameworkCore; // ConfigureEnumConventions, HasEnumStorage, HasEnumCheckConstraint

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ConfigureEnumConventions(EnumConventions.Default);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().Property(o => o.Priority).HasEnumStorage(EnumStorage.Integer);
        modelBuilder.Entity<Order>().Property(o => o.Kind).HasEnumCheckConstraint(false);
    }
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

- Precedence: `HasEnumStorage` per property, then `[StringEnum(Storage)]`, then `EnumConventions.Storage` (`FlagsStorage` for flags).
- Reads are tolerant (wire name, member name, aliases, any case); writes are canonical. An unknown stored value maps to the `[EnumFallback]` member, otherwise the query throws and the message names the column.
- Enums without generated metadata (no `[StringEnum]`) keep EF's default integer column. `ConfigureEnumConventionsWithReflection` opts them in through reflection (not trim or AOT safe).
- Properties with your own `HasConversion` are skipped.
- Compiled models (`dotnet ef dbcontext optimize`) are not supported for properties configured by these conventions: the converters capture the model's `EnumConventions` and metadata as instances, and `dbcontext optimize` fails for them.
- Adding a member changes only the constraint, so the migration is `DropCheckConstraint` + `AddCheckConstraint`.
- Opt out of constraints per property with `HasEnumCheckConstraint(false)`, or globally with `EnumConventions.CheckConstraints = false`.
- Renaming a member also changes the constraint; see [Renaming a member safely](../CSharpEssentials.Enums/Readme.MD#renaming-a-member-safely).

#### Upgrading an existing database

Existing enum columns hold integers (EF's default) or an older text format. Keep them as they are, so the first 5.0 migration is empty:

```csharp
using CSharpEssentials.Enums;               // EnumConventions, EnumStorage
using CSharpEssentials.EntityFrameworkCore; // EnumStoredAs, HasLegacyEnumStorage

configurationBuilder.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer);

// opt in one column at a time
modelBuilder.Entity<Order>().Property(o => o.Status).HasEnumStorage(EnumStorage.String);

// or keep a column's old text format (MemberName, CamelCase, LegacySnakeCase, FlagsText)
modelBuilder.Entity<Order>().Property(o => o.Kind).HasLegacyEnumStorage(EnumStoredAs.MemberName);
```

`existingStorage` and `HasLegacyEnumStorage` keep writing the old format, read every spelling and add no check constraint. The 4.x overloads (`ConfigureEnumConventions(params Assembly[])`, `EnumConventionOptions`) are obsolete forwarders; `UseLegacySnakeCase = true` maps to `existingStorage: EnumStoredAs.LegacySnakeCase`.

#### Converting the data

When you drop `HasLegacyEnumStorage` (or `existingStorage`) for a column, `dotnet ef migrations add` generates an `AlterColumn` that changes the type without converting the data. Replace it with `ConvertEnumColumn`, and keep the constraint operations around it:

```csharp
using CSharpEssentials.Enums;               // EnumStorage
using CSharpEssentials.EntityFrameworkCore; // ConvertEnumColumn, EnumStoredAs

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

| From | To | Notes |
|---|---|---|
| `Integer` | `EnumStorage.String` or a text format (`MemberName`, `CamelCase`, `LegacySnakeCase`) | undefined numbers are kept as their text and fail the new constraint |
| `Text` (any known spelling), `MemberName`, `CamelCase`, `LegacySnakeCase` | `EnumStorage.String` or another text format | unknown text is kept and fails the new constraint |
| any text | `EnumStorage.Integer` | integer text is kept; other unknown text aborts the migration with `cannot convert orders.Status to OrderStatus, unknown value: ...` |
| `FlagsText` (`"Read, Write"`) | `EnumStorage.Integer` | tokens are OR'ed; on PostgreSQL through a temporary column |
| flags `Integer` | `EnumStoredAs.FlagsText` | single-flag member names; values with undefined bits stay numbers |

- No conversion writes `NULL`. Text conversions skip canonical values, so running them twice changes nothing.
- SQL is generated for PostgreSQL and SQLite (`MigrationBuilder.ActiveProvider`); other providers throw. On SQLite EF rebuilds the table.
- `type:` sets a non-default target store type (for example `varchar(32)`); `schema:` the table schema.
- `ConvertEnumJsonPath<TEnum>("orders", "payload", ["status"])` rewrites one path of a PostgreSQL `jsonb` column (`to: EnumStorage.Integer` for `Down`); unknown values are left untouched.
- `EnumDataAudit.Sql<TEnum>("orders", "Status", storedAs: ..., provider: ...)` returns a read-only query that lists the values a conversion or constraint would reject, with their counts. Run it in production before you deploy the migration (see below).
- CSE0014 (build error) reports a generated `AlterColumn` that you forgot to delete next to `ConvertEnumColumn` for the same column.

#### Auditing the data

`EnumDataAudit.Sql` only builds the SQL (`SELECT "Status" AS "Value", count(*) AS "Count" ... GROUP BY ... ORDER BY ...`); it changes nothing. Print it and run it with psql or your SQL client, or run it through EF Core:

```csharp
using CSharpEssentials.EntityFrameworkCore; // EnumDataAudit, EnumStoredAs

string sql = EnumDataAudit.Sql<OrderStatus>("orders", "Status", storedAs: EnumStoredAs.Integer, provider: db.Database.ProviderName);

List<EnumAuditRow> rejected = await db.Database.SqlQueryRaw<EnumAuditRow>(sql).ToListAsync(ct);
foreach (EnumAuditRow row in rejected)
    logger.LogWarning("orders.Status holds {Value} in {Count} rows", row.Value, row.Count);

// Value has the column's type: int for EnumStoredAs.Integer, string for text columns
public sealed class EnumAuditRow
{
    public int Value { get; init; }
    public long Count { get; init; }
}
```

An empty result means every row converts and passes the new constraint. Pass `schema:` for a table outside the default schema.

### CQRS Write/Read

```csharp
using CSharpEssentials.EntityFrameworkCore.Extensions;

services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(
    configureWrite: (sp, options) => options.UseNpgsql(connectionString),
    configureRead: (sp, options) => options.UseNpgsql(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

`AddCqrsDbContexts` registers both contexts as pooled contexts (`AddDbContextPool`): the write context with `UseAsWriteContext()` (`QueryTrackingBehavior.TrackAll`) and the read context with `UseAsReadContext()` (`NoTracking`). Without `configureRead` the read context reuses `configureWrite`. The same pieces are available on their own: `AddWriteDbContext<T>`, `AddReadDbContext<T>`, `AddPooledDbContext<T>` and `RegisterDbContextFactory<T>` (an `IDbContextFactory<T>`), and `UseAsWriteContext`, `UseAsReadContext` and `UseAsReadContextWithIdentityResolution` on `DbContextOptionsBuilder`. Each registration method also has an overload that takes `Action<DbContextRegistrationOptions>` before the provider callback:

```csharp
services.AddWriteDbContext<WriteDbContext>(
    options =>
    {
        options.EnableAuditInterceptor = true;
        options.EnableSlowQueryInterceptor = true;
        options.EnableDetailedErrors = true;
    },
    (sp, builder) => builder.UseNpgsql(connectionString));
```

`DbContextRegistrationOptions` applies `EnableDetailedErrors`, `EnableSensitiveDataLogging`, `QueryTrackingBehavior`, and `EnableAuditInterceptor`, `EnableDomainEventInterceptor` and `EnableSlowQueryInterceptor`, which add the interceptor only when it is registered in DI. The properties `MigrationsAssembly`, `RetryOptions` and `QuerySplittingBehavior` exist on the class but are not applied by this version; set them in the provider callback instead (`UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly("MyApp.Migrations").EnableRetryOnFailure().UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))`).

### Entity Mapping

`EntityBaseExtensions` configures the audit columns of the `CSharpEssentials.Entity` base classes in `IEntityTypeConfiguration<T>` or `OnModelCreating`:

```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>   // Order : SoftDeletableEntityBase<Guid>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.SoftDeletableEntityBaseGuidIdMap();   // key Id, audit columns, IsDeleted/DeletedAt/DeletedBy; IsHardDeleted is ignored
        builder.OptimisticConcurrencyVersionMap();    // shadow byte[] "RowVersion" marked as a row version
    }
}

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>   // Invoice : EntityBase<long>
{
    public void Configure(EntityTypeBuilder<Invoice> builder) =>
        builder.EntityBaseMap<Invoice, long>(userIdMaxLength: 64);   // any key type
}
```

`EntityBaseGuidIdMap()` does the same for an `EntityBase<Guid>` entity without the soft delete columns. The audit columns are `CreatedAt`/`CreatedBy` (required) and `UpdatedAt`/`UpdatedBy` (optional), with the user columns limited to 40 characters unless you pass `userIdMaxLength`. `EntityBaseMap<TEntity>()` and `SoftDeletableEntityBaseMap<TEntity>()` map the audit columns without a key. `ApplySoftDeleteQueryFilter()` adds `!IsDeleted` as an anonymous filter to every root entity type that implements `ISoftDeletableBase`, and `AddQueryFilter<T>(expression)` combines your filter with the existing anonymous one with `AND` (on net10.0 use the named helpers below when you need to ignore filters by name). `HasJsonConversion(columnType: "jsonb")` stores a property as JSON with the CSharpEssentials JSON options.

### Result Helpers, Hard Delete and Seeding

```csharp
Result<Order> order = await db.Orders.Where(o => o.Id == id).FirstOrDefaultAsResultAsync(Error.NotFound("Order.NotFound", "Order not found"), ct);
Result<Order> single = await db.Orders.SingleOrDefaultAsResultAsync(cancellationToken: ct);   // Error.NotFound() when no row
Result<Order> found = await db.Orders.FindAsResultAsync([id], cancellationToken: ct);
Result saved = await db.SaveChangesAsResultAsync(ct);                                        // every exception becomes an Unknown error

db.HardDelete(order.Value);                       // MarkAsHardDeleted + Remove: the audit interceptor keeps the delete
db.HardDelete(await db.Orders.Where(o => o.IsDeleted).ToListAsync(ct));
await db.SaveChangesAsync(ct);
```

`DbSet<T>.Delete(entities)` is a hard delete as well, despite its name: it calls `MarkAsHardDeleted()` on every item and removes them. A `null` row gives the `notFoundError` you pass, or `Error.NotFound()`. `SaveChangesAsResultAsync` lets an `OperationCanceledException` from your token propagate; use `DbErrorTranslation` (below) when you want database errors translated into specific `Error` values. `AsNoTracking(bool isDisabled)` on `IQueryable<T>` applies `AsNoTracking()` unless `isDisabled` is `true`.

`MigrateDataAsync` adds seed rows when a precondition says they are missing, or syncs a table with a key-based diff (add, update, and remove rows that are not in the seed data):

```csharp
await db.MigrateDataAsync<Country, CountrySeed>(
    seed,
    (set, data) => set.Any(),                   // true: the table is already seeded, do nothing
    s => new Country { Code = s.Code },
    ct);

await db.MigrateDataAsync(seed, new MigrateDataOptions<Country, CountrySeed, string>
{
    Query = set => set,
    EntityKeyProperty = c => c.Code,
    DataKeyProperty = s => s.Code,
    IsUpdatedFunc = (c, s) => c.Name != s.Name,
    UpdateFunc = (c, s) => { c.Name = s.Name; return c; },
    Converter = s => new Country { Code = s.Code, Name = s.Name },
    HardDeleteMode = false,                      // false: a soft-deletable row missing from the seed goes through the normal delete (soft delete with the audit interceptor); true: it is hard deleted
}, ct);
```

### SQL Connection Factories

`ISqlConnectionFactory` (`IDbConnection Create()`) and `ISqlReadOnlyConnectionFactory` are interfaces only. Implement and register them yourself for Dapper or raw ADO.NET code that should not go through the `DbContext`.

### Transaction Runner

`EfCoreTransactionRunner<TDbContext>` implements `ITransactionRunner` (from `CSharpEssentials.Core`) for the Mediator `TransactionBehavior` or any code of your own.

```csharp
services.AddEfCoreTransactionRunner<AppDbContext>();

// namespaces: CSharpEssentials.Transactions (ITransactionRunner), CSharpEssentials.EntityFrameworkCore.Transactions (AddEfCoreTransactionRunner)
// orderService.PlaceAsync returns ValueTask<Result>
Result result = await transactionRunner.ExecuteAsync(
    ct => orderService.PlaceAsync(command, ct),
    outcome => outcome.IsSuccess,
    cancellationToken);
```

The work delegate returns a `ValueTask<T>` and `shouldCommit` decides whether the transaction commits; a work that throws rolls back.

- The outermost call runs inside `Database.CreateExecutionStrategy()` and opens a transaction with `BeginTransactionAsync`, so it works with retrying strategies such as `EnableRetryOnFailure`. A retry re-runs the whole unit of work, nested calls included, on a cleared change tracker. With a retrying strategy the context must have no pending changes when the call starts (the runner throws otherwise), because the clear would drop them.
- A nested call on the same context instance (`Database.CurrentTransaction` is set) joins the outer transaction: no new strategy, begin or commit. When the provider supports savepoints, the nested work runs behind a savepoint and is rolled back to it when it fails, so its partial writes are undone and the outer call still decides the commit. Entities that nested work saved stay tracked after a rollback to the savepoint, and entities still `Added` after a failed nested `SaveChanges` are written again by the next outer `SaveChanges`; reload or detach them if the outer work carries on. Without savepoint support (for example SQL Server with MARS enabled), a failed nested call keeps its writes and the outer call decides about them.
- Only the same `DbContext` instance is joined. Another context type or connection opens its own transaction and is not atomic with this one.
- It needs a relational provider: the InMemory provider throws on `BeginTransactionAsync` unless `TransactionIgnoredWarning` is ignored. Retrying strategies also refuse to run inside an ambient `TransactionScope`, so don't combine it with `TransactionScopeBehavior`.
- If the commit itself fails with a transient error after the database applied it, the retry runs the work again. Make the work idempotent when you use a retrying strategy.

### Slow Query Logging

```csharp
services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));            // or: options => options.Threshold = ...; the default is 1 second
services.AddSingleton<ISlowQueryHandler, MetricsSlowQueryHandler>();        // optional

services.AddDbContext<MyDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<SlowQueryInterceptor>()));

public sealed class MetricsSlowQueryHandler : ISlowQueryHandler
{
    public void OnSlowQuery(SlowQueryContext context) =>
        Console.WriteLine($"{context.ElapsedTime}: {context.CommandText}"); // also Parameters, Database, HasTransaction, MethodName
}
```

Every command (reader, scalar or non-query) that runs for at least the threshold is logged as a warning with its command text and parameter values, and passed to the `ISlowQueryHandler` when one is registered. Parameter values can be sensitive; keep that in mind before shipping the logs.

### Database Error Translation

`DbErrorTranslation` (namespace `CSharpEssentials.EntityFrameworkCore.DbErrors`) turns persistence exceptions into `Error` values, which the AspNetCore package renders as ProblemDetails. The built-in `SqlStateErrorTranslator` reads `DbException.SqlState` from the exception or any inner exception, so it needs no provider reference:

| SQLSTATE | Meaning | Error | Code |
|----------|---------|-------|------|
| `23505` | unique violation | `Conflict` (409) | `Database.UniqueViolation` |
| `23503` | foreign key violation | `Conflict` (409) | `Database.ForeignKeyViolation` |
| `23514` | check violation | `Validation` (400) | `Database.CheckViolation` |
| `23502` | not null violation | `Validation` (400) | `Database.NotNullViolation` |
| `40001` | serialization failure | `Conflict` (409), `retryable: true` | `Database.SerializationFailure` |
| `40P01` | deadlock | `Conflict` (409), `retryable: true` | `Database.Deadlock` |

The metadata carries `sqlState`, and `entities` (the entity type names of `DbUpdateException.Entries`) when there are any. The database message is not copied, because it can contain row values.

```csharp
services.AddDbErrorTranslation();

public sealed class RegisterUserHandler(AppDbContext db, DbErrorTranslation translation)
{
    public async Task<Result> Handle(RegisterUser command, CancellationToken ct)
    {
        db.Users.Add(new User(command.Email));
        Result<int> saved = await translation.SaveChangesAsync(db, ct); // Conflict on a duplicate email
        return saved.IsSuccess ? Result.Success() : saved.Errors;
    }
}
```

`translation.SaveChangesAsync` returns the translated error and rethrows exceptions no translator recognizes, so unexpected failures still reach your exception handler. `SaveChangesAsResultAsync` is different: it turns every exception into an `Unknown` error, except an `OperationCanceledException` caused by the `cancellationToken` you passed, which propagates. After a failure the change tracker keeps the unsaved changes. To translate an exception you caught yourself, call `translation.TryTranslate(exception, out Error error)`.

Providers that report no SQLSTATE (SQL Server, SQLite) or extra details (the constraint name of `PostgresException`) plug in an `IDbErrorTranslator`. Custom translators run in registration order before the SQLSTATE default, and the first match wins. `DbErrorTranslation` is a singleton, so translators must be stateless:

```csharp
using Npgsql; // PostgresException

services.AddDbErrorTranslator<PostgresConstraintTranslator>();

public sealed class PostgresConstraintTranslator : IDbErrorTranslator
{
    private readonly SqlStateErrorTranslator _sqlState = new();

    public bool TryTranslate(Exception exception, out Error error)
    {
        if (!_sqlState.TryTranslate(exception, out error))
            return false;

        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres)
                error.Metadata!.AddMetadata("constraint", postgres.ConstraintName);
        }

        return true;
    }
}
```

The default ProblemDetails fields (`Codes | ValidationErrors`) do not include error metadata. With `ProblemErrorFields.AllErrors`, the client also sees `sqlState`, `entities` and anything your translators add, such as constraint names.
