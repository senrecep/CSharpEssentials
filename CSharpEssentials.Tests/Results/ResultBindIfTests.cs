using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultBindIfTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.BindIf

    [Fact]
    public void Result_BindIf_BoolTrue_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();
        bool called = false;

        Result bound = result.BindIf(true, () => { called = true; return Result.Success(); });

        bound.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_BindIf_BoolFalse_WithSuccess_ShouldNotExecuteFunction()
    {
        var result = Result.Success();
        bool called = false;

        Result bound = result.BindIf(false, () => { called = true; return Result.Failure(TestError); });

        bound.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_BindIf_Bool_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result bound = result.BindIf(true, () => { called = true; return Result.Success(); });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_BindIf_PredicateTrue_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();
        bool called = false;

        Result bound = result.BindIf(() => true, () => { called = true; return Result.Success(); });

        bound.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_BindIf_PredicateFalse_WithSuccess_ShouldNotExecuteFunction()
    {
        var result = Result.Success();
        bool called = false;

        Result bound = result.BindIf(() => false, () => { called = true; return Result.Failure(TestError); });

        bound.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_BindIf_Predicate_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result bound = result.BindIf(() => true, () => { called = true; return Result.Success(); });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_BindIf_Chained_ShouldApplyConditionally()
    {
        Result result = Result.Success()
            .BindIf(true, () => Result.Success())
            .BindIf(false, () => Result.Failure(TestError))
            .BindIf(true, () => Result.Success());

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Result<T>.BindIf

    [Fact]
    public void ResultT_BindIf_BoolTrue_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<int> bound = result.BindIf(true, value => Result<int>.Success(value * 2));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(20);
    }

    [Fact]
    public void ResultT_BindIf_BoolFalse_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(10);

        Result<int> bound = result.BindIf(false, _ => Result<int>.Success(99));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(10);
    }

    [Fact]
    public void ResultT_BindIf_Bool_WithFailure_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> bound = result.BindIf(true, value => { called = true; return Result<int>.Success(value * 2); });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_BindIf_FuncBoolTrue_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<int> bound = result.BindIf(() => true, value => Result<int>.Success(value * 2));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(20);
    }

    [Fact]
    public void ResultT_BindIf_FuncBoolFalse_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(10);

        Result<int> bound = result.BindIf(() => false, _ => Result<int>.Success(99));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(10);
    }

    [Fact]
    public void ResultT_BindIf_FuncBool_WithFailure_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> bound = result.BindIf(() => true, value => { called = true; return Result<int>.Success(value * 2); });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_BindIf_ValuePredicateTrue_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<int> bound = result.BindIf(value => value > 5, value => Result<int>.Success(value * 2));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(20);
    }

    [Fact]
    public void ResultT_BindIf_ValuePredicateFalse_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(3);

        Result<int> bound = result.BindIf(value => value > 5, _ => Result<int>.Success(99));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(3);
    }

    [Fact]
    public void ResultT_BindIf_ValuePredicate_WithFailure_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> bound = result.BindIf(value => value > 5, value => { called = true; return Result<int>.Success(value * 2); });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_BindIf_Chained_ShouldApplyConditionally()
    {
        Result<int> result = Result<int>.Success(10)
            .BindIf(true, v => Result<int>.Success(v + 1))
            .BindIf(v => v > 5, v => Result<int>.Success(v + 1))
            .BindIf(false, v => Result<int>.Success(v + 100));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(12);
    }

    #endregion
}
