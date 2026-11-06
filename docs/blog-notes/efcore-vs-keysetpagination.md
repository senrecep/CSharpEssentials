# Blog notes: CSharpEssentials.EntityFrameworkCore and Entity against EF Core, MR.EntityFrameworkCore.KeysetPagination, X.PagedList.EF and EntityFrameworkCore.Exceptions

These are notes for a post, not the post. The full tables are in [CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md](../../CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md) and [CSharpEssentials.Entity/BENCHMARKS.md](../../CSharpEssentials.Entity/BENCHMARKS.md), and the raw reports in [`benchmarks/results/efcore/{tfm}/results/`](../../benchmarks/results/efcore/).

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0, on in-memory SQLite. The libraries compared are EF Core 8.0.33, 9.0.22 and 10.0.14 (one major per TFM; net11.0 runs EF Core 10), MR.EntityFrameworkCore.KeysetPagination 1.6.0 (MIT, net10.0 only), X.PagedList.EF 10.6.0 (MIT, net9.0 and later in this suite) and EntityFrameworkCore.Exceptions.Sqlite 10.0.1 with DbExceptionClassifier.Sqlite 1.0.1 (Apache-2.0, net10.0 only).

## Framing

- Working title: "What do EF Core add-ons cost per save and per page?"
- Most rows are overheads on top of EF Core: the plain EF Core or hand-written row is the baseline, so a ratio reads as "the cost of adding X". The post should say that first.
- CSE bundles features that the other libraries each cover one of: keyset paging (MR), offset paging (X.PagedList), typed database errors (EntityFrameworkCore.Exceptions). Audit fields, domain events, enum storage, seed sync and `BaseDbContext` have no package competitor, so they are compared with the code people write by hand. The post compares feature by feature, never "CSE against all of them".
- In-memory SQLite makes the database part small, so library overhead is a larger share than on a real server. Say this next to every table.

## What each library provides

- EF Core: `SaveChanges`, interceptors, `EnumToStringConverter`, `Skip`/`Take` and `CountAsync`, context pooling. Keyset paging, audit fields and domain events are documented patterns you write yourself.
- MR.EntityFrameworkCore.KeysetPagination 1.6.0: `KeysetPaginate` with a prebuilt (`KeysetQuery.Build`) or inline key definition, a reference object for the position, `EnsureCorrectOrder` for backward pages, and `HasNextAsync`/`HasPreviousAsync`, which each run a query. No cursor format; the caller serializes the reference.
- X.PagedList.EF 10.6.0: `ToPagedListAsync(pageNumber, pageSize)`, which runs a count and a Skip/Take query and returns an `IPagedList<T>` with page metadata.
- EntityFrameworkCore.Exceptions.Sqlite 10.0.1: `UseExceptionProcessor()` turns provider errors into typed exceptions (`UniqueConstraintException` and others). DbExceptionClassifier, shipped with it, classifies a `DbException` without EF Core.
- CSE: `AuditInterceptor`, `DomainEventInterceptor` and `BaseDbContext` dispatch, `KeysetPaginateAsync` with validated opaque cursors, `PaginateAsync` with a page-size cap, wire-name enum converters, `MigrateDataAsync`, and `DbErrorTranslation` from SQLSTATE plus your own translators. CSE.Entity provides `EntityBase` with audit fields and a domain event list.

## Feature matrix

| Feature | CSE | MR 1.6.0 | X.PagedList.EF 10.6.0 | EF.Exceptions 10.0.1 | EF Core alone |
|---|---|---|---|---|---|
| Keyset paging, composite key, mixed directions | Yes | Yes | No | No | By hand |
| Opaque, validated cursor | Yes | No (reference object) | No | No | No |
| Has-next/has-previous without an extra query | Yes (`limit + 1`) | No | n/a | n/a | By hand |
| Offset paging with total count | Yes | No | Yes | No | By hand |
| Page-size cap | Yes (100 by default) | n/a | No (throws below page 1) | n/a | No |
| Typed database errors | `Error`/`Result` through SQLSTATE and translators | No | No | Typed exceptions per provider | No |
| SQLite unique violation out of the box | No (needs a translator) | n/a | n/a | Yes | No |
| Audit fields and soft delete on save | Yes | No | No | No | By hand |
| Domain events around the save, outbox | Yes | No | No | No | By hand |
| Enum stored as a wire name with check constraint | Yes | No | No | No | Member name only |
| Seed data sync | Yes | No | No | No | By hand |
| Works with context pooling | `BaseDbContext` no, interceptors yes | n/a | n/a | Yes | Yes |
| Target frameworks | net8.0-net10.0 | net10.0 | net6.0, net8.0 (EF Core 9+), net10.0 | net10.0 | per major |

## Methodology

