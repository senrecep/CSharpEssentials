using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultElseDoTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.ElseDo

    [Fact]
    public void Result_ElseDo_ErrorsAction_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result elseResult = result.ElseDo((Error[] errors) => called = true);

        elseResult.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_ElseDo_ErrorsAction_WithFailure_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result elseResult = result.ElseDo((Error[] errors) => called = true);

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_ElseDo_FirstErrorAction_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result elseResult = result.ElseDoFirst(error => called = true);

        elseResult.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_ElseDo_FirstErrorAction_WithFailure_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result elseResult = result.ElseDoFirst(error => called = true);

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion

    #region Result<T>.ElseDo

    [Fact]
    public void ResultT_ElseDo_ErrorsAction_WithSuccess_ShouldNotExecute()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> elseResult = result.ElseDo((Error[] errors) => called = true);

        elseResult.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_ElseDo_ErrorsAction_WithFailure_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> elseResult = result.ElseDo((Error[] errors) => called = true);

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void ResultT_ElseDo_FirstErrorAction_WithSuccess_ShouldNotExecute()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> elseResult = result.ElseDoFirst(error => called = true);

        elseResult.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_ElseDo_FirstErrorAction_WithFailure_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> elseResult = result.ElseDoFirst(error => called = true);

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion

    #region Result.ElseDoAsync

    [Fact]
    public async Task Result_ElseDoAsync_ErrorsFunc_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result elseResult = await result.ElseDoAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        elseResult.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_ElseDoAsync_ErrorsFunc_WithFailure_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result elseResult = await result.ElseDoAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseDoAsync_FirstErrorFunc_WithSuccess_ShouldNotExecute()
    {
        var result = Result.Success();
        bool called = false;

        Result elseResult = await result.ElseDoFirstAsync(error => { called = true; return Task.CompletedTask; });

        elseResult.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_ElseDoAsync_FirstErrorFunc_WithFailure_ShouldExecute()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result elseResult = await result.ElseDoFirstAsync(error => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion

    #region Result<T>.ElseDoAsync

    [Fact]
    public async Task ResultT_ElseDoAsync_ErrorsFunc_WithSuccess_ShouldNotExecute()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> elseResult = await result.ElseDoAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        elseResult.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_ElseDoAsync_ErrorsFunc_WithFailure_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> elseResult = await result.ElseDoAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_ElseDoAsync_FirstErrorFunc_WithSuccess_ShouldNotExecute()
    {
        var result = 42.ToResult();
        bool called = false;

        Result<int> elseResult = await result.ElseDoFirstAsync(error => { called = true; return Task.CompletedTask; });

        elseResult.IsSuccess.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_ElseDoAsync_FirstErrorFunc_WithFailure_ShouldExecute()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> elseResult = await result.ElseDoFirstAsync(error => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion

    #region ElseDoAsync Extensions

    [Fact]
    public async Task Result_ElseDoAsync_Task_ErrorsFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result elseResult = await task.ElseDoAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseDoAsync_Task_FirstErrorFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result elseResult = await task.ElseDoFirstAsync(error => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseDoAsync_ValueTask_ErrorsFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result elseResult = await valueTask.ElseDoAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseDoAsync_ValueTask_FirstErrorFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result elseResult = await valueTask.ElseDoFirstAsync(error => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_ElseDoAsync_Task_ErrorsFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> elseResult = await task.ElseDoAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_ElseDoAsync_Task_FirstErrorFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> elseResult = await task.ElseDoFirstAsync(error => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_ElseDoAsync_ValueTask_ErrorsFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> elseResult = await valueTask.ElseDoAsync((Error[] errors) => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_ElseDoAsync_ValueTask_FirstErrorFunc_WithFailure_ShouldExecute()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> elseResult = await valueTask.ElseDoFirstAsync(error => { called = true; return Task.CompletedTask; });

        elseResult.IsFailure.Should().BeTrue();
        called.Should().BeTrue();
    }

    #endregion
}
