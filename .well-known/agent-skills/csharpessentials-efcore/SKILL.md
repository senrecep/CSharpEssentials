---
name: csharpessentials-efcore
description: Use when wiring EF Core with CSharpEssentials domain models. Covers AuditInterceptor/DomainEventInterceptor/SlowQueryInterceptor registered via DI, BaseDbContext (InterceptorsFromServices, DispatchDomainEventsOnSaveChanges), PaginateAsync/Paginate, KeysetPaginateAsync (composite keyset pagination with opaque cursors), batch SoftDeleteAsync, Result queries (FirstOrDefaultAsResultAsync, SaveChangesAsResultAsync), ConfigureEnumConventions for [StringEnum] storage, Maybe<T>? nullable columns (HasNullableMaybeConversion, ConfigureNullableMaybeConventions) and AddCqrsDbContexts.
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
using CSharpEssentials.EntityFrameworkCore.Pagination;            // PaginateAsync, Paginate, KeysetPaginateAsync
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;   // PaginationRequest, KeysetPaginationRequest
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;  // PaginationResponse<T>, KeysetPaginationResponse<T>
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;     // KeysetOrdering<T>, KeysetPaginationOptions, ICursorProtector, KeysetCursorErrors
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

## Enum Storage (5.0)

`ConfigureEnumConventions` stores `[StringEnum]` enums by wire name (same spelling as JSON) and adds a check constraint `ck_{table}_{column}_enum` per column. Enums without `[StringEnum]` keep EF's integer default.

```csharp
public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.ConfigureEnumConventions(EnumConventions.Default);
        // Existing database with integer enum columns: keep them, first migration stays empty
        // configurationBuilder.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer);
}

modelBuilder.Entity<Order>().Property(o => o.Status).HasEnumStorage(EnumStorage.String);              // per property
modelBuilder.Entity<Order>().Property(o => o.Kind).HasLegacyEnumStorage(EnumStoredAs.MemberName);     // keep old text format
modelBuilder.Entity<Order>().Property(o => o.Priority).HasEnumCheckConstraint(false);                 // no constraint
```

- Properties with a user `HasConversion` are skipped.
- Reads are tolerant; unknown values map to `[EnumFallback]` or throw naming the column.
- `ToJson()` columns use `EnumJsonValueReaderWriter<TEnum>`.
- The 4.x `params Assembly[]` overloads and `EnumConventionOptions` are obsolete forwarders.

---

## Maybe Columns

EF Core cannot make a non-nullable struct property optional, so `Maybe<T>` always maps to `NOT NULL`. Declare `Maybe<T>?` to store absence as `NULL`:

```csharp
public sealed class Customer
{
    public int Id { get; set; }
    public Maybe<int>? LoyaltyPoints { get; set; }   // INTEGER NULL
    public Maybe<string>? Nickname { get; set; }     // TEXT NULL
}

configurationBuilder.ConfigureNullableMaybeConventions();                                   // all Maybe<T>? properties
modelBuilder.Entity<Customer>().Property(c => c.LoyaltyPoints).HasNullableMaybeConversion(); // or per property

Maybe<int> points = customer.LoyaltyPoints.Flatten();   // null → None (CSharpEssentials.Maybe)
```

