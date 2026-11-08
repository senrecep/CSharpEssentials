# CSharpEssentials.Resilience

Retry, timeout, circuit breaker and fallback for operations that return `Result` or `Result<T>`. `ResiliencePolicy` is a fluent wrapper over Polly v8 (`Polly.Core`) that returns failures as `Result` values instead of throwing.

Benchmarks against Polly.Core (policy overhead, composed policies, `RetryIfFailed`, `Result` boxing) are in [BENCHMARKS.md](./BENCHMARKS.md).

## Installation

```bash
dotnet add package CSharpEssentials.Resilience
```

The package depends on `Polly.Core` `[8.0.0,9.0.0)`, not the full `Polly` package. Add `Polly` yourself if you use the v7 API (`Policy`, `AsyncRetryPolicy`).

It is not marked Native AOT compatible. A Native AOT publish gets ILC warning IL3054 from Polly's `DelegatingComponent.ExecuteCore` (generic recursion; ILC says an exception is thrown at run time if that path is reached), and Polly's `AddRetry<TResult>` asks for `DynamicallyAccessedMemberTypes.All` on the result type, which the retry extensions cannot meet for `Result<T>` without trim warnings.

## Namespaces

```csharp
using CSharpEssentials.Resilience;    // ResiliencePolicy, ResiliencePolicy<T>, options records, Func and Result extensions
using CSharpEssentials.ResultPattern; // Result, Result<T>
using CSharpEssentials.Errors;        // Error, ErrorType
```

## Quick start

```csharp
// Retry
Result<User> user = await ResiliencePolicy
    .Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromSeconds(1))
    .ExecuteAsync(_ => _db.GetUser(id));

// Retry, timeout and circuit breaker
Result<Data> data = await ResiliencePolicy
    .Create()
    .WithRetry(3)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .WithCircuitBreaker(minimumThroughput: 10)
    .ExecuteAsync(_ => _api.GetData());

// Circuit breaker and fallback (fallback needs the typed ResiliencePolicy<T>)
Result<Product> product = await ResiliencePolicy<Product>
    .Create()
    .WithCircuitBreaker(minimumThroughput: 10, failureRatio: 0.5)
    .WithFallback(ct => _cache.GetAsync<Product>(id, ct))
    .ExecuteAsync(_ => _productService.GetProduct(id));
```

The samples build the policy inline for brevity. A policy is immutable, so in real code build it once and reuse it: in the benchmarks a retry policy built per call costs 12 to 15 times a prebuilt one.

## Policy builder

`ResiliencePolicy` and `ResiliencePolicy<T>` are immutable structs: every `With...` call returns a new policy. `default(ResiliencePolicy)` and `default(ResiliencePolicy<T>)` behave like `Create()`.

| Method | Parameters (defaults) | Notes |
|--------|-----------------------|-------|
| `WithRetry` | `maxAttempts` (3), `delay` (1 s), `exponentialBackoff` (`true`) | `maxAttempts` counts retries after the first attempt, so the operation runs at most `maxAttempts + 1` times. With `exponentialBackoff` the delay doubles after each retry |
| `WithTimeout` | `timeout` | Polly accepts 1 second to 1 day; any other value throws `ValidationException` when the policy is built |
| `WithCircuitBreaker` | `minimumThroughput` (10), `samplingDuration` (1 min), `breakDuration` (30 s), `failureRatio` (0.5) | Counts the same failures as the retry strategy |
| `WithFallback` | `Func<CancellationToken, Task<T>>` or `Func<CancellationToken, Task<Result<T>>>` | `ResiliencePolicy<T>` only; runs after the other strategies are exhausted |
| `Create(ResiliencePolicyOptions)` | `Retry`, `Timeout`, `CircuitBreaker` records | `ResiliencePolicy` only. `WithRetry(RetryOptions)`, `WithTimeout(TimeoutOptions)` and `WithCircuitBreaker(CircuitBreakerOptions)` take the records on both policies |
| `Create(Action<ResiliencePipelineBuilder>)`, `FromPipeline(pipeline)`, `ToPipeline()` | | Interoperate with raw Polly pipelines (`ResiliencePipelineBuilder<Result<T>>` for `ResiliencePolicy<T>`) |

Strategies nest in the order you add them, the first one outermost. `WithRetry(3).WithTimeout(TimeSpan.FromSeconds(1))` applies the timeout to each attempt, while `WithTimeout(TimeSpan.FromSeconds(1)).WithRetry(3)` limits all attempts together. `WithFallback` always wraps the strategies of the policy it is called on. `Create(ResiliencePolicyOptions)` adds retry, then circuit breaker, then timeout.

```csharp
var options = new ResiliencePolicyOptions
{
    Retry = new RetryOptions { MaxAttempts = 3, Delay = TimeSpan.FromSeconds(1) },
    Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(5) },
    CircuitBreaker = new CircuitBreakerOptions { MinimumThroughput = 10, FailureRatio = 0.5, BreakDuration = TimeSpan.FromSeconds(30) }
};

Result<User> user = await ResiliencePolicy.Create(options).ExecuteAsync(_ => _db.GetUser(id));
```

