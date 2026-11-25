// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1CompensateAsyncTests
{
    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await source.CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Func<Task> act = async () => await source.CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return new TaskCompletionSource<Result<int, NotFoundError>>().Task; };

        Func<Task> act = async () => await source.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await Task.FromResult(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Func<Task> act = async () => await Task.FromResult(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_TaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Func<Task> act = async () => await pending.Task.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, Task<Result<int, NotFoundError>>> handler = _ => { calls++; return Task.FromResult<Result<int, NotFoundError>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, NotFoundError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await source.CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, NotFoundError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await source.CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, NotFoundError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError>>(9); };

        Func<Task> act = async () => await source.CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_InstanceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, NotFoundError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError>>(new TaskCompletionSource<Result<int, NotFoundError>>().Task); };

        Func<Task> act = async () => await source.CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, NotFoundError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).CompensateAsync(handler);

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, NotFoundError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError>>(9); };

        Result<int, NotFoundError> result = await new ValueTask<Result<int, NotFoundError>>(source).CompensateAsync(handler);

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, NotFoundError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(source).CompensateAsync(handler);

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task CompensateNotFoundErrorAsync_ValueTaskSourceValueTaskHandler_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        int calls = 0;
        Func<NotFoundError, ValueTask<Result<int, NotFoundError>>> handler = _ => { calls++; return new ValueTask<Result<int, NotFoundError>>(9); };

        Func<Task> act = async () => await new ValueTask<Result<int, NotFoundError>>(pending.Task).CompensateAsync(handler, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }
#endif
}
