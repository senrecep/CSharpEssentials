using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.Tests.Resilience;

public sealed class ResiliencePolicyOverloadTests
{
    [Fact]
    public async Task ExecuteAsync_Value_Should_Return_Unexpected_Error_When_Action_Throws()
    {
        Func<CancellationToken, Task<int>> action = _ => throw new InvalidOperationException("boom");

        Result<int> result = await ResiliencePolicy.Create().ExecuteAsync(action);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
        result.FirstError.Description.Should().Be("boom");
    }

    [Fact]
    public async Task ExecuteAsync_ResultOfT_Should_Return_Unexpected_Error_When_Action_Throws()
    {
        Func<CancellationToken, Task<Result<int>>> action = _ => throw new InvalidOperationException("boom");

        Result<int> result = await ResiliencePolicy.Create().ExecuteAsync(action);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
        result.FirstError.Description.Should().Be("boom");
    }

    [Fact]
    public async Task WithRetry_Options_Should_Retry_Up_To_MaxAttempts()
    {
        int attempts = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(new RetryOptions { MaxAttempts = 2, Delay = TimeSpan.FromMilliseconds(1), ExponentialBackoff = false });

        Result result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult<Result>(Error.Failure("transient", "transient"));
        });

        result.FirstError.Code.Should().Be("transient");
        attempts.Should().Be(3);
    }

    [Fact]
    public void WithRetry_Options_Should_Throw_When_Options_Null()
    {
        Action act = () => ResiliencePolicy.Create().WithRetry((RetryOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task WithTimeout_Options_Should_Return_Timeout_Error()
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithTimeout(new TimeoutOptions { Timeout = TimeSpan.FromSeconds(1) });

        Result result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            return Result.Success();
        });

        result.FirstError.Code.Should().Be("Resilience.Timeout");
    }

    [Fact]
    public void WithTimeout_Options_Should_Throw_When_Options_Null()
    {
        Action act = () => ResiliencePolicy.Create().WithTimeout((TimeoutOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task WithCircuitBreaker_Options_Should_Open_After_Failures()
    {
        int calls = 0;
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithCircuitBreaker(new CircuitBreakerOptions
            {
                MinimumThroughput = 2,
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                BreakDuration = TimeSpan.FromSeconds(30)
            });
        Func<CancellationToken, Task<Result>> failing = _ =>
        {
            calls++;
            return Task.FromResult<Result>(Error.Failure("down", "down"));
        };

        await policy.ExecuteAsync(failing);
        await policy.ExecuteAsync(failing);
        Result result = await policy.ExecuteAsync(failing);

        result.FirstError.Code.Should().Be("Resilience.CircuitBroken");
        calls.Should().Be(2);
    }

    [Fact]
    public void WithCircuitBreaker_Options_Should_Throw_When_Options_Null()
    {
        Action act = () => ResiliencePolicy.Create().WithCircuitBreaker((CircuitBreakerOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Generic_FromPipeline_Should_Use_Given_Pipeline()
    {
        int attempts = 0;
        ResiliencePipeline<Result<int>> pipeline = new ResiliencePipelineBuilder<Result<int>>()
            .AddRetry(new RetryStrategyOptions<Result<int>>
            {
                MaxRetryAttempts = 1,
                Delay = TimeSpan.Zero,
                ShouldHandle = args => new ValueTask<bool>(args.Outcome.Result.IsFailure)
            })
            .Build();
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.FromPipeline(pipeline);

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(attempts < 2 ? Result<int>.Failure(Error.Failure("x", "x")) : Result<int>.Success(5));
        });

        result.Value.Should().Be(5);
        policy.ToPipeline().Should().BeSameAs(pipeline);
    }

    [Fact]
    public async Task Generic_Create_Configure_Should_Build_Pipeline_From_Builder()
    {
        int attempts = 0;
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create(builder => builder.AddRetry(new RetryStrategyOptions<Result<int>>
        {
            MaxRetryAttempts = 2,
            Delay = TimeSpan.Zero,
            ShouldHandle = args => new ValueTask<bool>(args.Outcome.Exception is not null)
        }));

        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return attempts < 3 ? throw new InvalidOperationException("transient") : Task.FromResult(9);
        });

        result.Value.Should().Be(9);
        attempts.Should().Be(3);
    }

    [Fact]
    public void Generic_Create_Configure_Should_Throw_When_Configure_Null()
    {
        Action act = () => ResiliencePolicy<int>.Create((Action<ResiliencePipelineBuilder<Result<int>>>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Generic_WithFallback_Value_Should_Return_Failure_When_Fallback_Throws()
    {
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create()
            .WithFallback(_ => Task.FromException<int>(new InvalidOperationException("fallback broke")));

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(Result<int>.Failure(Error.Failure("primary", "primary"))));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(nameof(InvalidOperationException));
        result.FirstError.Description.Should().Be("fallback broke");
    }

    [Fact]
    public async Task Generic_WithFallback_Result_Should_Return_Failure_When_Fallback_Throws()
    {
        Func<CancellationToken, Task<Result<int>>> fallback = _ => Task.FromException<Result<int>>(new InvalidOperationException("fallback broke"));
        ResiliencePolicy<int> policy = ResiliencePolicy<int>.Create().WithFallback(fallback);

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(Result<int>.Failure(Error.Failure("primary", "primary"))));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(nameof(InvalidOperationException));
        result.FirstError.Description.Should().Be("fallback broke");
    }
}
