# CSharpEssentials.Resilience

Retry, timeout, circuit breaker and fallback for code that returns `Result` or `Result<T>`. It wraps Polly v8 (`Polly.Core`) and gives failures back as `Result` values instead of exceptions.

Benchmarks against Polly.Core (policy overhead, composed policies, `RetryIfFailed`, `Result` boxing) are in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/BENCHMARKS.md).

## Install

```bash
dotnet add package CSharpEssentials.Resilience
```

The package depends on `Polly.Core` `[8.0.0,9.0.0)`, not the full `Polly` package. Add `Polly` yourself if you use the v7 API (`Policy`, `AsyncRetryPolicy`).

```csharp
using CSharpEssentials.Resilience;    // ResiliencePolicy, ResiliencePolicy<T>, options records, extensions
using CSharpEssentials.ResultPattern; // Result, Result<T>
using CSharpEssentials.Errors;        // Error, ErrorType
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Resilience
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

// A policy is an immutable value. Build it once and reuse it.
ResiliencePolicy<string> policy = ResiliencePolicy<string>.Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10))
    .WithTimeout(TimeSpan.FromSeconds(5));

int calls = 0;
Result<string> stock = await policy.ExecuteAsync(async ct =>
{
    calls++;
    await Task.Delay(1, ct);
    return calls < 3
        ? Result<string>.Failure(Error.Failure("Stock.Busy", "The stock service is busy."))
        : Result<string>.Success("42 in stock");
});

Console.WriteLine($"{stock.Value} after {calls} calls"); // 42 in stock after 3 calls

// NotFound is a permanent failure, so it is not retried.
calls = 0;
Result<string> missing = await policy.ExecuteAsync(ct =>
{
    calls++;
    return Task.FromResult(Result<string>.Failure(Error.NotFound("Product.NotFound", "No such product.")));
});

Console.WriteLine($"{missing.FirstError.Code} after {calls} call"); // Product.NotFound after 1 call
```

## Building a policy

`ResiliencePolicy` and `ResiliencePolicy<T>` are readonly structs. Every `With...` call returns a new policy and leaves the old one unchanged. `default(ResiliencePolicy)` behaves like `Create()`.

| Method | Parameters and defaults | Notes |
|---|---|---|
| `WithRetry` | `maxAttempts` 3, `delay` 1 s, `exponentialBackoff` true, `useJitter` false, `maxDelay` none | `maxAttempts` counts retries after the first call, so the operation runs at most `maxAttempts + 1` times. Exponential backoff doubles the delay each retry. `useJitter` randomizes each delay so callers that failed together do not retry together. `maxDelay` caps every delay. |
| `WithTimeout` | `timeout` | Cancels the token passed to the operation when the time is up. |
| `WithCircuitBreaker` | `minimumThroughput` 10, `samplingDuration` 1 min, `breakDuration` 30 s, `failureRatio` 0.5 | Opens when at least `minimumThroughput` calls ran in the sampling window and the failure ratio is reached. While open, calls fail at once. |
| `WithFallback` | `Func<CancellationToken, Task<T>>` or `Func<CancellationToken, Task<Result<T>>>` | `ResiliencePolicy<T>` only. Runs when the rest of the policy still ends in a failure. |

Polly checks the values when the policy is built and throws `ValidationException` for values out of range. With Polly.Core 8.0.0 the limits are: `maxAttempts` at least 1, `timeout` 1 second to 1 day, `minimumThroughput` at least 2, `samplingDuration` and `breakDuration` 0.5 seconds to 1 day.

Strategies nest in the order you add them, the first one outermost. `WithFallback` always wraps everything added before it.

