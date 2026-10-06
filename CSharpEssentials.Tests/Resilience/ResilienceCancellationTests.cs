using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Resilience;

public sealed class ResilienceCancellationTests
{
    private static readonly TimeSpan LongDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CancelAfter = TimeSpan.FromMilliseconds(20);

    [Fact]
    public async Task Generic_WithRetry_Should_Throw_When_Caller_Cancels_During_Delay()
    {
        using CancellationTokenSource cts = new();
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 3, delay: LongDelay, exponentialBackoff: false);

        Func<Task> act = () => policy.ExecuteAsync(_ =>
        {
            attempts++;
            cts.CancelAfter(CancelAfter);
            return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task NonGeneric_WithRetry_Should_Throw_When_Caller_Cancels_During_Delay()
    {
        using CancellationTokenSource cts = new();
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 3, delay: LongDelay, exponentialBackoff: false);

        Func<Task> act = () => policy.ExecuteAsync(_ =>
        {
            attempts++;
            cts.CancelAfter(CancelAfter);
            return Task.FromResult(Result.Failure(Error.Unexpected()));
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task NonGeneric_WithRetry_Should_Throw_When_Caller_Cancels_During_Attempt()
    {
        using CancellationTokenSource cts = new();
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(1));

        Func<Task> act = () => policy.ExecuteAsync(async _ =>
        {
            attempts++;
            await cts.CancelAsync();
            return Result<int>.Failure(Error.Unexpected());
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task Generic_WithRetry_Should_Throw_When_Caller_Cancels_During_Final_Attempt()
    {
        using CancellationTokenSource cts = new();
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 1, delay: TimeSpan.FromMilliseconds(1));

        Func<Task> act = () => policy.ExecuteAsync(async _ =>
        {
            attempts++;
            if (attempts == 2)
                await cts.CancelAsync();
            return Result<int>.Failure(Error.Unexpected());
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task Generic_WithRetry_Should_Return_Non_Retryable_Failure_When_Caller_Cancels()
    {
        using CancellationTokenSource cts = new();
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(1));

        Result<int> result = await policy.ExecuteAsync(async _ =>
        {
            await cts.CancelAsync();
            return Result<int>.Failure(Error.NotFound());
        }, cts.Token);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Generic_ExecuteAsync_Should_Return_Success_When_Caller_Cancels_Concurrently()
    {
        using CancellationTokenSource cts = new();
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(1));

        Result<int> result = await policy.ExecuteAsync(async _ =>
        {
            await cts.CancelAsync();
            return Result<int>.Success(42);
        }, cts.Token);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task NonGeneric_ExecuteAsync_Should_Return_Success_When_Caller_Cancels_Concurrently()
    {
        using CancellationTokenSource cts = new();
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(1));

        Result result = await policy.ExecuteAsync(async _ =>
        {
            await cts.CancelAsync();
            return Result.Success();
        }, cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RetryIfFailed_Should_Return_Success_When_Caller_Cancels_Concurrently()
    {
        using CancellationTokenSource cts = new();
        Func<CancellationToken, Task<Result<int>>> operation = async _ =>
        {
            await cts.CancelAsync();
            return 42;
        };

        Result<int> result = await operation.RetryIfFailed(
            maxAttempts: 3,
            delay: TimeSpan.FromMilliseconds(1),
            cancellationToken: cts.Token);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Throw_When_Caller_Cancels_During_Delay()
    {
        using CancellationTokenSource cts = new();
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
        {
            attempts++;
            cts.CancelAfter(CancelAfter);
            return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
        };

        Func<Task> act = async () => await operation.RetryIfFailed(
            maxAttempts: 3,
            delay: LongDelay,
            exponentialBackoff: false,
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task Generic_CircuitBreaker_Only_Policy_Should_Throw_When_Caller_Cancels_After_Retryable_Failure()
    {
        using CancellationTokenSource cts = new();
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create().WithCircuitBreaker();

        Func<Task> act = () => policy.ExecuteAsync(async _ =>
        {
            await cts.CancelAsync();
            return Result<int>.Failure(Error.Unexpected());
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task NonGeneric_Timeout_Only_Policy_Should_Throw_When_Caller_Cancels_After_Retryable_Failure()
    {
        using CancellationTokenSource cts = new();
        ResiliencePolicy policy = ResiliencePolicy.Create().WithTimeout(TimeSpan.FromSeconds(30));

        Func<Task> act = () => policy.ExecuteAsync(async _ =>
        {
            await cts.CancelAsync();
            return Result.Failure(Error.Unexpected());
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Generic_WithResultFallback_Should_Throw_And_Skip_Fallback_When_Caller_Cancels()
    {
        using CancellationTokenSource cts = new();
        int attempts = 0;
        bool fallbackCalled = false;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1))
            .WithFallback(_ =>
            {
                fallbackCalled = true;
                return Task.FromResult(Result<int>.Success(99));
            });

        Func<Task> act = () => policy.ExecuteAsync(async _ =>
        {
            attempts++;
            await cts.CancelAsync();
            return Result<int>.Failure(Error.Unexpected());
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().Be(1);
        fallbackCalled.Should().BeFalse();
    }
}
