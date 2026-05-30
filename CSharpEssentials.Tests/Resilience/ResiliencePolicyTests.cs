using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.Tests.Resilience;

public class ResiliencePolicyTests
{
    [Fact]
    public async Task Create_Should_Return_Success_When_No_Failure()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create();

        Result result = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Create_Generic_Should_Return_Value()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create();

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(42));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task WithRetry_Should_Retry_On_Exception()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        Result result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("Transient failure");
            return Task.FromResult(Result.Success());
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task WithRetry_Should_Return_Failure_On_Persistent_Exception()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 1, delay: TimeSpan.FromMilliseconds(10));

        Result result = await policy.ExecuteAsync(_ => throw new InvalidOperationException("Persistent failure"));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public async Task WithTimeout_Should_Return_Timeout_Error()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithTimeout(TimeSpan.FromSeconds(1));

        Result result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return Result.Success();
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
    }

    [Fact]
    public async Task WithCircuitBreaker_Should_Open_On_Failures()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithCircuitBreaker(
                minimumThroughput: 3,
                samplingDuration: TimeSpan.FromSeconds(1),
                breakDuration: TimeSpan.FromSeconds(1));

        for (int i = 0; i < 3; i++)
        {
            Result result = await policy.ExecuteAsync(_ => throw new InvalidOperationException("failure"));
            result.IsFailure.Should().BeTrue();
        }

        Result resultAfterOpen = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));
        resultAfterOpen.IsFailure.Should().BeTrue();
        resultAfterOpen.FirstError.Code.Should().Be("Resilience.CircuitBroken");
    }

    [Fact]
    public async Task Generic_WithRetry_Should_Retry_On_Failure()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
            return Task.FromResult(Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task Generic_WithRetry_Should_Not_Retry_On_Success()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(1);
    }

    [Theory]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    public async Task Generic_WithRetry_Should_Not_Retry_NonRetryable_Errors(ErrorType errorType)
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(CreateError(errorType)));
        });

        result.IsFailure.Should().BeTrue();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task Generic_WithRetry_Should_Retry_On_Conflict()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Conflict()));
            return Task.FromResult(Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task Combined_Pipeline_Should_Retry_And_Timeout()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 1, delay: TimeSpan.FromMilliseconds(10))
            .WithTimeout(TimeSpan.FromSeconds(1));

        Result result = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Generic_WithFallback_Should_Return_Fallback_After_Retries_Are_Exhausted()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10))
            .WithFallback(_ => Task.FromResult(99));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
        attempts.Should().Be(3);
    }

    [Fact]
    public void Create_WithResiliencePolicyOptions_Should_Apply_Retry()
    {
        ResiliencePolicyOptions options = new()
        {
            Retry = new RetryOptions { MaxAttempts = 2, Delay = TimeSpan.FromMilliseconds(10), ExponentialBackoff = false }
        };

        ResiliencePolicy policy = ResiliencePolicy.Create(options);

        policy.ToPipeline().Should().NotBeNull();
    }

    [Fact]
    public void Create_WithResiliencePolicyOptions_Should_Apply_CircuitBreaker()
    {
        ResiliencePolicyOptions options = new()
        {
            CircuitBreaker = new CircuitBreakerOptions
            {
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(1),
                BreakDuration = TimeSpan.FromSeconds(1),
                FailureRatio = 0.5
            }
        };

        ResiliencePolicy policy = ResiliencePolicy.Create(options);

        policy.ToPipeline().Should().NotBeNull();
    }

    [Fact]
    public void Create_WithResiliencePolicyOptions_Should_Apply_Timeout()
    {
        ResiliencePolicyOptions options = new()
        {
            Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(5) }
        };

        ResiliencePolicy policy = ResiliencePolicy.Create(options);

        policy.ToPipeline().Should().NotBeNull();
    }

    [Fact]
    public void Create_WithBuilder_Should_Allow_Custom_Configuration()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create(builder =>
        {
            builder.AddTimeout(new Polly.Timeout.TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(5)
            });
        });

        policy.ToPipeline().Should().NotBeNull();
    }

    [Fact]
    public void Create_WithBuilder_Null_Should_Throw()
    {
        Action act = () => ResiliencePolicy.Create((Action<Polly.ResiliencePipelineBuilder>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToPipeline_Should_Return_NonNull_Pipeline()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create();

        var pipeline = policy.ToPipeline();

        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_NonGeneric_Result_Should_Return_Success()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create();

        Result result = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_NonGeneric_Result_Should_Return_Failure_On_Exception()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 1, delay: TimeSpan.FromMilliseconds(10));

        Result result = await policy.ExecuteAsync(_ => throw new InvalidOperationException("test"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_BrokenCircuitException_For_NonGeneric()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithCircuitBreaker(
                minimumThroughput: 2,
                samplingDuration: TimeSpan.FromSeconds(1),
                breakDuration: TimeSpan.FromSeconds(10));

        for (int i = 0; i < 2; i++)
        {
            await policy.ExecuteAsync(_ => throw new InvalidOperationException("fail"));
        }

        Result result = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.CircuitBroken");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_TimeoutRejectedException_For_NonGeneric()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithTimeout(TimeSpan.FromSeconds(1));

        Result result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return Result.Success();
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_Generic_T_Should_Handle_BrokenCircuitException()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithCircuitBreaker(
                minimumThroughput: 2,
                samplingDuration: TimeSpan.FromSeconds(1),
                breakDuration: TimeSpan.FromSeconds(10));

        for (int i = 0; i < 2; i++)
        {
            await policy.ExecuteAsync(_ => throw new InvalidOperationException("fail"));
        }

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(Result<int>.Success(42)));
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.CircuitBroken");
    }

    [Fact]
    public async Task ExecuteAsync_Generic_T_Should_Handle_TimeoutRejectedException()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithTimeout(TimeSpan.FromSeconds(1));

        Result<int> result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return 42;
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
    }

    [Fact]
    public async Task ExecuteAsync_Generic_T_Should_Handle_Unexpected_Exception()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 1, delay: TimeSpan.FromMilliseconds(10));

        Result<int> result = await policy.ExecuteAsync(_ => throw new InvalidOperationException("boom"));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_OperationCanceledException()
    {
        using CancellationTokenSource cts = new();

        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 1, delay: TimeSpan.FromMilliseconds(10));

        Result result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(100, ct);
            return Result.Success();
        }, cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Generic_Should_Handle_OperationCanceledException()
    {
        using CancellationTokenSource cts = new();

        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 1, delay: TimeSpan.FromMilliseconds(10));

        Result<int> result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(100, ct);
            return 42;
        }, cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Generic_WithRetry_Options_Should_Apply_Retry()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(new RetryOptions { MaxAttempts = 2, Delay = TimeSpan.FromMilliseconds(10), ExponentialBackoff = false });

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
            return Task.FromResult(Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public void Generic_WithRetry_Options_Null_Should_Throw()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create();

        Action act = () => policy.WithRetry((RetryOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Generic_WithTimeout_Options_Should_Apply_Timeout()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithTimeout(new TimeoutOptions { Timeout = TimeSpan.FromSeconds(1) });

        Result<int> result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return 42;
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
    }

    [Fact]
    public void Generic_WithTimeout_Options_Null_Should_Throw()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create();

        Action act = () => policy.WithTimeout((TimeoutOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Generic_WithCircuitBreaker_Options_Should_Open_On_Failures()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithCircuitBreaker(new CircuitBreakerOptions
            {
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromSeconds(1),
                BreakDuration = TimeSpan.FromSeconds(10),
                FailureRatio = 0.5
            });

        for (int i = 0; i < 2; i++)
        {
            await policy.ExecuteAsync(_ => throw new InvalidOperationException("fail"));
        }

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(Result<int>.Success(42)));
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.CircuitBroken");
    }

    [Fact]
    public void Generic_WithCircuitBreaker_Options_Null_Should_Throw()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create();

        Action act = () => policy.WithCircuitBreaker((CircuitBreakerOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NonGeneric_WithRetry_Options_Null_Should_Throw()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create();

        Action act = () => policy.WithRetry((RetryOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NonGeneric_WithTimeout_Options_Null_Should_Throw()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create();

        Action act = () => policy.WithTimeout((TimeoutOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NonGeneric_WithCircuitBreaker_Options_Null_Should_Throw()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create();

        Action act = () => policy.WithCircuitBreaker((CircuitBreakerOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Generic_WithFallback_Null_Should_Throw()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create();

        Action act = () => policy.WithFallback((Func<CancellationToken, Task<int>>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Generic_WithFallback_Result_Null_Should_Throw()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create();

        Action act = () => policy.WithFallback((Func<CancellationToken, Task<Result<int>>>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Generic_WithFallback_Result_Should_Return_Fallback_After_Failure()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithFallback(_ => Task.FromResult(Result<int>.Success(99)));

        Result<int> result = await policy.ExecuteAsync(_ =>
            Task.FromResult(Result<int>.Failure(Error.Unexpected())));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public void Generic_Create_Null_Should_Throw()
    {
        Action act = () => ResiliencePolicy<int>.Create(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Generic_ToPipeline_Should_Return_NonNull()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create();

        var pipeline = policy.ToPipeline();

        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task Generic_WithRetry_Constant_Backoff_Should_Work()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10), exponentialBackoff: false);

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
            return Task.FromResult(Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task NonGeneric_WithRetry_Constant_Backoff_Should_Work()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10), exponentialBackoff: false);

        Result result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("fail");
            return Task.FromResult(Result.Success());
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task Create_WithAllOptions_Should_Apply_All_Policies()
    {
        ResiliencePolicyOptions options = new()
        {
            Retry = new RetryOptions { MaxAttempts = 2, Delay = TimeSpan.FromMilliseconds(10) },
            CircuitBreaker = new CircuitBreakerOptions
            {
                MinimumThroughput = 10,
                SamplingDuration = TimeSpan.FromSeconds(1),
                BreakDuration = TimeSpan.FromSeconds(1),
                FailureRatio = 0.5
            },
            Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(5) }
        };

        ResiliencePolicy policy = ResiliencePolicy.Create(options);

        Result result = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));
        result.IsSuccess.Should().BeTrue();
    }

    private static Error CreateError(ErrorType type) => type switch
    {
        ErrorType.Unauthorized => Error.Unauthorized(),
        ErrorType.Forbidden => Error.Forbidden(),
        ErrorType.NotFound => Error.NotFound(),
        ErrorType.Validation => Error.Validation(),
        _ => Error.Failure()
    };
}
