using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultElseTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");
    private static readonly Error AnotherError = Error.Validation("VAL", "Validation error");

    #region Result.Else

    [Fact]
    public void Result_Else_FuncError_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result elseResult = result.Else(errors => AnotherError);

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_Else_FuncError_WithFailure_ShouldReturnNewError()
    {
        var result = Result.Failure(TestError);

        Result elseResult = result.Else(errors => AnotherError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public void Result_Else_FuncEnumerable_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result elseResult = result.Else(errors => new[] { AnotherError });

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_Else_FuncEnumerable_WithFailure_ShouldReturnNewErrors()
    {
        var result = Result.Failure(TestError);

        Result elseResult = result.Else(errors => new[] { AnotherError });

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().ContainSingle().Which.Should().Be(AnotherError);
    }

    [Fact]
    public void Result_Else_Error_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result elseResult = result.Else(AnotherError);

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_Else_Error_WithFailure_ShouldReturnNewError()
    {
        var result = Result.Failure(TestError);

        Result elseResult = result.Else(AnotherError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_ElseAsync_FuncTaskError_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result elseResult = await result.ElseAsync(errors => Task.FromResult(AnotherError));

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseAsync_FuncTaskError_WithFailure_ShouldReturnNewError()
    {
        var result = Result.Failure(TestError);

        Result elseResult = await result.ElseAsync(errors => Task.FromResult(AnotherError));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_ElseAsync_FuncTaskEnumerable_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result elseResult = await result.ElseAsync(errors => Task.FromResult<IEnumerable<Error>>(new[] { AnotherError }));

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseAsync_FuncTaskEnumerable_WithFailure_ShouldReturnNewErrors()
    {
        var result = Result.Failure(TestError);

        Result elseResult = await result.ElseAsync(errors => Task.FromResult<IEnumerable<Error>>(new[] { AnotherError }));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().ContainSingle().Which.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_ElseAsync_TaskError_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result elseResult = await result.ElseAsync(Task.FromResult(AnotherError));

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseAsync_TaskError_WithFailure_ShouldReturnNewError()
    {
        var result = Result.Failure(TestError);

        Result elseResult = await result.ElseAsync(Task.FromResult(AnotherError));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    #endregion

    #region Result Else Extensions

    [Fact]
    public async Task Result_Else_Task_FuncError_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result elseResult = await task.Else(errors => AnotherError);

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_Else_Task_FuncError_WithFailure_ShouldReturnNewError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result elseResult = await task.Else(errors => AnotherError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_Else_Task_FuncEnumerable_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result elseResult = await task.Else(errors => new[] { AnotherError });

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_Else_Task_FuncEnumerable_WithFailure_ShouldReturnNewErrors()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result elseResult = await task.Else(errors => new[] { AnotherError });

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().ContainSingle().Which.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_Else_Task_Error_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result elseResult = await task.Else(AnotherError);

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_Else_Task_Error_WithFailure_ShouldReturnNewError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result elseResult = await task.Else(AnotherError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_ElseAsync_Task_FuncTaskError_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result elseResult = await task.ElseAsync(errors => Task.FromResult(AnotherError));

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseAsync_Task_FuncTaskError_WithFailure_ShouldReturnNewError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result elseResult = await task.ElseAsync(errors => Task.FromResult(AnotherError));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_ElseAsync_Task_FuncTaskEnumerable_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result elseResult = await task.ElseAsync(errors => Task.FromResult<IEnumerable<Error>>(new[] { AnotherError }));

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseAsync_Task_FuncTaskEnumerable_WithFailure_ShouldReturnNewErrors()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result elseResult = await task.ElseAsync(errors => Task.FromResult<IEnumerable<Error>>(new[] { AnotherError }));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().ContainSingle().Which.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_ElseAsync_Task_TaskError_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result elseResult = await task.ElseAsync(Task.FromResult(AnotherError));

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ElseAsync_Task_TaskError_WithFailure_ShouldReturnNewError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result elseResult = await task.ElseAsync(Task.FromResult(AnotherError));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    #endregion

    #region Result<T>.Else

    [Fact]
    public void ResultT_Else_FuncError_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = result.Else(errors => AnotherError);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_Else_FuncError_WithFailure_ShouldReturnNewError()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(errors => AnotherError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public void ResultT_Else_FuncErrorArray_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = result.Else(errors => new[] { AnotherError });

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_Else_FuncErrorArray_WithFailure_ShouldReturnNewErrors()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(errors => new[] { AnotherError });

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().ContainSingle().Which.Should().Be(AnotherError);
    }

    [Fact]
    public void ResultT_Else_Error_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = result.Else(AnotherError);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_Else_Error_WithFailure_ShouldReturnNewError()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(AnotherError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public void ResultT_Else_FuncValue_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = result.Else(errors => 99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_Else_FuncValue_WithFailure_ShouldReturnNewValue()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(errors => 99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    [Fact]
    public void ResultT_Else_Value_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = result.Else(99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_Else_Value_WithFailure_ShouldReturnNewValue()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    [Fact]
    public async Task ResultT_ElseAsync_FuncTaskValue_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = await result.ElseAsync(errors => Task.FromResult(99));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_FuncTaskValue_WithFailure_ShouldReturnNewValue()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = await result.ElseAsync(errors => Task.FromResult(99));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    [Fact]
    public async Task ResultT_ElseAsync_FuncTaskError_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = await result.ElseAsync(errors => Task.FromResult(AnotherError));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_FuncTaskError_WithFailure_ShouldReturnNewError()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = await result.ElseAsync(errors => Task.FromResult(AnotherError));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_ElseAsync_FuncTaskErrorArray_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = await result.ElseAsync(errors => Task.FromResult(new[] { AnotherError }));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_FuncTaskErrorArray_WithFailure_ShouldReturnNewErrors()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = await result.ElseAsync(errors => Task.FromResult(new[] { AnotherError }));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().ContainSingle().Which.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_ElseAsync_TaskError_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = await result.ElseAsync(Task.FromResult(AnotherError));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_TaskError_WithFailure_ShouldReturnNewError()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = await result.ElseAsync(Task.FromResult(AnotherError));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_ElseAsync_TaskValue_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = await result.ElseAsync(Task.FromResult(99));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_TaskValue_WithFailure_ShouldReturnNewValue()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = await result.ElseAsync(Task.FromResult(99));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    #endregion

    #region Result<T> Else Extensions

    [Fact]
    public async Task ResultT_Else_Task_FuncValue_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(errors => 99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_Else_Task_FuncValue_WithFailure_ShouldReturnNewValue()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(errors => 99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    [Fact]
    public async Task ResultT_Else_Task_Value_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_Else_Task_Value_WithFailure_ShouldReturnNewValue()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_FuncTaskValue_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(errors => Task.FromResult(99));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_FuncTaskValue_WithFailure_ShouldReturnNewValue()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(errors => Task.FromResult(99));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_TaskValue_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(Task.FromResult(99));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_TaskValue_WithFailure_ShouldReturnNewValue()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(Task.FromResult(99));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    [Fact]
    public async Task ResultT_Else_Task_FuncError_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(errors => AnotherError);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_Else_Task_FuncError_WithFailure_ShouldReturnNewError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(errors => AnotherError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_Else_Task_FuncErrorArray_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(errors => new[] { AnotherError });

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_Else_Task_FuncErrorArray_WithFailure_ShouldReturnNewErrors()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(errors => new[] { AnotherError });

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().ContainSingle().Which.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_Else_Task_Error_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(AnotherError);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_Else_Task_Error_WithFailure_ShouldReturnNewError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.Else(AnotherError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_FuncTaskError_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(errors => Task.FromResult(AnotherError));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_FuncTaskError_WithFailure_ShouldReturnNewError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(errors => Task.FromResult(AnotherError));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_FuncTaskErrorArray_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(errors => Task.FromResult(new[] { AnotherError }));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_FuncTaskErrorArray_WithFailure_ShouldReturnNewErrors()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(errors => Task.FromResult(new[] { AnotherError }));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().ContainSingle().Which.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_TaskError_WithSuccess_ShouldReturnOriginal()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(Task.FromResult(AnotherError));

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ElseAsync_Task_TaskError_WithFailure_ShouldReturnNewError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> elseResult = await task.ElseAsync(Task.FromResult(AnotherError));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Should().Be(AnotherError);
    }

    #endregion
}
