# Blog notes: CSharpEssentials.Resilience against Polly.Core

These are notes for a post, not the post. The numbers come from the full default-job run under `benchmarks/results/http/{tfm}/results/`; [CSharpEssentials.Resilience/BENCHMARKS.md](../../CSharpEssentials.Resilience/BENCHMARKS.md#results) has every row on every TFM.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The library compared is Polly.Core 8.9.0 (BSD-3-Clause). Microsoft.Extensions.Http.Resilience 10.11.0 (MIT) appears in the HTTP resilience row, which is covered in the Http notes.

## Framing

- Working title: "What does a Result wrapper around Polly cost?"
- CSE.Resilience is built on Polly.Core. Every CSE row and every Polly row in the suite runs the same Polly.Core 8.9.0, so this is not engine against engine. The post measures the wrapper: the `Result` conversion, the try/catch, the extra async layer, and the way CSE nests one pipeline per strategy after the first.
- CSE's case rests on behaviour rather than speed: failed `Result` values count as transient without a hand-written predicate, caller cancellation is never retried, and nothing throws.

## What each library provides

- Polly.Core 8.9.0: `ResiliencePipelineBuilder` with retry, circuit breaker, timeout, fallback, hedging and chaos strategies; `ResiliencePipeline<T>` for typed outcomes; a static-lambda state overload to avoid closures; `ExecuteOutcomeAsync` for a call that does not throw. Telemetry and DI come from Polly.Extensions, and the rate limiter from Polly.RateLimiting.
- CSE: an immutable `ResiliencePolicy` and `ResiliencePolicy<T>` with `WithRetry`, `WithCircuitBreaker`, `WithTimeout` and (generic only) `WithFallback`; `Create(ResiliencePolicyOptions)`; `FromPipeline` and `ToPipeline` for raw Polly; `RetryIfFailed` on a `Func<CancellationToken, Task<Result<T>>>`, with a cached pipeline or a `shouldRetry` predicate.

## Feature matrix

| Feature | CSE | Polly.Core 8.9.0 |
|---|---|---|
| Returns `Result` instead of throwing | Yes | No (`ExecuteOutcomeAsync` returns an `Outcome<T>`) |
| Failed `Result` counted as transient | Built in, skips Unauthorized/Forbidden/NotFound/Validation | Hand-written `ShouldHandle` |
| Composition | Fluent and immutable, one nested pipeline per `With...` after the first | One builder, one pipeline |
| Fallback | `ResiliencePolicy<T>` only | Yes |
| Hedging, chaos, rate limiter | No | Hedging and chaos (rate limiter in Polly.RateLimiting) |
| Telemetry | None | Opt-in (Polly.Extensions) |
| Caller cancellation | Rethrown, never retried | `OperationCanceledException` not handled by default |
| Retry-count name | `maxAttempts` (retries, n + 1 runs) | `MaxRetryAttempts` |

## Methodology

- Four BenchmarkDotNet classes in `benchmarks/CSharpEssentials.Http.Comparison.Benchmarks/` (`PolicyOverheadComparison`, `ComposedPolicyComparison`, `RetryIfFailedComparison`, `ResultBoxingComparison`), each with `[MemoryDiagnoser]`. CSE is the baseline in every category that has a CSE row.
- Every operation is a cached delegate over a completed task. Every retry has a zero delay. No timeout fires and no circuit breaker trips. The failing operation returns a precomputed `Error`, so no row pays for building one.
- Policies and pipelines are built once, except in the labelled per-call rows, which show the Readme Quick Start shape (policy built inline) against Polly's advice to reuse pipelines.
- The bare Polly rows use the documented static-lambda state overload. The typed Polly pipelines get a predicate that mirrors CSE's classifier, and the Polly circuit breaker gets CSE's default values.
- Every class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks without BenchmarkDotNet: the value, the number of calls, and, with an always-failing operation, 4 calls for maxAttempts 3 on every row.

## How these numbers were produced

BenchmarkDotNet v0.14.0, default job, full runs on net8.0, net9.0, net10.0 and net11.0, one TFM after another on one Linux machine, in the same suite run as the Http classes. The 1-minute load average was 1.74 at the start of the suite and 1.60 at the end. No row kept a coefficient of variation above 5%, so none is marked noisy, and the smallest mean (13.63 ns) is well above timer resolution. `RetryIfFailedComparison` was rerun on main after pull requests #230 to #256 landed, with newer runtime patches (.NET 8.0.33, 9.0.22, 10.0.14). Reports: `benchmarks/results/http/{tfm}/results/`.

## The numbers that matter

Headline rows on net10.0, as `Mean, Allocated` from the reports. "-" means no allocation.

| Row (net10.0) | CSE | Polly.Core 8.9.0 |
|---|---|---|
| Empty policy (direct `await`: 13.94 ns, 72 B) | 84.48 ns, 80 B | 58.40 ns, - |
| Retry(3), prebuilt | 275.96 ns, 104 B | 260.21 ns, 24 B |
| Retry(3), built per call | 3,466.70 ns, 6984 B | 3,398.24 ns, 6904 B |
| Retry + circuit breaker + timeout, non-generic, options form | 815.0 ns, 128 B | 633.8 ns, 48 B |
| Same, CSE `FromPipeline` over the single Polly pipeline | 684.1 ns, 128 B | 633.8 ns, 48 B |
| Same, generic `Result<int>` | 781.4 ns, 80 B | 579.5 ns, - |
| `RetryIfFailed`, cached, success | 296.0 ns, 88 B | 227.1 ns, - |
| `RetryIfFailed`, cached, one retry | 472.8 ns, 88 B | 368.2 ns, - |
| `RetryIfFailed` with a predicate, success | 352.3 ns, - | 227.1 ns, - (prebuilt) |
| Non-generic policy over `Result<int>`, one retry | 517.2 ns, 144 B | 373.1 ns, - |
| Generic policy over `Result<int>`, one retry | 412.6 ns, 80 B | 373.1 ns, - |

Reports: [PolicyOverhead](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.PolicyOverheadComparison-report-github.md), [ComposedPolicy](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.ComposedPolicyComparison-report-github.md), [RetryIfFailed](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.RetryIfFailedComparison-report-github.md), [ResultBoxing](../../benchmarks/results/http/net10.0/results/CSharpEssentials.Http.Comparison.Benchmarks.ResultBoxingComparison-report-github.md).

## Where CSE loses (state it, do not soften it)

Measured. CSE is slower than the matching Polly row in every category on every TFM, apart from one row on net11.0 (below), and allocates more everywhere except the `RetryIfFailed` predicate row, which allocates nothing. The full list is in [the Results section](../../CSharpEssentials.Resilience/BENCHMARKS.md#where-cse-loses).

- The wrapper itself. Every `ExecuteAsync` is an async method with a try/catch around Polly's call (`CSharpEssentials.Resilience/ResiliencePolicy.cs:64-129`, `ResiliencePolicyT.cs:38-79`), so each call runs one more async state machine than the Polly row. CSE now passes Polly a static lambda with the operation as state, the same shape as the Polly rows, so there is no closure. Empty policy: 84.48 ns and 80 B against 58.40 ns and nothing on net10.0.
- Nesting. Each `With...` after the first builds a separate pipeline and `Merge` wraps the two in a third (`ResiliencePolicy.cs:131-145`). Retry + circuit breaker + timeout through CSE's options: 815.0 ns against 633.8 ns for one Polly builder on net10.0. `FromPipeline` over that one Polly pipeline is 684.1 ns, so the gap is part wrapper and part nesting.
- Boxing in the non-generic policy. The retry and circuit-breaker predicates test `args.Outcome.Result is IResultBase` on an `Outcome<object>` (`Modules/ResiliencePolicy.Retry.cs:20-22`, `Modules/ResiliencePolicy.CircuitBreaker.cs:24`), so the `Result<T>` struct is boxed on every attempt: 112 B with one attempt and 144 B with two, against 80 B for the generic policy, on every TFM.
- The predicate form of `RetryIfFailed` now reuses a cached pipeline and passes the predicate through a pooled `ResilienceContext` (`Extensions/ResilienceResultExtensions.cs:115-117`), so it allocates nothing. It is still the slowest `RetryIfFailed` row: 352.3 ns on net10.0, against 296.0 ns for the cached overload and 227.1 ns for Polly.
- Per-call construction: the Readme Quick Start builds the policy inline. On net8.0 to net10.0 that costs about the same in CSE and Polly (3,466.70 ns against 3,398.24 ns on net10.0), a ratio of 12.56 against the prebuilt CSE retry on net10.0.

## Where the order flips

No resilience row favors CSE on every TFM. The one row where Polly is slower is retry built per call on net11.0: CSE 3,533.64 ns against Polly 4,351.67 ns. Both rows have a low error, but on net8.0 to net10.0 the two are close and CSE is the slower one, so this is one measurement to recheck, not a result to claim.

Features only CSE ships (not a speed claim):
- Failed `Result` values as transient failures, with no predicate to write.
- A policy that never throws, and cancellation that is never retried.

## Caveats to keep in the post

- These are overhead numbers on operations that complete at once. A real operation takes far longer.
- Only the success path and one retry are timed.
- The suite pins Polly.Core 8.9.0; the CSE package accepts `[8.0.0,9.0.0)`.
- The CSE column is the repository source at the time of the run (a local integration of all open pull requests), not the published package.

## Possible angles

- "The price of a Result": what wrapping Polly in `Result` costs per call, and when that matters.
- "Build once": the per-call rows, for both libraries.
- "Nesting is not free": three pipelines against one, and `FromPipeline` to split nesting from the wrapper.
