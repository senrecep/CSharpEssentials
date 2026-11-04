# Blog notes: CSharpEssentials.Results vs CSharpFunctionalExtensions vs ErrorOr vs FluentResults

Raw material for a post. These notes are not the post. Every number comes from `CSharpEssentials.Results/BENCHMARKS.md`, which reads `benchmarks/results/results-comparison/{tfm}/results/*.csv`. The machine was an AMD Ryzen 5 5600 running a desktop session, with a 1-minute load average of 0.96-1.44 at the sampled points. The SDK was 11.0.100-rc.1, with BenchmarkDotNet 0.14.0. Primary runtime is .NET 10.0.11. Competitors: CSharpFunctionalExtensions 3.7.0 (CFE), ErrorOr 2.1.1, FluentResults 4.0.0.

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

1. Chaining is free for the three struct libraries. Map, Bind, Tap and Match allocate 0 B for CSE, CFE and ErrorOr, under 2.8 ns each. FluentResults allocates 88-344 B per step (Map success 256 B / 56.9 ns).
2. A failure is not free in CSE. Create failure: 104 B / 23.6 ns for CSE, 88 B / 18.4 ns for ErrorOr, 0 B / 0.34 ns for CFE (string error kept inline), 344 B / 92.7 ns for FluentResults. Ensure failure repeats this: 104 / 88 / 0 B. CSE loses here on every TFM.
3. Async 5-step pipeline. Success with completed tasks: CSE 95.07 ns / 480 B, CFE 104.30 ns / 480 B, ErrorOr 112.01 ns / 560 B. Failure at step 1: CSE 123.45 ns / 584 B against CFE 109.73 ns / 480 B (CSE loss). Yielding failure: 1,048 B vs 879 B. Yielding success: time tie, 943 B vs 879 B.
4. Error aggregation, three failing checks. CSE 237.91 ns / 696 B, ErrorOr 41.86 ns / 184 B (5.7× faster, 3.8× less allocation), FluentResults 205.11 ns / 712 B, CFE 372.08 ns / 784 B. ErrorOr's idiom builds no per-check results, so it does less work.
5. Maybe. CSE and CFE tie: 0 B everywhere, every operation at most 1.14 ns, gaps under 0.3 ns. Exception on net8: Bind on None takes 6.92 ns for CSE vs 5.54 ns for CFE.

## The AOT and dependency gap

- CSharpEssentials.Results, Errors and Maybe do not set `IsAotCompatible`, so the trim analyzer never runs on them. None of the three competitors sets it either, so on paper this is a tie.
- `Result<T>.ToString()` on success calls `ConvertToJson`, reflection-based System.Text.Json annotated `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]`. It would warn under the analyzer.
- `ErrorMetadata` is a `Dictionary<string, object?>`, so serializing errors needs reflection or a custom JSON context.
- Installing Results brings Errors, Json, Core and Enums (plus the Enums generator and analyzers). CFE brings nothing.
- For a thin layer: the result types are readonly record structs and chaining allocates 0 B. Against: the reflection-based `ToString()`, the `object?` metadata, four transitive packages and no `IsAotCompatible`. The "trim and AOT friendly" wording used for the Endpoints and DependencyInjection packages does not apply to Results today. State it as a known gap; no fix is part of this work.

## Caveats to keep in the post

- CFE was measured with its default string error. Its 0 B failure paths come from that; a typed `Result<T, E>` was not benchmarked.
- Aggregation idioms differ. Compare the ErrorOr row as "what its README tells you to write", not as the same work. CSE's typed `Result<T>.Combine(r1, r2, r3)` was not measured.
- FluentResults has no column for Ensure, Tap, the async pipeline and Maybe, because it has no matching API.
- Most single-step rows are under 2 ns. Differences under 1 ns are noise. ErrorOr's Match on failure (to int on every TFM, to string on net8 and net9) is 1.1-1.6 ns faster than CSE. CFE's Maybe Bind on None is faster on net8 (5.54 vs 6.92 ns). These are the competitor wins above 1 ns among the single-step rows.
- Runtime effects not investigated: CFE takes 7-9 ns per step on net8/net9 and about 2 ns on net10/net11; the yielding pipeline is 1.8-1.9× slower on net11 for CSE and CFE and 1.5-1.6× for ErrorOr, so on net11 ErrorOr beats CSE on yielding success (2,876.8 vs 3,009.8 ns); CSE all-fail aggregation took 423 ns on net9 against 214-238 ns elsewhere.
- CSE has no net8.0 target, so the net8 column runs its netstandard2.1 build against competitor assets built for net8.
- Load average 0.96-1.44 with a desktop session. Async gaps of a few percent are weak evidence.

