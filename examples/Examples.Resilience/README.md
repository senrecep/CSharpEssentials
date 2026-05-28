# Examples.Resilience

Demonstrates the `CSharpEssentials.Resilience` package — modular resilience policies for transient fault handling.

## Run

```bash
dotnet run
```

## What's Covered

- **Empty pipeline** — `ResiliencePolicy.Create()`
- **Retry** — exponential and constant backoff
- **Timeout** — cancelling long-running operations
- **Circuit Breaker** — opening after failure threshold
- **Combined policies** — chaining retry + timeout + circuit breaker
- **Generic `ResiliencePolicy<T>`** — typed result pipelines
- **Fallback** — providing default values on failure
- **Options record** — building from `ResiliencePolicyOptions`
- **Func extensions** — inline `Func<Task<T>>.ExecuteAsync()`
- **RetryIfFailed** — extension on `Func<CT, Task<Result<T>>>`
- **Error type handling** — validation, not-found, conflict errors
