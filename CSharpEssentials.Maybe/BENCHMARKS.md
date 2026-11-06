# CSharpEssentials.Maybe: Performance benchmarks

This page compares the CSharpEssentials.Maybe (CSE) collection helpers with CSharpFunctionalExtensions 3.7.0 (CFE) and with hand-written loops. It covers `TryFirst`, `TryLast`, `TryFind` and `Choose`. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The basic `Maybe<T>` operations (From, Map, Bind, Where, GetValueOrDefault, Match) are measured in [CSharpEssentials.Results/BENCHMARKS.md](../CSharpEssentials.Results/BENCHMARKS.md).

The numbers come from full default-job runs on net8.0 to net11.0 (see [How these numbers were produced](#how-these-numbers-were-produced) and [Results](#results)).

| Package | Version | License |
|---|---|---|
| CSharpFunctionalExtensions | 3.7.0, the latest stable release on NuGet as of 2026-11-03, pinned in `Directory.Packages.props` | MIT (nuspec expression) |

ErrorOr and FluentResults have no Maybe type, so they have no column. net8.0 runs CSE's netstandard2.1 build, because Maybe has no net8.0 target.

---

## What was measured

The code is `MaybeCollectionComparison` in `benchmarks/CSharpEssentials.Results.Comparison.Benchmarks/`.
- The class has `[MemoryDiagnoser]`, and CSE is the baseline in each category.
- `[GlobalSetup]` checks that every column gives the same answer, and `--self-check` runs the same check for both `Source` values without BenchmarkDotNet. A mismatch fails the run.
- The source is 1000 strings, `item0000` to `item0999`. The target `item0500` is the only match, so a search from either end scans the same distance.
- `Source` = `List` (a `List<string>`) or `Sequence` (a lazy `IEnumerable<string>` that wraps the same array with a `yield` loop). Both are passed as `IEnumerable<string>`, the parameter type of both libraries.
- Predicates are delegates built once in setup and shared by every column.
- The hand-written column returns `string?` or `int?`, where null means none. On `List` it uses an indexed `for` loop (in reverse for the last-match search). On `Sequence` it uses `foreach`.
- Each library row has a comment above it with the Readme or README URL.
- Both libraries define `TryFirst`, `TryLast`, `TryFind` and `Choose` with the same names on the same receiver types, so with both namespaces imported a normal extension call does not compile (CS0121). The suite calls each library's method as a static method.

| Category | Work | Columns |
|---|---|---|
| `TryFirst` | first element, no predicate | CSE, CFE |
| `TryLast` | last element, no predicate | CSE, CFE |
| `TryFirstWhere` | first element equal to `item0500` | CSE, CFE, hand-written |
| `TryLastWhere` | last element equal to `item0500` | CSE, CFE, hand-written |
| `TryFirstMiss` | first element equal to `absent` (none) | CSE, CFE, hand-written |
| `TryLastMiss` | last element equal to `absent` (none) | CSE, CFE, hand-written |
| `TryFindHit` | `Dictionary<string, int>` with 1000 entries, key `item0500` | CSE, CFE, hand-written `TryGetValue` |
| `TryFindMiss` | the same dictionary, key `absent` | CSE, CFE, hand-written `TryGetValue` |
| `Choose` | 1000 Maybe values, Some at even indexes, sum of the lengths of the chosen strings | CSE, CFE, hand-written loop over `string?[]` |

`TryFirst` and `TryLast` without a predicate have no hand-written row. On a list that row would be one indexer read, which says nothing about the libraries. `TryFind` and `Choose` do not depend on `Source`.

Parity keys: `ok:<value>` or `none` in each search category, `ok:500` and `none` for `TryFind`, and `ok:4000` for `Choose`.

All rows are like-for-like: same input, same delegate, same answer.

---

## Semantic differences

- **Reference-type elements on purpose.** With `int` elements CFE `TryLast` with a predicate returns `Some(0)` instead of none on a miss. Strings avoid that, so every column gives the same answer. CSE `TryFirst` and `TryLast` return none for value types too.
- **How each library walks the source.**
  - CSE `TryLast` reads the last element by index on an `IList<T>` or `IReadOnlyList<T>`, and with a predicate walks the list from the end. On a lazy sequence it walks to the end.
  - CFE `TryFirst` and `TryLast` without a predicate copy a non-collection source to a list first (`source as ICollection<T> ?? source.ToList()`). On `Sequence` that is a full copy.
  - CFE `TryFirst` with a predicate runs `Where().Take(1).ToList()`.
  - CFE `TryLast` with a predicate calls `LastOrDefault(predicate)`, which scans a `List` from the end.
- **`TryFind` does two lookups in both libraries.** Each checks `ContainsKey` and then reads the indexer. The hand-written row does one `TryGetValue`.

---

## Feature matrix

This table comes from reading each library's source and docs. None of it was measured.

| Feature | CSE | CFE 3.7.0 |
|---|---|---|
| Collection helpers | `TryFirst`, `TryLast`, `TryFind`, `Choose`, `Sequence`, `Traverse`, `Partition` | `TryFirst`, `TryLast`, `TryFind`, `Choose` |
| Value-type miss | None (documented in the Readme) | `TryLast(predicate)` gives `Some(default)` |
| `TryLast` on a list | O(1) without a predicate, reverse scan with one | O(1) without a predicate, reverse scan with one |
| `TryFind` | `IReadOnlyDictionary`, two lookups | `IReadOnlyDictionary`, two lookups |
| License | MIT | MIT |

---

## Results

All tables: mean and allocated bytes per operation, net10 unless stated. "Ratio" is the report's Ratio column (row mean divided by CSE mean; above 1.00 = slower than CSE). A `-` in the report's Allocated column is written as `0 B`.

### How these numbers were produced

- Full BenchmarkDotNet default-job runs (no `--quick`) on net8.0 (.NET 8.0.30, CSE netstandard2.1 build), net9.0 (9.0.19), net10.0 (10.0.11) and net11.0 (11.0.0), on a 6-core, 12-thread x64 desktop CPU with CachyOS, SDK 11.0.100-rc.1.26425.128, BenchmarkDotNet v0.14.0.
- 1-minute load average at the start and end of the comparison suite: 1.55 to 2.37, with a desktop session running. No `MaybeCollectionComparison` row stayed above 5% CV.
- Reports: `benchmarks/results/results-comparison/{tfm}/results/`, for example [MaybeCollectionComparison net10](../benchmarks/results/results-comparison/net10.0/results/CSharpEssentials.Results.Comparison.Benchmarks.MaybeCollectionComparison-report-github.md).

### net10

| Category | Source | CSE | CFE (ratio) | Hand-written (ratio) |
|---|---|---|---|---|
| `TryFirst` | List | 1.101 ns / 0 B | 7.664 ns / 0 B (6.96) | - |
| `TryFirst` | Sequence | 1.150 ns / 0 B | 4,360.931 ns / 16600 B (3,791.70) | - |
| `TryLast` | List | 2.995 ns / 0 B | 7.518 ns / 0 B (2.51) | - |
| `TryLast` | Sequence | 733.584 ns / 0 B | 4,335.030 ns / 16600 B (5.91) | - |
| `TryFirstWhere` | List | 1,466.973 ns / 0 B | 2,144.331 ns / 192 B (1.46) | 1,469.969 ns / 0 B (1.00) |
| `TryFirstWhere` | Sequence | 1,669.227 ns / 0 B | 2,707.672 ns / 224 B (1.62) | 1,606.616 ns / 0 B (0.96) |
| `TryLastWhere` | List | 1,460.609 ns / 0 B | 1,604.279 ns / 0 B (1.10) | 1,607.465 ns / 0 B (1.10) |
| `TryLastWhere` | Sequence | 3,495.891 ns / 0 B | 4,392.084 ns / 48 B (1.26) | 3,493.867 ns / 0 B (1.00) |
| `TryFirstMiss` | List | 1,243.765 ns / 0 B | 3,006.975 ns / 160 B (2.42) | 1,457.217 ns / 0 B (1.17) |
| `TryFirstMiss` | Sequence | 1,458.198 ns / 0 B | 3,864.882 ns / 192 B (2.65) | 1,463.751 ns / 0 B (1.00) |
| `TryLastMiss` | List | 1,228.511 ns / 0 B | 1,456.500 ns / 0 B (1.19) | 1,204.289 ns / 0 B (0.98) |
| `TryLastMiss` | Sequence | 1,463.018 ns / 0 B | 1,459.865 ns / 0 B (1.00) | 1,456.570 ns / 0 B (1.00) |
| `TryFindHit` | List | 21.515 ns / 0 B | 21.306 ns / 0 B (0.99) | 11.004 ns / 0 B (0.51) |
| `TryFindMiss` | List | 9.682 ns / 0 B | 9.761 ns / 0 B (1.01) | 9.065 ns / 0 B (0.94) |
| `Choose` | List | 3,467.147 ns / 88 B | 3,429.457 ns / 88 B (0.99) | 449.589 ns / 0 B (0.13) |

`TryFind` and `Choose` do not depend on `Source`; the `Sequence` rows for them (TryFindHit 21.504 ns, TryFindMiss 9.674 ns, Choose 3,408.543 ns for CSE) match the `List` rows.

- On net10 and net11 CSE is faster than CFE or within 1% of it in every search category, and allocates nothing. CFE copies a lazy source to a list for `TryFirst` and `TryLast` without a predicate (16600 B) and allocates 160-224 B on net10 in `TryFirstWhere` and `TryFirstMiss`.
- `TryLast` on a `List` now reads the last element by index: 2.995 ns on net10 and 2.5696 ns on net11, against 7.518 and 8.7540 ns for CFE.
- `TryFind` does two lookups in both libraries and takes about twice the hand-written `TryGetValue` on a hit (ratio 0.44 to 0.58 on every TFM).
- `Choose` takes 7-13× the hand-written loop for both libraries (ratio 0.08 to 0.14) and allocates 88 B.

### net8 and net9

On net8 (netstandard2.1 build) and net9 CSE's search helpers are slower, and `TryFirst` allocates: 40 B on a `List` and 48 B on a `Sequence`. On net10 and net11 the same rows allocate 0 B, which points at the runtime removing the enumerator allocation there.

| Category | Source | TFM | CSE | CFE | Hand-written |
|---|---|---|---|---|---|
| `TryFirst` | List | net8 | 17.453 ns / 40 B | 18.119 ns / 0 B | - |
| `TryFirst` | List | net9 | 15.320 ns / 40 B | 14.726 ns / 0 B | - |
| `TryFirstWhere` | List | net8 | 4,352.433 ns / 40 B | 3,641.372 ns / 216 B | 2,038.723 ns / 0 B |
| `TryFirstWhere` | List | net9 | 3,552.999 ns / 40 B | 2,456.392 ns / 192 B | 1,622.883 ns / 0 B |
| `TryFirstMiss` | List | net8 | 6,043.668 ns / 40 B | 4,346.232 ns / 160 B | 1,228.470 ns / 0 B |
| `TryFirstMiss` | List | net9 | 6,075.384 ns / 40 B | 3,546.207 ns / 160 B | 1,456.776 ns / 0 B |
| `TryLastWhere` | Sequence | net8 | 7,864.059 ns / 48 B | 7,294.826 ns / 48 B | 6,693.462 ns / 48 B |
| `TryLastWhere` | Sequence | net9 | 6,711.837 ns / 48 B | 6,571.505 ns / 48 B | 5,253.496 ns / 48 B |
| `TryLastMiss` | Sequence | net8 | 5,234.965 ns / 48 B | 4,753.006 ns / 48 B | 3,443.629 ns / 48 B |
| `TryLast` | List | net8 | 13.047 ns / 0 B | 19.879 ns / 0 B | - |
| `Choose` | List | net8 | 5,850.007 ns / 88 B | 6,220.321 ns / 88 B | 480.250 ns / 0 B |

### Where CSE loses

- **Against CFE:** `TryFirstWhere` and `TryFirstMiss` on a `List` on net8 and net9 (net9 `TryFirstMiss`: 6,075.384 vs 3,546.207 ns); `TryFirst` on a `List` on net8 and net9 in bytes (40 B vs 0 B; on net9 also 0.594 ns slower); `TryLastWhere` on a `Sequence` on net8 and net9 and on a `List` on net8 (2,762.695 vs 2,760.766 ns); `TryLastMiss` on a `Sequence` on net8 (5,234.965 vs 4,753.006 ns) and net10 (1,463.018 vs 1,459.865 ns). Under 1 ns: `TryFindHit` on net8 to net11 and `TryFindMiss` on net9 and net11.
- **Against the hand-written loops:** `Choose` and `TryFindHit` on every TFM; `TryFindMiss` on net8, net9 and net11 (net10 under 1 ns); the search categories on net8 and net9 for both sources; on net10 `TryFirstWhere` on a `Sequence` (1,669.227 vs 1,606.616 ns) and `TryLastMiss` on both sources; on net11 `TryFirstMiss` on a `List` (1,234.2120 vs 1,214.1907 ns).

---

## Caveats

- **CSE behavior checked against the code in this tree**:
  - `CSharpEssentials.Maybe/Modules/TryLast.cs`: `IList<T>` and `IReadOnlyList<T>` fast paths, without and with a predicate.
  - `CSharpEssentials.Maybe/Modules/TryFind.cs`: `ContainsKey` and then the indexer, two hash lookups on a hit.
  - `CSharpEssentials.Maybe/Modules/TryFirst.cs`: returns none for value types on empty input or a miss. It has no `IList<T>` path and enumerates through `IEnumerable<T>`, which is where the net8 and net9 cost above comes from.
- The CSE column is the working tree, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project results-comparison                 # all TFMs (net8-net11), default job
./run-benchmarks.sh --project results-comparison --quick         # net9.0, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.Results.Comparison.Benchmarks -- --self-check
dotnet run -c Release -f net10.0 --project CSharpEssentials.Results.Comparison.Benchmarks -- --filter "*.MaybeCollectionComparison.*"
```

Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/results-comparison/{tfm}/results/`.
