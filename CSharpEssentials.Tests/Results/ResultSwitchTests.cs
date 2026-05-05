using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultSwitchTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");
    private static readonly Error AnotherError = Error.Validation("VAL", "Validation error");

    #region Result.Switch

    [Fact]
    public void Result_Switch_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();
        bool successCalled = false;
        bool failureCalled = false;

        result.Switch(
            () => successCalled = true,
            errors => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public void Result_Switch_WithFailure_ShouldCallOnFailure()
    {
        var result = Result.Failure(TestError);
        bool successCalled = false;
        bool failureCalled = false;

        result.Switch(
            () => successCalled = true,
            errors => failureCalled = true);

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Result_SwitchAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();
        bool successCalled = false;
        bool failureCalled = false;

        await result.SwitchAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            errors => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_SwitchAsync_WithFailure_ShouldCallOnFailure()
    {
        var result = Result.Failure(TestError);
        bool successCalled = false;
        bool failureCalled = false;

        await result.SwitchAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            errors => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public void Result_SwitchFirst_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();
        bool successCalled = false;
        bool failureCalled = false;

        result.SwitchFirst(
            () => successCalled = true,
            error => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public void Result_SwitchFirst_WithFailure_ShouldCallOnFirstError()
    {
        var result = Result.Failure(TestError);
        bool successCalled = false;
        Error? captured = null;

        result.SwitchFirst(
            () => successCalled = true,
            error => captured = error);

        successCalled.Should().BeFalse();
        captured.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_SwitchFirstAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();
        bool successCalled = false;
        bool failureCalled = false;

        await result.SwitchFirstAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            error => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_SwitchFirstAsync_WithFailure_ShouldCallOnFirstError()
    {
        var result = Result.Failure(TestError);
        bool successCalled = false;
        Error? captured = null;

        await result.SwitchFirstAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            error => { captured = error; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        captured.Should().Be(TestError);
    }

    [Fact]
    public void Result_SwitchLast_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();
        bool successCalled = false;
        bool failureCalled = false;

        result.SwitchLast(
            () => successCalled = true,
            error => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public void Result_SwitchLast_WithFailure_ShouldCallOnLastError()
    {
        var result = Result.Failure(TestError, AnotherError);
        bool successCalled = false;
        Error? captured = null;

        result.SwitchLast(
            () => successCalled = true,
            error => captured = error);

        successCalled.Should().BeFalse();
        captured.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_SwitchLastAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();
        bool successCalled = false;
        bool failureCalled = false;

        await result.SwitchLastAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            error => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_SwitchLastAsync_WithFailure_ShouldCallOnLastError()
    {
        var result = Result.Failure(TestError, AnotherError);
        bool successCalled = false;
        Error? captured = null;

        await result.SwitchLastAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            error => { captured = error; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        captured.Should().Be(AnotherError);
    }

    #endregion

    #region Result Switch Extensions

    [Fact]
    public async Task Result_Switch_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.Switch(
            () => successCalled = true,
            errors => failureCalled = true,
            default);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_Switch_Task_WithFailure_ShouldCallOnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.Switch(
            () => successCalled = true,
            errors => failureCalled = true,
            default);

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Result_SwitchAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.SwitchAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            errors => { failureCalled = true; return Task.CompletedTask; },
            default);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_SwitchAsync_Task_WithFailure_ShouldCallOnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.SwitchAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            errors => { failureCalled = true; return Task.CompletedTask; },
            default);

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Result_SwitchFirst_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.SwitchFirst(
            () => successCalled = true,
            error => failureCalled = true,
            default);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_SwitchFirst_Task_WithFailure_ShouldCallOnFirstError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool successCalled = false;
        Error? captured = null;

        await task.SwitchFirst(
            () => successCalled = true,
            error => captured = error,
            default);

        successCalled.Should().BeFalse();
        captured.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_SwitchFirstAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.SwitchFirstAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            error => { failureCalled = true; return Task.CompletedTask; },
            default);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_SwitchFirstAsync_Task_WithFailure_ShouldCallOnFirstError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool successCalled = false;
        Error? captured = null;

        await task.SwitchFirstAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            error => { captured = error; return Task.CompletedTask; },
            default);

        successCalled.Should().BeFalse();
        captured.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_SwitchLast_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.SwitchLast(
            () => successCalled = true,
            error => failureCalled = true,
            default);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_SwitchLast_Task_WithFailure_ShouldCallOnLastError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError, AnotherError));
#pragma warning restore IDE0008
        bool successCalled = false;
        Error? captured = null;

        await task.SwitchLast(
            () => successCalled = true,
            error => captured = error,
            default);

        successCalled.Should().BeFalse();
        captured.Should().Be(AnotherError);
    }

    [Fact]
    public async Task Result_SwitchLastAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.SwitchLastAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            error => { failureCalled = true; return Task.CompletedTask; },
            default);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_SwitchLastAsync_Task_WithFailure_ShouldCallOnLastError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError, AnotherError));
