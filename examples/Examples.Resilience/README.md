# Examples.Resilience

Shows the `CSharpEssentials.Resilience` package, which builds modular resilience policies for transient fault handling.

## Run

```bash
dotnet run
```

## What's covered

- Empty pipeline: `ResiliencePolicy.Create()`
- Retry: a retry policy, and exponential and constant backoff policies
- Timeout: cancelling long-running operations
- Circuit breaker: opening after a failure threshold
- Combined policies: chaining retry + timeout + circuit breaker
- Generic `ResiliencePolicy<T>`: typed result pipelines
- Fallback: providing default values on failure
- Options record: building from `ResiliencePolicyOptions`
- Func extensions: inline `Func<Task<T>>.ExecuteAsync()`
- `RetryIfFailed`: extension on `Func<CT, Task<Result<T>>>`
- Error type handling: `Validation` and `NotFound` failures are returned without retrying, while a `Conflict` failure is retried by `RetryIfFailed`
