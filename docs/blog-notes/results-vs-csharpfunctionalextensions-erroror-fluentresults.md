# Blog notes: CSharpEssentials.Results vs CSharpFunctionalExtensions vs ErrorOr vs FluentResults

Raw material for a post. These notes are not the post. Every number comes from `CSharpEssentials.Results/BENCHMARKS.md`, `CSharpEssentials.Errors/BENCHMARKS.md` and `CSharpEssentials.Maybe/BENCHMARKS.md`, which read `benchmarks/results/results-comparison/{tfm}/results/` (and `results-pattern` for the async overload suite). Full BenchmarkDotNet default-job runs on an AMD Ryzen 5 5600 running a desktop session. The 1-minute load average was 1.41 to 2.65 at the start and end of the main runs and peaked at 4.72 during a rerun. The SDK was 11.0.100-rc.1, with BenchmarkDotNet 0.14.0. Primary runtime is .NET 10.0.11. Competitors: CSharpFunctionalExtensions 3.7.0 (CFE), ErrorOr 2.1.1, FluentResults 4.0.0.

## The problem

- Exceptions for expected failures (validation, not found, conflict) are costly and invisible in signatures.
- A result type makes failure part of the return type. The cost question is what each success, failure and chained step allocates, because these calls sit on every request path.
- The second question is what the library pulls into the app: dependencies, reflection, trimming and Native AOT behavior.

## What the framework already gives you

- Nothing built in. .NET has no `Result<T>` or `Option<T>` type.
- `Nullable<T>` and nullable reference annotations cover "maybe absent" for single values, not "failed with these errors".
- Tuples, `bool TryX(out T)` and `ProblemDetails` cover parts of the pattern by convention.

## What each library adds

- CSharpEssentials.Results (CSE).
  - `readonly record struct Result` / `Result<T>`; failure holds an `Error[]`.
  - Typed `Error` struct with code, description, `ErrorType` and `object?` metadata.
  - Map, Bind/Then, Ensure, Tap, Match, with Task and ValueTask overloads on every TFM.
  - `CombineAll` / `Combine` for aggregation, `Try`/`TryAsync`. Maybe lives in a separate package.
  - Depends on CSharpEssentials.Errors and Json, which bring Core and Enums.
- CSharpFunctionalExtensions 3.7.0.
  - `readonly struct Result<T>` with a `string` error by default; `Result<T, E>` for typed errors.
  - One error per result; `Combine` joins messages with ", ".
  - Maybe in the same package. No runtime dependencies.
- ErrorOr 2.1.1.
  - `readonly record struct ErrorOr<T>`, typed `Error` struct, `List<Error>`.
  - Then/ThenDo/FailIf/Match; Task only. No Maybe.
  - Aggregation idiom in the README is a hand-filled `List<Error>`.
- FluentResults 4.0.0.
  - `Result` / `Result<T>` are classes; errors are an `IError` class hierarchy with nested reasons.
  - Bind, Map, Merge, Try. No Ensure, Tap, Match or Maybe.
  - Depends on Microsoft.Extensions.Logging.Abstractions and System.Threading.Tasks.Extensions.

## Main numbers (net10 unless stated)

