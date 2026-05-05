using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultMapTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    #region Result.Map

    [Fact]
    public void Result_Map_Func_WithSuccess_ShouldTransform()
    {
        var result = Result.Success();

        Result<int> mapped = result.Map(() => 42);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void Result_Map_Func_WithFailure_ShouldNotTransform()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result<int> mapped = result.Map(() =>
        {
            called = true;
            return 42;
        });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_Map_ResultFunc_WithSuccess_ShouldTransform()
    {
        var result = Result.Success();

        Result<int> mapped = result.Map(() => 42.ToResult());

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void Result_Map_ResultFunc_WithFailure_ShouldNotTransform()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result<int> mapped = result.Map(() =>
        {
            called = true;
            return 42.ToResult();
        });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result<T>.Map

    [Fact]
    public void ResultT_Map_Func_WithSuccess_ShouldTransform()
    {
        var result = 10.ToResult();

        Result<string> mapped = result.Map(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_Map_Func_WithFailure_ShouldNotTransform()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> mapped = result.Map(v =>
        {
            called = true;
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Map_ResultFunc_WithSuccess_ShouldTransform()
    {
        var result = 10.ToResult();

        Result<string> mapped = result.Map(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_Map_ResultFunc_WithFailure_ShouldNotTransform()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> mapped = result.Map(v =>
        {
            called = true;
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult();
        });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion
}