- One BenchmarkDotNet project, `benchmarks/CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks/`, with ten classes, each with `[MemoryDiagnoser]`.
- One in-memory SQLite connection per class, open for the whole run. Every row that writes runs in a transaction that is rolled back, so tables do not grow between invocations and every row pays the same transaction.
- Entities and events are created inside the measured method, the same way for every row.
- Every class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks on every TFM without BenchmarkDotNet: written row counts, stamped users, published event counts, page ids and flags, the second page reached through each row's own cursor, a checksum of the synced table, the detected error.
- Rows that do different work are labelled, not presented as like-for-like: the plain EF Core reference rows, the interceptor that dispatches before the write, MR's `HasNextAsync` extra query, the hand-written cursor with no checks, the pooled context, the stored enum spelling, EntityFrameworkCore.Exceptions' second exception and CSE's user-written SQLite translator.
- Every competitor and EF Core row follows its documented usage and carries a link to that documentation.
- Each TFM runs the EF Core major CSE compiles that TFM against. MR and EntityFrameworkCore.Exceptions rows exist on net10.0 and net11.0 only, X.PagedList.EF rows on net9.0 and later.

## How these numbers were produced

BenchmarkDotNet v0.14.0, default job, full runs, one suite at a time, on a 6-core, 12-thread desktop CPU under Linux (exact model in each report header). TFMs net8.0, net9.0, net10.0 and net11.0. The 1-minute load average stayed between 1.99 and 2.60 during the suite and its reruns; `DbErrorClassificationComparison` was rerun on net10.0 and net11.0, and afterwards no row has a CV above 5%. Each number is `Mean (Ratio), Allocated` copied from the `-report-github.md` files. Compare within one TFM only: net8.0 and net9.0 run a different EF Core major from net10.0 and net11.0.

## The numbers that matter

1. **SaveChangesAudit, 1,000 Added, net10.0:** hand-written override 27,625.17 μs (1.07), 11403.25 KB; CSE `AuditInterceptor` 28,275.79 μs (1.09), 11411.65 KB; CSE `BaseDbContext` 27,447.65 μs (1.06), 11411.7 KB. Plain EF Core, which stamps nothing, is 25,902.24 μs. At Count 1 CSE is behind the override on 15 of 16 cells (net10.0 Added: 69.23 μs override, 70.90 μs and 71.32 μs CSE).
2. **SaveChangesDomainEvents, 1,000 with 0 events, net10.0:** EF Core 25,967.90 μs (1.00), hand-written 27,640.53 μs (1.06), CSE `BaseDbContext` 28,250.70 μs (1.09), CSE interceptor 26,537.53 μs (1.02). Allocation is 11379.94 to 11380.21 KB for the three dispatching rows.
3. **KeysetPagination After, net10.0:** hand-written 119.54 μs (1.00), 23.15 KB; CSE cached 124.43 μs (1.04), 25.33 KB; MR prebuilt with `Take(21)` 145.81 μs (1.22), 30.38 KB; MR prebuilt with `HasNextAsync` 233.43 μs (1.95), 48.59 KB. CSE `Func` 137.49 μs (1.15) against CSE cached 124.43 μs. On net11.0 CSE cached is 112.84 μs against 113.27 μs hand-written.
4. **OffsetPagination page 250, net10.0:** EF Core 132.06 μs (1.00), 23.59 KB; CSE 136.71 μs (1.04), 24.24 KB; X.PagedList 136.60 μs (1.03), 24 KB. All three send the same two queries.
5. **MigrateData 10,000:** CSE 835.763 ms (10.97) against 76.195 ms for the dictionary diff on net10.0; 1,852.582 ms (21.12) against 87.743 ms on net8.0.
6. **DbContextLifetime CreateDispose, net10.0:** `AddDbContext` scope 64,698.5 ns, 60029 B; `BaseDbContext` 69,587.6 ns, 60195 B; with interceptors 74,789.2 ns, 66535 B; pooled factory 245.1 ns, 112 B. Gen1 per 1,000 operations is 0.2441 for the scope and both `BaseDbContext` rows.
7. **DbErrorTranslation, net10.0:** plain `catch` 88.17 μs (1.00), 21.84 KB; EntityFrameworkCore.Exceptions 92.39 μs (1.05), 21.49 KB; CSE `Result` 85.75 μs (0.97), 22.05 KB. net11.0: 71.29 μs, 77.41 μs and 70.90 μs.
8. **`EntityBase.DomainEvents` read, net10.0:** 0 events 2.256 ns, 1 event 4.163 ns, 10 events 21.540 ns, no allocation; the hand-written cached view is 1.519 ns, 3.585 ns and 20.517 ns.

## Where CSE loses (state it, do not soften it)

