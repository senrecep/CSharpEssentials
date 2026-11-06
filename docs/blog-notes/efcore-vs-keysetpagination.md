# Blog notes: CSharpEssentials.EntityFrameworkCore and Entity against EF Core, MR.EntityFrameworkCore.KeysetPagination, X.PagedList.EF and EntityFrameworkCore.Exceptions (draft)

These are notes for a post, not the post. The numbers will come from `CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md` and `CSharpEssentials.Entity/BENCHMARKS.md` once the full default-job run is committed under `benchmarks/results/efcore/{tfm}/results/`. There are no numbers yet.

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

## The numbers that matter

Pending the full default-job run. These are the rows to pull:

1. SaveChangesAudit at 1,000 Added: the interceptor and `BaseDbContext` against the hand-written override.
2. SaveChangesDomainEvents at 1,000 with 0 events: the per-entity read cost when nothing is raised.
3. KeysetPagination After: CSE cached against MR prebuilt with `limit + 1`, and CSE `Func` against CSE cached.
4. OffsetPagination page 250: CSE against X.PagedList and plain EF Core.
5. MigrateData at 10,000: CSE against the dictionary diff.
6. DbContextLifetime CreateDispose: `BaseDbContext` against `AddDbContext` and pooling, with the Gen columns.
7. DbErrorTranslation: CSE's `Result` against a second typed exception and a plain catch.

## Where CSE is expected to lose (state it, do not soften it)

These predictions come from reading the code. Nothing has been measured yet.

- The `Func` ordering rebuilds the keys on every call (`CSharpEssentials.EntityFrameworkCore/Pagination/KeysetPaginationExtensions.cs:41`), including the SHA-256 key fingerprint (`Pagination/Keyset/KeysetOrdering.cs:56-63`). The Readme shows this form. The cached-ordering row shows the cost, and MR's inline row is the fair pair for it.
- `DomainEvents` allocates on every read (`CSharpEssentials.Entity/EntityBase.cs:33`), even when empty, and the collector copies it again per tracked entity (`Interceptors/DomainEvents/DomainEventCollector.cs:49`).
- `BaseDbContext` has a per-instance cost: a GUID, a DI scope, a log call and a finalizer with no `GC.SuppressFinalize` (`Abstractions/BaseDbContext.cs:16`, `:24-27`, `:172-182`). It also cannot be pooled.
- `MigrateDataAsync` at 10,000 rows matches with nested loops (`Extensions/DbContextExtensionMethods.cs:153-155`, `:165-167`) and compiles both key selectors on every call (`:147-148`).
- SQLite errors need your own translator in CSE; EntityFrameworkCore.Exceptions detects them out of the box.
- `AuditInterceptor` creates a DI scope on every save and resolves `IAuditUserIdProvider`, even when nothing is auditable (`Interceptors/Audit/AuditInterceptor.cs:42`). The hand-written `SaveChangesAsync` override reads a user it captured, so it is expected to win on the SaveChangesAudit rows.
- On the keyset After row, CSE cached is expected to be slower than the hand-written row and MR prebuilt with `limit + 1`. CSE validates the cursor (version, direction, key fingerprint), issues the previous cursor too, and builds the `Invalid` error even when the cursor decodes (`Pagination/Keyset/KeysetCursorCodec.cs:88`).

## Where CSE may win or tie

To be confirmed by the run:
- Keyset paging with the cached ordering against MR with `HasNextAsync`, which sends a second query per page.
- Offset paging, where all rows send the same two queries.
- Error translation without a second exception, against `UseExceptionProcessor`.

Features only CSE ships (not a speed claim):
- Validated opaque cursors with a pluggable protector.
- Audit fields, domain event dispatch with an outbox, seed sync and wire-name enum storage in one package.

## Caveats to keep in the post

- In-memory SQLite: no network, no server-side planning. The SQLSTATE path is measured only without a database.
- CSE's cursor codec and event collector are internal, so they have no standalone rows; the keyset `After` minus `First` difference is the public view of the cursor cost.
- MR and EntityFrameworkCore.Exceptions ship net10.0 only; X.PagedList.EF needs EF Core 9 or later, so it has no net8.0 row. `X.PagedList.EntityFramework` targets classic EF6 and is not compared.
- net11.0 runs EF Core 10, because EF Core 11 is still a release candidate.
- The CSE column is a working tree, not a NuGet release.

## Possible angles

- "One query or two": MR's `HasNextAsync` against `limit + 1`, and what a cursor format costs.
- "The price of a convenient overload": the inline `Func` ordering against a cached one.
- "Exceptions or results": typed exceptions against `Result<int>` for a duplicate key.
- "What a base class costs": `BaseDbContext` per instance against `AddDbContext` and pooling.
- "Reading an empty list allocates": `EntityBase.DomainEvents` at 0 events, as a follow-up fix.
