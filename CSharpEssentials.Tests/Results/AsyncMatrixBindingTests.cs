using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

/// <summary>
/// Compile-time guards for the flavour-matched async matrix. The typed locals stop compiling if a future overload changes the binding,
/// and the gate-based tests prove that async handlers are awaited instead of running as <c>async void</c>.
/// </summary>
public sealed class AsyncMatrixBindingTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    [Fact]
    public async Task TapAsync_Should_AwaitAsyncLambda_When_TaskSourceOfResultT()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(5);
        var gate = new TaskCompletionSource();

        Task<Result<int>> pending = source.TapAsync(async _ => await gate.Task);
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();

        ((await pending).Value, completedBeforeGate).Should().Be((5, false));
    }

    [Fact]
    public async Task TapAsync_Should_AwaitParameterlessAsyncLambda_When_TaskSourceOfResultT()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(5);
        var gate = new TaskCompletionSource();

        Task<Result<int>> pending = source.TapAsync(async () => await gate.Task);
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();

        ((await pending).Value, completedBeforeGate).Should().Be((5, false));
    }

    [Fact]
    public async Task TapAsync_Should_AwaitAsyncLambda_When_TaskSourceOfResult()
    {
        Task<Result> source = Task.FromResult(Result.Success());
        var gate = new TaskCompletionSource();

        Task<Result> pending = source.TapAsync(async () => await gate.Task);
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();

        ((await pending).IsSuccess, completedBeforeGate).Should().Be((true, false));
    }

    [Fact]
    public async Task TapAsync_Should_AwaitAsyncLambda_When_ValueTaskSourceOfResultT()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));
        var gate = new TaskCompletionSource();

        ValueTask<Result<int>> pending = source.TapAsync(async _ => await gate.Task);
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();

        ((await pending).Value, completedBeforeGate).Should().Be((5, false));
    }

    [Fact]
    public async Task TapAsync_Should_AwaitAsyncLambda_When_ValueTaskSourceOfResult()
    {
        ValueTask<Result> source = new(Result.Success());
        var gate = new TaskCompletionSource();

        ValueTask<Result> pending = source.TapAsync(async () => await gate.Task);
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();

        ((await pending).IsSuccess, completedBeforeGate).Should().Be((true, false));
    }

    [Fact]
    public async Task TapAsync_Should_BindAction_When_ValueTaskSourceGetsNonAsyncLambdaReturningTask()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));

        // No cross cells: a Task-returning call is not convertible to Func<int, ValueTask>, so the Action overload wins
        // and the returned task is discarded. Use an async lambda or return a ValueTask to have it awaited.
        ValueTask<Result<int>> pending = source.TapAsync(_ => Task.Delay(TimeSpan.FromSeconds(30)));
        bool completedImmediately = pending.IsCompleted;

        ((await pending).Value, completedImmediately).Should().Be((5, true));
    }

    [Fact]
    public async Task TapAsync_Should_AwaitNonAsyncLambdaReturningTask_When_TaskSourceOfResultT()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(5);
        var gate = new TaskCompletionSource();

        // Func<int, Task> beats Action<int> for a lambda whose body returns a Task, so the returned task is awaited.
        Task<Result<int>> pending = source.TapAsync(_ => gate.Task);
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();

        ((await pending).Value, completedBeforeGate).Should().Be((5, false));
    }

    [Fact]
    public async Task TapAsync_Should_BindAction_When_TaskSourceGetsNonAsyncLambdaReturningValueTask()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(5);
        var gate = new TaskCompletionSource();

        // No cross cells: a Task source has no Func<int, ValueTask> handler, so the Action overload wins and the
        // returned ValueTask is discarded. Use an async lambda or return a Task to have it awaited.
        Task<Result<int>> pending = source.TapAsync(_ => new ValueTask(gate.Task));
        Result<int> result = await pending;
        bool gateCompletedAfterTap = gate.Task.IsCompleted;
        gate.SetResult();

        (result.Value, gateCompletedAfterTap).Should().Be((5, false));
    }

    [Fact]
    public async Task MapAsync_Should_ReturnNestedTask_When_ValueTaskSourceGetsLambdaReturningTask()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));

        // No cross cells: a Task-returning lambda on a ValueTask source binds the sync map, so TOut is Task<int>.
        ValueTask<Result<Task<int>>> nested = source.MapAsync(x => Task.FromResult(x * 2));
        Result<Task<int>> result = await nested;

        (await result.Value).Should().Be(10);
    }

    [Fact]
    public async Task MapAsync_Should_ReturnValueTaskOfMappedResult_When_ValueTaskSourceGetsAsyncLambda()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));

        ValueTask<Result<int>> pending = source.MapAsync(async v =>
        {
            await Task.Yield();
            return v * 2;
        });

        (await pending).Value.Should().Be(10);
    }

    [Fact]
    public async Task MapAsync_Should_ReturnValueTaskOfMappedResult_When_ValueTaskSourceOfResultGetsAsyncLambda()
    {
        ValueTask<Result> source = new(Result.Success());

        ValueTask<Result<string>> pending = source.MapAsync(async () =>
        {
            await Task.Yield();
            return "mapped";
        });

        (await pending).Value.Should().Be("mapped");
    }

    [Fact]
    public async Task MapAsync_Should_BindTaskOverload_When_ResultTGetsAsyncLambda()
    {
        Result<int> source = 5;

        Task<Result<string>> pending = source.MapAsync(async v =>
        {
            await Task.Yield();
            return $"v{v}";
        });

        (await pending).Value.Should().Be("v5");
    }

    [Fact]
    public async Task MapAsync_Should_BindTaskOverload_When_ResultGetsAsyncLambda()
    {
        Result source = TestError;

        Task<Result<int>> pending = source.MapAsync(async () =>
        {
            await Task.Yield();
            return 1;
        });

        (await pending).FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task TapAsync_Should_BindTaskOverload_When_ResultTGetsAsyncLambda()
    {
        Result<int> source = 5;
        int seen = 0;

        Task<Result<int>> pending = source.TapAsync(async v =>
        {
            await Task.Yield();
            seen = v;
        });

        ((await pending).Value, seen).Should().Be((5, 5));
    }

    [Fact]
    public async Task TapAsync_Should_BindTaskOverload_When_ResultGetsAsyncLambda()
    {
        Result source = Result.Success();
        bool called = false;

        Task<Result> pending = source.TapAsync(async () =>
        {
            await Task.Yield();
            called = true;
        });

        ((await pending).IsSuccess, called).Should().Be((true, true));
    }

    [Fact]
    public async Task BindAsync_Should_ReturnTaskOfResultT_When_TaskSourceOfResultGetsAsyncLambda()
    {
        Task<Result> source = Task.FromResult(Result.Success());

        Task<Result<int>> pending = source.BindAsync(async () =>
        {
            await Task.Yield();
            return (Result<int>)42;
        });

        (await pending).Value.Should().Be(42);
    }

    [Fact]
    public async Task EnsureAsync_Should_BindSyncPredicate_When_TaskSourceGetsSyncLambda()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(5);

        Task<Result<int>> pending = source.EnsureAsync(v => v > 10, TestError);

        (await pending).FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task EnsureAsync_Should_BindSyncPredicate_When_ValueTaskSourceGetsSyncLambda()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));

        ValueTask<Result<int>> pending = source.EnsureAsync(v => v > 0, _ => TestError);

        (await pending).Value.Should().Be(5);
    }

