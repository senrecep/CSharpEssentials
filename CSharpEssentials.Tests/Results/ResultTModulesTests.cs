using System.Globalization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTModulesTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    #region Bind

    [Fact]
    public void Bind_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result<string> bound = result.Bind(value => value.ToString(CultureInfo.InvariantCulture).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public void Bind_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool functionCalled = false;

        Result<string> bound = result.Bind(value =>
        {
            functionCalled = true;
            return value.ToString(CultureInfo.InvariantCulture).ToResult();
        });

        bound.IsFailure.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    [Fact]
    public void Bind_ToNonGenericResult_ShouldWork()
    {
        var result = 10.ToResult();

        Result bound = result.Bind(value => value > 5 ? Result.Success() : Result.Failure(TestError));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result<string> bound = await task.BindAsync(value => value.ToString(CultureInfo.InvariantCulture).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task BindAsync_WithTaskFunc_ShouldWork()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result<string> bound = await task.BindAsync(
            value => Task.FromResult(value.ToString(CultureInfo.InvariantCulture).ToResult()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task BindAsync_WithValueTaskFunc_ShouldWork()
    {
#pragma warning disable IDE0008
        var task = ValueTask.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result<string> bound = await task.BindAsync(value => value.ToString(CultureInfo.InvariantCulture).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    #endregion

    #region Map

    [Fact]
    public void Map_WithSuccess_ShouldTransformValue()
    {
        var result = 10.ToResult();

        Result<string> mapped = result.Map(value => value.ToString(CultureInfo.InvariantCulture));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("10");
    }

    [Fact]
    public void Map_WithFailure_ShouldNotTransform()
    {
        var result = Result<int>.Failure(TestError);
        bool functionCalled = false;

        Result<string> mapped = result.Map(value =>
        {
            functionCalled = true;
            return value.ToString(CultureInfo.InvariantCulture);
        });

        mapped.IsFailure.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    #endregion

    #region Match

    [Fact]
    public void Match_WithSuccess_ShouldCallSuccessFunc()
    {
        var result = 42.ToResult();

        string matched = result.Match(
            value => $"Success: {value}",
            errors => $"Failure: {errors.Length}");

        matched.Should().Be("Success: 42");
    }

    [Fact]
    public void Match_WithFailure_ShouldCallFailureFunc()
    {
        var result = Result<int>.Failure(TestError);

        string matched = result.Match(
            value => $"Success: {value}",
            errors => $"Failure: {errors.Length}");

        matched.Should().Be("Failure: 1");
    }

    #endregion

    #region Tap

    [Fact]
    public void Tap_WithSuccess_ShouldExecuteAction()
    {
        var result = 42.ToResult();
        int capturedValue = 0;

        Result<int> tapped = result.Tap(value => capturedValue = value);

        tapped.IsSuccess.Should().BeTrue();
        capturedValue.Should().Be(42);
    }

    [Fact]
    public void Tap_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool actionCalled = false;

        Result<int> tapped = result.Tap(_ => actionCalled = true);

        tapped.IsFailure.Should().BeTrue();
        actionCalled.Should().BeFalse();
    }

    #endregion

    #region TryCatch

    [Fact]
    public void TryCatch_WithNoException_ShouldReturnSuccess()
    {
        var result = 10.ToResult();

        Result<string> tryResult = result.TryCatch(value => value.ToString(CultureInfo.InvariantCulture).ToResult());

        tryResult.IsSuccess.Should().BeTrue();
        tryResult.Value.Should().Be("10");
    }

    [Fact]
    public void TryCatch_WithException_ShouldReturnFailure()
    {
        var result = 10.ToResult();

        Result<string> tryResult = result.TryCatch<string>(value => throw new InvalidOperationException("Test"));

        tryResult.IsFailure.Should().BeTrue();
    }

    #endregion

    #region GetValue

    [Fact]
    public void GetValueOrDefault_WithSuccess_ShouldReturnValue()
    {
        var result = 42.ToResult();

        int value = result.GetValueOrDefault(0);

        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_WithFailure_ShouldReturnDefault()
    {
        var result = Result<int>.Failure(TestError);

        int value = result.GetValueOrDefault(99);

        value.Should().Be(99);
    }

    #endregion

    #region Select (LINQ)

    [Fact]
    public void Select_ShouldWorkLikeMap()
    {
        var result = 10.ToResult();

        Result<string> selected = result.Select(value => value.ToString(CultureInfo.InvariantCulture));

        selected.IsSuccess.Should().BeTrue();
        selected.Value.Should().Be("10");
    }

    #endregion
}