The records default to the same values as the method parameters, plus `TimeoutOptions.Timeout` = 30 s.

## What is retried

Both policies retry exceptions and failed `Result` / `Result<T>` values, and the circuit breaker counts the same failures. The non-generic `ResiliencePolicy` checks any return value that implements `IResultBase`; other return values count only when the delegate throws.

Errors of type `Unauthorized`, `Forbidden`, `NotFound` and `Validation` are not retried, because running the operation again gives the same answer. Every other failed result (`Failure`, `Unexpected`, `Conflict` and so on) is treated as transient. When the retries run out, the last failed result is returned.

```csharp
Result<Order> order = await ResiliencePolicy
    .Create()
    .WithRetry(maxAttempts: 3)
    .ExecuteAsync(ct => _orderService.TryGetOrder(id, ct)); // returns Result<Order>
```

## Delegate extensions

`ExecuteAsync` on a `Func<Task>`, `Func<Task<T>>` or `Func<Task<Result<T>>>` (with or without a `CancellationToken` parameter) runs it through an empty policy, so exceptions become failed results:

```csharp
Func<Task<User>> getUser = () => _db.GetUser(id);
Result<User> user = await getUser.ExecuteAsync();

Func<CancellationToken, Task<User>> getUserWithToken = ct => _db.GetUser(id, ct);
Result<User> userWithToken = await getUserWithToken.ExecuteAsync(cancellationToken);
```

`RetryIfFailed` retries a `Func<CancellationToken, Task<Result<T>>>` or `Func<CancellationToken, Task<Result>>` with the same classification, parameters and defaults as `WithRetry`:

```csharp
Func<CancellationToken, Task<Result<User>>> operation = ct => _db.TryGetUser(id, ct);
Result<User> result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromSeconds(1));

// Retry only errors you classify as transient
Result<User> classified = await operation.RetryIfFailed(
    shouldRetry: error => error.Code == "Db.Timeout",
    maxAttempts: 3,
    delay: TimeSpan.FromSeconds(1));
```

Both forms reuse a cached pipeline, keyed by `(maxAttempts, delay, exponentialBackoff)` and by whether a predicate is used, up to 64 entries per result type. The predicate form passes the delegate through a pooled `ResilienceContext`, so it does not allocate per call.

`shouldRetry` replaces the default classification. An exception thrown by the operation is converted to the error that would be returned for it (`ErrorType.Unexpected`) and passed to the predicate too, so an exception the predicate rejects is not retried. The predicate can run more than once for the same attempt, so keep it pure and cheap. If it throws, retrying stops and the exception is returned as an `Unexpected` error.

## Errors and cancellation

Resilience failures are returned as `Error` values:

- `Error.Failure("Resilience.Timeout", "Operation timed out.")`
- `Error.Failure("Resilience.CircuitBroken", "Circuit breaker is open.")`

When retries are exhausted, the last exception is returned as `Error.Exception(ex, ErrorType.Unexpected)`, with the exception type name as code. An exception thrown by the `WithFallback` delegate is returned the same way.

Cancellation of the caller's token is never converted to an error:

- When the token passed to `ExecuteAsync` or `RetryIfFailed` is cancelled and the operation throws `OperationCanceledException`, the exception is rethrown and `WithFallback` does not run the fallback.
- If the caller cancels after an attempt returned a retryable failed `Result`, `ExecuteAsync`, `RetryIfFailed` and `WithFallback` throw `OperationCanceledException` instead of returning that result. This holds whether the cancel lands during an attempt, during the backoff delay or right after the final attempt, and also for policies without a retry strategy (circuit breaker only, timeout only).
- If the operation throws an exception other than `OperationCanceledException` while the caller's token is cancelled, that exception is returned as an `Unexpected` error.
- An `OperationCanceledException` from another token, such as an `HttpClient` timeout, is a normal failure: it is retried and it triggers the fallback.

## Polly integration

`Create(Action<ResiliencePipelineBuilder>)` gives direct access to the Polly builder:

```csharp
using Polly;
using Polly.Retry;

Result<Data> result = await ResiliencePolicy
    .Create(builder => builder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential
    }))
    .ExecuteAsync(_ => _api.GetData());
```

A strategy added this way uses Polly's own `ShouldHandle`, which ignores returned `Result` values unless you set it.

## Upgrading to 8.0

An exception thrown by the `WithFallback` delegate is now returned as an `ErrorType.Unexpected` error, like an exception from the operation itself. Earlier versions returned it as `ErrorType.Failure`. A `TimeoutRejectedException` or `BrokenCircuitException` thrown by the fallback delegate now gets the same codes as on the operation, `Resilience.Timeout` or `Resilience.CircuitBroken`, instead of the exception type name.
