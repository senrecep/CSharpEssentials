using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Polly;
using Polly.CircuitBreaker;

namespace CSharpEssentials.Tests.Http;

public class HttpClientResilienceExtensionsTests
{
    [Fact]
    public async Task ExecuteAsResultAsync_Should_Return_Success_When_No_Failure()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateRetryPipeline(maxRetryAttempts: 1);

        Result result = await pipeline.ExecuteAsResultAsync(_ => Task.FromResult(Result.Success()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsResultAsync_Generic_Should_Return_Value()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateRetryPipeline(maxRetryAttempts: 1);

        Result<int> result = await pipeline.ExecuteAsResultAsync(_ => Task.FromResult(Result<int>.Success(42)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsResultAsync_Should_Retry_On_HttpRequestException()
    {
        int attempts = 0;
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateRetryPipeline(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        Result result = await pipeline.ExecuteAsResultAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                throw new HttpRequestException("Transient failure");
            return Task.FromResult(Result.Success());
        });

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsResultAsync_Should_Return_Failure_On_Persistent_Exception()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateRetryPipeline(maxRetryAttempts: 1, delay: TimeSpan.FromMilliseconds(10));

        Result result = await pipeline.ExecuteAsResultAsync(_ => throw new HttpRequestException("Persistent failure"));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public async Task CreateTimeoutPipeline_Should_Throw_On_Timeout()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateTimeoutPipeline(TimeSpan.FromSeconds(1));

        Func<Task> act = async () => await pipeline.ExecuteAsync(async token => await Task.Delay(TimeSpan.FromSeconds(5), token));

        await act.Should().ThrowAsync<Polly.Timeout.TimeoutRejectedException>();
    }

    [Fact]
    public async Task ExecuteAsResultAsync_With_GenericPipeline_Should_Return_Value()
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateRetryPipeline<int>(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await pipeline.ExecuteAsResultAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Unexpected()));
            return Task.FromResult(Result<int>.Success(99));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task CreateRetryPipeline_Generic_Should_Not_Retry_On_Success()
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateRetryPipeline<int>(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await pipeline.ExecuteAsResultAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreateResiliencePipeline_Generic_Should_Retry_And_Timeout()
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateResiliencePipeline<int>(
            maxRetryAttempts: 1,
            timeout: TimeSpan.FromSeconds(1),
            retryDelay: TimeSpan.FromMilliseconds(10));

        Result<int> result = await pipeline.ExecuteAsResultAsync(_ => Task.FromResult(Result<int>.Success(7)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
    }

    [Theory]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    public async Task CreateRetryPipeline_Generic_Should_Not_Retry_NonRetryable_Errors(ErrorType errorType)
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateRetryPipeline<int>(
            maxRetryAttempts: 2,
            delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await pipeline.ExecuteAsResultAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(CreateError(errorType)));
        });

        result.IsFailure.Should().BeTrue();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreateRetryPipeline_Generic_Should_Retry_On_Conflict()
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateRetryPipeline<int>(
            maxRetryAttempts: 2,
            delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await pipeline.ExecuteAsResultAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
                return Task.FromResult(Result<int>.Failure(Error.Conflict()));
            return Task.FromResult(Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(2);
    }

    [Fact]
    public void CreateCircuitBreakerPipeline_Should_Have_Sensible_Defaults()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateCircuitBreakerPipeline(minimumThroughput: 5);

        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCircuitBreakerPipeline_NonGeneric_Should_Open_On_Exceptions()
    {
        ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateCircuitBreakerPipeline(
            minimumThroughput: 3,
            samplingDuration: TimeSpan.FromSeconds(1),
            breakDuration: TimeSpan.FromSeconds(1));

        for (int i = 0; i < 3; i++)
        {
            Result result = await pipeline.ExecuteAsResultAsync(_ => throw new HttpRequestException("failure"));
            result.IsFailure.Should().BeTrue();
        }

        Func<Task> act = async () => await pipeline.ExecuteAsResultAsync(_ => Task.FromResult(Result.Success()));
        await act.Should().ThrowAsync<BrokenCircuitException>();
    }

    [Fact]
    public async Task CreateCircuitBreakerPipeline_Generic_Should_Open_On_Failures()
    {
        ResiliencePipeline<Result<int>> pipeline = HttpClientResilienceExtensions.CreateCircuitBreakerPipeline<int>(
            minimumThroughput: 3,
            samplingDuration: TimeSpan.FromSeconds(1),
            breakDuration: TimeSpan.FromSeconds(1));

        for (int i = 0; i < 3; i++)
        {
            Result<int> result = await pipeline.ExecuteAsResultAsync(_ => Task.FromResult(Result<int>.Failure(Error.Unexpected())));
            result.IsFailure.Should().BeTrue();
        }

        Func<Task> act = async () => await pipeline.ExecuteAsResultAsync(_ => Task.FromResult(Result<int>.Success(1)));
        await act.Should().ThrowAsync<BrokenCircuitException>();
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
