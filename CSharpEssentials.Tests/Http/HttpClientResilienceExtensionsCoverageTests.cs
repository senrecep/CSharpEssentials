using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Polly;

namespace CSharpEssentials.Tests.Http;

public class HttpClientResilienceExtensionsCoverageTests
{
    [Fact]
    public async Task CreateRetryPipeline_Generic_Should_Retry_Retryable_Failure()
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateRetryPipeline<int>(
            maxRetryAttempts: 2,
            delay: TimeSpan.FromMilliseconds(1));

        int attempts = 0;
        Result<int> result = await pipeline.ExecuteAsResultAsync(_ =>
        {
            attempts++;
            return Task.FromResult(attempts < 2 ? Result<int>.Failure(Error.Unexpected()) : Result<int>.Success(5));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task CreateTimeoutPipeline_Should_Return_Timeout_Error()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateTimeoutPipeline(TimeSpan.FromSeconds(1));

        Result result = await pipeline.ExecuteAsResultAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return Result.Success();
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
    }

    [Fact]
    public async Task CreateTimeoutPolicy_Should_Return_Timeout_Error()
    {
        ResiliencePolicy policy = HttpClientResilienceExtensions.CreateTimeoutPolicy(TimeSpan.FromSeconds(1));

        Result result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return Result.Success();
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
    }

    [Fact]
    public async Task CreateCircuitBreakerPipeline_Should_Open_After_Failures()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateCircuitBreakerPipeline(
            minimumThroughput: 2,
            breakDuration: TimeSpan.FromMinutes(1));

        int calls = 0;
        Func<CancellationToken, Task<Result>> failing = _ =>
        {
            calls++;
            throw new HttpRequestException("down");
        };

        await pipeline.ExecuteAsResultAsync(failing);
        await pipeline.ExecuteAsResultAsync(failing);
        Result result = await pipeline.ExecuteAsResultAsync(failing);

        result.FirstError.Code.Should().Be("Resilience.CircuitBroken");
        calls.Should().Be(2);
    }

    [Fact]
    public async Task CreateCircuitBreakerPipeline_Generic_Should_Open_After_Failures()
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateCircuitBreakerPipeline<int>(
            minimumThroughput: 2,
            breakDuration: TimeSpan.FromMinutes(1));

        int calls = 0;
        Func<CancellationToken, Task<Result<int>>> failing = _ =>
        {
            calls++;
            return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
        };

        await pipeline.ExecuteAsResultAsync(failing);
        await pipeline.ExecuteAsResultAsync(failing);
        Result<int> result = await pipeline.ExecuteAsResultAsync(failing);

        result.FirstError.Code.Should().Be("Resilience.CircuitBroken");
        calls.Should().Be(2);
    }

    [Fact]
    public async Task CreateCircuitBreakerPolicy_Generic_Should_Open_After_Failures()
    {
        ResiliencePolicy<int> policy = HttpClientResilienceExtensions.CreateCircuitBreakerPolicy<int>(
            minimumThroughput: 2,
            breakDuration: TimeSpan.FromMinutes(1));

        int calls = 0;
        Func<CancellationToken, Task<Result<int>>> failing = _ =>
        {
            calls++;
            return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
        };

        await policy.ExecuteAsync(failing);
        await policy.ExecuteAsync(failing);
        Result<int> result = await policy.ExecuteAsync(failing);

        result.FirstError.Code.Should().Be("Resilience.CircuitBroken");
        calls.Should().Be(2);
    }

    [Fact]
    public async Task CreateResiliencePipeline_Generic_Should_Time_Out()
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateResiliencePipeline<int>(
            maxRetryAttempts: 1,
            timeout: TimeSpan.FromSeconds(1),
            retryDelay: TimeSpan.FromMilliseconds(1));

        Result<int> result = await pipeline.ExecuteAsResultAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return Result<int>.Success(1);
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
    }

    [Fact]
    public async Task ExecuteAsResultAsync_Generic_On_NonGeneric_Pipeline_Should_Retry_And_Return_Value()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateRetryPipeline(
            maxRetryAttempts: 2,
            delay: TimeSpan.FromMilliseconds(1));

        int attempts = 0;
        Result<string> result = await pipeline.ExecuteAsResultAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                throw new HttpRequestException("transient");
            return Task.FromResult(Result<string>.Success("ok"));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
        attempts.Should().Be(2);
    }
}
