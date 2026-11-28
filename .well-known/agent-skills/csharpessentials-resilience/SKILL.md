---
name: csharpessentials-resilience
description: Use when adding retry, timeout, circuit breaker or fallback around code that returns Result or Result<T>. Covers ResiliencePolicy and ResiliencePolicy<T> (Create, WithRetry, WithTimeout, WithCircuitBreaker, WithFallback, ExecuteAsync, FromPipeline, ToPipeline), ResiliencePolicyOptions with RetryOptions, TimeoutOptions and CircuitBreakerOptions, RetryIfFailed with an optional shouldRetry predicate, which error types are retried, and caller cancellation that throws OperationCanceledException. Built on Polly.Core v8.
---

# CSharpEssentials.Resilience

Retry, timeout, circuit breaker and fallback for `Result` and `Result<T>` code. Failures come back as `Result` values, not exceptions.

## Install

```bash
dotnet add package CSharpEssentials.Resilience
```

Depends on `Polly.Core` `[8,9)`, not the full `Polly` package.

## Namespaces

```csharp
using CSharpEssentials.Resilience;    // ResiliencePolicy, ResiliencePolicy<T>, RetryOptions, RetryIfFailed
using CSharpEssentials.ResultPattern; // Result, Result<T>
using CSharpEssentials.Errors;        // Error, ErrorType
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Resilience
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

// Build once, reuse. Each With... returns a new immutable policy.
ResiliencePolicy<string> policy = ResiliencePolicy<string>.Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10))
    .WithTimeout(TimeSpan.FromSeconds(5))
    .WithFallback(ct => Task.FromResult("cached"));

int calls = 0;
Result<string> result = await policy.ExecuteAsync(async ct =>
{
    calls++;
    await Task.Delay(1, ct); // pass the token on
    return calls < 2
        ? Result<string>.Failure(Error.Unexpected("Api.Down", "The API is down."))
        : Result<string>.Success("live");
});
Console.WriteLine($"{result.Value} after {calls} calls"); // live after 2 calls

// Retries exhausted: the fallback value is returned.
Result<string> fallback = await policy.ExecuteAsync(ct =>
    Task.FromResult(Result<string>.Failure(Error.Unexpected("Api.Down", "The API is down."))));
Console.WriteLine(fallback.Value); // cached
```

## Creating a policy

| Call | Defaults | Limits (Polly.Core 8.0.0, else `ValidationException`) |
|---|---|---|
| `WithRetry(maxAttempts, delay, exponentialBackoff, useJitter, maxDelay)` | 3, 1 s, true, false, null | `maxAttempts` >= 1 |
| `WithTimeout(TimeSpan timeout)` | | 1 s to 1 day |
| `WithCircuitBreaker(minimumThroughput, samplingDuration, breakDuration, failureRatio)` | 10, 1 min, 30 s, 0.5 | throughput >= 2, durations 0.5 s to 1 day |
| `WithFallback(Func<CancellationToken, Task<T>>)` or `(Func<CancellationToken, Task<Result<T>>>)` | | `ResiliencePolicy<T>` only |

- `maxAttempts` counts retries after the first call: at most `maxAttempts + 1` calls.
- Order matters, first added is outermost. `WithRetry(...).WithTimeout(t)` limits each attempt. `WithTimeout(t).WithRetry(...)` limits all attempts together. `WithFallback` wraps everything added before it.
- Options records: `WithRetry(RetryOptions)`, `WithTimeout(TimeoutOptions)`, `WithCircuitBreaker(CircuitBreakerOptions)` on both policies. `ResiliencePolicy.Create(ResiliencePolicyOptions)` exists only on the non-generic policy (adds retry, circuit breaker, timeout in that order).

```csharp
ResiliencePolicy policy = ResiliencePolicy.Create(new ResiliencePolicyOptions
{
    Retry = new RetryOptions { MaxAttempts = 3, Delay = TimeSpan.FromMilliseconds(200), UseJitter = true, MaxDelay = TimeSpan.FromSeconds(5) },
    Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(10) },
    CircuitBreaker = new CircuitBreakerOptions { MinimumThroughput = 20, FailureRatio = 0.5 },
});
```

## Running code

`ExecuteAsync(delegate, cancellationToken = default)`. The delegate always takes a `CancellationToken`.

- `ResiliencePolicy`: `Func<CancellationToken, Task>` and `Task<Result>` give `Result`; `Task<T>` and `Task<Result<T>>` give `Result<T>`.
- `ResiliencePolicy<T>`: `Func<CancellationToken, Task<T>>` and `Task<Result<T>>` give `Result<T>`.
- Prefer `ResiliencePolicy<T>` for `Result<T>` code; the non-generic policy boxes each result.