1. Chaining is free for the three struct libraries. Map, Bind, Tap and Match allocate 0 B for CSE, CFE and ErrorOr, under 2.9 ns each. FluentResults allocates 88-344 B per step (Map success 256 B / 58.5485 ns).
2. A failure costs CSE 56 B. Create failure: 27.9547 ns / 56 B for CSE, 19.5903 ns / 88 B for ErrorOr, 0.3016 ns / 0 B for CFE (string error kept inline), 93.4794 ns / 344 B for FluentResults. The net10 CSE time is an outlier: on net8, net9 and net11 CSE is faster than ErrorOr (net11 9.9265 vs 16.9649 ns). Ensure failure: 12.903 ns / 56 B for CSE, 23.776 ns / 88 B for ErrorOr, 1.807 ns / 0 B for CFE. CSE loses to CFE on both, on every TFM.
3. Async 5-step pipeline. Success with completed tasks: CSE 89.92 ns / 480 B, CFE 101.11 ns / 480 B, ErrorOr 107.73 ns / 560 B; CSE is fastest on every TFM. Failure at step 1: CSE 106.24 ns / 536 B against CFE 103.39 ns / 480 B (CSE loss on net8 to net10; on net11 CSE is faster, 101.35 vs 103.56 ns). Yielding: CSE allocates 904 B (success) and 960 B (failure) against 880 B for CFE.
4. Error aggregation, three failing checks. CSE 99.09 ns / 336 B, ErrorOr 42.79 ns / 184 B (ratio 0.43), FluentResults 212.03 ns / 712 B (2.14), CFE 368.58 ns / 784 B (3.72). ErrorOr's idiom builds no per-check results, so it does less work. CSE beats CFE and FluentResults on both paths, on every TFM.
5. Maybe. CSE and CFE tie: 0 B everywhere, every net10 operation at most 1.1824 ns, gaps under 0.1 ns. On net9 CSE's Bind on None is 1.27 ns faster (5.5385 vs 6.8108 ns). Most Create rows and three GetValueOrDefault rows (None on net9 and net10, Some on net11) are at timer resolution; several of them, and CFE's net10 Create on Some (0.4084 ns), are noisy (CV above 5%).
6. Sequence of 100 typed results, all success: CSE 268.480 ns / 1640 B, ErrorOr's hand-written loop 229.394 ns / 1216 B, CFE 854.432 ns / 2344 B, FluentResults 2,311.158 ns / 6528 B. ErrorOr wins every `Sequence` row on every TFM.
7. `Try` on a throwing parse: the throw dominates. CSE 2,399.324 ns / 520 B, CFE 2,452.483 ns / 464 B, ErrorOr's hand-written `try`/`catch` 2,352.819 ns / 552 B, FluentResults 2,642.237 ns / 832 B. CSE's Readme idiom `Error.Exception(ex)` takes 7,742.162 ns / 5569 B, because it copies the stack trace text.

## AOT and dependencies

- CSharpEssentials.Results, Errors and Maybe declare `IsAotCompatible` on their net9.0+ targets, not on netstandard2.1 (so do Core, Enums and Json), so the trim analyzer runs on them at build time. None of the three competitors declares it. No AOT publish was run for these notes, so do not claim zero warnings.
- `Result<T>.ToString()` formats the value with `IFormattable` and the invariant culture. It no longer goes through JSON, and it is the fastest `Result` `ToString` of the four (11.61 ns / 48 B, CFE 34.73 ns / 48 B).
- `ErrorMetadata` is a `Dictionary<string, object?>`, so serializing errors with metadata needs reflection-based System.Text.Json or a custom JSON context.
- Installing Results brings Errors, Json, Core and Enums (plus the Enums generator and analyzers). CFE brings nothing.
- For a thin layer: the result types are readonly record structs, chaining allocates 0 B, and the packages declare `IsAotCompatible`. Against: the `object?` metadata and four transitive packages.

## Errors (net10 unless stated)

