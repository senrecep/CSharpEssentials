using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

/// <summary>
/// Behaviour of every cell added by the flavour-matched async matrix: success, failure and cancellation per overload.
/// Rows are keyed by name so each overload shows up as its own test case.
/// </summary>
public sealed class AsyncMatrixTests
{
    private static readonly Error First = Error.Failure("E1", "First error");
    private static readonly Error Second = Error.Failure("E2", "Second error");
    private static readonly Error Rejected = Error.Validation("REJECTED", "Predicate rejected the value");

    private sealed class Probe
    {
        private readonly bool _succeed;
        private readonly bool _pending;

        public Probe(bool succeed, bool pending, CancellationToken token)
        {
            _succeed = succeed;
            _pending = pending;
            Token = token;
        }

        public CancellationToken Token { get; }

        public int Calls { get; private set; }

        public Result Plain => _succeed ? Result.Success() : new[] { First, Second };

        public Result<int> Value => _succeed ? 5 : new[] { First, Second };

        public Task<Result> TaskOfPlain() => Source(Plain);

        public ValueTask<Result> VtOfPlain() => SourceVt(Plain);

        public Task<Result<int>> TaskOfValue() => Source(Value);

        public ValueTask<Result<int>> VtOfValue() => SourceVt(Value);

        public Task Work()
        {
            Calls++;
            return _pending ? new TaskCompletionSource().Task : Task.CompletedTask;
        }

        public Task<T> Work<T>(T value)
        {
            Calls++;
            return Source(value);
        }

        public ValueTask WorkVt()
        {
            Calls++;
            return _pending ? new ValueTask(new TaskCompletionSource().Task) : default;
        }

        public ValueTask<T> WorkVt<T>(T value)
        {
            Calls++;
            return SourceVt(value);
        }

        public bool Check(int value)
        {
            Calls++;
            return value > 0;
        }

        public ValueTask<bool> CheckVt(int value)
        {
            Calls++;
            return SourceVt(value > 0);
        }

        private Task<T> Source<T>(T value) =>
            _pending ? new TaskCompletionSource<T>().Task : Task.FromResult(value);

        private ValueTask<T> SourceVt<T>(T value) =>
            _pending ? new ValueTask<T>(new TaskCompletionSource<T>().Task) : new ValueTask<T>(value);
    }

    private sealed record Cell(Func<Probe, Task<string>> Run, string OnSuccess, string OnFailure, bool ChecksTokenFirst);

    private static string Describe(Result result) => result.IsSuccess ? "S" : Fail(result.Errors);

    private static string Describe<T>(Result<T> result) => result.IsSuccess ? $"S:{result.Value}" : Fail(result.Errors);

    private static string Fail(Error[] errors) => "F:" + string.Join(",", errors.Select(e => e.Code));

    private static ValueTask<string> FailVt(Error[] errors) => new(Fail(errors));

    private static ValueTask<string> FailVt(Error error) => new(Fail([error]));

    private const string Both = "F:E1,E2";

    private static readonly Dictionary<string, Cell> Cells = BuildCells();

