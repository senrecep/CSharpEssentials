# CSharpEssentials.Resilience: Performance benchmarks (draft)

This page compares CSharpEssentials.Resilience (CSE) with Polly.Core 8.9.0, the engine CSE is built on. It covers the cost of a policy around an operation that does nothing, composed policies, retrying failed `Result` values, and the boxing of `Result` in the non-generic policy. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The classes live in the Http comparison project, which also holds the CSharpEssentials.Http classes and the Microsoft.Extensions.Http.Resilience rows; those are documented in [CSharpEssentials.Http/BENCHMARKS.md](../CSharpEssentials.Http/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

The competitor is pinned exactly in `Directory.Packages.props`, and the pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License (nuspec expression) | Asset the suite loads |
|---|---|---|---|---|
| Polly.Core | 8.9.0 | 2026-10-28 | BSD-3-Clause | `lib/net8.0` on net8 and net9, `lib/net10.0` on net10 and net11 |

CSE is a wrapper over Polly.Core, so this is not a contest between two engines. Every row, CSE and Polly alike, runs Polly.Core 8.9.0. The repo pins Polly.Core to `[8.0.0,9.0.0)`, which resolves to 8.0.0; for this suite only, `Directory.Packages.props` pins 8.9.0, so CSE runs on the same engine as the bare Polly rows. The numbers then show what CSE's wrapper adds on top of Polly: the `Result` conversion, the try/catch, the extra async layer and the pipeline nesting.

| TFM | Polly.Core | CSE build |
|---|---|---|
| net8.0 | 8.9.0 | netstandard2.1 |
| net9.0 | 8.9.0 | net9.0 |
| net10.0 | 8.9.0 | net10.0 |
| net11.0 | 8.9.0 | net11.0 |

`--self-check` prints the loaded Polly.Core version and the TFM as its first line, which is where this table comes from. CSE.Resilience has no net8 target, so net8 runs its netstandard2.1 build.

---

## What was measured

The code is in `benchmarks/CSharpEssentials.Http.Comparison.Benchmarks/`, in four BenchmarkDotNet classes for this package.
- Every class has `[MemoryDiagnoser]`.
- CSE is the baseline in every category that has a CSE row.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check. Every row must give the same result, or the run fails.
- Each Polly row has a comment above it that links to the Polly 8.9.0 documentation it follows. The bare Polly rows use the documented static-lambda state overload, `ExecuteAsync(static (op, token) => new ValueTask<T>(op(token)), op, token)`.
- Every operation is a delegate cached in a field over an already completed task, so the numbers are the policy cost alone. Every retry has a zero delay. No timeout fires (30 s) and no circuit breaker trips (10 calls minimum, 0.5 failure ratio, 1 minute sampling, 30 s break, the CSE defaults).
- Policies and pipelines are built once in setup, except in the labelled per-call rows.

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `PolicyOverheadComparison` | A `Func<CancellationToken, Task<int>>` that returns 42 | Empty: a direct `await` (reference), `ResiliencePipeline.Empty`, CSE `ResiliencePolicy.Create()`. Retry: CSE `WithRetry(3, 0)` and a Polly retry pipeline, both prebuilt; RetryPerCall (labelled): both built on every call | 42 on every row |
| 2 | `ComposedPolicyComparison` | Retry(3) then circuit breaker then timeout(30 s), success path | NonGeneric: CSE `Create(ResiliencePolicyOptions)`, CSE fluent `WithRetry().WithCircuitBreaker().WithTimeout()`, CSE `FromPipeline(<the Polly pipeline>)`, one Polly builder with the three strategies. Generic (`Task<Result<int>>`): CSE `ResiliencePolicy<int>` fluent, a Polly `ResiliencePipelineBuilder<Result<int>>` | 42 on every row |
| 3 | `RetryIfFailedComparison` | A `Func<CancellationToken, Task<Result<int>>>`, maxAttempts 3. `FailFirst` = false and true | CSE `RetryIfFailed` (cached pipeline), CSE `RetryIfFailed(shouldRetry: ...)`, a prebuilt Polly `ResiliencePipeline<Result<int>>` | 42 and the call count (1, or 2 with `FailFirst`). An untimed case with an always-failing operation checks 4 calls on every row |
| 4 | `ResultBoxingComparison` | Retry(3) over a `Task<Result<int>>`, `FailFirst` = false and true | CSE non-generic `ResiliencePolicy`, CSE `ResiliencePolicy<int>`, the Polly pipeline from class 3 | 42 and the call count |

### Inputs

- The success operations return `Task.FromResult(42)` or `Task.FromResult(Result<int>.Success(42))`, both created once.
- With `FailFirst` the operation returns a precomputed `Error.Unexpected("Bench.Transient")` failure on its first call and the success on the second. The call counter is reset at the start of every benchmark call. The failure is built once, so no row pays for creating an `Error`.
- The Polly `Result<int>` pipelines use a hand-written `ShouldHandle` that mirrors CSE's classifier: any exception, or a failed `Result` whose first error is not Unauthorized, Forbidden, NotFound or Validation.
- The Polly circuit breaker gets the same values as CSE's defaults: `MinimumThroughput = 10`, `FailureRatio = 0.5`, `SamplingDuration = 1 min`, `BreakDuration = 30 s`.

---

## Semantic differences

- CSE returns a `Result`; Polly returns the value or throws. Every CSE row wraps the outcome in `Result<int>` inside a try/catch. The Empty category shows that cost on its own.
- Retry count. CSE's `maxAttempts` is the number of retries, like Polly's `MaxRetryAttempts`: maxAttempts 3 means 4 runs. The always-failing parity case checks this for CSE and Polly alike.
- What counts as transient: CSE retries exceptions (except caller cancellation) and failed `Result` values (except Unauthorized, Forbidden, NotFound and Validation). Polly's default retry handles every exception except `OperationCanceledException`, and nothing else. In the non-generic rows, which never fail, that difference is not exercised. The `Result<int>` Polly rows get a hand-written predicate that mirrors CSE's.
- Circuit breaker predicate: CSE's non-generic circuit breaker also counts failed `Result` values; Polly's default counts exceptions only. No failure reaches the circuit breaker rows.
- Composition: CSE builds one pipeline per `With...` call and nests them through `Merge` (`ResiliencePolicy.cs:131-145`, `ResiliencePolicyT.cs:80-89`). Polly builds one pipeline with all strategies. `CSharpEssentials_FromPipeline` runs that single Polly pipeline through CSE's wrapper, which separates the wrapper cost from the nesting cost.
- The generic `Create()` builds a new empty pipeline (`ResiliencePolicyT.cs:16`), and the generic `Merge` has no empty fast path, so `ResiliencePolicy<int>.Create().WithRetry(...)` nests one more pipeline than the non-generic form.
- Per-call policies: the CSE Readme's Quick Start builds the policy inline, inside the call. Polly's performance guide says to reuse pipelines. The RetryPerCall rows show what building per call costs in both.
- Wrapper closure: every CSE `ExecuteAsync` passes Polly a new capturing async lambda (`ResiliencePolicy.cs:71,87,103,121`; `ResiliencePolicyT.cs:47,70`): one closure and one more async state machine per call than the static-lambda Polly rows.
- Boxing in the non-generic policy: the retry and circuit-breaker predicates test `Outcome<object>.Result is IResultBase` (`Modules/ResiliencePolicy.Retry.cs:20-22`, `Modules/ResiliencePolicy.CircuitBreaker.cs:24`), so `Result<T>` is boxed per attempt; ResultBoxingComparison isolates this.
- The predicate form of `RetryIfFailed` builds a new pipeline on every call by design (`Extensions/ResilienceResultExtensions.cs:113,163-179`); a delegate cannot key the cache safely. The cached overload looks its pipeline up in a `ConcurrentDictionary`.

---

## Feature matrix

This table comes from reading CSE source, the Polly 8.9.0 docs, nuspecs and assembly metadata. None of it was measured. The Microsoft.Extensions.Http.Resilience column is here for context; its benchmark rows are in the Http page.

| Feature | CSE | Polly.Core 8.9.0 | Microsoft.Extensions.Http.Resilience 10.11.0 |
|---|---|---|---|
| Returns `Result` instead of throwing | Yes | No (throws, or `ExecuteOutcomeAsync`) | No (`HttpResponseMessage`) |
| Failed `Result` counted as transient | Built in | Hand-written `ShouldHandle` | Transient HTTP status codes |
| Composition | Immutable fluent policy; each `With...` nests a pipeline | Builder, one pipeline | Builder through DI |
| Fallback | `ResiliencePolicy<T>` only | Yes | Through the builder |
| Hedging, chaos, rate limiter | No | Hedging and chaos; rate limiter in the separate Polly.RateLimiting package | Yes (standard and hedging handlers) |
| Telemetry | None | Opt-in (Polly.Extensions) | On by default |
| DI registry | No | Polly.Extensions `AddResiliencePipeline` | `AddResilienceHandler` |
| Caller cancellation | Rethrows `OperationCanceledException`, never retried | `OperationCanceledException` not handled by default | As Polly |
| Retry-count name | `maxAttempts` (retries, n + 1 runs) | `MaxRetryAttempts` | `MaxRetryAttempts` |
| Trim/AOT assembly metadata | None | `IsTrimmable` (csproj sets IsAotCompatible at tag 8.9.0) | `IsTrimmable` + `IsAotCompatible` |
| License | MIT | BSD-3-Clause | MIT |

---

## Results

**Pending.** No full run has been done yet. The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/http/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- These are overhead numbers. The operations complete synchronously and never wait. With a real operation, a network call or a database query, the policy cost is a small part of the total.
- Only the success path and one retry are timed. Exhausted retries, open circuits and timeouts are not.
- The suite pins Polly.Core 8.9.0, while the CSE package itself accepts `[8.0.0,9.0.0)`. An app that restores an older 8.x gets that version's numbers.
- The CSE column is the working tree on branch `perf/bench-http`, which may not match the version published on NuGet.
- The feature matrix comes from reading source and docs.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project http                       # all TFMs (net8-net11), Http and Resilience classes, default job
./run-benchmarks.sh --project http --tfm net10.0         # one TFM
./run-benchmarks.sh --project http --quick --tfm net10.0 # one TFM, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.Http.Comparison.Benchmarks -- --self-check
```

Without `--tfm`, `--quick` runs on net9.0, the script default. Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/http/{tfm}/results/`.
