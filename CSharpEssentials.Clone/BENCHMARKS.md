# CSharpEssentials.Clone: Performance benchmarks

**Status: draft.** The suite builds on net8.0 to net11.0, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet. Short-job (`--quick`) numbers are not published.

CSharpEssentials.Clone (CSE) is compared with hand-written loops and the BCL: `List<T>.ConvertAll`, `Array.ConvertAll` and LINQ `Select`. There is no competitor library, only BCL and hand-written baselines.

The suite lives in `benchmarks/CSharpEssentials.Baselines.Benchmarks/`. It shares that project with the Time benchmarks ([CSharpEssentials.Time/BENCHMARKS.md](../CSharpEssentials.Time/BENCHMARKS.md)).

---

## What is measured

One class, `CloneComparison`, run at 10 and 1,000 items. Each row clones every item of a collection into a new, materialized collection.

| Category | Measured unit | Rows | Label |
|---|---|---|---|
| List | `List<CloneItem>` to a new `List<CloneItem>` | CSE `list.Clone().ToList()` (baseline), hand-written `for` loop into `new List<T>(capacity)`, BCL `List<T>.ConvertAll` | Like-for-like |
| List | Same | BCL `list.Select(x => x.Clone()).ToList()` | Reference row. Plain LINQ, so it shows what CSE adds on top |
| Array | `CloneItem[]` to a new `CloneItem[]` | CSE `items.Clone().ToArray()` (baseline), hand-written `for` loop into `new T[n]`, BCL `Array.ConvertAll` | Like-for-like. The CSE row is an array source through the `IEnumerable<T>` overload |
| LazySource | A `yield` iterator with no count, to a new `List<CloneItem>` | CSE `lazy.Clone().ToList()` (baseline), hand-written `foreach` into an unsized `List<T>` | Like-for-like |
| Queryable | `IQueryable<CloneItem>` over the list, to a new `List<CloneItem>` | CSE `queryable.Clone().ToList()` (baseline) | The overload runs in memory (`EnumerableQuery<T>`). No provider translation |
| Queryable | Same items | Hand-written `for` loop over the backing list | Reference row. It presizes its result. The CSE queryable path loses the source count and grows the list, so part of the gap is list growth, not only the wrapper |

`CloneItem` is a small sealed class with an `int Id` and a `string Name`. Its `Clone()` copies both fields into a new instance, as in the Readme. The name strings are shared by the clone in every row.

## Methodology

- The class has `[MemoryDiagnoser]`. CSE is the baseline in each category. A doc URL sits above every row: the package Readme for CSE rows and learn.microsoft.com for BCL and hand-written rows.
- Both CSE overloads are lazy (`Enumerable.Select`). Every CSE row calls `ToList()` or `ToArray()`, as the Readme shows. A row that returned the lazy sequence would measure nothing.
- The package has no array overload. Calling `Clone()` on a variable typed `T[]` binds to `Array.Clone()`, which makes a shallow copy and returns `object`. The Array row therefore holds the same array in a field typed `IEnumerable<CloneItem>`, so the CSE extension binds.
- Every row returns its result, so the JIT cannot remove the work.
- The source items are built once in setup. All rows read the same items.
- net8.0 runs the netstandard2.1 build of CSE, because the package has no net8 target.

## What the parity check enforces

`[GlobalSetup]` runs the check for each item count, and `--self-check` runs it without BenchmarkDotNet. A failure throws and the run stops.

Each row's result must match the source on all of these: the item count, the sum of `Id`, the sum of `Name` lengths, the same `Id` at every index (order is kept), and a new instance at every index (no item is the source object).

## Pinned versions

| Package | Version | License |
|---|---|---|
| BenchmarkDotNet | `[0.14.0,)` from `Directory.Packages.props`, resolves 0.14.0 | MIT |

There is no competitor library to pin. The baselines are the BCL and hand-written loops.

## Feature matrix

From reading the source and docs, not measured.

| Feature | BCL or hand-written | CSharpEssentials.Clone |
|---|---|---|
| Typed clone contract | `System.ICloneable` returns `object` | `ICloneable<out T>` returns `T` |
| Clone a list into a new list | `for` loop with `new List<T>(capacity)`, `List<T>.ConvertAll` | `list.Clone().ToList()` |
| Clone an array into a new array | `for` loop with `new T[n]`, `Array.ConvertAll` | `items.Clone().ToArray()` through the `IEnumerable<T>` overload (no array overload) |
| Lazy cloning | `Enumerable.Select` | `IEnumerable<T>.Clone()`, lazy: each enumeration clones again |
| Clone an `IQueryable<T>` | `AsEnumerable().Select(...)` | `IQueryable<T>.Clone()` runs in memory (`EnumerableQuery<T>`), not in the provider |
| Deep copy | User code | User code. The package only calls `Clone()` |

## Semantic differences

- The CSE result is lazy until it is materialized. The rows materialize it, so all rows do the same work.
- `Clone()` on a `T[]` variable binds to `Array.Clone()`, not to the CSE extension. That is a shallow copy of the array, and the items are not cloned. The Array row types its source as `IEnumerable<T>` to reach the extension.
- The `IQueryable<T>` overload does not translate to the provider. It reads the source and clones in memory.

## Expected outcome

Read from the source. Check it against the full run.

- `ToList()` over a `Select` on a `List<T>` uses LINQ's sized list path, so allocations should match the hand-written loop. CSE should be slower than the plain LINQ row. Its selector is a static generic method (`Clone<T>`), and that method calls `ICloneable<T>.Clone()` through the interface in shared generic code. The LINQ row's lambda calls the sealed `CloneItem.Clone()` directly. `ConvertAll` and the hand-written loop skip the LINQ iterator, so CSE should be the slowest row in the List category.
- The Array row goes through the `IEnumerable<T>` overload. LINQ's array path should size the result exactly, so the gap to the hand-written loop should come from the same delegate and interface call.
- In the LazySource category both rows grow an unsized list, so they should be close.
- The Queryable row should lose clearly to the hand-written reference row. The overload calls `Enumerable.Select` on the `EnumerableQuery<T>`, which is not a list, so the source count is lost. `ToList()` on the second `EnumerableQuery<T>` then grows an unsized list: 9 backing arrays (4 to 1,024) at 1,000 items instead of one sized array. It also allocates the `Select` iterator, the second wrapper and their enumerators. Expect more time and more allocated bytes than the presized reference row and the List category's CSE row. Allocation should be close to the hand-written LazySource row, which also grows an unsized list.

## Left out

Reflection-based deep cloners. They copy whole object graphs, which is a different job from calling a user-written `Clone()`.

## Results

Pending the full default-job run (`./run-benchmarks.sh --project baselines`). The reports will be committed under `benchmarks/results/baselines/{tfm}/results/`.

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project baselines                        # all TFMs (net8-net11), Time and Clone classes
./run-benchmarks.sh --project baselines --tfm net10.0          # one TFM
./run-benchmarks.sh --project baselines --quick --tfm net10.0  # one TFM, Short job
dotnet run -c Release --framework net10.0 --project CSharpEssentials.Baselines.Benchmarks -- --self-check
```

To run only the Clone class, call BenchmarkDotNet directly: `dotnet run -c Release --framework net10.0 --project CSharpEssentials.Baselines.Benchmarks -- --filter "*.CloneComparison.*"`.