```csharp Program.cs
// dotnet add package CSharpEssentials.Resilience
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

// Retry first, timeout second: each attempt gets its own 1 second.
int attempts = 0;
ResiliencePolicy<int> perAttempt = ResiliencePolicy<int>.Create()
    .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10))
    .WithTimeout(TimeSpan.FromSeconds(1));

Result<int> fast = await perAttempt.ExecuteAsync(async ct =>
{
    attempts++;
    if (attempts == 1)
        await Task.Delay(TimeSpan.FromSeconds(10), ct); // the first attempt hangs and times out
    return 7;
});
Console.WriteLine($"per attempt: {fast.Value} after {attempts} attempts"); // 7 after 2 attempts

// Timeout first, retry second: 1 second for all attempts together.
ResiliencePolicy<int> total = ResiliencePolicy<int>.Create()
    .WithTimeout(TimeSpan.FromSeconds(1))
    .WithRetry(maxAttempts: 10, delay: TimeSpan.FromMilliseconds(300), exponentialBackoff: false);

Result<int> slow = await total.ExecuteAsync(ct =>
    Task.FromResult(Result<int>.Failure(Error.Conflict("Busy", "Try again."))));
Console.WriteLine($"total: {slow.FirstError.Code}"); // Resilience.Timeout

// Circuit breaker: two failures in the window open the circuit.
ResiliencePolicy<string> breaker = ResiliencePolicy<string>.Create()
    .WithCircuitBreaker(minimumThroughput: 2, samplingDuration: TimeSpan.FromSeconds(10), breakDuration: TimeSpan.FromSeconds(30));

for (int i = 1; i <= 3; i++)
{
    Result<string> price = await breaker.ExecuteAsync(ct =>
        Task.FromResult(Result<string>.Failure(Error.Unexpected("Price.Down", "The price service is down."))));
    Console.WriteLine($"call {i}: {price.FirstError.Code}"); // Price.Down, Price.Down, Resilience.CircuitBroken
}

// Fallback: a value to return when everything else failed.
ResiliencePolicy<string> withFallback = breaker.WithFallback(ct => Task.FromResult("cached price"));
Result<string> fallback = await withFallback.ExecuteAsync(ct => Task.FromResult("live price"));
Console.WriteLine($"fallback: {fallback.Value}"); // cached price, because the circuit is still open
```

A circuit breaker keeps its state inside the policy value. Copies of one policy share the circuit; two policies built separately do not.

### Options records

`RetryOptions`, `TimeoutOptions` and `CircuitBreakerOptions` hold the same settings with the same defaults (`TimeoutOptions.Timeout` is 30 s). Pass them to `WithRetry`, `WithTimeout` and `WithCircuitBreaker` on either policy, or put them in a `ResiliencePolicyOptions` for `ResiliencePolicy.Create(options)`. That overload exists only on the non-generic policy and adds retry, then circuit breaker, then timeout.

```csharp Program.cs
// dotnet add package CSharpEssentials.Resilience
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

var options = new ResiliencePolicyOptions
{
    Retry = new RetryOptions { MaxAttempts = 2, Delay = TimeSpan.FromMilliseconds(10), UseJitter = true, MaxDelay = TimeSpan.FromSeconds(1) },
    Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(5) },
    CircuitBreaker = new CircuitBreakerOptions { MinimumThroughput = 10, FailureRatio = 0.5 },
};

ResiliencePolicy policy = ResiliencePolicy.Create(options);

int calls = 0;
Result<int> result = await policy.ExecuteAsync(ct =>
{
    calls++;
    return Task.FromResult(calls == 1 ? Result<int>.Failure(Error.Unexpected("Flaky", "Failed once.")) : Result<int>.Success(5));
});
Console.WriteLine($"{result.Value} after {calls} calls"); // 5 after 2 calls

// The typed policy takes the same records one by one.
ResiliencePolicy<int> typed = ResiliencePolicy<int>.Create()
    .WithRetry(options.Retry)
    .WithTimeout(options.Timeout);
Console.WriteLine((await typed.ExecuteAsync(ct => Task.FromResult(1))).IsSuccess); // True
```

## What counts as a failure

Retry and circuit breaker treat two things as failures: an exception, and a returned `Result` that failed with a transient error type.

- Not retried: `Unauthorized`, `Forbidden`, `NotFound`, `Validation` and `Domain`. Running the operation again gives the same answer.
- Retried: every other type, such as `Failure`, `Unexpected` and `Conflict`.

When the retries run out, the last failed `Result` is returned as it is. The non-generic `ResiliencePolicy` inspects any return value that implements `IResultBase`. Any other return value is a success unless the delegate throws.

## Running an operation

Both policies have `ExecuteAsync` overloads. Each takes a delegate that receives a `CancellationToken`, plus an optional caller token.

