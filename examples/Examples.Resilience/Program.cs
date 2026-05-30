using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Resilience Example");
Console.WriteLine("========================================\n");

// ============================================================================
// 1. BASIC POLICY — EMPTY PIPELINE
// ============================================================================
Console.WriteLine("--- 1. Basic Policy (Empty Pipeline) ---");

ResiliencePolicy policy = ResiliencePolicy.Create();

Result result = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));
Console.WriteLine($"Empty pipeline: IsSuccess={result.IsSuccess}");

Result<int> valueResult = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success(42)));
Console.WriteLine($"Empty pipeline (generic): Value={valueResult.Value}");
Console.WriteLine();

// ============================================================================
// 2. RETRY POLICY
// ============================================================================
Console.WriteLine("--- 2. Retry Policy ---");

int retryAttempts = 0;
ResiliencePolicy retryPolicy = ResiliencePolicy.Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(100));

Result retryResult = await retryPolicy.ExecuteAsync(_ =>
{
    retryAttempts++;
    if (retryAttempts < 3)
        throw new InvalidOperationException($"Attempt {retryAttempts} failed");
    return Task.FromResult(Result.Success());
});

Console.WriteLine($"Retry result: IsSuccess={retryResult.IsSuccess}, Total attempts={retryAttempts}");
Console.WriteLine();

// ============================================================================
// 3. RETRY WITH EXPONENTIAL BACKOFF
// ============================================================================
Console.WriteLine("--- 3. Retry with Exponential Backoff ---");

ResiliencePolicy exponentialPolicy = ResiliencePolicy.Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(50), exponentialBackoff: true);

ResiliencePolicy constantPolicy = ResiliencePolicy.Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(50), exponentialBackoff: false);

Console.WriteLine("Exponential backoff: delays grow exponentially (50ms, 100ms, 200ms)");
Console.WriteLine("Constant backoff: delays stay constant (50ms, 50ms, 50ms)");
Console.WriteLine();

// ============================================================================
// 4. TIMEOUT POLICY
// ============================================================================
Console.WriteLine("--- 4. Timeout Policy ---");

ResiliencePolicy timeoutPolicy = ResiliencePolicy.Create()
    .WithTimeout(TimeSpan.FromSeconds(1));

Result timeoutResult = await timeoutPolicy.ExecuteAsync(async ct =>
{
    await Task.Delay(TimeSpan.FromSeconds(5), ct);
    return Result.Success();
});

Console.WriteLine($"Timeout result: IsSuccess={timeoutResult.IsSuccess}, Error={timeoutResult.FirstError.Code}");
Console.WriteLine();

// ============================================================================
// 5. CIRCUIT BREAKER POLICY
// ============================================================================
Console.WriteLine("--- 5. Circuit Breaker Policy ---");

ResiliencePolicy cbPolicy = ResiliencePolicy.Create()
    .WithCircuitBreaker(
        minimumThroughput: 3,
        samplingDuration: TimeSpan.FromSeconds(1),
        breakDuration: TimeSpan.FromSeconds(5),
        failureRatio: 0.5);

int cbAttempts = 0;
for (int i = 0; i < 5; i++)
{
    Result cbResult = await cbPolicy.ExecuteAsync(_ =>
    {
        cbAttempts++;
        throw new InvalidOperationException("Service unavailable");
    });
    Console.WriteLine($"  Attempt {i + 1}: IsFailure={cbResult.IsFailure}, Code={cbResult.FirstError.Code}");
}

Console.WriteLine($"Circuit opened after threshold failures");
Console.WriteLine();

// ============================================================================
// 6. COMBINED POLICIES (RETRY + TIMEOUT + CIRCUIT BREAKER)
// ============================================================================
Console.WriteLine("--- 6. Combined Policies ---");

ResiliencePolicy combinedPolicy = ResiliencePolicy.Create()
    .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(50))
    .WithTimeout(TimeSpan.FromSeconds(1))
    .WithCircuitBreaker(minimumThroughput: 10, samplingDuration: TimeSpan.FromSeconds(1));

Result combinedResult = await combinedPolicy.ExecuteAsync(_ => Task.FromResult(Result.Success()));
Console.WriteLine($"Combined pipeline: IsSuccess={combinedResult.IsSuccess}");
Console.WriteLine();

// ============================================================================
// 7. GENERIC RESILIENCE POLICY<T>
// ============================================================================
Console.WriteLine("--- 7. Generic ResiliencePolicy<T> ---");

int genAttempts = 0;
ResiliencePolicy<int> genericPolicy = ResiliencePolicy<int>.Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(50));

