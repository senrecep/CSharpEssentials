using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultFailIfTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");
    private static readonly Error AnotherError = Error.Validation("Another.Code", "Another message");

    #region Result<T>.FailIf

    [Fact]
    public void ResultT_FailIf_ConditionTrue_WithSuccess_ShouldReturnFailure()
    {
        var result = Result<int>.Success(10);

        Result<int> failIfResult = result.FailIf(value => value > 5, TestError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_FailIf_ConditionFalse_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(3);

        Result<int> failIfResult = result.FailIf(value => value > 5, TestError);

        failIfResult.IsSuccess.Should().BeTrue();
        failIfResult.Value.Should().Be(3);
    }

    [Fact]
    public void ResultT_FailIf_WithFailure_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> failIfResult = result.FailIf(value => value > 5, AnotherError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_FailIf_WithErrorFunc_ConditionTrue_ShouldReturnComputedError()
    {
        var result = Result<int>.Success(10);

        Result<int> failIfResult = result.FailIf(
            value => value > 5,
            value => Error.Validation("Computed.Code", $"Value {value} is too large"));

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Description.Should().Contain("10");
    }

    [Fact]
    public void ResultT_FailIf_WithErrorFunc_ConditionFalse_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(3);

        Result<int> failIfResult = result.FailIf(
            value => value > 5,
            value => Error.Validation("Computed.Code", $"Value {value} is too large"));

        failIfResult.IsSuccess.Should().BeTrue();
        failIfResult.Value.Should().Be(3);
    }

    [Fact]
    public async Task ResultT_FailIfAsync_ConditionTrue_WithSuccess_ShouldReturnFailure()
    {
        var result = Result<int>.Success(10);

        Result<int> failIfResult = await result.FailIfAsync(
            value => Task.FromResult(value > 5),
            TestError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_FailIfAsync_ConditionFalse_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(3);

        Result<int> failIfResult = await result.FailIfAsync(
            value => Task.FromResult(value > 5),
            TestError);

        failIfResult.IsSuccess.Should().BeTrue();
        failIfResult.Value.Should().Be(3);
    }

    [Fact]
    public async Task ResultT_FailIfAsync_WithFailure_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> failIfResult = await result.FailIfAsync(
            value => Task.FromResult(value > 5),
            AnotherError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_FailIfAsync_WithErrorFunc_ConditionTrue_ShouldReturnComputedError()
    {
        var result = Result<int>.Success(10);

        Result<int> failIfResult = await result.FailIfAsync(
            value => Task.FromResult(value > 5),
            value => Task.FromResult(Error.Validation("Computed.Code", $"Value {value}")));

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Description.Should().Contain("10");
    }

    [Fact]
    public async Task ResultT_FailIfAsync_WithErrorFunc_ConditionFalse_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(3);

        Result<int> failIfResult = await result.FailIfAsync(
            value => Task.FromResult(value > 5),
            value => Task.FromResult(Error.Validation("Computed.Code", $"Value {value}")));

        failIfResult.IsSuccess.Should().BeTrue();
        failIfResult.Value.Should().Be(3);
    }

    #endregion

    #region Task<Result<T>>.FailIf

    [Fact]
    public async Task TaskResultT_FailIf_ConditionTrue_WithSuccess_ShouldReturnFailure()
    {
        var task = Task.FromResult(Result<int>.Success(10));

        Result<int> failIfResult = await task.FailIf(
            value => value > 5,
            TestError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task TaskResultT_FailIf_ConditionFalse_WithSuccess_ShouldReturnOriginal()
    {
        var task = Task.FromResult(Result<int>.Success(3));

        Result<int> failIfResult = await task.FailIf(
            value => value > 5,
            TestError);

        failIfResult.IsSuccess.Should().BeTrue();
        failIfResult.Value.Should().Be(3);
    }

    [Fact]
    public async Task TaskResultT_FailIf_WithFailure_ShouldReturnOriginalFailure()
    {
        var task = Task.FromResult(Result<int>.Failure(TestError));

        Result<int> failIfResult = await task.FailIf(
            value => value > 5,
            AnotherError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task TaskResultT_FailIf_WithErrorFunc_ConditionTrue_ShouldReturnComputedError()
    {
        var task = Task.FromResult(Result<int>.Success(10));

        Result<int> failIfResult = await task.FailIf(
            value => value > 5,
            value => Error.Validation("Computed.Code", $"Value {value}"));

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Description.Should().Contain("10");
    }

    [Fact]
    public async Task TaskResultT_FailIfAsync_ConditionTrue_WithSuccess_ShouldReturnFailure()
    {
        var task = Task.FromResult(Result<int>.Success(10));

        Result<int> failIfResult = await task.FailIfAsync(
            value => Task.FromResult(value > 5),
            TestError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task TaskResultT_FailIfAsync_ConditionFalse_WithSuccess_ShouldReturnOriginal()
    {
        var task = Task.FromResult(Result<int>.Success(3));

        Result<int> failIfResult = await task.FailIfAsync(
            value => Task.FromResult(value > 5),
            TestError);

        failIfResult.IsSuccess.Should().BeTrue();
        failIfResult.Value.Should().Be(3);
    }

    [Fact]
    public async Task TaskResultT_FailIfAsync_WithFailure_ShouldReturnOriginalFailure()
    {
        var task = Task.FromResult(Result<int>.Failure(TestError));

        Result<int> failIfResult = await task.FailIfAsync(
            value => Task.FromResult(value > 5),
            AnotherError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task TaskResultT_FailIfAsync_WithErrorFunc_ConditionTrue_ShouldReturnComputedError()
    {
        var task = Task.FromResult(Result<int>.Success(10));

        Result<int> failIfResult = await task.FailIfAsync(
            value => Task.FromResult(value > 5),
            value => Task.FromResult(Error.Validation("Computed.Code", $"Value {value}")));

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Description.Should().Contain("10");
    }

    #endregion

    #region Chaining

    [Fact]
    public void ResultT_FailIf_Chained_ShouldApplyConditionally()
    {
        Result<int> result = Result<int>.Success(10)
            .FailIf(v => v > 100, TestError)
            .Then(v => v * 2)
            .FailIf(v => v > 15, AnotherError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(AnotherError);
    }

    #endregion
}
