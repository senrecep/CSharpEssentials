using System.Runtime.CompilerServices;
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
            return Error.Failure("Transient");
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
            return Error.Failure("Permanent");
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
    public async Task RetryIfFailed_Should_Return_Unexpected_Error_When_Predicate_Throws()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.Failure("Transient")));
        };

        Result<int> result = await operation.RetryIfFailed(
            shouldRetry: _ => throw new InvalidOperationException("predicate failed"),
            maxAttempts: 3,
            delay: ShortDelay);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
        result.FirstError.Code.Should().Be(nameof(InvalidOperationException));
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Throw_When_Predicate_Is_Null()
    {
        Func<CancellationToken, Task<Result<int>>> operation = _ => Task.FromResult(Result<int>.Success(1));

        Func<Task> act = async () => await operation.RetryIfFailed(shouldRetry: null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RetryIfFailed_Should_Use_Each_Calls_Own_Predicate_When_Options_Match()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(Error.Failure("Transient")));
        };

        foreach (string retryableCode in new[] { "Transient", "Other", "Transient" })
        {
            attempts = 0;
            Result<int> result = await operation.RetryIfFailed(
                shouldRetry: error => error.Code == retryableCode,
                maxAttempts: 2,
                delay: ShortDelay);

            result.IsFailure.Should().BeTrue();
            attempts.Should().Be(retryableCode == "Transient" ? 3 : 1);
        }
    }

    [Fact]
    public async Task RetryIfFailed_Default_Classification_Should_Not_Inherit_Predicate_From_Earlier_Call()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result.Failure(Error.Validation("Invalid")));
        };

        await operation.RetryIfFailed(shouldRetry: _ => true, maxAttempts: 2, delay: ShortDelay);
        attempts.Should().Be(3);

        attempts = 0;
        Result result = await operation.RetryIfFailed(maxAttempts: 2, delay: ShortDelay);

        result.IsFailure.Should().BeTrue();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Keep_Predicates_Apart_When_Calls_Run_Concurrently()
    {
        static Func<CancellationToken, Task<Result<int>>> Operation(StrongBox<int> attempts) => async token =>
        {
            Interlocked.Increment(ref attempts.Value);
            await Task.Yield();
            return Result<int>.Failure(Error.Failure("Transient"));
        };

        StrongBox<int>[] counters = Enumerable.Range(0, 16).Select(_ => new StrongBox<int>()).ToArray();
        await Task.WhenAll(counters.Select((counter, index) => Operation(counter).RetryIfFailed(
            shouldRetry: _ => index % 2 == 0,
            maxAttempts: 2,
            delay: ShortDelay).AsTask()));

        for (int index = 0; index < counters.Length; index++)
            counters[index].Value.Should().Be(index % 2 == 0 ? 3 : 1);
    }
}
