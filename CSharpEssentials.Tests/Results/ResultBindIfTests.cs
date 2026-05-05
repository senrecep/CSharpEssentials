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
        var result = 10.ToResult();

        Result<int> bound = result.BindIf(true, value => (value * 2).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(20);
    }

    [Fact]
    public void ResultT_BindIf_BoolFalse_WithSuccess_ShouldReturnOriginal()
    {
        var result = 10.ToResult();

        Result<int> bound = result.BindIf(false, _ => 99.ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(10);
    }

    [Fact]
    public void ResultT_BindIf_Bool_WithFailure_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> bound = result.BindIf(true, value => { called = true; return (value * 2).ToResult(); });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_BindIf_FuncBoolTrue_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result<int> bound = result.BindIf(() => true, value => (value * 2).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(20);
    }

    [Fact]
    public void ResultT_BindIf_FuncBoolFalse_WithSuccess_ShouldReturnOriginal()
    {
        var result = 10.ToResult();

        Result<int> bound = result.BindIf(() => false, _ => 99.ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(10);
    }

    [Fact]
    public void ResultT_BindIf_FuncBool_WithFailure_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> bound = result.BindIf(() => true, value => { called = true; return (value * 2).ToResult(); });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_BindIf_ValuePredicateTrue_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result<int> bound = result.BindIf(value => value > 5, value => (value * 2).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(20);
    }

    [Fact]
    public void ResultT_BindIf_ValuePredicateFalse_WithSuccess_ShouldReturnOriginal()
    {
        var result = 3.ToResult();

        Result<int> bound = result.BindIf(value => value > 5, _ => 99.ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(3);
    }

    [Fact]
    public void ResultT_BindIf_ValuePredicate_WithFailure_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> bound = result.BindIf(value => value > 5, value => { called = true; return (value * 2).ToResult(); });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_BindIf_Chained_ShouldApplyConditionally()
    {
        Result<int> result = 10.ToResult()
            .BindIf(true, v => (v + 1).ToResult())
            .BindIf(v => v > 5, v => (v + 1).ToResult())
            .BindIf(false, v => (v + 100).ToResult());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(12);
    }

    #endregion
}
