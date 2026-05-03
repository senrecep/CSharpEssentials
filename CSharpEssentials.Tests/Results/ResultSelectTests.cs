using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultSelectTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result<T>.Select

    [Fact]
    public void ResultT_Select_Func_WithSuccess_ShouldTransform()
    {
        var result = Result<int>.Success(10);

        Result<string> selected = result.Select(value => value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        selected.IsSuccess.Should().BeTrue();
        selected.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_Select_Func_WithFailure_ShouldNotTransform()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> selected = result.Select(value =>
        {
            called = true;
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        selected.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Select_ResultFunc_WithSuccess_ShouldTransform()
    {
        var result = Result<int>.Success(10);

        Result<string> selected = result.Select(value => Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        selected.IsSuccess.Should().BeTrue();
        selected.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_Select_ResultFunc_WithFailure_ShouldNotTransform()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> selected = result.Select(value =>
        {
            called = true;
            return Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        });

        selected.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result<T>.SelectMany

    [Fact]
    public void ResultT_SelectMany_Func_WithSuccess_ShouldTransform()
    {
        var result = Result<int>.Success(10);

        Result<string> selected = result.SelectMany(value => value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        selected.IsSuccess.Should().BeTrue();
        selected.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_SelectMany_Func_WithFailure_ShouldNotTransform()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> selected = result.SelectMany(value =>
        {
            called = true;
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        selected.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_SelectMany_ResultFunc_WithSuccess_ShouldTransform()
    {
        var result = Result<int>.Success(10);

        Result<string> selected = result.SelectMany(value => Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        selected.IsSuccess.Should().BeTrue();
        selected.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_SelectMany_ResultFunc_WithFailure_ShouldNotTransform()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> selected = result.SelectMany(value =>
        {
            called = true;
            return Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        });

        selected.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_SelectMany_WithProjector_WithSuccess_ShouldTransform()
    {
        var result = Result<int>.Success(10);

        Result<string> selected = result.SelectMany(
            value => Result<string>.Success("intermediate"),
            (value, intermediate) => $"{value}-{intermediate}");

        selected.IsSuccess.Should().BeTrue();
        selected.Value.Should().Be("10-intermediate");
    }

    [Fact]
    public void ResultT_SelectMany_WithProjector_SelectorFailure_ShouldReturnFailure()
    {
        var result = Result<int>.Success(10);

        Result<string> selected = result.SelectMany(
            _ => Result<string>.Failure(TestError),
            (value, intermediate) => $"{value}-{intermediate}");

        selected.IsFailure.Should().BeTrue();
        selected.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_SelectMany_WithProjector_OriginalFailure_ShouldNotExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> selected = result.SelectMany(
            value => { called = true; return Result<string>.Success("intermediate"); },
            (value, intermediate) => $"{value}-{intermediate}");

        selected.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Chaining (LINQ query syntax compatibility)

    [Fact]
    public void ResultT_Select_Chained_ShouldWorkLikeMap()
    {
        Result<string> result = Result<int>.Success(10)
            .Select(v => v * 2)
            .Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20");
    }

    [Fact]
    public void ResultT_SelectMany_Chained_ShouldWorkLikeBindThenMap()
    {
        Result<int> result = Result<int>.Success(10)
            .SelectMany(v => Result<int>.Success(v + 5))
            .SelectMany(v => Result<int>.Success(v * 2));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(30);
    }

    [Fact]
    public void ResultT_SelectMany_WithProjector_Chained_ShouldFlattenCorrectly()
    {
        Result<string> result = Result<int>.Success(10)
            .SelectMany(
                v => Result<int>.Success(v + 5),
                (a, b) => $"{a}:{b}")
            .SelectMany(
                v => Result<string>.Success(v.ToUpperInvariant()),
                (a, b) => $"{a}|{b}");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("10:15|10:15");
    }

    #endregion
}
