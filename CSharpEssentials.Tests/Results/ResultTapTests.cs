using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTapTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    #region Result.Tap

    [Fact]
    public void Result_Tap_WithSuccess_ShouldExecuteAction()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.Tap(() => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_Tap_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.Tap(() => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_Tap_BoolCondition_WithSuccess_True_ShouldExecuteAction()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.Tap(true, () => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_Tap_BoolCondition_WithSuccess_False_ShouldNotExecuteAction()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.Tap(false, () => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_Tap_BoolCondition_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.Tap(true, () => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_Tap_FuncCondition_WithSuccess_True_ShouldExecuteAction()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.Tap(() => true, () => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_Tap_FuncCondition_WithSuccess_False_ShouldNotExecuteAction()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.Tap(() => false, () => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_Tap_FuncCondition_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.Tap(() => true, () => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result<T>.Tap

    [Fact]
    public void ResultT_Tap_ValueAction_WithSuccess_ShouldExecuteAction()
    {
        var result = 42.ToResult();
        int captured = 0;

        Result<int> tapped = result.Tap(v => captured = v);

        tapped.IsSuccess.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact]
    public void ResultT_Tap_ValueAction_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.Tap(_ => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_Action_WithSuccess_ShouldExecuteAction()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> tapped = result.Tap(() => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void ResultT_Tap_Action_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.Tap(() => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_BoolCondition_ValueAction_WithSuccess_True_ShouldExecuteAction()
    {
        var result = 42.ToResult();
        int captured = 0;

        Result<int> tapped = result.Tap(true, v => captured = v);

        tapped.IsSuccess.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact]
    public void ResultT_Tap_BoolCondition_ValueAction_WithSuccess_False_ShouldNotExecuteAction()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> tapped = result.Tap(false, _ => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_BoolCondition_ValueAction_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.Tap(true, _ => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_BoolCondition_Action_WithSuccess_True_ShouldExecuteAction()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> tapped = result.Tap(true, () => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void ResultT_Tap_BoolCondition_Action_WithSuccess_False_ShouldNotExecuteAction()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> tapped = result.Tap(false, () => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_BoolCondition_Action_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.Tap(true, () => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_FuncCondition_ValueAction_WithSuccess_True_ShouldExecuteAction()
    {
        var result = 42.ToResult();
        int captured = 0;

        Result<int> tapped = result.Tap(() => true, v => captured = v);

        tapped.IsSuccess.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact]
    public void ResultT_Tap_FuncCondition_ValueAction_WithSuccess_False_ShouldNotExecuteAction()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> tapped = result.Tap(() => false, _ => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_FuncCondition_ValueAction_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.Tap(() => true, _ => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_FuncCondition_Action_WithSuccess_True_ShouldExecuteAction()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> tapped = result.Tap(() => true, () => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void ResultT_Tap_FuncCondition_Action_WithSuccess_False_ShouldNotExecuteAction()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> tapped = result.Tap(() => false, () => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Tap_FuncCondition_Action_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.Tap(() => true, () => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion
}
