// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2TapAsyncTests
{
    [Fact]
    public async Task TapAsync_InstanceTaskHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await source.TapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await source.TapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_TaskSourceSyncHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Func<Task> act = async () => await Task.FromResult(source).TapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Action<int> handler = _ => calls++;

        Func<Task> act = async () => await pending.Task.TapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_TaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await Task.FromResult(source).TapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await pending.Task.TapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await Task.FromResult(source).TapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceSyncHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Action<int> handler = _ => calls++;

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).TapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).TapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_InstanceTaskHandler_Should_CallTheAction_When_ThePredicateAllowsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapIfAsync(v => v > 0, handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_InstanceTaskHandler_Should_SkipTheAction_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = -5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapIfAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await source.TapIfAsync(v => v > 0, handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return new TaskCompletionSource().Task; };

        Func<Task> act = async () => await source.TapIfAsync(v => v > 0, handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceSyncHandler_Should_CallTheAction_When_ThePredicateAllowsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceSyncHandler_Should_SkipTheAction_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = -5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Func<Task> act = async () => await Task.FromResult(source).TapIfAsync(v => v > 0, handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Action<int> handler = _ => calls++;

        Func<Task> act = async () => await pending.Task.TapIfAsync(v => v > 0, handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceTaskHandler_Should_CallTheAction_When_ThePredicateAllowsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceTaskHandler_Should_SkipTheAction_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = -5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await Task.FromResult(source).TapIfAsync(v => v > 0, handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await pending.Task.TapIfAsync(v => v > 0, handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceSyncHandler_Should_CallTheAction_When_ThePredicateAllowsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceSyncHandler_Should_SkipTheAction_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = -5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Action<int> handler = _ => calls++;

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Action<int> handler = _ => calls++;

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).TapIfAsync(v => v > 0, handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceTaskHandler_Should_CallTheAction_When_ThePredicateAllowsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceTaskHandler_Should_SkipTheAction_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = -5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).TapIfAsync(v => v > 0, handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task TapAsync_InstanceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await source.TapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await source.TapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_InstanceValueTaskHandler_Should_CallTheAction_When_ThePredicateAllowsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapIfAsync(v => v > 0, handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_InstanceValueTaskHandler_Should_SkipTheAction_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = -5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await source.TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapIfAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await source.TapIfAsync(v => v > 0, handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return new ValueTask(new TaskCompletionSource().Task); };

        Func<Task> act = async () => await source.TapIfAsync(v => v > 0, handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceValueTaskHandler_Should_CallTheAction_When_ThePredicateAllowsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceValueTaskHandler_Should_SkipTheAction_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = -5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapIfAsync(v => v > 0, handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapIfAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).TapIfAsync(v => v > 0, handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
#endif
}
