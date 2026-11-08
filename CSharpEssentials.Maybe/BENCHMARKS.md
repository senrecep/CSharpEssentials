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
- `MaybeCollectionComparison` was rerun on 2026-11-07 on .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0 with SDK 11.0.100-rc.3.26522.148. Every number below comes from that rerun, and no rerun row is above 5% CV.
- Reports: `benchmarks/results/results-comparison/{tfm}/results/`, for example [MaybeCollectionComparison net10](../benchmarks/results/results-comparison/net10.0/results/CSharpEssentials.Results.Comparison.Benchmarks.MaybeCollectionComparison-report-github.md).

### net10

| Category | Source | CSE | CFE (ratio) | Hand-written (ratio) |
|---|---|---|---|---|
| `TryFirst` | List | 2.687 ns / 0 B | 8.070 ns / 0 B (3.01) | - |
| `TryFirst` | Sequence | 5.898 ns / 0 B | 4,423.863 ns / 16600 B (750.37) | - |
| `TryLast` | List | 2.637 ns / 0 B | 7.646 ns / 0 B (2.90) | - |
| `TryLast` | Sequence | 909.606 ns / 0 B | 4,102.166 ns / 16600 B (4.51) | - |
| `TryFirstWhere` | List | 1,511.619 ns / 0 B | 2,381.262 ns / 192 B (1.58) | 1,549.859 ns / 0 B (1.03) |
| `TryFirstWhere` | Sequence | 1,842.250 ns / 0 B | 2,815.999 ns / 224 B (1.53) | 1,815.016 ns / 0 B (0.99) |
| `TryLastWhere` | List | 1,520.830 ns / 0 B | 1,527.976 ns / 0 B (1.00) | 1,663.832 ns / 0 B (1.09) |
| `TryLastWhere` | Sequence | 3,580.297 ns / 0 B | 4,368.203 ns / 48 B (1.22) | 3,264.382 ns / 0 B (0.91) |
| `TryFirstMiss` | List | 1,314.010 ns / 0 B | 3,083.930 ns / 160 B (2.35) | 1,509.095 ns / 0 B (1.15) |
| `TryFirstMiss` | Sequence | 1,516.854 ns / 0 B | 3,830.247 ns / 192 B (2.53) | 1,779.556 ns / 0 B (1.17) |
| `TryLastMiss` | List | 1,264.376 ns / 0 B | 1,291.321 ns / 0 B (1.02) | 1,247.162 ns / 0 B (0.99) |
| `TryLastMiss` | Sequence | 1,534.901 ns / 0 B | 1,517.922 ns / 0 B (0.99) | 1,546.054 ns / 0 B (1.01) |
| `TryFindHit` | List | 22.916 ns / 0 B | 22.038 ns / 0 B (0.96) | 11.405 ns / 0 B (0.50) |
| `TryFindMiss` | List | 9.931 ns / 0 B | 10.054 ns / 0 B (1.01) | 9.526 ns / 0 B (0.96) |
| `Choose` | List | 3,319.996 ns / 88 B | 3,397.408 ns / 88 B (1.02) | 475.180 ns / 0 B (0.14) |

`TryFind` and `Choose` do not depend on `Source`; the `Sequence` rows for them (TryFindHit 24.853 ns, TryFindMiss 10.469 ns, Choose 3,273.527 ns for CSE) are close to the `List` rows.

- On net10 CSE is faster than CFE or tied with it in every search category. On net11 the one exception is `TryLastMiss` on a `List` (1,210.729 vs 1,184.469 ns). CSE allocates nothing on either TFM. CFE copies a lazy source to a list for `TryFirst` and `TryLast` without a predicate (16600 B) and allocates 160-224 B on net10 in `TryFirstWhere` and `TryFirstMiss`.
- `TryLast` on a `List` reads the last element by index: 2.637 ns on net10 and 2.540 ns on net11, against 7.646 and 7.629 ns for CFE. `TryFirst` on a `List` now reads by index too (2.687 ns on net10).
- `TryFind` does two lookups in both libraries and takes about twice the hand-written `TryGetValue` on a hit (ratio 0.43 to 0.52 on every TFM).
- `Choose` takes 7-11× the hand-written loop for CSE (ratio 0.09 to 0.14), CFE is in the same range, and both allocate 88 B.

### net8 and net9

