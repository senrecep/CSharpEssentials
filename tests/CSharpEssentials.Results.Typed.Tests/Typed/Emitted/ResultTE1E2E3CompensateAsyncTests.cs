// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3CompensateAsyncTests
{
    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, ConflictError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Func<Task> act = async () => await source.CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new TaskCompletionSource<Result<int, ConflictError, RateLimited>>().Task; };

        Func<Task> act = async () => await source.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, ConflictError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Func<Task> act = async () => await Task.FromResult(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Func<Task> act = async () => await pending.Task.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, ConflictError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, ConflictError, RateLimited>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Func<Task> act = async () => await source.CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new TaskCompletionSource<Result<int, NotFoundError, RateLimited>>().Task; };

        Func<Task> act = async () => await source.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_TaskSourceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_TaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_TaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Func<Task> act = async () => await Task.FromResult(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Func<Task> act = async () => await pending.Task.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, RateLimited>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await source.CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Func<Task> act = async () => await source.CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new TaskCompletionSource<Result<int, NotFoundError, ConflictError>>().Task; };

        Func<Task> act = async () => await source.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_TaskSourceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_TaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_TaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Func<Task> act = async () => await Task.FromResult(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Func<Task> act = async () => await pending.Task.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, ConflictError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Func<Task> act = async () => await source.CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(new TaskCompletionSource<Result<int, ConflictError, RateLimited>>().Task); };

        Func<Task> act = async () => await source.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, ConflictError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Result<int, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, ConflictError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, ConflictError, RateLimited>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceValueTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Func<Task> act = async () => await source.CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(new TaskCompletionSource<Result<int, NotFoundError, RateLimited>>().Task); };

        Func<Task> act = async () => await source.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Result<int, NotFoundError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValueTask<Result<int, NotFoundError, RateLimited>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, RateLimited>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceValueTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await source.CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Func<Task> act = async () => await source.CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(new TaskCompletionSource<Result<int, NotFoundError, ConflictError>>().Task); };

        Func<Task> act = async () => await source.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValueTask<Result<int, NotFoundError, ConflictError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
#endif
}
