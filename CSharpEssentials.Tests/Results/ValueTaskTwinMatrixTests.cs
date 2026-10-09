#if NET9_0_OR_GREATER
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

/// <summary>
/// Behaviour of the ValueTask twins added in 6.5.0 (issue #122): outcome and handler calls on a successful and a failed source,
/// and cancellation while the ValueTask handler is pending. Rows are keyed by name so each overload shows up as its own test case.
/// </summary>
public sealed class ValueTaskTwinMatrixTests
{
    private static readonly Error First = Error.Failure("E1", "First error");
    private static readonly Error Second = Error.Failure("E2", "Second error");
    private static readonly Error Rejected = Error.Validation("REJECTED", "Handler rejected the value");
    private static readonly Error Thrown = Error.Failure("THROWN", "Handler threw");

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

        public ValueTask<Result> VtOfPlain() => new(Plain);

        public ValueTask<Result<int>> VtOfValue() => new(Value);

        public ValueTask WorkVt()
        {
            Calls++;
            return _pending ? new ValueTask(new TaskCompletionSource().Task) : default;
        }

        public ValueTask<T> WorkVt<T>(T value)
        {
            Calls++;
            return _pending ? new ValueTask<T>(new TaskCompletionSource<T>().Task) : new ValueTask<T>(value);
        }

        public ValueTask AttemptVt()
        {
            Calls++;
            if (_pending)
                return new ValueTask(new TaskCompletionSource().Task);
            return _succeed ? default : ValueTask.FromException(new InvalidOperationException("boom"));
        }

