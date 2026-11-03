# CSharpEssentials.EntityFrameworkCore: Performance benchmarks (draft)

This page compares CSharpEssentials.EntityFrameworkCore (CSE) with plain EF Core, MR.EntityFrameworkCore.KeysetPagination 1.6.0, X.PagedList.EF 10.6.0 and EntityFrameworkCore.Exceptions.Sqlite 10.0.1. It covers SaveChanges with the audit and domain event paths, keyset and offset paging, the enum column converters, `MigrateDataAsync`, context creation, and database error translation. Every database row runs on in-memory SQLite. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

The same project, `benchmarks/CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks/` (suite key `efcore`), also holds the `EntityBase` domain event class, documented in [CSharpEssentials.Entity/BENCHMARKS.md](../CSharpEssentials.Entity/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

The competitors are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License (nuspec expression) | Asset the suite loads |
|---|---|---|---|---|
| MR.EntityFrameworkCore.KeysetPagination | 1.6.0 | 2026-04-06 | MIT | `lib/net10.0` (the only asset) on net10.0 and net11.0 |
| X.PagedList.EF (with X.PagedList 10.6.0) | 10.6.0 | 2026-09-06 | MIT | `lib/net8.0` on net9.0, `lib/net10.0` on net10.0 and net11.0 |
| EntityFrameworkCore.Exceptions.Sqlite (with .Common 10.0.1, DbExceptionClassifier.Sqlite 1.0.1) | 10.0.1 | 2026-05-24 | Apache-2.0 | `lib/net10.0` (the only asset) on net10.0 and net11.0 |
| Microsoft.EntityFrameworkCore and .Sqlite | 8.0.33, 9.0.22, 10.0.14 | 2026-10-25 | MIT | one major per TFM, see below |

`X.PagedList.EntityFramework` is not in the table: its 10.6.0 release depends on classic EF6 (`EntityFramework [6.5.2,7)`) and cannot page an EF Core query. `X.PagedList.EF` is the same author's EF Core package. Its `lib/net8.0` asset needs EF Core 9.0.18 or later, so restore fails with NU1605 against CSE's net8.0 EF Core 8 build; X.PagedList has no net8.0 row. MR and EntityFrameworkCore.Exceptions ship a `net10.0` asset only, so their rows run on net10.0 and net11.0. Transitive pinning is off for this project, as it is for the DI, mediator, endpoints, Http and AspNetCore suites: the central `[9.0.4,10)` EF Core pin would fight the EF Core 8 and 10 rows.

What each TFM runs (`--self-check` prints this as its first line):

| TFM | EF Core and Sqlite provider | CSE.EntityFrameworkCore build | Competitor rows |
|---|---|---|---|
| net8.0 | 8.0.33 | net8.0 | none (CSE and hand-written rows only) |
| net9.0 | 9.0.22 | net9.0 | X.PagedList.EF |
| net10.0 | 10.0.14 | net10.0 | X.PagedList.EF, MR, EntityFrameworkCore.Exceptions |
| net11.0 | 10.0.14 | net10.0 (the package has no net11.0 target; EF Core 11 is still a release candidate) | X.PagedList.EF, MR, EntityFrameworkCore.Exceptions |

On net10.0 and net11.0 EntityFrameworkCore.Exceptions brings `Microsoft.Data.Sqlite` 10.0.5 next to the provider's `Microsoft.Data.Sqlite.Core` 10.0.14.

---

## What was measured

The code is in `benchmarks/CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks/`, in nine classes for this package.
- Every class has `[MemoryDiagnoser]`.
- Each class opens one `SqliteConnection("DataSource=:memory:")` in `[GlobalSetup]` and keeps it open until `[GlobalCleanup]`. Every context of the class uses it.
- Every row that writes runs in a transaction that is rolled back, then clears the change tracker, so the tables do not grow between invocations. Every row of a class pays the same transaction.
- Entities and events are created inside the measured method, the same way for every row.
- Logging is registered with no provider, so CSE's Information logs are disabled. That is the floor cost of the logging calls.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check for every parameter value. A mismatch fails the run.
- Each competitor, EF Core and hand-written row has a comment above it that links to the documentation it follows; each CSE row links to its Readme section, and the two direct enum converter rows link to their source files.

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `SaveChangesAuditComparison` | SaveChanges of `Count` = 1, 100, 1000 orders, `State` = Added or Modified | plain EF Core (baseline, reference: sets no audit fields), hand-written `SaveChangesAsync` override, CSE `AuditInterceptor` on the options, CSE `BaseDbContext` with `InterceptorsFromServices = Audit` | every audited row writes `Count` rows and stamps the same user on every entity |
| 2 | `SaveChangesDomainEventsComparison` | SaveChanges of `Count` = 1, 100, 1000 new orders raising `EventsPerEntity` = 0 or 1 events | plain EF Core (baseline, reference: dispatches nothing), hand-written collect-save-publish override, CSE `BaseDbContext` with `DispatchDomainEventsOnSaveChanges`, CSE `DomainEventInterceptor` (labelled) | `Count` rows, `Count * EventsPerEntity` published events, no event left on any entity |
| 3 | `KeysetPaginationComparison` | one 20-row page over 10,000 rows, `Page` = First or After (cursor at ordinal 5,000) | hand-written Where/OrderBy/Take(21) with a base64url JSON cursor (baseline), CSE `KeysetPaginateAsync` with a cached `KeysetOrdering`, CSE with the inline `Func` ordering, MR prebuilt + Take(21), MR inline + Take(21), MR prebuilt + documented `HasNextAsync` (labelled) | same 20 ids, has-next and has-previous; each row's next cursor leads to the same second page |
| 4 | `OffsetPaginationComparison` | one 20-row page with a total count, `PageNumber` = 1 or 250 | EF Core `CountAsync` + Skip/Take (baseline), CSE `PaginateAsync`, X.PagedList `ToPagedListAsync` | same ids, total count 10,000 and page count |
| 5 | `EnumConverterComparison` | write and read all five members of a `[StringEnum]` enum through the typed converter delegates, no database | Write and Read: EF Core `EnumToStringConverter` (baseline, labelled), CSE `ConfigureEnumConventions` (the wire name converter it installs), CSE `HasLegacyEnumStorage(LegacySnakeCase)` (labelled), and two direct converter classes, not the Readme path: `EnumToFormattedStringConverter` and `LegacySnakeCaseEnumConverter` (labelled). ReadLegacy (CSE only): the `ConfigureEnumConventions` converter reading 3.x text (baseline), `HasLegacyEnumStorage`, and the two direct classes | every converter reads back what it wrote; every CSE converter reads the legacy text to the same members; each direct class writes the same text as its Readme-path counterpart |
| 6 | `MigrateDataComparison` | sync a table of `Count` = 1,000 or 10,000 countries to seed data that drops 10%, renames 10% and adds 10% | hand-written dictionary diff with `RemoveRange`/`UpdateRange`/`AddRange` (baseline), CSE `MigrateDataAsync` with the Readme's options | row count and a SHA-256 of every (Code, Name) pair after the save |
| 7 | `DbContextLifetimeComparison` | create, initialize and dispose a context; CreateQueryDispose adds one `AnyAsync` | `new` (baseline), `AddDbContext` scope, pooled factory (labelled), CSE `BaseDbContext` through `AddDbContext`, the same with the audit and slow query interceptors | every row initializes; every query row finds the one row |
| 8 | `DbErrorTranslationComparison` | a save that violates a unique index | `catch` on `SqliteException` extended code 2067 (baseline), EntityFrameworkCore.Exceptions `UseExceptionProcessor` + `catch (UniqueConstraintException)` (labelled), CSE `DbErrorTranslation.SaveChangesAsync` with a SQLite translator (labelled) | every row reports the unique violation and the table keeps one row |
| 9 | `DbErrorClassificationComparison` | classify an exception already caught, no database | Sqlite: pattern match (baseline), DbExceptionClassifier `IsUniqueConstraintError`, CSE `DbErrorTranslation.TryTranslate`. SqlState: pattern match on SQLSTATE 23505 (baseline), CSE `SqlStateErrorTranslator` (labelled) | every row classifies the input as a unique violation |

### Inputs

- `AuditedOrder : EntityBase<int>` with `Name` and `Amount`. For Modified, `[GlobalSetup]` inserts `Count` rows once per parameter value; each invocation attaches the same detached instances, changes `Amount` and saves.
- `FeedItem` with `Id`, `PublishedAt` (`DateTime`, UTC) and `Title`, 10,000 rows, ten rows per timestamp, indexed on (PublishedAt, Id). It does not derive from `EntityBase`: that type already has a `DateTimeOffset CreatedAt`, and SQLite cannot order by `DateTimeOffset`. `DateTime` plus `int` is a key every library translates on SQLite.
- `Country` keyed by `Code` (`C000000` onwards), and `CountrySeed(Code, Name)`.
- `UniqueItem` with a unique index on `Name`; `[GlobalSetup]` inserts `dup`, and every invocation adds another `dup`.
- `OrderStatus { Pending, InTransit, Delivered, Cancelled, Level2Check }`, a `[StringEnum]`. `Level2Check` is `level2_check` as a wire name and `level_2_check` in the 3.x legacy format, so the legacy input differs from the wire input; `[GlobalSetup]` checks that it matches neither the wire name nor the member name. The generated metadata lists every 3.x spelling as an alias, so the current converter reads legacy text by lookup and never reaches its fallback loop (`Converters/EnumToStringConverter.cs:35-37`).
- The DbErrorClassification inputs are a `DbUpdateException` wrapping `new SqliteException(…, 19, 2067)` and a provider-neutral `DbException` whose `SqlState` is `23505`, both built once.

---

## Semantic differences

- **Reference rows.** The plain EF Core rows of SaveChangesAudit and SaveChangesDomainEvents do less work: no audit fields, no dispatch. They show what the feature adds, not a competitor.
- **Audit scope.** CSE's `AuditInterceptor` creates a DI scope and resolves `IAuditUserIdProvider` on every save (`Interceptors/Audit/AuditInterceptor.cs:42-44`). The hand-written override reads a user captured in the constructor. Both stamp the same fields with the same user.
- **When AfterSave events run.** `DomainEventInterceptor` dispatches `AfterSave` events inside `SavingChanges`, before the write (`Interceptors/DomainEvents/DomainEventInterceptor.cs:46-49`). The hand-written row and `BaseDbContext` dispatch after the save. The event count is the same.
- **Cursor format.** The hand-written cursor is `{"c":ticks,"i":id}` in base64url with no version, direction or key check. CSE's cursor is versioned JSON with a direction and a key fingerprint, and CSE also issues the previous cursor. MR has no cursor: the rows decode the hand-written cursor and pass it as MR's reference object, then encode the next one the same way.
- **MR has-next.** MR's documented `HasNextAsync` runs an extra `Any` query per page. The two `TakePlusOne` rows read `limit + 1` rows instead, the way CSE and the hand-written row do, so they send one query.
- **X.PagedList past the end.** X.PagedList skips the data query when the page is past the last one and throws when the page number is below 1. CSE normalizes the request and always runs both queries. Both pages here are in range, so all rows do the same work.
- **Stored enum text.** EF Core stores the member name (`InTransit`), CSE the wire name (`in_transit`), and `HasLegacyEnumStorage(LegacySnakeCase)` the 3.x text. EF Core also reads numbers and any casing; an unknown value throws in every row.
- **Readme path and direct classes.** The `ConfigureEnumConventions` and `HasLegacyEnumStorage` rows take their converters from a model built the way the Readme shows (`EnumStorageContext`), so they measure what the documented setup installs. `EnumToFormattedStringConverter` and `LegacySnakeCaseEnumConverter` are public converter classes the Readme does not document; their rows measure the classes directly and are not the Readme path.
- **Pooling.** The pooled factory reuses context instances. `BaseDbContext` cannot be pooled, because its constructor needs `IServiceScopeFactory`, so the pooled row is EF Core only.
- **SQLite errors.** SQLite reports no SQLSTATE, so CSE's built-in `SqlStateErrorTranslator` returns false there (`DbErrors/SqlStateErrorTranslator.cs:38-40`). CSE's documented SQLite path is a translator you write; the suite registers the one from CSE's tests (`SqliteUniqueTranslator`). CSE returns a failed `Result<int>` and does not throw. EntityFrameworkCore.Exceptions throws a second, typed exception; on SQLite it does not fill `ConstraintName` or `ConstraintProperties`.
- **SqlState rows have no competitor.** EntityFrameworkCore.Exceptions needs a provider exception type and cannot classify a SQLSTATE-only `DbException`. CSE's translator also builds metadata and an `Error` (`DbErrors/SqlStateErrorTranslator.cs:42-55`).

---

## Feature matrix

This table comes from reading CSE source and the competitors' source and docs at their release commits. None of it was measured.

| Feature | CSE | MR KeysetPagination 1.6.0 | X.PagedList.EF 10.6.0 | EF.Exceptions 10.0.1 | EF Core alone |
|---|---|---|---|---|---|
| Keyset paging, composite key, mixed directions | Yes | Yes | No | No | By hand |
| Opaque, validated cursor (version, direction, key fingerprint), pluggable protector | Yes | No (reference object) | No | No | No |
| Has-next/has-previous without an extra query | Yes (`limit + 1`) | No (`HasNextAsync`/`HasPreviousAsync` run queries) | n/a | n/a | By hand |
| Reusable prebuilt ordering | `KeysetOrdering<T>` | `KeysetQuery.Build` | n/a | n/a | n/a |
| Nullable keys | Rejected at run time (`ArgumentException`) | Not supported: compile-time analyzer warning `KeysetPagination1000`; [coalescing or computed-column workaround documented](https://github.com/mrahhal/MR.EntityFrameworkCore.KeysetPagination/blob/84fd37923e35db4a23244e40189fd42efb60caf6/docs/caveats.md#null) | n/a | n/a | n/a |
| Offset paging with total count | `PaginateAsync` | No | Yes | No | By hand |
| Page-size cap and normalization | Yes (100 by default) | n/a | Throws below page 1 | n/a | No |
| Typed database errors | `Error`/`Result` through SQLSTATE plus custom translators | No | No | Typed exceptions, one package per provider | No |
| SQLite unique violation out of the box | No (needs a translator) | n/a | n/a | Yes | No |
| Audit fields and soft delete on save | Yes | No | No | No | By hand |
| Domain event dispatch around the save (before, after, outbox) | Yes | No | No | No | By hand |
| Enum stored as a wire name, tolerant read, check constraint | Yes | No | No | No | `EnumToStringConverter` (member name) |
| Seed data sync | `MigrateDataAsync` | No | No | No | By hand |
| Works with context pooling | `BaseDbContext` no, interceptors yes | n/a | n/a | Yes | Yes |
| Target frameworks | net8.0, net9.0, net10.0 (net11.0 loads net10.0) | net10.0 | net6.0, net8.0 (EF Core 9+), net10.0 | net10.0 | per major |
| License | MIT | MIT | MIT | Apache-2.0 | MIT |

---

## Results

**Pending the full default-job run.** The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/efcore/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **In-memory SQLite, not a server.** There is no network and no server-side planning cost, so the database part of each row is small and the library overhead is a larger share than it would be against a server database such as PostgreSQL. The SQLSTATE path is measured only in the CPU-only class; an end-to-end PostgreSQL run is out of scope.
- **No standalone cursor codec row.** CSE's `KeysetCursorCodec` (`Pagination/Keyset/KeysetCursorCodec.cs:12`) and `DomainEventCollector` (`Interceptors/DomainEvents/DomainEventCollector.cs:13`) are internal, and the benchmarks do not change package source to reach them. `After` minus `First` on the CSE keyset rows is the only view of the cursor decode and predicate cost.
- **`MigrateDataAsync` at 10,000 rows** matches entities to seed rows with nested `All` loops, so expect seconds per operation.
- **No Native AOT sample.** EF Core builds its model and translates LINQ at run time, MR builds expression trees and reads the reference object by reflection, and EntityFrameworkCore.Exceptions is an EF Core interceptor, so none of these rows is AOT-ready.
- **Expected CSE costs, from reading the code** (not yet measured):
  - `Interceptors/Audit/AuditInterceptor.cs:42`: a DI scope on every save, even when nothing is auditable; `:47` enumerates `ChangeTracker.Entries()`, which runs change detection.
  - `Interceptors/DomainEvents/DomainEventCollector.cs:49`: `[.. entity.DomainEvents]` copies a list that `EntityBase.DomainEvents` already copied, once per tracked entity per save; `:26-28` and `:45-47` call `ChangeTracker.Entries`, which runs change detection again.
  - `Abstractions/BaseDbContext.cs:16` and `:24-27`: a GUID, a DI scope and an Information log call per instance. `:172-182`: a finalizer with no `GC.SuppressFinalize` in `Dispose`, so every disposed context still goes through the finalizer queue.
  - `Pagination/KeysetPaginationExtensions.cs:41`: the `Func` overload, the form the Readme shows, builds a new `KeysetOrdering` on every call, including the SHA-256 key fingerprint (`Pagination/Keyset/KeysetOrdering.cs:56-63`) and the key validation.
  - `Pagination/Keyset/KeysetCursorCodec.cs:88`: the `Invalid` error is built before the cursor is checked, so a successful decode creates it too.
  - `Extensions/DbContextExtensionMethods.cs:153-155` and `:165-167`: O(n·m) `All` matching. `:147-148`: both key selectors are compiled on every call.
  - `Converters/EnumToStringConverter.cs:35-37`: the legacy fallback allocates `Enum.GetValues` and a `ToString().ToSnakeCase()` per member on every row it reads. It runs only when the enum's metadata has no 3.x alias (an enum without `[StringEnum]` generated metadata); a `[StringEnum]` enum, as in this suite, never reaches it. `Converters/LegacySnakeCaseEnumConverter.cs:22-24`: allocates on every read and write.
- The CSE column is the working tree on branch `perf/bench-efcore`, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project efcore                       # all TFMs (net8-net11), default job
./run-benchmarks.sh --project efcore --tfm net10.0         # one TFM
./run-benchmarks.sh --project efcore --quick               # net10.0, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks -- --self-check
```

Without `--tfm`, `--quick` runs on net10.0, because the MR and EntityFrameworkCore.Exceptions rows exist only there and on net11.0. Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/efcore/{tfm}/results/`.
