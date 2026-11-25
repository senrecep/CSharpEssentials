using System.Diagnostics;
using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

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
}
