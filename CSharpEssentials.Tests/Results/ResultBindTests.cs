using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultBindTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");


    #region Result.Bind

    [Fact]
    public void Result_Bind_ToGeneric_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result<int> bound = result.Bind(() => 42.ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(42);
    }

    [Fact]
    public void Result_Bind_ToGeneric_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result<int> bound = result.Bind(() =>
        {
            called = true;
            return 42.ToResult();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_Bind_ToResult_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result bound = result.Bind(() => Result.Success());

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_Bind_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result bound = result.Bind(() =>
        {
            called = true;
            return Result.Success();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_Bind_ToGeneric_Task_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result<int> bound = await result.Bind(() => Task.FromResult(42.ToResult()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(42);
    }

    [Fact]
    public async Task Result_Bind_ToGeneric_Task_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result<int> bound = await result.Bind(() =>
        {
            called = true;
            return Task.FromResult(42.ToResult());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_Bind_ToResult_Task_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result bound = await result.Bind(() => Task.FromResult(Result.Success()));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_Bind_ToResult_Task_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result bound = await result.Bind(() =>
        {
            called = true;
            return Task.FromResult(Result.Success());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_Bind_ToGeneric_ValueTask_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result<int> bound = await result.Bind(() => ValueTask.FromResult(42.ToResult()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(42);
    }

    [Fact]
    public async Task Result_Bind_ToGeneric_ValueTask_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result<int> bound = await result.Bind(() =>
        {
            called = true;
            return ValueTask.FromResult(42.ToResult());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_Bind_ToResult_ValueTask_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result bound = await result.Bind(() => ValueTask.FromResult(Result.Success()));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_Bind_ToResult_ValueTask_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result bound = await result.Bind(() =>
        {
            called = true;
            return ValueTask.FromResult(Result.Success());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result.BindAsync Extensions

    [Fact]
    public async Task Result_BindAsync_Task_ToResult_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result bound = await task.BindAsync(() => Result.Success());

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_BindAsync_Task_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result bound = await task.BindAsync(() =>
        {
            called = true;
            return Result.Success();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_BindAsync_Task_ToResult_TaskFunc_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result bound = await task.BindAsync(() => Task.FromResult(Result.Success()));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_BindAsync_Task_ToResult_TaskFunc_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result bound = await task.BindAsync(() =>
        {
            called = true;
            return Task.FromResult(Result.Success());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_BindAsync_ValueTask_ToGeneric_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result<int> bound = await valueTask.BindAsync(() => 42.ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(42);
    }

    [Fact]
    public async Task Result_BindAsync_ValueTask_ToGeneric_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> bound = await valueTask.BindAsync(() =>
        {
            called = true;
            return 42.ToResult();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_BindAsync_ValueTask_ToResult_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result bound = await valueTask.BindAsync(() => Result.Success());

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_BindAsync_ValueTask_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result bound = await valueTask.BindAsync(() =>
        {
            called = true;
            return Result.Success();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_BindAsync_ValueTask_ToGeneric_ValueTaskFunc_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result<int> bound = await valueTask.BindAsync(() => ValueTask.FromResult(42.ToResult()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(42);
    }

    [Fact]
    public async Task Result_BindAsync_ValueTask_ToGeneric_ValueTaskFunc_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<int> bound = await valueTask.BindAsync(() =>
        {
            called = true;
            return ValueTask.FromResult(42.ToResult());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_BindAsync_ValueTask_ToResult_ValueTaskFunc_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result bound = await valueTask.BindAsync(() => ValueTask.FromResult(Result.Success()));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_BindAsync_ValueTask_ToResult_ValueTaskFunc_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result bound = await valueTask.BindAsync(() =>
        {
            called = true;
            return ValueTask.FromResult(Result.Success());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result<T>.Bind

    [Fact]
    public void ResultT_Bind_ToGeneric_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result<string> bound = result.Bind(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_Bind_ToGeneric_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> bound = result.Bind(v =>
        {
            called = true;
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Bind_ToResult_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result bound = result.Bind(v => v > 5 ? Result.Success() : Result.Failure(TestError));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ResultT_Bind_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result bound = result.Bind(v =>
        {
            called = true;
            return Result.Success();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_Bind_ToGeneric_Task_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result<string> bound = await result.Bind(v => Task.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task ResultT_Bind_ToGeneric_Task_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> bound = await result.Bind(v =>
        {
            called = true;
            return Task.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_Bind_ToResult_Task_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result bound = await result.Bind(v => Task.FromResult(v > 5 ? Result.Success() : Result.Failure(TestError)));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_Bind_ToResult_Task_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result bound = await result.Bind(v =>
        {
            called = true;
            return Task.FromResult(Result.Success());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_Bind_ToGeneric_ValueTask_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result<string> bound = await result.Bind(v => ValueTask.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task ResultT_Bind_ToGeneric_ValueTask_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> bound = await result.Bind(v =>
        {
            called = true;
            return ValueTask.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_Bind_ToResult_ValueTask_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result bound = await result.Bind(v => ValueTask.FromResult(v > 5 ? Result.Success() : Result.Failure(TestError)));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_Bind_ToResult_ValueTask_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result bound = await result.Bind(v =>
        {
            called = true;
            return ValueTask.FromResult(Result.Success());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result<T>.BindAsync Extensions

    [Fact]
    public async Task ResultT_BindAsync_Task_ToGeneric_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result<string> bound = await task.BindAsync((int v) => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task ResultT_BindAsync_Task_ToGeneric_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<string> bound = await task.BindAsync((int v) =>
        {
            called = true;
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_BindAsync_Task_ToResult_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result bound = await task.BindAsync((int v) => v > 5 ? Result.Success() : Result.Failure(TestError));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_BindAsync_Task_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result bound = await task.BindAsync((int v) =>
        {
            called = true;
            return Result.Success();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_BindAsync_Task_ToGeneric_TaskFunc_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result<string> bound = await task.BindAsync((int v) => Task.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task ResultT_BindAsync_Task_ToGeneric_TaskFunc_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<string> bound = await task.BindAsync((int v) =>
        {
            called = true;
            return Task.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_BindAsync_Task_ToResult_TaskFunc_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result bound = await task.BindAsync((int v) => Task.FromResult(v > 5 ? Result.Success() : Result.Failure(TestError)));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_BindAsync_Task_ToResult_TaskFunc_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result bound = await task.BindAsync((int v) =>
        {
            called = true;
            return Task.FromResult(Result.Success());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_BindAsync_ValueTask_ToGeneric_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result<string> bound = await valueTask.BindAsync((int v) => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task ResultT_BindAsync_ValueTask_ToGeneric_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<string> bound = await valueTask.BindAsync((int v) =>
        {
            called = true;
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_BindAsync_ValueTask_ToResult_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result bound = await valueTask.BindAsync((int v) => v > 5 ? Result.Success() : Result.Failure(TestError));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_BindAsync_ValueTask_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result bound = await valueTask.BindAsync((int v) =>
        {
            called = true;
            return Result.Success();
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_BindAsync_ValueTask_ToGeneric_ValueTaskFunc_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result<string> bound = await valueTask.BindAsync((int v) => ValueTask.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task ResultT_BindAsync_ValueTask_ToGeneric_ValueTaskFunc_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result<string> bound = await valueTask.BindAsync((int v) =>
        {
            called = true;
            return ValueTask.FromResult(v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_BindAsync_ValueTask_ToResult_ValueTaskFunc_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(10.ToResult());
#pragma warning restore IDE0008

        Result bound = await valueTask.BindAsync((int v) => ValueTask.FromResult(v > 5 ? Result.Success() : Result.Failure(TestError)));

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_BindAsync_ValueTask_ToResult_ValueTaskFunc_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool called = false;

        Result bound = await valueTask.BindAsync((int v) =>
        {
            called = true;
            return ValueTask.FromResult(Result.Success());
        });

        bound.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion
}
