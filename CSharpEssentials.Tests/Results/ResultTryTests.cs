using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTryTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.Try

    [Fact]
    public void Result_Try_Action_Success_ShouldReturnSuccess()
    {
        bool called = false;

        var result = Result.Try(() => { called = true; }, ex => TestError);

        result.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_Try_Action_Exception_ShouldReturnFailure()
    {
        var result = Result.Try(
            () => throw new InvalidOperationException("boom"),
            ex =>
            {
                ex.Message.Should().Be("boom");
                return TestError;
            });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result.Try<TValue>

    [Fact]
    public void Result_TryT_Func_Success_ShouldReturnSuccessWithValue()
    {
        var result = Result.Try(() => 42, ex => TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Result_TryT_Func_Exception_ShouldReturnFailure()
    {
        var result = Result.Try(
            (Func<Result<int>>)(() => throw new InvalidOperationException("boom")),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Result_TryT_ResultFunc_Success_ShouldReturnSuccessWithValue()
    {
        var result = Result.Try(() => 42.ToResult(), ex => TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Result_TryT_ResultFunc_Exception_ShouldReturnFailure()
    {
        var result = Result.Try<int>(
            () => throw new InvalidOperationException("boom"),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result.TryAsync

    [Fact]
    public async Task Result_TryAsync_Action_Success_ShouldReturnSuccess()
    {
        bool called = false;

        Result result = await Result.TryAsync(
            () => { called = true; return Task.CompletedTask; },
            ex => TestError);

        result.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_TryAsync_Action_Exception_ShouldReturnFailure()
    {
        Result result = await Result.TryAsync(
            () => Task.FromException(new InvalidOperationException("boom")),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_TryAsync_Action_CallerCancelled_ShouldPropagate()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => Result.TryAsync(
            () => Task.FromException(new OperationCanceledException(cts.Token)),
            ex => TestError,
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Result_TryAsync_Action_OceWithUnrelatedToken_ShouldReturnFailure()
    {
        using var other = new CancellationTokenSource();
        await other.CancelAsync();
        using var caller = new CancellationTokenSource();

        Result result = await Result.TryAsync(
            () => Task.FromException(new OperationCanceledException(other.Token)),
            ex => TestError,
            caller.Token);

        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_TryAsyncT_Func_CallerCancelled_ShouldPropagate()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => Result.TryAsync(
            () => Task.FromException<int>(new OperationCanceledException(cts.Token)),
            ex => TestError,
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Result_TryAsyncT_Func_OceWithUnrelatedToken_ShouldReturnFailure()
    {
        using var other = new CancellationTokenSource();
        await other.CancelAsync();
        using var caller = new CancellationTokenSource();

        Result<int> result = await Result.TryAsync(
            () => Task.FromException<int>(new OperationCanceledException(other.Token)),
            ex => TestError,
            caller.Token);

        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_TryAsyncT_ResultFunc_CallerCancelled_ShouldPropagate()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => Result.TryAsync(
            () => Task.FromException<Result<int>>(new OperationCanceledException(cts.Token)),
            ex => TestError,
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Result_TryAsyncT_ResultFunc_OceWithUnrelatedToken_ShouldReturnFailure()
    {
        using var other = new CancellationTokenSource();
        await other.CancelAsync();
        using var caller = new CancellationTokenSource();

        Result<int> result = await Result.TryAsync(
            () => Task.FromException<Result<int>>(new OperationCanceledException(other.Token)),
            ex => TestError,
            caller.Token);

        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_TryAsync_ResultFunc_CallerCancelled_ShouldPropagate()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => Result.TryAsync(
            () => Task.FromException<Result>(new OperationCanceledException(cts.Token)),
            ex => TestError,
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Result_TryAsync_ResultFunc_OceWithUnrelatedToken_ShouldReturnFailure()
    {
        using var other = new CancellationTokenSource();
        await other.CancelAsync();
        using var caller = new CancellationTokenSource();

        Result result = await Result.TryAsync(
            () => Task.FromException<Result>(new OperationCanceledException(other.Token)),
            ex => TestError,
            caller.Token);

        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_TryAsync_Action_OceWithoutCallerToken_ShouldReturnFailure()
    {
        Result result = await Result.TryAsync(
            () => Task.FromException(new OperationCanceledException()),
            ex => TestError);

        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_TryAsync_Action_TaskCanceledExceptionWithCallerCancelled_ShouldPropagate()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => Result.TryAsync(
            () => Task.FromException(new TaskCanceledException()),
            ex => TestError,
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Result.TryAsync<TValue>

    [Fact]
    public async Task Result_TryAsyncT_Func_Success_ShouldReturnSuccessWithValue()
    {
        Result<int> result = await Result.TryAsync(() => Task.FromResult(42), ex => TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Result_TryAsyncT_Func_Exception_ShouldReturnFailure()
    {
        Result<int> result = await Result.TryAsync(
            () => Task.FromException<int>(new InvalidOperationException("boom")),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_TryAsyncT_ResultFunc_Success_ShouldReturnSuccessWithValue()
    {
        Result<int> result = await Result.TryAsync(
            () => Task.FromResult(42.ToResult()),
            ex => TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Result_TryAsyncT_ResultFunc_Exception_ShouldReturnFailure()
    {
        Result<int> result = await Result.TryAsync(
            () => Task.FromException<Result<int>>(new InvalidOperationException("boom")),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion
}
