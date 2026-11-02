using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeTaskOrAsyncTests
{
    private static Task<Maybe<int>> SomeTask(int value) => Task.FromResult(Maybe<int>.From(value));
    private static Task<Maybe<int>> NoneTask() => Task.FromResult(Maybe<int>.None);
    private static ValueTask<Maybe<int>> SomeValueTask(int value) => new(Maybe<int>.From(value));
    private static ValueTask<Maybe<int>> NoneValueTask() => new(Maybe<int>.None);

    #region Task<Maybe<T>>

    [Fact]
    public async Task OrAsync_Task_PlainFallback_ShouldReturnOriginalOrFallback()
    {
        (await SomeTask(1).OrAsync(9)).Value.Should().Be(1);
        (await NoneTask().OrAsync(9)).Value.Should().Be(9);
    }

    [Fact]
    public async Task OrAsync_Task_FuncFallback_ShouldInvokeOnlyWhenNone()
    {
        int calls = 0;

        (await SomeTask(1).OrAsync(() => { calls++; return 9; })).Value.Should().Be(1);
        calls.Should().Be(0);

        (await NoneTask().OrAsync(() => { calls++; return 9; })).Value.Should().Be(9);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrAsync_Task_MaybeFallback_ShouldReturnOriginalOrFallback()
    {
        (await SomeTask(1).OrAsync(Maybe<int>.From(9))).Value.Should().Be(1);
        (await NoneTask().OrAsync(Maybe<int>.From(9))).Value.Should().Be(9);
        (await NoneTask().OrAsync(Maybe<int>.None)).HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task OrAsync_Task_TaskFallback_ShouldReturnOriginalOrFallback()
    {
        (await SomeTask(1).OrAsync(Task.FromResult(9))).Value.Should().Be(1);
        (await NoneTask().OrAsync(Task.FromResult(9))).Value.Should().Be(9);
    }

    [Fact]
    public async Task OrAsync_Task_FuncTaskFallback_ShouldInvokeOnlyWhenNone()
    {
        int calls = 0;

        (await SomeTask(1).OrAsync(() => { calls++; return Task.FromResult(9); })).Value.Should().Be(1);
        calls.Should().Be(0);

        (await NoneTask().OrAsync(() => { calls++; return Task.FromResult(9); })).Value.Should().Be(9);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrAsync_Task_FuncMaybeFallback_ShouldInvokeOnlyWhenNone()
    {
        int calls = 0;

        (await SomeTask(1).OrAsync(() => { calls++; return Maybe<int>.From(9); })).Value.Should().Be(1);
        calls.Should().Be(0);

        (await NoneTask().OrAsync(() => { calls++; return Maybe<int>.None; })).HasNoValue.Should().BeTrue();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrAsync_Task_FuncTaskMaybeFallback_ShouldInvokeOnlyWhenNone()
    {
        int calls = 0;

        (await SomeTask(1).OrAsync(() => { calls++; return Task.FromResult(Maybe<int>.From(9)); })).Value.Should().Be(1);
        calls.Should().Be(0);

        (await NoneTask().OrAsync(() => { calls++; return Task.FromResult(Maybe<int>.From(9)); })).Value.Should().Be(9);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrAsync_Task_CancelledWhileSourcePending_ShouldThrow()
    {
        var source = new TaskCompletionSource<Maybe<int>>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => source.Task.OrAsync(9, cts.Token).WaitAsync(TimeSpan.FromSeconds(5));

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region ValueTask<Maybe<T>>

    [Fact]
    public async Task OrAsync_ValueTask_PlainFallback_ShouldReturnOriginalOrFallback()
    {
        (await SomeValueTask(1).OrAsync(9)).Value.Should().Be(1);
        (await NoneValueTask().OrAsync(9)).Value.Should().Be(9);
    }

    [Fact]
    public async Task OrAsync_ValueTask_FuncFallback_ShouldInvokeOnlyWhenNone()
    {
        int calls = 0;

        (await SomeValueTask(1).OrAsync(() => { calls++; return 9; })).Value.Should().Be(1);
        calls.Should().Be(0);

        (await NoneValueTask().OrAsync(() => { calls++; return 9; })).Value.Should().Be(9);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrAsync_ValueTask_MaybeFallback_ShouldReturnOriginalOrFallback()
    {
        (await SomeValueTask(1).OrAsync(Maybe<int>.From(9))).Value.Should().Be(1);
        (await NoneValueTask().OrAsync(Maybe<int>.From(9))).Value.Should().Be(9);
    }

    [Fact]
    public async Task OrAsync_ValueTask_FuncMaybeFallback_ShouldInvokeOnlyWhenNone()
    {
        int calls = 0;

        (await SomeValueTask(1).OrAsync(() => { calls++; return Maybe<int>.From(9); })).Value.Should().Be(1);
        calls.Should().Be(0);

        (await NoneValueTask().OrAsync(() => { calls++; return Maybe<int>.From(9); })).Value.Should().Be(9);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrAsync_ValueTask_ValueTaskFallback_ShouldReturnOriginalOrFallback()
    {
        (await SomeValueTask(1).OrAsync(new ValueTask<int>(9))).Value.Should().Be(1);
        (await NoneValueTask().OrAsync(new ValueTask<int>(9))).Value.Should().Be(9);
    }

    [Fact]
    public async Task OrAsync_ValueTask_FuncValueTaskFallback_ShouldInvokeOnlyWhenNone()
    {
        int calls = 0;

        (await SomeValueTask(1).OrAsync(() => { calls++; return new ValueTask<int>(9); })).Value.Should().Be(1);
        calls.Should().Be(0);

        (await NoneValueTask().OrAsync(() => { calls++; return new ValueTask<int>(9); })).Value.Should().Be(9);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrAsync_ValueTask_FuncValueTaskMaybeFallback_ShouldInvokeOnlyWhenNone()
    {
        int calls = 0;

        (await SomeValueTask(1).OrAsync(() => { calls++; return new ValueTask<Maybe<int>>(Maybe<int>.From(9)); })).Value.Should().Be(1);
        calls.Should().Be(0);

        (await NoneValueTask().OrAsync(() => { calls++; return new ValueTask<Maybe<int>>(Maybe<int>.None); })).HasNoValue.Should().BeTrue();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrAsync_ValueTask_CancelledWhileSourcePending_ShouldThrow()
    {
        var source = new TaskCompletionSource<Maybe<int>>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await new ValueTask<Maybe<int>>(source.Task).OrAsync(9, cts.Token).AsTask().WaitAsync(TimeSpan.FromSeconds(5));

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
