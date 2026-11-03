# CSharpEssentials.Results: Performance Benchmarks

Compared with CSharpFunctionalExtensions 3.7.0, ErrorOr 2.1.1 and FluentResults 4.0.0 · BenchmarkDotNet v0.14.0 · AMD Ryzen 5 5600 · .NET 8 / 9 / 10 / 11

The same suite has ten newer classes that have not had a full run yet. Sequence, CombineAll, Try and TryAsync are described in [Pending: Sequence, CombineAll, Try and TryAsync](#pending-sequence-combineall-try-and-tryasync). The `Error` classes are in [CSharpEssentials.Errors/BENCHMARKS.md](../CSharpEssentials.Errors/BENCHMARKS.md), and the Maybe collection helpers are in [CSharpEssentials.Maybe/BENCHMARKS.md](../CSharpEssentials.Maybe/BENCHMARKS.md).

---

## What was measured

Nine measured scenarios, one benchmark class each, in `benchmarks/CSharpEssentials.Results.Comparison.Benchmarks/`. The ten newer classes in that folder are listed under Pending and in the Errors and Maybe pages. Every class uses `[MemoryDiagnoser]`, CSE as the baseline, and a `[Params]` switch for the success and failure path. A `[GlobalSetup]` parity check runs every library on the same input and refuses to start if the outcomes differ (success value, or error count and first message).

| # | Class | Operation | Columns |
|---|---|---|---|
| 1 | `CreateComparison` | Success from `42`, failure from a cached error, through each library's documented implicit conversion (CFE: `Result.Failure<int>(string)`, it has no string-to-result conversion) | CSE, CFE, ErrorOr, FluentResults |
| 2 | `MapComparison` | `x => x * 2` over a result built in setup (ErrorOr: `Then(Func<T, TNext>)`) | all four |
| 3 | `BindComparison` | `x => Ok(x + 1)` (CFE `Bind`, ErrorOr `Then`, FluentResults `Bind`) | all four |
| 4 | `EnsureComparison` | `x > 0` on 42 (passes) or -1 (fails and creates the error). ErrorOr: `FailIf` with the inverted predicate | CSE, CFE, ErrorOr. FluentResults has no Ensure; its alternatives drop the value or are a hand-written Bind |
| 5 | `MatchComparison` | Match to an int (-1 on failure) and to a string (first error message) | all four. FluentResults has no Match; its column uses the README's `IsSuccess`/`Value`/`Errors[0].Message` idiom |
| 6 | `TapComparison` | Side effect into a field, one cached delegate shared by all (ErrorOr: `ThenDo`) | CSE, CFE, ErrorOr. FluentResults has no Tap |
| 7 | `AsyncPipelineComparison` | Five steps over a `Task` source: async Bind (x + 1) → Map (x * 2) → Ensure (x > 0) → Tap → Map (x + 1). `Succeed=false` fails at step 1. `Yield=false`: source and step complete synchronously; `Yield=true`: both complete after `Task.Yield()` | CSE, CFE, ErrorOr. FluentResults has only Bind and Map on `Task<Result<T>>` |
| 8 | `ErrorAggregationComparison` | Three independent checks on `Order(Id, Total)`, collect every error. All-pass `Order(42, 10)`, all-fail `Order(-1, -5)` | all four, each with its documented idiom (see section 8) |
| 9 | `MaybeComparison` | From/None, Map, Bind, Where, GetValueOrDefault, Match over Some(42) or None | CSE, CFE. ErrorOr and FluentResults have no Maybe/Option type |

Every competitor method carries a comment with the README or source URL at the pinned version. Errors are `static readonly` (CFE: a `const string`, its default error type), so error construction is outside the timing except where the operation itself builds the failure. The numbers come from the reports committed under `benchmarks/results/results-comparison/{tfm}/results/` (default BenchmarkDotNet job).

### Environment

| Property | Value |
|---|---|
| Machine | AMD Ryzen 5 5600, 6 cores / 12 threads (x64, RyuJIT AVX2) |
| OS | CachyOS, Linux kernel 7.3.0-4-cachyos |
| Machine load | Desktop session in the background; 1-minute load average 0.96 at the net10 start, 1.08 net9, 1.44 net8, 1.21 net11, 1.19 at the end (run order net10 → net9 → net8 → net11) |
| Tool | BenchmarkDotNet v0.14.0, [MemoryDiagnoser], default job |
| .NET SDK | 11.0.100-rc.1.26425.128 |
| Runtimes tested | .NET 8.0.30 (CSE: netstandard2.1 build), .NET 9.0.19, .NET 10.0.11, .NET 11.0.0 |
| CSharpFunctionalExtensions (CFE) | 3.7.0, default idiom `Result<T>` with a `string` error |
| ErrorOr | 2.1.1 |
| FluentResults | 4.0.0 |
| CSharpEssentials.Results / Maybe (CSE) | The repository source, referenced as a project |

---

## Executive summary

Primary runtime: net10. The main result is allocation. A tie is a difference within the sum of the two BenchmarkDotNet error margins. An absolute time difference under 1 ns is reported as noise even when it falls outside error; many of these operations take under 2 ns.

| Finding | Details |
|---|---|
| Map, Bind, Tap, Match, Ensure (pass), Create (success): 0 B for CSE, CFE and ErrorOr; FluentResults 88-344 B | net10: FluentResults Map 256 B / 56.9 ns (success), Bind 320 B / 69.7 ns, Create 112 B / 22.7 ns, Match-to-string on failure 240 B / 76.3 ns. CSE, CFE and ErrorOr stay under 2.8 ns. Same byte counts on every TFM for CSE, CFE and ErrorOr; FluentResults allocates 8-64 B more on net8 and 24 B more on net9 for Match to string on failure (264 B) |
| Create failure: CSE allocates more than ErrorOr and CFE (CSE loss) | 104 B / 23.6 ns for CSE, 88 B / 18.4 ns for ErrorOr, 0 B / 0.34 ns for CFE. CSE wraps the error in an `Error[]`, ErrorOr in a `List<Error>`; CFE stores the string in its struct, so it allocates nothing. FluentResults: 344 B / 92.7 ns. Same bytes on every TFM, except FluentResults on net8 (384 B) |
| Ensure failure: same pattern (CSE loss) | 104 B / 24.7 ns for CSE, 88 B / 18.9 ns for ErrorOr, 0 B / 1.8 ns for CFE (string error). Same bytes on every TFM |
| Map, Bind, Ensure (pass): CSE = ErrorOr; CFE 1.0-1.3 ns slower on net10 | Map success 0.576 / 0.577 / 1.899 ns (CSE / ErrorOr / CFE). On net8 and net9 CFE takes 8.4-9.1 ns for Map, Bind and Ensure, and 7.3-7.4 ns for Create, against 0.85-1.5 ns for CSE (0.15-0.17 ns for Create success). On net10 and net11 CFE drops to about 2 ns for Map, Bind and Ensure and to 0.07-0.34 ns for Create. This looks like a runtime/JIT effect; not investigated |
| Match on failure: ErrorOr about 1.1-1.6 ns faster (small CSE loss) | Match to int on failure: 1.151 vs 2.241 ns on net10; 1.08 vs 2.70 ns on net8, 0.99 vs 2.21 ns on net9, 1.01 vs 2.23 ns on net11. Match to string on failure: 0.90 vs 2.35 ns on net8, 0.91 vs 2.06 ns on net9 (net10 0.98 ns, net11 0.78 ns: noise). CFE's Match to int takes about 2.3 ns on every TFM, about 1.2 ns slower than CSE on success and than ErrorOr on both paths; CFE's Match to string on success is 1.0-1.1 ns slower than CSE on net9 and net11; on net8 ErrorOr's Tap failure is 1.2 ns slower than CSE. The remaining Match and Tap gaps are under 1 ns |
| Async pipeline, success, completed tasks: CSE fastest, allocation tie with CFE | 95.07 ns / 480 B for CSE, 104.30 ns / 480 B for CFE (1.10×), 112.01 ns / 560 B for ErrorOr (1.18×). CSE is faster than CFE on every TFM |
| Async pipeline, success, yielding: time tie, CSE allocates 64 B more than CFE (CSE loss) | 1,591.97 ns / 943 B for CSE, 1,609.77 ns / 879 B for CFE (tie on time), 1,849.47 ns / 1,036 B for ErrorOr |
| Async pipeline, failure at step 1: CFE allocates less and is faster on both paths (CSE loss) | Completed: 123.45 ns / 584 B for CSE vs 109.73 ns / 480 B for CFE (CSE takes 1.13× as long and allocates 1.22×). Yielding: 1,680.14 ns / 1,048 B vs 1,625.30 ns / 879 B (1.03× time, outside error; 1.19× allocation). CFE is faster on the completed failure path on every TFM; on net11 the yielding failure path is a tie. ErrorOr allocates more than CSE on every async row (648 B and 1,126 B on failure) |
| Error aggregation, all-fail: ErrorOr 5.7× faster with 3.8× less allocation; FluentResults 14% faster (CSE loss) | 237.91 ns / 696 B for CSE, 41.86 ns / 184 B for ErrorOr, 205.11 ns / 712 B for FluentResults, 372.08 ns / 784 B for CFE (1.56× CSE). The idioms differ: ErrorOr's documented idiom fills a `List<Error>` by hand and builds no per-check results (see section 8). FluentResults is faster than CSE on net9, net10 and net11 and slower on net8 (254.0 vs 214.1 ns) |
| Error aggregation, all-pass: ErrorOr allocates less (CSE loss) | 16.01 ns / 80 B for CSE, 11.27 ns / 32 B for ErrorOr, 30.80 ns / 160 B for CFE, 100.54 ns / 368 B for FluentResults. CSE allocates 112 B here on net8 and net9 |
| Maybe: tie with CFE on net9-net11; one CSE loss on net8 | Both allocate 0 B in every operation. Every net10 mean is at most 1.14 ns and every CSE vs CFE gap is under 0.3 ns. On net8, Bind on None takes 6.92 ns for CSE vs 5.54 ns for CFE (1.25×) |
| Yielding pipeline on net11: 1.5-1.9× slower than net10; ErrorOr slows least and beats CSE on success (CSE loss) | Success: 2,876.75 ns for ErrorOr vs 3,009.78 ns for CSE (1.05×, outside error), CFE 2,993.30 ns (tie with CSE). Failure: all three within error (2,919-2,968 ns). Growth vs net10: CSE 1.89×/1.77×, CFE 1.86×/1.81×, ErrorOr 1.56×/1.51×. Allocation unchanged. Not investigated |
| AOT and dependencies: known gap, no fix in this release | CSharpEssentials.Results, Errors and Maybe do not set `IsAotCompatible` (neither do the three competitors), so the trim analyzer never runs on them. `Result<T>.ToString()` on success calls `ConvertToJson`, reflection-based System.Text.Json annotated `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`. `ErrorMetadata` stores `object?` values. Results pulls in Errors and Json, which bring Core and Enums; CFE has no runtime dependencies. For a thin layer: `Result`/`Result<T>` are readonly record structs and Map/Bind/Tap/Match/Ensure (pass) allocate 0 B. Against: the reflection-based `ToString()`, the `object?` metadata bag, four transitive CSharpEssentials packages (CFE: none), and no `IsAotCompatible`. The "trim and AOT friendly" wording used for Endpoints and DependencyInjection does not apply to Results today |

---

## Results by scenario

All tables: net10, mean and allocated bytes per operation. "vs CSE" divides the library's mean by the CSE mean (above 1.00 = slower than CSE). A dash means the library has no column in that class (reason in the What was measured table).

### 1. Create

| Library | Success mean | Success alloc | Failure mean | Failure alloc | Failure vs CSE |
|---|---:|---:|---:|---:|---:|
| CSE | 0.269 ns | 0 B | 23.615 ns | 104 B | 1.00 |
| CFE (string error) | 0.292 ns | 0 B | 0.342 ns | 0 B | 0.01 |
| ErrorOr | 0.270 ns | 0 B | 18.439 ns | 88 B | 0.78 |
| FluentResults | 22.684 ns | 112 B | 92.668 ns | 344 B | 3.92 |

Success is free for the three struct-based libraries. On failure CSE allocates an `Error[]` of one element and ErrorOr a `List<Error>`; CFE's default `Result<T>` keeps the string inline. A CFE `Result<T, E>` column with a typed error was not benchmarked.

### 2. Map (`x => x * 2`)

| Library | Success mean | Success alloc | Failure mean | Failure alloc |
|---|---:|---:|---:|---:|
| CSE | 0.576 ns | 0 B | 0.888 ns | 0 B |
| CFE | 1.899 ns | 0 B | 1.952 ns | 0 B |
| ErrorOr | 0.577 ns | 0 B | 0.875 ns | 0 B |
| FluentResults | 56.867 ns | 256 B | 118.303 ns | 344 B |

CSE and ErrorOr tie. FluentResults returns a new class instance on both paths.

### 3. Bind / Then (`x => Ok(x + 1)`)

| Library | Success mean | Success alloc | Failure mean | Failure alloc |
|---|---:|---:|---:|---:|
| CSE | 0.576 ns | 0 B | 0.891 ns | 0 B |
| CFE | 1.870 ns | 0 B | 1.953 ns | 0 B |
| ErrorOr | 0.577 ns | 0 B | 0.876 ns | 0 B |
| FluentResults | 69.668 ns | 320 B | 53.881 ns | 208 B |

### 4. Ensure (`x > 0`)

| Library | Pass mean | Pass alloc | Fail mean | Fail alloc | Fail vs CSE |
|---|---:|---:|---:|---:|---:|
| CSE | 1.153 ns | 0 B | 24.654 ns | 104 B | 1.00 |
| CFE (string error) | 2.348 ns | 0 B | 1.805 ns | 0 B | 0.07 |
| ErrorOr (`FailIf`) | 1.150 ns | 0 B | 18.880 ns | 88 B | 0.77 |
| FluentResults | - | - | - | - | - |

The failing case turns a success into a failure, so it costs what Create-failure costs: CSE allocates the most of the three. CFE's 0 B comes from the string error.

### 5. Match

| Library | ToInt success | ToInt failure | ToString success | ToString failure | Alloc |
|---|---:|---:|---:|---:|---:|
| CSE | 1.119 ns | 2.241 ns | 2.030 ns | 1.989 ns | 0 B |
| CFE | 2.325 ns | 2.328 ns | 2.748 ns | 1.588 ns | 0 B |
| ErrorOr | 1.122 ns | 1.151 ns | 2.604 ns | 1.010 ns | 0 B |
| FluentResults (properties idiom) | 20.782 ns | 20.944 ns | 22.401 ns | 76.271 ns | 88-240 B |

ErrorOr's ToInt failure is 1.09 ns faster than CSE on net10 and about 1.1-1.6 ns faster on every TFM. ErrorOr's ToString failure is 0.98 ns faster on net10 and 1.16-1.45 ns faster on net9 and net8. CFE's ToInt rows (2.325 / 2.328 ns) are about 1.2 ns slower than CSE on success and than ErrorOr on both paths. The other net10 differences are under 1 ns. FluentResults allocates even when only reading properties (its `Errors` is computed with `Reasons.OfType<IError>()`).

### 6. Tap

| Library | Success mean | Failure mean | Alloc |
|---|---:|---:|---:|
| CSE | 1.618 ns | 0.656 ns | 0 B |
| CFE | 1.923 ns | 1.194 ns | 0 B |
| ErrorOr (`ThenDo`) | 1.597 ns | 0.815 ns | 0 B |

All differences under 1 ns: noise.

### 7. Async 5-step pipeline (Bind async → Map → Ensure → Tap → Map)

| Path | CSE | CFE | ErrorOr | CFE vs CSE (time / alloc) |
|---|---|---|---|---|
| Success, completed | 95.07 ns / 480 B | 104.30 ns / 480 B | 112.01 ns / 560 B | 1.10 / 1.00 |
| Success, yielding | 1,591.97 ns / 943 B | 1,609.77 ns / 879 B | 1,849.47 ns / 1,036 B | 1.01 (tie) / 0.93 |
| Failure at step 1, completed | 123.45 ns / 584 B | 109.73 ns / 480 B | 132.43 ns / 648 B | 0.89 / 0.82 |
| Failure at step 1, yielding | 1,680.14 ns / 1,048 B | 1,625.30 ns / 879 B | 1,926.91 ns / 1,126 B | 0.97 / 0.84 |

- CSE wins on the success path with completed tasks and ties CFE in time when the tasks yield, but allocates 64 B more than CFE there.
- On the failure path CFE is ahead on both time and allocation (104 B and 169 B less). Part of that is the error representation: CFE carries one string, CSE an `Error[]` built at step 1 (the Create-failure cost from section 1).
- ErrorOr allocates the most on every path, and is the slowest on net8 to net10; on net11 it is the fastest on the yielding success path.

### 8. Error aggregation (three independent checks)

> The idioms differ, so the work differs. ErrorOr's README idiom is a hand-filled `List<Error>`: three `if` statements, no intermediate result objects. CSE (`new[] { Result.FailureIf(...) × 3 }.CombineAll()`), CFE (`Result.Combine(Result.FailureIf(...) × 3)`) and FluentResults (`Result.Merge(Result.FailIf(...) × 3)`) build one result per check and then combine them. CFE's `Combine` joins string messages with ", " and collapses a repeated message into one entry suffixed with its count (`message (N×)`). CSE's typed `Result<T>.Combine(r1, r2, r3)` skips the array and list but was not benchmarked.

| Library | All-pass mean | All-pass alloc | All-fail mean | All-fail alloc | All-fail vs CSE (time / alloc) |
|---|---:|---:|---:|---:|---|
| CSE (`CombineAll`) | 16.01 ns | 80 B | 237.91 ns | 696 B | 1.00 / 1.00 |
| CFE (`Result.Combine`, strings) | 30.80 ns | 160 B | 372.08 ns | 784 B | 1.56 / 1.13 |
| ErrorOr (manual `List<Error>`) | 11.27 ns | 32 B | 41.86 ns | 184 B | 0.18 / 0.26 |
| FluentResults (`Result.Merge`) | 100.54 ns | 368 B | 205.11 ns | 712 B | 0.86 / 1.02 |

CSE loses to ErrorOr on both paths and to FluentResults on all-fail time (allocation within 16 B). It beats CFE on both paths and FluentResults on all-pass.

### 9. Maybe (CSE `Maybe<T>` vs CFE `Maybe<T>`)

| Operation | Some: CSE | Some: CFE | None: CSE | None: CFE |
|---|---:|---:|---:|---:|
| From / None | 0.002 ns | 0.289 ns | 0.000 ns | 0.000 ns |
| Map | 0.683 ns | 0.645 ns | 0.365 ns | 0.365 ns |
| Bind | 0.687 ns | 0.635 ns | 0.371 ns | 0.366 ns |
| Where | 1.029 ns | 1.016 ns | 0.546 ns | 0.412 ns |
| GetValueOrDefault | 0.301 ns | 0.291 ns | 0.011 ns | 0.001 ns |
| Match | 1.124 ns | 1.111 ns | 1.065 ns | 1.047 ns |

Every row allocates 0 B for both. Values near zero are indistinguishable from an empty method; the BenchmarkDotNet ratios for those rows (for example 243× on From) are meaningless and not quoted. On net8 and net9, Map and Bind on Some and From on Some take 4.9-7.1 ns for both libraries, and Bind on None takes 5.5-6.9 ns. On net8 CFE's Bind on None is 1.39 ns faster (5.54 vs 6.92 ns); the other net8/net9 gaps are under 0.3 ns.

---

## Cross-TFM

Mean / allocated per TFM for the rows where libraries differ. Map, Bind, Tap, Match and Maybe allocate the same bytes on every TFM (0 B except FluentResults) and are left out.

| Scenario | Library | net8 | net9 | net10 | net11 |
|---|---|---|---|---|---|
| Create failure | CSE | 24.35 ns / 104 B | 27.05 ns / 104 B | 23.62 ns / 104 B | 18.73 ns / 104 B |
| | CFE | 7.35 ns / 0 B | 7.35 ns / 0 B | 0.34 ns / 0 B | 0.27 ns / 0 B |
| | ErrorOr | 18.48 ns / 88 B | 20.94 ns / 88 B | 18.44 ns / 88 B | 16.22 ns / 88 B |
| | FluentResults | 177.54 ns / 384 B | 129.71 ns / 344 B | 92.67 ns / 344 B | 85.71 ns / 344 B |
| Ensure failure | CSE | 24.54 ns / 104 B | 26.13 ns / 104 B | 24.65 ns / 104 B | 19.03 ns / 104 B |
| | CFE | 8.45 ns / 0 B | 8.46 ns / 0 B | 1.81 ns / 0 B | 1.80 ns / 0 B |
| | ErrorOr | 20.00 ns / 88 B | 21.31 ns / 88 B | 18.88 ns / 88 B | 16.26 ns / 88 B |
| Map success | CSE | 0.94 ns / 0 B | 0.85 ns / 0 B | 0.58 ns / 0 B | 0.58 ns / 0 B |
| | CFE | 8.93 ns / 0 B | 8.91 ns / 0 B | 1.90 ns / 0 B | 1.80 ns / 0 B |
| | ErrorOr | 0.94 ns / 0 B | 0.85 ns / 0 B | 0.58 ns / 0 B | 0.58 ns / 0 B |
| | FluentResults | 132.97 ns / 288 B | 103.45 ns / 256 B | 56.87 ns / 256 B | 55.55 ns / 256 B |
| Async success, completed | CSE | 104.3 ns / 480 B | 99.6 ns / 480 B | 95.1 ns / 480 B | 84.5 ns / 480 B |
| | CFE | 115.1 ns / 480 B | 111.0 ns / 480 B | 104.3 ns / 480 B | 94.5 ns / 480 B |
| | ErrorOr | 123.3 ns / 560 B | 117.7 ns / 560 B | 112.0 ns / 560 B | 99.8 ns / 560 B |
| Async success, yielding | CSE | 1,630.7 ns / 944 B | 1,599.9 ns / 944 B | 1,592.0 ns / 943 B | 3,009.8 ns / 944 B |
| | CFE | 1,656.9 ns / 879 B | 1,594.7 ns / 880 B | 1,609.8 ns / 879 B | 2,993.3 ns / 880 B |
| | ErrorOr | 1,850.1 ns / 1,037 B | 1,866.5 ns / 1,037 B | 1,849.5 ns / 1,036 B | 2,876.8 ns / 1,039 B |
| Async failure, completed | CSE | 137.9 ns / 584 B | 129.0 ns / 584 B | 123.5 ns / 584 B | 106.4 ns / 584 B |
| | CFE | 112.7 ns / 480 B | 107.5 ns / 480 B | 109.7 ns / 480 B | 95.7 ns / 480 B |
| | ErrorOr | 142.7 ns / 648 B | 136.0 ns / 648 B | 132.4 ns / 648 B | 111.2 ns / 648 B |
| Async failure, yielding | CSE | 1,639.0 ns / 1,048 B | 1,602.3 ns / 1,048 B | 1,680.1 ns / 1,048 B | 2,967.9 ns / 1,048 B |
| | CFE | 1,582.6 ns / 879 B | 1,539.3 ns / 880 B | 1,625.3 ns / 879 B | 2,934.8 ns / 880 B |
| | ErrorOr | 1,871.5 ns / 1,127 B | 1,873.3 ns / 1,126 B | 1,926.9 ns / 1,126 B | 2,919.0 ns / 1,128 B |
| Aggregation all-fail | CSE | 214.1 ns / 696 B | 423.2 ns / 696 B | 237.9 ns / 696 B | 215.2 ns / 696 B |
| | CFE | 434.4 ns / 800 B | 451.5 ns / 784 B | 372.1 ns / 784 B | 327.3 ns / 784 B |
| | ErrorOr | 46.3 ns / 184 B | 46.1 ns / 184 B | 41.9 ns / 184 B | 33.5 ns / 184 B |
| | FluentResults | 254.0 ns / 712 B | 274.8 ns / 712 B | 205.1 ns / 712 B | 189.7 ns / 712 B |
| Aggregation all-pass | CSE | 30.8 ns / 112 B | 32.9 ns / 112 B | 16.0 ns / 80 B | 15.2 ns / 80 B |
| | CFE | 52.2 ns / 160 B | 53.8 ns / 160 B | 30.8 ns / 160 B | 28.8 ns / 160 B |
| | ErrorOr | 8.1 ns / 32 B | 11.5 ns / 32 B | 11.3 ns / 32 B | 11.2 ns / 32 B |
| | FluentResults | 148.5 ns / 368 B | 141.6 ns / 368 B | 100.5 ns / 368 B | 96.9 ns / 368 B |

- The allocation findings hold on every TFM: CSE allocates more than CFE on async failure (+104 B completed, +168-169 B yielding) and on Create/Ensure failure (+104 B), and more than ErrorOr on Create/Ensure failure (+16 B) and aggregation (+48-80 B all-pass, +512 B all-fail).
- net9 aggregation all-fail for CSE (423.2 ns) is a single-TFM outlier against 214-238 ns elsewhere, with unchanged allocation. Not investigated; treat it as suspect.
- net11 yielding rows take 1.77-1.89× the net10 time for CSE and CFE and 1.51-1.56× for ErrorOr, so on net11 ErrorOr beats CSE on yielding success (2,876.8 vs 3,009.8 ns).

---

## Pending: Sequence, CombineAll, Try and TryAsync

**Status: draft.** These three classes build, and their parity self-check passes on every TFM. No full default-job run has been done, so this section has no numbers yet. The reports will be committed under `benchmarks/results/results-comparison/{tfm}/results/` next to the ones above. Short-job (`--quick`) numbers are not published.

The rules are the same as for the nine classes above: `[MemoryDiagnoser]`, CSE as the baseline, a `[GlobalSetup]` parity check (also run by `--self-check`), and a doc URL above every library row.

| Class | Measured unit | Rows | Label |
|---|---|---|---|
| `SequenceComparison` | `Sequence`: N typed results to one result with every value. `CombineAll`: N untyped results to one result. `N` = 10 or 100, `Succeed` = true, or false with one failure at index N/2. Values are `0..N-1` | CSE `Sequence()` and `CombineAll()`; CFE `Combine` plus `.Map(ToArray)` and `Result.Combine`; ErrorOr a hand-written loop into `List<int>` and `List<Error>`; FluentResults `Result.Merge` | CSE vs FluentResults like-for-like. CFE labelled (adds `ToArray`). ErrorOr labelled (README manual idiom) |
| `TryComparison` | `int.Parse` of `"42"` or `"x"` through a delegate built once in setup | CSE `Result.Try` with a cached error; CSE `Result.Try` with `Error.Exception(ex)`; CFE `Result.Try`; FluentResults `Result.Try`; ErrorOr a hand-written `try`/`catch` | CSE, CFE and FluentResults like-for-like (each handler returns a cached error). The `Error.Exception` row is the CSE Readme `#safe-execution` idiom. The cached-error baseline is there to match CFE and FluentResults. The `Error.Exception` row and the ErrorOr row are labelled |
| `TryAsyncComparison` | the same parse in an `async Task<int>`. `Yield` = false completes synchronously, true awaits `Task.Yield()` first | CSE `Result.TryAsync`; CFE `Result.Try`; FluentResults `Result.Try`; ErrorOr a hand-written async `try`/`catch` | CSE, CFE and FluentResults like-for-like. ErrorOr labelled. Only the cached-error handler is measured, because the `Error.Exception` cost is the one measured in `TryComparison` |

Parity keys: on success the value count and sum (`ok:<count>:<sum>`) or `ok:42`; on failure `fail:1:Order is invalid`. The `Error.Exception` row is checked for one error with code `FormatException`.

Semantic differences:
- CFE `Combine` returns a lazy `IEnumerable<T>`. The row adds `ToArray`, so every column hands back materialized values.
- FluentResults `Merge` returns its values as a `List<T>` behind `IEnumerable<T>`. CSE `Sequence` returns `T[]`.
- ErrorOr has no Sequence, Combine or Try. Its rows follow the README: a manual error list, and a `try`/`catch` that returns an error.
- CFE keeps one string error; FluentResults `Result.Try` and CFE `Result.Try` take an optional handler, and CSE requires one.
- `Error.Exception` builds three metadata entries, including the stack trace text, so on failure it adds the cost measured by `ErrorExceptionComparison` in the Errors suite.

Feature matrix for these classes, from reading each library's source and docs (not measured):

| Feature | CSE | ErrorOr 2.1.1 | FluentResults 4.0.0 | CFE 3.7.0 |
|---|---|---|---|---|
| Sequence of typed results | `Sequence()` gives `T[]` | manual list | `Merge` gives a `List<T>` | `Combine` gives a lazy `IEnumerable<T>` |
| Try / TryAsync | `Result.Try` and `TryAsync`; the handler is required, and a cancelled token propagates instead of becoming an error | none | `Result.Try`, optional handler | `Result.Try`, optional handler |

---

## Feature matrix

CSE cells come from this repository (each csproj, `Directory.Build.props`, `Readme.MD`, `ResultT.cs`, `CSharpEssentials.Errors/ErrorMetadata.cs`, `CSharpEssentials.Json/Extensions.cs`). Competitor links point at the pinned tags: CFE `v3.7.0`, ErrorOr `v2.1.1` (repository now at `error-or/error-or`), FluentResults `v4.0` (no `v4.0.0` tag; commit matches the 4.0.0 nuspec). Nothing was trim-analyzed or AOT-published for this table.

| Feature | CSharpEssentials.Results 7.0.0 | CSharpFunctionalExtensions 3.7.0 | ErrorOr 2.1.1 | FluentResults 4.0.0 |
|---|---|---|---|---|
| Runtime reflection | Result and Error core: none. `Result<T>.ToString()` on success calls `ConvertToJson`, reflection-based System.Text.Json annotated `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]` (`ResultT.cs:89`, `CSharpEssentials.Json/Extensions.cs:30-32`). The Json dependency's converter factories use `MakeGenericType` | Result core: none. JSON converter factories use `MakeGenericType` and `Activator.CreateInstance` ([ResultJsonOfTConverter.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Json/Serialization/ResultJsonOfTConverter.cs)); `EnumValueObject` uses `GetFields` ([EnumValueObject.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/ValueObject/EnumValueObject.cs)) | None found in source ([ErrorOr.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.cs)) | None beyond `GetType()` for reason labels ([Error.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Reasons/Error.cs)) |
| Source generator | None in Results. The Enums generator and analyzers flow in transitively through Errors (`ErrorType` is a `[StringEnum]`) | No | No | No |
| Native AOT annotated (`IsAotCompatible`) / trim warnings | Not declared on Results, Errors or Maybe, so the trim analyzer does not run on them; the `ToString()` call above would warn if it did. Enums and Json declare it on net8+. Known gap | Not declared ([csproj](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/CSharpFunctionalExtensions.csproj), [Common.Build.props](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/Common.Build.props)) | Not declared ([csproj](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr.csproj)) | Not declared ([csproj](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/FluentResults.csproj)) |
| Extra runtime dependencies | CSharpEssentials.Errors and CSharpEssentials.Json; Errors brings Core and Enums (+ System.Text.Json on netstandard2.1); Json brings Enums. Maybe adds Results and Errors | None on any TFM ([nuspec](https://api.nuget.org/v3-flatcontainer/csharpfunctionalextensions/3.7.0/csharpfunctionalextensions.nuspec)) | None on net8/net10; Microsoft.Bcl.HashCode and System.Memory on netstandard2.0 ([nuspec](https://api.nuget.org/v3-flatcontainer/erroror/2.1.1/erroror.nuspec)) | Microsoft.Extensions.Logging.Abstractions 2.1.1 and System.Threading.Tasks.Extensions 4.5.4 on every TFM ([nuspec](https://api.nuget.org/v3-flatcontainer/fluentresults/4.0.0/fluentresults.nuspec)) |
| Base class required | No. `Result`/`Result<T>` are `readonly record struct`; errors are built with `Error` factories, no subclassing | No. Results are `readonly struct`; `Result<T, E>` takes any `E` ([ResultTE.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultTE.cs)) | No. `ErrorOr<T>` is a `readonly record struct`; custom kinds via `Error.Custom` ([Error.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/Errors/Error.cs)) | No for results (classes). Custom errors derive from `Error` or implement `IError` ([README](https://github.com/altmann/FluentResults/blob/v4.0/README.md#designing-errors-and-success-messages)) |
| Error type model | Typed `Error` struct: Code, Description, `ErrorType` (8 values), NumericType, `ErrorMetadata` (`Dictionary<string, object?>`) | `string` by default (several messages joined with ", "); typed `E` through `Result<T, E>` ([ResultT.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultT.cs)) | Typed `Error` struct: Code, Description, `ErrorType` (7 values), NumericType, `Dictionary<string, object>` metadata ([Error.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/Errors/Error.cs), [ErrorType.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/Errors/ErrorType.cs)) | `IError` class hierarchy with Message, metadata and nested `Reasons` ([Error.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Reasons/Error.cs)) |
| Multiple errors per result | Yes (`Error[]`) | No: one string or one `E`; `Combine` concatenates strings ([Combine.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Combine.cs)) | Yes (`List<Error>`) ([ErrorOr.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.cs)) | Yes (`List<IReason>`) ([ResultBase.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Results/ResultBase.cs)) |
| Maybe / Option type | Yes, separate package CSharpEssentials.Maybe (`readonly struct Maybe<T>`) | Yes, in the package ([Maybe.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Maybe/Maybe.cs)) | No | No |
| Async extension coverage | Map, Bind, Tap, Ensure, Match with Task and ValueTask overloads on every TFM including netstandard2.1 (Readme, "Async overload matrix") | Map, Bind, Tap, Ensure, Match on Task (left, right, both); ValueTask only under `NET5_0_OR_GREATER` ([Extensions](https://github.com/vkhorikov/CSharpFunctionalExtensions/tree/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions)) | Task only: ThenAsync, ThenDoAsync, FailIfAsync, MatchAsync ([ThenExtensions](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.ThenExtensions.cs)) | Task and ValueTask for Bind, Map and Try; no Tap, Ensure or Match ([ResultExtensions.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Extensions/ResultExtensions.cs), [Results.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Factories/Results.cs) for `Try`) |
| Implicit conversions | `T`, `Error`, `Error[]`, `List<Error>`, `HashSet<Error>` → `Result<T>`; `bool` → `Result` (Readme, "Returning Failures") | `T` → `Result<T>`; `T` and `E` → `Result<T, E>`; no `string` → `Result<T>` ([ResultT.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultT.cs), [ResultTE.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultTE.cs)) | `T`, `Error`, `List<Error>`, `Error[]` → `ErrorOr<T>` ([ImplicitConverters](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.ImplicitConverters.cs)) | `T`, `Error`, `List<Error>` → `Result<T>`; `Error`, `List<Error>` → `Result` ([Result.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Results/Result.cs)) |

On the AOT row the four libraries are equal on paper: none declares `IsAotCompatible`. CSE's gap is that its own surface still contains a reflection-based call (`ToString()`), an `object?` metadata bag, and a five-package dependency chain, against zero dependencies for CFE. No fix is part of this benchmark work.

---

## Caveats

- Every CFE number uses its default `Result<T>` with a `string` error. That is why CFE allocates 0 B on Create and Ensure failure and less on the async failure path. A `Result<T, E>` column with a typed error was not benchmarked, so the cost of typed errors in CFE is unknown here.
- The aggregation idioms differ. ErrorOr's documented aggregation builds no per-check results; the other three do. The ErrorOr row measures less work, by design of its idiom. CSE's typed `Result<T>.Combine` was not measured.
- FluentResults has no Ensure, Tap or Maybe and only two of the five async steps, so it has no column there. Its Match column uses property access, the README's processing idiom. ErrorOr and FluentResults have no Maybe.
- Many rows are under 2 ns. Differences under 1 ns are reported as noise. Near-zero Maybe rows are indistinguishable from an empty method.
- CFE's 7-9 ns on net8/net9 against about 2 ns on net10/net11, the 4.9-7.1 ns Maybe rows on net8/net9, and the 1.5-1.9× slower yielding rows on net11 affect libraries as a group and were not investigated. They are reported, not explained.
- net8 uses CSE's netstandard2.1 build. Results, Errors and Maybe do not target net8.0, so the net8 column runs CSE's netstandard2.1 assembly against competitor assets built for net8. net8 gaps may reflect that build, not only the runtime.
- net9 aggregation outlier: CSE all-fail took 423.2 ns on net9 against 214-238 ns on the other TFMs, with the same allocation.
- Machine load: load average 0.96-1.44 at the sampled points with a desktop session running. Lower than the endpoints run, but gaps of a few percent on the async rows are still weak evidence even when outside error.
- The CSE column is the 7.0.0 working tree on branch `perf/bench-results`, which may differ from the last version published to NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project results-comparison                       # all TFMs (net8-net11), all classes, default job
./run-benchmarks.sh --project results-comparison --tfm net10.0         # one TFM, default job
./run-benchmarks.sh --project results-comparison --quick --tfm net10.0 # one TFM, all classes, Short job
```

`--quick` without `--tfm` runs on net9.0, the script default for this suite, and uses BenchmarkDotNet's Short job, so its numbers are not comparable with the committed ones. Results are stored per framework under `benchmarks/results/results-comparison/{tfm}/results/` (GitHub Markdown, CSV, JSON and HTML).
