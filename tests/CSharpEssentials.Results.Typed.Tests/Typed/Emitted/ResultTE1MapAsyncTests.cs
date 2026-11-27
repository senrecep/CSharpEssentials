// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1MapAsyncTests
{
    [Fact]
    public async Task MapAsync_InstanceTaskHandler_Should_MapTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await source.MapAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapAsync_InstanceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await source.MapAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task MapAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.MapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.MapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_TaskSourceSyncHandler_Should_MapTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, string> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError> result = await Task.FromResult(source).MapAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapAsync_TaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, string> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError> result = await Task.FromResult(source).MapAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task MapAsync_TaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, string> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await Task.FromResult(source).MapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_TaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, string> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await pending.Task.MapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_TaskSourceTaskHandler_Should_MapTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await Task.FromResult(source).MapAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapAsync_TaskSourceTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await Task.FromResult(source).MapAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task MapAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await Task.FromResult(source).MapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await pending.Task.MapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, Task<string>> handler = v => { calls++; return Task.FromResult<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await Task.FromResult(source).MapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceSyncHandler_Should_MapTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, string> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).MapAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceSyncHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, string> handler = v => { calls++; return TypedFixture.Text(v); };

        Result<string, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).MapAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, string> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).MapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceSyncHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, string> handler = v => { calls++; return TypedFixture.Text(v); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).MapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceValueTaskHandler_Should_MapTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).MapAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).MapAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).MapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).MapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).MapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task MapAsync_InstanceValueTaskHandler_Should_MapTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await source.MapAsync(handler);

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task MapAsync_InstanceValueTaskHandler_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Result<string, NotFoundError> result = await source.MapAsync(handler);

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task MapAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.MapAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task MapAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled()
    {
        Result<int, NotFoundError> source = 5;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<int, ValueTask<string>> handler = v => { calls++; return new ValueTask<string>(TypedFixture.Text(v)); };

        Func<Task> act = async () => await source.MapAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
#endif
}
