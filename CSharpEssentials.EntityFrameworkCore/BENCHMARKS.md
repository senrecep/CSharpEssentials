# CSharpEssentials.EntityFrameworkCore: Performance benchmarks

This page compares CSharpEssentials.EntityFrameworkCore (CSE) with plain EF Core, MR.EntityFrameworkCore.KeysetPagination 1.6.0, X.PagedList.EF 10.6.0 and EntityFrameworkCore.Exceptions.Sqlite 10.0.1. It covers SaveChanges with the audit and domain event paths, keyset and offset paging, the enum column converters, `MigrateDataAsync`, context creation, and database error translation. Every database row runs on in-memory SQLite. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

The same project, `benchmarks/CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks/` (suite key `efcore`), also holds the `EntityBase` domain event class, documented in [CSharpEssentials.Entity/BENCHMARKS.md](../CSharpEssentials.Entity/BENCHMARKS.md).

The numbers on this page come from full default-job runs on net8.0, net9.0, net10.0 and net11.0 (see [Results](#results) and [How these numbers were produced](#how-these-numbers-were-produced)). The parity self-check passes on every TFM.

The competitors are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License (nuspec expression) | Asset the suite loads |
|---|---|---|---|---|
| MR.EntityFrameworkCore.KeysetPagination | 1.6.0 | 2026-04-06 | MIT | `lib/net10.0` (the only asset) on net10.0 and net11.0 |
| X.PagedList.EF (with X.PagedList 10.6.0) | 10.6.0 | 2026-09-06 | MIT | `lib/net8.0` on net9.0, `lib/net10.0` on net10.0 and net11.0 |
| EntityFrameworkCore.Exceptions.Sqlite (with .Common 10.0.1, DbExceptionClassifier.Sqlite 1.0.1) | 10.0.1 | 2026-05-24 | Apache-2.0 | `lib/net10.0` (the only asset) on net10.0 and net11.0 |
| EF Core and its SQLite provider | 8.0.33, 9.0.22, 10.0.14 | 2026-10-25 | MIT | one major per TFM, see below |

`X.PagedList.EntityFramework` is not in the table: its 10.6.0 release depends on classic EF6 (`EntityFramework [6.5.2,7)`) and cannot page an EF Core query. `X.PagedList.EF` is the same author's EF Core package. Its `lib/net8.0` asset needs EF Core 9.0.18 or later, so restore fails with NU1605 against CSE's net8.0 EF Core 8 build; X.PagedList has no net8.0 row. MR and EntityFrameworkCore.Exceptions ship a `net10.0` asset only, so their rows run on net10.0 and net11.0. Transitive pinning is off for this project, as it is for the DI, mediator, endpoints, Http and AspNetCore suites: the central `[9.0.4,10)` EF Core pin would fight the EF Core 8 and 10 rows.

What each TFM runs (`--self-check` prints this as its first line):

| TFM | EF Core and Sqlite provider | CSE.EntityFrameworkCore build | Competitor rows |
|---|---|---|---|
| net8.0 | 8.0.33 | net8.0 | none (CSE and hand-written rows only) |
| net9.0 | 9.0.22 | net9.0 | X.PagedList.EF |
| net10.0 | 10.0.14 | net10.0 | X.PagedList.EF, MR, EntityFrameworkCore.Exceptions |
| net11.0 | 10.0.14 | net10.0 (the package has no net11.0 target; EF Core 11 is still a release candidate) | X.PagedList.EF, MR, EntityFrameworkCore.Exceptions |

On net10.0 and net11.0 EntityFrameworkCore.Exceptions brings the SQLite ADO.NET package 10.0.5 next to the 10.0.14 core package that EF Core's SQLite provider uses.

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
- The DbErrorClassification inputs are a `DbUpdateException` wrapping `new SqliteException(..., 19, 2067)` and a provider-neutral `DbException` whose `SqlState` is `23505`, both built once.

---

## Semantic differences

- Reference rows: the plain EF Core rows of SaveChangesAudit and SaveChangesDomainEvents do less work: no audit fields, no dispatch. They show what the feature adds, not a competitor.
- Audit scope: CSE's `AuditInterceptor` creates a DI scope and resolves `IAuditUserIdProvider` once per save that has entries to stamp (`Interceptors/Audit/AuditInterceptor.cs:83-87`). The hand-written override reads a user captured in the constructor. Both stamp the same fields with the same user.
- When AfterSave events run: `DomainEventInterceptor` dispatches `AfterSave` events inside `SavingChanges`, before the write (`Interceptors/DomainEvents/DomainEventInterceptor.cs:58-61`). The hand-written row and `BaseDbContext` dispatch after the save. The event count is the same.
- Cursor format: the hand-written cursor is `{"c":ticks,"i":id}` in base64url with no version, direction or key check. CSE's cursor is versioned JSON with a direction and a key fingerprint, and CSE also issues the previous cursor. MR has no cursor: the rows decode the hand-written cursor and pass it as MR's reference object, then encode the next one the same way.
- MR has-next: MR's documented `HasNextAsync` runs an extra `Any` query per page. The two `TakePlusOne` rows read `limit + 1` rows instead, the way CSE and the hand-written row do, so they send one query.
- X.PagedList past the end: X.PagedList skips the data query when the page is past the last one and throws when the page number is below 1. CSE normalizes the request and always runs both queries. Both pages here are in range, so all rows do the same work.
- Stored enum text: EF Core stores the member name (`InTransit`), CSE the wire name (`in_transit`), and `HasLegacyEnumStorage(LegacySnakeCase)` the 3.x text. EF Core also reads numbers and any casing; an unknown value throws in every row.
- Readme path and direct classes: the `ConfigureEnumConventions` and `HasLegacyEnumStorage` rows take their converters from a model built the way the Readme shows (`EnumStorageContext`), so they measure what the documented setup installs. `EnumToFormattedStringConverter` and `LegacySnakeCaseEnumConverter` are public converter classes the Readme does not document; their rows measure the classes directly and are not the Readme path.
- Pooling: the pooled factory reuses context instances. `BaseDbContext` cannot be pooled, because its constructor needs `IServiceScopeFactory`, so the pooled row is EF Core only.
- SQLite errors: SQLite reports no SQLSTATE, so CSE's built-in `SqlStateErrorTranslator` returns false there (`DbErrors/SqlStateErrorTranslator.cs:38-40`). CSE's documented SQLite path is a translator you write; the suite registers the one from CSE's tests (`SqliteUniqueTranslator`). CSE returns a failed `Result<int>` and does not throw. EntityFrameworkCore.Exceptions throws a second, typed exception; on SQLite it does not fill `ConstraintName` or `ConstraintProperties`.
- SqlState rows have no competitor: EntityFrameworkCore.Exceptions needs a provider exception type and cannot classify a SQLSTATE-only `DbException`. CSE's translator also builds metadata and an `Error` (`DbErrors/SqlStateErrorTranslator.cs:42-55`).

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

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job, full runs (no `--quick`), one suite at a time, on a 6-core, 12-thread x64 desktop CPU on Linux. The exact SDK and runtime builds are in the header of each report.
- EF Core per TFM: 8.0.33 (net8.0), 9.0.22 (net9.0), 10.0.14 (net10.0 and net11.0).
- The reports come from two runs:
  - First run, SDK 11.0.100-rc.1: every class except the two below, on .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0. The 1-minute load average stayed between 1.99 and 2.60 during the suite and its reruns. `DbErrorClassificationComparison` was rerun on net10.0 and net11.0 because of its CV.
  - Rerun after the changes in #230 to #256, SDK 11.0.100-rc.3: `EnumConverterComparison` and `MigrateDataComparison`, on .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0.
- No row of this suite has a CV above 5%.
- Reports: [`benchmarks/results/efcore/{tfm}/results/`](../benchmarks/results/efcore/), one `*-report-github.md` and one `.csv` per class. Every number below is copied from the `-report-github.md` files.

How to read the tables:
- Each cell is `Mean (Ratio), Allocated` for that TFM. Ratio is against the row marked baseline in the same group and TFM. `-` in Allocated means BenchmarkDotNet measured no allocation.
- Compare rows within one TFM column only. Each TFM runs a different runtime and, for net8.0 and net9.0, a different EF Core major, so the columns do not rank the runtimes against each other.
- `n/a` means the row does not exist on that TFM (see the table at the top of the page).
- In-memory SQLite keeps the database share of each row small, so the library overhead is a larger share than it would be on a server database.

### SaveChangesAudit

Count 1 and 1,000 are shown; Count 100 is in the reports. Plain EF Core is a reference that stamps nothing.

| Count | State | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| 1 | Modified | EF Core `SaveChanges` (baseline, reference) | 68.93 μs (1.00), 12.91 KB | 65.09 μs (1.00), 12.96 KB | 57.48 μs (1.00), 12.27 KB | 54.00 μs (1.00), 12.21 KB |
| 1 | Modified | Hand-written override | 75.87 μs (1.10), 14.22 KB | 71.44 μs (1.10), 14.32 KB | 62.66 μs (1.09), 13.61 KB | 54.97 μs (1.02), 13.49 KB |
| 1 | Modified | CSE `AuditInterceptor` | 77.42 μs (1.12), 14.75 KB | 71.12 μs (1.09), 14.83 KB | 63.74 μs (1.11), 14.08 KB | 58.32 μs (1.08), 13.96 KB |
| 1 | Modified | CSE `BaseDbContext` audit | 77.42 μs (1.12), 14.75 KB | 72.54 μs (1.11), 14.83 KB | 64.33 μs (1.12), 14.02 KB | 59.77 μs (1.11), 13.96 KB |
| 1 | Added | EF Core `SaveChanges` (baseline, reference) | 77.41 μs (1.00), 16.4 KB | 72.81 μs (1.00), 16.38 KB | 67.80 μs (1.00), 16 KB | 61.50 μs (1.00), 15.91 KB |
| 1 | Added | Hand-written override | 81.03 μs (1.05), 17.11 KB | 76.53 μs (1.05), 17.13 KB | 69.23 μs (1.02), 16.63 KB | 65.29 μs (1.06), 16.58 KB |
| 1 | Added | CSE `AuditInterceptor` | 82.81 μs (1.07), 17.64 KB | 78.32 μs (1.08), 17.63 KB | 70.90 μs (1.05), 17.1 KB | 67.81 μs (1.10), 17.05 KB |
| 1 | Added | CSE `BaseDbContext` audit | 84.01 μs (1.09), 17.64 KB | 78.89 μs (1.08), 17.63 KB | 71.32 μs (1.05), 17.1 KB | 65.95 μs (1.07), 17.05 KB |
| 1000 | Modified | EF Core `SaveChanges` (baseline, reference) | 20,431.85 μs (1.00), 7551.55 KB | 21,835.88 μs (1.00), 7598.48 KB | 18,860.87 μs (1.00), 7262.28 KB | 18,511.63 μs (1.00), 7191.94 KB |
| 1000 | Modified | Hand-written override | 25,690.91 μs (1.26), 8629.91 KB | 25,424.01 μs (1.16), 8731.54 KB | 22,548.00 μs (1.20), 8395.23 KB | 21,340.98 μs (1.15), 8364.11 KB |
| 1000 | Modified | CSE `AuditInterceptor` | 26,303.08 μs (1.29), 8646.56 KB | 25,145.12 μs (1.15), 8739.85 KB | 21,914.24 μs (1.16), 8403.51 KB | 22,043.16 μs (1.19), 8372.38 KB |
| 1000 | Modified | CSE `BaseDbContext` audit | 26,573.26 μs (1.30), 8646.85 KB | 24,599.40 μs (1.13), 8739.92 KB | 22,563.81 μs (1.20), 8403.57 KB | 22,131.50 μs (1.20), 8372.29 KB |
| 1000 | Added | EF Core `SaveChanges` (baseline, reference) | 30,334.55 μs (1.00), 11035.93 KB | 28,213.75 μs (1.00), 11012.56 KB | 25,902.24 μs (1.00), 10879.47 KB | 24,474.74 μs (1.00), 10848.49 KB |
| 1000 | Added | Hand-written override | 31,481.34 μs (1.04), 11520.56 KB | 30,462.29 μs (1.08), 11536.54 KB | 27,625.17 μs (1.07), 11403.25 KB | 27,050.79 μs (1.11), 11371.76 KB |
| 1000 | Added | CSE `AuditInterceptor` | 30,864.49 μs (1.02), 11537.19 KB | 30,630.63 μs (1.09), 11544.57 KB | 28,275.79 μs (1.09), 11411.65 KB | 27,032.64 μs (1.11), 11380.25 KB |
| 1000 | Added | CSE `BaseDbContext` audit | 31,591.57 μs (1.04), 11537.29 KB | 30,138.90 μs (1.07), 11544.6 KB | 27,447.65 μs (1.06), 11411.7 KB | 27,309.38 μs (1.12), 11380.42 KB |

At Count 1, both CSE rows are slower than the hand-written override in 15 of 16 cells; the exception is the net9.0 Modified `AuditInterceptor` row (71.12 μs against 71.44 μs). At Count 1,000 the CSE rows are slower in 9 of 16 cells and faster in the other 7. At Count 100 (in the reports, not shown) they are slower in 9 of 16 cells. CSE allocates more than the hand-written override in every cell, by about 0.5 KB at Count 1 and 8 to 17 KB at Count 1,000.

### SaveChangesDomainEvents

Count 1 and 1,000 are shown; Count 100 is in the reports. Plain EF Core is a reference that dispatches nothing.

| Count | Events per entity | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| 1 | 0 | EF Core `SaveChanges` (baseline, reference) | 77.15 μs (1.00), 16.4 KB | 73.01 μs (1.00), 16.38 KB | 66.56 μs (1.00), 15.96 KB | 61.11 μs (1.00), 15.91 KB |
| 1 | 0 | Hand-written dispatch after save | 79.42 μs (1.03), 17.18 KB | 75.14 μs (1.03), 17.2 KB | 69.31 μs (1.04), 16.7 KB | 64.15 μs (1.05), 16.69 KB |
| 1 | 0 | CSE `BaseDbContext` dispatch | 82.96 μs (1.08), 17.52 KB | 78.01 μs (1.07), 17.41 KB | 69.66 μs (1.05), 17 KB | 64.14 μs (1.05), 16.86 KB |
| 1 | 0 | CSE `DomainEventInterceptor` (labelled) | 80.04 μs (1.04), 17.42 KB | 75.27 μs (1.03), 17.44 KB | 70.60 μs (1.06), 16.95 KB | 65.07 μs (1.06), 16.92 KB |
| 1 | 1 | EF Core `SaveChanges` (baseline, reference) | 77.95 μs (1.00), 16.48 KB | 71.94 μs (1.00), 16.45 KB | 66.66 μs (1.00), 16.08 KB | 61.33 μs (1.00), 15.98 KB |
| 1 | 1 | Hand-written dispatch after save | 80.66 μs (1.04), 17.31 KB | 75.41 μs (1.05), 17.33 KB | 69.68 μs (1.05), 16.88 KB | 64.34 μs (1.05), 16.78 KB |
| 1 | 1 | CSE `BaseDbContext` dispatch | 88.66 μs (1.14), 18.09 KB | 79.43 μs (1.10), 17.99 KB | 71.45 μs (1.07), 17.54 KB | 66.67 μs (1.09), 17.45 KB |
| 1 | 1 | CSE `DomainEventInterceptor` (labelled) | 87.89 μs (1.13), 18.27 KB | 82.39 μs (1.15), 18.17 KB | 72.98 μs (1.09), 17.72 KB | 65.96 μs (1.08), 17.63 KB |
| 1000 | 0 | EF Core `SaveChanges` (baseline, reference) | 31,131.96 μs (1.00), 11035.93 KB | 28,846.53 μs (1.00), 11012.53 KB | 25,967.90 μs (1.00), 10879.46 KB | 24,563.32 μs (1.00), 10848.17 KB |
| 1000 | 0 | Hand-written dispatch after save | 31,337.46 μs (1.01), 11497.29 KB | 29,219.42 μs (1.01), 11512.94 KB | 27,640.53 μs (1.06), 11379.99 KB | 26,951.60 μs (1.10), 11348.49 KB |
| 1000 | 0 | CSE `BaseDbContext` dispatch | 31,492.46 μs (1.01), 11497.63 KB | 29,843.47 μs (1.03), 11513.31 KB | 28,250.70 μs (1.09), 11380.21 KB | 26,062.53 μs (1.06), 11348.78 KB |
| 1000 | 0 | CSE `DomainEventInterceptor` (labelled) | 30,170.68 μs (0.97), 11497.6 KB | 30,794.20 μs (1.07), 11513.13 KB | 26,537.53 μs (1.02), 11379.94 KB | 26,263.05 μs (1.07), 11348.73 KB |
| 1000 | 1 | EF Core `SaveChanges` (baseline, reference) | 30,310.24 μs (1.00), 11114.27 KB | 29,628.09 μs (1.00), 11090.69 KB | 26,376.36 μs (1.00), 10957.6 KB | 25,250.46 μs (1.00), 10926.46 KB |
| 1000 | 1 | Hand-written dispatch after save | 31,467.85 μs (1.04), 11591.56 KB | 29,357.71 μs (0.99), 11607.58 KB | 26,420.06 μs (1.00), 11474.06 KB | 26,612.96 μs (1.05), 11442.95 KB |
| 1000 | 1 | CSE `BaseDbContext` dispatch | 31,050.66 μs (1.02), 11703.79 KB | 29,645.27 μs (1.00), 11686.89 KB | 26,958.07 μs (1.02), 11553.71 KB | 26,811.85 μs (1.06), 11522.66 KB |
| 1000 | 1 | CSE `DomainEventInterceptor` (labelled) | 31,616.78 μs (1.04), 11703.71 KB | 30,554.50 μs (1.03), 11687.03 KB | 27,041.79 μs (1.03), 11553.84 KB | 26,982.25 μs (1.07), 11522.77 KB |

At Count 1, the CSE rows are slower than the hand-written row in 15 of 16 cells (the exception is net11.0 `BaseDbContext` with 0 events, 64.14 μs against 64.15 μs). At Count 1,000 they are slower in 11 of 16 cells, and at Count 100 (in the reports, not shown) in 12 of 16. CSE allocates more than the hand-written row in every cell of the three counts but one: net10.0, Count 1,000, 0 events, where the interceptor row allocates 11379.94 KB against 11379.99 KB. With one event per entity, CSE allocates about 80 to 112 KB more than the hand-written row at Count 1,000 (for example net10.0: 11553.71 KB against 11474.06 KB).

### KeysetPagination

MR rows exist on net10.0 and net11.0 only.

| Page | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| First | Hand-written Where/OrderBy/Take (baseline) | 87.87 μs (1.00), 15.79 KB | 81.99 μs (1.00), 18.1 KB | 75.46 μs (1.00), 18.26 KB | 70.23 μs (1.00), 18.08 KB |
| First | CSE cached `KeysetOrdering` | 84.37 μs (0.96), 15.52 KB | 79.02 μs (0.96), 17.83 KB | 72.12 μs (0.96), 17.88 KB | 68.13 μs (0.97), 17.81 KB |
| First | CSE inline `Func` ordering | 110.80 μs (1.26), 19.48 KB | 99.56 μs (1.21), 21.77 KB | 87.31 μs (1.16), 21.96 KB | 82.95 μs (1.18), 21.74 KB |
| First | MR prebuilt + Take(21) | n/a | n/a | 72.24 μs (0.96), 17.97 KB | 69.67 μs (0.99), 17.81 KB |
| First | MR inline + Take(21) | n/a | n/a | 78.66 μs (1.04), 21.06 KB | 71.51 μs (1.02), 20.89 KB |
| First | MR prebuilt + `HasNextAsync` (labelled) | n/a | n/a | 173.53 μs (2.30), 36.13 KB | 156.74 μs (2.23), 35.7 KB |
| After | Hand-written Where/OrderBy/Take (baseline) | 138.85 μs (1.00), 19.86 KB | 127.62 μs (1.00), 22.54 KB | 119.54 μs (1.00), 23.15 KB | 113.27 μs (1.00), 23.1 KB |
| After | CSE cached `KeysetOrdering` | 140.99 μs (1.02), 21.34 KB | 131.77 μs (1.03), 24.18 KB | 124.43 μs (1.04), 25.33 KB | 112.84 μs (1.00), 25.13 KB |
| After | CSE inline `Func` ordering | 163.95 μs (1.18), 25.74 KB | 146.95 μs (1.15), 28.11 KB | 137.49 μs (1.15), 28.95 KB | 128.46 μs (1.13), 29.13 KB |
| After | MR prebuilt + Take(21) | n/a | n/a | 145.81 μs (1.22), 30.38 KB | 134.76 μs (1.19), 30.54 KB |
| After | MR inline + Take(21) | n/a | n/a | 411.78 μs (3.44), 42.58 KB | 413.32 μs (3.65), 42.52 KB |
| After | MR prebuilt + `HasNextAsync` (labelled) | n/a | n/a | 233.43 μs (1.95), 48.59 KB | 217.47 μs (1.92), 47.98 KB |

- On the After page, CSE with a cached ordering is slower than the hand-written row on net8.0, net9.0 and net10.0 (ratio 1.02 to 1.04) and allocates 1.48 to 2.18 KB more on every TFM; it validates and issues two cursors where the hand-written row decodes one unchecked cursor. It is faster than MR prebuilt with `Take(21)` on both TFMs that have MR.
- On the First page, CSE cached and MR prebuilt are within 0.12 μs of each other on net10.0 (72.12 μs against 72.24 μs); on net11.0 CSE is at 68.13 μs and MR at 69.67 μs with the same allocation.
- The inline `Func` ordering, the form the Readme shows, costs 13.06 to 26.43 μs more than the cached ordering. On the First page it is slower than MR's inline form: 87.31 μs against 78.66 μs on net10.0 and 82.95 μs against 71.51 μs on net11.0, and it allocates more. The `Func` form is also slower than MR prebuilt with `Take(21)` on the First page (87.31 μs against 72.24 μs on net10.0, 82.95 μs against 69.67 μs on net11.0) and slower than the hand-written row on every page and TFM (ratio 1.13 to 1.26). On the After page MR's inline form is the slowest row (411.78 μs on net10.0).

### OffsetPagination

X.PagedList.EF has no net8.0 row.

| Page number | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| 1 | EF Core `CountAsync` + Skip/Take (baseline) | 104.2 μs (1.00), 19.37 KB | 101.1 μs (1.00), 23.43 KB | 90.74 μs (1.00), 23.59 KB | 84.24 μs (1.00), 23.49 KB |
| 1 | CSE `PaginateAsync` | 110.2 μs (1.06), 19.56 KB | 100.4 μs (0.99), 23.47 KB | 91.74 μs (1.01), 23.63 KB | 86.80 μs (1.03), 23.53 KB |
| 1 | X.PagedList `ToPagedListAsync` | n/a | 100.3 μs (0.99), 23.7 KB | 90.38 μs (1.00), 23.86 KB | 87.10 μs (1.03), 23.63 KB |
| 250 | EF Core `CountAsync` + Skip/Take (baseline) | 147.9 μs (1.00), 19.37 KB | 143.5 μs (1.00), 23.43 KB | 132.06 μs (1.00), 23.59 KB | 126.32 μs (1.00), 23.49 KB |
| 250 | CSE `PaginateAsync` | 149.7 μs (1.01), 19.41 KB | 142.2 μs (0.99), 23.47 KB | 136.71 μs (1.04), 24.24 KB | 126.63 μs (1.00), 23.53 KB |
| 250 | X.PagedList `ToPagedListAsync` | n/a | 142.8 μs (1.00), 23.7 KB | 136.60 μs (1.03), 24 KB | 128.59 μs (1.02), 23.76 KB |

All three rows send the same two queries, and the differences are a few percent in both directions. CSE is slower than X.PagedList on net9.0 page 1 (100.4 μs against 100.3 μs), net10.0 page 1 (91.74 μs against 90.38 μs) and net10.0 page 250 (136.71 μs against 136.60 μs, and 24.24 KB against 24 KB). CSE is slower than plain EF Core in 6 of 8 cells, the largest gap being net8.0 page 1 (110.2 μs against 104.2 μs), and allocates 0.04 to 0.65 KB more than plain EF Core in all 8 cells.

### EnumConverter

Cells are `Mean (Ratio)`. No Read or ReadLegacy row allocates. On Write, EF Core's `EnumToStringConverter` allocates 120 B per call on every TFM and every CSE row allocates nothing.

| Group | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| Write | EF Core `EnumToStringConverter` (baseline) | 60.53 ns (1.00) | 59.14 ns (1.00) | 47.04 ns (1.00) | 49.55 ns (1.00) |
| Write | CSE `ConfigureEnumConventions` | 53.88 ns (0.89) | 53.00 ns (0.90) | 36.02 ns (0.77) | 35.32 ns (0.71) |
| Write | CSE `HasLegacyEnumStorage` | 32.14 ns (0.53) | 31.28 ns (0.53) | 30.79 ns (0.65) | 30.65 ns (0.62) |
| Write | `EnumToFormattedStringConverter` (direct class) | 51.70 ns (0.85) | 48.32 ns (0.82) | 55.61 ns (1.18) | 55.62 ns (1.12) |
| Write | `LegacySnakeCaseEnumConverter` (direct class) | 22.31 ns (0.37) | 22.45 ns (0.38) | 26.50 ns (0.56) | 26.49 ns (0.53) |
| Read | EF Core `EnumToStringConverter` (baseline) | 94.49 ns (1.00) | 90.62 ns (1.00) | 79.94 ns (1.00) | 73.32 ns (1.00) |
| Read | CSE `ConfigureEnumConventions` | 78.17 ns (0.83) | 78.92 ns (0.87) | 70.06 ns (0.88) | 68.40 ns (0.93) |
| Read | CSE `HasLegacyEnumStorage` | 60.46 ns (0.64) | 58.16 ns (0.64) | 56.49 ns (0.71) | 56.66 ns (0.77) |
| Read | `EnumToFormattedStringConverter` (direct class) | 74.34 ns (0.79) | 73.57 ns (0.81) | 54.09 ns (0.68) | 53.03 ns (0.72) |
| Read | `LegacySnakeCaseEnumConverter` (direct class) | 52.68 ns (0.56) | 53.13 ns (0.59) | 46.17 ns (0.58) | 46.11 ns (0.63) |
| ReadLegacy | CSE `ConfigureEnumConventions` (baseline) | 85.68 ns (1.00) | 84.65 ns (1.00) | 72.30 ns (1.00) | 72.86 ns (1.00) |
| ReadLegacy | CSE `HasLegacyEnumStorage` | 62.31 ns (0.73) | 58.12 ns (0.69) | 57.45 ns (0.79) | 55.13 ns (0.76) |
| ReadLegacy | `EnumToFormattedStringConverter` (direct class) | 79.30 ns (0.93) | 77.11 ns (0.91) | 60.39 ns (0.84) | 60.69 ns (0.83) |
| ReadLegacy | `LegacySnakeCaseEnumConverter` (direct class) | 66.64 ns (0.78) | 68.16 ns (0.81) | 63.47 ns (0.88) | 61.79 ns (0.85) |

CSE writes are faster than EF Core in every Readme-path cell and allocate nothing. CSE reads are faster than EF Core in every cell as well: `ConfigureEnumConventions` at 0.83 to 0.93 and `HasLegacyEnumStorage` at 0.64 to 0.77. The direct `EnumToFormattedStringConverter` class still writes slower than EF Core on net10.0 (55.61 ns against 47.04 ns) and net11.0 (55.62 ns against 49.55 ns).

### MigrateData

| Count | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| 1000 | Hand-written dictionary diff (baseline) | 7.033 ms (1.00), 3.04 MB | 6.586 ms (1.00), 3 MB | 6.109 ms (1.00), 2.94 MB | 5.803 ms (1.00), 2.94 MB |
| 1000 | CSE `MigrateDataAsync` | 7.302 ms (1.04), 3.16 MB | 6.832 ms (1.04), 3.12 MB | 6.208 ms (1.02), 3.06 MB | 6.181 ms (1.07), 3.07 MB |
| 10000 | Hand-written dictionary diff (baseline) | 87.142 ms (1.00), 29.45 MB | 78.564 ms (1.00), 29.06 MB | 70.924 ms (1.00), 28.53 MB | 69.880 ms (1.00), 28.49 MB |
| 10000 | CSE `MigrateDataAsync` | 88.815 ms (1.02), 30.9 MB | 82.540 ms (1.05), 30.48 MB | 73.432 ms (1.04), 29.93 MB | 72.614 ms (1.04), 29.89 MB |

CSE is slower on every cell, by 2% to 7%: 1.02 to 1.07 times the dictionary diff at 1,000 rows and 1.02 to 1.05 times at 10,000. `MigrateDataAsync` diffs keys through hash sets (`Extensions/DbContextExtensionMethods.cs:153-168`), so the gap does not grow with the table. It also allocates 4% to 5% more.

### DbContextLifetime

| Group | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| CreateDispose | `new` (baseline) | 53,390.5 ns (1.000), 43969 B | 53,390.0 ns (1.000), 43249 B | 50,928.0 ns (1.001), 43784 B | 42,676.1 ns (1.000), 43529 B |
| CreateDispose | `AddDbContext` scope | 68,526.6 ns (1.284), 58011 B | 71,164.4 ns (1.333), 62164 B | 64,698.5 ns (1.271), 60029 B | 58,869.6 ns (1.380), 60075 B |
| CreateDispose | Pooled factory (labelled) | 286.6 ns (0.005), 152 B | 245.7 ns (0.005), 112 B | 245.1 ns (0.005), 112 B | 192.2 ns (0.005), 72 B |
| CreateDispose | CSE `BaseDbContext` | 70,903.3 ns (1.328), 58178 B | 74,459.5 ns (1.395), 62331 B | 69,587.6 ns (1.367), 60195 B | 59,028.3 ns (1.383), 60245 B |
| CreateDispose | CSE `BaseDbContext` + interceptors | 76,859.1 ns (1.440), 64316 B | 82,290.8 ns (1.542), 69711 B | 74,789.2 ns (1.470), 66535 B | 65,919.2 ns (1.545), 66583 B |
| CreateQueryDispose | `new` (baseline) | 100,673.9 ns (1.00), 48393 B | 104,089.2 ns (1.00), 48274 B | 95,389.9 ns (1.00), 48817 B | 86,530.2 ns (1.00), 48523 B |
| CreateQueryDispose | `AddDbContext` scope | 118,032.4 ns (1.17), 62436 B | 125,849.5 ns (1.21), 67191 B | 112,085.0 ns (1.18), 65062 B | 103,705.1 ns (1.20), 65070 B |
| CreateQueryDispose | Pooled factory (labelled) | 14,790.0 ns (0.15), 3984 B | 17,132.5 ns (0.16), 4544 B | 14,692.4 ns (0.15), 4504 B | 13,518.7 ns (0.16), 4464 B |
| CreateQueryDispose | CSE `BaseDbContext` | 118,058.1 ns (1.17), 62605 B | 122,202.2 ns (1.17), 67359 B | 112,902.6 ns (1.18), 65230 B | 103,408.8 ns (1.20), 65236 B |
| CreateQueryDispose | CSE `BaseDbContext` + interceptors | 129,211.5 ns (1.28), 69846 B | 131,359.0 ns (1.26), 75807 B | 124,157.0 ns (1.30), 72669 B | 112,195.7 ns (1.30), 72680 B |

`BaseDbContext` is slower than an `AddDbContext` scope on CreateDispose on every TFM (for example net10.0: 69,587.6 ns against 64,698.5 ns) and allocates 166 to 170 B more per context. With one query the two are within 1% on net8.0 and net10.0, and `BaseDbContext` is faster on net9.0 and net11.0. The pooled factory is far ahead of every non-pooled row; `BaseDbContext` cannot be pooled (see [Semantic differences](#semantic-differences)).

### DbErrorTranslation

EntityFrameworkCore.Exceptions rows exist on net10.0 and net11.0 only.

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `catch` on `SqliteException` 2067 (baseline) | 234.9 μs (1.00), 26.16 KB | 122.3 μs (1.00), 22.12 KB | 88.17 μs (1.00), 21.84 KB | 71.29 μs (1.00), 21.81 KB |
| EntityFrameworkCore.Exceptions `UseExceptionProcessor` (labelled) | n/a | n/a | 92.39 μs (1.05), 21.49 KB | 77.41 μs (1.09), 21.51 KB |
| CSE `DbErrorTranslation.SaveChangesAsync` (labelled) | 233.7 μs (1.00), 27.89 KB | 124.2 μs (1.02), 22.33 KB | 85.75 μs (0.97), 22.05 KB | 70.90 μs (0.99), 22.02 KB |

CSE is faster than EntityFrameworkCore.Exceptions on net10.0 and net11.0, and within 2% of the plain `catch` on every TFM (slower on net9.0). CSE allocates the most of the three rows on every TFM where they run, for example 22.05 KB against 21.49 KB and 21.84 KB on net10.0.

### DbErrorClassification

No database. The two hand-written baselines take under 1.4 ns, so the ratio column is large; read the absolute times.

| Group | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| SqlState | Pattern match on 23505 (baseline) | 0.9317 ns (1.00), - | 0.9612 ns (1.00), - | 0.4459 ns (1.00), - | 0.4166 ns (1.00), - |
| SqlState | CSE `SqlStateErrorTranslator` (labelled) | 116.8037 ns (125.38), 288 B | 81.4948 ns (84.82), 288 B | 49.3231 ns (110.64), 248 B | 45.9486 ns (110.34), 248 B |
| Sqlite | Pattern match (baseline) | 1.3259 ns (1.00), - | 0.6815 ns (1.00), - | 0.7026 ns (1.00), - | 0.6730 ns (1.00), - |
| Sqlite | DbExceptionClassifier `IsUniqueConstraintError` | n/a | n/a | 0.9238 ns (1.32), - | 0.9571 ns (1.42), - |
| Sqlite | CSE `DbErrorTranslation.TryTranslate` | 12.1964 ns (9.20), - | 3.6440 ns (5.35), - | 2.5700 ns (3.66), - | 2.5418 ns (3.78), - |

CSE is the slowest row in both groups on every TFM. In the SqlState group the CSE translator also builds `ErrorMetadata` and an `Error` (`DbErrors/SqlStateErrorTranslator.cs:42-55`) and allocates 248 to 288 B; the pattern match only returns a bool. In the Sqlite group CSE takes 2.5418 to 12.1964 ns against 0.9238 and 0.9571 ns for DbExceptionClassifier, with no allocation in either.

### Where CSE loses

Every row below is in the tables above.

- Audit and domain events: slower than the hand-written code in 30 of 32 CSE cells at Count 1, 21 of 32 at Count 100 and 20 of 32 at Count 1,000. CSE allocates more than the hand-written rows in every SaveChanges cell but one (net10.0, Count 1,000, 0 events, interceptor: 11379.94 KB against 11379.99 KB).
- Keyset After page: CSE cached is slower than the hand-written row on net8.0 (140.99 μs against 138.85 μs), net9.0 (131.77 μs against 127.62 μs) and net10.0 (124.43 μs against 119.54 μs), and allocates more on every TFM.
- Keyset `Func` ordering: slower than the hand-written row on every page and TFM (1.13 to 1.26), and on the First page slower than MR inline on net10.0 (87.31 μs against 78.66 μs) and net11.0 (82.95 μs against 71.51 μs), and than MR prebuilt with `Take(21)`.
- Offset paging: slower than X.PagedList on net9.0 page 1, net10.0 page 1 and net10.0 page 250, by 0.1 to 1.36 μs; slower than plain EF Core in 6 of 8 cells and allocates more in all 8.
- Enum writes, direct class: the direct `EnumToFormattedStringConverter` class writes slower than EF Core on net10.0 and net11.0 (1.18 and 1.12).
- `MigrateDataAsync`: 1.02 to 1.07 times the dictionary diff (net9.0, 10,000 rows: 82.540 ms against 78.564 ms).
- `BaseDbContext` creation: slower than an `AddDbContext` scope on every TFM and 166 to 170 B larger.
- Error translation: CSE allocates more than EntityFrameworkCore.Exceptions and the plain `catch`; it is slower than the plain `catch` on net9.0 (124.2 μs against 122.3 μs).
- Error classification: CSE is the slowest row in both groups (for example net10.0 Sqlite: 2.5700 ns against 0.9238 ns for DbExceptionClassifier).

---

## Caveats

- In-memory SQLite, not a server: there is no network and no server-side planning cost, so the database part of each row is small and the library overhead is a larger share than it would be against a server database such as PostgreSQL. The SQLSTATE path is measured only in the CPU-only class; an end-to-end PostgreSQL run is out of scope.
- No standalone cursor codec row: CSE's `KeysetCursorCodec` (`Pagination/Keyset/KeysetCursorCodec.cs:12`) and `DomainEventCollector` (`Interceptors/DomainEvents/DomainEventCollector.cs:13`) are internal, and the benchmarks do not change package source to reach them. `After` minus `First` on the CSE keyset rows is the only view of the cursor decode and predicate cost.
- No Native AOT sample: EF Core builds its model and translates LINQ at run time, MR builds expression trees and reads the reference object by reflection, and EntityFrameworkCore.Exceptions is an EF Core interceptor, so none of these rows is AOT-ready.
- CSE costs seen in the code, with the rows that show them:
  - `Interceptors/Audit/AuditInterceptor.cs:56` copies `ChangeTracker.Entries()`, which runs change detection; `:83-87` creates a DI scope and resolves `IAuditUserIdProvider` once per save that has an entry to stamp. The Count 1 SaveChangesAudit rows show this as a few microseconds over the hand-written override.
  - `Interceptors/DomainEvents/DomainEventCollector.cs:26` calls `ChangeTracker.Entries<IDomainEventHolder>()`, and `:29` copies each tracked entity's events into an array once per save.
  - `Abstractions/BaseDbContext.cs:23-26`: a DI scope, a logger lookup and an Information log call per instance. The finalizer (`:215-219`) is suppressed in `Dispose` and `DisposeAsync` (`:226`, `:237`). The CreateDispose rows show the per-instance cost against an `AddDbContext` scope.
  - `Pagination/KeysetPaginationExtensions.cs:41`: the `Func` overload, the form the Readme shows, builds a new `KeysetOrdering` on every call, including the SHA-256 key fingerprint (`Pagination/Keyset/KeysetOrdering.cs:56-63`) and the key validation. Compare the `Func` and cached keyset rows.
  - `Pagination/Keyset/KeysetCursorCodec.cs:88`: the `Invalid` error is built before the cursor is checked, so a successful decode creates it too.
  - `Extensions/DbContextExtensionMethods.cs:153-168`: the key diff runs through two hash sets and a `Join`; the MigrateData rows show the 2% to 7% that is left of the gap. The key selectors are compiled once per options instance (`Extensions/MigrateDataOptions.cs:19-21`).
  - `Converters/EnumToStringConverter.cs:35-37`: the legacy fallback allocates `Enum.GetValues` and a `ToString().ToSnakeCase()` per member on every row it reads. It runs only when the enum's metadata has no 3.x alias (an enum without `[StringEnum]` generated metadata); a `[StringEnum]` enum, as in this suite, never reaches it. `Converters/LegacySnakeCaseEnumConverter.cs:22-27` reads and writes through cached dictionaries, and its rows allocate nothing.
- The CSE column is repository source, not a NuGet release. The first run used branch `integ/all`, a local integration of the open pull requests; the EnumConverter and MigrateData rerun used the code after #230 to #256.

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
