using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultThenEnsureTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result<T>.ThenEnsure

    [Fact]
    public void ResultT_ThenEnsure_ResultTValidator_WithSuccess_Valid_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> ensured = result.ThenEnsure(v => Result<int>.Success(v));

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_ThenEnsure_ResultTValidator_WithSuccess_Invalid_ShouldReturnFailure()
    {
        var result = Result<int>.Success(42);

        Result<int> ensured = result.ThenEnsure(v => Result<int>.Failure(TestError));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_ThenEnsure_ResultTValidator_WithFailure_ShouldReturnOriginal()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> ensured = result.ThenEnsure(v => Result<int>.Success(99));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_ThenEnsure_ResultValidator_WithSuccess_Valid_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> ensured = result.ThenEnsure(v => Result.Success());

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_ThenEnsure_ResultValidator_WithSuccess_Invalid_ShouldReturnFailure()
    {
        var result = Result<int>.Success(42);

        Result<int> ensured = result.ThenEnsure(v => Result.Failure(TestError));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_ThenEnsure_ResultValidator_WithFailure_ShouldReturnOriginal()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> ensured = result.ThenEnsure(v => Result.Success());

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result<T>.ThenEnsureAsync

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ResultTValidator_WithSuccess_Valid_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> ensured = await result.ThenEnsureAsync(v => Task.FromResult(Result<int>.Success(v)));

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ResultTValidator_WithSuccess_Invalid_ShouldReturnFailure()
    {
        var result = Result<int>.Success(42);

        Result<int> ensured = await result.ThenEnsureAsync(v => Task.FromResult(Result<int>.Failure(TestError)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ResultTValidator_WithFailure_ShouldReturnOriginal()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> ensured = await result.ThenEnsureAsync(v => Task.FromResult(Result<int>.Success(99)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ResultValidator_WithSuccess_Valid_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> ensured = await result.ThenEnsureAsync(v => Task.FromResult(Result.Success()));

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ResultValidator_WithSuccess_Invalid_ShouldReturnFailure()
    {
        var result = Result<int>.Success(42);

        Result<int> ensured = await result.ThenEnsureAsync(v => Task.FromResult(Result.Failure(TestError)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ResultValidator_WithFailure_ShouldReturnOriginal()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> ensured = await result.ThenEnsureAsync(v => Task.FromResult(Result.Success()));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    #endregion

    #region ThenEnsureAsync Extensions

    [Fact]
    public async Task ResultT_ThenEnsureAsync_Task_ResultTValidator_WithSuccess_Valid_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> ensured = await task.ThenEnsureAsync(v => Task.FromResult(Result<int>.Success(v)));

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_Task_ResultTValidator_WithSuccess_Invalid_ShouldReturnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> ensured = await task.ThenEnsureAsync(v => Task.FromResult(Result<int>.Failure(TestError)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_Task_ResultValidator_WithSuccess_Valid_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> ensured = await task.ThenEnsureAsync(v => Task.FromResult(Result.Success()));

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_Task_ResultValidator_WithSuccess_Invalid_ShouldReturnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> ensured = await task.ThenEnsureAsync(v => Task.FromResult(Result.Failure(TestError)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ValueTask_ResultTValidator_WithSuccess_Valid_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> ensured = await valueTask.ThenEnsureAsync(v => Task.FromResult(Result<int>.Success(v)));

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ValueTask_ResultTValidator_WithSuccess_Invalid_ShouldReturnFailure()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> ensured = await valueTask.ThenEnsureAsync(v => Task.FromResult(Result<int>.Failure(TestError)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ValueTask_ResultValidator_WithSuccess_Valid_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> ensured = await valueTask.ThenEnsureAsync(v => Task.FromResult(Result.Success()));

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ThenEnsureAsync_ValueTask_ResultValidator_WithSuccess_Invalid_ShouldReturnFailure()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> ensured = await valueTask.ThenEnsureAsync(v => Task.FromResult(Result.Failure(TestError)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    #endregion
}
