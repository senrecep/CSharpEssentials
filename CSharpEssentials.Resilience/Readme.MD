# CSharpEssentials.Resilience

HTTP-agnostic resilience patterns (Retry, Timeout, Circuit Breaker, Fallback) with `Result<T>` integration. Composable `ResiliencePolicy` builder backed by Polly v8 (`Polly.Core`) for transient fault handling without exceptions.

## Installation

```bash
dotnet add package CSharpEssentials.Resilience
```

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

// Circuit Breaker + Fallback
Result<Product> product = await ResiliencePolicy
    .Create()
    .WithCircuitBreaker(minimumThroughput: 10, failureRatio: 0.5)
    .WithFallback(ct => _cache.GetAsync<Product>(id, ct))
    .ExecuteAsync(_ => _productService.GetProduct(id));
```

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
Result<User> user = await (() => _db.GetUser(id)).ExecuteAsync();

// With CancellationToken
Result<User> user = await ((ct) => _db.GetUser(id, ct)).ExecuteAsync(cancellationToken);
```

## Result Retry

```csharp
// Retry a failed Result directly
Func<CancellationToken, Task<Result<User>>> operation = ct => _db.GetUser(id);
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

When retries exhaust, the last exception is returned as `ErrorType.Unexpected`.

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
