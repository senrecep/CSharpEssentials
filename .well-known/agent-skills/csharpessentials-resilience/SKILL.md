---
name: csharpessentials-resilience
description: Use when adding transient fault handling around Result-based operations. Covers ResiliencePolicy/ResiliencePolicy<T> retry, timeout, circuit breaker and fallback on Polly.Core v8, RetryIfFailed with an optional shouldRetry predicate, Result-aware retry filtering and caller cancellation that throws OperationCanceledException.
---

# CSharpEssentials.Resilience

Retry, timeout, circuit breaker and fallback for `Result` and `Result<T>` operations, without Polly types in application code.

## Installation

```bash
dotnet add package CSharpEssentials.Resilience
```

Depends on `Polly.Core` `[8,9)` (the v8 `ResiliencePipeline` API), not the full `Polly` package. Add `Polly` yourself if you use the v7 API (`Policy`, `AsyncRetryPolicy`).

## Namespace

```csharp
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;  // Result, Result<T>
using CSharpEssentials.Errors;         // Error, ErrorType
```

## When to use

| Scenario | Use this package? |
|----------|-------------------|
| Retry, timeout, circuit breaker around `Result<T>` operations | Yes |
| Fallback values after retries are exhausted | Yes |
| HTTP-specific resilience (redirects, status code mapping) | No. Use `CSharpEssentials.Http` |
| Exception-only retry logic with no `Result` | Polly directly is simpler |

## Key types

| Type | Description |
|------|-------------|
| `ResiliencePolicy` | Non-generic policy; `ExecuteAsync` accepts `Task`, `Task<T>`, `Task<Result>` and `Task<Result<T>>` callbacks |
| `ResiliencePolicy<T>` | Generic policy for `Result<T>` operations; adds `WithFallback` |
| `ResiliencePolicyOptions` | `Retry`, `Timeout`, `CircuitBreaker` records for the non-generic `Create(options)` |
| `RetryOptions` | `MaxAttempts` (3), `Delay` (1 s), `ExponentialBackoff` (true) |
| `TimeoutOptions` | `Timeout` (30 s) |
| `CircuitBreakerOptions` | `MinimumThroughput` (10), `SamplingDuration` (1 min), `BreakDuration` (30 s), `FailureRatio` (0.5) |

Both policies are immutable structs: each `With...` call returns a new policy. `default(ResiliencePolicy)` and `default(ResiliencePolicy<T>)` behave like `Create()`. `Create(Action<ResiliencePipelineBuilder>)`, `FromPipeline(...)` and `ToPipeline()` interoperate with raw Polly pipelines.

## Builder

```csharp
Result<User> user = await ResiliencePolicy
    .Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromSeconds(1))
    .WithTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync(ct => users.GetAsync(id, ct));

ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
    .WithRetry(maxAttempts: 3)
    .WithCircuitBreaker(minimumThroughput: 10);

Result<int> count = await policy.ExecuteAsync(ct => users.CountAsync(ct));

// Options form (non-generic policy)
ResiliencePolicy fromOptions = ResiliencePolicy.Create(new ResiliencePolicyOptions
{
    Retry = new RetryOptions { MaxAttempts = 5 },
    Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(10) }
});
```

- `maxAttempts` is the number of retries after the first attempt, so the operation runs at most `maxAttempts + 1` times.
- `WithRetry` takes `maxAttempts` (3), `delay` (1 s), `exponentialBackoff` (`true`). `WithCircuitBreaker` takes `minimumThroughput` (10), `samplingDuration` (1 min), `breakDuration` (30 s), `failureRatio` (0.5). `WithTimeout` accepts 1 second to 1 day (Polly throws `ValidationException` otherwise).
- Strategies nest in the order you add them, the first one outermost: `WithRetry(3).WithTimeout(t)` times out each attempt, `WithTimeout(t).WithRetry(3)` limits all attempts together. `WithFallback` always wraps the strategies of the policy it is called on.
- Build a policy once and reuse it; building per call costs far more than executing.

## Delegate extensions

```csharp
// Wraps any Func<Task<T>> in a Result (exceptions become failures)
Func<Task<User>> getUser = () => users.GetAsync(id, CancellationToken.None);
Result<User> user = await getUser.ExecuteAsync();

// RetryIfFailed: retries retryable Result failures and exceptions
Func<CancellationToken, Task<Result<User>>> operation = ct => users.TryGetAsync(id, ct);
Result<User> retried = await operation.RetryIfFailed(maxAttempts: 3);

// A predicate replaces the default classification
Result<User> conflictsOnly = await operation.RetryIfFailed(
    shouldRetry: error => error.Type == ErrorType.Conflict,
    maxAttempts: 5);
```

`RetryIfFailed` exists for `Func<CancellationToken, Task<Result<T>>>` and `Func<CancellationToken, Task<Result>>`, with and without `shouldRetry`. Both forms reuse a cached pipeline keyed by `(maxAttempts, delay, exponentialBackoff)`; the predicate form passes the delegate through a pooled `ResilienceContext`. Exceptions reach the predicate as `ErrorType.Unexpected` errors. Keep the predicate pure: it can run more than once per attempt.

## Retry filtering

Both policies (and the circuit breaker) treat exceptions and failed `Result` / `Result<T>` values as failures, but do not retry these error types:

- `ErrorType.Unauthorized`
- `ErrorType.Forbidden`
- `ErrorType.NotFound`
- `ErrorType.Validation`

`Conflict`, `Failure` and `Unexpected` failures are retried.

## Cancellation

Cancelling the caller's `CancellationToken` is never converted to an error: `ExecuteAsync`, `RetryIfFailed` and `WithFallback` throw `OperationCanceledException`, also when the cancel lands during a backoff delay or after the last retryable failure. An `OperationCanceledException` from another token (for example an `HttpClient` timeout) is a normal, retryable failure.

## Error codes

| Code | When |
|---|---|
| `Resilience.Timeout` | Operation exceeded the configured timeout |
| `Resilience.CircuitBroken` | Circuit breaker is open |

Both are `ErrorType.Failure` errors returned as failed `Result` values, not thrown. Other exceptions become `ErrorType.Unexpected` errors with the exception type name as code.

## Fallback

`WithFallback` exists on `ResiliencePolicy<T>` only and runs after the strategies before it are exhausted. It accepts `Func<CancellationToken, Task<T>>` or `Func<CancellationToken, Task<Result<T>>>`. An exception from the fallback delegate becomes an `Unexpected` error.

```csharp
Result<Product> product = await ResiliencePolicy<Product>
    .Create()
    .WithRetry(maxAttempts: 3)
    .WithFallback(ct => cache.GetProductAsync(id, ct))
    .ExecuteAsync(ct => catalog.GetProductAsync(id, ct));
```

## Guidelines

- Use `ResiliencePolicy<T>` for operations that already return `Result<T>`; do not wrap `Result<T>` again. The non-generic policy boxes a returned `Result<T>` on every attempt.
- Pass the `CancellationToken` through the callback so timeout and cancellation work.
- Return `NotFound`, `Validation`, `Unauthorized` or `Forbidden` errors for permanent failures so they are not retried, or pass `shouldRetry` to `RetryIfFailed`.
- Keep Polly types inside the package boundary; expose `ResiliencePolicy` from application and library code.
