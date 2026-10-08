using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTraverseAsyncTests
{
    private static Error Fail(int n) => Error.Failure($"ERR_{n}", $"Error {n}");

    private static List<int> Items(params int[] values) => [.. values];

    private static IEnumerable<ValueTask<Result<int>>> ValueTasks(params Result<int>[] results)
    {
        foreach (Result<int> result in results)
            yield return new ValueTask<Result<int>>(result);
    }

    #region TraverseAsync (Task selector)

    [Fact]
    public async Task TraverseAsync_Should_ReturnAllValues_When_AllSucceed()
    {
        List<int> source = Items(1, 2, 3);

        Result<int[]> result = await source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return Result<int>.Success(x * 10);
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(10, 20, 30);
    }

    [Fact]
    public async Task TraverseAsync_Should_AccumulateAllErrorsInOrder_When_MultipleFail()
    {
        List<int> source = Items(1, 2, 3, 4, 5);

        Result<int[]> result = await source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return x % 2 == 0 ? Result<int>.Failure(Fail(x)) : Result<int>.Success(x);
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(e => e.Code).Should().Equal("ERR_2", "ERR_4");
    }

    [Fact]
    public async Task TraverseAsync_Should_KeepInvokingSelector_After_Failure()
    {
        List<int> seen = [];

        Result<int[]> result = await Items(1, 2, 3).TraverseAsync(x =>
        {
            seen.Add(x);
            return Task.FromResult(Result<int>.Failure(Fail(x)));
        });

        seen.Should().Equal(1, 2, 3);
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task TraverseAsync_Should_ReturnEmptySuccess_When_SourceIsEmpty()
    {
        Result<int[]> result = await Array.Empty<int>().TraverseAsync(x => Task.FromResult(Result<int>.Success(x)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task TraverseAsync_Should_RunSequentially_And_InOrder()
    {
        List<string> log = [];
        int running = 0;
        int maxRunning = 0;

        Result<int[]> result = await Enumerable.Range(1, 4).TraverseAsync(async x =>
        {
            int now = Interlocked.Increment(ref running);
            maxRunning = Math.Max(maxRunning, now);
            log.Add($"start{x}");
            await Task.Delay(5);
            log.Add($"end{x}");
            Interlocked.Decrement(ref running);
            return Result<int>.Success(x);
        });

        result.IsSuccess.Should().BeTrue();
        maxRunning.Should().Be(1);
        log.Should().Equal("start1", "end1", "start2", "end2", "start3", "end3", "start4", "end4");
    }

    [Fact]
    public async Task TraverseAsync_Should_ThrowOperationCanceled_When_TokenCancelledBeforeStart()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        int calls = 0;

        Func<Task> act = () => Items(1, 2).TraverseAsync(x =>
        {
            calls++;
            return Task.FromResult(Result<int>.Success(x));
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TraverseAsync_Should_ThrowOperationCanceled_When_CancelledMidWay()
    {
        using CancellationTokenSource cts = new();
        List<int> seen = [];

        Func<Task> act = () => Items(1, 2, 3, 4).TraverseAsync(async x =>
        {
            seen.Add(x);
            if (x == 2)
                await cts.CancelAsync();
            return Result<int>.Success(x);
        }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        seen.Should().Equal(1, 2);
    }

    [Fact]
    public async Task TraverseAsync_Should_NotConvertSelectorException_ToError()
    {
        Func<Task> act = () => Items(1).TraverseAsync(
            new Func<int, Task<Result<int>>>(_ => throw new InvalidOperationException("boom")));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void TraverseAsync_Should_ThrowArgumentNull_When_ArgumentsAreNull()
    {
        IEnumerable<int> source = [1];
        Func<int, Task<Result<int>>> selector = x => Task.FromResult(Result<int>.Success(x));

        Action nullSource = () => ((IEnumerable<int>)null!).TraverseAsync(selector);
        Action nullSelector = () => source.TraverseAsync((Func<int, Task<Result<int>>>)null!);

        nullSource.Should().Throw<ArgumentNullException>().WithParameterName("source");
        nullSelector.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    #endregion

    #region TraverseAsync (Task selector with CancellationToken)

    [Fact]
    public async Task TraverseAsync_WithToken_Should_PassTokenToSelector_And_ReturnValues()
    {
        using CancellationTokenSource cts = new();
        CancellationToken received = default;

        Result<int[]> result = await Items(1, 2).TraverseAsync(async (x, ct) =>
        {
            received = ct;
            await Task.Yield();
            return Result<int>.Success(x + 1);
        }, cts.Token);

        received.Should().Be(cts.Token);
        result.Value.Should().Equal(2, 3);
    }

    [Fact]
    public async Task TraverseAsync_WithToken_Should_AccumulateErrorsInOrder()
    {
        Result<int[]> result = await Items(1, 2, 3).TraverseAsync(async (x, _) =>
        {
            await Task.Yield();
            return x == 2 ? Result<int>.Success(x) : Result<int>.Failure(Fail(x));
        });

        result.Errors.Select(e => e.Code).Should().Equal("ERR_1", "ERR_3");
    }

    [Fact]
    public async Task TraverseAsync_WithToken_Should_ThrowOperationCanceled_When_Cancelled()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        Func<Task> act = () => Items(1).TraverseAsync(
            (x, _) => Task.FromResult(Result<int>.Success(x)), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void TraverseAsync_WithToken_Should_ThrowArgumentNull_When_SelectorIsNull()
    {
        Action act = () => Items(1).TraverseAsync((Func<int, CancellationToken, Task<Result<int>>>)null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    #endregion

    #region TraverseAsync (ValueTask selector)

#if NET9_0_OR_GREATER
    [Fact]
    public async Task TraverseAsync_ValueTask_Should_ReturnAllValues_When_AllSucceed()
    {
        ValueTask<Result<int[]>> pending = Items(1, 2, 3).TraverseAsync(
            new Func<int, ValueTask<Result<int>>>(x => ValueTask.FromResult(Result<int>.Success(x * 2))));

        Result<int[]> result = await pending;

        result.Value.Should().Equal(2, 4, 6);
    }

    [Fact]
    public async Task TraverseAsync_ValueTask_Should_AccumulateErrorsInOrder()
    {
        Result<int[]> result = await Items(1, 2, 3).TraverseAsync(
            new Func<int, ValueTask<Result<int>>>(x => ValueTask.FromResult(Result<int>.Failure(Fail(x)))));

        result.Errors.Select(e => e.Code).Should().Equal("ERR_1", "ERR_2", "ERR_3");
    }

    [Fact]
    public async Task TraverseAsync_ValueTask_Should_ThrowOperationCanceled_When_Cancelled()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        Func<Task> act = async () => await Items(1).TraverseAsync(
            new Func<int, ValueTask<Result<int>>>(x => ValueTask.FromResult(Result<int>.Success(x))), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TraverseAsync_ValueTaskWithToken_Should_PassTokenAndAccumulate()
    {
        using CancellationTokenSource cts = new();
        CancellationToken received = default;

        Result<int[]> result = await Items(1, 2).TraverseAsync(
            new Func<int, CancellationToken, ValueTask<Result<int>>>((x, ct) =>
            {
                received = ct;
                return ValueTask.FromResult(Result<int>.Failure(Fail(x)));
            }), cts.Token);

        received.Should().Be(cts.Token);
        result.Errors.Select(e => e.Code).Should().Equal("ERR_1", "ERR_2");
    }

    [Fact]
    public async Task TraverseAsync_ValueTask_Should_ThrowArgumentNull_When_SelectorIsNull()
    {
        Func<Task> act = async () => await Items(1).TraverseAsync((Func<int, ValueTask<Result<int>>>)null!);

        (await act.Should().ThrowAsync<ArgumentNullException>()).WithParameterName("selector");
    }
#endif

    [Fact]
    public async Task TraverseAsync_Should_BindUntypedAsyncLambdas_ToTaskFlavour()
    {
        List<int> source = Items(1, 2);

        Task<Result<int[]>> byTask = source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return Result<int>.Success(x);
        });
        Task<Result<int[]>> byTaskWithToken = source.TraverseAsync(async (x, ct) =>
        {
            await Task.Yield();
            return Result<int>.Success(x);
        });

        (await byTask).Value.Should().Equal(1, 2);
        (await byTaskWithToken).Value.Should().Equal(1, 2);
    }

    #endregion

    #region SequenceAsync

    [Fact]
    public async Task SequenceAsync_Task_Should_ReturnValues_When_AllSucceed()
    {
        Task<Result<int>>[] tasks = [Task.FromResult(Result<int>.Success(1)), Task.FromResult(Result<int>.Success(2))];

        Result<int[]> result = await tasks.SequenceAsync();

        result.Value.Should().Equal(1, 2);
    }

    [Fact]
    public async Task SequenceAsync_Task_Should_AccumulateErrorsInOrder()
    {
        Task<Result<int>>[] tasks =
        [
            Task.FromResult(Result<int>.Failure(Fail(1))),
            Task.FromResult(Result<int>.Success(2)),
            Task.FromResult(Result<int>.Failure(Fail(3)))
        ];

        Result<int[]> result = await tasks.SequenceAsync();

        result.Errors.Select(e => e.Code).Should().Equal("ERR_1", "ERR_3");
    }

    [Fact]
    public async Task SequenceAsync_Task_Should_ReturnEmptySuccess_When_SourceIsEmpty()
    {
        Result<int[]> result = await Array.Empty<Task<Result<int>>>().SequenceAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task SequenceAsync_Task_Should_AwaitAllTasks_Even_When_OneFaults()
    {
        TaskCompletionSource<Result<int>> second = new();
        TaskCompletionSource<Result<int>> third = new();
        Task<Result<int>>[] tasks =
        [
            Task.FromException<Result<int>>(new InvalidOperationException("first")),
            second.Task,
            third.Task
        ];

        Task<Result<int[]>> pending = tasks.SequenceAsync();
        pending.IsCompleted.Should().BeFalse();

        second.SetException(new ArgumentException("second"));
        third.SetResult(Result<int>.Success(3));

        Func<Task> act = () => pending;

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("first");
        tasks.Should().OnlyContain(t => t.IsCompleted);
    }

    [Fact]
    public async Task SequenceAsync_Task_Should_ThrowOperationCanceled_When_TokenCancelled()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        Func<Task> act = () => new[] { Task.FromResult(Result<int>.Success(1)) }.SequenceAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void SequenceAsync_Task_Should_ThrowArgumentNull_When_SourceIsNull()
    {
        Action act = () => ((IEnumerable<Task<Result<int>>>)null!).SequenceAsync();

        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [Fact]
    public async Task SequenceAsync_ValueTask_Should_AccumulateErrorsInOrder()
    {
        IEnumerable<ValueTask<Result<int>>> tasks = ValueTasks(
            Result<int>.Success(1), Result<int>.Failure(Fail(2)), Result<int>.Failure(Fail(3)));

        Result<int[]> result = await tasks.SequenceAsync();

        result.Errors.Select(e => e.Code).Should().Equal("ERR_2", "ERR_3");
    }

    [Fact]
    public async Task SequenceAsync_ValueTask_Should_ReturnValues_When_AllSucceed()
    {
        IEnumerable<ValueTask<Result<int>>> tasks = ValueTasks(Result<int>.Success(1), Result<int>.Success(2));

        Result<int[]> result = await tasks.SequenceAsync();

        result.Value.Should().Equal(1, 2);
    }

    [Fact]
    public async Task SequenceAsync_ValueTask_Should_AwaitAllTasks_Even_When_OneFaults()
    {
        bool secondAwaited = false;
        async Task<Result<int>> Second()
        {
            await Task.Yield();
            secondAwaited = true;
            return Result<int>.Success(2);
        }

        IEnumerable<ValueTask<Result<int>>> tasks = FaultedThenSecond();

        IEnumerable<ValueTask<Result<int>>> FaultedThenSecond()
        {
            yield return new ValueTask<Result<int>>(Task.FromException<Result<int>>(new InvalidOperationException("first")));
            yield return new ValueTask<Result<int>>(Second());
        }

        Func<Task> act = async () => await tasks.SequenceAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("first");
        secondAwaited.Should().BeTrue();
    }

    [Fact]
    public async Task SequenceAsync_ValueTask_Should_ThrowOperationCanceled_When_TokenCancelled()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        Func<Task> act = async () => await ValueTasks(Result<int>.Success(1)).SequenceAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SequenceAsync_ValueTask_Should_ThrowArgumentNull_When_SourceIsNull()
    {
        Func<Task> act = async () => await ((IEnumerable<ValueTask<Result<int>>>)null!).SequenceAsync();

        (await act.Should().ThrowAsync<ArgumentNullException>()).WithParameterName("source");
    }

    #endregion
}
