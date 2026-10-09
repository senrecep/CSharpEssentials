# CSharpEssentials.Resilience

HTTP-agnostic resilience patterns (Retry, Timeout, Circuit Breaker, Fallback) with `Result<T>` integration. Composable `ResiliencePolicy` builder backed by Polly v8 (`Polly.Core`) for transient fault handling without exceptions.

## Installation

```bash
dotnet add package CSharpEssentials.Resilience
```

## Namespaces

```csharp
using CSharpEssentials.Resilience;    // ResiliencePolicy, ResiliencePolicy<T>, options records, Func and Result extensions
using CSharpEssentials.ResultPattern; // Result, Result<T>
using CSharpEssentials.Errors;        // Error, ErrorType
```

The samples below assume these usings.

## Quick Start

```csharp
using CSharpEssentials.Resilience;

// Simple retry
Result<User> user = await ResiliencePolicy
    .Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromSeconds(1))
    .ExecuteAsync(_ => _db.GetUser(id));

// Retry + Timeout
Result<Order> order = await ResiliencePolicy
    .Create()
    .WithRetry(3)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync(_ => _orderService.GetOrder(id));

// Circuit Breaker + Fallback (fallback needs the typed ResiliencePolicy<T>)
Result<Product> product = await ResiliencePolicy<Product>
    .Create()
    .WithCircuitBreaker(minimumThroughput: 10, failureRatio: 0.5)
    .WithFallback(ct => _cache.GetAsync<Product>(id, ct))
    .ExecuteAsync(_ => _productService.GetProduct(id));
```

## Policy Builder

Both `ResiliencePolicy` and `ResiliencePolicy<T>` are immutable structs: every `With...` call returns a new policy.

| Method | Parameters (defaults) | Notes |
|--------|-----------------------|-------|
| `WithRetry` | `maxAttempts` (3), `delay` (1 s), `exponentialBackoff` (`true`) | `maxAttempts` is the number of retries after the first attempt, so the operation runs at most `maxAttempts + 1` times. With `exponentialBackoff` the delay doubles after each retry |
| `WithTimeout` | `timeout` | Polly accepts 1 second to 1 day; any other value throws `ValidationException` when the policy is built |
| `WithCircuitBreaker` | `minimumThroughput` (10), `samplingDuration` (1 min), `breakDuration` (30 s), `failureRatio` (0.5) | Counts the same failures as the retry strategy |
| `WithFallback` | `Func<CancellationToken, Task<T>>` or `Func<CancellationToken, Task<Result<T>>>` | `ResiliencePolicy<T>` only; runs after the other strategies are exhausted |
| `Create(ResiliencePolicyOptions)` | `Retry`, `Timeout`, `CircuitBreaker` records | `ResiliencePolicy` only. `WithRetry(RetryOptions)`, `WithTimeout(TimeoutOptions)` and `WithCircuitBreaker(CircuitBreakerOptions)` take the records on both policies |
| `Create(Action<ResiliencePipelineBuilder>)`, `FromPipeline(pipeline)`, `ToPipeline()` | | Interoperate with raw Polly pipelines (`ResiliencePipelineBuilder<Result<T>>` for `ResiliencePolicy<T>`) |

Strategies nest in the order you add them: the first one is the outermost. `WithRetry(3).WithTimeout(TimeSpan.FromSeconds(1))` applies the timeout to each attempt, while `WithTimeout(TimeSpan.FromSeconds(1)).WithRetry(3)` limits all attempts together. `WithFallback` always wraps the strategies of the policy it is called on.

## Retrying Failed Results

Both `ResiliencePolicy` and `ResiliencePolicy<T>` retry exceptions and failed `Result` / `Result<T>` values. The circuit breaker counts the same failures. The non-generic `ResiliencePolicy` checks any return value that implements `IResultBase`; other return values count only when the delegate throws.

Errors of type `Unauthorized`, `Forbidden`, `NotFound` and `Validation` are not retried, because running the operation again gives the same answer. Every other failed result (`Failure`, `Unexpected`, `Conflict`, …) is treated as transient. When the retries run out, the last failed result is returned.

```csharp
Result<Order> order = await ResiliencePolicy
    .Create()
    .WithRetry(maxAttempts: 3)
    .ExecuteAsync(ct => _orderService.TryGetOrder(id, ct)); // returns Result<Order>
```

