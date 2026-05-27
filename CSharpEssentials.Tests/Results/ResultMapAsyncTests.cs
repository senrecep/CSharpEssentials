using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultMapAsyncTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    #region Task<Result>.MapAsync(Func<TOut>)

    [Fact]
    public async Task MapAsync_Should_TransformResult_When_TaskResultIsSuccess()
    {
        Task<Result> task = Task.FromResult(Result.Success());

        Result<int> mapped = await task.MapAsync(() => 42);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_Should_ReturnFailure_When_TaskResultIsFailure()
    {
        Task<Result> task = Task.FromResult(Result.Failure(TestError));
        bool called = false;

        Result<int> mapped = await task.MapAsync(() =>
        {
            called = true;
            return 42;
        });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Task<Result>.MapAsync(Func<Task<TOut>>)

    [Fact]
    public async Task MapAsync_Should_TransformResult_When_TaskResultIsSuccess_AndMapIsAsync()
    {
        Task<Result> task = Task.FromResult(Result.Success());

        Result<int> mapped = await task.MapAsync(() => Task.FromResult(42));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_Should_ReturnFailure_When_TaskResultIsFailure_AndMapIsAsync()
    {
        Task<Result> task = Task.FromResult(Result.Failure(TestError));
        bool called = false;

        Result<int> mapped = await task.MapAsync(() =>
        {
            called = true;
            return Task.FromResult(42);
        });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task MapAsync_Should_PreserveErrors_When_TaskResultIsFailure_AndMapIsAsync()
    {
        Task<Result> task = Task.FromResult(Result.Failure(TestError));

        Result<int> mapped = await task.MapAsync(() => Task.FromResult(99));

        mapped.IsFailure.Should().BeTrue();
        mapped.FirstError.Code.Should().Be("TEST");
    }

    #endregion

    #region Task<Result<TValue>>.MapAsync(Func<TValue, TOut>)

    [Fact]
    public async Task MapAsync_Should_TransformValue_When_TaskResultTIsSuccess()
    {
        Task<Result<int>> task = Task.FromResult(10.ToResult());

        Result<string> mapped = await task.MapAsync(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("10");
    }

    [Fact]
    public async Task MapAsync_Should_ReturnFailure_When_TaskResultTIsFailure()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));
        bool called = false;

        Result<string> mapped = await task.MapAsync(v =>
        {
            called = true;
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task MapAsync_Should_PreserveErrors_When_TaskResultTIsFailure()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));

        Result<string> mapped = await task.MapAsync(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.IsFailure.Should().BeTrue();
        mapped.FirstError.Code.Should().Be("TEST");
    }

    #endregion

    #region Task<Result<TValue>>.MapAsync(Func<TValue, Task<TOut>>)

    [Fact]
    public async Task MapAsync_Should_TransformValue_When_TaskResultTIsSuccess_AndMapIsAsync()
    {
        Task<Result<int>> task = Task.FromResult(10.ToResult());

        Result<string> mapped = await task.MapAsync(v => Task.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("10");
    }

    [Fact]
    public async Task MapAsync_Should_ReturnFailure_When_TaskResultTIsFailure_AndMapIsAsync()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));
        bool called = false;

        Result<string> mapped = await task.MapAsync(v =>
        {
            called = true;
            return Task.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
        });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task MapAsync_Should_PreserveErrors_When_TaskResultTIsFailure_AndMapIsAsync()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));

        Result<string> mapped = await task.MapAsync(v => Task.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        mapped.IsFailure.Should().BeTrue();
        mapped.FirstError.Code.Should().Be("TEST");
    }

    #endregion

    #region CancellationToken

    [Fact]
    public async Task MapAsync_Should_RespectCancellationToken_When_TaskResultIsSuccess()
    {
        using var cts = new CancellationTokenSource();
        Task<Result> task = Task.FromResult(Result.Success());

        Result<int> mapped = await task.MapAsync(() => 42, cts.Token);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_Should_RespectCancellationToken_When_TaskResultTIsSuccess()
    {
        using var cts = new CancellationTokenSource();
        Task<Result<int>> task = Task.FromResult(10.ToResult());

        Result<string> mapped = await task.MapAsync(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture), cts.Token);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("10");
    }

    #endregion
}
