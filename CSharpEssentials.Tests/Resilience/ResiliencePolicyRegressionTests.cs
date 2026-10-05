using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Polly;

namespace CSharpEssentials.Tests.Resilience;

public class ResiliencePolicyRegressionTests
{
    [Fact]
    public async Task Default_ExecuteAsync_Should_Return_Success()
    {
        ResiliencePolicy policy = default;

        Result result = await policy.ExecuteAsync(_ => Task.CompletedTask);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Default_WithRetry_Should_Retry_On_Exception()
    {
        int attempts = 0;
        ResiliencePolicy policy = default(ResiliencePolicy)
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("transient");
            return Task.FromResult(42);
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public void Default_ToPipeline_Should_Return_Empty_Pipeline()
    {
        ResiliencePolicy policy = default;

        ResiliencePipeline pipeline = policy.ToPipeline();

        pipeline.Should().BeSameAs(ResiliencePipeline.Empty);
    }

    [Fact]
    public async Task Generic_Default_ExecuteAsync_Should_Return_Value()
    {
        ResiliencePolicy<int> policy = default;

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(42));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Generic_Default_WithRetry_Should_Retry_On_Failure()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = default(ResiliencePolicy<int>)
            .WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(attempts < 2 ? Result<int>.Failure(Error.Unexpected()) : Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task Generic_Default_WithFallback_Should_Return_Fallback_Value()
    {
        ResiliencePolicy<int> policy = default(ResiliencePolicy<int>)
            .WithFallback(_ => Task.FromResult(99));

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(Result<int>.Failure(Error.Unexpected())));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public void Generic_Default_ToPipeline_Should_Return_NonNull_Pipeline()
    {
        ResiliencePolicy<int> policy = default;

        ResiliencePipeline<Result<int>> pipeline = policy.ToPipeline();

        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task Generic_WithFallback_Should_Propagate_Cancellation_From_Action()
    {
        using CancellationTokenSource cts = new();
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithFallback(_ => Task.FromResult(99));

        Func<Task> act = () => policy.ExecuteAsync(async ct =>
        {
            await cts.CancelAsync();
            ct.ThrowIfCancellationRequested();
            return 42;
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Generic_WithFallback_Should_Propagate_Cancellation_From_Fallback()
    {
        using CancellationTokenSource cts = new();
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithFallback(ct =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult(Result<int>.Success(99));
            });

        Func<Task> act = () => policy.ExecuteAsync(async _ =>
        {
            await cts.CancelAsync();
            return Result<int>.Failure(Error.Unexpected());
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RetryIfFailed_Should_Propagate_Cancellation()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        Func<CancellationToken, Task<Result<int>>> operation = ct =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(Result<int>.Success(42));
        };

        Func<Task> act = async () => await operation.RetryIfFailed(
            maxAttempts: 2,
            delay: TimeSpan.FromMilliseconds(1),
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RetryIfFailed_NonGeneric_Should_Propagate_Cancellation()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        Func<CancellationToken, Task<Result>> operation = ct =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(Result.Success());
        };

        Func<Task> act = async () => await operation.RetryIfFailed(
            maxAttempts: 2,
            delay: TimeSpan.FromMilliseconds(1),
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
