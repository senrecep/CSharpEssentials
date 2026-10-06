---
name: csharpessentials-resilience
description: Use when adding transient fault handling around Result-based operations. Covers ResiliencePolicy/ResiliencePolicy<T> retry, timeout, circuit breaker and fallback on Polly.Core v8, RetryIfFailed with an optional shouldRetry predicate, Result-aware retry filtering and caller cancellation that throws OperationCanceledException.
---

# CSharpEssentials.Resilience

HTTP-agnostic resilience patterns for `Result` and `Result<T>`. Compose retry, timeout, circuit breaker, and fallback without leaking Polly types into application code.

## Installation

```bash
dotnet add package CSharpEssentials.Resilience
```

Depends on `Polly.Core` `[8,9)` (the v8 `ResiliencePipeline` API), not the full `Polly` package. Add `Polly` yourself if you use the v7 API (`Policy`, `AsyncRetryPolicy`).

## Namespace

```csharp
using CSharpEssentials.Resilience;
```

---

## When to Use / When NOT to Use

| Scenario | Use this package? |
|----------|-------------------|
| Transient fault handling (retry, timeout, circuit breaker) | Yes |
| Composing resilience policies around `Result<T>` pipelines | Yes |
| Fallback values after retries are exhausted | Yes |
| HTTP-specific resilience (redirects, status code mapping) | No. Use `CSharpEssentials.Http` |
| Non-Result exception-only retry logic | Consider Polly directly for simpler scenarios |

---

## Key Types

| Type | Description |
|------|-------------|
| `ResiliencePolicy` | Non-generic policy; `ExecuteAsync` accepts `Task`, `Task<T>`, `Task<Result>` and `Task<Result<T>>` callbacks |
| `ResiliencePolicy<T>` | Generic policy for `Result<T>` operations; adds `WithFallback` |
| `ResiliencePolicyOptions` | Options record combining `Retry`, `Timeout`, `CircuitBreaker` (for the non-generic `Create(options)`) |
| `RetryOptions` | `MaxAttempts` (3), `Delay` (1 s), `ExponentialBackoff` (true) |
| `TimeoutOptions` | `Timeout` (30 s) |
| `CircuitBreakerOptions` | `MinimumThroughput` (10), `SamplingDuration` (1 min), `BreakDuration` (30 s), `FailureRatio` (0.5) |

Both policies are immutable structs: each `With…` call returns a new policy. `default(ResiliencePolicy)` and `default(ResiliencePolicy<T>)` behave like `Create()`. `Create(Action<ResiliencePipelineBuilder>)`, `FromPipeline(...)` and `ToPipeline()` interoperate with raw Polly pipelines.

---

## Builder Pattern

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

`maxAttempts` is the number of retries after the first attempt, so the operation runs at most `maxAttempts + 1` times.

---

## Delegate Extensions

```csharp
// Direct execution — wraps any Func<Task<T>> in a Result (exceptions become failures)
Func<Task<User>> getUser = () => users.GetAsync(id, CancellationToken.None);
Result<User> user = await getUser.ExecuteAsync();

// RetryIfFailed — retries retryable Result failures and exceptions
Func<CancellationToken, Task<Result<User>>> operation = ct => users.TryGetAsync(id, ct);
Result<User> retried = await operation.RetryIfFailed(maxAttempts: 3);

// Custom predicate replaces the default classification
Result<User> conflictsOnly = await operation.RetryIfFailed(
    shouldRetry: error => error.Type == ErrorType.Conflict,
    maxAttempts: 5);
```

`RetryIfFailed` exists for `Func<CancellationToken, Task<Result<T>>>` and `Func<CancellationToken, Task<Result>>`, with and without `shouldRetry`. Pipelines for the overloads without a predicate are cached per `(maxAttempts, delay, exponentialBackoff)`.

---

## Result-Aware Retry Filtering

Both `ResiliencePolicy` and `ResiliencePolicy<T>` (and the circuit breaker) treat exceptions and failed `Result` / `Result<T>` values as failures, but skip these non-transient error types:

- `ErrorType.Unauthorized`
- `ErrorType.Forbidden`
- `ErrorType.NotFound`
- `ErrorType.Validation`

`Conflict`, `Failure` and `Unexpected` failures are retried.

> **4.0 breaking change:** the non-generic `ResiliencePolicy` now retries failed `Result` values returned by the callback. In 3.x it only retried exceptions.

---

## Cancellation

Cancelling the caller's `CancellationToken` is never converted to an error: `ExecuteAsync`, `RetryIfFailed` and `WithFallback` throw `OperationCanceledException`, also when the cancel lands during a backoff delay or after the last retryable failure. An `OperationCanceledException` from another token (for example an `HttpClient` timeout) is a normal, retryable failure.

---

## Error Codes

| Code | When |
|---|---|
| `Resilience.Timeout` | Operation exceeded the configured timeout |
| `Resilience.CircuitBroken` | Circuit breaker is open |

Timeout and circuit-breaker failures are returned as failed `Result` values rather than rethrown. Other exceptions become `ErrorType.Unexpected` errors.

---

## Fallback

`WithFallback` is available on `ResiliencePolicy<T>` only and runs after prior strategies in the pipeline have been exhausted. It accepts `Func<CancellationToken, Task<T>>` or `Func<CancellationToken, Task<Result<T>>>`.

```csharp
Result<Product> product = await ResiliencePolicy<Product>
    .Create()
    .WithRetry(maxAttempts: 3)
    .WithFallback(ct => cache.GetProductAsync(id, ct))
    .ExecuteAsync(ct => catalog.GetProductAsync(id, ct));
```

---

## Best Practices

- Use `ResiliencePolicy<T>` for operations that already return `Result<T>`; do not wrap `Result<T>` again.
- Always pass the `CancellationToken` through the callback so timeout and cancellation behave correctly.
- Return `NotFound`/`Validation`/`Unauthorized`/`Forbidden` errors for permanent failures so they are not retried, or pass `shouldRetry` to `RetryIfFailed`.
- Keep Polly namespaces internal to the package boundary; expose `ResiliencePolicy` from application and library code.
- Prefer the HTTP package only for HTTP-specific helpers; general retry/timeout logic belongs in `CSharpEssentials.Resilience`.