#pragma warning restore IDE0008
        bool successCalled = false;
        Error? captured = null;

        await task.SwitchLastAsync(
            () => { successCalled = true; return Task.CompletedTask; },
            error => { captured = error; return Task.CompletedTask; },
            default);

        successCalled.Should().BeFalse();
        captured.Should().Be(AnotherError);
    }

    #endregion

    #region Result<T>.Switch

    [Fact]
    public void ResultT_Switch_WithSuccess_ShouldCallOnSuccess()
    {
        var result = 42.ToResult();
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        result.Switch(
            v => { successCalled = true; captured = v; },
            errors => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public void ResultT_Switch_WithFailure_ShouldCallOnFailure()
    {
        var result = Result<int>.Failure(TestError);
        bool successCalled = false;
        bool failureCalled = false;

        result.Switch(
            v => successCalled = true,
            errors => failureCalled = true);

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_SwitchAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = 42.ToResult();
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await result.SwitchAsync(
            v => { successCalled = true; captured = v; return Task.CompletedTask; },
            errors => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_SwitchAsync_WithFailure_ShouldCallOnFailure()
    {
        var result = Result<int>.Failure(TestError);
        bool successCalled = false;
        bool failureCalled = false;

        await result.SwitchAsync(
            v => { successCalled = true; return Task.CompletedTask; },
            errors => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public void ResultT_SwitchFirst_WithSuccess_ShouldCallOnSuccess()
    {
        var result = 42.ToResult();
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        result.SwitchFirst(
            v => { successCalled = true; captured = v; },
            error => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public void ResultT_SwitchFirst_WithFailure_ShouldCallOnFirstError()
    {
        var result = Result<int>.Failure(TestError);
        bool successCalled = false;
        Error? captured = null;

        result.SwitchFirst(
            v => successCalled = true,
            error => captured = error);

        successCalled.Should().BeFalse();
        captured.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_SwitchFirstAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = 42.ToResult();
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await result.SwitchFirstAsync(
            v => { successCalled = true; captured = v; return Task.CompletedTask; },
            error => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_SwitchFirstAsync_WithFailure_ShouldCallOnFirstError()
    {
        var result = Result<int>.Failure(TestError);
        bool successCalled = false;
        Error? captured = null;

        await result.SwitchFirstAsync(
            v => { successCalled = true; return Task.CompletedTask; },
            error => { captured = error; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        captured.Should().Be(TestError);
    }

    [Fact]
    public void ResultT_SwitchLast_WithSuccess_ShouldCallOnSuccess()
    {
        var result = 42.ToResult();
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        result.SwitchLast(
            v => { successCalled = true; captured = v; },
            error => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public void ResultT_SwitchLast_WithFailure_ShouldCallOnLastError()
    {
        var result = Result<int>.Failure(TestError, AnotherError);
        bool successCalled = false;
        Error? captured = null;

        result.SwitchLast(
            v => successCalled = true,
            error => captured = error);

        successCalled.Should().BeFalse();
        captured.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_SwitchLastAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = 42.ToResult();
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await result.SwitchLastAsync(
            v => { successCalled = true; captured = v; return Task.CompletedTask; },
            error => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_SwitchLastAsync_WithFailure_ShouldCallOnLastError()
    {
        var result = Result<int>.Failure(TestError, AnotherError);
        bool successCalled = false;
        Error? captured = null;

        await result.SwitchLastAsync(
            v => { successCalled = true; return Task.CompletedTask; },
            error => { captured = error; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        captured.Should().Be(AnotherError);
    }

    #endregion

    #region Result<T> Switch Extensions

    [Fact]
    public async Task ResultT_Switch_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(42.ToResult());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await task.Switch(
            v => { successCalled = true; captured = v; },
            errors => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_Switch_Task_WithFailure_ShouldCallOnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.Switch(
            v => successCalled = true,
            errors => failureCalled = true);

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_SwitchAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(42.ToResult());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await task.SwitchAsync(
            v => { successCalled = true; captured = v; return Task.CompletedTask; },
            errors => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_SwitchAsync_Task_WithFailure_ShouldCallOnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;

        await task.SwitchAsync(
            v => { successCalled = true; return Task.CompletedTask; },
            errors => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_SwitchFirst_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(42.ToResult());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await task.SwitchFirst(
            v => { successCalled = true; captured = v; },
            error => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_SwitchFirst_Task_WithFailure_ShouldCallOnFirstError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool successCalled = false;
        Error? captured = null;

        await task.SwitchFirst(
            v => successCalled = true,
            error => captured = error);

        successCalled.Should().BeFalse();
        captured.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_SwitchFirstAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(42.ToResult());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await task.SwitchFirstAsync(
            v => { successCalled = true; captured = v; return Task.CompletedTask; },
            error => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_SwitchFirstAsync_Task_WithFailure_ShouldCallOnFirstError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008
        bool successCalled = false;
        Error? captured = null;

        await task.SwitchFirstAsync(
            v => { successCalled = true; return Task.CompletedTask; },
            error => { captured = error; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        captured.Should().Be(TestError);
    }

    [Fact]
    public async Task ResultT_SwitchLast_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(42.ToResult());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await task.SwitchLast(
            v => { successCalled = true; captured = v; },
            error => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_SwitchLast_Task_WithFailure_ShouldCallOnLastError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError, AnotherError));
#pragma warning restore IDE0008
        bool successCalled = false;
        Error? captured = null;

        await task.SwitchLast(
            v => successCalled = true,
            error => captured = error);

        successCalled.Should().BeFalse();
        captured.Should().Be(AnotherError);
    }

    [Fact]
    public async Task ResultT_SwitchLastAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(42.ToResult());
#pragma warning restore IDE0008
        bool successCalled = false;
        bool failureCalled = false;
        int captured = 0;

        await task.SwitchLastAsync(
            v => { successCalled = true; captured = v; return Task.CompletedTask; },
            error => { failureCalled = true; return Task.CompletedTask; });

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_SwitchLastAsync_Task_WithFailure_ShouldCallOnLastError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError, AnotherError));
#pragma warning restore IDE0008
        bool successCalled = false;
        Error? captured = null;

        await task.SwitchLastAsync(
            v => { successCalled = true; return Task.CompletedTask; },
            error => { captured = error; return Task.CompletedTask; });

        successCalled.Should().BeFalse();
        captured.Should().Be(AnotherError);
    }

    #endregion
}
