// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3EnsureAsyncTests
{
    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = _ => { calls++; return new TaskCompletionSource<bool>().Task; };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await Task.FromResult(source).EnsureAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await pending.Task.EnsureAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await Task.FromResult(source).EnsureAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await pending.Task.EnsureAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = _ => { calls++; return new TaskCompletionSource<bool>().Task; };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await pending.Task.EnsureAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await pending.Task.EnsureAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = _ => { calls++; return new TaskCompletionSource<bool>().Task; };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await pending.Task.EnsureAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await Task.FromResult(source).EnsureAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await pending.Task.EnsureAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v > 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = _ => { calls++; return new TaskCompletionSource<bool>().Task; };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await Task.FromResult(source).FailIfAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await pending.Task.FailIfAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await Task.FromResult(source).FailIfAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await pending.Task.FailIfAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = _ => { calls++; return new TaskCompletionSource<bool>().Task; };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await pending.Task.FailIfAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await pending.Task.FailIfAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = _ => { calls++; return new TaskCompletionSource<bool>().Task; };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await pending.Task.FailIfAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await Task.FromResult(source).FailIfAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await pending.Task.FailIfAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceSyncHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceSyncHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, bool> handler = v => { calls++; return v < 0; };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<bool>> handler = v => { calls++; return Task.FromResult<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = _ => { calls++; return new ValueTask<bool>(new TaskCompletionSource<bool>().Task); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = _ => { calls++; return new ValueTask<bool>(new TaskCompletionSource<bool>().Task); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = _ => { calls++; return new ValueTask<bool>(new TaskCompletionSource<bool>().Task); };

        Func<Task> act = async () => await source.EnsureAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).EnsureAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task EnsureRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v > 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).EnsureAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = _ => { calls++; return new ValueTask<bool>(new TaskCompletionSource<bool>().Task); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.NotFound);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.NotFound, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = _ => { calls++; return new ValueTask<bool>(new TaskCompletionSource<bool>().Task); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Conflict);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfConflictErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.Conflict, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await source.FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = _ => { calls++; return new ValueTask<bool>(new TaskCompletionSource<bool>().Task); };

        Func<Task> act = async () => await source.FailIfAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = -5;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Result<int, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).FailIfAsync(handler, TypedFixture.Limited);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task FailIfRateLimitedAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<bool>> handler = v => { calls++; return new ValueTask<bool>(v < 0); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).FailIfAsync(handler, TypedFixture.Limited, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
#endif
}