## Possible angles

- "Struct results cost nothing until something fails": the 0 B chaining rows against FluentResults, then the 104 B / 88 B / 0 B failure row, and what a string error buys and costs.
- "Where CSE loses": failure creation, the async failure path against CFE, aggregation against ErrorOr and FluentResults, Match on failure against ErrorOr, Maybe Bind on None against CFE on net8, and the net11 yielding success path against ErrorOr. Name the losses first.
- "Dependencies and AOT are part of the cost": the package chain and the reflection-based `ToString()`, set against CFE's zero dependencies.

## New classes, results pending

Ten classes were added to the same suite after the run above. They have no numbers yet. The full default-job run is pending, and short-job numbers are not used here. The drafts are `CSharpEssentials.Errors/BENCHMARKS.md`, `CSharpEssentials.Maybe/BENCHMARKS.md` and the pending section of `CSharpEssentials.Results/BENCHMARKS.md`.

### ErrorFactoryComparison

- Creating an error, plain and with three metadata entries. CSE and ErrorOr build the same struct shape, so that pair is like-for-like.
- FluentResults allocates a class, an empty metadata dictionary and an empty reasons list per error, and has no code or type. Label it.
- CFE has no column: its default error is a string, so there is nothing to build.
- Angle: what a typed error costs over a string, and whether a struct error stays cheap once metadata is attached.

### ErrorEqualityComparison

- `Equals`, `GetHashCode` and a `HashSet` of 100 errors, CSE against ErrorOr.
- Both hash metadata in insertion order while `Equals` ignores order. Two equal errors can land in different hash buckets. This is a shared bug, worth one honest paragraph.
- FluentResults compares errors by reference, so it cannot be in this table.

### ErrorExceptionComparison

- CSE `Error.Exception(ex)` copies the type name, the stack trace text and the message into metadata. ErrorOr has no bridge, and FluentResults keeps a reference.
- A never-thrown exception makes CSE call `Environment.StackTrace`, a stack walk. A thrown one formats its trace on every call.
- Expect CSE to be the slowest row here. Say it first and explain what the extra work buys.

### ErrorMetadataToStringComparison

- `ErrorMetadata.ToString()` against a hand-written loop at 1, 8 and 32 entries.
- The CSE method is O(n²) because it calls `ElementAtOrDefault(i)` inside a loop. The curve is the point of the class.
- Expect CSE to lose to the hand-written loop, and the gap to grow with the entry count.

### ErrorToStringComparison

- Without metadata, CSE and ErrorOr print the same record text.
- With metadata, and for a success `Result<int>`, every library prints something different. Those rows are labelled.
- CSE `Result<int>.ToString()` goes through reflection-based JSON. This ties back to the AOT section above.
- Expect the CSE `Result` row to lose to CFE because of the JSON call. ErrorOr prints the whole record, so do not assume CSE is the slowest row.

### ErrorJsonComparison

- Serialize: CSE and ErrorOr write the same JSON with default options. Like-for-like.
- Deserialize is CSE only. ErrorOr silently comes back as `default`, and FluentResults throws and its README says not to serialize results.
- Known CSE gaps: metadata does not round-trip to an equal error, and metadata holding an exception throws on serialize.

### SequenceComparison

- Turning N results into one, typed (`Sequence`) and untyped (`CombineAll`), at N = 10 and 100, with all success or one failure.
- CFE `Combine` is lazy, so its row adds `ToArray`. ErrorOr has no API and uses the README's manual list. Both are labelled.

### TryComparison and TryAsyncComparison

- Wrapping `int.Parse` that succeeds or throws. CSE, CFE and FluentResults each turn the exception into a cached error, so they do the same work.
- Expect the exception to dominate the failure rows. The success path and the async state machine are where the libraries differ.
- A second CSE row uses `Error.Exception(ex)`, the Readme idiom. It shows what the detailed error adds on top of the throw.

### MaybeCollectionComparison

- `TryFirst`, `TryLast`, `TryFind` and `Choose`, CSE against CFE and hand-written loops, over a `List` and a lazy sequence of 1000 strings.
- Strings on purpose: with `int` elements both libraries return `Some(0)` on some misses. That is a bug in both, worth a sentence.
- CSE `TryLast` walks the whole list. CFE copies a lazy source to a list for `TryFirst` and `TryLast`. Both `TryFind` implementations do two lookups.
- Expect CSE to lose `TryLast` and `TryLastWhere` on a `List` to CFE, and `TryFind` to the hand-written `TryGetValue`.
- Angle: small helpers that look free can scan or copy the whole input.
