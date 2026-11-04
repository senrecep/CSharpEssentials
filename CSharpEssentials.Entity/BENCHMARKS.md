# CSharpEssentials.Entity: Performance benchmarks (draft)

This page measures the domain event list of CSharpEssentials.Entity (CSE) `EntityBase` against a hand-written list, without a database, and points to the SaveChanges rows where the same list is read during a save. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. There is no third-party row: the comparison is against a hand-written list.

The classes live in `benchmarks/CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks/` (suite key `efcore`), next to the EF Core classes documented in [CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md](../CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

net9.0, net10.0 and net11.0 run CSE.Entity's own build for that TFM; net8.0 runs its netstandard2.1 build, because the package has no net8.0 target. Nothing is pinned for this page beyond BenchmarkDotNet.

---

## What was measured

- Every class has `[MemoryDiagnoser]`.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check for every parameter value. A mismatch fails the run.
- Each hand-written row has a comment above it that links to the documentation it follows; each CSE row links to its Readme section.

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `DomainEventsReadComparison` | `Events` = 0, 1 or 10 events on one entity | Read: hand-written array snapshot (baseline), hand-written cached read-only view (labelled), CSE `EntityBase.DomainEvents`. RaiseClear: hand-written list (baseline), CSE `Raise` + `ClearDomainEvents`. Collect1k: 1,000 entities raise and are collected and cleared, hand-written (baseline) and CSE the way `DomainEventCollector` does it | each pair reads and collects the same event instances in the same order; the lists are empty after a clear |
| 2 | `SaveChangesDomainEventsComparison` (EF Core page) | SaveChanges of 1, 100 or 1,000 new orders raising 0 or 1 event each | see the EF Core page | see the EF Core page |

With `EventsPerEntity = 0`, the SaveChangesDomainEvents rows minus the plain EF Core row show the per-entity read cost during a save: every tracked entity's `DomainEvents` is read even when it holds nothing.

### Inputs

- `AuditedOrder : EntityBase<int>`, the same entity the EF Core rows save.
- `OrderPlaced(int Ordinal) : IDomainEvent`, built once per parameter value; every row raises the same instances.
- The hand-written holder keeps a `List<IDomainEvent>`. Its snapshot returns `[]` when empty and `ToArray()` otherwise; its view is one cached `AsReadOnly()` wrapper.
- Every Read row walks the list by index, so no enumerator is allocated and the numbers show the read itself.

---

## Semantic differences

- **Snapshot or live view.** CSE's `DomainEvents` and the hand-written snapshot return a copy: raising or clearing later does not change what the caller holds. The cached view is live and copies nothing, so it is not like-for-like; it shows the floor.
- **Collect1k is a stand-in.** It repeats, without EF Core, what `DomainEventCollector.CollectByEntity` does per tracked entity during a save (`[.. entity.DomainEvents]`, then clear). The real collector also enumerates the change tracker.

---

## Feature matrix

This table comes from reading CSE source. None of it was measured.

| Feature | CSE `EntityBase` | Hand-written list |
|---|---|---|
| Raise, clear, read | Yes | Yes |
| Read returns a snapshot | Yes (new list on every read) | As written |
| Per-event timing (`[DomainEventTiming]` BeforeSave/AfterSave) | Yes, honoured by the EF Core package | By hand |
| Audit fields on the same base type | Yes | By hand |
| Collected by CSE's interceptor and `BaseDbContext` | Yes (`IDomainEventHolder`) | Only if it implements `IDomainEventHolder` |
| Trim/AOT | Clean | Clean |
| License | MIT | n/a |

---

## Results

**Pending the full default-job run.** The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/efcore/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **Expected CSE costs, from reading the code** (not yet measured):
  - `EntityBase.cs:33`: `DomainEvents => _domainEvents.ToList().AsReadOnly()` allocates a `List` and a `ReadOnlyCollection` on every read, including when the list is empty.
  - `CSharpEssentials.EntityFrameworkCore/Interceptors/DomainEvents/DomainEventCollector.cs:49`: the collector copies that copy again into an array, once per tracked entity per save.
- The CSE column is the working tree on branch `perf/bench-efcore`, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
dotnet run -c Release -f net10.0 --project CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks -- --filter "*.DomainEventsReadComparison.*"
dotnet run -c Release -f net10.0 --project CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks -- --self-check
```

Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/efcore/{tfm}/results/`.
