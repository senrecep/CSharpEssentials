// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3MapErrorAsyncTests
{
    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError, ConflictError, RateLimited> result = await source.MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError, ConflictError, RateLimited> result = await source.MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError, ConflictError, RateLimited> result = await source.MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError, ConflictError, RateLimited> result = await source.MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceSyncHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, ValidationError, ConflictError, RateLimited> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, ValidationError, ConflictError, RateLimited> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
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
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError, ConflictError, RateLimited> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError, ConflictError, RateLimited> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
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
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, ValidationError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, ValidationError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, ValidationError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_InstanceTaskHandler_Should_MapTheError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await source.MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await source.MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_InstanceValueTaskHandler_Should_MapTheError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await source.MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await source.MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceSyncHandler_Should_MapTheError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await pending.Task.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_MapTheError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await pending.Task.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceSyncHandler_Should_MapTheError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_MapTheError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ValidationError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_InstanceTaskHandler_Should_MapTheError_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await source.MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_InstanceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await source.MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_InstanceValueTaskHandler_Should_MapTheError_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await source.MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await source.MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await source.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceSyncHandler_Should_MapTheError_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await pending.Task.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_MapTheError_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await Task.FromResult(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await pending.Task.MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task<ValidationError>> handler = _ => { calls++; return Task.FromResult<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await Task.FromResult(source).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceSyncHandler_Should_MapTheError_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValidationError> handler = _ => { calls++; return TypedFixture.Invalid; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_MapTheError_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Result<int, NotFoundError, ConflictError, ValidationError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValueTask<ValidationError>> handler = _ => { calls++; return new ValueTask<ValidationError>(TypedFixture.Invalid); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).MapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
}
