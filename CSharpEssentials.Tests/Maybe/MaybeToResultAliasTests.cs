using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeToResultAliasTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region ToResult

    [Fact]
    public void ToResult_WithValue_ShouldReturnSuccess()
    {
        Maybe<int> maybe = 42;

        Result<int> result = maybe.ToResult<int>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToResult_WithoutValue_ShouldReturnDefaultError()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Result<int> result = maybe.ToResult<int>();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Maybe.Result");
    }

    [Fact]
    public void ToResult_WithoutValue_WithCustomError_ShouldReturnCustomError()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Result<int> result = maybe.ToResult<int>(TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region ToUnitResult

    [Fact]
    public void ToUnitResult_WithValue_ShouldReturnSuccess()
    {
        Maybe<int> maybe = 42;

        Result result = maybe.ToUnitResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToUnitResult_WithoutValue_ShouldReturnDefaultError()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Result result = maybe.ToUnitResult();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Maybe.Result");
    }

    [Fact]
    public void ToUnitResult_WithoutValue_WithCustomError_ShouldReturnCustomError()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Result result = maybe.ToUnitResult(TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region ToResultAsync(Task<Maybe<T>>)

    [Fact]
    public async Task ToResultAsync_Task_WithValue_ShouldReturnSuccess()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));

        Result<int> result = await maybeTask.ToResultAsync<int>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ToResultAsync_Task_WithoutValue_ShouldReturnDefaultError()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Result<int> result = await maybeTask.ToResultAsync<int>();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Maybe.Result");
    }

    [Fact]
    public async Task ToResultAsync_Task_WithoutValue_WithCustomError_ShouldReturnCustomError()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Result<int> result = await maybeTask.ToResultAsync<int>(TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region ToResultAsync(ValueTask<Maybe<T>>)

    [Fact]
    public async Task ToResultAsync_ValueTask_WithValue_ShouldReturnSuccess()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));

        Result<int> result = await maybeTask.ToResultAsync<int>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ToResultAsync_ValueTask_WithoutValue_ShouldReturnDefaultError()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Result<int> result = await maybeTask.ToResultAsync<int>();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Maybe.Result");
    }

    [Fact]
    public async Task ToResultAsync_ValueTask_WithoutValue_WithCustomError_ShouldReturnCustomError()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Result<int> result = await maybeTask.ToResultAsync<int>(TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region ToUnitResultAsync(Task<Maybe<T>>)

    [Fact]
    public async Task ToUnitResultAsync_Task_WithValue_ShouldReturnSuccess()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));

        Result result = await maybeTask.ToUnitResultAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ToUnitResultAsync_Task_WithoutValue_ShouldReturnDefaultError()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Result result = await maybeTask.ToUnitResultAsync();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Maybe.Result");
    }

    [Fact]
    public async Task ToUnitResultAsync_Task_WithoutValue_WithCustomError_ShouldReturnCustomError()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Result result = await maybeTask.ToUnitResultAsync(TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region ToUnitResultAsync(ValueTask<Maybe<T>>)

    [Fact]
    public async Task ToUnitResultAsync_ValueTask_WithValue_ShouldReturnSuccess()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));

        Result result = await maybeTask.ToUnitResultAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ToUnitResultAsync_ValueTask_WithoutValue_ShouldReturnDefaultError()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Result result = await maybeTask.ToUnitResultAsync();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Maybe.Result");
    }

    [Fact]
    public async Task ToUnitResultAsync_ValueTask_WithoutValue_WithCustomError_ShouldReturnCustomError()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Result result = await maybeTask.ToUnitResultAsync(TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion
}