Result<int> genResult = await genericPolicy.ExecuteAsync(_ =>
{
    genAttempts++;
    if (genAttempts < 2)
        return Task.FromResult(Result<int>.Failure(Error.Unexpected("Transient", "Temporary failure")));
    return Task.FromResult(Result<int>.Success(100));
});

Console.WriteLine($"Generic result: Value={genResult.Value}, Attempts={genAttempts}");
Console.WriteLine();

// ============================================================================
// 8. FALLBACK POLICY
// ============================================================================
Console.WriteLine("--- 8. Fallback Policy ---");

ResiliencePolicy<string> fallbackPolicy = ResiliencePolicy<string>.Create()
    .WithFallback(_ => Task.FromResult(Result<string>.Success("fallback value")));

Result<string> fallbackResult = await fallbackPolicy.ExecuteAsync(_ =>
    Task.FromResult(Result<string>.Failure(Error.Unexpected())));

Console.WriteLine($"Fallback result: Value={fallbackResult.Value}");
Console.WriteLine();

// ============================================================================
// 9. CREATE FROM OPTIONS RECORD
// ============================================================================
Console.WriteLine("--- 9. Create from ResiliencePolicyOptions ---");

ResiliencePolicyOptions options = new()
{
    Retry = new RetryOptions { MaxAttempts = 2, Delay = TimeSpan.FromMilliseconds(50), ExponentialBackoff = false },
    Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(2) },
    CircuitBreaker = new CircuitBreakerOptions
    {
        MinimumThroughput = 10,
        SamplingDuration = TimeSpan.FromSeconds(1),
        BreakDuration = TimeSpan.FromSeconds(1),
        FailureRatio = 0.5
    }
};

ResiliencePolicy optionsPolicy = ResiliencePolicy.Create(options);
Result optionsResult = await optionsPolicy.ExecuteAsync(_ => Task.FromResult(Result.Success()));
Console.WriteLine($"Options-based pipeline: IsSuccess={optionsResult.IsSuccess}");
Console.WriteLine();

// ============================================================================
// 10. FUNC EXTENSIONS — INLINE EXECUTION
// ============================================================================
Console.WriteLine("--- 10. Func Extensions (Inline Execution) ---");

Func<Task<int>> computeValue = () => Task.FromResult(42);
Result<int> funcResult = await computeValue.ExecuteAsync();
Console.WriteLine($"Func<Task<int>>: Value={funcResult.Value}");

Func<CancellationToken, Task<Result<string>>> fetchString = ct => Task.FromResult(Result<string>.Success("hello"));
Result<string> funcResult2 = await fetchString.ExecuteAsync();
Console.WriteLine($"Func<CT, Task<Result<string>>>: Value={funcResult2.Value}");
Console.WriteLine();

// ============================================================================
// 11. RETRY IF FAILED — EXTENSION ON RESULT-RETURNING FUNCTIONS
// ============================================================================
Console.WriteLine("--- 11. RetryIfFailed Extension ---");

int rifAttempts = 0;
Func<CancellationToken, Task<Result<int>>> unreliableOperation = ct =>
{
    rifAttempts++;
    if (rifAttempts < 3)
        return Task.FromResult(Result<int>.Failure(Error.Conflict("RateLimited", "Too many requests")));
    return Task.FromResult(Result<int>.Success(999));
};

Result<int> rifResult = await unreliableOperation.RetryIfFailed(
    maxAttempts: 5,
    delay: TimeSpan.FromMilliseconds(50));

Console.WriteLine($"RetryIfFailed: Value={rifResult.Value}, Attempts={rifAttempts}");
Console.WriteLine();

// ============================================================================
// 12. HANDLE DIFFERENT ERROR TYPES
// ============================================================================
Console.WriteLine("--- 12. Error Type Handling ---");

ResiliencePolicy errorPolicy = ResiliencePolicy.Create()
    .WithRetry(maxAttempts: 1, delay: TimeSpan.FromMilliseconds(10));

Result validationError = await errorPolicy.ExecuteAsync(_ =>
    Task.FromResult(Result.Failure(Error.Validation("InvalidInput", "Name is required"))));

Result notFoundError = await errorPolicy.ExecuteAsync(_ =>
    Task.FromResult(Result.Failure(Error.NotFound("User not found"))));

Console.WriteLine($"Validation error: Code={validationError.FirstError.Code}, Type={validationError.FirstError.Type}");
Console.WriteLine($"NotFound error: Code={notFoundError.FirstError.Code}, Type={notFoundError.FirstError.Type}");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
