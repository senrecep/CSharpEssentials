# CSharpEssentials.Resilience

HTTP-agnostic resilience patterns (Retry, Timeout, Circuit Breaker, Fallback) with `Result<T>` integration. Composable `ResiliencePolicy` builder backed by Polly v8 for transient fault handling without exceptions.

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
    .ExecuteAsync(() => _db.GetUser(id));

// Retry + Timeout
Result<Order> order = await ResiliencePolicy
    .Create()
    .WithRetry(3)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync(() => _orderService.GetOrder(id));

// Circuit Breaker + Fallback
Result<Product> product = await ResiliencePolicy
    .Create()
    .WithCircuitBreaker(samplingDuration: 10, failureRatio: 0.5)
    .WithFallback(ct => _cache.GetAsync<Product>(id, ct))
    .ExecuteAsync(() => _productService.GetProduct(id));
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
Result<User> result = await _db.GetUser(id)
    .RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromSeconds(1));
```

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
    .ExecuteAsync(() => _db.GetUser(id));
```

## Error Handling

Resilience-specific errors are returned as `Error` values:

- `Error.Failure("Resilience.Timeout", "Operation timed out.")`
- `Error.Failure("Resilience.CircuitBroken", "Circuit breaker is open.")`

When retries exhaust, the last exception is returned as `ErrorType.Unexpected`.

## Pipeline Composition

```csharp
// Stack multiple policies
Result<Data> result = await ResiliencePolicy
    .Create()
    .WithRetry(3)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .WithCircuitBreaker(samplingDuration: 10)
    .ExecuteAsync(() => _api.GetData());
```

## Polly Integration

Under the hood, uses Polly v8 `ResiliencePipeline`:

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
    .ExecuteAsync(() => _api.GetData());
```
