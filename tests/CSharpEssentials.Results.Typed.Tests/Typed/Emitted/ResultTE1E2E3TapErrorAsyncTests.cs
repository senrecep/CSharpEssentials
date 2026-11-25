// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3TapErrorAsyncTests
{
    [Fact]
    public async Task TapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_CallTheAction_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await source.TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return new TaskCompletionSource().Task; };

        Func<Task> act = async () => await source.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await Task.FromResult(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await pending.Task.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_InstanceTaskHandler_Should_CallTheAction_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_InstanceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await source.TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return new TaskCompletionSource().Task; };

        Func<Task> act = async () => await source.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await Task.FromResult(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await pending.Task.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_ValueTaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_ValueTaskSourceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_InstanceTaskHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_InstanceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await source.TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return new TaskCompletionSource().Task; };

        Func<Task> act = async () => await source.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await Task.FromResult(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await pending.Task.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_ValueTaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_ValueTaskSourceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task TapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await source.TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask> handler = _ => { calls++; return new ValueTask(new TaskCompletionSource().Task); };

        Func<Task> act = async () => await source.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_InstanceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_InstanceValueTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await source.TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValueTask> handler = _ => { calls++; return new ValueTask(new TaskCompletionSource().Task); };

        Func<Task> act = async () => await source.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;
        Func<ConflictError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<ConflictError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<ConflictError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<ConflictError, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_InstanceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_InstanceValueTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await source.TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValueTask> handler = _ => { calls++; return new ValueTask(new TaskCompletionSource().Task); };

        Func<Task> act = async () => await source.TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<RateLimited, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<RateLimited, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<RateLimited, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).TapErrorAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<RateLimited, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).TapErrorAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
#endif
}