#if NET9_0_OR_GREATER
    [Fact]
    public async Task MapAsync_Should_BindValueTaskOverload_When_ResultTGetsLambdaReturningValueTask()
    {
        Result<int> source = 5;

        ValueTask<Result<int>> pending = source.MapAsync(v => ValueTask.FromResult(v + 1));

        (await pending).Value.Should().Be(6);
    }

    [Fact]
    public async Task TapAsync_Should_BindValueTaskOverload_When_ResultGetsLambdaReturningValueTask()
    {
        Result source = Result.Success();

        ValueTask<Result> pending = source.TapAsync(() => ValueTask.CompletedTask);

        (await pending).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MatchAsync_Should_BindTaskOverload_When_ResultTGetsUntypedAsyncLambdas()
    {
        Result<int> source = 5;

        Task<string> pending = source.MatchAsync(
            async v => { await Task.Yield(); return $"v{v}"; },
            async errors => { await Task.Yield(); return errors[0].Code; });

        (await pending).Should().Be("v5");
    }

    [Fact]
    public async Task MatchAsync_Should_BindValueTaskOverload_When_ResultTGetsLambdasReturningValueTask()
    {
        Result<int> source = TestError;

        ValueTask<string> pending = source.MatchAsync(
            v => ValueTask.FromResult($"v{v}"),
            errors => ValueTask.FromResult(errors[0].Code));

        (await pending).Should().Be("TEST");
    }

    [Fact]
    public async Task ThenAsync_Should_BindTaskOverload_When_ResultGetsUntypedAsyncLambda()
    {
        Result source = Result.Success();

        Task<Result> pending = source.ThenAsync(async () =>
        {
            await Task.Yield();
            return Result.Success();
        });

        (await pending).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureAsync_Should_BindTaskOverload_When_ResultTGetsUntypedAsyncPredicate()
    {
        Result<int> source = 5;

        Task<Result<int>> pending = source.EnsureAsync(async v =>
        {
            await Task.Yield();
            return v > 10;
        }, TestError);

        (await pending).FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task SwitchAsync_Should_BindValueTaskOverload_When_ValueTaskSourceGetsLambdasReturningValueTask()
    {
        ValueTask<Result> source = new(Result.Success());
        string outcome = string.Empty;

        ValueTask pending = source.SwitchAsync(
            () => { outcome = "success"; return ValueTask.CompletedTask; },
            _ => { outcome = "failure"; return ValueTask.CompletedTask; });
        await pending;

        outcome.Should().Be("success");
    }

    [Fact]
    public async Task TapIfAsync_Should_BindTaskOverload_When_ResultTGetsUntypedAsyncLambda()
    {
        Result<int> source = 5;
        int seen = 0;

        Task<Result<int>> pending = source.TapIfAsync(true, async v =>
        {
            await Task.Yield();
            seen = v;
        });

        ((await pending).Value, seen).Should().Be((5, 5));
    }

    [Fact]
    public async Task ElseAsync_Should_BindTaskOverload_When_ResultTGetsUntypedAsyncLambda()
    {
        Result<int> source = TestError;

        Task<Result<int>> pending = source.ElseAsync(async _ =>
        {
            await Task.Yield();
            return 42;
        });

        (await pending).Value.Should().Be(42);
    }

    [Fact]
    public async Task ElseAsync_Should_BindValueTaskOverload_When_ResultGetsLambdaReturningValueTask()
    {
        Result source = TestError;

        ValueTask<Result> pending = source.ElseAsync(_ => ValueTask.FromResult(Error.Conflict("C", "c")));

        (await pending).FirstError.Code.Should().Be("C");
    }

    [Fact]
    public async Task FailIfAsync_Should_BindTaskOverload_When_ResultTGetsUntypedAsyncPredicate()
    {
        Result<int> source = 5;

        Task<Result<int>> pending = source.FailIfAsync(async v =>
        {
            await Task.Yield();
            return v > 0;
        }, TestError);

        (await pending).FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task SwitchFirstAsync_Should_BindTaskOverload_When_ResultGetsUntypedAsyncLambdas()
    {
        Result source = TestError;
        string outcome = string.Empty;

        Task pending = source.SwitchFirstAsync(
            async () => { await Task.Yield(); outcome = "success"; },
            async e => { await Task.Yield(); outcome = e.Code; });
        await pending;

        outcome.Should().Be("TEST");
    }

    [Fact]
    public async Task TapErrorAsync_Should_BindTaskOverload_When_ResultGetsUntypedAsyncLambda()
    {
        Result source = TestError;
        int calls = 0;

        Task<Result> pending = source.TapErrorAsync(async _ =>
        {
            await Task.Yield();
            calls++;
        });

        ((await pending).IsFailure, calls).Should().Be((true, 1));
    }

    [Fact]
    public async Task TapErrorAsync_Should_BindValueTaskOverload_When_ResultTGetsLambdaReturningValueTask()
    {
        Result<int> source = TestError;
        int calls = 0;

        ValueTask<Result<int>> pending = source.TapErrorAsync(_ => { calls++; return ValueTask.CompletedTask; });

        ((await pending).IsFailure, calls).Should().Be((true, 1));
    }

    [Fact]
    public async Task ElseDoFirstAsync_Should_BindTaskOverload_When_ResultTGetsUntypedAsyncLambda()
    {
        Result<int> source = TestError;
        string seen = string.Empty;

        Task<Result<int>> pending = source.ElseDoFirstAsync(async e =>
        {
            await Task.Yield();
            seen = e.Code;
        });

        ((await pending).IsFailure, seen).Should().Be((true, "TEST"));
    }

    [Fact]
    public async Task CompensateAsync_Should_BindTaskOverload_When_ResultTGetsUntypedAsyncLambda()
    {
        Result<int> source = TestError;

        Task<Result<int>> pending = source.CompensateAsync(async _ =>
        {
            await Task.Yield();
            return Result<int>.Success(42);
        });

        (await pending).Value.Should().Be(42);
    }

    [Fact]
    public async Task ThenEnsureAsync_Should_BindTaskOverload_When_ResultTGetsUntypedAsyncLambda()
    {
        Result<int> source = 5;

        Task<Result<int>> pending = source.ThenEnsureAsync(async _ =>
        {
            await Task.Yield();
            return Result.Failure(TestError);
        });

        (await pending).FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task TryAsync_Should_BindTaskOverload_When_GivenUntypedAsyncLambda()
    {
        Task<Result<int>> pending = Result.TryAsync(async () =>
        {
            await Task.Yield();
            return 7;
        }, _ => TestError);

        (await pending).Value.Should().Be(7);
    }

    [Fact]
    public async Task TryAsync_Should_BindValueTaskOverload_When_GivenLambdaReturningValueTask()
    {
        ValueTask<Result<int>> pending = Result.TryAsync(() => ValueTask.FromResult(7), _ => TestError);

        (await pending).Value.Should().Be(7);
    }

    [Fact]
    public async Task FinallyAsync_Should_BindTaskHandler_When_ValueTaskSourceGetsUntypedAsyncBlockLambda()
    {
        ValueTask<Result> source = new(Result.Success());
        var gate = new TaskCompletionSource();

        ValueTask<Result> pending = source.FinallyAsync(async _ => await gate.Task);
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();

        ((await pending).IsSuccess, completedBeforeGate).Should().Be((true, false));
    }

    [Fact]
    public async Task SwitchFirstAsync_Should_AwaitLambdasReturningValueTask_When_ValueTaskSourceOfResult()
    {
        ValueTask<Result> source = new(Result.Success());
        var gate = new TaskCompletionSource();

        // 6.5.0: the ValueTask twin beats the Action pair, so the returned ValueTask is awaited instead of discarded.
        ValueTask pending = source.SwitchFirstAsync(() => new ValueTask(gate.Task), _ => new ValueTask());
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();
        await pending;

        completedBeforeGate.Should().BeFalse();
    }

    [Fact]
    public async Task SwitchLastAsync_Should_AwaitLambdasReturningValueTask_When_ValueTaskSourceOfResultT()
    {
        ValueTask<Result<int>> source = new(Result<int>.Failure(TestError));
        var gate = new TaskCompletionSource();

        ValueTask pending = source.SwitchLastAsync(_ => new ValueTask(), _ => new ValueTask(gate.Task));
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();
        await pending;

        completedBeforeGate.Should().BeFalse();
    }

    [Fact]
    public async Task FinallyAsync_Should_AwaitLambdaReturningValueTask_When_ValueTaskSourceOfResult()
    {
        ValueTask<Result> source = new(Result.Success());
        var gate = new TaskCompletionSource();

        // 6.5.0: Func<Result, ValueTask> beats Func<Result, TOut>, so the result is ValueTask<Result>, not ValueTask<ValueTask>.
        ValueTask<Result> pending = source.FinallyAsync(_ => new ValueTask(gate.Task));
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult();

        ((await pending).IsSuccess, completedBeforeGate).Should().Be((true, false));
    }

    [Fact]
    public async Task FinallyAsync_Should_ReturnValueTaskOfTOut_When_ValueTaskSourceOfResultTGetsLambdaReturningValueTaskOfT()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));

        // 6.5.0: Func<Result<T>, ValueTask<TOut>> beats Func<Result<T>, TOut>, which used to give ValueTask<ValueTask<int>>.
        ValueTask<int> pending = source.FinallyAsync(r => ValueTask.FromResult(r.Value * 2));

        (await pending).Should().Be(10);
    }

    [Fact]
    public async Task FinallyAsync_Should_ReturnValueTaskOfTOut_When_ValueTaskSourceGetsAsyncLambdaReturningValue()
    {
        ValueTask<Result> source = new(Result.Success());

        // 6.5.0: the async lambda binds Func<Result, ValueTask<TOut>>; it used to bind Func<Result, TOut> and give ValueTask<Task<int>>.
        ValueTask<int> pending = source.FinallyAsync(async r =>
        {
            await Task.Yield();
            return r.IsSuccess ? 1 : 0;
        });

        (await pending).Should().Be(1);
    }

    [Fact]
    public async Task FinallyAsync_Should_AwaitTaskOfT_When_ValueTaskSourceGetsNonAsyncLambdaReturningTaskOfT()
    {
        ValueTask<Result> source = new(Result.Success());
        var gate = new TaskCompletionSource<int>();

        // 6.5.0 on .NET 9+: the prioritised Func<Result, Task> handler wins over Func<Result, TOut>, so the task is awaited
        // and the result is ValueTask<Result>. It used to bind Func<Result, TOut> and give ValueTask<Task<int>>.
        ValueTask<Result> pending = source.FinallyAsync(_ => gate.Task);
        bool completedBeforeGate = pending.IsCompleted;
        gate.SetResult(1);

        ((await pending).IsSuccess, completedBeforeGate).Should().Be((true, false));
    }
#endif
}
