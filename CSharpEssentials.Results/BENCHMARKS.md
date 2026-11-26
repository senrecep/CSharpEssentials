# CSharpEssentials.Results: Performance benchmarks

Compared with CSharpFunctionalExtensions 3.7.0, ErrorOr 2.1.1 and FluentResults 4.0.0 (typed results also with OneOf 3.0.271 and Dunet 1.16.2) · BenchmarkDotNet v0.14.0 · 6-core, 12-thread x64 desktop CPU · .NET 8 / 9 / 10 / 11

Two suites. The comparison suite (`results-comparison`) measures CSE against the three libraries; its Result and Maybe classes are below, the `Error` classes are in [CSharpEssentials.Errors/BENCHMARKS.md](../CSharpEssentials.Errors/BENCHMARKS.md), and the Maybe collection helpers are in [CSharpEssentials.Maybe/BENCHMARKS.md](../CSharpEssentials.Maybe/BENCHMARKS.md). The async overload suite (`results-pattern`) measures CSE's own async overloads against hand-written code, in [Async overload matrix](#async-overload-matrix-results-pattern-suite). The typed results (`Result<T, E1>` to `Result<T, E1, E2, E3>`) have their own classes in both suites, in [Typed results](#typed-results).

---

## What was measured

Twelve Result and Maybe classes in `benchmarks/CSharpEssentials.Results.Comparison.Benchmarks/`: the nine scenarios in the table below, plus `SequenceComparison`, `TryComparison` and `TryAsyncComparison` in [Sequence, CombineAll, Try and TryAsync](#sequence-combineall-try-and-tryasync). The five typed-result classes in the same project are described in [Typed results](#typed-results). Every class uses `[MemoryDiagnoser]`, CSE as the baseline, and a `[Params]` switch for the success and failure path. A `[GlobalSetup]` parity check runs every library on the same input and refuses to start if the outcomes differ (success value, or error count and first message).

| # | Class | Operation | Columns |
|---|---|---|---|
| 1 | `CreateComparison` | Success from `42`, failure from a cached error, through each library's documented implicit conversion (CFE: `Result.Failure<int>(string)`, it has no string-to-result conversion) | CSE, CFE, ErrorOr, FluentResults |
| 2 | `MapComparison` | `x => x * 2` over a result built in setup (ErrorOr: `Then(Func<T, TNext>)`) | all four |
| 3 | `BindComparison` | `x => Ok(x + 1)` (CFE `Bind`, ErrorOr `Then`, FluentResults `Bind`) | all four, plus a CSE typed row on `Result<int, FailureError>` |
| 4 | `EnsureComparison` | `x > 0` on 42 (passes) or -1 (fails and creates the error). ErrorOr: `FailIf` with the inverted predicate | CSE, CFE, ErrorOr. FluentResults has no Ensure; its alternatives drop the value or are a hand-written Bind |
| 5 | `MatchComparison` | Match to an int (-1 on failure) and to a string (first error message) | all four, plus a CSE typed row on `Result<int, FailureError>`. FluentResults has no Match; its column uses the README's `IsSuccess`/`Value`/`Errors[0].Message` idiom |
| 6 | `TapComparison` | Side effect into a field, one cached delegate shared by all (ErrorOr: `ThenDo`) | CSE, CFE, ErrorOr. FluentResults has no Tap |
| 7 | `AsyncPipelineComparison` | Five steps over a `Task` source: async Bind (x + 1) → Map (x * 2) → Ensure (x > 0) → Tap → Map (x + 1). `Succeed=false` fails at step 1. `Yield=false`: source and step complete synchronously; `Yield=true`: both complete after `Task.Yield()` | CSE, CFE, ErrorOr. FluentResults has only Bind and Map on `Task<Result<T>>` |
| 8 | `ErrorAggregationComparison` | Three independent checks on `Order(Id, Total)`, collect every error. All-pass `Order(42, 10)`, all-fail `Order(-1, -5)` | all four, each with its documented idiom (see section 8) |
| 9 | `MaybeComparison` | From/None, Map, Bind, Where, GetValueOrDefault, Match over Some(42) or None | CSE, CFE. ErrorOr and FluentResults have no Maybe/Option type |

Every competitor method carries a comment with the README or source URL at the pinned version. Errors are `static readonly` (CFE: a `const string`, its default error type), so error construction is outside the timing except where the operation itself builds the failure.

### Environment

| Property | Value |
|---|---|
| Machine | 6-core, 12-thread x64 desktop CPU (RyuJIT AVX2) |
| OS | Linux (kernel 7.3.0-4-cachyos) |
| Machine load | Desktop session in the background. 1-minute load average: see [How these numbers were produced](#how-these-numbers-were-produced) |
| Tool | BenchmarkDotNet v0.14.0, [MemoryDiagnoser], default job |
| .NET SDK | 11.0.100-rc.1.26425.128; 11.0.100 for the typed-result classes and the `BindComparison` and `MatchComparison` reruns |
| Runtimes tested | .NET 8.0.30 (CSE: netstandard2.1 build), .NET 9.0.19, .NET 10.0.11, .NET 11.0.0; .NET 8.0.33, 9.0.22, 10.0.15 and 11.0.0 for the typed-result classes and the Bind and Match reruns |
| CSharpFunctionalExtensions (CFE) | 3.7.0, default idiom `Result<T>` with a `string` error |
| ErrorOr | 2.1.1 |
| FluentResults | 4.0.0 |
| OneOf / OneOf.Serialization.SystemTextJson | 3.0.271 / 1.1.1 (typed-result classes only) |
| Dunet | 1.16.2 (typed-result classes only) |
| CSharpEssentials.Results / Maybe (CSE) | The repository source, referenced as a project |

### How these numbers were produced

- Full BenchmarkDotNet default-job runs (no `--quick`), on net8.0, net9.0, net10.0 and net11.0, one suite at a time on one machine.
- Comparison suite reports: `benchmarks/results/results-comparison/{tfm}/results/`. Async overload suite reports: `benchmarks/results/results-pattern/{tfm}/results/`.
- 1-minute load average, sampled at the start and end of each suite: 1.55 to 2.37 for the comparison suite and 1.41 to 2.65 for the async overload suite. `MaybeComparison` (all four TFMs) and `CreateComparison` and `SequenceComparison` on net11.0 were rerun because their coefficient of variation (CV) stayed high. The reruns saw 1.43 to 4.72; the 4.72 peak was at the start of the net10.0 `MaybeComparison` rerun. `AsyncPipelineComparison` was rerun after #230-#256 on SDK 11.0.100-rc.3 with .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0; the load average was not recorded for that rerun.
- The typed-result classes, `BindComparison` and `MatchComparison` ran again on 2026-11-26, one suite at a time, against commit c97867c8, on SDK 11.0.100 with .NET 8.0.33, 9.0.22, 10.0.15 and 11.0.0. The 1-minute load average was 1.1 to 2.7 at the start of each suite; no end value was recorded. `TypedCreateComparison` on net8.0 was rerun after a first pass above 5% CV. Sections 3 and 5 use these reruns; the other scenario tables come from the earlier run.
- Rows whose CV stayed above 5% after two reruns are marked "noisy (CV above 5%)". In this suite they are `MaybeComparison` rows and some `CreateComparison` success rows, all near 0 ns. In the typed-result runs one row is above 5% and was not rerun: `TypedMatchComparison` `Dunet_Switch` Success on net9 (0.0584 ns, CV 8.0%).
- Every number is copied from the `*-report-github.md` files. A `-` in the report's Allocated column is written as `0 B` here.

---

## Executive summary

Primary runtime: net10. A tie is a difference within the sum of the two BenchmarkDotNet error margins. An absolute time difference under 1 ns is reported as noise even when it falls outside error; many of these operations take under 2 ns. Rows below about 0.1 ns are at timer resolution and are not ranked.

| Finding | Details |
|---|---|
| Map, Bind, Tap, Match, Ensure (pass), Create (success): 0 B for CSE, CFE and ErrorOr; FluentResults 88-344 B on net10 | net10: FluentResults Map 256 B / 58.5485 ns (success), Bind 320 B / 72.3925 ns, Create 112 B / 25.5255 ns, Match to string on failure 240 B / 78.7860 ns. CSE, CFE and ErrorOr stay under 2.9 ns. CSE, CFE and ErrorOr allocate the same bytes on every TFM; FluentResults allocates more on net8 (Map success 288 B, Bind success 352 B, Create success 120 B, Match to string on failure 304 B) |
| Create failure: CSE 56 B, ErrorOr 88 B, CFE 0 B. CSE is faster than ErrorOr on every TFM | net10: 13.6011 ns / 56 B for CSE, 20.4178 ns / 88 B for ErrorOr (ratio 1.50), 0.0002 ns / 0 B for CFE (timer resolution), 105.5643 ns / 344 B for FluentResults. On net8, net9 and net11 CSE is also faster than ErrorOr (17.6672 vs 24.3424 ns, 14.8156 vs 22.9566 ns, 9.2621 vs 15.1515 ns). CFE stores the string error in its struct and allocates nothing, so CSE loses to CFE on every TFM |
| Ensure failure: CSE beats ErrorOr, loses to CFE | net10: 12.903 ns / 56 B for CSE, 23.776 ns / 88 B for ErrorOr (1.84), 1.807 ns / 0 B for CFE (0.14). Same order on every TFM |
| Map, Bind, Ensure (pass): CSE and ErrorOr within 1 ns; CFE 0.73-1.25 ns slower on net10 | Map success 0.5791 / 0.6358 / 1.8174 ns (CSE / ErrorOr / CFE). On net8 and net9 CFE takes 8.9011 to 9.2949 ns for Map, Bind and Ensure pass, and 5.5893 to 7.6524 ns for Create success, against 0.1647 to 2.386 ns for CSE on the same rows. On net10 and net11 CFE drops to about 2 ns for Map, Bind and Ensure (net11 Bind success: 3.1325 ns). This looks like a runtime/JIT effect; not investigated |
| Match: no gap above 1 ns between CSE and ErrorOr; CFE's Match to int 1.04-1.14 ns slower than CSE | net10 Match to int: 1.2037 / 1.3206 ns (CSE success / failure), 1.0917 / 0.9854 ns (ErrorOr), 2.3437 / 2.3646 ns (CFE). CFE's Match to int takes 2.2878-2.3869 ns on every TFM. ErrorOr is ahead of CSE on Match to int on failure on every TFM (net10 0.9854 vs 1.3206 ns) and on Match to string on failure on net8 and net11 (0.5562 vs 0.8550 ns, 0.6883 vs 0.9778 ns), all under 1 ns |
| Async pipeline, success, completed tasks: CSE fastest on every TFM, same allocation as CFE | net10: 84.00 ns / 480 B for CSE, 95.84 ns / 480 B for CFE (1.14), 103.95 ns / 560 B for ErrorOr (1.24) |
| Async pipeline, failure at step 1: CFE allocates 56 B less; CFE faster on net8 to net10 (CSE loss) | Completed, net10: 103.31 ns / 536 B for CSE, 98.65 ns / 480 B for CFE (0.95), 123.15 ns / 648 B for ErrorOr. CFE is faster on net8 (113.7 vs 123.6 ns), net9 (109.1 vs 113.8 ns) and net10; on net11 CSE is faster (96.42 vs 98.24 ns). Yielding failure, net10: 1,586.32 ns / 960 B for CSE vs 1,558.09 ns / 879 B for CFE (CSE loss, outside error); CFE is also faster on net8 and net9, a tie on net11 |
| Async pipeline, yielding success: CSE allocates 24 B more than CFE (CSE loss) | net10: 1,580.19 ns / 904 B for CSE, 1,594.53 ns / 880 B for CFE (within error), 1,836.72 ns / 1037 B for ErrorOr. CSE is faster on net8 (1,628.6 vs 1,656.9 ns) and net9 (1,572.1 vs 1,611.5 ns); time is a tie on net10 and net11 |
| Error aggregation: ErrorOr faster and smaller on both paths (CSE loss); CSE beats CFE and FluentResults | All-fail, net10: 99.09 ns / 336 B for CSE, 42.79 ns / 184 B for ErrorOr (0.43), 212.03 ns / 712 B for FluentResults (2.14), 368.58 ns / 784 B for CFE (3.72). All-pass: 14.51 ns / 48 B for CSE, 11.63 ns / 32 B for ErrorOr (0.80). Same order on every TFM. ErrorOr's idiom does less work (see section 8) |
| Maybe: tie with CFE | Both allocate 0 B in every operation. Every net10 mean is at most 1.1824 ns and every measurable CSE vs CFE gap on net10 is under 0.1 ns. On net9, CSE Bind on None is 1.27 ns faster (5.5385 vs 6.8108 ns). Create and GetValueOrDefault on None are at timer resolution and partly noisy |
| Sequence and CombineAll: ErrorOr's hand-written loop is faster (CSE loss); CSE beats CFE and FluentResults | net10, N = 100, all success: `Sequence` 268.480 ns / 1640 B for CSE, 229.394 ns / 1216 B for ErrorOr (0.85), 854.432 ns / 2344 B for CFE, 2,311.158 ns / 6528 B for FluentResults. `CombineAll` allocates the least of the four on every row (0 B on success, 56 B on failure) |
| Try and TryAsync: the exception dominates; CSE and the other libraries tie or differ by a few percent | net10 `Try` failure: 2,399.324 ns / 520 B for CSE, 2,452.483 ns / 464 B for CFE, 2,352.819 ns / 552 B for ErrorOr, 2,642.237 ns / 832 B for FluentResults. `TryAsync` failure, completed: CFE is faster than CSE on net9 to net11 (net10 5,076.65 vs 5,385.12 ns) and allocates 56 B less |
| Typed results: a failure from a cached error allocates nothing; Match, async `Task` bind and Bind on net8/net9 are slower than `Result<T>` (CSE loss) | net10: assigning a cached error takes 2.6047 ns / 0 B against 14.7804 ns / 56 B for `Result<int>` and 21.5302 ns / 88 B for ErrorOr. A four-handler Match takes 3.3643-4.6895 ns against 1.2075-1.6354 ns for `Result<int>`; `switch` takes 1.0416-1.7657 ns. Task `BindAsync` on completed tasks: 56.78 ns / 240 B against 21.91 ns / 160 B. See [Typed results](#typed-results) |
| AOT and dependencies | Results, Errors and Maybe (and Core, Enums and Json) declare `IsAotCompatible` on their net9.0+ targets, not on netstandard2.1; none of the three competitors does. No trim or AOT publish was run. Results pulls in Errors and Json, which bring Core and Enums; CFE has no runtime dependencies. See [Feature matrix](#feature-matrix) |

### Where CSE loses

Every row where a competitor's mean is lower than CSE's by more than the two error margins, or where it allocates fewer bytes. Rows from the Error, Maybe collection and async overload classes are listed on their own pages and in the async overload section below.

- Create failure: CFE on every TFM (net10: 0.0002 ns / 0 B vs 13.6011 ns / 56 B; on net10 and net11 CFE is at timer resolution).
- Ensure failure: CFE on every TFM (net10: 1.807 ns / 0 B vs 12.903 ns / 56 B).
- Async pipeline: CFE allocates less on every failure and yielding row: 480 B vs 536 B (failure, completed), 879-880 B vs 960 B (failure, yielding), 880 B vs 904 B (success, yielding), all TFMs. CFE is faster on completed failure on net8 (113.7 vs 123.6 ns), net9 (109.1 vs 113.8 ns) and net10 (98.65 vs 103.31 ns), and on yielding failure on net8 to net10 (net10 1,558.09 vs 1,586.32 ns).
- Error aggregation: ErrorOr on both paths, every TFM (net10 all-fail 42.79 ns / 184 B vs 99.09 ns / 336 B; all-pass 11.63 ns / 32 B vs 14.51 ns / 48 B).
- Sequence: ErrorOr on every row and TFM, in time and bytes (net10 N = 10 failure 82.588 ns / 400 B vs 118.384 ns / 488 B). FluentResults allocates less on N = 100 failure (net10 1288 B vs 1456 B) while taking 945.368 vs 484.463 ns.
- CombineAll: ErrorOr on time at N = 100 on every TFM (net10 failure 69.222 vs 137.997 ns, success 37.141 vs 67.625 ns) and at N = 10 failure on net11 (26.990 vs 38.115 ns); CFE on N = 100 success on net10 (62.774 vs 67.625 ns) and net11 (64.439 vs 68.541 ns). CSE allocates the least on all of these rows.
- Try: ErrorOr's hand-written `try`/`catch` on failure on net10 (2,352.819 vs 2,399.324 ns) and net11 (1,318.470 vs 1,339.987 ns), and on success on net8 (6.586 vs 7.838 ns). CFE allocates 56 B less on failure on net9 to net11 (net10 464 B vs 520 B).
- TryAsync: CFE on completed failure on net9 to net11 (net10 5,076.65 vs 5,385.12 ns) and ErrorOr on the same row (net10 5,217.23 ns). CFE allocates 56-64 B less on every failure row and up to 9 B less on yielding success; ErrorOr allocates 23-25 B less on yielding success. ErrorOr is faster on completed success on net8, net9 and net11 (net11 31.48 vs 33.61 ns).
- `ErrorToStringComparison` `Result` row: CFE allocates 48 B against CSE's 72 B on net8 (CSE is faster: 16.03 vs 51.51 ns).
- Typed results, against `Result<T>` and competitors: listed in [Typed results, where the typed result loses](#where-the-typed-result-loses).
- Measured, under 1 ns (outside error but below the noise threshold): ErrorOr on Map, Bind, Ensure pass, Match and Tap rows by 0.01-0.77 ns on some TFMs (largest: Bind on failure on net9, 0.8654 vs 1.6394 ns); CFE on many measurable Maybe rows on net8 to net11 by 0.003-0.15 ns (rows at timer resolution excluded; largest: net9 Where Some, 0.8730 vs 1.0273 ns, and net9 Map None, 0.5743 vs 0.6797 ns); ErrorOr on `Try` success on net9 to net11 (net10 6.048 vs 7.008 ns) and `TryAsync` completed success on net10 (33.64 vs 34.40 ns).

---

## Results by scenario

All tables: net10, mean and allocated bytes per operation. "Ratio" is the report's Ratio column (library mean divided by CSE mean; above 1.00 = slower than CSE). A dash means the library has no column in that class (reason in the What was measured table).

### 1. Create

| Library | Success mean | Success alloc | Failure mean | Failure alloc | Failure ratio |
|---|---:|---:|---:|---:|---:|
| CSE | 0.1588 ns (noisy, CV above 5%) | 0 B | 13.6011 ns | 56 B | 1.00 |
| CFE (string error) | 0.0233 ns (timer resolution) | 0 B | 0.0002 ns (timer resolution) | 0 B | 0.00 |
| ErrorOr | 0.1551 ns | 0 B | 20.4178 ns | 88 B | 1.50 |
| FluentResults | 25.5255 ns | 112 B | 105.5643 ns | 344 B | 7.76 |

Success is free for the three struct-based libraries. On failure CSE allocates 56 B and ErrorOr 88 B (a `List<Error>`); CFE's default `Result<T>` keeps the string inline. This class does not have a CFE `Result<T, E>` column. The typed-result classes use CFE `Result<int, TypedError>`: it allocates 0 B on a cached error and 40 B on a new one, the same as the CSE typed result (see [Typed results](#typed-results)).

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
| CSE | 0.7201 ns | 0 B | 1.2449 ns | 0 B |
| CSE typed (`Result<int, FailureError>`) | 1.1559 ns | 0 B | 1.3721 ns | 0 B |
| CFE | 1.9689 ns | 0 B | 1.9763 ns | 0 B |
| ErrorOr | 0.6052 ns | 0 B | 0.9622 ns | 0 B |
| FluentResults | 72.3925 ns | 320 B | 55.1934 ns | 208 B |

From the 2026-11-26 rerun. The typed row is within 0.44 ns of `Result<T>` on net10 and within 0.71 ns on net11 (1.3875 vs 0.6854 ns on failure). On net8 and net9 it takes 5.5150 to 7.489 ns against 1.1069 to 2.386 ns for `Result<T>` (CSE loss, see [Typed results](#typed-results)).

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
| CSE | 1.2037 ns | 1.3206 ns | 2.6181 ns | 1.0312 ns | 0 B |
| CSE typed (`Result<int, FailureError>`) | 1.2270 ns | 1.8412 ns | 2.4725 ns | 1.1931 ns | 0 B |
| CFE | 2.3437 ns | 2.3646 ns | 2.7688 ns | 1.6065 ns | 0 B |
| ErrorOr | 1.0917 ns | 0.9854 ns | 2.6193 ns | 1.0323 ns | 0 B |
| FluentResults (properties idiom) | 20.7559 ns | 20.8902 ns | 22.4659 ns | 78.7860 ns | 88-240 B |

From the 2026-11-26 rerun. CFE's ToInt rows are 1.14 ns (success) and 1.04 ns (failure) slower than CSE. The other net10 differences are under 1 ns; the largest is the typed row on ToInt failure, 0.52 ns behind `Result<T>`. The typed row here has one error slot. With two or three slots Match gets slower (see [Typed results](#typed-results)). FluentResults allocates even when only reading properties (its `Errors` is computed with `Reasons.OfType<IError>()`).

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
| Success, completed | 84.00 ns / 480 B | 95.84 ns / 480 B | 103.95 ns / 560 B | 1.14 |
| Success, yielding | 1,580.19 ns / 904 B | 1,594.53 ns / 880 B | 1,836.72 ns / 1037 B | 1.01 |
| Failure at step 1, completed | 103.31 ns / 536 B | 98.65 ns / 480 B | 123.15 ns / 648 B | 0.95 |
| Failure at step 1, yielding | 1,586.32 ns / 960 B | 1,558.09 ns / 879 B | 1,848.80 ns / 1127 B | 0.98 |

- CSE is fastest on the success path with completed tasks, on every TFM.
- CFE allocates less than CSE on the failure path (56 B and 80-81 B less) and on yielding success (24 B less). Part of that is the error representation: CFE carries one string, CSE an error array built at step 1.
- ErrorOr allocates the most on every path and is the slowest on every path and TFM.

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
| Create failure | CSE | 17.6672 ns / 56 B | 14.8156 ns / 56 B | 13.6011 ns / 56 B | 9.2621 ns / 56 B |
| | CFE | 7.6536 ns / 0 B | 7.6155 ns / 0 B | 0.0002 ns / 0 B (timer resolution) | 0.0001 ns / 0 B (timer resolution) |
| | ErrorOr | 24.3424 ns / 88 B | 22.9566 ns / 88 B | 20.4178 ns / 88 B | 15.1515 ns / 88 B |
| | FluentResults | 167.0927 ns / 384 B | 143.2895 ns / 344 B | 105.5643 ns / 344 B | 90.3819 ns / 344 B |
| Ensure failure | CSE | 14.817 ns / 56 B | 13.780 ns / 56 B | 12.903 ns / 56 B | 10.647 ns / 56 B |
| | CFE | 8.411 ns / 0 B | 8.452 ns / 0 B | 1.807 ns / 0 B | 1.802 ns / 0 B |
| | ErrorOr | 19.726 ns / 88 B | 22.502 ns / 88 B | 23.776 ns / 88 B | 17.045 ns / 88 B |
| Map success | CSE | 0.8658 ns / 0 B | 0.8478 ns / 0 B | 0.5791 ns / 0 B | 0.5651 ns / 0 B |
| | CFE | 8.9349 ns / 0 B | 9.1264 ns / 0 B | 1.8174 ns / 0 B | 1.7713 ns / 0 B |
| | ErrorOr | 0.7119 ns / 0 B | 0.8482 ns / 0 B | 0.6358 ns / 0 B | 0.5745 ns / 0 B |
| | FluentResults | 139.7369 ns / 288 B | 105.4259 ns / 256 B | 58.5485 ns / 256 B | 60.6824 ns / 256 B |
| Async success, completed | CSE | 103.9 ns / 480 B | 109.6 ns / 480 B | 84.00 ns / 480 B | 83.03 ns / 480 B |
| | CFE | 115.4 ns / 480 B | 113.0 ns / 480 B | 95.84 ns / 480 B | 93.77 ns / 480 B |
| | ErrorOr | 122.0 ns / 560 B | 118.2 ns / 560 B | 103.95 ns / 560 B | 109.93 ns / 560 B |
| Async success, yielding | CSE | 1,628.6 ns / 904 B | 1,572.1 ns / 904 B | 1,580.19 ns / 904 B | 1,179.85 ns / 904 B |
| | CFE | 1,656.9 ns / 880 B | 1,611.5 ns / 880 B | 1,594.53 ns / 880 B | 1,201.19 ns / 880 B |
| | ErrorOr | 1,884.8 ns / 1038 B | 1,865.3 ns / 1037 B | 1,836.72 ns / 1037 B | 1,338.94 ns / 1037 B |
| Async failure, completed | CSE | 123.6 ns / 536 B | 113.8 ns / 536 B | 103.31 ns / 536 B | 96.42 ns / 536 B |
| | CFE | 113.7 ns / 480 B | 109.1 ns / 480 B | 98.65 ns / 480 B | 98.24 ns / 480 B |
| | ErrorOr | 162.7 ns / 648 B | 143.5 ns / 648 B | 123.15 ns / 648 B | 113.05 ns / 648 B |
| Async failure, yielding | CSE | 1,648.5 ns / 960 B | 1,582.9 ns / 960 B | 1,586.32 ns / 960 B | 1,179.38 ns / 960 B |
| | CFE | 1,591.0 ns / 880 B | 1,550.8 ns / 879 B | 1,558.09 ns / 879 B | 1,170.73 ns / 879 B |
| | ErrorOr | 1,902.4 ns / 1127 B | 1,885.9 ns / 1126 B | 1,848.80 ns / 1127 B | 1,332.85 ns / 1127 B |
| Aggregation all-fail | CSE | 116.756 ns / 336 B | 108.90 ns / 336 B | 99.09 ns / 336 B | 81.97 ns / 336 B |
| | CFE | 458.489 ns / 800 B | 464.36 ns / 784 B | 368.58 ns / 784 B | 356.16 ns / 784 B |
| | ErrorOr | 47.569 ns / 184 B | 46.15 ns / 184 B | 42.79 ns / 184 B | 36.77 ns / 184 B |
| | FluentResults | 292.322 ns / 712 B | 257.75 ns / 712 B | 212.03 ns / 712 B | 197.59 ns / 712 B |
| Aggregation all-pass | CSE | 17.040 ns / 48 B | 16.71 ns / 48 B | 14.51 ns / 48 B | 13.91 ns / 48 B |
| | CFE | 54.476 ns / 160 B | 53.38 ns / 160 B | 31.65 ns / 160 B | 33.03 ns / 160 B |
| | ErrorOr | 8.318 ns / 32 B | 11.81 ns / 32 B | 11.63 ns / 32 B | 11.36 ns / 32 B |
| | FluentResults | 151.108 ns / 368 B | 140.42 ns / 368 B | 106.85 ns / 368 B | 97.63 ns / 368 B |

- The allocation findings hold on every TFM: CSE allocates more than CFE on Create/Ensure failure (+56 B), async failure (+56 B completed, +80-81 B yielding) and yielding success (+24 B), and more than ErrorOr on aggregation (+16 B all-pass, +152 B all-fail). CSE allocates less than ErrorOr on Create/Ensure failure (56 B vs 88 B).
- The yielding rows on net11 take 0.74-0.75× the net10 time for CSE and CFE and 0.72-0.73× for ErrorOr. On net11 CSE and CFE are within error on both yielding paths; ErrorOr is the slowest.

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

`benchmarks/CSharpEssentials.Results.Benchmarks/` measures CSE's own async overloads, with no competitor columns. Each class (`Map`, `Bind`, `Ensure`, `Else`, `Match`, `Tap`, `TapError`, `Then`) calls the operation on three source shapes (a `Result<T>` instance, a `Task<Result<T>>` and a `ValueTask<Result<T>>`) with sync, `Task` and `ValueTask` handlers, against a hand-written `await` plus `if` baseline. `Succeed` picks the path; `Yield = false` uses completed tasks, `Yield = true` completes every awaitable after `Task.Yield()`. `RenamedMemberBenchmarks` checks that the obsolete pre-6.4 names cost the same as their `Async` replacements, and `TraverseBenchmarks` measures `TraverseAsync` and `SequenceAsync` over 16 or 256 items. The `Instance_ValueTask*` and `TraverseAsync_ValueTaskSelector` rows need net9.0 or later, so net8 has no such rows. The typed-result classes of this suite (`TypedMatchBenchmarks`, `TypedBindBenchmarks`, `TypedAsyncBenchmarks`) are in [Typed results](#typed-results).

### Bind (net10)

| Row | Success, completed | Success, yielding | Failure, completed | Failure, yielding |
|---|---|---|---|---|
| Hand-written, `Task` source and handler (baseline) | 21.488 ns / 160 B | 1,363.928 ns / 328 B | 15.927 ns / 80 B | 833.483 ns / 223 B |
| `Task` source, sync handler | 16.852 ns / 80 B | 829.137 ns / 239 B | 17.883 ns / 80 B | 838.519 ns / 239 B |
| `Task` source, `Task` handler | 23.231 ns / 160 B | 1,341.255 ns / 352 B | 19.299 ns / 80 B | 834.980 ns / 247 B |
| `ValueTask` source and handler | 32.092 ns / 0 B | 1,483.770 ns / 464 B | 24.630 ns / 0 B | 848.918 ns / 342 B |
| Instance, `Task` handler | 7.511 ns / 80 B | 792.467 ns / 104 B | 9.033 ns / 80 B | 9.072 ns / 80 B |
| Instance, `ValueTask` handler | 2.841 ns / 0 B | 791.883 ns / 120 B | 4.852 ns / 0 B | 5.139 ns / 0 B |

On the instance rows a failure never calls the handler and never awaits, so "Failure, yielding" does not yield there.

### Traverse (net10, 256 items)

| Row | Success, completed | Success, yielding | Failure, completed | Failure, yielding |
|---|---|---|---|---|
| Hand-written sequential loop (baseline) | 4,183.0 ns / 21608 B | 140,920.7 ns / 27864 B | 2,148.5 ns / 11504 B | 70,743.0 ns / 14687 B |
| `TraverseAsync`, `Task` selector | 3,440.5 ns / 22720 B | 136,371.2 ns / 29008 B | 3,451.9 ns / 21968 B | 134,163.2 ns / 28232 B |
| `TraverseAsync`, `ValueTask` selector | 2,612.7 ns / 2160 B | 152,914.1 ns / 33072 B | 2,622.5 ns / 1408 B | 145,722.0 ns / 32200 B |
| `SequenceAsync` over started tasks | 5,029.4 ns / 24792 B | 87,617.9 ns / 30940 B | 4,570.4 ns / 24040 B | 87,170.5 ns / 30164 B |

`TraverseAsync` runs every selector and collects every error; the hand-written loop stops at the failing middle item. That is why, on failure, `TraverseAsync` takes 1.60-1.99× the baseline with a `Task` selector and 1.20-2.07× with a `ValueTask` selector across both sizes and all TFMs. `SequenceAsync` starts all tasks before awaiting them, so it is the fastest row on the yielding success path (0.58-0.64× the baseline with 256 items).

### Findings

- On net10 the `Task`-source overloads with a sync handler or action take 0.60-1.02× the hand-written baseline when the source yields (0.60-0.62× where the baseline awaits twice) and 0.72-1.12× with completed tasks.
- Where CSE loses against the hand-written code: a `Task` handler on a `Task` source adds work. On net10 `TaskSource_TaskHandler` takes up to 1.20× the baseline when tasks yield (`Match` success 1,640.94 vs 1,372.82 ns, `Else` failure 1,587.090 vs 1,360.879 ns) and 1.39-2.14× with completed tasks in `Else`, `Ensure`, `Match`, `Tap` and `TapError`, and allocates more on most of those rows (`Match` failure, yielding: 491 B vs 328 B). `ThenBenchmarks` uses `BindAsync` as its baseline, not hand-written code.
- `ValueTask` sources on the failure path: with completed tasks the `ValueTask`-source rows take 0.95-3.32× the hand-written `Task` baseline across classes and TFMs. They allocate what they allocate on success: 0 B (80 B with a `Task` handler). net10 examples: Map `ValueTaskSource_SyncHandler` 20.290 ns / 0 B vs 16.696 ns / 80 B, Ensure `ValueTaskSource_ValueTaskPredicate` 53.71 ns / 0 B vs 17.13 ns / 80 B, Match `ValueTaskSource_ValueTaskHandler` 31.45 ns / 0 B vs 13.93 ns / 0 B. The instance `ValueTask` overloads also allocate 0 B on failure (Bind 4.852 ns / 0 B vs 2.841 ns / 0 B on success).
- Other rows slower than the hand-written code on net10: `ValueTask` sources on the success path with completed tasks take up to 2.81× the baseline (Ensure `ValueTaskSource_ValueTaskPredicate` 56.58 ns vs 20.17 ns), and up to 1.21× when tasks yield, with more bytes (Match `ValueTaskSource_ValueTaskHandler`: 1,666.30 ns / 572 B vs 1,372.82 ns / 328 B). The instance overloads with completed tasks take up to 1.88× (Ensure `Instance_ValueTaskPredicate` on failure: 32.16 ns / 0 B vs 17.13 ns / 80 B).
- `RenamedMemberBenchmarks`: the obsolete names and the `Async` names allocate the same bytes and differ by 1.06× or less on net10 (for example `Instance_Bind_Obsolete` 9.469 ns vs `Instance_BindAsync` 9.327 ns, both 80 B).
- net11: single-operation rows that yield are faster than on net10, for the hand-written baseline too (Bind, success, yielding: baseline 903.853 ns on net11 vs 1,363.928 ns on net10, `TaskSource_SyncHandler` 547.012 vs 829.137 ns). The yielding `Task` and `ValueTask` source rows take 0.63-0.81× their net10 time. The comparison suite's async pipeline agrees: its yielding rows take 0.72-0.75× their net10 time on net11. The `Traverse` rows that yield are also faster on net11 than on net10.
- Reports for every class and TFM: `benchmarks/results/results-pattern/{tfm}/results/`, for example [Bind net10](../benchmarks/results/results-pattern/net10.0/results/CSharpEssentials.Results.Benchmarks.BindBenchmarks-report-github.md) and [Traverse net10](../benchmarks/results/results-pattern/net10.0/results/CSharpEssentials.Results.Benchmarks.TraverseBenchmarks-report-github.md).

---

## Typed results

Typed results carry their errors in slots: `Result<T, E1>`, `Result<T, E1, E2>` and `Result<T, E1, E2, E3>`. Five classes in the comparison suite and three in the async overload suite measure them. `BindComparison` and `MatchComparison` also have a typed row with one slot (sections 3 and 5). All of them ran on 2026-11-26 (see [How these numbers were produced](#how-these-numbers-were-produced)).

In the comparison classes the CSE row is `Result<int, NotFoundError, ConflictError, ValidationError>`, with the error kinds from `CSharpEssentials.Errors`. The other columns hold the same cases:

- `Result<int>`: CSE's untyped result, with `Error` values that carry the same codes.
- `Any<int, NotFoundError, ConflictError, ValidationError>` from CSharpEssentials.Any.
- OneOf 3.0.271: `OneOf<int, NotFoundError, ConflictError, ValidationError>`.
- Dunet 1.16.2: a generated union record, `OrderOutcome`, with one case for the value and one per error.
- ErrorOr 2.1.1 with `Error.NotFound`, `Error.Conflict` and `Error.Validation`.
- CFE 3.7.0: `Result<int, TypedError>`, CFE's typed-error result.
- FluentResults 4.0.0 with one `Error` subclass per kind.
- net11 only: a C# `union OrderUnion(int, NotFoundError, ConflictError, ValidationError)` row in `TypedCreateComparison` and in the Switch category of `TypedMatchComparison`.

Every class has `[MemoryDiagnoser]`, CSE typed as the baseline and a `[GlobalSetup]` parity check.

| Class | Operation | Columns |
|---|---|---|
| `TypedCreateComparison` | Write Ok(42), a cached `NotFoundError` (`static readonly`) or a `new ConflictError(...)` into each slot of a 1024-element array. `OperationsPerInvoke = 1024`, so the mean is per assignment | CSE typed, `Result<int>`, `Any`, OneOf, Dunet, ErrorOr, CFE, FluentResults; native union on net11 |
| `TypedMatchComparison` | `Case` = Success, NotFound or Validation, mapped to 42, 404 or 400. Match category: each library's Match with one handler per case (`Result<int>` and ErrorOr read the first error's type; FluentResults reads `IsSuccess`, `Value` and `Errors[0]`). Switch category: a C# `switch` on the CSE value, a CSE `TryGetValue` chain, a `switch` on OneOf's `.Value`, a `switch` on Dunet's case records | Match: the eight columns of Create. Switch: CSE switch (baseline), CSE `TryGetValue`, OneOf, Dunet; native union on net11 |
| `TypedBindChainComparison` | Map (x * 2), Bind (x + 1), Bind (fails with NotFound when x <= 0), then Match to a status. `Succeed=false` starts from NotFound | CSE typed, `Result<int>`, OneOf (`MapT0`, then `Match` to rebuild the union, since it has no Bind), ErrorOr (`Then`), CFE, FluentResults (properties at the end) |
| `TypedAsyncBindComparison` | One `BindAsync` over a `Task` or `ValueTask` source, handler `x => x + 1`. `Yield=false`: source and handler complete synchronously. `Yield=true`: both complete after `Task.Yield()` | CSE typed `Task` (baseline) and `ValueTask`, `Result<int>` `Task` and `ValueTask`, ErrorOr `ThenAsync`, CFE `Bind`, FluentResults `Bind`, the last three on `Task` |
| `TypedJsonComparison` | `JsonSerializer.Serialize` and `Deserialize` of a Success or NotFound value | CSE typed through its envelope converter, OneOf through OneOf.Serialization.SystemTextJson 1.1.1 (OneOf has no converter of its own), Dunet with default options |
| `TypedMatchBenchmarks` (async suite project) | Match over `Result<int>` (baseline) and over typed results with one, two and three slots, plus a `switch` and a `TryGetValue` chain on the three-slot type. The failure holds the last slot | CSE only |
| `TypedBindBenchmarks` (async suite project) | Map (x * 2), Bind, Bind on `Result<int>` (baseline) and on typed results with one, two and three slots, without a final Match | CSE only |
| `TypedAsyncBenchmarks` (async suite project) | `BindAsync` on the three-slot type against a hand-written `await` plus `TryGetValue` baseline, with the source and handler shapes of the [async overload matrix](#async-overload-matrix-results-pattern-suite). The `Instance_*` rows return on failure without calling the handler, so they never yield on failure | CSE only |

### Create (net10, per assignment)

| Library | Ok | Cached error | New error |
|---|---|---|---|
| CSE typed | 0.5647 ns / 0 B | 2.6047 ns / 0 B | 7.8162 ns / 40 B |
| CSE `Result<int>` | 0.3003 ns / 0 B | 14.7804 ns / 56 B | 8.9502 ns / 56 B |
| CSE `Any` | 6.9744 ns / 24 B | 2.6037 ns / 0 B | 7.8310 ns / 40 B |
| OneOf | 0.7431 ns / 0 B | 2.6077 ns / 0 B | 8.0987 ns / 40 B |
| Dunet | 10.0228 ns / 24 B | 11.6612 ns / 24 B | 16.1199 ns / 64 B |
| ErrorOr | 0.3051 ns / 0 B | 21.5302 ns / 88 B | 14.8849 ns / 88 B |
| CFE `Result<int, TypedError>` | 0.6250 ns / 0 B | 2.6040 ns / 0 B | 8.0833 ns / 40 B |
| FluentResults | 26.3494 ns / 112 B | 30.2794 ns / 120 B | 53.6179 ns / 272 B |

- A cached error allocates nothing: the typed result stores the reference. `Result<int>` wraps the error in a new `Error[]` (56 B) and ErrorOr in a `List<Error>` (88 B). CSE typed, `Any`, OneOf and CFE tie at 2.6037 to 2.6077 ns on net10.
- With a new error the error object (40 B) is most of the cost, so typed, `Any`, OneOf and CFE are within 0.29 ns of each other.
- The Ok rows of CSE typed, `Result<int>`, OneOf, ErrorOr and CFE are under 1 ns, near timer resolution; `Result<int>` and ErrorOr are 0.26 ns faster than typed. `Any` boxes the int (24 B) and Dunet allocates its case record.
- net11 native union: 2.6054 ns / 0 B with a cached error, 7.3795 ns / 40 B with a new one, and 6.6133 ns / 24 B for Ok, because the union boxes the int.

### Match and switch (net10)

| Row | Success | NotFound | Validation |
|---|---|---|---|
| CSE typed `Match` (4 handlers) | 3.3643 ns | 4.6895 ns | 3.9342 ns |
| CSE `Result<int>` `Match` | 1.2075 ns | 1.6275 ns | 1.6354 ns |
| CSE `Any` `Match` | 5.3619 ns | 5.3828 ns | 5.3811 ns |
| OneOf `Match` | 1.6700 ns | 1.9141 ns | 2.1979 ns |
| Dunet `Match` | 5.7703 ns | 5.8460 ns | 5.8473 ns |
| ErrorOr `Match` | 0.8203 ns | 1.6243 ns | 1.6312 ns |
| CFE `Match` | 2.3192 ns | 2.4930 ns | 3.0698 ns |
| FluentResults (properties) | 20.9278 ns / 96 B | 77.1119 ns / 240 B | 80.0767 ns / 240 B |
| CSE typed `switch` | 1.0416 ns | 1.7657 ns | 1.3360 ns |
| CSE typed `TryGetValue` chain | 0.4762 ns | 0.7369 ns | 0.2941 ns |
| OneOf `switch` on `.Value` | 5.7799 ns / 24 B | 2.0325 ns | 3.0769 ns |
| Dunet `switch` | 0.2951 ns | 0.8721 ns | 0.8665 ns |

Rows without bytes allocate 0 B. The `TryGetValue` and Dunet `switch` rows are under 1 ns, near timer resolution. On net9 Dunet `switch` on Success is 0.0584 ns with a CV of 8.0%, noise level.

- Match on the typed result is slower than `Result<int>`, ErrorOr and OneOf on every case and TFM (CSE loss). On net10 it takes 3.3643 to 4.6895 ns against 1.2075 to 1.6354 ns for `Result<int>`. It is faster than `Any` and Dunet on net10 (5.3619 to 5.8473 ns).
- The cost grows with the number of slots. In `TypedMatchBenchmarks` on net10, Match with one slot costs 1.02 (failure) and 1.01 (success) of `Result<int>`, with two slots 2.24 and 2.18, with three slots 2.86 and 3.04.
- A `switch` on the typed result avoids most of that: 1.0416 to 1.7657 ns in the comparison class, and 0.92 (failure) and 0.76 (success) of `Result<int>` Match in `TypedMatchBenchmarks`. OneOf's `switch` on `.Value` boxes the int on Success (24 B).
- net11 native union `switch`: 0.5782 ns (Success), 0.7584 ns (NotFound), 1.3347 ns (Validation), 0 B. The first two are near timer resolution.

### Bind chain and single Bind (net10)

| Library | Success | Failure at the start |
|---|---|---|
| CSE typed | 8.076 ns | 8.902 ns |
| CSE `Result<int>` | 6.538 ns | 5.478 ns |
| OneOf (`MapT0` + `Match`) | 31.756 ns | 30.930 ns |
| ErrorOr | 4.516 ns | 6.610 ns |
| CFE `Result<int, TypedError>` | 7.708 ns | 7.346 ns |
| FluentResults | 250.991 ns / 992 B | 310.874 ns / 1000 B |

Every row except FluentResults allocates 0 B.

- The chain ends in a Match, and typed Match is the slow step above, so part of the gap is Match. `Result<int>` and ErrorOr are faster on both paths on every TFM, and CFE on failure on every TFM and on success on net9 to net11 (CSE loss).
- In `TypedBindBenchmarks` (Map, Bind, Bind, no Match) on net10 the typed results take 4.429 to 4.576 ns on success against 4.175 ns for `Result<int>` (1.06 to 1.10), and 5.025 to 5.116 ns on failure against 3.992 ns (1.26 to 1.28). One to three slots differ by at most 0.15 ns.

### Async bind (net10)

| Row | Success, completed | Success, yielding | Failure, completed | Failure, yielding |
|---|---|---|---|---|
| CSE typed, `Task` | 56.78 ns / 240 B | 1,622.01 ns / 527 B | 51.44 ns / 160 B | 853.58 ns / 352 B |
| CSE typed, `ValueTask` | 71.69 ns / 0 B | 1,795.35 ns / 662 B | 69.23 ns / 0 B | 883.84 ns / 359 B |
| CSE `Result<int>`, `Task` | 21.91 ns / 160 B | 1,361.94 ns / 384 B | 15.99 ns / 80 B | 827.99 ns / 263 B |
| CSE `Result<int>`, `ValueTask` | 39.12 ns / 0 B | 1,501.36 ns / 496 B | 34.12 ns / 0 B | 834.98 ns / 358 B |
| ErrorOr `ThenAsync` | 39.41 ns / 240 B | 1,587.75 ns / 518 B | 33.43 ns / 160 B | 840.72 ns / 343 B |
| CFE `Bind` | 30.64 ns / 160 B | 1,438.51 ns / 408 B | 33.66 ns / 160 B | 847.11 ns / 344 B |
| FluentResults `Bind` | 117.48 ns / 536 B | 1,754.33 ns / 768 B | 86.22 ns / 352 B | 875.24 ns / 504 B |

- The typed `Task` bind allocates 80 B more than the `Result<int>` `Task` bind with completed tasks (240 vs 160 B, 160 vs 80 B) and takes 2.6 and 3.2 times as long (CSE loss). The 80 B gap holds on every TFM.
- It is also slower than ErrorOr and CFE with completed tasks, with the same bytes as ErrorOr and 80 B more than CFE on success.
- With yielding tasks on success it allocates 527 B against 384 B for `Result<int>` and 408 B for CFE, and takes 1.19 times the `Result<int>` time.
- The typed `ValueTask` bind allocates 0 B with completed tasks but takes 71.69 and 69.23 ns, against 39.12 and 34.12 ns for the `Result<int>` `ValueTask` bind.

`TypedAsyncBenchmarks` against its hand-written `await` plus `TryGetValue` baseline, net10:

| Row | Success, completed | Success, yielding | Failure, completed | Failure, yielding |
|---|---|---|---|---|
| Hand-written, `Task` source and handler (baseline) | 25.59 ns / 160 B | 1,379.78 ns / 328 B | 18.32 ns / 80 B | 825.14 ns / 223 B |
| `Task` source, sync handler | 26.60 ns / 80 B | 824.11 ns / 239 B | 25.51 ns / 80 B | 815.17 ns / 239 B |
| `Task` source, `Task` handler | 54.46 ns / 240 B | 1,630.81 ns / 495 B | 51.04 ns / 160 B | 854.03 ns / 336 B |
| `ValueTask` source and handler | 79.87 ns / 0 B | 1,747.23 ns / 629 B | 73.69 ns / 0 B | 894.73 ns / 343 B |
| Instance, `Task` handler | 37.69 ns / 160 B | 810.40 ns / 239 B | 31.41 ns / 80 B | 31.48 ns / 80 B |
| Instance, `ValueTask` handler | 20.04 ns / 0 B | 836.57 ns / 287 B | 15.36 ns / 0 B | 15.11 ns / 0 B |

- With completed tasks the `Task` source with a `Task` handler takes 2.13 (success) and 2.79 (failure) of the baseline and allocates 80 B more; the `ValueTask` source takes 3.12 and 4.02 with 0 B. With yielding tasks on success the `Task` handler row takes 1.18 of the baseline and the `ValueTask` row 1.27.
- A sync handler on a `Task` source is the cheapest shape: 1.04 of the baseline with completed tasks on success, 0.60 with yielding tasks (the baseline awaits twice).

### JSON (net10)

| Operation | Case | CSE typed | OneOf | Dunet |
|---|---|---|---|---|
| Serialize | Success | 195.8 ns / 80 B | 160.0 ns / 88 B | 229.2 ns / 384 B |
| Serialize | NotFound | 419.5 ns / 304 B | 370.9 ns / 312 B | 464.2 ns / 600 B |
| Deserialize | Success | 293.4 ns / 40 B | 314.0 ns / 168 B | 276.1 ns / 160 B |
| Deserialize | NotFound | 1,068.5 ns / 320 B | 898.9 ns / 432 B | 789.4 ns / 912 B |

- CSE allocates the least in every row on every TFM.
- On net10 OneOf is faster on both Serialize rows (0.82 and 0.88) and on Deserialize NotFound (0.84); it wins Deserialize NotFound on every TFM. Dunet is faster on both Deserialize rows (0.94 and 0.74). Deserializing a NotFound error takes CSE 1,068.5 ns, the slowest of the three (CSE loss).
- On net8 and net9 OneOf allocates more (Serialize Success 344 B, Deserialize NotFound 496 B), and CSE Serialize Success is faster than OneOf on net8 (247.1 vs 340.1 ns).

### Cross-TFM

| Row | Library | net8 | net9 | net10 | net11 |
|---|---|---|---|---|---|
| Create, cached error | CSE typed | 3.5903 ns / 0 B | 2.9322 ns / 0 B | 2.6047 ns / 0 B | 2.6078 ns / 0 B |
| | CSE `Result<int>` | 18.8305 ns / 56 B | 16.5638 ns / 56 B | 14.7804 ns / 56 B | 13.4252 ns / 56 B |
| Match, NotFound | CSE typed | 4.2091 ns | 3.9563 ns | 4.6895 ns | 4.2067 ns |
| | CSE `Result<int>` | 1.6321 ns | 1.6458 ns | 1.6275 ns | 1.6268 ns |
| | ErrorOr | 1.6584 ns | 2.1842 ns | 1.6243 ns | 1.6288 ns |
| Bind (`BindComparison`), success | CSE typed, one slot | 6.032 ns | 5.7298 ns | 1.1559 ns | 1.3927 ns |
| | CSE `Result<int>` | 1.379 ns | 1.1069 ns | 0.7201 ns | 0.7545 ns |
| Bind chain, success | CSE typed | 17.264 ns | 17.561 ns | 8.076 ns | 8.285 ns |
| | CSE `Result<int>` | 6.856 ns | 6.362 ns | 6.538 ns | 5.666 ns |
| | ErrorOr | 5.676 ns | 5.114 ns | 4.516 ns | 4.812 ns |
| | CFE | 45.966 ns / 24 B | 15.172 ns | 7.708 ns | 7.689 ns |
| Async `Task` bind, success, completed | CSE typed | 117.06 ns / 240 B | 59.33 ns / 240 B | 56.78 ns / 240 B | 55.78 ns / 240 B |
| | CSE `Result<int>` | 28.88 ns / 160 B | 25.12 ns / 160 B | 21.91 ns / 160 B | 21.47 ns / 160 B |
| Async `Task` bind, success, yielding | CSE typed | 1,736.70 ns / 528 B | 1,665.51 ns / 527 B | 1,622.01 ns / 527 B | 1,088.30 ns / 525 B |
| | CSE `Result<int>` | 1,407.33 ns / 384 B | 1,353.80 ns / 384 B | 1,361.94 ns / 384 B | 946.30 ns / 384 B |
| JSON Deserialize, NotFound | CSE typed | 1,250.8 ns / 320 B | 1,189.2 ns / 320 B | 1,068.5 ns / 320 B | 997.2 ns / 320 B |
| | OneOf | 1,075.8 ns / 496 B | 1,009.2 ns / 496 B | 898.9 ns / 432 B | 849.1 ns / 432 B |

Match, switch and Bind rows without bytes allocate 0 B.

- Bind on net8 and net9 is the largest typed loss. In `BindComparison` the one-slot typed Bind takes 5.5150 to 7.489 ns against 1.1069 to 2.386 ns for `Result<int>`. In `TypedBindBenchmarks` the typed rows take 17.173 to 21.215 ns on success against 4.989 and 5.506 ns. On net10 and net11 the gap is at most 1.12 ns (net10 `TypedBindBenchmarks` failure, 5.116 vs 3.992 ns). net9 runs the net9.0 build of Results, so this is not only the netstandard2.1 build that net8 gets. It was not investigated.
- On net8 the typed async `Task` rows with completed tasks take about twice their net9 time (`Task` bind failure 98.85 vs 52.45 ns); `Result<int>` changes less (21.67 vs 19.73 ns).
- The yielding rows of `TypedAsyncBindComparison` take 0.66 to 0.78 of their net10 time on net11, for every library, as in the rest of this suite.
- Full reports: `benchmarks/results/results-comparison/{tfm}/results/` and `benchmarks/results/results-pattern/{tfm}/results/`, for example [TypedMatchComparison net10](../benchmarks/results/results-comparison/net10.0/results/CSharpEssentials.Results.Comparison.Benchmarks.TypedMatchComparison-report-github.md).

### Where the typed result loses

- Match: `Result<int>`, ErrorOr and OneOf on every case and TFM (net10 NotFound 4.6895 ns against 1.6275, 1.6243 and 1.9141 ns). CFE on every case on net10 and net11 (net10 NotFound 2.4930 ns) and on Success and NotFound on net8 and net9 (net8 NotFound 3.1136 vs 4.2091 ns). Two- and three-slot Match costs 2.14 to 3.20 of `Result<int>` Match on every TFM.
- Bind on net8 and net9: 5.5150 to 7.489 ns against 1.1069 to 2.386 ns for `Result<int>` (`BindComparison`), and 17.173 to 21.215 ns against 4.989 to 5.506 ns on success (`TypedBindBenchmarks`).
- Bind chain: `Result<int>` and ErrorOr on both paths on every TFM, CFE on failure on every TFM and on success on net9 to net11 (net10 success 8.076 ns against 6.538, 4.516 and 7.708 ns).
- Async `Task` bind: 80 B more than `Result<int>` with completed tasks and 2.6 to 3.2 times its time on net10. Slower than ErrorOr and CFE with completed tasks. With yielding tasks on success, 527 B against 384 B (`Result<int>`) and 408 B (CFE) on net10.
- JSON: OneOf faster on Deserialize NotFound on every TFM (net8 1,075.8 vs 1,250.8 ns) and on Serialize on net10 and net11; Dunet faster on Deserialize on net8 to net10 (on net11 only on NotFound: 749.2 vs 997.2 ns).
- Create Ok: `Result<int>` and ErrorOr are 0.26 ns faster on net10, near timer resolution.

### Caveats

- The `Result<int>` Match row reads the first error's type through one failure handler; the typed row dispatches to one of four handlers. They return the same status for every case, but the code paths differ.
- OneOf has no Bind, so its chain rebuilds the union with `Match` at each step. FluentResults reads properties at the end because it has no Match.
- The native union rows exist only on net11, and they are in `TypedCreateComparison` and in the Switch category only.
- Nothing in the typed classes was profiled. The net8 and net9 Bind gap and the async `Task` allocation are measured, not explained.
- Load average 1.1 to 2.7 at the start of each suite, with no end value. Gaps of a few percent are weak evidence even when outside error.

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
| Error type model | Typed `Error` struct: Code, Description, `ErrorType` (8 values), NumericType, `ErrorMetadata` (`Dictionary<string, object?>`). Typed errors through `Result<T, E1>`, `Result<T, E1, E2>` and `Result<T, E1, E2, E3>`, each a `readonly struct` (`CSharpEssentials.Results/ResultTE1.cs` to `CSharpEssentials.Results/ResultTE1E2E3.cs`) | `string` by default (several messages joined with ", "); typed `E` through `Result<T, E>` ([ResultT.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultT.cs)) | Typed `Error` struct: Code, Description, `ErrorType` (7 values), NumericType, `Dictionary<string, object>` metadata ([Error.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/Errors/Error.cs), [ErrorType.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/Errors/ErrorType.cs)) | `IError` class hierarchy with Message, metadata and nested `Reasons` ([Error.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Reasons/Error.cs)) |
| Multiple errors per result | Yes (`Error[]`) | No: one string or one `E`; `Combine` concatenates strings ([Combine.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Combine.cs)) | Yes (`List<Error>`) ([ErrorOr.cs](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.cs)) | Yes (`List<IReason>`) ([ResultBase.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Results/ResultBase.cs)) |
| Maybe / Option type | Yes, separate package CSharpEssentials.Maybe (`readonly struct Maybe<T>`) | Yes, in the package ([Maybe.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Maybe/Maybe.cs)) | No | No |
| Async extension coverage | Map, Bind, Tap, Ensure, Match on `Task` and `ValueTask` sources on every TFM including netstandard2.1; `ValueTask` handlers on instances need .NET 9+ (Readme, "Async overload matrix") | Map, Bind, Tap, Ensure, Match on Task (left, right, both); ValueTask only under `NET5_0_OR_GREATER` ([Extensions](https://github.com/vkhorikov/CSharpFunctionalExtensions/tree/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions)) | Task only: ThenAsync, ThenDoAsync, FailIfAsync, MatchAsync ([ThenExtensions](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.ThenExtensions.cs)) | Task and ValueTask for Bind, Map and Try; no Tap, Ensure or Match ([ResultExtensions.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Extensions/ResultExtensions.cs), [Results.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Factories/Results.cs) for `Try`) |
| Implicit conversions | `T`, `Error`, `Error[]`, `List<Error>`, `HashSet<Error>` → `Result<T>`; `bool` → `Result` (Readme, "Returning failures") | `T` → `Result<T>`; `T` and `E` → `Result<T, E>`; no `string` → `Result<T>` ([ResultT.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultT.cs), [ResultTE.cs](https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultTE.cs)) | `T`, `Error`, `List<Error>`, `Error[]` → `ErrorOr<T>` ([ImplicitConverters](https://github.com/error-or/error-or/blob/v2.1.1/src/ErrorOr/ErrorOr.ImplicitConverters.cs)) | `T`, `Error`, `List<Error>` → `Result<T>`; `Error`, `List<Error>` → `Result` ([Result.cs](https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Results/Result.cs)) |

On the AOT row CSE is the only one of the four that declares `IsAotCompatible`. What still counts against it: the `object?` metadata bag and a five-package dependency chain, against zero dependencies for CFE.

---

## Caveats

- Every CFE number in sections 1 to 9 uses its default `Result<T>` with a `string` error. That is why CFE allocates 0 B on Create and Ensure failure and less on the async failure path. CFE's typed `Result<T, E>` appears only in the typed-result classes.
- The aggregation idioms differ: ErrorOr's documented idiom builds no per-check results, the other three do (section 8). CSE's typed `Result<T>.Combine` was not measured.
- FluentResults has no Ensure, Tap or Maybe and only two of the five async steps, so it has no column there. Its Match column uses property access, the README's idiom.
- CFE's 5.5893 to 9.2949 ns on net8/net9 against about 2 ns on net10/net11, the 4.8533 to 7.1051 ns Maybe rows on net8/net9, and the faster yielding rows on net11 (0.72-0.75× net10) affect libraries as a group and were not investigated. They are reported, not explained.
- Results, Errors and Maybe do not target net8.0, so the net8 column runs CSE's netstandard2.1 assembly against competitor assets built for net8. net8 gaps may reflect that build, not only the runtime.
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
