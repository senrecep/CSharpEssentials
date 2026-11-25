using System.Diagnostics;
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.Tests.Resilience;

public class ResilienceRetryDelayTests
{
    private static readonly TimeSpan LongDelay = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan Cap = TimeSpan.FromMilliseconds(1);
    private static readonly TimeSpan Bound = TimeSpan.FromSeconds(30);

    private static Func<CancellationToken, Task<Result>> FailTwice(Func<int> next) =>
        _ => Task.FromResult(next() < 3 ? Result.Failure(Error.Failure()) : Result.Success());

    [Fact]
    public async Task WithRetry_WithMaxDelay_Should_Cap_The_Backoff()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create().WithRetry(maxAttempts: 2, delay: LongDelay, maxDelay: Cap);
        var watch = Stopwatch.StartNew();

        Result result = await policy.ExecuteAsync(FailTwice(() => ++attempts));

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(3);
        watch.Elapsed.Should().BeLessThan(Bound);
    }

    [Fact]
    public async Task WithRetry_Generic_WithMaxDelay_Should_Cap_The_Backoff()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create().WithRetry(maxAttempts: 2, delay: LongDelay, useJitter: true, maxDelay: Cap);
        var watch = Stopwatch.StartNew();

        Result<int> result = await policy.ExecuteAsync(_ =>
            Task.FromResult(++attempts < 3 ? Result<int>.Failure(Error.Failure()) : Result<int>.Success(7)));

        result.Value.Should().Be(7);
        attempts.Should().Be(3);
        watch.Elapsed.Should().BeLessThan(Bound);
    }

    [Fact]
    public async Task WithRetry_WithJitter_Should_Still_Retry_Failed_Results()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create().WithRetry(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1), useJitter: true);

        Result result = await policy.ExecuteAsync(FailTwice(() => ++attempts));

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task Create_WithRetryOptions_Should_Forward_UseJitter_And_MaxDelay()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create(new ResiliencePolicyOptions
        {
            Retry = new RetryOptions { MaxAttempts = 2, Delay = LongDelay, UseJitter = true, MaxDelay = Cap }
        });
        var watch = Stopwatch.StartNew();

        Result result = await policy.ExecuteAsync(FailTwice(() => ++attempts));

        result.IsSuccess.Should().BeTrue();
        watch.Elapsed.Should().BeLessThan(Bound);
    }

    [Fact]
    public async Task WithRetry_Generic_WithRetryOptions_Should_Forward_MaxDelay()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithRetry(new RetryOptions { MaxAttempts = 2, Delay = LongDelay, MaxDelay = Cap });
        var watch = Stopwatch.StartNew();

        Result<int> result = await policy.ExecuteAsync(_ =>
            Task.FromResult(++attempts < 3 ? Result<int>.Failure(Error.Failure()) : Result<int>.Success(1)));

        result.IsSuccess.Should().BeTrue();
        watch.Elapsed.Should().BeLessThan(Bound);
    }

    [Fact]
    public void RetryOptions_Should_Default_To_No_Jitter_And_No_Cap()
    {
        var options = new RetryOptions();

        options.UseJitter.Should().BeFalse();
        options.MaxDelay.Should().BeNull();
    }

    [Theory]
    [InlineData(true, 5)]
    [InlineData(false, null)]
    public void RetryStrategyFactory_Should_Set_UseJitter_And_MaxDelay(bool useJitter, int? maxDelayMs)
    {
        TimeSpan? maxDelay = maxDelayMs is { } ms ? TimeSpan.FromMilliseconds(ms) : null;

        RetryStrategyOptions<Result> options = RetryStrategyFactory.Create<RetryStrategyOptions<Result>, Result>(
            4, LongDelay, exponentialBackoff: false, useJitter, maxDelay, static _ => new ValueTask<bool>(true));

        options.MaxRetryAttempts.Should().Be(4);
        options.Delay.Should().Be(LongDelay);
        options.BackoffType.Should().Be(DelayBackoffType.Constant);
        options.UseJitter.Should().Be(useJitter);
        options.MaxDelay.Should().Be(maxDelay);
    }

    [Fact]
    public async Task RetryStrategyFactory_MaxDelay_Should_Cap_The_Delay_Polly_Uses()
    {
        var delays = new List<TimeSpan>();
        RetryStrategyOptions<Result> options = RetryStrategyFactory.Create<RetryStrategyOptions<Result>, Result>(
            2, LongDelay, exponentialBackoff: true, useJitter: true, Cap, static args => new ValueTask<bool>(args.Outcome.Result!.IsFailure));
        options.OnRetry = args =>
        {
            delays.Add(args.RetryDelay);
            return default;
        };
        ResiliencePipeline<Result> pipeline = new ResiliencePipelineBuilder<Result>().AddRetry(options).Build();
        int attempts = 0;

        Result result = await pipeline.ExecuteAsync(_ => new ValueTask<Result>(++attempts < 3 ? Result.Failure(Error.Failure()) : Result.Success()));

        result.IsSuccess.Should().BeTrue();
        delays.Should().Equal(Cap, Cap);
    }

    [Fact]
    public async Task RetryIfFailed_WithMaxDelay_Should_Cap_The_Backoff()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = FailTwice(() => ++attempts);
        var watch = Stopwatch.StartNew();

        Result result = await operation.RetryIfFailed(maxAttempts: 2, delay: LongDelay, useJitter: true, maxDelay: Cap);

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(3);
        watch.Elapsed.Should().BeLessThan(Bound);
    }

    [Fact]
    public async Task RetryIfFailed_Generic_WithPredicate_And_MaxDelay_Should_Cap_The_Backoff()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result<int>>> operation = _ =>
            Task.FromResult(++attempts < 3 ? Result<int>.Failure(Error.Failure()) : Result<int>.Success(9));

        Result<int> result = await operation.RetryIfFailed(static _ => true, maxAttempts: 2, delay: LongDelay, maxDelay: Cap);

        result.Value.Should().Be(9);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task RetryIfFailed_Should_Not_Share_A_Capped_Pipeline_With_An_Uncapped_Call()
    {
        int capped = 0;
        await FailTwice(() => ++capped).RetryIfFailed(maxAttempts: 2, delay: LongDelay, maxDelay: Cap);

        int uncapped = 0;
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        Func<Task> act = () => FailTwice(() => ++uncapped).RetryIfFailed(maxAttempts: 2, delay: LongDelay, cancellationToken: cts.Token).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
        uncapped.Should().Be(1);
    }
}