- Creating an error: CSE 5.771 ns / 0 B, ErrorOr 5.280 ns / 0 B, FluentResults 28.055 ns / 152 B (a class with an empty metadata dictionary and reasons list, no code or type). With three metadata entries: 64.585 / 60.802 / 68.929 ns, 216 / 216 / 288 B.
- Equality: `Equals` ties or beats ErrorOr except one net11 row (0.122 ns). `GetHashCode` is slower than ErrorOr on most TFMs (net8 41.338 vs 27.051 ns without metadata), and building a `HashSet` of 100 errors is slower on every TFM (net10 10,173.842 vs 5,407.757 ns, both 5392 B). That net10 CSE row and ErrorOr's net10 `GetHashCode` row (74.036 ns) are outliers. CSE's hash no longer depends on metadata insertion order; ErrorOr's still does.
- `Error.Exception(ex)` is the slowest row by design: 66.286 ns / 216 B for a never-thrown exception, 2,719.927 ns / 4720 B for a thrown one (the stack trace text). `Error.Failure(type name, message)` costs the same as ErrorOr (5.215 vs 5.229 ns), except on net8, where it is slower (17.94 vs 13.32 ns).
- `ErrorMetadata.ToString()` loses to a hand-written `StringBuilder` loop at 1, 8 and 32 entries on every TFM (32 entries: 1,723.50 ns / 4880 B vs 736.66 ns / 2400 B). Both grow about linearly.
- `ToString` of an error without metadata: same text and 816 B as ErrorOr, 175.50 vs 170.33 ns. With metadata CSE is the slowest (498.34 ns / 2008 B) because it formats every entry.
- JSON serialize: same text and 272 B as ErrorOr; 280.2 vs 280.3 ns. Deserialize is CSE only (492.1 ns / 304 B). ErrorOr silently comes back as `default`, and FluentResults throws.

## Maybe collection helpers (net10 unless stated)

- `TryFirst` on a lazy sequence: CSE 1.150 ns / 0 B, CFE 4,360.931 ns / 16600 B, because CFE copies the source to a list first.
- `TryLast` on a `List` now reads by index: 2.995 ns for CSE, 7.518 ns for CFE.
- On net8 and net9 the CSE search helpers are slower than CFE on a `List` (net9 `TryFirstMiss`: 6,075.384 vs 3,546.207 ns) and `TryFirst` allocates 40 B. On net10 and net11 that cost is gone.
- `TryFind` does two lookups in both libraries: 21.515 ns against 11.004 ns for a hand-written `TryGetValue`.
- `Choose`: both libraries take 7-13× the hand-written loop (3,467.147 vs 449.589 ns).

## Caveats to keep in the post

- CFE was measured with its default string error. Its 0 B failure paths come from that; a typed `Result<T, E>` was not benchmarked.
- Aggregation idioms differ. Compare the ErrorOr row as "what its README tells you to write", not as the same work. CSE's typed `Result<T>.Combine(r1, r2, r3)` was not measured.
- FluentResults has no column for Ensure, Tap, the async pipeline and Maybe, because it has no matching API.
- Most single-step rows are under 2 ns. Differences under 1 ns are noise, and rows below about 0.1 ns are at timer resolution.
- Runtime effects not investigated: CFE takes 7-9 ns per step on net8/net9 and about 2 ns on net10/net11; the yielding pipeline is 1.90-1.94× slower on net11 for CSE and CFE and 1.66-1.68× for ErrorOr, which puts all three within error there; CSE's Create failure takes 27.9547 ns on net10 against 9.9265 to 13.8671 ns elsewhere.
- CSE has no net8.0 target, so the net8 column runs its netstandard2.1 build against competitor assets built for net8.
- Load average 1.41 to 2.65 with a desktop session, 4.72 at one rerun start. Async gaps of a few percent are weak evidence.

## Possible angles

- "Struct results cost nothing until something fails": the 0 B chaining rows against FluentResults, then the 56 B / 88 B / 0 B failure row, and what a string error buys and costs.
- "Where CSE loses": failure creation and the async failure path against CFE, aggregation and `Sequence` against ErrorOr's hand-written idiom, `GetHashCode` and `HashSet` against ErrorOr, metadata `ToString` against a hand-written loop, and the net8/net9 Maybe search helpers against CFE. Name the losses first.
- "The async overloads": the `Task`-source overloads with a sync handler take 0.60-1.15× the hand-written `await` baseline on net10, while `ValueTask` sources on the failure path with completed tasks take 1.66-3.62× across all TFMs and allocate 56 B (136 B with a `Task` handler).
- "Dependencies are part of the cost": the package chain set against CFE's zero dependencies, now with `IsAotCompatible` on the CSE side.