## Which failures are retried

Exceptions, and failed results whose `FirstError.Type` is transient. The circuit breaker counts the same failures.

- Not retried: `Unauthorized`, `Forbidden`, `NotFound`, `Validation`, `Domain`.
- Retried: `Failure`, `Unexpected`, `Conflict` and any other type.
- After the last retry the last failed `Result` is returned unchanged.

## Retrying a delegate without a policy

```csharp Program.cs
// dotnet add package CSharpEssentials.Resilience
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

int calls = 0;
Func<CancellationToken, Task<Result<int>>> load = ct =>
{
    calls++;
    return Task.FromResult(calls < 3
        ? Result<int>.Failure(Error.Failure("Db.Timeout", "Timed out."))
        : Result<int>.Success(10));
};

// Returns ValueTask<Result<int>>.
Result<int> count = await load.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));
Console.WriteLine($"{count.Value} after {calls} calls"); // 10 after 3 calls

// shouldRetry replaces the default classification. Exceptions reach it as Unexpected errors.
calls = 0;
Result<int> onlyTimeouts = await load.RetryIfFailed(
    shouldRetry: error => error.Code == "Db.Timeout",
    maxAttempts: 1,
    delay: TimeSpan.FromMilliseconds(10));
Console.WriteLine($"{onlyTimeouts.FirstError.Code} after {calls} calls"); // Db.Timeout after 2 calls

// ExecuteAsync on a delegate turns an exception into a failed Result (no retry).
Func<Task<int>> broken = () => throw new InvalidOperationException("boom");
Result<int> wrapped = await broken.ExecuteAsync();
Console.WriteLine(wrapped.FirstError.Code); // InvalidOperationException
```

`RetryIfFailed` exists for `Func<CancellationToken, Task<Result<T>>>` and `Func<CancellationToken, Task<Result>>`, with or without `shouldRetry`, and takes the `WithRetry` parameters plus `cancellationToken` last. `ExecuteAsync` extensions exist for `Func<Task>`, `Func<Task<T>>`, `Func<Task<Result<T>>>` and the same with a `CancellationToken` parameter.

## Errors and cancellation

| Code | Type | When |
|---|---|---|
| `Resilience.Timeout` | `Failure` | `WithTimeout` ran out |
| `Resilience.CircuitBroken` | `Failure` | Circuit open |
| exception type name | `Unexpected` | Operation or fallback threw, no retry left |

- Caller's token cancelled: `ExecuteAsync`, `RetryIfFailed` and `WithFallback` throw `OperationCanceledException`. Not returned as an error, the fallback does not run.
- `OperationCanceledException` from another token (for example `HttpClient.Timeout`) is a normal, retryable failure.

## Polly interop

```csharp
using Polly;
using Polly.Retry;

ResiliencePolicy<int> custom = ResiliencePolicy<int>.Create(b => b.AddRetry(new RetryStrategyOptions<Result<int>>
{
    MaxRetryAttempts = 2,
    ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result.IsFailure), // Polly's default ignores failed Results
}));
ResiliencePipeline<Result<int>> pipeline = custom.ToPipeline();
ResiliencePolicy<int> wrapped = ResiliencePolicy<int>.FromPipeline(pipeline);
```

Non-generic: `ResiliencePolicy.Create(Action<ResiliencePipelineBuilder>)`, `FromPipeline(ResiliencePipeline)`, `ToPipeline()`.

## Rules

- `RetryIfFailed` returns `ValueTask`, `ExecuteAsync` returns `Task`. Both must be awaited.
- `WithFallback` is only on `ResiliencePolicy<T>`. `Create(ResiliencePolicyOptions)` is only on `ResiliencePolicy`.
- There is no `ExecuteAsync` extension for `Func<Task<Result>>` or `Func<CancellationToken, Task<Result>>`: they bind to the `Task<T>` forms, return `Result<Result>` and report success for a failed inner result. Use `ResiliencePolicy.Create().ExecuteAsync(ct => ...)` or `RetryIfFailed`.
- Do not build a policy per call. Keep it in a field or a singleton (a circuit breaker only works if the same policy instance is reused).
- Default retry delay is 1 s with exponential backoff (1, 2, 4 s). Pass a small `delay` in tests.
- `WithTimeout` below 1 second throws `ValidationException` with Polly.Core 8.0.0.
- Return `NotFound`, `Validation`, `Domain`, `Unauthorized` or `Forbidden` for permanent failures so they are not retried.
- Strategies added through `Create(Action<...>)` use Polly's `ShouldHandle`, which ignores failed `Result` values unless you set it.
- No analyzer diagnostics ship with this package.
