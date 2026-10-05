using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Resilience;

public sealed class ResilienceRetryPredicateTests
{
    private static readonly TimeSpan ShortDelay = TimeSpan.FromMilliseconds(1);

    [Fact]
    public async Task RetryIfFailed_Should_Retry_When_Predicate_Returns_True()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(attempts < 3
                ? Result<int>.Failure(Error.Failure("Transient"))
                : Result<int>.Success(42));
        };

        Result<int> result = await operation.RetryIfFailed(
            shouldRetry: error => error.Code == "Transient",
            maxAttempts: 3,
            delay: ShortDelay);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Not_Retry_When_Predicate_Returns_False()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.Failure("Permanent")));
        };

        Result<int> result = await operation.RetryIfFailed(
            shouldRetry: error => error.Code == "Transient",
            maxAttempts: 3,
            delay: ShortDelay);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Permanent");
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task RetryIfFailed_Predicate_Should_Override_Default_Classification()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.NotFound()));
        };

        Result<int> result = await operation.RetryIfFailed(
            shouldRetry: _ => true,
            maxAttempts: 2,
            delay: ShortDelay);

        result.IsFailure.Should().BeTrue();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Retry_Only_When_Predicate_Returns_True()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(attempts == 1
                ? Result.Failure(Error.Failure("Transient"))
                : Result.Failure(Error.Failure("Permanent")));
        };

        Result result = await operation.RetryIfFailed(
            shouldRetry: error => error.Code == "Transient",
            maxAttempts: 3,
            delay: ShortDelay);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Permanent");
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Pass_Exceptions_Through_Predicate()
    {
        int attempts = 0;
        List<Error> seen = [];
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            throw new InvalidOperationException("boom");
        };

        Result<int> result = await operation.RetryIfFailed(
            shouldRetry: error =>
            {
                seen.Add(error);
                return false;
            },
            maxAttempts: 3,
            delay: ShortDelay);

        attempts.Should().Be(1);
        seen.Should().ContainSingle();
        seen[0].Type.Should().Be(ErrorType.Unexpected);
        seen[0].Code.Should().Be(nameof(InvalidOperationException));
        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Retry_Exceptions_Accepted_By_Predicate()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = _ =>
        {
            attempts++;
            if (attempts < 2)
                throw new TimeoutException("slow");
            return Task.FromResult(Result.Success());
        };

        Result result = await operation.RetryIfFailed(
            shouldRetry: error => error.Code == nameof(TimeoutException),
            maxAttempts: 3,
            delay: ShortDelay);

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task RetryIfFailed_With_Predicate_Should_Propagate_PreCancelled_Token()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        bool predicateCalled = false;
        Func<CancellationToken, Task<Result<int>>> operation = ct =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(Result<int>.Success(42));
        };

        Func<Task> act = async () => await operation.RetryIfFailed(
            shouldRetry: _ =>
            {
                predicateCalled = true;
                return true;
            },
            maxAttempts: 3,
            delay: ShortDelay,
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        predicateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task RetryIfFailed_With_Predicate_Should_Throw_When_Caller_Cancels_After_Retryable_Failure()
    {
        using CancellationTokenSource cts = new();
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = async _ =>
        {
            attempts++;
            await cts.CancelAsync();
            return Result.Failure(Error.Failure("Transient"));
        };

        Func<Task> act = async () => await operation.RetryIfFailed(
            shouldRetry: error => error.Code == "Transient",
            maxAttempts: 3,
            delay: ShortDelay,
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task RetryIfFailed_With_Predicate_Should_Return_Rejected_Failure_When_Caller_Cancels()
    {
        using CancellationTokenSource cts = new();
        Func<CancellationToken, Task<Result<int>>> operation = async _ =>
        {
            await cts.CancelAsync();
            return Result<int>.Failure(Error.Failure("Permanent"));
        };

        Result<int> result = await operation.RetryIfFailed(
            shouldRetry: error => error.Code == "Transient",
            maxAttempts: 3,
            delay: ShortDelay,
            cancellationToken: cts.Token);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Permanent");
    }

    [Fact]
    public async Task RetryIfFailed_Should_Throw_When_Predicate_Is_Null()
    {
        Func<CancellationToken, Task<Result<int>>> operation = _ => Task.FromResult(Result<int>.Success(1));

        Func<Task> act = async () => await operation.RetryIfFailed(shouldRetry: null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