| Policy | Delegate | Returns |
|---|---|---|
| `ResiliencePolicy` | `Func<CancellationToken, Task>` | `Result` |
| `ResiliencePolicy` | `Func<CancellationToken, Task<Result>>` | `Result` |
| `ResiliencePolicy` | `Func<CancellationToken, Task<T>>` | `Result<T>` |
| `ResiliencePolicy` | `Func<CancellationToken, Task<Result<T>>>` | `Result<T>` |
| `ResiliencePolicy<T>` | `Func<CancellationToken, Task<T>>` | `Result<T>` |
| `ResiliencePolicy<T>` | `Func<CancellationToken, Task<Result<T>>>` | `Result<T>` |

Pass the token on to the code you call. Without it, the timeout and the caller's cancellation cannot stop the work. Prefer `ResiliencePolicy<T>` for code that returns `Result<T>`: the non-generic policy boxes the result on every attempt.

## Delegate extensions

`ResilienceFuncExtensions.ExecuteAsync` runs a delegate through an empty policy, so an exception becomes a failed `Result`. It exists for `Func<Task>`, `Func<Task<T>>` and `Func<Task<Result<T>>>`, each with or without a `CancellationToken` parameter.

`ResilienceResultExtensions.RetryIfFailed` retries a `Func<CancellationToken, Task<Result<T>>>` or `Func<CancellationToken, Task<Result>>` with the same classification and the same parameters as `WithRetry`. It returns a `ValueTask`. Pass `shouldRetry` to decide yourself which errors are transient. An exception from the operation reaches the predicate as the error it would become (`ErrorType.Unexpected`, the exception type name as code).

```csharp Program.cs
// dotnet add package CSharpEssentials.Resilience
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

// An exception becomes a failed Result.
Func<Task<int>> broken = () => throw new InvalidOperationException("Disk full.");
Result<int> wrapped = await broken.ExecuteAsync();
Console.WriteLine($"{wrapped.FirstError.Type}: {wrapped.FirstError.Code}"); // Unexpected: InvalidOperationException

// RetryIfFailed with the default classification.
int calls = 0;
Func<CancellationToken, Task<Result<string>>> load = ct =>
{
    calls++;
    return Task.FromResult(calls < 2
        ? Result<string>.Failure(Error.Conflict("Order.Locked", "Locked by another request."))
        : Result<string>.Success("order 17"));
};
Result<string> order = await load.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));
Console.WriteLine($"{order.Value} after {calls} calls"); // order 17 after 2 calls

// RetryIfFailed with your own predicate: retry only "Db.Timeout".
calls = 0;
Func<CancellationToken, Task<Result>> save = ct =>
{
    calls++;
    return Task.FromResult(Result.Failure(Error.Failure("Db.Deadlock", "Deadlock.")));
};
Result saved = await save.RetryIfFailed(
    shouldRetry: error => error.Code == "Db.Timeout",
    maxAttempts: 3,
    delay: TimeSpan.FromMilliseconds(10));
Console.WriteLine($"{saved.FirstError.Code} after {calls} call"); // Db.Deadlock after 1 call
```

`RetryIfFailed` caches its Polly pipeline by `(maxAttempts, delay, exponentialBackoff, useJitter, maxDelay)` and by whether a predicate is used, up to 64 entries per result type. The predicate travels in a pooled `ResilienceContext`, so a call does not allocate a pipeline. The predicate can run more than once for the same attempt: keep it pure and cheap. If it throws, retrying stops and the exception comes back as an `Unexpected` error.

There is no `ExecuteAsync` extension for `Func<Task<Result>>` or `Func<CancellationToken, Task<Result>>`. Such a delegate binds to the matching `Task<T>` overload and gives a `Result<Result>` that reports success even when the inner `Result` failed. Use `ResiliencePolicy.Create().ExecuteAsync(...)` or `RetryIfFailed` for it.

## Errors and cancellation

Failures from the policy itself come back as errors:

| Code | Type | When |
|---|---|---|
| `Resilience.Timeout` | `Failure` | `WithTimeout` ran out |
| `Resilience.CircuitBroken` | `Failure` | The circuit is open |
| exception type name | `Unexpected` | The operation or the fallback threw, and no retry is left |

