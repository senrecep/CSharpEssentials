// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1BindAsyncTests
{
    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await source.BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await source.BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await source.BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await source.BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Result<string, NotFoundError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Result<string, NotFoundError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Result<string, NotFoundError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await Task.FromResult(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, NotFoundError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await pending.Task.BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await Task.FromResult(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await pending.Task.BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Result<string, NotFoundError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Result<string, NotFoundError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Result<string, NotFoundError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, NotFoundError>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ValidationError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ValidationError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await source.BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceValueTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ValidationError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ValidationError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden1Async_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await source.BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceSyncHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Result<string, NotFoundError, ValidationError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Result<string, NotFoundError, ValidationError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
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
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ValidationError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ValidationError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await Task.FromResult(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
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
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Result<string, NotFoundError, ValidationError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Result<string, NotFoundError, ValidationError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, ValidationError>> handler = _ => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceValueTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ValidationError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Result<string, NotFoundError, ValidationError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden1Async_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError>>(new Result<string, ValidationError>(TypedFixture.Invalid)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_InstanceTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsPlainError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Plain;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Plain);
    }

    [Fact]
    public async Task BindWiden2Async_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden2Async_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Func<Task> act = async () => await source.BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_InstanceValueTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsPlainError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Plain;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Plain);
    }

    [Fact]
    public async Task BindWiden2Async_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await source.BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden2Async_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Func<Task> act = async () => await source.BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_TaskSourceSyncHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsPlainError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Result<string, ValidationError, PlainError>> handler = _ => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Plain;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Plain);
    }

    [Fact]
    public async Task BindWiden2Async_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Result<string, ValidationError, PlainError>> handler = _ => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden2Async_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Result<string, ValidationError, PlainError>> handler = _ => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); };

        Func<Task> act = async () => await Task.FromResult(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, ValidationError, PlainError>> handler = _ => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); };

        Func<Task> act = async () => await pending.Task.BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_TaskSourceTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsPlainError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Plain;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Plain);
    }

    [Fact]
    public async Task BindWiden2Async_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await Task.FromResult(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden2Async_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Task<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Func<Task> act = async () => await Task.FromResult(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return Task.FromResult<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Func<Task> act = async () => await pending.Task.BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_ValueTaskSourceSyncHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsPlainError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Result<string, ValidationError, PlainError>> handler = _ => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Plain;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Plain);
    }

    [Fact]
    public async Task BindWiden2Async_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Result<string, ValidationError, PlainError>> handler = _ => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden2Async_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Result<string, ValidationError, PlainError>> handler = _ => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, ValidationError, PlainError>> handler = _ => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_ValueTaskSourceValueTaskHandler_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsPlainError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Plain;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Plain);
    }

    [Fact]
    public async Task BindWiden2Async_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Result<string, NotFoundError, ValidationError, PlainError> result = await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task BindWiden2Async_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).BindWidenAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindWiden2Async_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<Result<string, ValidationError, PlainError>>> handler = _ => { calls++; return new ValueTask<Result<string, ValidationError, PlainError>>(new Result<string, ValidationError, PlainError>(TypedFixture.Plain)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).BindWidenAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
}
