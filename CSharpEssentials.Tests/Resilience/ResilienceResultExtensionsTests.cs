using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Resilience;

public class ResilienceResultExtensionsTests
{
    [Fact]
    public async Task RetryIfFailed_Should_Return_Success()
    {
        Func<CancellationToken, Task<Result<int>>> operation = _ => Task.FromResult(Result<int>.Success(42));

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 3);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Retry_On_Transient_Failure()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            if (attempts < 3)
                return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
            return Task.FromResult(Result<int>.Success(42));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Return_Failure_When_All_Retries_Failed()
    {
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
            Task.FromResult(Result<int>.Failure(Error.Unexpected()));

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Return_Success()
    {
        Func<CancellationToken, Task<Result>> operation = _ => Task.FromResult(Result.Success());

        Result result = await operation.RetryIfFailed(maxAttempts: 3);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Retry_On_Transient_Failure()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = _ =>
        {
            attempts++;
            if (attempts < 3)
                return Task.FromResult(Result.Failure(Error.Unexpected()));
            return Task.FromResult(Result.Success());
        };

        Result result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Return_Failure_When_All_Retries_Failed()
    {
        Func<CancellationToken, Task<Result>> operation = _ =>
            Task.FromResult(Result.Failure(Error.Unexpected()));

        Result result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RetryIfFailed_Should_Not_Retry_On_Unauthorized()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.Unauthorized()));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Not_Retry_On_Forbidden()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.Forbidden()));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Not_Retry_On_NotFound()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.NotFound()));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Not_Retry_On_Validation()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.Validation()));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Retry_On_Conflict()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Conflict()));
            return Task.FromResult(Result<int>.Success(42));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Use_Constant_Backoff()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
            return Task.FromResult(Result<int>.Success(42));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10), exponentialBackoff: false);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Use_Constant_Backoff()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = _ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result.Failure(Error.Unexpected()));
            return Task.FromResult(Result.Success());
        };

        Result result = await operation.RetryIfFailed(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10), exponentialBackoff: false);

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Retry_On_Conflict()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = _ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result.Failure(Error.Conflict()));
            return Task.FromResult(Result.Success());
        };

        Result result = await operation.RetryIfFailed(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Retry_On_Unauthorized()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = _ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result.Failure(Error.Unauthorized()));
            return Task.FromResult(Result.Success());
        };

        Result result = await operation.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Retry_On_Failure_Type()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Failure()));
            return Task.FromResult(Result<int>.Success(42));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Retry_On_Unexpected_Type()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
            return Task.FromResult(Result<int>.Success(42));
        };

        Result<int> result = await operation.RetryIfFailed(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(2);
    }
}