    private static Dictionary<string, Cell> BuildCells()
    {
        var cells = new Dictionary<string, Cell>(StringComparer.Ordinal)
        {
            // Map
            ["Result.MapAsync(Func<Task<TOut>>)"] = new(
                async p => Describe(await p.Plain.MapAsync(() => p.Work(7), p.Token)), "S:7", Both, true),
            ["ValueTask<Result>.MapAsync(Func<ValueTask<TOut>>)"] = new(
                async p => Describe(await p.VtOfPlain().MapAsync(() => p.WorkVt(7), p.Token)), "S:7", Both, true),
            ["Result<T>.MapAsync(Func<T, Task<TOut>>)"] = new(
                async p => Describe(await p.Value.MapAsync(v => p.Work(v * 2), p.Token)), "S:10", Both, true),
            ["ValueTask<Result<T>>.MapAsync(Func<T, ValueTask<TOut>>)"] = new(
                async p => Describe(await p.VtOfValue().MapAsync(v => p.WorkVt(v * 2), p.Token)), "S:10", Both, true),

            // Bind
            ["Task<Result>.BindAsync(Func<Task<Result<TOut>>>)"] = new(
                async p => Describe(await p.TaskOfPlain().BindAsync(() => p.Work<Result<int>>(7), p.Token)), "S:7", Both, false),

            // Tap
            ["Result.TapAsync(Func<Task>)"] = new(
                async p => Describe(await p.Plain.TapAsync(() => p.Work(), p.Token)), "S", Both, true),
            ["Task<Result>.TapAsync(Func<Task>)"] = new(
                async p => Describe(await p.TaskOfPlain().TapAsync(() => p.Work(), p.Token)), "S", Both, true),
            ["ValueTask<Result>.TapAsync(Func<ValueTask>)"] = new(
                async p => Describe(await p.VtOfPlain().TapAsync(() => p.WorkVt(), p.Token)), "S", Both, true),
            ["Result<T>.TapAsync(Func<T, Task>)"] = new(
                async p => Describe(await p.Value.TapAsync(_ => p.Work(), p.Token)), "S:5", Both, true),
            ["Task<Result<T>>.TapAsync(Func<T, Task>)"] = new(
                async p => Describe(await p.TaskOfValue().TapAsync(_ => p.Work(), p.Token)), "S:5", Both, true),
            ["Task<Result<T>>.TapAsync(Func<Task>)"] = new(
                async p => Describe(await p.TaskOfValue().TapAsync(() => p.Work(), p.Token)), "S:5", Both, true),
            ["ValueTask<Result<T>>.TapAsync(Func<T, ValueTask>)"] = new(
                async p => Describe(await p.VtOfValue().TapAsync(_ => p.WorkVt(), p.Token)), "S:5", Both, true),
            ["ValueTask<Result<T>>.TapAsync(Func<ValueTask>)"] = new(
                async p => Describe(await p.VtOfValue().TapAsync(() => p.WorkVt(), p.Token)), "S:5", Both, true),

            // Conditional Tap
            ["Task<Result>.TapAsync(bool, Func<Task>)"] = new(
                async p => Describe(await p.TaskOfPlain().TapAsync(true, () => p.Work(), p.Token)), "S", Both, true),
            ["Task<Result>.TapAsync(Func<bool>, Func<Task>)"] = new(
                async p => Describe(await p.TaskOfPlain().TapAsync(() => true, () => p.Work(), p.Token)), "S", Both, true),
            ["ValueTask<Result>.TapAsync(bool, Func<ValueTask>)"] = new(
                async p => Describe(await p.VtOfPlain().TapAsync(true, () => p.WorkVt(), p.Token)), "S", Both, true),
            ["ValueTask<Result>.TapAsync(Func<bool>, Func<ValueTask>)"] = new(
                async p => Describe(await p.VtOfPlain().TapAsync(() => true, () => p.WorkVt(), p.Token)), "S", Both, true),
            ["Task<Result<T>>.TapAsync(bool, Func<T, Task>)"] = new(
                async p => Describe(await p.TaskOfValue().TapAsync(true, _ => p.Work(), p.Token)), "S:5", Both, true),
            ["Task<Result<T>>.TapAsync(Func<bool>, Func<T, Task>)"] = new(
                async p => Describe(await p.TaskOfValue().TapAsync(() => true, _ => p.Work(), p.Token)), "S:5", Both, true),
            ["ValueTask<Result<T>>.TapAsync(bool, Func<T, ValueTask>)"] = new(
                async p => Describe(await p.VtOfValue().TapAsync(true, _ => p.WorkVt(), p.Token)), "S:5", Both, true),
            ["ValueTask<Result<T>>.TapAsync(Func<bool>, Func<T, ValueTask>)"] = new(
                async p => Describe(await p.VtOfValue().TapAsync(() => true, _ => p.WorkVt(), p.Token)), "S:5", Both, true),

            // Ensure with a sync predicate
            ["Task<Result<T>>.EnsureAsync(Func<T, bool>, Error)"] = new(
                async p => Describe(await p.TaskOfValue().EnsureAsync(v => p.Check(v), Rejected, p.Token)), "S:5", Both, false),
            ["Task<Result<T>>.EnsureAsync(Func<T, bool>, Func<T, Error>)"] = new(
                async p => Describe(await p.TaskOfValue().EnsureAsync(v => p.Check(v), _ => Rejected, p.Token)), "S:5", Both, false),
            ["ValueTask<Result<T>>.EnsureAsync(Func<T, bool>, Error)"] = new(
                async p => Describe(await p.VtOfValue().EnsureAsync(v => p.Check(v), Rejected, p.Token)), "S:5", Both, false),
            ["ValueTask<Result<T>>.EnsureAsync(Func<T, bool>, Func<T, Error>)"] = new(
                async p => Describe(await p.VtOfValue().EnsureAsync(v => p.Check(v), _ => Rejected, p.Token)), "S:5", Both, false),
        };

#if NET9_0_OR_GREATER
        // Instance ValueTask variants of the new Map/Tap pairs
        cells["Result.MapAsync(Func<ValueTask<TOut>>)"] = new(
            async p => Describe(await p.Plain.MapAsync(() => p.WorkVt(7), p.Token)), "S:7", Both, true);
        cells["Result<T>.MapAsync(Func<T, ValueTask<TOut>>)"] = new(
            async p => Describe(await p.Value.MapAsync(v => p.WorkVt(v * 2), p.Token)), "S:10", Both, true);
        cells["Result.TapAsync(Func<ValueTask>)"] = new(
            async p => Describe(await p.Plain.TapAsync(() => p.WorkVt(), p.Token)), "S", Both, true);
        cells["Result<T>.TapAsync(Func<T, ValueTask>)"] = new(
            async p => Describe(await p.Value.TapAsync(_ => p.WorkVt(), p.Token)), "S:5", Both, true);

        // Instance ValueTask twins on Result
        cells["Result.MatchAsync(Func<ValueTask<T>>, Func<Error[], ValueTask<T>>)"] = new(
            p => p.Plain.MatchAsync(() => p.WorkVt("S"), FailVt, p.Token).AsTask(), "S", Both, false);
        cells["Result.MatchFirstAsync(Func<ValueTask<T>>, Func<Error, ValueTask<T>>)"] = new(
            p => p.Plain.MatchFirstAsync(() => p.WorkVt("S"), FailVt, p.Token).AsTask(), "S", "F:E1", false);
        cells["Result.MatchLastAsync(Func<ValueTask<T>>, Func<Error, ValueTask<T>>)"] = new(
            p => p.Plain.MatchLastAsync(() => p.WorkVt("S"), FailVt, p.Token).AsTask(), "S", "F:E2", false);
        cells["Result.SwitchAsync(Func<ValueTask>, Func<Error[], ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.Plain.SwitchAsync(
                    () => { outcome = "S"; return p.WorkVt(); },
                    errors => { outcome = Fail(errors); return ValueTask.CompletedTask; },
                    p.Token);
                return outcome;
            }, "S", Both, false);
        cells["Result.ThenAsync(Func<ValueTask<Result>>)"] = new(
            async p => Describe(await p.Plain.ThenAsync(() => p.WorkVt(Result.Success()), p.Token)), "S", Both, false);
        cells["Result.ThenDoAsync(Func<ValueTask>)"] = new(
            async p => Describe(await p.Plain.ThenDoAsync(() => p.WorkVt(), p.Token)), "S", Both, false);

        // Instance ValueTask twins on Result<T>
        cells["Result<T>.MatchAsync(Func<T, ValueTask<T>>, Func<Error[], ValueTask<T>>)"] = new(
            p => p.Value.MatchAsync(v => p.WorkVt($"S:{v}"), FailVt, p.Token).AsTask(), "S:5", Both, false);
        cells["Result<T>.MatchFirstAsync(Func<T, ValueTask<T>>, Func<Error, ValueTask<T>>)"] = new(
            p => p.Value.MatchFirstAsync(v => p.WorkVt($"S:{v}"), FailVt, p.Token).AsTask(), "S:5", "F:E1", false);
        cells["Result<T>.MatchLastAsync(Func<T, ValueTask<T>>, Func<Error, ValueTask<T>>)"] = new(
            p => p.Value.MatchLastAsync(v => p.WorkVt($"S:{v}"), FailVt, p.Token).AsTask(), "S:5", "F:E2", false);
        cells["Result<T>.SwitchAsync(Func<T, ValueTask>, Func<Error[], ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.Value.SwitchAsync(
                    v => { outcome = $"S:{v}"; return p.WorkVt(); },
                    errors => { outcome = Fail(errors); return ValueTask.CompletedTask; },
                    p.Token);
                return outcome;
            }, "S:5", Both, false);
        cells["Result<T>.ThenAsync(Func<T, ValueTask<Result<T>>>)"] = new(
            async p => Describe(await p.Value.ThenAsync(v => p.WorkVt<Result<int>>(v * 2), p.Token)), "S:10", Both, false);
        cells["Result<T>.ThenAsync(Func<T, ValueTask<T>>)"] = new(
            async p => Describe(await p.Value.ThenAsync(v => p.WorkVt(v * 2), p.Token)), "S:10", Both, false);
        cells["Result<T>.ThenDoAsync(Func<T, ValueTask>)"] = new(
            async p => Describe(await p.Value.ThenDoAsync(_ => p.WorkVt(), p.Token)), "S:5", Both, false);
        cells["Result<T>.EnsureAsync(Func<T, ValueTask<bool>>, Error)"] = new(
            async p => Describe(await p.Value.EnsureAsync(v => p.CheckVt(v), Rejected, p.Token)), "S:5", Both, false);
        cells["Result<T>.EnsureAsync(Func<T, ValueTask<bool>>, Func<T, Error>)"] = new(
            async p => Describe(await p.Value.EnsureAsync(v => p.CheckVt(v), _ => Rejected, p.Token)), "S:5", Both, false);
        cells["Result<T>.TapIfAsync(bool, Func<T, ValueTask>)"] = new(
            async p => Describe(await p.Value.TapIfAsync(true, _ => p.WorkVt(), p.Token)), "S:5", Both, false);
        cells["Result<T>.TapIfAsync(Func<T, bool>, Func<T, ValueTask>)"] = new(
            async p => Describe(await p.Value.TapIfAsync(v => v > 0, _ => p.WorkVt(), p.Token)), "S:5", Both, false);

        // ValueTask-source twins on ValueTask<Result>
        cells["ValueTask<Result>.MatchAsync(Func<ValueTask<T>>, Func<Error[], ValueTask<T>>)"] = new(
            p => p.VtOfPlain().MatchAsync(() => p.WorkVt("S"), FailVt, p.Token).AsTask(), "S", Both, false);
        cells["ValueTask<Result>.MatchFirstAsync(Func<ValueTask<T>>, Func<Error, ValueTask<T>>)"] = new(
            p => p.VtOfPlain().MatchFirstAsync(() => p.WorkVt("S"), FailVt, p.Token).AsTask(), "S", "F:E1", false);
        cells["ValueTask<Result>.MatchLastAsync(Func<ValueTask<T>>, Func<Error, ValueTask<T>>)"] = new(
            p => p.VtOfPlain().MatchLastAsync(() => p.WorkVt("S"), FailVt, p.Token).AsTask(), "S", "F:E2", false);
        cells["ValueTask<Result>.SwitchAsync(Func<ValueTask>, Func<Error[], ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.VtOfPlain().SwitchAsync(
                    () => { outcome = "S"; return p.WorkVt(); },
                    errors => { outcome = Fail(errors); return ValueTask.CompletedTask; },
                    p.Token);
                return outcome;
            }, "S", Both, false);
        cells["ValueTask<Result>.ThenAsync(Func<ValueTask<Result>>)"] = new(
            async p => Describe(await p.VtOfPlain().ThenAsync(() => p.WorkVt(Result.Success()), p.Token)), "S", Both, false);
        cells["ValueTask<Result>.ThenDoAsync(Func<ValueTask>)"] = new(
            async p => Describe(await p.VtOfPlain().ThenDoAsync(() => p.WorkVt(), p.Token)), "S", Both, false);

        // ValueTask-source twins on ValueTask<Result<T>>
        cells["ValueTask<Result<T>>.MatchAsync(Func<T, ValueTask<T>>, Func<Error[], ValueTask<T>>)"] = new(
            p => p.VtOfValue().MatchAsync(v => p.WorkVt($"S:{v}"), FailVt, p.Token).AsTask(), "S:5", Both, false);
        cells["ValueTask<Result<T>>.MatchFirstAsync(Func<T, ValueTask<T>>, Func<Error, ValueTask<T>>)"] = new(
            p => p.VtOfValue().MatchFirstAsync(v => p.WorkVt($"S:{v}"), FailVt, p.Token).AsTask(), "S:5", "F:E1", false);
        cells["ValueTask<Result<T>>.MatchLastAsync(Func<T, ValueTask<T>>, Func<Error, ValueTask<T>>)"] = new(
            p => p.VtOfValue().MatchLastAsync(v => p.WorkVt($"S:{v}"), FailVt, p.Token).AsTask(), "S:5", "F:E2", false);
        cells["ValueTask<Result<T>>.SwitchAsync(Func<T, ValueTask>, Func<Error[], ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.VtOfValue().SwitchAsync(
                    v => { outcome = $"S:{v}"; return p.WorkVt(); },
                    errors => { outcome = Fail(errors); return ValueTask.CompletedTask; },
                    p.Token);
                return outcome;
            }, "S:5", Both, false);
        cells["ValueTask<Result<T>>.ThenAsync(Func<T, ValueTask<Result<T>>>)"] = new(
            async p => Describe(await p.VtOfValue().ThenAsync(v => p.WorkVt<Result<int>>(v * 2), p.Token)), "S:10", Both, false);
        cells["ValueTask<Result<T>>.ThenAsync(Func<T, ValueTask<T>>)"] = new(
            async p => Describe(await p.VtOfValue().ThenAsync(v => p.WorkVt(v * 2), p.Token)), "S:10", Both, false);
        cells["ValueTask<Result<T>>.ThenDoAsync(Func<T, ValueTask>)"] = new(
            async p => Describe(await p.VtOfValue().ThenDoAsync(_ => p.WorkVt(), p.Token)), "S:5", Both, false);
        cells["ValueTask<Result<T>>.EnsureAsync(Func<T, ValueTask<bool>>, Error)"] = new(
            async p => Describe(await p.VtOfValue().EnsureAsync(v => p.CheckVt(v), Rejected, p.Token)), "S:5", Both, false);
        cells["ValueTask<Result<T>>.EnsureAsync(Func<T, ValueTask<bool>>, Func<T, Error>)"] = new(
            async p => Describe(await p.VtOfValue().EnsureAsync(v => p.CheckVt(v), _ => Rejected, p.Token)), "S:5", Both, false);
        cells["ValueTask<Result<T>>.TapIfAsync(bool, Func<T, ValueTask>)"] = new(
            async p => Describe(await p.VtOfValue().TapIfAsync(true, _ => p.WorkVt(), p.Token)), "S:5", Both, false);
        cells["ValueTask<Result<T>>.TapIfAsync(Func<T, bool>, Func<T, ValueTask>)"] = new(
            async p => Describe(await p.VtOfValue().TapIfAsync(v => v > 0, _ => p.WorkVt(), p.Token)), "S:5", Both, false);
