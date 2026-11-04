using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeGetValueAsyncOverloadTests
{
    private static Task<Maybe<int>> SomeTask(int value) => Task.FromResult(Maybe<int>.From(value));
    private static Task<Maybe<int>> NoneTask() => Task.FromResult(Maybe<int>.None);
    private static ValueTask<Maybe<int>> SomeValueTask(int value) => new(Maybe<int>.From(value));
    private static ValueTask<Maybe<int>> NoneValueTask() => new(Maybe<int>.None);

    #region Maybe<T> with Task and ValueTask factories

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskSelectorAndTaskDefault_ShouldPickBranch()
    {
        (await Maybe<int>.From(4).GetValueOrDefaultAsync(x => Task.FromResult(x * 2), () => Task.FromResult(-1))).Should().Be(8);
        (await Maybe<int>.None.GetValueOrDefaultAsync(x => Task.FromResult(x * 2), () => Task.FromResult(-1))).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTaskDefault_ShouldPickBranch()
    {
        (await Maybe<int>.From(4).GetValueOrDefaultAsync(() => new ValueTask<int>(-1))).Should().Be(4);
        (await Maybe<int>.None.GetValueOrDefaultAsync(() => new ValueTask<int>(-1))).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_SyncSelectorAndValueTaskDefault_ShouldPickBranch()
    {
        (await Maybe<int>.From(4).GetValueOrDefaultAsync(x => x * 10, () => new ValueTask<int>(-1))).Should().Be(40);
        (await Maybe<int>.None.GetValueOrDefaultAsync(x => x * 10, () => new ValueTask<int>(-1))).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTaskSelectorAndPlainDefault_ShouldPickBranch()
    {
        (await Maybe<int>.From(4).GetValueOrDefaultAsync(x => new ValueTask<int>(x * 2), -1)).Should().Be(8);
        (await Maybe<int>.None.GetValueOrDefaultAsync(x => new ValueTask<int>(x * 2), -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTaskSelectorAndValueTaskDefault_ShouldPickBranch()
    {
        (await Maybe<int>.From(4).GetValueOrDefaultAsync(x => new ValueTask<int>(x * 2), () => new ValueTask<int>(-1))).Should().Be(8);
        (await Maybe<int>.None.GetValueOrDefaultAsync(x => new ValueTask<int>(x * 2), () => new ValueTask<int>(-1))).Should().Be(-1);
    }

    #endregion

    #region Task<Maybe<T>>

    [Fact]
    public async Task GetValueOrDefaultAsync_Task_FuncDefault_ShouldPickBranch()
    {
        (await SomeTask(4).GetValueOrDefaultAsync(() => -1)).Should().Be(4);
        (await NoneTask().GetValueOrDefaultAsync(() => -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_Task_SelectorAndPlainDefault_ShouldPickBranch()
    {
        (await SomeTask(4).GetValueOrDefaultAsync(x => x * 2, -1)).Should().Be(8);
        (await NoneTask().GetValueOrDefaultAsync(x => x * 2, -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_Task_SelectorAndFuncDefault_ShouldPickBranch()
    {
        (await SomeTask(4).GetValueOrDefaultAsync(x => x * 2, () => -1)).Should().Be(8);
        (await NoneTask().GetValueOrDefaultAsync(x => x * 2, () => -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_Task_TaskDefault_ShouldPickBranch()
    {
        (await SomeTask(4).GetValueOrDefaultAsync(() => Task.FromResult(-1))).Should().Be(4);
        (await NoneTask().GetValueOrDefaultAsync(() => Task.FromResult(-1))).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_Task_TaskSelectorAndPlainDefault_ShouldPickBranch()
    {
        (await SomeTask(4).GetValueOrDefaultAsync(x => Task.FromResult(x * 2), -1)).Should().Be(8);
        (await NoneTask().GetValueOrDefaultAsync(x => Task.FromResult(x * 2), -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_Task_TaskSelectorAndTaskDefault_ShouldPickBranch()
    {
        (await SomeTask(4).GetValueOrDefaultAsync(x => Task.FromResult(x * 2), () => Task.FromResult(-1))).Should().Be(8);
        (await NoneTask().GetValueOrDefaultAsync(x => Task.FromResult(x * 2), () => Task.FromResult(-1))).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_Task_CancelledWhileSourcePending_ShouldThrow()
    {
        var source = new TaskCompletionSource<Maybe<int>>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => source.Task.GetValueOrDefaultAsync(() => -1, cts.Token).WaitAsync(TimeSpan.FromSeconds(5));

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region ValueTask<Maybe<T>>

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTask_FuncDefault_ShouldPickBranch()
    {
        (await SomeValueTask(4).GetValueOrDefaultAsync(() => -1)).Should().Be(4);
        (await NoneValueTask().GetValueOrDefaultAsync(() => -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTask_SelectorAndPlainDefault_ShouldPickBranch()
    {
        (await SomeValueTask(4).GetValueOrDefaultAsync(x => x * 2, -1)).Should().Be(8);
        (await NoneValueTask().GetValueOrDefaultAsync(x => x * 2, -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTask_SelectorAndFuncDefault_ShouldPickBranch()
    {
        (await SomeValueTask(4).GetValueOrDefaultAsync(x => x * 2, () => -1)).Should().Be(8);
        (await NoneValueTask().GetValueOrDefaultAsync(x => x * 2, () => -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTask_ValueTaskDefault_ShouldPickBranch()
    {
        (await SomeValueTask(4).GetValueOrDefaultAsync(() => new ValueTask<int>(-1))).Should().Be(4);
        (await NoneValueTask().GetValueOrDefaultAsync(() => new ValueTask<int>(-1))).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTask_ValueTaskSelectorAndPlainDefault_ShouldPickBranch()
    {
        (await SomeValueTask(4).GetValueOrDefaultAsync(x => new ValueTask<int>(x * 2), -1)).Should().Be(8);
        (await NoneValueTask().GetValueOrDefaultAsync(x => new ValueTask<int>(x * 2), -1)).Should().Be(-1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTask_ValueTaskSelectorAndValueTaskDefault_ShouldPickBranch()
    {
        (await SomeValueTask(4).GetValueOrDefaultAsync(x => new ValueTask<int>(x * 2), () => new ValueTask<int>(-1))).Should().Be(8);
        (await NoneValueTask().GetValueOrDefaultAsync(x => new ValueTask<int>(x * 2), () => new ValueTask<int>(-1))).Should().Be(-1);
    }

    #endregion

    #region GetValueOrThrowAsync

    [Fact]
    public async Task GetValueOrThrowAsync_Task_ShouldReturnValueOrThrow()
    {
        (await SomeTask(4).GetValueOrThrowAsync()).Should().Be(4);

        Func<Task> act = () => NoneTask().GetValueOrThrowAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Maybe has no value.");
    }

    [Fact]
    public async Task GetValueOrThrowAsync_Task_WithMessage_ShouldUseMessage()
    {
        (await SomeTask(4).GetValueOrThrowAsync("missing")).Should().Be(4);

        Func<Task> act = () => NoneTask().GetValueOrThrowAsync("missing");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("missing");
    }

    [Fact]
    public async Task GetValueOrThrowAsync_ValueTask_ShouldReturnValueOrThrow()
    {
        (await SomeValueTask(4).GetValueOrThrowAsync()).Should().Be(4);

        Func<Task> act = async () => await NoneValueTask().GetValueOrThrowAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Maybe has no value.");
    }

    [Fact]
    public async Task GetValueOrThrowAsync_ValueTask_WithMessage_ShouldUseMessage()
    {
        (await SomeValueTask(4).GetValueOrThrowAsync("missing")).Should().Be(4);

        Func<Task> act = async () => await NoneValueTask().GetValueOrThrowAsync("missing");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("missing");
    }

    #endregion
}
