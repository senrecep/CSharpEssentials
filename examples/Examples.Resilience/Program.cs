using System.Diagnostics;
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using Polly;
using Polly.Retry;

// Small delays keep the demo fast. Real code usually keeps the 1 s default.
TimeSpan shortDelay = TimeSpan.FromMilliseconds(20);

Section("1. Empty policy: exceptions become failed results");
{
    ResiliencePolicy policy = ResiliencePolicy.Create();
    Result ok = await policy.ExecuteAsync(_ => Task.CompletedTask);
    Result<int> thrown = await policy.ExecuteAsync<int>(_ => throw new InvalidOperationException("Disk full."));
    Console.WriteLine($"ok: {ok.IsSuccess}, thrown: {thrown.FirstError.Type} {thrown.FirstError.Code}");
}

Section("2. Retry: exceptions and transient failures");
{
    int calls = 0;
    ResiliencePolicy policy = ResiliencePolicy.Create().WithRetry(maxAttempts: 3, delay: shortDelay);
    Result result = await policy.ExecuteAsync(_ =>
    {
        calls++;
        if (calls < 3)
            throw new TimeoutException($"call {calls} failed");
        return Task.CompletedTask;
    });
    Console.WriteLine($"success: {result.IsSuccess} after {calls} calls");
}

Section("3. Backoff: exponential, constant, jitter and a cap");
{
    foreach ((string name, ResiliencePolicy<int> policy) in new[]
    {
        ("exponential", ResiliencePolicy<int>.Create().WithRetry(3, TimeSpan.FromMilliseconds(50))),
        ("constant", ResiliencePolicy<int>.Create().WithRetry(3, TimeSpan.FromMilliseconds(50), exponentialBackoff: false)),
        ("capped jitter", ResiliencePolicy<int>.Create().WithRetry(3, TimeSpan.FromMilliseconds(50), useJitter: true, maxDelay: TimeSpan.FromMilliseconds(60))),
    })
    {
        var watch = Stopwatch.StartNew();
        await policy.ExecuteAsync(_ => Task.FromResult(Result<int>.Failure(Error.Unexpected("Down", "Down."))));
        Console.WriteLine($"{name}: 3 retries waited about {watch.ElapsedMilliseconds} ms");
    }
}

Section("4. Which error types are retried");
{
    foreach (Error error in new[]
    {
        Error.Validation("Bad", "Bad input."),
        Error.NotFound("Missing", "Not found."),
        Error.Domain("Rule", "Rule broken."),
        Error.Conflict("Busy", "Busy."),
        Error.Unexpected("Crash", "Crashed."),
    })
    {
        int calls = 0;
        await ResiliencePolicy<int>.Create().WithRetry(2, shortDelay)
            .ExecuteAsync(_ => { calls++; return Task.FromResult(Result<int>.Failure(error)); });
        Console.WriteLine($"{error.Type,-10} -> {calls} call(s)");
    }
}

Section("5. Timeout per attempt and for all attempts");
{
    int attempts = 0;
    Result<int> perAttempt = await ResiliencePolicy<int>.Create()
        .WithRetry(2, shortDelay)
        .WithTimeout(TimeSpan.FromSeconds(1))
        .ExecuteAsync(async ct =>
        {
            attempts++;
            if (attempts == 1)
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return 7;
        });
    Console.WriteLine($"retry then timeout: {perAttempt.Value} after {attempts} attempts");

    Result<int> total = await ResiliencePolicy<int>.Create()
        .WithTimeout(TimeSpan.FromSeconds(1))
        .WithRetry(10, TimeSpan.FromMilliseconds(300), exponentialBackoff: false)
        .ExecuteAsync(_ => Task.FromResult(Result<int>.Failure(Error.Conflict("Busy", "Busy."))));
    Console.WriteLine($"timeout then retry: {total.FirstError.Code}");
}

Section("6. Circuit breaker");
{
    ResiliencePolicy<string> breaker = ResiliencePolicy<string>.Create()
        .WithCircuitBreaker(minimumThroughput: 2, samplingDuration: TimeSpan.FromSeconds(10), breakDuration: TimeSpan.FromSeconds(30), failureRatio: 0.5);
    for (int i = 1; i <= 3; i++)
    {
        Result<string> result = await breaker.ExecuteAsync(_ =>
            Task.FromResult(Result<string>.Failure(Error.Unexpected("Price.Down", "Price service is down."))));
        Console.WriteLine($"call {i}: {result.FirstError.Code}");
    }
}

