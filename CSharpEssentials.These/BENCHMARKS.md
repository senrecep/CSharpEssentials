# CSharpEssentials.These: Performance benchmarks (draft)

This page compares CSharpEssentials.These (CSE) `These<string, int>` with a hand-written `readonly record struct Outcome`. `Outcome` has the same four fields and the same JSON names, and it is handled with plain if/else and no delegates. No library competitor is included. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

What the comparison answers: how much CSE's delegate-based API costs compared with writing the three-state logic by hand.

---

## What was measured

The four These classes are in `benchmarks/CSharpEssentials.Any.Comparison.Benchmarks/`, next to the Any classes ([CSharpEssentials.Any/BENCHMARKS.md](../CSharpEssentials.Any/BENCHMARKS.md)).
- Every class has `[MemoryDiagnoser]`, and CSE is the baseline.
- `TheseMatchComparison` folds a 1024-element array and reports the time per fold (`OperationsPerInvoke = 1024`). In a single-call version the hand-written row was too short for BenchmarkDotNet to tell apart from an empty method.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check. The CSE row and the hand-written row must give the same result, or the run fails.

| # | Class | Measured unit | Expected |
|---|---|---|---|
| 1 | `TheseMatchComparison` | Fold to an int: Left gives the error length, Right gives the value, Both gives value + error length. CSE `Match` takes three delegates; the hand-written row is if/else in the same branch order | 4 / 42 / 46 |
| 2 | `ThesePipelineComparison` | `Map(v * 2)`, then `FlatMap(v > 50 ? Right(v + 1) : Left("small"))`, then the same fold | 4 / 85 / 89 |
| 3 | `ThesePartitionComparison` | Split 1000 values (334 Left, 333 Right, 333 Both) into three lists | Counts, sums and text lengths |
| 4 | `TheseJsonComparison` | System.Text.Json `Serialize` and `Deserialize` | Identical JSON text and decoded state |

Inputs: Left(`"warn"`), Right(`42`) and Both(`"warn"`, `42`), picked with `[Params(Left, Right, Both)]`. In the partition input, `i % 3 == 0` gives Left(`"t" + i`), `1` gives Right(`i`) and `2` gives Both(`"t" + i`, `i`). Both sources are typed as `IEnumerable<T>` over arrays built in setup.

On net8.0, CSE runs its netstandard2.1 build, because the package has no net8 target.

---

## Semantic differences

- **FlatMap on Both.** CSE never combines errors here. If the mapper returns Right or Both, the original left is kept and the mapper's left is dropped. If it returns Left, the mapper's left is returned and the original left is dropped (`CSharpEssentials.These/These.cs:72-75`). The hand-written pipeline copies this rule on purpose so that parity holds. Copying it does not mean the rule is recommended, because one of the two errors never reaches the caller.
- **JSON.** Both rows write `{"isLeft":...,"isRight":...,"left":...,"right":...}`, and parity checks that the two JSON strings are equal. CSE binds through a private `[JsonConstructor]`; `Outcome` binds through its public record constructor.
- **Why a value tuple was not used.** The baseline is a record and not a value tuple, because STJ skips tuple fields unless `IncludeFields` is set and would silently write `{}`.
- **Partition.** CSE's `Partition` returns `IReadOnlyList`s (unlike Any's arrays), and so does the hand-written row.

---

## Feature matrix

| Feature | CSE These | Hand-written `Outcome` |
|---|---|---|
| States | Left, Right, Both (+ an invalid `default`) | Whatever the caller codes |
| Map / MapLeft / FlatMap / Tap / Match | Yes | Hand-written if/else |
| Maybe and Result bridges | `GetRight`/`GetLeft`, `ToResult`, `ToResultLenient`, `FromResult` | None |
| Partition | Yes (lists) | Hand-written |
| JSON | `{"isLeft","isRight","left","right"}` | The same, through `[JsonPropertyName]` |
| Combining errors in FlatMap on Both | None. One of the two lefts is always dropped (the mapper's on Right or Both, the original on Left) | The caller decides |
| Native AOT | Not flagged. Private `[JsonConstructor]` (`These.cs:9-11`) | Public record constructor |

No library competitor is included. These is a small type, and the useful question is what it costs compared with the equivalent hand-written code.

---

## Results

**Pending.** No full run has been done yet. The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/any/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- The baseline is hand-written code, not another library. It has none of CSE's API surface, such as the bridges or `Tap`.
- The pipeline baseline copies CSE's lossy FlatMap rule on Both.
- On net8.0 CSE runs its netstandard2.1 build, and System.Text.Json resolves to the 9.0.4 package for both rows. On net9.0 and later the framework's copy wins.
- The CSE column is the 7.0.0 working tree on branch `perf/bench-any`.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project any # all TFMs, Any and These classes
dotnet run -c Release -f net10.0 --project CSharpEssentials.Any.Comparison.Benchmarks -- --filter "*.These*" # These classes only
```

Results are stored per framework under `benchmarks/results/any/{tfm}/results/`.
