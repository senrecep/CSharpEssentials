---
name: csharpessentials-resilience
description: Use when adding transient fault handling around Result-based operations — ResiliencePolicy/ResiliencePolicy<T> for retry, timeout, circuit breaker, and fallback composition backed by Polly v8 with Result-aware retry filtering.
---

# CSharpEssentials.Resilience

HTTP-agnostic resilience patterns for `Result` and `Result<T>`. Compose retry, timeout, circuit breaker, and fallback without leaking Polly types into application code.

## Installation

```bash
dotnet add package CSharpEssentials.Resilience
```

## Namespace

```csharp
using CSharpEssentials.Resilience;
```

---

## When to Use / When NOT to Use

| Scenario | Use this package? |
|----------|-------------------|
| Transient fault handling (retry, timeout, circuit breaker) | ✅ Yes |
| Composing resilience policies around `Result<T>` pipelines | ✅ Yes |
| Fallback values after retries are exhausted | ✅ Yes |
| HTTP-specific resilience (redirects, status code mapping) | ❌ No — use `CSharpEssentials.Http` |
| Non-Result exception-only retry logic | ⚠️ Consider Polly directly for simpler scenarios |

---

## Key Types

| Type | Description |
|------|-------------|
| `ResiliencePolicy` | Non-generic policy for `Result` (unit) operations |
| `ResiliencePolicy<T>` | Generic policy for `Result<T>` operations; supports fallback |
| `ResiliencePolicyOptions` | Options record combining Retry, Timeout, CircuitBreaker |
| `RetryOptions` | MaxAttempts, Delay, ExponentialBackoff |
| `TimeoutOptions` | Timeout duration |
| `CircuitBreakerOptions` | MinimumThroughput, SamplingDuration, BreakDuration, FailureRatio |

---

## Builder Pattern

```csharp
Result<User> user = await ResiliencePolicy
    .Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromSeconds(1))
    .WithTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync(ct => _db.GetUser(id, ct));

ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
    .WithRetry(maxAttempts: 3)
    .WithCircuitBreaker(minimumThroughput: 10);

Result<int> result = await policy.ExecuteAsync(ct => _api.GetValue(ct));
```

---

## Delegate Extensions

```csharp
// Direct execution — wraps any Func<Task<T>> in a Result
Result<User> user = await (() => _db.GetUser(id)).ExecuteAsync();

// RetryIfFailed — retries on transient Result failures
Func<CancellationToken, Task<Result<User>>> operation = ct => _db.GetUser(id, ct);
Result<User> retried = await operation.RetryIfFailed(maxAttempts: 3);
```

---

## Result-Aware Retry Filtering

`ResiliencePolicy<T>` retries exceptions and failed `Result<T>` values, but skips these non-transient error types:

- `ErrorType.Unauthorized`
- `ErrorType.Forbidden`
- `ErrorType.NotFound`
- `ErrorType.Validation`

`Conflict` and unexpected failures are retried.

---

## Error Codes

| Code | When |
|---|---|
| `Resilience.Timeout` | Operation exceeded the configured timeout |
| `Resilience.CircuitBroken` | Circuit breaker is open |

Timeout and circuit-breaker failures are returned as failed `Result` values rather than rethrown.

---

## Fallback

Fallback is available on `ResiliencePolicy<T>` and runs after prior strategies in the pipeline have been exhausted.

```csharp
Result<Product> product = await ResiliencePolicy<Product>
    .Create()
    .WithRetry(maxAttempts: 3)
    .WithFallback(ct => _cache.GetAsync<Product>(id, ct))
    .ExecuteAsync(ct => _catalog.GetProduct(id, ct));
```

---

## Best Practices

- Use `ResiliencePolicy<T>` for operations that already return `Result<T>`; do not wrap `Result<T>` again.
- Always pass the `CancellationToken` through the callback so Polly timeout and cancellation behave correctly.
- Keep Polly namespaces internal to the package boundary; expose `ResiliencePolicy` from application and library code.
- Prefer the HTTP package only for HTTP-specific helpers; general retry/timeout logic belongs in `CSharpEssentials.Resilience`.