#endif

        return cells;
    }

    public static TheoryData<string> AllCells()
    {
        var data = new TheoryData<string>();
        foreach (string name in Cells.Keys)
            data.Add(name);
        return data;
    }

    public static TheoryData<string> TokenFirstCells()
    {
        var data = new TheoryData<string>();
        foreach (KeyValuePair<string, Cell> cell in Cells)
        {
            if (cell.Value.ChecksTokenFirst)
                data.Add(cell.Key);
        }
        return data;
    }

    public static TheoryData<string> MirroredCells()
    {
        var data = new TheoryData<string>();
        foreach (KeyValuePair<string, Cell> cell in Cells)
        {
            if (!cell.Value.ChecksTokenFirst)
                data.Add(cell.Key);
        }
        return data;
    }

    [Fact]
    public void Cells_Should_CoverEveryNewOverload_When_MatrixIsBuilt()
    {
        int expected = 25;
#if NET9_0_OR_GREATER
        expected += 38;
#endif

        Cells.Should().HaveCount(expected);
    }

    [Theory]
    [MemberData(nameof(AllCells))]
    public async Task Cell_Should_InvokeHandlerOnce_When_SourceSucceeds(string name)
    {
        Cell cell = Cells[name];
        var probe = new Probe(succeed: true, pending: false, CancellationToken.None);

        string outcome = await cell.Run(probe);

        (outcome, probe.Calls).Should().Be((cell.OnSuccess, 1));
    }

    [Theory]
    [MemberData(nameof(AllCells))]
    public async Task Cell_Should_SkipHandlerAndKeepErrorOrder_When_SourceFails(string name)
    {
        Cell cell = Cells[name];
        var probe = new Probe(succeed: false, pending: false, CancellationToken.None);

        string outcome = await cell.Run(probe);

        (outcome, probe.Calls).Should().Be((cell.OnFailure, 0));
    }

    [Theory]
    [MemberData(nameof(AllCells))]
    public async Task Cell_Should_ThrowOperationCanceled_When_TokenIsCancelledWhileAwaiting(string name)
    {
        Cell cell = Cells[name];
        using var cts = new CancellationTokenSource();
        var probe = new Probe(succeed: true, pending: true, cts.Token);

        Task<string> running = cell.Run(probe);
        running.IsCompleted.Should().BeFalse();
        await cts.CancelAsync();

        Func<Task> act = () => running;

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [MemberData(nameof(TokenFirstCells))]
    public async Task Cell_Should_ThrowBeforeHandler_When_TokenIsAlreadyCancelled(string name)
    {
        Cell cell = Cells[name];
        var probe = new Probe(succeed: true, pending: false, new CancellationToken(canceled: true));

        Func<Task> act = () => cell.Run(probe);

        await act.Should().ThrowAsync<OperationCanceledException>();
        probe.Calls.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(MirroredCells))]
    public async Task Cell_Should_MirrorTaskSibling_When_TokenIsAlreadyCancelledAndHandlerCompletesSynchronously(string name)
    {
        Cell cell = Cells[name];
        var probe = new Probe(succeed: true, pending: false, new CancellationToken(canceled: true));

        string outcome = await cell.Run(probe);

        (outcome, probe.Calls).Should().Be((cell.OnSuccess, 1));
    }

    [Fact]
    public async Task EnsureAsync_Should_ReturnError_When_TaskSourceSyncPredicateRejects()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(-1);

        Result<int> result = await source.EnsureAsync(v => v > 0, Rejected);

        result.FirstError.Should().Be(Rejected);
    }

    [Fact]
    public async Task EnsureAsync_Should_UseErrorFactory_When_ValueTaskSourceSyncPredicateRejects()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(-1));

        Result<int> result = await source.EnsureAsync(v => v > 0, v => Error.Validation("NEG", $"{v} is negative"));

        result.FirstError.Code.Should().Be("NEG");
    }

    [Fact]
    public async Task TapAsync_Should_PropagateHandlerException_When_TaskSourceHandlerThrows()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(5);

        Func<Task> act = async () => await source.TapAsync(_ => Task.FromException(new InvalidOperationException("boom")));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_PropagateAsyncHandlerException_When_TaskSourceOfResult()
    {
        Task<Result> source = Task.FromResult(Result.Success());

        Func<Task> act = async () => await source.TapAsync(true, async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_PropagateAsyncHandlerException_When_ValueTaskSourceOfResult()
    {
        ValueTask<Result> source = new(Result.Success());

        Func<Task> act = async () => await source.TapAsync(() => true, async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_PropagateAsyncHandlerException_When_TaskSourceOfResultT()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(5);

        Func<Task> act = async () => await source.TapAsync(() => true, async _ =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_PropagateAsyncHandlerException_When_ValueTaskSourceOfResultT()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));

        Func<Task> act = async () => await source.TapAsync(true, async _ =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_SkipHandler_When_TaskSourceConditionIsFalse()
    {
        Task<Result> source = Task.FromResult(Result.Success());
        int calls = 0;

        Result result = await source.TapAsync(false, () => { calls++; return Task.CompletedTask; });

        (result.IsSuccess, calls).Should().Be((true, 0));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_SkipHandler_When_ValueTaskSourceFuncConditionIsFalse()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));
        int calls = 0;

        Result<int> result = await source.TapAsync(() => false, _ => { calls++; return default(ValueTask); });

        (result.Value, calls).Should().Be((5, 0));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_NotEvaluateCondition_When_TaskSourceOfResultTFails()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(First);
        int conditionCalls = 0;
        int handlerCalls = 0;

        Result<int> result = await source.TapAsync(
            () => { conditionCalls++; return true; },
            _ => { handlerCalls++; return Task.CompletedTask; });

        (result.IsFailure, conditionCalls, handlerCalls).Should().Be((true, 0, 0));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_NotEvaluateCondition_When_TaskSourceOfResultFails()
    {
        Task<Result> source = Task.FromResult(Result.Failure(First));
        int conditionCalls = 0;
        int handlerCalls = 0;

        Result result = await source.TapAsync(
            () => { conditionCalls++; return true; },
            () => { handlerCalls++; return Task.CompletedTask; });

        (result.IsFailure, conditionCalls, handlerCalls).Should().Be((true, 0, 0));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_NotEvaluateCondition_When_ValueTaskSourceOfResultFails()
    {
        ValueTask<Result> source = new(Result.Failure(First));
        int conditionCalls = 0;
        int handlerCalls = 0;

        Result result = await source.TapAsync(
            () => { conditionCalls++; return true; },
            () => { handlerCalls++; return default(ValueTask); });

        (result.IsFailure, conditionCalls, handlerCalls).Should().Be((true, 0, 0));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_NotEvaluateCondition_When_ValueTaskSourceOfResultTFails()
    {
        ValueTask<Result<int>> source = new(Result<int>.Failure(First));
        int conditionCalls = 0;
        int handlerCalls = 0;

        Result<int> result = await source.TapAsync(
            () => { conditionCalls++; return true; },
            _ => { handlerCalls++; return default(ValueTask); });

        (result.IsFailure, conditionCalls, handlerCalls).Should().Be((true, 0, 0));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_ThrowOperationCanceled_When_TokenIsCancelledWhileAwaitingHandlerOnTaskSourceOfResult()
    {
        Task<Result> source = Task.FromResult(Result.Success());
        var handler = new TaskCompletionSource();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        Func<Task> act = async () => await source.TapAsync(true, () => handler.Task, cts.Token);

        await act.Should().ThrowWithinAsync<OperationCanceledException>(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_ThrowOperationCanceled_When_TokenIsCancelledWhileAwaitingHandlerOnValueTaskSourceOfResult()
    {
        ValueTask<Result> source = new(Result.Success());
        var handler = new TaskCompletionSource();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        Func<Task> act = async () => await source.TapAsync(() => true, () => new ValueTask(handler.Task), cts.Token);

        await act.Should().ThrowWithinAsync<OperationCanceledException>(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_ThrowOperationCanceled_When_TokenIsCancelledWhileAwaitingHandlerOnTaskSourceOfResultT()
    {
        Task<Result<int>> source = Task.FromResult<Result<int>>(5);
        var handler = new TaskCompletionSource();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        Func<Task> act = async () => await source.TapAsync(() => true, _ => handler.Task, cts.Token);

        await act.Should().ThrowWithinAsync<OperationCanceledException>(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ConditionalTapAsync_Should_ThrowOperationCanceled_When_TokenIsCancelledWhileAwaitingHandlerOnValueTaskSourceOfResultT()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(5));
        var handler = new TaskCompletionSource();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        Func<Task> act = async () => await source.TapAsync(true, _ => new ValueTask(handler.Task), cts.Token);

        await act.Should().ThrowWithinAsync<OperationCanceledException>(TimeSpan.FromSeconds(5));
    }

#if NET9_0_OR_GREATER
    [Fact]
    public async Task EnsureAsync_Should_ReturnError_When_ValueTaskPredicateRejects()
    {
        Result<int> source = -1;

        Result<int> result = await source.EnsureAsync(v => new ValueTask<bool>(v > 0), Rejected);

        result.FirstError.Should().Be(Rejected);
    }

    [Fact]
    public async Task TapIfAsync_Should_SkipHandler_When_ValueTaskPredicateIsFalse()
    {
        ValueTask<Result<int>> source = new(Result<int>.Success(-1));
        int calls = 0;

        Result<int> result = await source.TapIfAsync(v => v > 0, _ => { calls++; return ValueTask.CompletedTask; });

        (result.Value, calls).Should().Be((-1, 0));
    }
#endif
}
