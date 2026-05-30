using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpClientResilienceExtensionsTests
{
    [Fact]
    public async Task CreateRetryPolicy_Should_Return_Success()
    {
        ResiliencePolicy policy = HttpClientResilienceExtensions.CreateRetryPolicy(maxRetryAttempts: 1);

        Result result = await policy.ExecuteAsync(_ => Task.FromResult(Result.Success()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRetryPolicy_Generic_Should_Return_Value()
    {
        ResiliencePolicy<int> policy = HttpClientResilienceExtensions.CreateRetryPolicy<int>(maxRetryAttempts: 1);

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(Result<int>.Success(42)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task CreateRetryPolicy_Should_Retry_On_Exception()
    {
        int attempts = 0;
        ResiliencePolicy policy = HttpClientResilienceExtensions.CreateRetryPolicy(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        Result result = await policy.ExecuteAsync(_ =>
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
    public async Task CreateRetryPolicy_Should_Return_Failure_On_Persistent_Exception()
    {
        ResiliencePolicy policy = HttpClientResilienceExtensions.CreateRetryPolicy(maxRetryAttempts: 1, delay: TimeSpan.FromMilliseconds(10));

        Result result = await policy.ExecuteAsync(_ => throw new HttpRequestException("Persistent failure"));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public async Task CreateRetryPolicy_Generic_Should_Retry_On_Failure()
    {
        ResiliencePolicy<int> policy = HttpClientResilienceExtensions.CreateRetryPolicy<int>(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await policy.ExecuteAsync(_ =>
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
    public async Task CreateRetryPolicy_Generic_Should_Not_Retry_On_Success()
    {
        ResiliencePolicy<int> policy = HttpClientResilienceExtensions.CreateRetryPolicy<int>(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Success(42));
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreateResiliencePolicy_Should_Apply_Default_Timeout()
    {
        ResiliencePolicy policy = HttpClientResilienceExtensions.CreateResiliencePolicy(
            maxRetryAttempts: 1,
            timeout: TimeSpan.FromSeconds(1));

        Result result = await policy.ExecuteAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return Result.Success();
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
    }

    [Fact]
    public async Task CreateResiliencePolicy_Generic_Should_Retry_And_Timeout()
    {
        ResiliencePolicy<int> policy = HttpClientResilienceExtensions.CreateResiliencePolicy<int>(
            maxRetryAttempts: 1,
            timeout: TimeSpan.FromSeconds(1),
            retryDelay: TimeSpan.FromMilliseconds(10));

        Result<int> result = await policy.ExecuteAsync(_ => Task.FromResult(Result<int>.Success(7)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
    }

    [Theory]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    public async Task CreateRetryPolicy_Generic_Should_Not_Retry_NonRetryable_Errors(ErrorType errorType)
    {
        ResiliencePolicy<int> policy = HttpClientResilienceExtensions.CreateRetryPolicy<int>(
            maxRetryAttempts: 2,
            delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(Result<int>.Failure(CreateError(errorType)));
        });

        result.IsFailure.Should().BeTrue();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreateRetryPolicy_Generic_Should_Retry_On_Conflict()
    {
        ResiliencePolicy<int> policy = HttpClientResilienceExtensions.CreateRetryPolicy<int>(
            maxRetryAttempts: 2,
            delay: TimeSpan.FromMilliseconds(10));

        int attempts = 0;
        Result<int> result = await policy.ExecuteAsync(_ =>
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
    public void CreateCircuitBreakerPolicy_Should_Have_Sensible_Defaults()
    {
        ResiliencePolicy pipeline = HttpClientResilienceExtensions.CreateCircuitBreakerPolicy(minimumThroughput: 5);

        pipeline.Should().NotBeNull();
    }

    // ── Legacy API tests ──

    [Fact]
    public async Task Legacy_CreateRetryPipeline_Should_Return_Success()
    {
        Polly.ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateRetryPipeline(maxRetryAttempts: 1);

        Result result = await pipeline.ExecuteAsResultAsync(_ => Task.FromResult(Result.Success()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Legacy_CreateResiliencePipeline_Should_Apply_Default_Timeout()
    {
        Polly.ResiliencePipeline pipeline = HttpClientResilienceExtensions.CreateResiliencePipeline(
            maxRetryAttempts: 1,
            timeout: TimeSpan.FromSeconds(1));

        Result result = await pipeline.ExecuteAsResultAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return Result.Success();
        });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Resilience.Timeout");
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
