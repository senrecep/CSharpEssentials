// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3FinallyAsyncTests
{
    [Fact]
    public async Task FinallyAsync_TaskSourceSyncHandler_Should_ReturnTheFunctionValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, bool> handler = r => { calls++; return r.IsSuccess; };

        bool result = await Task.FromResult(source).FinallyAsync(handler);

        calls.Should().Be(1);
        bool expected = true;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyAsync_TaskSourceSyncHandler_Should_ReturnTheFunctionValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, bool> handler = r => { calls++; return r.IsSuccess; };

        bool result = await Task.FromResult(source).FinallyAsync(handler);

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, bool> handler = r => { calls++; return r.IsSuccess; };

        Func<Task> act = async () => await pending.Task.FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FinallyAsync_TaskSourceTaskHandler_Should_ReturnTheFunctionValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task<bool>> handler = r => { calls++; return Task.FromResult<bool>(r.IsSuccess); };

        bool result = await Task.FromResult(source).FinallyAsync(handler);

        calls.Should().Be(1);
        bool expected = true;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyAsync_TaskSourceTaskHandler_Should_ReturnTheFunctionValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task<bool>> handler = r => { calls++; return Task.FromResult<bool>(r.IsSuccess); };

        bool result = await Task.FromResult(source).FinallyAsync(handler);

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task<bool>> handler = r => { calls++; return Task.FromResult<bool>(r.IsSuccess); };

        Func<Task> act = async () => await pending.Task.FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FinallyAsync_ValueTaskSourceSyncHandler_Should_ReturnTheFunctionValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, bool> handler = r => { calls++; return r.IsSuccess; };

        bool result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        bool expected = true;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyAsync_ValueTaskSourceSyncHandler_Should_ReturnTheFunctionValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, bool> handler = r => { calls++; return r.IsSuccess; };

        bool result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, bool> handler = r => { calls++; return r.IsSuccess; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FinallyActionAsync_TaskSourceSyncHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Action<Result<int, NotFoundError, ConflictError, RateLimited>> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyActionAsync_TaskSourceSyncHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Action<Result<int, NotFoundError, ConflictError, RateLimited>> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FinallyActionAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Action<Result<int, NotFoundError, ConflictError, RateLimited>> handler = _ => calls++;

        Func<Task> act = async () => await pending.Task.FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FinallyActionAsync_TaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyActionAsync_TaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FinallyActionAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await pending.Task.FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceSyncHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Action<Result<int, NotFoundError, ConflictError, RateLimited>> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceSyncHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Action<Result<int, NotFoundError, ConflictError, RateLimited>> handler = _ => calls++;

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Action<Result<int, NotFoundError, ConflictError, RateLimited>> handler = _ => calls++;

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceTaskHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, Task> handler = _ => { calls++; return Task.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task FinallyAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheFunctionValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, ValueTask<bool>> handler = r => { calls++; return new ValueTask<bool>(r.IsSuccess); };

        bool result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        bool expected = true;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheFunctionValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, ValueTask<bool>> handler = r => { calls++; return new ValueTask<bool>(r.IsSuccess); };

        bool result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, ValueTask<bool>> handler = r => { calls++; return new ValueTask<bool>(r.IsSuccess); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceValueTaskHandler_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FinallyAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FinallyActionAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<Result<int, NotFoundError, ConflictError, RateLimited>, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FinallyAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
#endif
}
