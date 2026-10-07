# CSharpEssentials.EntityFrameworkCore

Extensions, interceptors, and utilities for Entity Framework Core. Audit trails, domain events, pagination, and CQRS support.

## Features

- **Audit Interceptor**: Auto-populates `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` and handles soft deletes.
- **Domain Event Interceptor**: Dispatches domain events with before/after save timing and outbox support.
- **Slow Query Logging**: Logs queries exceeding a configurable threshold.
- **Pagination**: Offset-based and cursor-based pagination on `IQueryable`.
- **Batch Soft Delete**: Soft-delete many rows with a single `UPDATE` statement.
- **BaseDbContext**: Opt-in interceptor attachment from DI and domain event dispatch around `SaveChanges`.
- **CQRS Support**: Register separate write and read DbContexts.
- **SQL Connection Factories**: Abstractions for raw SQL/Dapper scenarios.

## Installation

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore
```

Targets `net10.0` (EF Core 10), `net9.0` (EF Core 9) and `net8.0` (EF Core 8); there is no `net11.0` target. Use the EF Core major that matches your target framework.

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
        modelBuilder.Entity<Order>().Property(o => o.Note).HasEnumCheckConstraint(false);
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
services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(
    configureWrite: (sp, options) => options.UseNpgsql(connectionString),
    configureRead: (sp, options) => options.UseNpgsql(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

### Transaction Runner

`EfCoreTransactionRunner<TDbContext>` implements `ITransactionRunner` (from `CSharpEssentials.Core`) for the Mediator `TransactionBehavior` or any code of your own.

```csharp
services.AddEfCoreTransactionRunner<AppDbContext>();

Result result = await transactionRunner.ExecuteAsync(
    ct => orderService.PlaceAsync(command, ct),
    outcome => outcome.IsSuccess,
    cancellationToken);
```

- The outermost call runs inside `Database.CreateExecutionStrategy()` and opens a transaction with `BeginTransactionAsync`, so it works with retrying strategies such as `EnableRetryOnFailure`. A retry re-runs the whole unit of work, nested calls included, on a cleared change tracker. With a retrying strategy the context must have no pending changes when the call starts (the runner throws otherwise), because the clear would drop them.
- A nested call on the same context instance (`Database.CurrentTransaction` is set) joins the outer transaction: no new strategy, begin or commit. When the provider supports savepoints, the nested work runs behind a savepoint and is rolled back to it when it fails, so its partial writes are undone and the outer call still decides the commit. Entities that nested work saved stay tracked after a rollback to the savepoint, and entities still `Added` after a failed nested `SaveChanges` are written again by the next outer `SaveChanges`; reload or detach them if the outer work carries on. Without savepoint support (for example SQL Server with MARS enabled), a failed nested call keeps its writes and the outer call decides about them.
- Only the same `DbContext` instance is joined. Another context type or connection opens its own transaction and is not atomic with this one.
- It needs a relational provider: the InMemory provider throws on `BeginTransactionAsync` unless `TransactionIgnoredWarning` is ignored. Retrying strategies also refuse to run inside an ambient `TransactionScope`, so don't combine it with `TransactionScopeBehavior`.
- If the commit itself fails with a transient error after the database applied it, the retry runs the work again. Make the work idempotent when you use a retrying strategy.

### Slow Query Logging

```csharp
services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));
```