- `null` and `None` are both written as `NULL`; `NULL` reads back as `null`. Use `Flatten()` on the domain side.
- Query absence with `== null` or `!x.HasValue`, presence with `!= null`, a value with `== Maybe<int>.From(5)`. Never `== Maybe<T>.None`: it matches no `NULL` row. Members of `Maybe<T>` (`.Value.HasValue`, `.Value.Value`) do not translate. Background: EF Core issues #24685, #34943, #13850.
- The convention covers entity, owned, derived (TPH) and complex types; it skips ignored/`[NotMapped]` properties and user `HasConversion` (yours wins). It reads CLR properties once at model build with `MakeGenericType` (`[RequiresDynamicCode]`): use a compiled model under NativeAOT (this works for `Maybe<T>?` columns only; `ConfigureEnumConventions` properties cannot be compiled).
- `Maybe<T?>?` (`Maybe<int?>?`) is unsupported: EF fails the model build; ignore it or give it your own converter. `HasNullableMaybeConversion` needs a concrete `T` (CS0452 from unconstrained generic code); use the convention or `HasConversion(new NullableMaybeConverter<T>())` there.
- Mutable reference values (`Maybe<byte[]>?`): in-place changes are not detected; assign a new value.
- `MaybeConversion<T>()` on plain `Maybe<T>` is lossy: `None` of a value type is stored as `default(T)` and read as `Some(default(T))`; `None` of a reference type fails on insert (NOT NULL). Switching to `Maybe<T>?` alters the column to `NULL` in the next migration; old `default(T)` rows are not converted.
- Alternative: map a private `T?` field (`b.Property<string?>("_nickname")`, `b.Ignore(c => c.Nickname)`) and expose `Maybe<T>` from the domain property.

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
```

The single-column cursor `PaginateAsync(cursorRequest, cursorSelector, ...)` is `[Obsolete]` since 6.0; use `KeysetPaginateAsync` below with a unique key (add the id as a tie-breaker).

`Paginate` is the synchronous variant and also works on in-memory `IQueryable<T>`. Offset pagination caps `PageSize` at `PaginationDefaults.MaxPageSize` (100) by default: `IPaginationRequest.Normalize()` lowers larger values to 100, and `PaginateAsync`/`Paginate` take an optional `maxPageSize` (pass `int.MaxValue` to turn the cap off; cap < 1 throws `ArgumentOutOfRangeException`).

### Keyset (composite cursor) pagination

Prefer this for feeds and APIs. One query reads `limit + 1` rows, no COUNT, opaque cursors for both directions:

```csharp
Result<KeysetPaginationResponse<Order>> page = await db.Orders
    .Where(o => o.CustomerId == customerId)
    .KeysetPaginateAsync(
        new KeysetPaginationRequest { Limit = 20, After = after, Before = before }, // never both
        k => k.Descending(o => o.CreatedAt).Descending(o => o.Id),                // end with a unique key
        ct);
// page.Value.Items, NextCursor (send as After), PreviousCursor (send as Before), HasNext, HasPrevious

// Options: max limit (default 100), max cursor length (default 2048), cursor protection, custom predicate
var options = new KeysetPaginationOptions { MaxLimit = 50, Protector = myDataProtectionWrapper };
```

- Keys replace any existing `OrderBy`; directions may be mixed; build a `KeysetOrdering<T>` once and reuse it.
- Bad cursors (garbage, too long, tampered, other ordering or entity, wrong direction, old version) and `After` + `Before` return `Error.Validation` with a `KeysetCursorErrors.*Code`; they never throw. Map to HTTP 400.
- Nullable keys (`int?`, `string?`), keys through a nullable navigation (`x => x.Audit!.CreatedAt`), computed keys and unsupported types throw `ArgumentException` when the ordering is built.
- Cursors are base64url JSON, readable by clients. Implement `ICursorProtector` (wrap `IDataProtector`, return `false` instead of throwing) to sign or encrypt them.
- The provider must order and compare the key type: SQLite cannot for `decimal`, `DateTimeOffset`, `TimeSpan`. Enum keys must be stored as numbers: enums stored as strings via `ConfigureEnumConventions`/`[StringEnum]` (or `HasConversion<string>()`) cannot be keys.
- Replace `IKeysetPredicateBuilder` (e.g. row-value comparisons on PostgreSQL) through `KeysetPaginationOptions.PredicateBuilder`.

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

On `net10.0` (EF Core 10) use named filters instead, so a query can drop soft delete and keep the others (never mix them with the anonymous `ApplySoftDeleteQueryFilter`; EF Core rejects both on one entity):

```csharp
modelBuilder.Entity<Order>()
    .HasSoftDeleteQueryFilter()                                            // named QueryFilterNames.SoftDelete
    .HasQueryFilter(QueryFilterNames.Tenant, o => o.TenantId == tenantId);