Section("7. Fallback with a value and with a Result");
{
    ResiliencePolicy<string> retry = ResiliencePolicy<string>.Create().WithRetry(1, shortDelay);
    Result<string> fromValue = await retry
        .WithFallback(_ => Task.FromResult("cached"))
        .ExecuteAsync(_ => Task.FromResult(Result<string>.Failure(Error.Unexpected("Down", "Down."))));
    Result<string> fromResult = await retry
        .WithFallback(_ => Task.FromResult(Result<string>.Failure(Error.Failure("NoCache", "Cache is empty."))))
        .ExecuteAsync(_ => Task.FromResult(Result<string>.Failure(Error.Unexpected("Down", "Down."))));
    Console.WriteLine($"value fallback: {fromValue.Value}, Result fallback: {fromResult.FirstError.Code}");
}

Section("8. Options records");
{
    var options = new ResiliencePolicyOptions
    {
        Retry = new RetryOptions { MaxAttempts = 2, Delay = shortDelay, ExponentialBackoff = false },
        Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(5) },
        CircuitBreaker = new CircuitBreakerOptions { MinimumThroughput = 10 },
    };
    int calls = 0;
    Result<int> result = await ResiliencePolicy.Create(options).ExecuteAsync(_ =>
    {
        calls++;
        return Task.FromResult(calls == 1 ? Result<int>.Failure(Error.Unexpected("Flaky", "Flaky.")) : Result<int>.Success(5));
    });
    ResiliencePolicy<int> typed = ResiliencePolicy<int>.Create()
        .WithRetry(options.Retry).WithTimeout(options.Timeout).WithCircuitBreaker(options.CircuitBreaker);
    Console.WriteLine($"Create(options): {result.Value} after {calls} calls, typed policy ok: {(await typed.ExecuteAsync(_ => Task.FromResult(1))).IsSuccess}");
}

Section("9. Delegate extensions and RetryIfFailed");
{
    Func<Task<int>> broken = () => throw new InvalidOperationException("boom");
    Result<int> wrapped = await broken.ExecuteAsync();
    Console.WriteLine($"Func<Task<int>>.ExecuteAsync: {wrapped.FirstError.Code}");

    int calls = 0;
    Func<CancellationToken, Task<Result<string>>> load = _ =>
    {
        calls++;
        return Task.FromResult(calls < 2 ? Result<string>.Failure(Error.Conflict("Locked", "Locked.")) : Result<string>.Success("order 17"));
    };
    Result<string> order = await load.RetryIfFailed(maxAttempts: 3, delay: shortDelay);
    Console.WriteLine($"RetryIfFailed: {order.Value} after {calls} calls");

    calls = 0;
    Func<CancellationToken, Task<Result>> save = _ =>
    {
        calls++;
        return Task.FromResult(Result.Failure(Error.Failure("Db.Deadlock", "Deadlock.")));
    };
    Result saved = await save.RetryIfFailed(error => error.Code == "Db.Timeout", maxAttempts: 3, delay: shortDelay);
    Console.WriteLine($"RetryIfFailed with predicate: {saved.FirstError.Code} after {calls} call");
}

Section("10. Caller cancellation throws");
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
    try
    {
        await ResiliencePolicy<int>.Create().WithRetry(3, shortDelay)
            .ExecuteAsync(async ct => { await Task.Delay(TimeSpan.FromSeconds(10), ct); return 1; }, cts.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("OperationCanceledException, not a failed Result");
    }
}

Section("11. Polly interop");
{
    ResiliencePolicy<string> custom = ResiliencePolicy<string>.Create(builder => builder.AddRetry(new RetryStrategyOptions<Result<string>>
    {
        MaxRetryAttempts = 1,
        Delay = shortDelay,
        ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result.IsFailure),
    }));
    ResiliencePipeline<Result<string>> pipeline = custom.ToPipeline();
    Result<string> viaPipeline = await ResiliencePolicy<string>.FromPipeline(pipeline).ExecuteAsync(_ => Task.FromResult("ok"));

    ResiliencePolicy untyped = ResiliencePolicy.Create(builder => builder.AddTimeout(TimeSpan.FromSeconds(2)));
    Result fromUntyped = await ResiliencePolicy.FromPipeline(untyped.ToPipeline()).ExecuteAsync(_ => Task.CompletedTask);
    Console.WriteLine($"typed pipeline: {viaPipeline.Value}, untyped pipeline: {fromUntyped.IsSuccess}");
}

Console.WriteLine();
Console.WriteLine("Done.");

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"--- {title} ---");
}
