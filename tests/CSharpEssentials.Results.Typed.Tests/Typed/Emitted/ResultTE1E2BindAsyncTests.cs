// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2BindAsyncTests
{
    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError> result = await source.BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError> result = await source.BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError> result = await source.BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError> result = await source.BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError, ConflictError> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError, ConflictError> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await Task.FromResult(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await pending.Task.BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await Task.FromResult(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await pending.Task.BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await source.BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceValueTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await source.BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceSyncHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await pending.Task.BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await Task.FromResult(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await pending.Task.BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceSyncHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceValueTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ConflictError, ValidationError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError>>(pending.Task).BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
}
