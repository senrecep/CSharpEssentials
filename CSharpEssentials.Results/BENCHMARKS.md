# CSharpEssentials.Results: Performance benchmarks

Compared with CSharpFunctionalExtensions 3.7.0, ErrorOr 2.1.1 and FluentResults 4.0.0 · BenchmarkDotNet v0.14.0 · 6-core, 12-thread x64 desktop CPU · .NET 8 / 9 / 10 / 11

This page has two suites. The comparison suite (`results-comparison`) measures CSE against the three libraries; its Result and Maybe classes are below, the `Error` classes are in [CSharpEssentials.Errors/BENCHMARKS.md](../CSharpEssentials.Errors/BENCHMARKS.md), and the Maybe collection helpers are in [CSharpEssentials.Maybe/BENCHMARKS.md](../CSharpEssentials.Maybe/BENCHMARKS.md). The async overload suite (`results-pattern`) measures CSE's own async overloads against hand-written code, in [Async overload matrix](#async-overload-matrix-results-pattern-suite).

---

## What was measured

Twelve Result and Maybe classes in `benchmarks/CSharpEssentials.Results.Comparison.Benchmarks/`: the nine scenarios in the table below, plus `SequenceComparison`, `TryComparison` and `TryAsyncComparison` in [Sequence, CombineAll, Try and TryAsync](#sequence-combineall-try-and-tryasync). Every class uses `[MemoryDiagnoser]`, CSE as the baseline, and a `[Params]` switch for the success and failure path. A `[GlobalSetup]` parity check runs every library on the same input and refuses to start if the outcomes differ (success value, or error count and first message).

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

Every competitor method carries a comment with the README or source URL at the pinned version. Errors are `static readonly` (CFE: a `const string`, its default error type), so error construction is outside the timing except where the operation itself builds the failure.

### Environment

| Property | Value |
|---|---|
| Machine | 6-core, 12-thread x64 desktop CPU (RyuJIT AVX2) |
| OS | CachyOS, Linux kernel 7.3.0-4-cachyos |
| Machine load | Desktop session in the background. 1-minute load average: see [How these numbers were produced](#how-these-numbers-were-produced) |
| Tool | BenchmarkDotNet v0.14.0, [MemoryDiagnoser], default job |
| .NET SDK | 11.0.100-rc.1.26425.128 |
| Runtimes tested | .NET 8.0.30 (CSE: netstandard2.1 build), .NET 9.0.19, .NET 10.0.11, .NET 11.0.0 |
| CSharpFunctionalExtensions (CFE) | 3.7.0, default idiom `Result<T>` with a `string` error |
| ErrorOr | 2.1.1 |
| FluentResults | 4.0.0 |
| CSharpEssentials.Results / Maybe (CSE) | The repository source, referenced as a project |

### How these numbers were produced

- Full BenchmarkDotNet default-job runs (no `--quick`), on net8.0, net9.0, net10.0 and net11.0, one suite at a time on one machine.
- Comparison suite reports: `benchmarks/results/results-comparison/{tfm}/results/`. Async overload suite reports: `benchmarks/results/results-pattern/{tfm}/results/`.
- 1-minute load average, sampled at the start and end of each suite: 1.55 to 2.37 for the comparison suite and 1.41 to 2.65 for the async overload suite. `MaybeComparison` (all four TFMs) and `CreateComparison` and `SequenceComparison` on net11.0 were rerun because their coefficient of variation (CV) stayed high. The reruns saw 1.43 to 4.72; the 4.72 peak was at the start of the net10.0 `MaybeComparison` rerun.
- Rows whose CV stayed above 5% after two reruns are marked "noisy (CV above 5%)". In this suite they are all `MaybeComparison` rows near 0 ns.
- Every number is copied from the `*-report-github.md` files. A `-` in the report's Allocated column is written as `0 B` here.

---

## Executive summary

Primary runtime: net10. A tie is a difference within the sum of the two BenchmarkDotNet error margins. An absolute time difference under 1 ns is reported as noise even when it falls outside error; many of these operations take under 2 ns. Rows below about 0.1 ns are at timer resolution and are not ranked.

| Finding | Details |
|---|---|
| Map, Bind, Tap, Match, Ensure (pass), Create (success): 0 B for CSE, CFE and ErrorOr; FluentResults 88-344 B on net10 | net10: FluentResults Map 256 B / 58.5485 ns (success), Bind 320 B / 73.3675 ns, Create 112 B / 23.8158 ns, Match to string on failure 240 B / 75.4939 ns. CSE, CFE and ErrorOr stay under 2.9 ns. CSE, CFE and ErrorOr allocate the same bytes on every TFM; FluentResults allocates more on net8 (Map success 288 B, Bind success 352 B, Create success 120 B, Match to string on failure 304 B) |
| Create failure: CSE 56 B, ErrorOr 88 B, CFE 0 B. On net10 CSE is slower than ErrorOr (CSE loss) | net10: 27.9547 ns / 56 B for CSE, 19.5903 ns / 88 B for ErrorOr (ratio 0.70), 0.3016 ns / 0 B for CFE, 93.4794 ns / 344 B for FluentResults. On net8, net9 and net11 CSE is faster than ErrorOr (13.8171 vs 19.3543 ns, 13.8671 vs 21.5718 ns, 9.9265 vs 16.9649 ns). CFE stores the string error in its struct and allocates nothing, so CSE loses to CFE on every TFM. The net10 CSE mean is about twice its own net8, net9 and net11 means; see Caveats |
| Ensure failure: CSE beats ErrorOr, loses to CFE | net10: 12.903 ns / 56 B for CSE, 23.776 ns / 88 B for ErrorOr (1.84), 1.807 ns / 0 B for CFE (0.14). Same order on every TFM |
| Map, Bind, Ensure (pass): CSE and ErrorOr within 1 ns; CFE 0.78-1.41 ns slower on net10 | Map success 0.5791 / 0.6358 / 1.8174 ns (CSE / ErrorOr / CFE). On net8 and net9 CFE takes 8.9011 to 9.2949 ns for Map, Bind and Ensure pass, and 5.4266 to 7.4354 ns for Create success, against 0.1520 to 2.0426 ns for CSE on the same rows. On net10 and net11 CFE drops to about 2 ns for Map, Bind and Ensure. This looks like a runtime/JIT effect; not investigated |
| Match: no gap above 1 ns between CSE and ErrorOr; CFE's Match to int about 1.2 ns slower | net10 Match to int: 1.2063 / 1.0570 ns (CSE success / failure), 1.1083 / 1.0181 ns (ErrorOr), 2.3368 / 2.3593 ns (CFE). CFE's Match to int takes 2.3065-2.3593 ns on every TFM. ErrorOr is ahead of CSE on Match to string on failure on net8 and net9 (0.5514 vs 1.0937 ns, 0.6149 vs 0.9161 ns), under 1 ns |
| Async pipeline, success, completed tasks: CSE fastest on every TFM, same allocation as CFE | net10: 89.92 ns / 480 B for CSE, 101.11 ns / 480 B for CFE (1.12), 107.73 ns / 560 B for ErrorOr (1.20) |
| Async pipeline, failure at step 1: CFE allocates 56 B less; CFE faster on net8 to net10 (CSE loss) | Completed, net10: 106.24 ns / 536 B for CSE, 103.39 ns / 480 B for CFE (0.97), 129.79 ns / 648 B for ErrorOr. CFE is faster on net8 (115.5 vs 126.4 ns), net9 (112.7 vs 118.2 ns) and net10; on net11 CSE is faster (101.35 vs 103.56 ns). Yielding failure, net10: 1,614.62 ns / 960 B for CSE vs 1,588.20 ns / 880 B for CFE (CSE loss, outside error); a tie on the other TFMs |
| Async pipeline, yielding success: CSE allocates 24 B more than CFE (CSE loss) | net10: 1,607.53 ns / 904 B for CSE, 1,636.71 ns / 880 B for CFE (CSE faster, outside error), 1,872.45 ns / 1037 B for ErrorOr. Time is a tie on net8, net9 and net11 |
| Error aggregation: ErrorOr faster and smaller on both paths (CSE loss); CSE beats CFE and FluentResults | All-fail, net10: 99.09 ns / 336 B for CSE, 42.79 ns / 184 B for ErrorOr (0.43), 212.03 ns / 712 B for FluentResults (2.14), 368.58 ns / 784 B for CFE (3.72). All-pass: 14.51 ns / 48 B for CSE, 11.63 ns / 32 B for ErrorOr (0.80). Same order on every TFM. ErrorOr's idiom does less work (see section 8) |
| Maybe: tie with CFE | Both allocate 0 B in every operation. Every net10 mean is at most 1.1824 ns and every measurable CSE vs CFE gap on net10 is under 0.1 ns. On net9, CSE Bind on None is 1.27 ns faster (5.5385 vs 6.8108 ns). Create and GetValueOrDefault on None are at timer resolution and partly noisy |
| Sequence and CombineAll: ErrorOr's hand-written loop is faster (CSE loss); CSE beats CFE and FluentResults | net10, N = 100, all success: `Sequence` 268.480 ns / 1640 B for CSE, 229.394 ns / 1216 B for ErrorOr (0.85), 854.432 ns / 2344 B for CFE, 2,311.158 ns / 6528 B for FluentResults. `CombineAll` allocates the least of the four on every row (0 B on success, 56 B on failure) |
| Try and TryAsync: the exception dominates; CSE and the other libraries tie or differ by a few percent | net10 `Try` failure: 2,399.324 ns / 520 B for CSE, 2,452.483 ns / 464 B for CFE, 2,352.819 ns / 552 B for ErrorOr, 2,642.237 ns / 832 B for FluentResults. `TryAsync` failure, completed: CFE is faster than CSE on net9 to net11 (net10 5,076.65 vs 5,385.12 ns) and allocates 56 B less |
| AOT and dependencies | Results, Errors and Maybe declare `IsAotCompatible` on their net9.0+ targets, not on netstandard2.1 (as do Core, Enums and Json), so the trim analyzer runs on them at build time. None of the three competitors declares it. No trim or AOT publish was run for this page. `Result<T>.ToString()` formats the value with `IFormattable` and the invariant culture, without JSON. `ErrorMetadata` stores `object?` values. Results pulls in Errors and Json, which bring Core and Enums; CFE has no runtime dependencies |

### Where CSE loses

Every row where a competitor's mean is lower than CSE's by more than the two error margins, or where it allocates fewer bytes. Rows from the Error, Maybe collection and async overload classes are listed on their own pages and in the async overload section below.

- **Create failure**: CFE on every TFM (net10: 0.3016 ns / 0 B vs 27.9547 ns / 56 B; net11 CFE is at timer resolution, 0.0000 ns). ErrorOr on net10 time only (19.5903 vs 27.9547 ns; ErrorOr allocates 88 B against CSE's 56 B).
- **Ensure failure**: CFE on every TFM (net10: 1.807 ns / 0 B vs 12.903 ns / 56 B).
- **Async pipeline**: CFE allocates less on every failure and yielding row: 480 B vs 536 B (failure, completed), 880 B vs 960 B (failure, yielding), 880 B vs 904 B (success, yielding), all TFMs. CFE is faster on completed failure on net8 (115.5 vs 126.4 ns), net9 (112.7 vs 118.2 ns) and net10 (103.39 vs 106.24 ns), and on yielding failure on net10 (1,588.20 vs 1,614.62 ns).
- **Error aggregation**: ErrorOr on both paths, every TFM (net10 all-fail 42.79 ns / 184 B vs 99.09 ns / 336 B; all-pass 11.63 ns / 32 B vs 14.51 ns / 48 B).
- **Sequence**: ErrorOr on every row and TFM, in time and bytes (net10 N = 10 failure 82.588 ns / 400 B vs 118.384 ns / 488 B). FluentResults allocates less on N = 100 failure (net10 1288 B vs 1456 B) while taking 945.368 vs 484.463 ns.
- **CombineAll**: ErrorOr on time at N = 100 on every TFM (net10 failure 69.222 vs 137.997 ns, success 37.141 vs 67.625 ns) and at N = 10 failure on net11 (26.990 vs 38.115 ns); CFE on N = 100 success on net10 (62.774 vs 67.625 ns) and net11 (64.439 vs 68.541 ns). CSE allocates the least on all of these rows.
- **Try**: ErrorOr's hand-written `try`/`catch` on failure on net10 (2,352.819 vs 2,399.324 ns) and net11 (1,318.470 vs 1,339.987 ns), and on success on net8 (6.586 vs 7.838 ns). CFE allocates 56 B less on failure on net9 to net11 (net10 464 B vs 520 B).
- **TryAsync**: CFE on completed failure on net9 to net11 (net10 5,076.65 vs 5,385.12 ns) and ErrorOr on the same row (net10 5,217.23 ns). CFE allocates 56-64 B less on every failure row and up to 9 B less on yielding success; ErrorOr allocates 23-25 B less on yielding success. ErrorOr is faster on completed success on net8, net9 and net11 (net11 31.48 vs 33.61 ns).
- **`ErrorToStringComparison` `Result` row**: CFE allocates 48 B against CSE's 72 B on net8 (CSE is faster: 16.03 vs 51.51 ns).
- **Measured, under 1 ns** (outside error but below the noise threshold): ErrorOr on Map, Bind, Ensure pass, Match and Tap rows by 0.01-0.54 ns on some TFMs (largest: Match to string on failure on net8, 0.5514 vs 1.0937 ns); CFE on many measurable Maybe rows on net8 to net11 by 0.003-0.15 ns (rows at timer resolution excluded; largest: net9 Where Some, 0.8730 vs 1.0273 ns, and net9 Map None, 0.5743 vs 0.6797 ns); ErrorOr on `Try` success on net9 to net11 (net10 6.048 vs 7.008 ns) and `TryAsync` completed success on net10 (33.64 vs 34.40 ns).

---

## Results by scenario

All tables: net10, mean and allocated bytes per operation. "Ratio" is the report's Ratio column (library mean divided by CSE mean; above 1.00 = slower than CSE). A dash means the library has no column in that class (reason in the What was measured table).

### 1. Create

| Library | Success mean | Success alloc | Failure mean | Failure alloc | Failure ratio |
|---|---:|---:|---:|---:|---:|
| CSE | 0.2711 ns | 0 B | 27.9547 ns | 56 B | 1.00 |
| CFE (string error) | 0.2907 ns | 0 B | 0.3016 ns | 0 B | 0.01 |
| ErrorOr | 0.2708 ns | 0 B | 19.5903 ns | 88 B | 0.70 |
| FluentResults | 23.8158 ns | 112 B | 93.4794 ns | 344 B | 3.34 |

Success is free for the three struct-based libraries. On failure CSE allocates 56 B and ErrorOr 88 B (a `List<Error>`); CFE's default `Result<T>` keeps the string inline. A CFE `Result<T, E>` column with a typed error was not benchmarked.

### 2. Map (`x => x * 2`)

| Library | Success mean | Success alloc | Failure mean | Failure alloc |
|---|---:|---:|---:|---:|
| CSE | 0.5791 ns | 0 B | 0.8850 ns | 0 B |
| CFE | 1.8174 ns | 0 B | 1.9766 ns | 0 B |
| ErrorOr | 0.6358 ns | 0 B | 0.8659 ns | 0 B |
| FluentResults | 58.5485 ns | 256 B | 109.2260 ns | 344 B |

CSE and ErrorOr are within 0.06 ns. FluentResults returns a new class instance on both paths.

### 3. Bind / Then (`x => Ok(x + 1)`)

| Library | Success mean | Success alloc | Failure mean | Failure alloc |
|---|---:|---:|---:|---:|
| CSE | 0.5813 ns | 0 B | 0.8944 ns | 0 B |
| CFE | 1.9860 ns | 0 B | 1.9525 ns | 0 B |
| ErrorOr | 0.5817 ns | 0 B | 0.8771 ns | 0 B |
| FluentResults | 73.3675 ns | 320 B | 72.7987 ns | 208 B |

### 4. Ensure (`x > 0`)

| Library | Pass mean | Pass alloc | Fail mean | Fail alloc | Fail ratio |
|---|---:|---:|---:|---:|---:|
| CSE | 1.567 ns | 0 B | 12.903 ns | 56 B | 1.00 |
| CFE (string error) | 2.346 ns | 0 B | 1.807 ns | 0 B | 0.14 |
| ErrorOr (`FailIf`) | 1.157 ns | 0 B | 23.776 ns | 88 B | 1.84 |
| FluentResults | - | - | - | - | - |

The failing case turns a success into a failure, so it costs what Create failure costs. CSE allocates less than ErrorOr; CFE's 0 B comes from the string error.

### 5. Match

| Library | ToInt success | ToInt failure | ToString success | ToString failure | Alloc |
|---|---:|---:|---:|---:|---:|
| CSE | 1.2063 ns | 1.0570 ns | 2.6708 ns | 0.7223 ns | 0 B |
| CFE | 2.3368 ns | 2.3593 ns | 2.8218 ns | 1.5866 ns | 0 B |
| ErrorOr | 1.1083 ns | 1.0181 ns | 2.6557 ns | 1.0124 ns | 0 B |
| FluentResults (properties idiom) | 24.6687 ns | 21.7977 ns | 23.2173 ns | 75.4939 ns | 88-240 B |

CFE's ToInt rows are about 1.2 ns slower than CSE and ErrorOr. The other net10 differences are under 1 ns. FluentResults allocates even when only reading properties (its `Errors` is computed with `Reasons.OfType<IError>()`).

### 6. Tap

| Library | Success mean | Failure mean | Alloc |
|---|---:|---:|---:|
| CSE | 1.6085 ns | 0.5006 ns | 0 B |
| CFE | 1.7265 ns | 1.1046 ns | 0 B |
| ErrorOr (`ThenDo`) | 1.5741 ns | 0.8163 ns | 0 B |

All differences under 1 ns: noise. On net8 ErrorOr's Tap on failure takes 2.0245 ns against 0.4930 ns for CSE.

### 7. Async 5-step pipeline (Bind async → Map → Ensure → Tap → Map)

| Path | CSE | CFE | ErrorOr | CFE ratio (time) |
|---|---|---|---|---|
| Success, completed | 89.92 ns / 480 B | 101.11 ns / 480 B | 107.73 ns / 560 B | 1.12 |
| Success, yielding | 1,607.53 ns / 904 B | 1,636.71 ns / 880 B | 1,872.45 ns / 1037 B | 1.02 |
| Failure at step 1, completed | 106.24 ns / 536 B | 103.39 ns / 480 B | 129.79 ns / 648 B | 0.97 |
| Failure at step 1, yielding | 1,614.62 ns / 960 B | 1,588.20 ns / 880 B | 1,869.76 ns / 1127 B | 0.98 |

- CSE is fastest on the success path with completed tasks, on every TFM.
- CFE allocates less than CSE on the failure path (56 B and 80 B less) and on yielding success (24 B less). Part of that is the error representation: CFE carries one string, CSE an error array built at step 1.
- ErrorOr allocates the most on every path and is the slowest on every path except the net11 yielding ones, where all three are within error.

### 8. Error aggregation (three independent checks)

> The idioms differ, so the work differs. ErrorOr's README idiom is a hand-filled `List<Error>`: three `if` statements, no intermediate result objects. CSE (`new[] { Result.FailureIf(...) × 3 }.CombineAll()`), CFE (`Result.Combine(Result.FailureIf(...) × 3)`) and FluentResults (`Result.Merge(Result.FailIf(...) × 3)`) build one result per check and then combine them. CFE's `Combine` joins string messages with ", " and collapses a repeated message into one entry suffixed with its count (`message (N×)`). CSE's typed `Result<T>.Combine(r1, r2, r3)` skips the array and list but was not benchmarked.

| Library | All-pass mean | All-pass alloc | All-fail mean | All-fail alloc | All-fail ratio |
|---|---:|---:|---:|---:|---:|
| CSE (`CombineAll`) | 14.51 ns | 48 B | 99.09 ns | 336 B | 1.00 |
| CFE (`Result.Combine`, strings) | 31.65 ns | 160 B | 368.58 ns | 784 B | 3.72 |
| ErrorOr (manual `List<Error>`) | 11.63 ns | 32 B | 42.79 ns | 184 B | 0.43 |
| FluentResults (`Result.Merge`) | 106.85 ns | 368 B | 212.03 ns | 712 B | 2.14 |

CSE loses to ErrorOr on both paths and beats CFE and FluentResults on both paths, on every TFM.

### 9. Maybe (CSE `Maybe<T>` vs CFE `Maybe<T>`)

| Operation | Some: CSE | Some: CFE | None: CSE | None: CFE |
|---|---:|---:|---:|---:|
| From / None | 0.0003 ns (noisy, CV above 5%) | 0.4084 ns (noisy, CV above 5%) | 0.0001 ns (noisy, CV above 5%) | 0.0006 ns (noisy, CV above 5%) |
| Map | 0.7175 ns | 0.6285 ns | 0.3868 ns | 0.3576 ns |
| Bind | 0.6781 ns | 0.6284 ns | 0.3665 ns | 0.3642 ns |
| Where | 1.0175 ns | 1.0015 ns | 0.4920 ns | 0.4346 ns |
| GetValueOrDefault | 0.3026 ns | 0.2896 ns | 0.0122 ns (noisy, CV above 5%) | 0.0020 ns (noisy, CV above 5%) |
| Match | 1.1824 ns | 1.1100 ns | 1.0639 ns | 1.0473 ns |

Every row allocates 0 B for both. The From / None row and GetValueOrDefault on None are at timer resolution (CFE From on Some, 0.4084 ns, is noisy) and cannot be told apart; the BenchmarkDotNet ratios for those rows are meaningless and not quoted. The same holds on the other TFMs for Create on None (net8, net9, net11), GetValueOrDefault on None (net9) and on Some (net11), and Create on Some (net11); the net8 Create None, net9 GetValueOrDefault None and net11 GetValueOrDefault Some rows are also noisy (CV above 5%). On net8 and net9, Map and Bind on Some, Create on Some and Bind on None take 4.8533 to 7.1051 ns for both libraries; on net9 CSE's Bind on None is 1.27 ns faster (5.5385 vs 6.8108 ns). The other gaps are under 0.2 ns.

---

## Cross-TFM

Mean / allocated per TFM for the rows where libraries differ. Map, Bind, Tap, Match and Maybe allocate the same bytes on every TFM (0 B except FluentResults) and are left out.

| Scenario | Library | net8 | net9 | net10 | net11 |
|---|---|---|---|---|---|
| Create failure | CSE | 13.8171 ns / 56 B | 13.8671 ns / 56 B | 27.9547 ns / 56 B | 9.9265 ns / 56 B |
| | CFE | 7.4449 ns / 0 B | 7.3223 ns / 0 B | 0.3016 ns / 0 B | 0.0000 ns / 0 B (timer resolution) |
| | ErrorOr | 19.3543 ns / 88 B | 21.5718 ns / 88 B | 19.5903 ns / 88 B | 16.9649 ns / 88 B |
| | FluentResults | 148.2368 ns / 384 B | 129.2978 ns / 344 B | 93.4794 ns / 344 B | 90.7117 ns / 344 B |
| Ensure failure | CSE | 14.817 ns / 56 B | 13.780 ns / 56 B | 12.903 ns / 56 B | 10.647 ns / 56 B |
| | CFE | 8.411 ns / 0 B | 8.452 ns / 0 B | 1.807 ns / 0 B | 1.802 ns / 0 B |
| | ErrorOr | 19.726 ns / 88 B | 22.502 ns / 88 B | 23.776 ns / 88 B | 17.045 ns / 88 B |
| Map success | CSE | 0.8658 ns / 0 B | 0.8478 ns / 0 B | 0.5791 ns / 0 B | 0.5651 ns / 0 B |
| | CFE | 8.9349 ns / 0 B | 9.1264 ns / 0 B | 1.8174 ns / 0 B | 1.7713 ns / 0 B |
| | ErrorOr | 0.7119 ns / 0 B | 0.8482 ns / 0 B | 0.6358 ns / 0 B | 0.5745 ns / 0 B |
| | FluentResults | 139.7369 ns / 288 B | 105.4259 ns / 256 B | 58.5485 ns / 256 B | 60.6824 ns / 256 B |
| Async success, completed | CSE | 107.8 ns / 480 B | 102.2 ns / 480 B | 89.92 ns / 480 B | 85.17 ns / 480 B |
| | CFE | 120.3 ns / 480 B | 115.3 ns / 480 B | 101.11 ns / 480 B | 100.81 ns / 480 B |
| | ErrorOr | 130.9 ns / 560 B | 124.0 ns / 560 B | 107.73 ns / 560 B | 105.79 ns / 560 B |
| Async success, yielding | CSE | 1,672.1 ns / 904 B | 1,653.0 ns / 904 B | 1,607.53 ns / 904 B | 3,098.28 ns / 904 B |
| | CFE | 1,684.8 ns / 880 B | 1,644.0 ns / 880 B | 1,636.71 ns / 880 B | 3,114.88 ns / 880 B |
| | ErrorOr | 1,923.3 ns / 1037 B | 1,901.9 ns / 1037 B | 1,872.45 ns / 1037 B | 3,116.52 ns / 1040 B |
| Async failure, completed | CSE | 126.4 ns / 536 B | 118.2 ns / 536 B | 106.24 ns / 536 B | 101.35 ns / 536 B |
| | CFE | 115.5 ns / 480 B | 112.7 ns / 480 B | 103.39 ns / 480 B | 103.56 ns / 480 B |
| | ErrorOr | 150.3 ns / 648 B | 145.5 ns / 648 B | 129.79 ns / 648 B | 123.30 ns / 648 B |
| Async failure, yielding | CSE | 1,658.3 ns / 960 B | 1,611.2 ns / 960 B | 1,614.62 ns / 960 B | 3,128.82 ns / 960 B |
| | CFE | 1,640.8 ns / 880 B | 1,598.8 ns / 880 B | 1,588.20 ns / 880 B | 3,080.85 ns / 880 B |
| | ErrorOr | 1,920.0 ns / 1127 B | 1,906.4 ns / 1127 B | 1,869.76 ns / 1127 B | 3,149.58 ns / 1128 B |
| Aggregation all-fail | CSE | 116.756 ns / 336 B | 108.90 ns / 336 B | 99.09 ns / 336 B | 81.97 ns / 336 B |
| | CFE | 458.489 ns / 800 B | 464.36 ns / 784 B | 368.58 ns / 784 B | 356.16 ns / 784 B |
| | ErrorOr | 47.569 ns / 184 B | 46.15 ns / 184 B | 42.79 ns / 184 B | 36.77 ns / 184 B |
| | FluentResults | 292.322 ns / 712 B | 257.75 ns / 712 B | 212.03 ns / 712 B | 197.59 ns / 712 B |
| Aggregation all-pass | CSE | 17.040 ns / 48 B | 16.71 ns / 48 B | 14.51 ns / 48 B | 13.91 ns / 48 B |
| | CFE | 54.476 ns / 160 B | 53.38 ns / 160 B | 31.65 ns / 160 B | 33.03 ns / 160 B |
| | ErrorOr | 8.318 ns / 32 B | 11.81 ns / 32 B | 11.63 ns / 32 B | 11.36 ns / 32 B |
| | FluentResults | 151.108 ns / 368 B | 140.42 ns / 368 B | 106.85 ns / 368 B | 97.63 ns / 368 B |

- The allocation findings hold on every TFM: CSE allocates more than CFE on Create/Ensure failure (+56 B), async failure (+56 B completed, +80 B yielding) and yielding success (+24 B), and more than ErrorOr on aggregation (+16 B all-pass, +152 B all-fail). CSE allocates less than ErrorOr on Create/Ensure failure (56 B vs 88 B).
- The net10 Create failure mean for CSE (27.9547 ns) is about twice the net8, net9 and net11 means with the same 56 B. Ensure failure, which builds the same failure, takes 12.903 ns on net10. Not investigated; treat it as suspect.
- The yielding rows on net11 take 1.90-1.94× the net10 time for CSE and CFE and 1.66-1.68× for ErrorOr. On net11 the three libraries are within error on both yielding paths.

---

## Sequence, CombineAll, Try and TryAsync

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

### Sequence and CombineAll (net10)

| Operation | N | Succeed | CSE | CFE | ErrorOr | FluentResults |
|---|---:|---|---|---|---|---|
| `CombineAll` | 10 | false | 31.122 ns / 56 B | 183.490 ns / 536 B | 32.546 ns / 184 B | 144.540 ns / 248 B |
| `CombineAll` | 10 | true | 8.727 ns / 0 B | 29.557 ns / 88 B | 11.183 ns / 32 B | 99.212 ns / 152 B |
| `CombineAll` | 100 | false | 137.997 ns / 56 B | 211.938 ns / 536 B | 69.222 ns / 184 B | 744.986 ns / 248 B |
| `CombineAll` | 100 | true | 67.625 ns / 0 B | 62.774 ns / 88 B | 37.141 ns / 32 B | 606.378 ns / 152 B |
| `Sequence` | 10 | false | 118.384 ns / 488 B | 299.347 ns / 848 B | 82.588 ns / 400 B | 265.587 ns / 568 B |
| `Sequence` | 10 | true | 68.816 ns / 312 B | 161.258 ns / 544 B | 54.330 ns / 248 B | 386.860 ns / 1128 B |
| `Sequence` | 100 | false | 484.463 ns / 1456 B | 868.817 ns / 2288 B | 364.533 ns / 1368 B | 945.368 ns / 1288 B |
| `Sequence` | 100 | true | 268.480 ns / 1640 B | 854.432 ns / 2344 B | 229.394 ns / 1216 B | 2,311.158 ns / 6528 B |

- ErrorOr's hand-written loop is faster than CSE `Sequence` on every row and TFM and allocates 64-456 B less. On net8 and net9 `Sequence` with N = 10 success allocates 344 B for CSE (312 B on net10 and net11), and N = 100 success 1672 B (1640 B).
- `CombineAll` allocates the least of the four on every row. ErrorOr is faster at N = 100 on every TFM; at N = 10 CSE is faster than or ties ErrorOr, except on net11 failure (38.115 vs 26.990 ns).
- CFE `Combine` at N = 100 success is faster than CSE on net10 and net11 (62.774 vs 67.625 ns, 64.439 vs 68.541 ns) and slower on net8 and net9 (171.027 and 84.664 ns).
- Full tables for every TFM: [`SequenceComparison` net10 report](../benchmarks/results/results-comparison/net10.0/results/CSharpEssentials.Results.Comparison.Benchmarks.SequenceComparison-report-github.md).

### Try (net10)

| Row | Success | Failure | Failure ratio |
|---|---|---|---:|
| CSE `Result.Try`, cached error | 7.008 ns / 0 B | 2,399.324 ns / 520 B | 1.00 |
| CSE `Result.Try`, `Error.Exception(ex)` | 6.959 ns / 0 B | 7,742.162 ns / 5569 B | 3.23 |
| CFE `Result.Try` | 7.735 ns / 0 B | 2,452.483 ns / 464 B | 1.02 |
| ErrorOr hand-written `try`/`catch` | 6.048 ns / 0 B | 2,352.819 ns / 552 B | 0.98 |
| FluentResults `Result.Try` | 29.261 ns / 112 B | 2,642.237 ns / 832 B | 1.10 |

The throw dominates the failure rows, and its cost drops with each runtime: CSE failure takes 8,672.053 ns on net8, 3,879.014 ns on net9, 2,399.324 ns on net10 and 1,339.987 ns on net11. `Error.Exception(ex)` adds 5,342.838 ns and 5,049 B over the cached-error row on net10 (3.98× on net11), because it copies the stack trace text into metadata.

### TryAsync (net10)

| Succeed | Yield | CSE | CFE | ErrorOr | FluentResults |
|---|---|---|---|---|---|
| true | false | 34.40 ns / 152 B | 36.56 ns / 152 B | 33.64 ns / 152 B | 58.34 ns / 256 B |
| true | true | 823.95 ns / 239 B | 843.04 ns / 231 B | 824.57 ns / 216 B | 851.43 ns / 328 B |
| false | false | 5,385.12 ns / 1512 B | 5,076.65 ns / 1456 B | 5,217.23 ns / 1544 B | 5,576.34 ns / 1816 B |
| false | true | 6,718.95 ns / 1601 B | 6,591.55 ns / 1537 B | 6,604.87 ns / 1609 B | 6,896.05 ns / 1889 B |

CSE is slower than CFE and ErrorOr on completed failure on net9 to net11, and allocates 56-64 B more than CFE on every failure row. On net8 the four completed failure rows take 19,264.78 ns (CSE) to 20,778.00 ns (FluentResults); the exception cost there dwarfs the library code.

---

## Async overload matrix (results-pattern suite)

`benchmarks/CSharpEssentials.Results.Benchmarks/` measures CSE's own async overloads, with no competitor columns. Each class (`Map`, `Bind`, `Ensure`, `Else`, `Match`, `Tap`, `TapError`, `Then`) calls the operation on three source shapes (a `Result<T>` instance, a `Task<Result<T>>` and a `ValueTask<Result<T>>`) with sync, `Task` and `ValueTask` handlers, against a hand-written `await` plus `if` baseline. `Succeed` picks the path; `Yield = false` uses completed tasks, `Yield = true` completes every awaitable after `Task.Yield()`. `RenamedMemberBenchmarks` checks that the obsolete pre-6.4 names cost the same as their `Async` replacements, and `TraverseBenchmarks` measures `TraverseAsync` and `SequenceAsync` over 16 or 256 items. The `Instance_ValueTask*` and `TraverseAsync_ValueTaskSelector` rows need net9.0 or later, so net8 has no such rows.

### Bind (net10)

| Row | Success, completed | Success, yielding | Failure, completed | Failure, yielding |
|---|---|---|---|---|
| Hand-written, `Task` source and handler (baseline) | 22.865 ns / 160 B | 1,394.088 ns / 328 B | 16.110 ns / 80 B | 839.886 ns / 280 B |
| `Task` source, sync handler | 17.758 ns / 80 B | 838.352 ns / 239 B | 18.513 ns / 80 B | 857.767 ns / 296 B |
| `Task` source, `Task` handler | 24.702 ns / 160 B | 1,363.037 ns / 352 B | 19.792 ns / 80 B | 851.000 ns / 303 B |
| `ValueTask` source and handler | 30.290 ns / 0 B | 1,499.829 ns / 464 B | 41.168 ns / 56 B | 876.554 ns / 398 B |
| Instance, `Task` handler | 7.911 ns / 80 B | 785.422 ns / 104 B | 23.226 ns / 136 B | 22.447 ns / 136 B |
| Instance, `ValueTask` handler | 3.122 ns / 0 B | 800.291 ns / 120 B | 18.000 ns / 56 B | 17.703 ns / 56 B |

On the instance rows a failure never calls the handler and never awaits, so "Failure, yielding" does not yield there.

### Traverse (net10, 256 items)

| Row | Success, completed | Success, yielding | Failure, completed | Failure, yielding |
|---|---|---|---|---|
| Hand-written sequential loop (baseline) | 4,368.4 ns / 21608 B | 139,510.5 ns / 27864 B | 2,239.4 ns / 11504 B | 73,943.8 ns / 14687 B |
| `TraverseAsync`, `Task` selector | 3,545.6 ns / 22720 B | 142,568.6 ns / 29008 B | 3,659.3 ns / 21968 B | 140,139.7 ns / 28232 B |
| `TraverseAsync`, `ValueTask` selector | 2,630.7 ns / 2160 B | 150,662.1 ns / 33072 B | 2,627.8 ns / 1408 B | 148,059.0 ns / 32200 B |
| `SequenceAsync` over started tasks | 4,613.2 ns / 24792 B | 98,648.2 ns / 30941 B | 4,983.9 ns / 24040 B | 87,802.7 ns / 30163 B |

`TraverseAsync` runs every selector and collects every error; the hand-written loop stops at the failing middle item. That is why, on failure, `TraverseAsync` takes 1.61-2.11× the baseline with a `Task` selector and 1.15-2.04× with a `ValueTask` selector across both sizes and all TFMs. `SequenceAsync` starts all tasks before awaiting them, so it is the fastest row on the yielding success path (0.60-0.71× the baseline with 256 items).

### Findings

- On net10 the `Task`-source overloads with a sync handler or action take 0.60-1.03× the hand-written baseline when the source yields (0.60-0.62× where the baseline awaits twice) and 0.70-1.15× with completed tasks.
- **Where CSE loses against the hand-written code:** a `Task` handler on a `Task` source adds work. On net10 `TaskSource_TaskHandler` takes up to 1.19× the baseline when tasks yield (`Else` failure 1,625.02 vs 1,367.34 ns, `Match` failure 1,619.21 vs 1,398.95 ns) and 1.31-2.01× with completed tasks in `Else`, `Ensure`, `Match`, `Tap` and `TapError`, and allocates more on most of those rows (`Match` failure, yielding: 549 B vs 384 B). `ThenBenchmarks` uses `BindAsync` as its baseline, not hand-written code.
- **`ValueTask` sources on the failure path:** with completed tasks the `ValueTask`-source rows take 1.66-3.62× the hand-written `Task` baseline across classes and TFMs and allocate 56 B (136 B with a `Task` handler), while the same rows allocate 0 B on success (80 B with a `Task` handler). net10 examples: Map `ValueTaskSource_SyncHandler` 45.25 ns / 56 B vs 17.52 ns / 80 B, Ensure `ValueTaskSource_ValueTaskPredicate` 70.19 ns / 56 B vs 27.89 ns / 80 B, Match `ValueTaskSource_ValueTaskHandler` 45.48 ns / 56 B vs 13.10 ns / 0 B. The instance `ValueTask` overloads show the same 56 B on failure (Bind 18.000 ns / 56 B vs 3.122 ns / 0 B on success). Not investigated.
- **Other rows slower than the hand-written code on net10:** `ValueTask` sources on the success path with completed tasks take up to 2.63× the baseline (Ensure `ValueTaskSource_ValueTaskPredicate` 53.65 ns vs 20.38 ns), and up to 1.22× when tasks yield, with more bytes (the same row: 1,682.62 ns / 634 B vs 1,373.91 ns / 352 B). The instance overloads with completed tasks take up to 1.97× (Match `Instance_ValueTaskHandler` on failure: 25.75 ns / 56 B vs 13.10 ns / 0 B).
- `RenamedMemberBenchmarks`: the obsolete names and the `Async` names allocate the same bytes and differ by 1.04× or less on net10 (for example `Instance_Bind_Obsolete` 22.773 ns vs `Instance_BindAsync` 22.408 ns, both 136 B).
- net11: single-operation rows that yield are slower than on net10, for the hand-written baseline too (Bind, success, yielding: baseline 3,450.667 ns on net11 vs 1,394.088 ns on net10, `TaskSource_SyncHandler` 3,229.473 vs 838.352 ns). This is the same runtime effect as in the comparison suite's async pipeline. The 256-item `Traverse` rows that yield are faster on net11 than on net10.
- Reports for every class and TFM: `benchmarks/results/results-pattern/{tfm}/results/`, for example [Bind net10](../benchmarks/results/results-pattern/net10.0/results/CSharpEssentials.Results.Benchmarks.BindBenchmarks-report-github.md) and [Traverse net10](../benchmarks/results/results-pattern/net10.0/results/CSharpEssentials.Results.Benchmarks.TraverseBenchmarks-report-github.md).

---

## Feature matrix

CSE cells come from this repository (each csproj, `Directory.Build.props`, `Readme.MD`, `ResultT.cs`, `CSharpEssentials.Errors/ErrorMetadata.cs`, the `CSharpEssentials.Json` sources). Competitor links point at the pinned tags: CFE `v3.7.0`, ErrorOr `v2.1.1` (repository now at `error-or/error-or`), FluentResults `v4.0` (no `v4.0.0` tag; commit matches the 4.0.0 nuspec). Nothing was trim-analyzed or AOT-published for this table.

| Feature | CSharpEssentials.Results 7.0.0 | CSharpFunctionalExtensions 3.7.0 | ErrorOr 2.1.1 | FluentResults 4.0.0 |
|---|---|---|---|---|
| Runtime reflection | Result and Error core: none. `Result<T>.ToString()` formats the value through `IFormattable` with the invariant culture (`ResultT.cs`). The Json dependency's polymorphic and enum converter factories use `MakeGenericType` and are annotated `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`; Results does not call them | Result core: none. JSON converter factories use `MakeGenericType` and `Activator.CreateInstance` ([ResultJsonOfTConverter.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Json/Serialization/ResultJsonOfTConverter.cs)); `EnumValueObject` uses `GetFields` ([EnumValueObject.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/ValueObject/EnumValueObject.cs)) | None found in source ([ErrorOr.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.cs)) | None beyond `GetType()` for reason labels ([Error.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Reasons/Error.cs)) |
| Source generator | None in Results. The Enums generator and analyzers flow in transitively through Errors (`ErrorType` is a `[StringEnum]`) | No | No | No |
| Native AOT annotated (`IsAotCompatible`) / trim warnings | Declared on the net9.0+ targets (not netstandard2.1, which net8 consumers get) for Results, Errors and Maybe, and for Core, Enums and Json, so the trim analyzer runs on them at build time. No AOT publish was run for this table | Not declared ([csproj](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/CSharpFunctionalExtensions.csproj), [Common.Build.props](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/Common.Build.props)) | Not declared ([csproj](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr.csproj)) | Not declared ([csproj](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/FluentResults.csproj)) |
| Extra runtime dependencies | CSharpEssentials.Errors and CSharpEssentials.Json; Errors brings Core and Enums (+ System.Text.Json on netstandard2.1); Json brings Enums. Maybe adds Results and Errors | None on any TFM ([nuspec](https://api.nuget.org/v3-flatcontainer/csharpfunctionalextensions/3.7.0/csharpfunctionalextensions.nuspec)) | None on net8/net10; Microsoft.Bcl.HashCode and System.Memory on netstandard2.0 ([nuspec](https://api.nuget.org/v3-flatcontainer/erroror/2.1.1/erroror.nuspec)) | Microsoft.Extensions.Logging.Abstractions 2.1.1 and System.Threading.Tasks.Extensions 4.5.4 on every TFM ([nuspec](https://api.nuget.org/v3-flatcontainer/fluentresults/4.0.0/fluentresults.nuspec)) |
| Base class required | No. `Result`/`Result<T>` are `readonly record struct`; errors are built with `Error` factories, no subclassing | No. Results are `readonly struct`; `Result<T, E>` takes any `E` ([ResultTE.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultTE.cs)) | No. `ErrorOr<T>` is a `readonly record struct`; custom kinds via `Error.Custom` ([Error.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/Errors/Error.cs)) | No for results (classes). Custom errors derive from `Error` or implement `IError` ([README](https://github.com/altmann/FluentResults/blob/v4.0/README.md#designing-errors-and-success-messages)) |
| Error type model | Typed `Error` struct: Code, Description, `ErrorType` (8 values), NumericType, `ErrorMetadata` (`Dictionary<string, object?>`) | `string` by default (several messages joined with ", "); typed `E` through `Result<T, E>` ([ResultT.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultT.cs)) | Typed `Error` struct: Code, Description, `ErrorType` (7 values), NumericType, `Dictionary<string, object>` metadata ([Error.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/Errors/Error.cs), [ErrorType.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/Errors/ErrorType.cs)) | `IError` class hierarchy with Message, metadata and nested `Reasons` ([Error.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Reasons/Error.cs)) |
| Multiple errors per result | Yes (`Error[]`) | No: one string or one `E`; `Combine` concatenates strings ([Combine.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Combine.cs)) | Yes (`List<Error>`) ([ErrorOr.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.cs)) | Yes (`List<IReason>`) ([ResultBase.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Results/ResultBase.cs)) |
| Maybe / Option type | Yes, separate package CSharpEssentials.Maybe (`readonly struct Maybe<T>`) | Yes, in the package ([Maybe.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Maybe/Maybe.cs)) | No | No |
| Async extension coverage | Map, Bind, Tap, Ensure, Match with Task and ValueTask overloads on every TFM including netstandard2.1 (Readme, "Async overload matrix") | Map, Bind, Tap, Ensure, Match on Task (left, right, both); ValueTask only under `NET5_0_OR_GREATER` ([Extensions](https://github.com/vkhorikov/CSharpFunctionalExtensions/tree/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions)) | Task only: ThenAsync, ThenDoAsync, FailIfAsync, MatchAsync ([ThenExtensions](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.ThenExtensions.cs)) | Task and ValueTask for Bind, Map and Try; no Tap, Ensure or Match ([ResultExtensions.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Extensions/ResultExtensions.cs), [Results.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Factories/Results.cs) for `Try`) |
| Implicit conversions | `T`, `Error`, `Error[]`, `List<Error>`, `HashSet<Error>` → `Result<T>`; `bool` → `Result` (Readme, "Returning Failures") | `T` → `Result<T>`; `T` and `E` → `Result<T, E>`; no `string` → `Result<T>` ([ResultT.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultT.cs), [ResultTE.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultTE.cs)) | `T`, `Error`, `List<Error>`, `Error[]` → `ErrorOr<T>` ([ImplicitConverters](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.ImplicitConverters.cs)) | `T`, `Error`, `List<Error>` → `Result<T>`; `Error`, `List<Error>` → `Result` ([Result.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Results/Result.cs)) |

On the AOT row CSE is the only one of the four that declares `IsAotCompatible`. What still counts against it: the `object?` metadata bag and a five-package dependency chain, against zero dependencies for CFE.

---

## Caveats

- Every CFE number uses its default `Result<T>` with a `string` error. That is why CFE allocates 0 B on Create and Ensure failure and less on the async failure path. A `Result<T, E>` column with a typed error was not benchmarked, so the cost of typed errors in CFE is unknown here.
- The aggregation idioms differ. ErrorOr's documented aggregation builds no per-check results; the other three do. The ErrorOr row measures less work, by design of its idiom. CSE's typed `Result<T>.Combine` was not measured.
- FluentResults has no Ensure, Tap or Maybe and only two of the five async steps, so it has no column there. Its Match column uses property access, the README's processing idiom. ErrorOr and FluentResults have no Maybe.
- Many rows are under 2 ns. Differences under 1 ns are reported as noise. Rows below about 0.1 ns (several Maybe rows, CFE Create on net11) are at timer resolution and cannot be told apart.
- CFE's 5.4266 to 9.2949 ns on net8/net9 against about 2 ns on net10/net11, the 4.8533 to 7.1051 ns Maybe rows on net8/net9, and the 1.66-1.94× slower yielding rows on net11 affect libraries as a group and were not investigated. They are reported, not explained.
- net10 Create failure outlier: CSE took 27.9547 ns on net10 against 9.9265-13.8671 ns on the other TFMs, with the same 56 B.
- net8 uses CSE's netstandard2.1 build. Results, Errors and Maybe do not target net8.0, so the net8 column runs CSE's netstandard2.1 assembly against competitor assets built for net8. net8 gaps may reflect that build, not only the runtime.
- Machine load: 1-minute load average 1.41 to 2.65 during the main runs and up to 4.72 during reruns, with a desktop session running. Gaps of a few percent on the async rows are weak evidence even when outside error.
- The CSE column is the 7.0.0 working tree, which may differ from the last version published to NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project results-comparison                       # all TFMs (net8-net11), all classes, default job
./run-benchmarks.sh --project results-comparison --tfm net10.0         # one TFM, default job
./run-benchmarks.sh --project results-comparison --quick --tfm net10.0 # one TFM, all classes, Short job
./run-benchmarks.sh --project results                                  # async overload suite, all TFMs, default job
```

`--quick` without `--tfm` runs on net9.0, the script default for this suite, and uses BenchmarkDotNet's Short job, so its numbers are not comparable with the committed ones. Results are stored per framework under `benchmarks/results/results-comparison/{tfm}/results/` and `benchmarks/results/results-pattern/{tfm}/results/` (GitHub Markdown, CSV, JSON and HTML).