// or modelBuilder.ApplyNamedSoftDeleteQueryFilter() at the end of OnModelCreating

List<Order> withDeleted = await db.Orders.IgnoreSoftDeleteQueryFilter().ToListAsync(ct); // Tenant filter still applies
```

Migrating from the anonymous `ApplySoftDeleteQueryFilter()`: named keys do not switch off an anonymous filter, so `IgnoreSoftDeleteQueryFilter()` ignores nothing until the model uses `ApplyNamedSoftDeleteQueryFilter()`/`HasSoftDeleteQueryFilter()`. Change the model first, then the queries. The helpers throw when the entity already has an anonymous filter, skip owned types, and only filter root types (a soft-deletable type under a non-soft-deletable root gets no filter). Pass filter keys in a `static readonly string[]` or `new[] { ... }`, never a collection expression or `List`: EF Core 10.0.x recompiles the query for those.

Analyzer CSE3001 (Info by default; raise with `dotnet_diagnostic.CSE3001.severity = warning` in `.editorconfig`) flags parameterless `IgnoreQueryFilters()` when EF Core 10 is referenced; the code fix rewrites it to `IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete })` for `ISoftDeletableBase` entities and requires the named filter in the model.

---

## Entity Mapping and Result Helpers

```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>   // Order : SoftDeletableEntityBase<Guid>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.SoftDeletableEntityBaseGuidIdMap();   // key, audit columns (user columns max 40), IsDeleted/DeletedAt/DeletedBy; IsHardDeleted is ignored
        builder.OptimisticConcurrencyVersionMap();    // shadow byte[] "RowVersion" row version
        builder.Property(o => o.Metadata).HasJsonConversion(columnType: "jsonb");
    }
}
// EntityBaseGuidIdMap() for an EntityBase<Guid>; EntityBaseMap<TEntity, TId>(userIdMaxLength: 64) for any key type

Result<Order> one = await db.Orders.SingleOrDefaultAsResultAsync(cancellationToken: ct);   // Error.NotFound() when no row
db.HardDelete(entity);                        // MarkAsHardDeleted + Remove (the AuditInterceptor keeps the delete)
await db.MigrateDataAsync<Country, CountrySeed>(seed, (set, data) => set.Any(), s => new Country { Code = s.Code }, ct);
```

`ApplySoftDeleteQueryFilter()` adds an anonymous `!IsDeleted` filter to every root `ISoftDeletableBase` type; `AddQueryFilter<T>(expression)` ANDs your filter with the existing anonymous one. `MigrateDataAsync` also has an overload with `MigrateDataOptions<TEntity, TSeed, TKey>` for a key-based add/update/remove diff. `ISqlConnectionFactory` and `ISqlReadOnlyConnectionFactory` are interfaces for Dapper-style code; you implement and register them.

---

## Database Error Translation

`DbErrorTranslation` turns persistence exceptions into `Error` values (the AspNetCore package renders them as ProblemDetails). The built-in `SqlStateErrorTranslator` reads `DbException.SqlState`:

| SQLSTATE | Error | Code |
|---|---|---|
| `23505` | `Conflict` | `Database.UniqueViolation` |
| `23503` | `Conflict` | `Database.ForeignKeyViolation` |
| `23514` | `Validation` | `Database.CheckViolation` |
| `23502` | `Validation` | `Database.NotNullViolation` |
| `40001` | `Conflict`, `retryable: true` | `Database.SerializationFailure` |
| `40P01` | `Conflict`, `retryable: true` | `Database.Deadlock` |

```csharp
using CSharpEssentials.EntityFrameworkCore.DbErrors;

builder.Services.AddDbErrorTranslation();
builder.Services.AddDbErrorTranslator<MyProviderTranslator>();   // optional IDbErrorTranslator, runs before the SQLSTATE default

