# Blog notes: CSharpEssentials.Resilience against Polly.Core (draft)

These are notes for a post, not the post. The numbers will come from `CSharpEssentials.Resilience/BENCHMARKS.md` once the full default-job run is committed under `benchmarks/results/http/{tfm}/results/`. There are no numbers yet.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The library compared is Polly.Core 8.9.0 (BSD-3-Clause). Microsoft.Extensions.Http.Resilience 10.11.0 (MIT) appears in the HTTP resilience row, which is covered in the Http notes.

## Framing

- Working title: "What does a Result wrapper around Polly cost?"
- CSE.Resilience is built on Polly.Core. Every CSE row and every Polly row in the suite runs the same Polly.Core 8.9.0, so this is not engine against engine. The post measures the wrapper: the `Result` conversion, the try/catch, the extra async layer, and the way CSE nests one pipeline per strategy.
- CSE's case is behaviour, not speed: failed `Result` values count as transient without a hand-written predicate, caller cancellation is never retried, and nothing throws.

## What each library provides

- **Polly.Core 8.9.0:** `ResiliencePipelineBuilder` with retry, circuit breaker, timeout, fallback, hedging and chaos strategies; `ResiliencePipeline<T>` for typed outcomes; a static-lambda state overload to avoid closures; `ExecuteOutcomeAsync` for a call that does not throw. Telemetry and DI come from Polly.Extensions, and the rate limiter from Polly.RateLimiting.
- **CSE:** an immutable `ResiliencePolicy` and `ResiliencePolicy<T>` with `WithRetry`, `WithCircuitBreaker`, `WithTimeout` and (generic only) `WithFallback`; `Create(ResiliencePolicyOptions)`; `FromPipeline` and `ToPipeline` for raw Polly; `RetryIfFailed` on a `Func<CancellationToken, Task<Result<T>>>`, with a cached pipeline or a `shouldRetry` predicate.

## Feature matrix

| Feature | CSE | Polly.Core 8.9.0 |
|---|---|---|
| Returns `Result` instead of throwing | Yes | No (`ExecuteOutcomeAsync` returns an `Outcome<T>`) |
| Failed `Result` counted as transient | Built in, skips Unauthorized/Forbidden/NotFound/Validation | Hand-written `ShouldHandle` |
| Composition | Fluent and immutable, one nested pipeline per `With...` | One builder, one pipeline |
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

## The numbers that matter

Pending the full run. These are the rows to pull:

1. PolicyOverhead Empty: `ResiliencePolicy.Create()` against `ResiliencePipeline.Empty` and a direct `await`. This is the wrapper on its own.
2. PolicyOverhead Retry, prebuilt and per call.
3. ComposedPolicy: options and fluent CSE against one Polly builder, with `FromPipeline` in between to split wrapper cost from nesting cost.
4. RetryIfFailed: cached against predicate, both with `FailFirst` false and true.
5. ResultBoxing: the non-generic policy against the generic one and typed Polly.

## Where CSE is expected to lose (state it, do not soften it)

These predictions come from reading the code. Nothing has been measured yet.

- **The wrapper itself.** Every `ExecuteAsync` passes Polly a new capturing async lambda (`CSharpEssentials.Resilience/ResiliencePolicy.cs:71,87,103,121`; `ResiliencePolicyT.cs:46-52,70`), so each call allocates a closure and runs one more async state machine than the static-lambda Polly row.
- **Nesting.** Each `With...` builds a separate pipeline and `Merge` wraps the two in a third (`ResiliencePolicy.cs:131-145`). Retry + circuit breaker + timeout is three nested pipelines, not one.
- **The generic policy.** `ResiliencePolicy<T>.Create()` builds a new empty pipeline instead of using `Empty` (`ResiliencePolicyT.cs:16`), and the generic `Merge` has no empty fast path (`:80-89`), so even one strategy is nested.
- **Boxing in the non-generic policy.** The retry and circuit-breaker predicates test `args.Outcome.Result is IResultBase` on an `Outcome<object>` (`Modules/ResiliencePolicy.Retry.cs:20-22`, `Modules/ResiliencePolicy.CircuitBreaker.cs:24`), so the `Result<T>` struct is boxed on every attempt.
- **Predicate `RetryIfFailed`** builds a new pipeline on every call (`Extensions/ResilienceResultExtensions.cs:113`, built at `:163-179`). The code comment says this is deliberate.

## Where CSE may win or tie

To be confirmed by the run:
- `CSharpEssentials_FromPipeline` against `Polly_SingleBuilder`, where only the wrapper differs.
- The cached `RetryIfFailed` against prebuilt Polly, where the extra cost is one `ConcurrentDictionary` lookup and the closure.

Features only CSE ships (not a speed claim):
- Failed `Result` values as transient failures, with no predicate to write.
- A policy that never throws, and cancellation that is never retried.

## Caveats to keep in the post

- These are overhead numbers on operations that complete at once. A real operation dwarfs them.
- Only the success path and one retry are timed.
- The suite pins Polly.Core 8.9.0; the CSE package accepts `[8.0.0,9.0.0)`.
- The CSE column is the working tree, not the published package.

## Possible angles

- "The price of a Result": what wrapping Polly in `Result` costs per call, and when that matters.
- "Build once": the per-call rows, for both libraries.
- "Nesting is not free": three pipelines against one.
