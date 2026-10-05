using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Resilience;

public class ResiliencePolicyResultRetryTests
{
    [Fact]
    public async Task WithRetry_Should_Retry_Transient_Failed_Result()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        Result result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(attempts < 3 ? Result.Failure(Error.Unexpected()) : Result.Success());
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task WithRetry_Should_Retry_Transient_Failed_Generic_Result()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(attempts < 3 ? Result<int>.Failure(Error.Conflict()) : Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task WithRetry_Should_Return_Last_Failed_Result_When_Retries_Are_Exhausted()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.Failure("Transient.Failure", "transient")));
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Transient.Failure");
        attempts.Should().Be(3);
    }

    [Theory]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    public async Task WithRetry_Should_Not_Retry_NonRetryable_Result(ErrorType errorType)
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        Result result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result.Failure(CreateError(errorType)));
        });

        result.IsFailure.Should().BeTrue();
        attempts.Should().Be(1);
    }

    [Theory]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    public async Task WithRetry_Should_Not_Retry_NonRetryable_Generic_Result(ErrorType errorType)
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(CreateError(errorType)));
        });

        result.IsFailure.Should().BeTrue();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task WithRetry_Should_Not_Retry_Successful_Result()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        Result result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result.Success());
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task WithCircuitBreaker_Should_Open_On_Transient_Failed_Results()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithCircuitBreaker(
                minimumThroughput: 3,
                samplingDuration: TimeSpan.FromSeconds(1),
                breakDuration: TimeSpan.FromSeconds(1));

        for (int i = 0; i < 3; i++)
        {
            await policy.ExecuteAsync(_ => Task.FromResult(Result.Failure(Error.Unexpected())));
        }

        Result resultAfterOpen = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));

        resultAfterOpen.IsFailure.Should().BeTrue();
        resultAfterOpen.FirstError.Code.Should().Be("Resilience.CircuitBroken");
    }

    [Fact]
    public async Task WithCircuitBreaker_Should_Not_Open_On_Validation_Failed_Results()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithCircuitBreaker(
                minimumThroughput: 3,
                samplingDuration: TimeSpan.FromSeconds(1),
                breakDuration: TimeSpan.FromSeconds(1));

        for (int i = 0; i < 3; i++)
        {
            await policy.ExecuteAsync(_ => Task.FromResult(Result.Failure(Error.Validation())));
        }

        Result resultAfter = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));

        resultAfter.IsSuccess.Should().BeTrue();
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