Result<int> saved = await translation.SaveChangesAsync(db, ct);   // translated error, or rethrows an unrecognized exception
bool known = translation.TryTranslate(exception, out Error error);
```

The metadata carries `sqlState` and, when there are entries, `entities`; the database message is not copied. Translators must be stateless (`DbErrorTranslation` is a singleton). `SaveChangesAsResultAsync` is different: it turns every exception into an `Unknown` error, except cancellation of your token.

---

## Transaction Runner

```csharp
using CSharpEssentials.EntityFrameworkCore.Transactions;   // AddEfCoreTransactionRunner
using CSharpEssentials.Transactions;                       // ITransactionRunner (CSharpEssentials.Core)

builder.Services.AddEfCoreTransactionRunner<AppDbContext>();   // scoped ITransactionRunner

Result result = await runner.ExecuteAsync(
    ct => orders.PlaceAsync(command, ct),     // returns ValueTask<Result>
    outcome => outcome.IsSuccess,             // commit or roll back
    cancellationToken);
```

The outermost call runs inside `CreateExecutionStrategy()` with a transaction, so retrying strategies work (a retry re-runs the whole unit on a cleared change tracker; the context must have no pending changes at the start). A nested call on the same context instance joins the transaction, behind a savepoint when the provider supports them. It needs a relational provider (the InMemory provider throws on `BeginTransactionAsync`) and does not combine with an ambient `TransactionScope`.

---

## Enum Column Migrations

```csharp
migrationBuilder.ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);
migrationBuilder.ConvertEnumJsonPath<OrderStatus>("orders", "payload", ["status"]);   // PostgreSQL jsonb
string sql = EnumDataAudit.Sql<OrderStatus>("orders", "Status", storedAs: EnumStoredAs.Integer, provider: db.Database.ProviderName);
```

When you drop `HasLegacyEnumStorage`/`existingStorage` for a column, replace the generated `AlterColumn` with `ConvertEnumColumn` (analyzer CSE0014 reports a forgotten one) and keep the check-constraint operations around it. `EnumDataAudit.Sql` returns a read-only query that lists the stored values the conversion or constraint would reject, with their counts; run it before deploying. SQL is generated for PostgreSQL and SQLite only.

---

## CQRS Registration

```csharp
builder.Services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(
    configureWrite: (sp, options) => options.UseSqlite("Data Source=app.db"));
```

The write context is pooled with change tracking; the read context is pooled with `NoTracking`. `AddWriteDbContext<T>` and `AddReadDbContext<T>` register them one at a time.  `AddPooledDbContext<T>` and `RegisterDbContextFactory<T>` register a pooled context and an `IDbContextFactory<T>`. Every registration method has an overload that takes `Action<DbContextRegistrationOptions>` first (`EnableDetailedErrors`, `EnableSensitiveDataLogging`, `QueryTrackingBehavior`, and `EnableAuditInterceptor`/`EnableDomainEventInterceptor`/`EnableSlowQueryInterceptor`, which attach the interceptor only when it is registered in DI). `MigrationsAssembly`, `RetryOptions` and `QuerySplittingBehavior` on that class are not applied by this version; set them in the provider callback. `UseAsWriteContext()`, `UseAsReadContext()` and `UseAsReadContextWithIdentityResolution()` are available on `DbContextOptionsBuilder`.

---

## Best Practices

- Build the unit of work around `ITransactionRunner` rather than your own `BeginTransaction` when retries are enabled
- Pick one domain event path: `DomainEventInterceptor` or `BaseDbContext.DispatchDomainEventsOnSaveChanges`
- `SoftDeleteAsync` skips audit and domain events; use `MarkAsDeleted` + `SaveChanges` when those must run
- `PaginateAsync` issues a COUNT and a data query; pass `includeTotalCount: false` to skip the COUNT
- Upgrading to 6.0: offset `PageSize` is capped at 100; pass `maxPageSize:` to `PaginateAsync`/`Paginate` when an endpoint needs larger pages
- Upgrading to 5.0: pass `existingStorage: EnumStoredAs.Integer` (or the column's old format) to `ConfigureEnumConventions`, otherwise `[StringEnum]` integer columns become text in the next migration
