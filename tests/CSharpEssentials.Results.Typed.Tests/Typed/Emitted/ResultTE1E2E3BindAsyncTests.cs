// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3BindAsyncTests
{
    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await source.BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await source.BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task BindAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await source.BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await source.BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task BindAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError, RateLimited>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError, RateLimited>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError, RateLimited>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await Task.FromResult(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError, RateLimited>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await pending.Task.BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await Task.FromResult(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await Task.FromResult(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return Task.FromResult<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await pending.Task.BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError, RateLimited>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError, RateLimited>> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError, RateLimited>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Result<string, NotFoundError, ConflictError, RateLimited>> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).BindAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Result<string, NotFoundError, ConflictError, RateLimited> result = await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).BindAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(source).BindAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task BindAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError, RateLimited>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>> handler = v => { calls++; return new ValueTask<Result<string, NotFoundError, ConflictError, RateLimited>>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>>(pending.Task).BindAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
}
