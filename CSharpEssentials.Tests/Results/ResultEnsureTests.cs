using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultEnsureTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result<T>.Ensure

    [Fact]
    public void ResultT_Ensure_WithSuccess_PredicateTrue_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(10);

        Result<int> ensured = result.Ensure(v => v > 5, TestError);

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public void ResultT_Ensure_WithSuccess_PredicateFalse_ShouldReturnFailure()
    {
        var result = Result<int>.Success(3);

        Result<int> ensured = result.Ensure(v => v > 5, TestError);

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_Ensure_WithFailure_ShouldReturnOriginal()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> ensured = result.Ensure(v => v > 5, Error.Validation("Other", "Other"));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_Ensure_ErrorFactory_WithSuccess_PredicateTrue_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(10);

        Result<int> ensured = result.Ensure(
            v => v > 5,
            v => Error.Validation("Factory", v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        ensured.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ResultT_Ensure_ErrorFactory_WithSuccess_PredicateFalse_ShouldReturnFailure()
    {
        var result = Result<int>.Success(3);

        Result<int> ensured = result.Ensure(
            v => v > 5,
            v => Error.Validation("Factory", v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Code.Should().Be("Factory");
    }

    #endregion

    #region Result<T>.EnsureNotNull

    [Fact]
    public void ResultT_EnsureNotNull_WithSuccess_NotNull_ShouldReturnOriginal()
    {
        var result = Result<string>.Success("hello");

        Result<string> ensured = result.EnsureNotNull(TestError);

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be("hello");
    }

    [Fact]
    public void ResultT_EnsureNotNull_WithSuccess_Null_ShouldReturnFailure()
    {
        var result = default(Result<string>);

        Result<string> ensured = result.EnsureNotNull(TestError);

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_EnsureNotNull_WithFailure_ShouldReturnOriginal()
    {
        var result = Result<string>.Failure(TestError);

        Result<string> ensured = result.EnsureNotNull(Error.Validation("Other", "Other"));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_EnsureNotNull_ErrorFactory_WithSuccess_NotNull_ShouldReturnOriginal()
    {
        var result = Result<string>.Success("hello");

        Result<string> ensured = result.EnsureNotNull(v => Error.Validation("Factory", v ?? "null"));

        ensured.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ResultT_EnsureNotNull_ErrorFactory_WithSuccess_Null_ShouldReturnFailure()
    {
        var result = default(Result<string>);

        Result<string> ensured = result.EnsureNotNull(v => Error.Validation("Factory", v ?? "null"));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Code.Should().Be("Factory");
    }

    #endregion

    #region Result<T>.EnsureAsync

    [Fact]
    public async Task ResultT_EnsureAsync_WithSuccess_PredicateTrue_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(10);

        Result<int> ensured = await result.EnsureAsync(v => Task.FromResult(v > 5), TestError);

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public async Task ResultT_EnsureAsync_WithSuccess_PredicateFalse_ShouldReturnFailure()
    {
        var result = Result<int>.Success(3);

        Result<int> ensured = await result.EnsureAsync(v => Task.FromResult(v > 5), TestError);

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_EnsureAsync_WithFailure_ShouldReturnOriginal()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> ensured = await result.EnsureAsync(
            v => Task.FromResult(v > 5),
            Error.Validation("Other", "Other"));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_EnsureAsync_ErrorFactory_WithSuccess_PredicateTrue_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(10);

        Result<int> ensured = await result.EnsureAsync(
            v => Task.FromResult(v > 5),
            v => Error.Validation("Factory", v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        ensured.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_EnsureAsync_ErrorFactory_WithSuccess_PredicateFalse_ShouldReturnFailure()
    {
        var result = Result<int>.Success(3);

        Result<int> ensured = await result.EnsureAsync(
            v => Task.FromResult(v > 5),
            v => Error.Validation("Factory", v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Code.Should().Be("Factory");
    }

    #endregion

    #region EnsureAsync Extensions

    [Fact]
    public async Task ResultT_EnsureAsync_Task_WithSuccess_PredicateTrue_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(10));
#pragma warning restore IDE0008

        Result<int> ensured = await task.EnsureAsync(v => Task.FromResult(v > 5), TestError);

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public async Task ResultT_EnsureAsync_Task_WithSuccess_PredicateFalse_ShouldReturnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(3));
#pragma warning restore IDE0008

        Result<int> ensured = await task.EnsureAsync(v => Task.FromResult(v > 5), TestError);

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_EnsureAsync_ValueTask_WithSuccess_PredicateTrue_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Success(10));
#pragma warning restore IDE0008

        Result<int> ensured = await valueTask.EnsureAsync(v => Task.FromResult(v > 5), TestError);

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public async Task ResultT_EnsureAsync_ValueTask_WithSuccess_PredicateFalse_ShouldReturnFailure()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Success(3));
#pragma warning restore IDE0008

        Result<int> ensured = await valueTask.EnsureAsync(v => Task.FromResult(v > 5), TestError);

        ensured.IsFailure.Should().BeTrue();
        ensured.FirstError.Should().Be(TestError);
    }

    #endregion
}
