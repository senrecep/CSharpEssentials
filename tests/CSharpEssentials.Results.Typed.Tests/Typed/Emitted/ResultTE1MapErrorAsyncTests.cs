// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1MapErrorAsyncTests
{
    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError> result = await source.MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError> result = await source.MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_FinishTheRunningHandler_When_TheTokenIsCanceledWhileItRuns()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        TaskCompletionSource<ValidationError> running = new();
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return running.Task; };

        Task<Result<int, ValidationError>> call = source.MapErrorAsync(handler, cancellation.Token);
        await cancellation.CancelAsync();
        running.SetResult(TypedFixture.Invalid);
        Result<int, ValidationError> result = await call;

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError> result = await source.MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError> result = await source.MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_FinishTheRunningHandler_When_TheTokenIsCanceledWhileItRuns()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        TaskCompletionSource<ValidationError> running = new();
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(running.Task); };

        ValueTask<Result<int, ValidationError>> call = source.MapErrorAsync(handler, cancellation.Token);
        await cancellation.CancelAsync();
        running.SetResult(TypedFixture.Invalid);
        Result<int, ValidationError> result = await call;

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceSyncHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, ValidationError> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, ValidationError> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await pending.Task.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await pending.Task.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_FinishTheRunningHandler_When_TheTokenIsCanceledWhileItRuns()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        TaskCompletionSource<ValidationError> running = new();
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return running.Task; };

        Task<Result<int, ValidationError>> call = Task.FromResult(source).MapErrorAsync(handler, cancellation.Token);
        await cancellation.CancelAsync();
        running.SetResult(TypedFixture.Invalid);
        Result<int, ValidationError> result = await call;

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, ValidationError> result = await new ValueTask<Result<int, NotFoundError>>(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, ValidationError> result = await new ValueTask<Result<int, NotFoundError>>(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError> result = await new ValueTask<Result<int, NotFoundError>>(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError> result = await new ValueTask<Result<int, NotFoundError>>(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_FinishTheRunningHandler_When_TheTokenIsCanceledWhileItRuns()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        TaskCompletionSource<ValidationError> running = new();
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(running.Task); };

        ValueTask<Result<int, ValidationError>> call = new ValueTask<Result<int, NotFoundError>>(source).MapErrorAsync(handler, cancellation.Token);
        await cancellation.CancelAsync();
        running.SetResult(TypedFixture.Invalid);
        Result<int, ValidationError> result = await call;

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
    }
}