        public ValueTask<T> AttemptVt<T>(T value)
        {
            Calls++;
            if (_pending)
                return new ValueTask<T>(new TaskCompletionSource<T>().Task);
            return _succeed ? new ValueTask<T>(value) : ValueTask.FromException<T>(new InvalidOperationException("boom"));
        }
    }

    private sealed record Cell(
        Func<Probe, Task<string>> Run,
        (string Outcome, int Calls) OnSuccess,
        (string Outcome, int Calls) OnFailure,
        bool? CancelOnSucceedingSource);

    private static string Describe(Result result) => result.IsSuccess ? "S" : Fail(result.Errors);

    private static string Describe<T>(Result<T> result) => result.IsSuccess ? $"S:{result.Value}" : Fail(result.Errors);

    private static string Fail(Error[] errors) => "F:" + string.Join(",", errors.Select(e => e.Code));

    private const string Both = "F:E1,E2";
    private const string Reversed = "F:E2,E1";
    private const string RejectedOutcome = "F:REJECTED";

    private static readonly Dictionary<string, Cell> Cells = BuildCells();

    private static Dictionary<string, Cell> BuildCells() => new(StringComparer.Ordinal)
    {
        // Result instance
        ["Result.ElseAsync(Func<Error[], ValueTask<Error>>)"] = new(
            async p => Describe(await p.Plain.ElseAsync(_ => p.WorkVt(Rejected), p.Token)), ("S", 0), (RejectedOutcome, 1), false),
        ["Result.ElseAsync(Func<Error[], ValueTask<IEnumerable<Error>>>)"] = new(
            async p => Describe(await p.Plain.ElseAsync(e => p.WorkVt<IEnumerable<Error>>(Enumerable.Reverse(e)), p.Token)), ("S", 0), (Reversed, 1), false),
        ["Result.ElseAsync(ValueTask<Error>)"] = new(
            async p => Describe(await p.Plain.ElseAsync(p.WorkVt(Rejected), p.Token)), ("S", 1), (RejectedOutcome, 1), false),
        ["Result.SwitchFirstAsync(Func<ValueTask>, Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.Plain.SwitchFirstAsync(
                    () => { outcome = "S"; return p.WorkVt(); },
                    e => { outcome = Fail([e]); return p.WorkVt(); },
                    p.Token);
                return outcome;
            }, ("S", 1), ("F:E1", 1), true),
        ["Result.SwitchLastAsync(Func<ValueTask>, Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.Plain.SwitchLastAsync(
                    () => { outcome = "S"; return p.WorkVt(); },
                    e => { outcome = Fail([e]); return p.WorkVt(); },
                    p.Token);
                return outcome;
            }, ("S", 1), ("F:E2", 1), false),
        ["Result.TapErrorAsync(Func<Error[], ValueTask>)"] = new(
            async p => Describe(await p.Plain.TapErrorAsync(_ => p.WorkVt(), p.Token)), ("S", 0), (Both, 1), false),
        ["Result.TapErrorFirstAsync(Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string seen = string.Empty;
                Result result = await p.Plain.TapErrorFirstAsync(e => { seen = e.Code; return p.WorkVt(); }, p.Token);
                return Describe(result) + "|" + seen;
            }, ("S|", 0), (Both + "|E1", 1), false),
        ["Result.ElseDoAsync(Func<Error[], ValueTask>)"] = new(
            async p => Describe(await p.Plain.ElseDoAsync(_ => p.WorkVt(), p.Token)), ("S", 0), (Both, 1), false),
        ["Result.ElseDoFirstAsync(Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string seen = string.Empty;
                Result result = await p.Plain.ElseDoFirstAsync(e => { seen = e.Code; return p.WorkVt(); }, p.Token);
                return Describe(result) + "|" + seen;
            }, ("S|", 0), (Both + "|E1", 1), false),
        ["Result.CompensateAsync(Func<Error[], ValueTask<Result>>)"] = new(
            async p => Describe(await p.Plain.CompensateAsync(_ => p.WorkVt(Result.Success()), p.Token)), ("S", 0), ("S", 1), false),
        ["Result.CompensateFirstAsync(Func<Error, ValueTask<Result>>)"] = new(
            async p => Describe(await p.Plain.CompensateFirstAsync(e => p.WorkVt<Result>(e), p.Token)), ("S", 0), ("F:E1", 1), false),
        ["Result.TryAsync(Func<ValueTask>)"] = new(
            async p => Describe(await Result.TryAsync(() => p.AttemptVt(), _ => Thrown, p.Token)), ("S", 1), ("F:THROWN", 1), true),
        ["Result.TryAsync(Func<ValueTask<T>>)"] = new(
            async p => Describe(await Result.TryAsync(() => p.AttemptVt(7), _ => Thrown, p.Token)), ("S:7", 1), ("F:THROWN", 1), true),
        ["Result.TryAsync(Func<ValueTask<Result<T>>>)"] = new(
            async p => Describe(await Result.TryAsync(() => p.AttemptVt(Result<int>.Success(7)), _ => Thrown, p.Token)), ("S:7", 1), ("F:THROWN", 1), true),
        ["Result.TryAsync(Func<ValueTask<Result>>)"] = new(
            async p => Describe(await Result.TryAsync(() => p.AttemptVt(Result.Success()), _ => Thrown, p.Token)), ("S", 1), ("F:THROWN", 1), true),

        // Result<T> instance
        ["Result<T>.ElseAsync(Func<Error[], ValueTask<T>>)"] = new(
            async p => Describe(await p.Value.ElseAsync(_ => p.WorkVt(42), p.Token)), ("S:5", 0), ("S:42", 1), false),
        ["Result<T>.ElseAsync(Func<Error[], ValueTask<Error>>)"] = new(
            async p => Describe(await p.Value.ElseAsync(_ => p.WorkVt(Rejected), p.Token)), ("S:5", 0), (RejectedOutcome, 1), false),
        ["Result<T>.ElseAsync(Func<Error[], ValueTask<Error[]>>)"] = new(
            async p => Describe(await p.Value.ElseAsync(e => p.WorkVt(Enumerable.Reverse(e).ToArray()), p.Token)), ("S:5", 0), (Reversed, 1), false),
        ["Result<T>.ElseAsync(ValueTask<Error>)"] = new(
            async p => Describe(await p.Value.ElseAsync(p.WorkVt(Rejected), p.Token)), ("S:5", 1), (RejectedOutcome, 1), false),
        ["Result<T>.ElseAsync(ValueTask<T>)"] = new(
            async p => Describe(await p.Value.ElseAsync(p.WorkVt(42), p.Token)), ("S:5", 1), ("S:42", 1), false),
        ["Result<T>.FailIfAsync(Func<T, ValueTask<bool>>, Error)"] = new(
            async p => Describe(await p.Value.FailIfAsync(v => p.WorkVt(v > 0), Rejected, p.Token)), (RejectedOutcome, 1), (Both, 0), true),
        ["Result<T>.FailIfAsync(Func<T, ValueTask<bool>>, Func<T, ValueTask<Error>>)"] = new(
            async p => Describe(await p.Value.FailIfAsync(v => p.WorkVt(v > 0), _ => p.WorkVt(Rejected), p.Token)), (RejectedOutcome, 2), (Both, 0), true),
        ["Result<T>.SwitchFirstAsync(Func<T, ValueTask>, Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.Value.SwitchFirstAsync(
                    v => { outcome = $"S:{v}"; return p.WorkVt(); },
                    e => { outcome = Fail([e]); return p.WorkVt(); },
                    p.Token);
                return outcome;
            }, ("S:5", 1), ("F:E1", 1), true),
        ["Result<T>.SwitchLastAsync(Func<T, ValueTask>, Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.Value.SwitchLastAsync(
                    v => { outcome = $"S:{v}"; return p.WorkVt(); },
                    e => { outcome = Fail([e]); return p.WorkVt(); },
                    p.Token);
                return outcome;
            }, ("S:5", 1), ("F:E2", 1), false),
        ["Result<T>.TapErrorAsync(Func<Error[], ValueTask>)"] = new(
            async p => Describe(await p.Value.TapErrorAsync(_ => p.WorkVt(), p.Token)), ("S:5", 0), (Both, 1), false),
        ["Result<T>.TapErrorFirstAsync(Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string seen = string.Empty;
                Result<int> result = await p.Value.TapErrorFirstAsync(e => { seen = e.Code; return p.WorkVt(); }, p.Token);
                return Describe(result) + "|" + seen;
            }, ("S:5|", 0), (Both + "|E1", 1), false),
        ["Result<T>.ElseDoAsync(Func<Error[], ValueTask>)"] = new(
            async p => Describe(await p.Value.ElseDoAsync(_ => p.WorkVt(), p.Token)), ("S:5", 0), (Both, 1), false),
        ["Result<T>.ElseDoFirstAsync(Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string seen = string.Empty;
                Result<int> result = await p.Value.ElseDoFirstAsync(e => { seen = e.Code; return p.WorkVt(); }, p.Token);
                return Describe(result) + "|" + seen;
            }, ("S:5|", 0), (Both + "|E1", 1), false),
        ["Result<T>.CompensateAsync(Func<Error[], ValueTask<Result<T>>>)"] = new(
            async p => Describe(await p.Value.CompensateAsync(_ => p.WorkVt(Result<int>.Success(42)), p.Token)), ("S:5", 0), ("S:42", 1), false),
        ["Result<T>.CompensateFirstAsync(Func<Error, ValueTask<Result<T>>>)"] = new(
            async p => Describe(await p.Value.CompensateFirstAsync(e => p.WorkVt<Result<int>>(e), p.Token)), ("S:5", 0), ("F:E1", 1), false),
        ["Result<T>.ThenEnsureAsync(Func<T, ValueTask<Result<T>>>)"] = new(
            async p => Describe(await p.Value.ThenEnsureAsync(v => p.WorkVt(Result<int>.Success(v * 2)), p.Token)), ("S:10", 1), (Both, 0), null),
        ["Result<T>.ThenEnsureAsync(Func<T, ValueTask<Result>>)"] = new(
            async p => Describe(await p.Value.ThenEnsureAsync(_ => p.WorkVt<Result>(Rejected), p.Token)), (RejectedOutcome, 1), (Both, 0), null),

        // ValueTask<Result> source
        ["ValueTask<Result>.ElseAsync(Func<Error[], ValueTask<Error>>)"] = new(
            async p => Describe(await p.VtOfPlain().ElseAsync(_ => p.WorkVt(Rejected), p.Token)), ("S", 0), (RejectedOutcome, 1), false),
        ["ValueTask<Result>.ElseAsync(Func<Error[], ValueTask<IEnumerable<Error>>>)"] = new(
            async p => Describe(await p.VtOfPlain().ElseAsync(e => p.WorkVt<IEnumerable<Error>>(Enumerable.Reverse(e)), p.Token)), ("S", 0), (Reversed, 1), false),
        ["ValueTask<Result>.ElseAsync(ValueTask<Error>)"] = new(
            async p => Describe(await p.VtOfPlain().ElseAsync(p.WorkVt(Rejected), p.Token)), ("S", 1), (RejectedOutcome, 1), false),
        ["ValueTask<Result>.SwitchFirstAsync(Func<ValueTask>, Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.VtOfPlain().SwitchFirstAsync(
                    () => { outcome = "S"; return p.WorkVt(); },
                    e => { outcome = Fail([e]); return p.WorkVt(); },
                    p.Token);
                return outcome;
            }, ("S", 1), ("F:E1", 1), false),
        ["ValueTask<Result>.SwitchLastAsync(Func<ValueTask>, Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.VtOfPlain().SwitchLastAsync(
                    () => { outcome = "S"; return p.WorkVt(); },
                    e => { outcome = Fail([e]); return p.WorkVt(); },
                    p.Token);
                return outcome;
            }, ("S", 1), ("F:E2", 1), true),
        ["ValueTask<Result>.TapErrorAsync(Func<Error[], ValueTask>)"] = new(
            async p => Describe(await p.VtOfPlain().TapErrorAsync(_ => p.WorkVt(), p.Token)), ("S", 0), (Both, 1), false),
        ["ValueTask<Result>.TapErrorFirstAsync(Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string seen = string.Empty;
                Result result = await p.VtOfPlain().TapErrorFirstAsync(e => { seen = e.Code; return p.WorkVt(); }, p.Token);
                return Describe(result) + "|" + seen;
            }, ("S|", 0), (Both + "|E1", 1), false),
        ["ValueTask<Result>.ElseDoAsync(Func<Error[], ValueTask>)"] = new(
            async p => Describe(await p.VtOfPlain().ElseDoAsync(_ => p.WorkVt(), p.Token)), ("S", 0), (Both, 1), false),
        ["ValueTask<Result>.ElseDoFirstAsync(Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string seen = string.Empty;
                Result result = await p.VtOfPlain().ElseDoFirstAsync(e => { seen = e.Code; return p.WorkVt(); }, p.Token);
                return Describe(result) + "|" + seen;
            }, ("S|", 0), (Both + "|E1", 1), false),
        ["ValueTask<Result>.CompensateAsync(Func<Error[], ValueTask<Result>>)"] = new(
            async p => Describe(await p.VtOfPlain().CompensateAsync(_ => p.WorkVt(Result.Success()), p.Token)), ("S", 0), ("S", 1), false),
        ["ValueTask<Result>.CompensateFirstAsync(Func<Error, ValueTask<Result>>)"] = new(
            async p => Describe(await p.VtOfPlain().CompensateFirstAsync(e => p.WorkVt<Result>(e), p.Token)), ("S", 0), ("F:E1", 1), false),
        ["ValueTask<Result>.FinallyAsync(Func<Result, ValueTask<TOut>>)"] = new(
            async p => await p.VtOfPlain().FinallyAsync(r => p.WorkVt(Describe(r)), p.Token), ("S", 1), (Both, 1), true),
        ["ValueTask<Result>.FinallyAsync(Func<Result, ValueTask>)"] = new(
            async p => Describe(await p.VtOfPlain().FinallyAsync(_ => p.WorkVt(), p.Token)), ("S", 1), (Both, 1), false),

        // ValueTask<Result<T>> source
        ["ValueTask<Result<T>>.ElseAsync(Func<Error[], ValueTask<T>>)"] = new(
            async p => Describe(await p.VtOfValue().ElseAsync(_ => p.WorkVt(42), p.Token)), ("S:5", 0), ("S:42", 1), false),
        ["ValueTask<Result<T>>.ElseAsync(Func<Error[], ValueTask<Error>>)"] = new(
            async p => Describe(await p.VtOfValue().ElseAsync(_ => p.WorkVt(Rejected), p.Token)), ("S:5", 0), (RejectedOutcome, 1), false),
        ["ValueTask<Result<T>>.ElseAsync(Func<Error[], ValueTask<Error[]>>)"] = new(
            async p => Describe(await p.VtOfValue().ElseAsync(e => p.WorkVt(Enumerable.Reverse(e).ToArray()), p.Token)), ("S:5", 0), (Reversed, 1), false),
        ["ValueTask<Result<T>>.ElseAsync(ValueTask<Error>)"] = new(
            async p => Describe(await p.VtOfValue().ElseAsync(p.WorkVt(Rejected), p.Token)), ("S:5", 1), (RejectedOutcome, 1), false),
        ["ValueTask<Result<T>>.ElseAsync(ValueTask<T>)"] = new(
            async p => Describe(await p.VtOfValue().ElseAsync(p.WorkVt(42), p.Token)), ("S:5", 1), ("S:42", 1), false),
        ["ValueTask<Result<T>>.FailIfAsync(Func<T, ValueTask<bool>>, Error)"] = new(
            async p => Describe(await p.VtOfValue().FailIfAsync(v => p.WorkVt(v > 0), Rejected, p.Token)), (RejectedOutcome, 1), (Both, 0), true),
        ["ValueTask<Result<T>>.FailIfAsync(Func<T, ValueTask<bool>>, Func<T, ValueTask<Error>>)"] = new(
            async p => Describe(await p.VtOfValue().FailIfAsync(v => p.WorkVt(v > 0), _ => p.WorkVt(Rejected), p.Token)), (RejectedOutcome, 2), (Both, 0), true),
        ["ValueTask<Result<T>>.SwitchFirstAsync(Func<T, ValueTask>, Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.VtOfValue().SwitchFirstAsync(
                    v => { outcome = $"S:{v}"; return p.WorkVt(); },
                    e => { outcome = Fail([e]); return p.WorkVt(); },
                    p.Token);
                return outcome;
            }, ("S:5", 1), ("F:E1", 1), true),
        ["ValueTask<Result<T>>.SwitchLastAsync(Func<T, ValueTask>, Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string outcome = string.Empty;
                await p.VtOfValue().SwitchLastAsync(
                    v => { outcome = $"S:{v}"; return p.WorkVt(); },
                    e => { outcome = Fail([e]); return p.WorkVt(); },
                    p.Token);
                return outcome;
            }, ("S:5", 1), ("F:E2", 1), false),
        ["ValueTask<Result<T>>.TapErrorAsync(Func<Error[], ValueTask>)"] = new(
            async p => Describe(await p.VtOfValue().TapErrorAsync(_ => p.WorkVt(), p.Token)), ("S:5", 0), (Both, 1), false),
        ["ValueTask<Result<T>>.TapErrorFirstAsync(Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string seen = string.Empty;
                Result<int> result = await p.VtOfValue().TapErrorFirstAsync(e => { seen = e.Code; return p.WorkVt(); }, p.Token);
                return Describe(result) + "|" + seen;
            }, ("S:5|", 0), (Both + "|E1", 1), false),
        ["ValueTask<Result<T>>.ElseDoAsync(Func<Error[], ValueTask>)"] = new(
            async p => Describe(await p.VtOfValue().ElseDoAsync(_ => p.WorkVt(), p.Token)), ("S:5", 0), (Both, 1), false),
        ["ValueTask<Result<T>>.ElseDoFirstAsync(Func<Error, ValueTask>)"] = new(
            async p =>
            {
                string seen = string.Empty;
                Result<int> result = await p.VtOfValue().ElseDoFirstAsync(e => { seen = e.Code; return p.WorkVt(); }, p.Token);
                return Describe(result) + "|" + seen;
            }, ("S:5|", 0), (Both + "|E1", 1), false),
        ["ValueTask<Result<T>>.CompensateAsync(Func<Error[], ValueTask<Result<T>>>)"] = new(
            async p => Describe(await p.VtOfValue().CompensateAsync(_ => p.WorkVt(Result<int>.Success(42)), p.Token)), ("S:5", 0), ("S:42", 1), false),
        ["ValueTask<Result<T>>.CompensateFirstAsync(Func<Error, ValueTask<Result<T>>>)"] = new(
            async p => Describe(await p.VtOfValue().CompensateFirstAsync(e => p.WorkVt<Result<int>>(e), p.Token)), ("S:5", 0), ("F:E1", 1), false),
        ["ValueTask<Result<T>>.ThenEnsureAsync(Func<T, ValueTask<Result<T>>>)"] = new(
            async p => Describe(await p.VtOfValue().ThenEnsureAsync(v => p.WorkVt(Result<int>.Success(v * 2)), p.Token)), ("S:10", 1), (Both, 0), null),
        ["ValueTask<Result<T>>.ThenEnsureAsync(Func<T, ValueTask<Result>>)"] = new(
            async p => Describe(await p.VtOfValue().ThenEnsureAsync(_ => p.WorkVt<Result>(Rejected), p.Token)), (RejectedOutcome, 1), (Both, 0), null),
        ["ValueTask<Result<T>>.FinallyAsync(Func<Result<T>, ValueTask<TOut>>)"] = new(
            async p => await p.VtOfValue().FinallyAsync(r => p.WorkVt(Describe(r)), p.Token), ("S:5", 1), (Both, 1), true),
        ["ValueTask<Result<T>>.FinallyAsync(Func<Result<T>, ValueTask>)"] = new(
            async p => Describe(await p.VtOfValue().FinallyAsync(_ => p.WorkVt(), p.Token)), ("S:5", 1), (Both, 1), false),
    };

    public static TheoryData<string> AllCells()
    {
        var data = new TheoryData<string>();
        foreach (string name in Cells.Keys)
            data.Add(name);
        return data;
    }

    public static TheoryData<string> CancellableCells()
    {
        var data = new TheoryData<string>();
        foreach (KeyValuePair<string, Cell> cell in Cells)
        {
            if (cell.Value.CancelOnSucceedingSource.HasValue)
                data.Add(cell.Key);
        }
        return data;
    }

    [Fact]
    public void Cells_Should_CoverEveryNewTwin_When_MatrixIsBuilt()
    {
        Cells.Should().HaveCount(64);
    }

    [Theory]
    [MemberData(nameof(AllCells))]
    public async Task Cell_Should_ReturnExpectedOutcome_When_SourceSucceeds(string name)
    {
        Cell cell = Cells[name];
        var probe = new Probe(succeed: true, pending: false, CancellationToken.None);

        string outcome = await cell.Run(probe);

        (outcome, probe.Calls).Should().Be(cell.OnSuccess);
    }

    [Theory]
    [MemberData(nameof(AllCells))]
    public async Task Cell_Should_ReturnExpectedOutcome_When_SourceFails(string name)
    {
        Cell cell = Cells[name];
        var probe = new Probe(succeed: false, pending: false, CancellationToken.None);

        string outcome = await cell.Run(probe);

        (outcome, probe.Calls).Should().Be(cell.OnFailure);
    }

    [Theory]
    [MemberData(nameof(CancellableCells))]
    public async Task Cell_Should_ThrowOperationCanceled_When_TokenIsCancelledWhileHandlerIsPending(string name)
    {
        Cell cell = Cells[name];
        using var cts = new CancellationTokenSource();
        var probe = new Probe(cell.CancelOnSucceedingSource!.Value, pending: true, cts.Token);

        Task<string> running = cell.Run(probe);
        bool completedBeforeCancel = running.IsCompleted;
        await cts.CancelAsync();

        Func<Task> act = () => running;

        await act.Should().ThrowAsync<OperationCanceledException>();
        completedBeforeCancel.Should().BeFalse();
    }

    [Fact]
    public async Task ThenEnsureAsync_Should_ThrowBeforeValidator_When_TokenIsAlreadyCancelled()
    {
        var probe = new Probe(succeed: true, pending: false, new CancellationToken(canceled: true));

        Func<Task> act = async () => await probe.Value.ThenEnsureAsync(v => probe.WorkVt(Result<int>.Success(v)), probe.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        probe.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ThenEnsureAsync_Should_ThrowBeforeValidator_When_ValueTaskSourceTokenIsAlreadyCancelled()
    {
        var probe = new Probe(succeed: true, pending: false, new CancellationToken(canceled: true));

        Func<Task> act = async () => await probe.VtOfValue().ThenEnsureAsync(_ => probe.WorkVt(Result.Success()), probe.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        probe.Calls.Should().Be(0);
    }

    [Fact]
    public async Task TryAsync_Should_PropagateCancellation_When_HandlerThrowsForCancelledToken()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await Result.TryAsync(
            () => ValueTask.FromCanceled(cts.Token), _ => Thrown, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryAsync_Should_ReturnHandlerError_When_ValueTaskOfResultHandlerThrows()
    {
        Result<int> result = await Result.TryAsync(
            () => ValueTask.FromException<Result<int>>(new InvalidOperationException("boom")),
            ex => Error.Failure("CAUGHT", ex.Message));

        (result.FirstError.Code, result.FirstError.Description).Should().Be(("CAUGHT", "boom"));
    }
}
#endif