Cancellation by the caller is never turned into an error:

- When the caller's token is cancelled and the operation throws `OperationCanceledException`, `ExecuteAsync`, `RetryIfFailed` and `WithFallback` rethrow it. The fallback does not run.
- When the caller cancels after an attempt returned a retryable failed `Result`, they throw `OperationCanceledException` instead of returning that result. This holds during an attempt, during the backoff delay and after the last attempt, also for policies with no retry strategy.
- An exception other than `OperationCanceledException` thrown while the caller's token is cancelled is returned as an `Unexpected` error.
- An `OperationCanceledException` from some other token, such as an `HttpClient` timeout, is an ordinary failure. It is retried and it triggers the fallback.

```csharp Program.cs
// dotnet add package CSharpEssentials.Resilience
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
try
{
    await policy.ExecuteAsync(async ct =>
    {
        await Task.Delay(TimeSpan.FromSeconds(10), ct);
        return 1;
    }, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("cancelled by the caller: thrown, not returned");
}
```

## Using Polly directly

`Create(Action<ResiliencePipelineBuilder>)` hands you the Polly builder. `FromPipeline(pipeline)` wraps a pipeline you already have, and `ToPipeline()` returns the pipeline behind a policy. `ResiliencePolicy<T>` uses `ResiliencePipelineBuilder<Result<T>>` and `ResiliencePipeline<Result<T>>`.

A strategy you add this way uses Polly's own `ShouldHandle`, which only looks at exceptions. Set `ShouldHandle` yourself if returned failures should count.

```csharp Program.cs
// dotnet add package CSharpEssentials.Resilience
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using Polly;
using Polly.Retry;

ResiliencePolicy<string> policy = ResiliencePolicy<string>.Create(builder => builder.AddRetry(new RetryStrategyOptions<Result<string>>
{
    MaxRetryAttempts = 2,
    Delay = TimeSpan.FromMilliseconds(10),
    ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result.IsFailure),
}));

int calls = 0;
Result<string> result = await policy.ExecuteAsync(ct =>
{
    calls++;
    return Task.FromResult(calls < 2 ? Result<string>.Failure(Error.NotFound("X", "Missing.")) : Result<string>.Success("found"));
});
Console.WriteLine($"{result.Value} after {calls} calls"); // found after 2 calls: this ShouldHandle retries NotFound too

// Hand the pipeline to code that expects Polly, or wrap an existing one.
ResiliencePipeline<Result<string>> pipeline = policy.ToPipeline();
ResiliencePolicy<string> again = ResiliencePolicy<string>.FromPipeline(pipeline);
Console.WriteLine((await again.ExecuteAsync(ct => Task.FromResult("ok"))).Value); // ok
```

## Notes

- Build policies once, for example in a static field or a singleton. In the benchmarks a retry policy built per call costs 12 to 15 times a prebuilt one.
- The default retry waits 1 s, 2 s, 4 s. Lower `delay` in tests.
- The package is not marked Native AOT compatible. A Native AOT publish gets ILC warning IL3054 from Polly's `DelegatingComponent.ExecuteCore` (generic recursion; ILC says an exception is thrown at run time if that path is reached), and Polly's `AddRetry<TResult>` asks for `DynamicallyAccessedMemberTypes.All` on the result type, which the retry extensions cannot meet for `Result<T>` without trim warnings.

## Upgrade notes

- 9.0: `WithRetry`, `RetryOptions` and every `RetryIfFailed` overload gained `useJitter` and `maxDelay`. `ErrorType.Domain` is new and is not retried. Assemblies compiled against 8.x must be recompiled. See [v8 to v9](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v8-to-v9.md).
- 8.0: an exception thrown by the `WithFallback` delegate is returned as `ErrorType.Unexpected` (was `Failure`). A `TimeoutRejectedException` or `BrokenCircuitException` from the fallback gets `Resilience.Timeout` or `Resilience.CircuitBroken`.

## See also

- [Agent skill](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-resilience/SKILL.md)
- [Example project](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.Resilience)
- [CSharpEssentials.Http](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD) builds HTTP retry helpers on this package
- [CSharpEssentials.Results](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD) for `Result` and `Result<T>`