On net8 (netstandard2.1 build) and net9 every column is slower than on net10 and net11, and on a `Sequence` every column allocates 48 B, the hand-written `foreach` included. On net10 and net11 the CSE and hand-written `Sequence` rows allocate 0 B, which points at the runtime removing the enumerator allocation there. `TryFirst` on a `List` now uses the index path and allocates 0 B on every TFM, and on a `List` CSE is now faster than CFE in `TryFirst`, `TryFirstWhere` and `TryFirstMiss` on net8 and net9.

| Category | Source | TFM | CSE | CFE | Hand-written |
|---|---|---|---|---|---|
| `TryFirst` | List | net8 | 14.89 ns / 0 B | 19.56 ns / 0 B | - |
| `TryFirst` | List | net9 | 11.054 ns / 0 B | 15.528 ns / 0 B | - |
| `TryFirstWhere` | List | net8 | 2,828.73 ns / 0 B | 3,753.73 ns / 216 B | 2,148.24 ns / 0 B |
| `TryFirstWhere` | List | net9 | 1,778.366 ns / 0 B | 2,428.312 ns / 192 B | 1,606.592 ns / 0 B |
| `TryFirstMiss` | List | net8 | 1,887.61 ns / 0 B | 4,646.53 ns / 160 B | 1,311.59 ns / 0 B |
| `TryFirstMiss` | List | net9 | 1,465.363 ns / 0 B | 3,821.654 ns / 160 B | 1,473.769 ns / 0 B |
| `TryLastWhere` | Sequence | net8 | 8,404.63 ns / 48 B | 7,784.34 ns / 48 B | 7,107.47 ns / 48 B |
| `TryLastWhere` | Sequence | net9 | 6,947.844 ns / 48 B | 6,581.671 ns / 48 B | 5,774.985 ns / 48 B |
| `TryLastMiss` | Sequence | net8 | 5,593.46 ns / 48 B | 5,024.79 ns / 48 B | 3,506.99 ns / 48 B |
| `TryLast` | List | net8 | 13.71 ns / 0 B | 21.02 ns / 0 B | - |
| `Choose` | List | net8 | 6,285.35 ns / 88 B | 6,660.65 ns / 88 B | 547.66 ns / 0 B |

### Where CSE loses

- **Against CFE:** on net8 `TryLastWhere` on a `Sequence` (8,404.63 vs 7,784.34 ns), `TryFirstMiss` on a `Sequence` (5,706.86 vs 5,510.63 ns), `TryLastMiss` on both sources (`Sequence` 5,593.46 vs 5,024.79 ns) and `TryFindHit` (43.44 vs 39.61 ns); on net9 `TryLastWhere` and `TryLastMiss` on a `Sequence` (6,947.844 vs 6,581.671 ns, 5,986.563 vs 5,675.622 ns) and `TryFindHit` in the `Sequence` run (39.956 vs 37.156 ns); on net11 `TryLastMiss` on a `List` (1,210.729 vs 1,184.469 ns), `Choose` in the `List` run (3,545.760 vs 3,224.333 ns) and `TryFindMiss` in the `Sequence` run (11.811 vs 10.051 ns). Under 1 ns: `TryFindMiss` on net9 (11.718 vs 11.652 ns), `TryFindHit` on net10 (22.916 vs 22.038 ns) and in the net11 `Sequence` run. CSE never allocates more than CFE.
- **Against the hand-written loops:** `Choose`, `TryFindHit` and `TryFindMiss` on every TFM (`TryFindMiss` under 1 ns on net10 and net11); every search category on both sources on net8, and on net9 except `TryFirstMiss` on a `List` (a tie); on net10 `TryLastWhere` on a `Sequence` (3,580.297 vs 3,264.382 ns); on net11 `TryFirstWhere` on both sources (`Sequence` 1,617.460 vs 1,469.123 ns), `TryLastWhere` and `TryFirstMiss` on a `Sequence` (2,919.927 vs 2,914.465 ns, 1,474.651 vs 1,454.889 ns) and `TryLastMiss` on a `List` (1,210.729 vs 1,194.491 ns).

---

## Caveats

- **CSE behavior checked against the code in this tree**:
  - `CSharpEssentials.Maybe/Modules/TryLast.cs`: `IList<T>` and `IReadOnlyList<T>` fast paths, without and with a predicate.
  - `CSharpEssentials.Maybe/Modules/TryFind.cs`: `ContainsKey` and then the indexer, two hash lookups on a hit.
  - `CSharpEssentials.Maybe/Modules/TryFirst.cs`: returns none for value types on empty input or a miss. It reads an `IList<T>` or `IReadOnlyList<T>` by index and enumerates any other source.
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