- **Count 1 saves.** Audit and domain event dispatch are slower than the hand-written code in 30 of 32 CSE cells, and CSE allocates more than the hand-written rows in every SaveChanges cell but one (net10.0, Count 1,000, 0 events, interceptor: 11379.94 KB against 11379.99 KB). At Count 100 and 1,000 CSE is slower in 21 and 20 of 32 cells. The audit path copies `ChangeTracker.Entries()` and creates a DI scope per save that has entries to stamp (`Interceptors/Audit/AuditInterceptor.cs:56`, `:83-87`).
- **The `Func` ordering.** The form the Readme shows rebuilds the keys and the SHA-256 fingerprint on every call (`CSharpEssentials.EntityFrameworkCore/Pagination/KeysetPaginationExtensions.cs:41`, `Pagination/Keyset/KeysetOrdering.cs:56-63`). It is 13.06 to 26.43 μs slower than the cached ordering, and on the First page it loses to MR's inline form: 87.31 μs against 78.66 μs on net10.0, 82.95 μs against 71.51 μs on net11.0.
- **Keyset After against hand-written code.** CSE cached is 1.02 to 1.04 times the hand-written row on net8.0, net9.0 and net10.0, and allocates 1.48 to 2.18 KB more on every TFM. It validates the cursor, issues a previous cursor and builds the `Invalid` error even when the cursor decodes (`Pagination/Keyset/KeysetCursorCodec.cs:88`).
- **`MigrateDataAsync`.** 10.83 to 21.12 times the dictionary diff at 10,000 rows, from nested `All` matching (`Extensions/DbContextExtensionMethods.cs:153-155`, `:165-167`).
- **`BaseDbContext` creation.** Slower than an `AddDbContext` scope on every TFM (net10.0: 69,587.6 ns against 64,698.5 ns), 166 to 170 B larger, and it cannot be pooled.
- **Enum reads.** The `ConfigureEnumConventions` converter reads slower than EF Core's `EnumToStringConverter` on every TFM; on net10.0 it is 142.34 ns (1.90) against 74.79 ns. That net10.0 number is far from the other TFMs (81.07 ns on net11.0) and is not explained yet.
- **Error classification.** CSE is the slowest classifier: net10.0 Sqlite 2.5700 ns against 0.9238 ns for DbExceptionClassifier, and the SqlState translator 49.3231 ns with 248 B against a 0.4459 ns pattern match.
- **SQLite errors** need your own translator in CSE; EntityFrameworkCore.Exceptions detects them out of the box.
- **Offset paging.** Slower than X.PagedList on net9.0 page 1, net10.0 page 1 and net10.0 page 250, by 0.1 to 1.36 μs.

## Where CSE wins or ties

- Keyset paging with the cached ordering against MR: 124.43 μs against 145.81 μs (MR with `Take(21)`) and 233.43 μs (MR with `HasNextAsync`) on the After page on net10.0. On the First page CSE and MR prebuilt are 72.12 μs and 72.24 μs.
- Offset paging: CSE, X.PagedList and plain EF Core are within 4% of each other on net9.0, net10.0 and net11.0. On net8.0, where X.PagedList has no row, CSE is 1.06 times EF Core on page 1 (110.2 μs against 104.2 μs).
- Error translation without a second exception: CSE 85.75 μs against 92.39 μs for `UseExceptionProcessor` on net10.0, and 70.90 μs against 77.41 μs on net11.0.
- Enum writes: every Readme-path CSE writer is faster than EF Core and allocates nothing, against 120 B for EF Core.
- `EntityBase.DomainEvents` no longer allocates on read (`CSharpEssentials.Entity/EntityBase.cs:40`).

Features only CSE ships (not a speed claim):
- Validated opaque cursors with a pluggable protector.
- Audit fields, domain event dispatch with an outbox, seed sync and wire-name enum storage in one package.

## Caveats to keep in the post

- In-memory SQLite: no network, no server-side planning. The SQLSTATE path is measured only without a database.
- CSE's cursor codec and event collector are internal, so they have no standalone rows; the keyset `After` minus `First` difference is the public view of the cursor cost.
- MR and EntityFrameworkCore.Exceptions ship net10.0 only; X.PagedList.EF needs EF Core 9 or later, so it has no net8.0 row. `X.PagedList.EntityFramework` targets classic EF6 and is not compared.
- net11.0 runs EF Core 10, because EF Core 11 is still a release candidate.
- The CSE column is the working tree on branch `integ/all`, a local integration of the open pull requests, not a NuGet release.

## Possible angles

- "One query or two": MR's `HasNextAsync` against `limit + 1`, and what a cursor format costs.
- "The price of a convenient overload": the inline `Func` ordering against a cached one.
- "Exceptions or results": typed exceptions against `Result<int>` for a duplicate key.
- "What a base class costs": `BaseDbContext` per instance against `AddDbContext` and pooling.
- "Quadratic seed sync": `MigrateDataAsync` at 1,000 and 10,000 rows against a dictionary diff, as a follow-up fix.
