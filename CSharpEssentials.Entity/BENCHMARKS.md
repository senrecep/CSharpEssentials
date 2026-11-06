# CSharpEssentials.Entity: Performance benchmarks

This page measures the domain event list of CSharpEssentials.Entity (CSE) `EntityBase` against a hand-written list, without a database, and points to the SaveChanges rows where the same list is read during a save. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. There is no third-party row: the comparison is against a hand-written list.

The classes live in `benchmarks/CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks/` (suite key `efcore`), next to the EF Core classes documented in [CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md](../CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md).

The numbers on this page come from full default-job runs on net8.0, net9.0, net10.0 and net11.0 (see [Results](#results) and [How these numbers were produced](#how-these-numbers-were-produced)). The parity self-check passes on every TFM.

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

- **Snapshot or live view.** CSE's `DomainEvents` returns one cached read-only view of the list (`EntityBase.cs:40`): it copies nothing, and later raises and clears show through it. The hand-written cached view does the same work, so it is the like-for-like row. The hand-written snapshot copies to an array on every read.
- **Collect1k is a stand-in.** It repeats, without EF Core, what `DomainEventCollector.CollectByEntity` does per tracked entity during a save (`[.. entity.DomainEvents]`, then clear; `CSharpEssentials.EntityFrameworkCore/Interceptors/DomainEvents/DomainEventCollector.cs:29`). The real collector also enumerates the change tracker.

---

## Feature matrix

This table comes from reading CSE source. None of it was measured.

| Feature | CSE `EntityBase` | Hand-written list |
|---|---|---|
| Raise, clear, read | Yes | Yes |
| Read returns a snapshot | No (cached live read-only view, `EntityBase.cs:40`) | As written |
| Per-event timing (`[DomainEventTiming]` BeforeSave/AfterSave) | Yes, honoured by the EF Core package | By hand |
| Audit fields on the same base type | Yes | By hand |
| Collected by CSE's interceptor and `BaseDbContext` | Yes (`IDomainEventHolder`) | Only if it implements `IDomainEventHolder` |
| Trim/AOT | Clean | Clean |
| License | MIT | n/a |

---

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job, full runs (no `--quick`), one suite at a time, on a 6-core, 12-thread desktop CPU under Linux. The exact CPU, SDK and runtime builds are in the header of each report.
- TFMs: net8.0 (.NET 8.0.30, CSE.Entity netstandard2.1 build), net9.0 (.NET 9.0.19), net10.0 (.NET 10.0.11) and net11.0 (.NET 11.0.0).
- The 1-minute load average stayed between 1.99 and 2.60 during the `efcore` suite and its reruns. No row of this class has a CV above 5%.
- Reports: [`benchmarks/results/efcore/{tfm}/results/`](../benchmarks/results/efcore/), file `CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks.DomainEventsReadComparison-report-github.md`. Every number below is copied from those files.

Each cell is `Mean (Ratio), Allocated` for that TFM; Ratio is against the baseline row of the same group and event count, and `-` means no allocation. Compare rows within one TFM column only: each column is a different runtime.

### DomainEventsRead

| Group | Events | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Read | 0 | Hand-written array snapshot (baseline) | 4.054 ns (1.00), - | 3.375 ns (1.00), - | 1.689 ns (1.00), - | 1.687 ns (1.00), - |
| Read | 0 | Hand-written cached view (labelled) | 2.349 ns (0.58), - | 2.310 ns (0.68), - | 1.519 ns (0.90), - | 1.517 ns (0.90), - |
| Read | 0 | CSE `EntityBase.DomainEvents` | 2.896 ns (0.71), - | 2.889 ns (0.86), - | 2.256 ns (1.34), - | 2.255 ns (1.34), - |
| Read | 1 | Hand-written array snapshot (baseline) | 23.201 ns (1.00), 32 B | 24.048 ns (1.00), 32 B | 14.270 ns (1.00), 32 B | 12.899 ns (1.00), 32 B |
| Read | 1 | Hand-written cached view (labelled) | 7.827 ns (0.34), - | 6.927 ns (0.29), - | 3.585 ns (0.25), - | 3.661 ns (0.28), - |
| Read | 1 | CSE `EntityBase.DomainEvents` | 7.581 ns (0.33), - | 7.073 ns (0.29), - | 4.163 ns (0.29), - | 4.090 ns (0.32), - |
| Read | 10 | Hand-written array snapshot (baseline) | 74.813 ns (1.00), 104 B | 79.716 ns (1.00), 104 B | 26.181 ns (1.00), 104 B | 26.952 ns (1.00), 104 B |
| Read | 10 | Hand-written cached view (labelled) | 41.108 ns (0.55), - | 40.591 ns (0.51), - | 20.517 ns (0.78), - | 20.529 ns (0.76), - |
| Read | 10 | CSE `EntityBase.DomainEvents` | 41.616 ns (0.56), - | 41.033 ns (0.51), - | 21.540 ns (0.82), - | 20.926 ns (0.78), - |
| RaiseClear | 0 | Hand-written list (baseline) | 2.304 ns (1.00), - | 2.299 ns (1.00), - | 2.259 ns (1.00), - | 2.223 ns (1.00), - |
| RaiseClear | 0 | CSE `Raise` + `ClearDomainEvents` | 2.111 ns (0.92), - | 2.040 ns (0.89), - | 2.655 ns (1.18), - | 2.611 ns (1.17), - |
| RaiseClear | 1 | Hand-written list (baseline) | 11.441 ns (1.00), - | 11.612 ns (1.00), - | 11.293 ns (1.00), - | 9.493 ns (1.00), - |
| RaiseClear | 1 | CSE `Raise` + `ClearDomainEvents` | 12.430 ns (1.09), - | 12.355 ns (1.06), - | 9.920 ns (0.88), - | 9.359 ns (0.99), - |
| RaiseClear | 10 | Hand-written list (baseline) | 63.929 ns (1.00), - | 67.319 ns (1.00), - | 73.591 ns (1.00), - | 66.329 ns (1.00), - |
| RaiseClear | 10 | CSE `Raise` + `ClearDomainEvents` | 64.758 ns (1.01), - | 67.266 ns (1.00), - | 63.274 ns (0.86), - | 67.114 ns (1.01), - |
| Collect1k | 0 | Hand-written (baseline) | 10,516.232 ns (1.00), 32 B | 9,879.891 ns (1.00), 32 B | 3,273.520 ns (1.00), 32 B | 3,482.026 ns (1.00), 32 B |
| Collect1k | 0 | CSE, collector style | 8,535.421 ns (0.81), 32 B | 7,243.147 ns (0.73), 32 B | 4,830.394 ns (1.48), 32 B | 4,785.733 ns (1.37), 32 B |
| Collect1k | 1 | Hand-written (baseline) | 45,092.678 ns (1.00), 48600 B | 45,567.657 ns (1.00), 48600 B | 45,216.923 ns (1.00), 48600 B | 40,135.234 ns (1.00), 48600 B |
| Collect1k | 1 | CSE, collector style | 55,502.522 ns (1.23), 48600 B | 54,012.410 ns (1.19), 48600 B | 39,632.000 ns (0.88), 48600 B | 35,256.158 ns (0.88), 48600 B |
| Collect1k | 10 | Hand-written (baseline) | 123,528.515 ns (1.00), 268056 B | 121,972.929 ns (1.00), 268056 B | 114,461.980 ns (1.00), 268056 B | 101,777.675 ns (1.00), 268056 B |
| Collect1k | 10 | CSE, collector style | 137,731.792 ns (1.11), 268056 B | 125,176.648 ns (1.03), 268056 B | 114,706.463 ns (1.00), 268056 B | 100,576.696 ns (0.99), 268056 B |

- **Read.** CSE's read allocates nothing on any TFM, like the hand-written cached view. Against that view, its like-for-like row, CSE is slower in 11 of 12 cells, by 0.15 to 1.02 ns; the exception is net8.0 with one event (7.581 ns against 7.827 ns). Against the copying snapshot, CSE is faster except with 0 events on net10.0 and net11.0 (2.256 ns and 2.255 ns against 1.689 ns and 1.687 ns), where the snapshot returns an empty array without copying.
- **RaiseClear.** CSE is slower than the hand-written list in 6 of 12 cells: net8.0 with 1 and 10 events, net9.0 with 1, net10.0 with 0, net11.0 with 0 and 10. The largest gap is net8.0 with 1 event (12.430 ns against 11.441 ns).
- **Collect1k.** CSE is slower in 7 of 12 cells: net8.0 and net9.0 with 1 and 10 events (up to 1.23 times), net10.0 and net11.0 with 0 events (1.48 and 1.37 times), and net10.0 with 10 events (114,706.463 ns against 114,461.980 ns, ratio 1.00). Allocation is the same in every pair.

The SaveChanges rows that read the same list during a save are on the [EF Core page](../CSharpEssentials.EntityFrameworkCore/BENCHMARKS.md#savechangesdomainevents).

---

## Caveats

- **CSE costs seen in the code:**
  - `EntityBase.cs:40`: `DomainEvents` returns a read-only view created once per entity and cached in `_domainEventsView` (`:12`); the Read rows show no allocation.
  - `CSharpEssentials.EntityFrameworkCore/Interceptors/DomainEvents/DomainEventCollector.cs:29`: the collector copies each entity's events into an array once per tracked entity per save.
- The CSE column is the working tree on branch `integ/all`, a local integration of the open pull requests, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
dotnet run -c Release -f net10.0 --project CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks -- --filter "*.DomainEventsReadComparison.*"
dotnet run -c Release -f net10.0 --project CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks -- --self-check
```

Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/efcore/{tfm}/results/`.
