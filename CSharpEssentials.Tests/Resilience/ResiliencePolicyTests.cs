using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

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

    private static Error CreateError(ErrorType type) => type switch
    {
        ErrorType.Unauthorized => Error.Unauthorized(),
        ErrorType.Forbidden => Error.Forbidden(),
        ErrorType.NotFound => Error.NotFound(),
        ErrorType.Validation => Error.Validation(),
        _ => Error.Failure()
    };
}