## Delegate Extensions

```csharp
using CSharpEssentials.Resilience;

// Direct execution — wraps any Func<Task<T>> in a Result
Func<Task<User>> getUser = () => _db.GetUser(id);
Result<User> user = await getUser.ExecuteAsync();

// With CancellationToken
Func<CancellationToken, Task<User>> getUserWithToken = ct => _db.GetUser(id, ct);
Result<User> userWithToken = await getUserWithToken.ExecuteAsync(cancellationToken);
```

## Result Retry

```csharp
// Retry a failed Result directly
Func<CancellationToken, Task<Result<User>>> operation = ct => _db.TryGetUser(id, ct); // returns Result<User>
Result<User> result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromSeconds(1));
```

By default every failure is retried except `Unauthorized`, `Forbidden`, `NotFound` and `Validation` errors. Pass a `shouldRetry` predicate to decide yourself:

```csharp
// Retry only errors you classify as transient
Result<User> result = await operation.RetryIfFailed(
    shouldRetry: error => error.Code == "Db.Timeout",
    maxAttempts: 3,
    delay: TimeSpan.FromSeconds(1));
```

The predicate replaces the default classification. An exception thrown by the operation is converted to the error that would be returned for it (`ErrorType.Unexpected`) and passed to the predicate too, so an exception the predicate rejects is not retried. Caller cancellation works as without a predicate: it is never retried and throws `OperationCanceledException`.

## Configuration Options

```csharp
using CSharpEssentials.Resilience;

// Use options records
var options = new ResiliencePolicyOptions
{
    Retry = new RetryOptions { MaxAttempts = 3, Delay = TimeSpan.FromSeconds(1) },
    Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(5) },
    CircuitBreaker = new CircuitBreakerOptions
    {
        MinimumThroughput = 10,
        FailureRatio = 0.5,
        BreakDuration = TimeSpan.FromSeconds(30)
    }
};

Result<User> user = await ResiliencePolicy
    .Create(options)
    .ExecuteAsync(_ => _db.GetUser(id));
```

## Error Handling

Resilience-specific errors are returned as `Error` values:

- `Error.Failure("Resilience.Timeout", "Operation timed out.")`
- `Error.Failure("Resilience.CircuitBroken", "Circuit breaker is open.")`

When retries exhaust, the last exception is returned as `Error.Exception(ex, ErrorType.Unexpected)`, with the exception type name as code.

Cancellation of the caller's token is not converted to an error. When the `CancellationToken` passed to `ExecuteAsync` or `RetryIfFailed` is cancelled and the operation throws `OperationCanceledException`, the exception is rethrown and `WithFallback` does not run the fallback. If the caller cancels after an attempt returned a retryable failed `Result`, `ExecuteAsync`, `RetryIfFailed` and `WithFallback` throw `OperationCanceledException` instead of returning that result. This is the same whether the cancel lands during an attempt or during the backoff delay, and it also applies right after the final attempt: cancellation wins over returning the last failed result. It also applies to policies without a retry strategy, such as a circuit-breaker-only or timeout-only policy. This rule covers returned failures only: if the operation throws an exception other than `OperationCanceledException` while the caller's token is cancelled, the exception is returned as an `Unexpected` error. An `OperationCanceledException` from another token, such as an `HttpClient` timeout, is treated as a normal failure: it is retried and it triggers the fallback.

A `default(ResiliencePolicy)` or `default(ResiliencePolicy<T>)` behaves like `Create()`.

## Pipeline Composition

```csharp
// Stack multiple policies
Result<Data> result = await ResiliencePolicy
    .Create()
    .WithRetry(3)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .WithCircuitBreaker(minimumThroughput: 10)
    .ExecuteAsync(_ => _api.GetData());
```

## Polly Integration

Under the hood, uses the Polly v8 `ResiliencePipeline` from the `Polly.Core` package. The package does not reference the full `Polly` package; add it yourself if you use the Polly v7 API (`Policy`, `AsyncRetryPolicy`, …).

```csharp
// Advanced: Direct Polly configuration
using Polly;
using Polly.Retry;

Result<Data> result = await ResiliencePolicy
    .Create(builder =>
    {
        builder.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential
        });
    })
    .ExecuteAsync(_ => _api.GetData());
```
