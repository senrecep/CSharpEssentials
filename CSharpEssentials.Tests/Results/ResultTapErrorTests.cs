using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTapErrorTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.TapError

    [Fact]
    public void Result_TapError_ErrorsAction_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.TapError((Error[] errors) => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_TapError_ErrorsAction_WithFailure_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.TapError((Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_TapError_FirstErrorAction_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.TapErrorFirst(error => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_TapError_FirstErrorAction_WithFailure_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.TapErrorFirst(error => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion

    #region Result<T>.TapError

    [Fact]
    public void ResultT_TapError_ErrorsAction_WithSuccess_ShouldNotExecute()
    {
        var result = Result<int>.Success(42);
        bool called = false;

        Result<int> tapped = result.TapError((Error[] errors) => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_TapError_ErrorsAction_WithFailure_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.TapError((Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void ResultT_TapError_FirstErrorAction_WithSuccess_ShouldNotExecute()
    {
        var result = Result<int>.Success(42);
        bool called = false;

        Result<int> tapped = result.TapErrorFirst(error => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_TapError_FirstErrorAction_WithFailure_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.TapErrorFirst(error => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion

    #region Result.TapErrorIf

    [Fact]
    public void Result_TapErrorIf_BoolCondition_WithFailure_True_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.TapErrorIf(true, (Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_TapErrorIf_BoolCondition_WithFailure_False_ShouldNotExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.TapErrorIf(false, (Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_TapErrorIf_BoolCondition_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.TapErrorIf(true, (Error[] errors) => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_TapErrorIf_FuncCondition_WithFailure_True_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.TapErrorIf(() => true, (Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_TapErrorIf_FuncCondition_WithFailure_False_ShouldNotExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = result.TapErrorIf(() => false, (Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_TapErrorIf_FuncCondition_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = result.TapErrorIf(() => true, (Error[] errors) => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result<T>.TapErrorIf

    [Fact]
    public void ResultT_TapErrorIf_BoolCondition_WithFailure_True_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.TapErrorIf(true, (Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void ResultT_TapErrorIf_BoolCondition_WithFailure_False_ShouldNotExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.TapErrorIf(false, (Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_TapErrorIf_BoolCondition_WithSuccess_ShouldNotExecute()
    {
        var result = Result<int>.Success(42);
        bool called = false;

        Result<int> tapped = result.TapErrorIf(true, (Error[] errors) => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_TapErrorIf_FuncCondition_WithFailure_True_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.TapErrorIf(() => true, (Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void ResultT_TapErrorIf_FuncCondition_WithFailure_False_ShouldNotExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = result.TapErrorIf(() => false, (Error[] errors) => called = true);

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_TapErrorIf_FuncCondition_WithSuccess_ShouldNotExecute()
    {
        var result = Result<int>.Success(42);
        bool called = false;

        Result<int> tapped = result.TapErrorIf(() => true, (Error[] errors) => called = true);

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result.TapErrorAsync

    [Fact]
    public async Task Result_TapErrorAsync_ErrorsFunc_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = await result.TapErrorAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_TapErrorAsync_ErrorsFunc_WithFailure_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = await result.TapErrorAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapErrorAsync_FirstErrorFunc_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result tapped = await result.TapErrorFirstAsync(error => { called = true; return Task.CompletedTask; });

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_TapErrorAsync_FirstErrorFunc_WithFailure_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result tapped = await result.TapErrorFirstAsync(error => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion

    #region Result<T>.TapErrorAsync

    [Fact]
    public async Task ResultT_TapErrorAsync_ErrorsFunc_WithSuccess_ShouldNotExecute()
    {
        var result = Result<int>.Success(42);
        bool called = false;

        Result<int> tapped = await result.TapErrorAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_TapErrorAsync_ErrorsFunc_WithFailure_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = await result.TapErrorAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_TapErrorAsync_FirstErrorFunc_WithSuccess_ShouldNotExecute()
    {
        var result = Result<int>.Success(42);
        bool called = false;

        Result<int> tapped = await result.TapErrorFirstAsync(error => { called = true; return Task.CompletedTask; });

        tapped.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_TapErrorAsync_FirstErrorFunc_WithFailure_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> tapped = await result.TapErrorFirstAsync(error => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion

    #region TapErrorAsync Extensions

    [Fact]
    public async Task Result_TapErrorAsync_Task_ErrorsFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result tapped = await task.TapErrorAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapErrorAsync_Task_FirstErrorFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result tapped = await task.TapErrorFirstAsync(error => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapErrorAsync_ValueTask_ErrorsFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result tapped = await valueTask.TapErrorAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapErrorAsync_ValueTask_FirstErrorFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result tapped = await valueTask.TapErrorFirstAsync(error => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_TapErrorAsync_Task_ErrorsFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> tapped = await task.TapErrorAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_TapErrorAsync_Task_FirstErrorFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> tapped = await task.TapErrorFirstAsync(error => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_TapErrorAsync_ValueTask_ErrorsFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> tapped = await valueTask.TapErrorAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_TapErrorAsync_ValueTask_FirstErrorFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> tapped = await valueTask.TapErrorFirstAsync(error => { called = true; return Task.CompletedTask; });

        tapped.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion
}
