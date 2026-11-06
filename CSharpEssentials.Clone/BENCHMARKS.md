# CSharpEssentials.Clone: performance benchmarks

Full default-job results for net8.0, net9.0, net10.0 and net11.0 are in [Results](#results). The parity self-check passes on every TFM. Short version: CSE is slower than the hand-written loop in every category on every TFM, with the widest gap in the Queryable category (hand-written ratio 0.31 on net9.0 at 10 items). It allocates more than the hand-written loop for lists and arrays (528 B against 456 B for a 10-item list on every TFM), and less than it for an unsized lazy source on net9.0 and later. Every loss is listed in [Where CSE loses](#where-cse-loses).

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

- The class has `[MemoryDiagnoser]`. CSE is the baseline in each category. A doc URL sits above every row: the package Readme for CSE rows and the .NET API docs for BCL and hand-written rows.
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

Read from the source before the full run. [Against the expected outcome](#against-the-expected-outcome) says which points held.

- `ToList()` over a `Select` on a `List<T>` uses LINQ's sized list path, so allocations should match the hand-written loop. CSE should be slower than the plain LINQ row. Its selector is a static lambda inside the generic `Clone<T>`, and it calls `ICloneable<T>.Clone()` through the interface in shared generic code. The LINQ row's lambda calls the sealed `CloneItem.Clone()` directly. `ConvertAll` and the hand-written loop skip the LINQ iterator, so CSE should be the slowest row in the List category.
- The Array row goes through the `IEnumerable<T>` overload. LINQ's array path should size the result exactly, so the gap to the hand-written loop should come from the same delegate and interface call.
- In the LazySource category both rows grow an unsized list, so they should be close.
- The Queryable row should lose clearly to the hand-written reference row. The overload calls `Enumerable.Select` on the `EnumerableQuery<T>`, which is not a list, so the source count is lost. `ToList()` on the second `EnumerableQuery<T>` then grows an unsized list: 9 backing arrays (4 to 1,024) at 1,000 items instead of one sized array. It also allocates the `Select` iterator, the second wrapper and their enumerators. Expect more time and more allocated bytes than the presized reference row and the List category's CSE row. Allocation should be close to the hand-written LazySource row, which also grows an unsized list.

## Left out

Reflection-based deep cloners. They copy whole object graphs, which is a different job from calling a user-written `Clone()`.

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, default job, full runs (no `--quick`), one suite at a time, on one Linux x64 machine with a 6-core, 12-thread desktop CPU (AVX2). The exact CPU, SDK and runtime builds are in the header of each report.
- TFMs: net8.0 (.NET 8.0.30, running the netstandard2.1 build of CSE), net9.0 (.NET 9.0.19), net10.0 (.NET 10.0.11) and net11.0 (.NET 11.0.0).
- The 1-minute load average stayed between 1.55 and 2.60 during the baselines suite and the net10.0 rerun of `CloneComparison`. After the rerun no row of this class has a CV above 5%.
- Reports: [`benchmarks/results/baselines/{tfm}/results/`](../benchmarks/results/baselines/), file `CSharpEssentials.Baselines.Benchmarks.CloneComparison-report-github.md` (and `.csv`) per TFM. Every number below is copied from the `-report-github.md` files.

How to read the tables: each cell is `Mean (Ratio)` or `Allocated (Alloc Ratio)` for that TFM. The ratio is against the CSE row of the same category, count and TFM. Compare cells within one TFM column.

### Time

| Category | Count | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Array | 10 | `CSharpEssentials_CloneToArray` | 163.13 ns (1.00) | 162.07 ns (1.00) | 138.07 ns (1.00) | 113.75 ns (1.00) |
| Array | 10 | `HandWritten_ArrayLoop` | 91.89 ns (0.56) | 98.47 ns (0.61) | 86.80 ns (0.63) | 82.63 ns (0.73) |
| Array | 10 | `Bcl_ArrayConvertAll` | 105.26 ns (0.65) | 110.31 ns (0.68) | 97.11 ns (0.70) | 87.76 ns (0.77) |
| Array | 1000 | `CSharpEssentials_CloneToArray` | 12,922.80 ns (1.00) | 13,597.92 ns (1.00) | 10,987.40 ns (1.00) | 9,285.47 ns (1.00) |
| Array | 1000 | `HandWritten_ArrayLoop` | 9,468.39 ns (0.73) | 9,389.75 ns (0.69) | 7,961.21 ns (0.72) | 7,698.00 ns (0.83) |
| Array | 1000 | `Bcl_ArrayConvertAll` | 9,744.39 ns (0.75) | 10,436.58 ns (0.77) | 9,522.98 ns (0.87) | 7,971.84 ns (0.86) |
| LazySource | 10 | `CSharpEssentials_LazyCloneToList` | 304.36 ns (1.00) | 273.85 ns (1.00) | 225.66 ns (1.00) | 205.41 ns (1.00) |
| LazySource | 10 | `HandWritten_LazyForeach` | 201.75 ns (0.66) | 215.38 ns (0.79) | 195.14 ns (0.86) | 180.11 ns (0.88) |
| LazySource | 1000 | `CSharpEssentials_LazyCloneToList` | 19,657.20 ns (1.00) | 19,229.59 ns (1.00) | 15,185.36 ns (1.00) | 14,440.84 ns (1.00) |
| LazySource | 1000 | `HandWritten_LazyForeach` | 13,127.94 ns (0.67) | 13,705.07 ns (0.71) | 12,767.87 ns (0.84) | 12,428.57 ns (0.86) |
| List | 10 | `CSharpEssentials_CloneToList` | 188.21 ns (1.00) | 180.34 ns (1.00) | 135.04 ns (1.00) | 118.82 ns (1.00) |
| List | 10 | `HandWritten_ForLoop` | 120.34 ns (0.64) | 107.85 ns (0.60) | 104.65 ns (0.78) | 101.37 ns (0.85) |
| List | 10 | `Bcl_ConvertAll` | 116.03 ns (0.62) | 118.16 ns (0.66) | 105.10 ns (0.78) | 95.33 ns (0.80) |
| List | 10 | `Bcl_LinqSelectToList` | 154.65 ns (0.82) | 155.58 ns (0.86) | 144.85 ns (1.07) | 109.39 ns (0.92) |
| List | 1000 | `CSharpEssentials_CloneToList` | 12,052.88 ns (1.00) | 13,696.53 ns (1.00) | 10,736.55 ns (1.00) | 8,737.23 ns (1.00) |
| List | 1000 | `HandWritten_ForLoop` | 9,735.70 ns (0.81) | 9,732.65 ns (0.71) | 8,815.49 ns (0.82) | 8,170.05 ns (0.94) |
| List | 1000 | `Bcl_ConvertAll` | 10,039.36 ns (0.83) | 10,739.28 ns (0.78) | 9,138.58 ns (0.85) | 8,424.16 ns (0.96) |
| List | 1000 | `Bcl_LinqSelectToList` | 9,322.25 ns (0.77) | 11,865.62 ns (0.87) | 11,893.25 ns (1.11) | 8,436.10 ns (0.97) |
| Queryable | 10 | `CSharpEssentials_QueryableCloneToList` | 386.03 ns (1.00) | 352.68 ns (1.00) | 244.74 ns (1.00) | 231.78 ns (1.00) |
| Queryable | 10 | `HandWritten_ForLoopFromList` | 131.95 ns (0.34) | 108.68 ns (0.31) | 157.37 ns (0.64) | 100.55 ns (0.43) |
| Queryable | 1000 | `CSharpEssentials_QueryableCloneToList` | 23,821.94 ns (1.00) | 20,970.64 ns (1.00) | 13,267.85 ns (1.00) | 13,371.17 ns (1.00) |
| Queryable | 1000 | `HandWritten_ForLoopFromList` | 9,694.35 ns (0.41) | 9,716.74 ns (0.46) | 9,033.44 ns (0.68) | 8,058.16 ns (0.60) |

### Allocation

| Category | Count | Method | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Array | 10 | `CSharpEssentials_CloneToArray` | 472 B (1.00) | 472 B (1.00) | 472 B (1.00) | 472 B (1.00) |
| Array | 10 | `HandWritten_ArrayLoop` | 424 B (0.90) | 424 B (0.90) | 424 B (0.90) | 424 B (0.90) |
| Array | 10 | `Bcl_ArrayConvertAll` | 424 B (0.90) | 424 B (0.90) | 424 B (0.90) | 424 B (0.90) |
| Array | 1000 | `CSharpEssentials_CloneToArray` | 40072 B (1.00) | 40072 B (1.00) | 40072 B (1.00) | 40072 B (1.00) |
| Array | 1000 | `HandWritten_ArrayLoop` | 40024 B (1.00) | 40024 B (1.00) | 40024 B (1.00) | 40024 B (1.00) |
| Array | 1000 | `Bcl_ArrayConvertAll` | 40024 B (1.00) | 40024 B (1.00) | 40024 B (1.00) | 40024 B (1.00) |
| LazySource | 10 | `CSharpEssentials_LazyCloneToList` | 704 B (1.00) | 512 B (1.00) | 512 B (1.00) | 512 B (1.00) |
| LazySource | 10 | `HandWritten_LazyForeach` | 648 B (0.92) | 648 B (1.27) | 648 B (1.27) | 648 B (1.27) |
| LazySource | 1000 | `CSharpEssentials_LazyCloneToList` | 48656 B (1.00) | 40112 B (1.00) | 40112 B (1.00) | 40112 B (1.00) |
| LazySource | 1000 | `HandWritten_LazyForeach` | 48600 B (1.00) | 48600 B (1.21) | 48600 B (1.21) | 48600 B (1.21) |
| List | 10 | `CSharpEssentials_CloneToList` | 528 B (1.00) | 528 B (1.00) | 528 B (1.00) | 528 B (1.00) |
| List | 10 | `HandWritten_ForLoop` | 456 B (0.86) | 456 B (0.86) | 456 B (0.86) | 456 B (0.86) |
| List | 10 | `Bcl_ConvertAll` | 456 B (0.86) | 456 B (0.86) | 456 B (0.86) | 456 B (0.86) |
| List | 10 | `Bcl_LinqSelectToList` | 528 B (1.00) | 528 B (1.00) | 528 B (1.00) | 528 B (1.00) |
| List | 1000 | `CSharpEssentials_CloneToList` | 40128 B (1.00) | 40128 B (1.00) | 40128 B (1.00) | 40128 B (1.00) |
| List | 1000 | `HandWritten_ForLoop` | 40056 B (1.00) | 40056 B (1.00) | 40056 B (1.00) | 40056 B (1.00) |
| List | 1000 | `Bcl_ConvertAll` | 40056 B (1.00) | 40056 B (1.00) | 40056 B (1.00) | 40056 B (1.00) |
| List | 1000 | `Bcl_LinqSelectToList` | 40128 B (1.00) | 40128 B (1.00) | 40128 B (1.00) | 40128 B (1.00) |
| Queryable | 10 | `CSharpEssentials_QueryableCloneToList` | 800 B (1.00) | 800 B (1.00) | 800 B (1.00) | 800 B (1.00) |
| Queryable | 10 | `HandWritten_ForLoopFromList` | 456 B (0.57) | 456 B (0.57) | 456 B (0.57) | 456 B (0.57) |
| Queryable | 1000 | `CSharpEssentials_QueryableCloneToList` | 48752 B (1.00) | 48752 B (1.00) | 48752 B (1.00) | 48752 B (1.00) |
| Queryable | 1000 | `HandWritten_ForLoopFromList` | 40056 B (0.82) | 40056 B (0.82) | 40056 B (0.82) | 40056 B (0.82) |

### Where CSE loses

CSE is slower than the hand-written loop in every category, at both counts, on every TFM. It is also slower than `ConvertAll` in the List and Array categories everywhere. The details:

- List: CSE is the slowest row on net8.0, net9.0 and net11.0. On net10.0 the plain LINQ row is slower than CSE (144.85 ns, ratio 1.07, at 10 items and 11,893.25 ns, ratio 1.11, at 1,000 items), and the hand-written loop and `ConvertAll` are still faster. On every TFM CSE allocates 528 B against 456 B at 10 items and 40128 B against 40056 B at 1,000 items, the same as the LINQ row.
- Array: the hand-written loop is 0.56 to 0.73 of the CSE time at 10 items and 0.69 to 0.83 at 1,000 items. CSE allocates more on every TFM (472 B against 424 B, 40072 B against 40024 B).
- LazySource: the hand-written `foreach` takes 0.66 to 0.88 of the CSE time. On net8.0 CSE also allocates more (704 B against 648 B at 10 items, 48656 B against 48600 B at 1,000 items).
- Queryable: the largest gap. The presized hand-written loop takes 0.31 to 0.68 of the CSE time. On net8.0 at 1,000 items that is 23,821.94 ns for CSE against 9,694.35 ns. CSE allocates 800 B against 456 B at 10 items and 48752 B against 40056 B at 1,000 items on every TFM.

Where CSE does better: in LazySource on net9.0, net10.0 and net11.0, CSE allocates less than the hand-written loop (512 B against 648 B at 10 items, 40112 B against 48600 B at 1,000 items). Its time is still higher there.

### Against the expected outcome

- List allocation matched the LINQ row on every TFM, as expected. The expectation that CSE is slower than plain LINQ held on net8.0, net9.0 and net11.0 and failed on net10.0, where LINQ was slower.
- Array: the gap to the hand-written loop is there on every TFM. The allocation is the same on every TFM: 472 B against 424 B at 10 items and 40072 B against 40024 B at 1,000 items.
- LazySource: the rows were close on net10.0 and net11.0 (ratios 0.84 to 0.88). On net8.0 and net9.0 the gap is wider (0.66 to 0.79).
- Queryable: CSE lost to the presized loop on every TFM, as expected. At 1,000 items it allocates 48752 B on every TFM, close to the 48600 B (every TFM) of the hand-written LazySource row, which also grows an unsized list.

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project baselines                        # all TFMs (net8-net11), Time and Clone classes
./run-benchmarks.sh --project baselines --tfm net10.0          # one TFM
./run-benchmarks.sh --project baselines --quick --tfm net10.0  # one TFM, Short job
dotnet run -c Release --framework net10.0 --project CSharpEssentials.Baselines.Benchmarks -- --self-check
```

To run only the Clone class, call BenchmarkDotNet directly: `dotnet run -c Release --framework net10.0 --project CSharpEssentials.Baselines.Benchmarks -- --filter "*.CloneComparison.*"`.
