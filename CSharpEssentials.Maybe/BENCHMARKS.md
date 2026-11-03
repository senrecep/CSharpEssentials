# CSharpEssentials.Maybe: Performance benchmarks (draft)

This page compares the CSharpEssentials.Maybe (CSE) collection helpers with CSharpFunctionalExtensions 3.7.0 (CFE) and with hand-written loops. It covers `TryFirst`, `TryLast`, `TryFind` and `Choose`. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The basic `Maybe<T>` operations (From, Map, Bind, Where, GetValueOrDefault, Match) are measured in [CSharpEssentials.Results/BENCHMARKS.md](../CSharpEssentials.Results/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

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

- **Reference-type elements on purpose.** With `int` elements both libraries return `Some(0)` instead of none on some misses: CSE `TryFirst` (`FirstOrDefault` gives `default`, which is not null for a value type) and CFE `TryLast` with a predicate. Strings avoid both bugs, so every column gives the same answer.
- **How each library walks the source.**
  - CSE `TryLast` walks the whole sequence, even for a `List`, with and without a predicate.
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
| Value-type miss | `TryFirst` gives `Some(default)` (documented in the Readme) | `TryLast(predicate)` gives `Some(default)` |
| `TryLast` on a list | walks the whole list | O(1) without a predicate, reverse scan with one |
| `TryFind` | `IReadOnlyDictionary`, two lookups | `IReadOnlyDictionary`, two lookups |
| License | MIT | MIT |

---

## Results

**Pending the full default-job run on net8.0-net11.0.** The reports will be committed under `benchmarks/results/results-comparison/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **Expected CSE costs, from reading the code** (not yet measured):
  - `CSharpEssentials.Maybe/Modules/TryLast.cs:13-16`: no `IList<T>` fast path, so `TryLast` is O(n) on a list.
  - `CSharpEssentials.Maybe/Modules/TryFind.cs:15-17`: `ContainsKey` and then the indexer, two hash lookups on a hit.
  - `CSharpEssentials.Maybe/Modules/TryFirst.cs:13,25`: `Some(default)` for value types on empty input or a miss.
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
