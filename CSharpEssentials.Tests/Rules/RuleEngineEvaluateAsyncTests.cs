using System.Collections.Concurrent;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public sealed class RuleEngineEvaluateAsyncTests
{
    public sealed class Ctx
    {
        public List<string> Log { get; } = [];
    }

    #region Rule kinds

    private sealed class Leaf(string name, bool success, int value = 0, bool throws = false) : IRule<Ctx>, IRule<Ctx, int>
    {
        Result IRule<Ctx>.Evaluate(Ctx context, CancellationToken cancellationToken)
        {
            context.Log.Add(name);
            if (throws)
                throw new InvalidOperationException(name);
            return success ? Result.Success() : Error.Validation(name, name);
        }

        Result<int> IRule<Ctx, int>.Evaluate(Ctx context, CancellationToken cancellationToken)
        {
            context.Log.Add(name);
            if (throws)
                throw new InvalidOperationException(name);
            return success ? value : Error.Validation(name, name);
        }
    }

    private sealed class AsyncLeaf(string name, bool success, int value = 0, bool throws = false) : IAsyncRule<Ctx>, IAsyncRule<Ctx, int>
    {
        async ValueTask<Result> IAsyncRule<Ctx>.EvaluateAsync(Ctx context, CancellationToken cancellationToken)
        {
            await Task.Yield();
            context.Log.Add(name);
            if (throws)
                throw new InvalidOperationException(name);
            return success ? Result.Success() : Error.Validation(name, name);
        }

        async ValueTask<Result<int>> IAsyncRule<Ctx, int>.EvaluateAsync(Ctx context, CancellationToken cancellationToken)
        {
            await Task.Yield();
            context.Log.Add(name);
            if (throws)
                throw new InvalidOperationException(name);
            return success ? value : Error.Validation(name, name);
        }
    }

    private sealed class Unknown : IRuleBase<Ctx>, IRuleBase<Ctx, int>;

    private sealed class Linear(IRule<Ctx> head, IRuleBase<Ctx>? next) : ILinearRule<Ctx>
    {
        public IRuleBase<Ctx>? Next => next;
        public Result Evaluate(Ctx context, CancellationToken cancellationToken = default) => head.Evaluate(context, cancellationToken);
    }

    private sealed class LinearAsync(IAsyncRule<Ctx> head, IRuleBase<Ctx>? next) : ILinearAsyncRule<Ctx>
    {
        public IRuleBase<Ctx>? Next => next;
        public ValueTask<Result> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => head.EvaluateAsync(context, cancellationToken);
    }

    private sealed class Conditional(IRule<Ctx> head, IRuleBase<Ctx>? success, IRuleBase<Ctx>? failure) : IConditionalRule<Ctx>
    {
        public IRuleBase<Ctx>? Success => success;
        public IRuleBase<Ctx>? Failure => failure;
        public Result Evaluate(Ctx context, CancellationToken cancellationToken = default) => head.Evaluate(context, cancellationToken);
    }

    private sealed class ConditionalAsync(IAsyncRule<Ctx> head, IRuleBase<Ctx>? success, IRuleBase<Ctx>? failure) : IConditionalAsyncRule<Ctx>
    {
        public IRuleBase<Ctx>? Success => success;
        public IRuleBase<Ctx>? Failure => failure;
        public ValueTask<Result> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => head.EvaluateAsync(context, cancellationToken);
    }

    private sealed class And(IRule<Ctx> head, params IRuleBase<Ctx>[] rules) : IAndRule<Ctx>
    {
        public IRuleBase<Ctx>[] Rules => rules;
        public Result Evaluate(Ctx context, CancellationToken cancellationToken = default) => head.Evaluate(context, cancellationToken);
    }

    private sealed class AndAsync(IAsyncRule<Ctx> head, params IRuleBase<Ctx>[] rules) : IAndAsyncRule<Ctx>
    {
        public IRuleBase<Ctx>[] Rules => rules;
        public ValueTask<Result> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => head.EvaluateAsync(context, cancellationToken);
    }

    private sealed class Or(IRule<Ctx> head, params IRuleBase<Ctx>[] rules) : IOrRule<Ctx>
    {
        public IRuleBase<Ctx>[] Rules => rules;
        public Result Evaluate(Ctx context, CancellationToken cancellationToken = default) => head.Evaluate(context, cancellationToken);
    }

    private sealed class OrAsync(IAsyncRule<Ctx> head, params IRuleBase<Ctx>[] rules) : IOrAsyncRule<Ctx>
    {
        public IRuleBase<Ctx>[] Rules => rules;
        public ValueTask<Result> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => head.EvaluateAsync(context, cancellationToken);
    }

    private sealed class LinearT(IRule<Ctx, int> head, IRuleBase<Ctx, int>? next) : ILinearRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>? Next => next;
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default) => head.Evaluate(context, cancellationToken);
    }

    private sealed class LinearAsyncT(IAsyncRule<Ctx, int> head, IRuleBase<Ctx, int>? next) : ILinearAsyncRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>? Next => next;
        public ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => head.EvaluateAsync(context, cancellationToken);
    }

    private sealed class ConditionalT(IRule<Ctx, int> head, IRuleBase<Ctx, int>? success, IRuleBase<Ctx, int>? failure) : IConditionalRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>? Success => success;
        public IRuleBase<Ctx, int>? Failure => failure;
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default) => head.Evaluate(context, cancellationToken);
    }

    private sealed class ConditionalAsyncT(IAsyncRule<Ctx, int> head, IRuleBase<Ctx, int>? success, IRuleBase<Ctx, int>? failure) : IConditionalAsyncRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>? Success => success;
        public IRuleBase<Ctx, int>? Failure => failure;
        public ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => head.EvaluateAsync(context, cancellationToken);
    }

    private sealed class AndT(IRule<Ctx, int> head, params IRuleBase<Ctx, int>[] rules) : IAndRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default) => head.Evaluate(context, cancellationToken);
    }

    private sealed class AndAsyncT(IAsyncRule<Ctx, int> head, params IRuleBase<Ctx, int>[] rules) : IAndAsyncRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => head.EvaluateAsync(context, cancellationToken);
    }

    private sealed class OrT(IRule<Ctx, int> head, params IRuleBase<Ctx, int>[] rules) : IOrRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default) => head.Evaluate(context, cancellationToken);
    }

    private sealed class OrAsyncT(IAsyncRule<Ctx, int> head, params IRuleBase<Ctx, int>[] rules) : IOrAsyncRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => head.EvaluateAsync(context, cancellationToken);
    }

    private static Leaf Ok(string name, int value = 0) => new(name, true, value);
    private static Leaf Fail(string name) => new(name, false);
    private static Leaf Throw(string name) => new(name, true, throws: true);
    private static AsyncLeaf AOk(string name, int value = 0) => new(name, true, value);
    private static AsyncLeaf AFail(string name) => new(name, false);
    private static AsyncLeaf AThrow(string name) => new(name, true, throws: true);

    #endregion

    #region Trees

    private static readonly Dictionary<string, Func<IRuleBase<Ctx>>> Trees = new()
    {
        ["simple-ok"] = () => Ok("a"),
        ["simple-fail"] = () => Fail("a"),
        ["simple-throw"] = () => Throw("a"),
        ["async-ok"] = () => AOk("a"),
        ["async-fail"] = () => AFail("a"),
        ["async-throw"] = () => AThrow("a"),
        ["unknown"] = () => new Unknown(),
        ["linear-ok"] = () => new Linear(Ok("h"), new Linear(Ok("a"), Ok("b"))),
        ["linear-head-fail"] = () => new Linear(Fail("h"), Ok("a")),
        ["linear-next-fail"] = () => new Linear(Ok("h"), new Linear(Fail("a"), Ok("b"))),
        ["linear-null-next"] = () => new Linear(Ok("h"), null),
        ["linear-head-throw"] = () => new Linear(Throw("h"), Ok("a")),
        ["linear-async-ok"] = () => new LinearAsync(AOk("h"), new LinearAsync(AOk("a"), AOk("b"))),
        ["linear-async-head-fail"] = () => new LinearAsync(AFail("h"), AOk("a")),
        ["linear-async-next-fail"] = () => new LinearAsync(AOk("h"), AFail("a")),
        ["linear-async-head-throw"] = () => new LinearAsync(AThrow("h"), AOk("a")),
        ["linear-async-null-next"] = () => new LinearAsync(AOk("h"), null),
        ["conditional-success"] = () => new Conditional(Ok("h"), Ok("s"), Ok("f")),
        ["conditional-failure"] = () => new Conditional(Fail("h"), Ok("s"), Fail("f")),
        ["conditional-null-success"] = () => new Conditional(Ok("h"), null, Ok("f")),
        ["conditional-null-failure"] = () => new Conditional(Fail("h"), Ok("s"), null),
        ["conditional-async-success"] = () => new ConditionalAsync(AOk("h"), AFail("s"), AOk("f")),
        ["conditional-async-failure"] = () => new ConditionalAsync(AFail("h"), AOk("s"), AFail("f")),
        ["conditional-async-null-branches"] = () => new ConditionalAsync(AFail("h"), null, null),
        ["and-ok"] = () => new And(Ok("h"), Ok("a"), Ok("b")),
        ["and-short-circuit"] = () => new And(Ok("h"), Ok("a"), Fail("b"), Fail("c")),
        ["and-head-fail"] = () => new And(Fail("h"), Ok("a")),
        ["and-empty"] = () => new And(Ok("h")),
        ["and-child-throw"] = () => new And(Ok("h"), Throw("a"), Ok("b")),
        ["and-async-ok"] = () => new AndAsync(AOk("h"), AOk("a"), AOk("b")),
        ["and-async-short-circuit"] = () => new AndAsync(AOk("h"), AOk("a"), AFail("b"), AFail("c")),
        ["and-async-head-fail"] = () => new AndAsync(AFail("h"), AOk("a")),
        ["or-all-fail"] = () => new Or(Ok("h"), Fail("a"), Fail("b"), Fail("c")),
        ["or-short-circuit"] = () => new Or(Ok("h"), Fail("a"), Ok("b"), Fail("c")),
        ["or-head-fail"] = () => new Or(Fail("h"), Ok("a")),
        ["or-empty"] = () => new Or(Ok("h")),
        ["or-async-all-fail"] = () => new OrAsync(AOk("h"), AFail("a"), AFail("b")),
        ["or-async-short-circuit"] = () => new OrAsync(AOk("h"), AFail("a"), AOk("b"), AFail("c")),
        ["or-async-head-fail"] = () => new OrAsync(AFail("h"), AOk("a")),
        ["extensions-and-empty"] = () => Array.Empty<IRuleBase<Ctx>>().And(),
        ["extensions-or-mixed"] = () => new IRuleBase<Ctx>[] { AFail("a"), Fail("b"), AOk("c"), Ok("d") }.Or(),
        ["mixed-and-of-async"] = () => new And(Ok("h"), AOk("a"), Ok("b"), AFail("c"), AOk("d")),
        ["mixed-or-of-sync-in-async"] = () => new OrAsync(AOk("h"), Fail("a"), AFail("b"), Ok("c")),
        ["mixed-nested"] = () => new And(
            Ok("root"),
            new LinearAsync(AOk("l1"), new Conditional(Fail("c"), AOk("s"), new OrAsync(AOk("o"), Fail("o1"), AFail("o2"), AOk("o3"), Ok("o4")))),
            new Or(Ok("or"), AFail("x"), new ConditionalAsync(AOk("y"), new And(Ok("z"), AOk("z1"), Ok("z2")), null)),
            new Linear(Ok("tail"), AFail("last"))),
        ["mixed-nested-throw"] = () => new AndAsync(
            AOk("root"),
            new Linear(Ok("a"), new OrAsync(AOk("b"), AFail("c"), AThrow("d")))),
    };

    private static readonly Dictionary<string, Func<IRuleBase<Ctx, int>>> TreesT = new()
    {
        ["simple-ok"] = () => Ok("a", 1),
        ["simple-fail"] = () => Fail("a"),
        ["simple-throw"] = () => Throw("a"),
        ["async-ok"] = () => AOk("a", 1),
        ["async-fail"] = () => AFail("a"),
        ["async-throw"] = () => AThrow("a"),
        ["unknown"] = () => new Unknown(),
        ["linear-ok"] = () => new LinearT(Ok("h", 1), new LinearT(Ok("a", 2), Ok("b", 3))),
        ["linear-head-fail"] = () => new LinearT(Fail("h"), Ok("a", 2)),
        ["linear-next-fail"] = () => new LinearT(Ok("h", 1), Fail("a")),
        ["linear-null-next"] = () => new LinearT(Ok("h", 1), null),
        ["linear-async-ok"] = () => new LinearAsyncT(AOk("h", 1), new LinearAsyncT(AOk("a", 2), AOk("b", 3))),
        ["linear-async-head-fail"] = () => new LinearAsyncT(AFail("h"), AOk("a", 2)),
        ["linear-async-head-throw"] = () => new LinearAsyncT(AThrow("h"), AOk("a", 2)),
        ["conditional-success"] = () => new ConditionalT(Ok("h", 1), Ok("s", 2), Ok("f", 3)),
        ["conditional-failure"] = () => new ConditionalT(Fail("h"), Ok("s", 2), Fail("f")),
        ["conditional-null-success"] = () => new ConditionalT(Ok("h", 1), null, Ok("f", 3)),
        ["conditional-async-success"] = () => new ConditionalAsyncT(AOk("h", 1), AOk("s", 2), AOk("f", 3)),
        ["conditional-async-failure"] = () => new ConditionalAsyncT(AFail("h"), AOk("s", 2), AFail("f")),
        ["conditional-async-null-branches"] = () => new ConditionalAsyncT(AFail("h"), null, null),
        ["and-ok-first-value"] = () => new AndT(Ok("h"), Ok("a", 2), Ok("b", 3)),
        ["and-aggregates-all-errors"] = () => new AndT(Ok("h"), Ok("a", 2), Fail("b"), Ok("c", 4), Fail("d")),
        ["and-head-fail"] = () => new AndT(Fail("h"), Ok("a", 2)),
        ["and-empty"] = () => new AndT(Ok("h")),
        ["and-child-throw"] = () => new AndT(Ok("h"), Throw("a"), Ok("b", 3)),
        ["and-async-ok-first-value"] = () => new AndAsyncT(AOk("h"), AOk("a", 2), AOk("b", 3)),
        ["and-async-aggregates-all-errors"] = () => new AndAsyncT(AOk("h"), AFail("a"), AOk("b", 3), AFail("c")),
        ["and-async-head-fail"] = () => new AndAsyncT(AFail("h"), AOk("a", 2)),
        ["or-all-fail"] = () => new OrT(Ok("h"), Fail("a"), Fail("b")),
        ["or-short-circuit"] = () => new OrT(Ok("h"), Fail("a"), Ok("b", 3), Ok("c", 4)),
        ["or-head-fail"] = () => new OrT(Fail("h"), Ok("a", 2)),
        ["or-async-all-fail"] = () => new OrAsyncT(AOk("h"), AFail("a"), AFail("b")),
        ["or-async-short-circuit"] = () => new OrAsyncT(AOk("h"), AFail("a"), AOk("b", 3), AOk("c", 4)),
        ["extensions-and-empty"] = () => Array.Empty<IRuleBase<Ctx, int>>().And(),
        ["mixed-nested"] = () => new AndT(
            Ok("root"),
            new LinearAsyncT(AOk("l1", 1), new ConditionalT(Fail("c"), AOk("s", 2), new OrAsyncT(AOk("o"), Fail("o1"), AFail("o2"), AOk("o3", 3), Ok("o4", 4)))),
            new OrT(Ok("or"), AFail("x"), new ConditionalAsyncT(AOk("y", 5), new AndT(Ok("z"), AOk("z1", 6), Ok("z2", 7)), null)),
            new LinearT(Ok("tail", 8), AFail("last"))),
        ["mixed-nested-success"] = () => new AndAsyncT(
            AOk("root"),
            new LinearT(Ok("a", 1), new OrAsyncT(AOk("b"), AFail("c"), Ok("d", 4))),
            AOk("e", 5)),
    };

    public static TheoryData<string> TreeNames() => [.. Trees.Keys];
    public static TheoryData<string> TreeNamesT() => [.. TreesT.Keys];

    #endregion

    #region Parity

    [Theory]
    [MemberData(nameof(TreeNames))]
    public async Task EvaluateAsync_Should_Match_Evaluate(string tree)
    {
        var syncContext = new Ctx();
        var asyncContext = new Ctx();

        Result expected = EvaluateSync(Trees[tree](), syncContext);
        Result actual = await RuleEngine.EvaluateAsync(Trees[tree](), asyncContext);

        actual.IsSuccess.Should().Be(expected.IsSuccess);
        Describe(actual.ErrorsOrEmptyArray).Should().Equal(Describe(expected.ErrorsOrEmptyArray));
        asyncContext.Log.Should().Equal(syncContext.Log);
    }

    [Theory]
    [MemberData(nameof(TreeNamesT))]
    public async Task EvaluateAsync_TResult_Should_Match_Evaluate(string tree)
    {
        var syncContext = new Ctx();
        var asyncContext = new Ctx();

        Result<int> expected = EvaluateSync(TreesT[tree](), syncContext);
        Result<int> actual = await RuleEngine.EvaluateAsync(TreesT[tree](), asyncContext);

        actual.IsSuccess.Should().Be(expected.IsSuccess);
        actual.Value.Should().Be(expected.Value);
        Describe(actual.ErrorsOrEmptyArray).Should().Equal(Describe(expected.ErrorsOrEmptyArray));
        asyncContext.Log.Should().Equal(syncContext.Log);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Report_Mixed_Nested_Errors_In_Order()
    {
        var context = new Ctx();

        Result result = await RuleEngine.EvaluateAsync(Trees["mixed-nested"](), context);

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(e => e.Code).Should().Equal("last");
        context.Log.Should().Equal("root", "l1", "c", "o", "o1", "o2", "o3", "or", "x", "y", "z", "z1", "z2", "tail", "last");
    }

    [Fact]
    public async Task EvaluateAsync_TResult_Should_Aggregate_All_And_Errors()
    {
        var context = new Ctx();

        Result<int> result = await RuleEngine.EvaluateAsync(TreesT["and-async-aggregates-all-errors"](), context);

        result.Errors.Select(e => e.Code).Should().Equal("a", "c");
        context.Log.Should().Equal("h", "a", "b", "c");
    }

    [Fact]
    public async Task EvaluateAsync_Should_Wrap_Child_Exception_With_Child_Rule_Type()
    {
        Result result = await RuleEngine.EvaluateAsync(Trees["mixed-nested-throw"](), new Ctx());

        result.Errors.Select(e => e.Code).Should().Equal("c", "RuleEngine.Evaluate.SimpleAsyncRule");
    }

    private static Result EvaluateSync(IRuleBase<Ctx> rule, Ctx context) => RuleEngine.Evaluate(rule, context);

    private static Result<int> EvaluateSync(IRuleBase<Ctx, int> rule, Ctx context) => RuleEngine.Evaluate(rule, context);

    private static IEnumerable<string> Describe(IEnumerable<Error> errors) =>
        errors.Select(e => $"{e.Type}|{e.Code}|{e.Description}");

    #endregion

    #region Async behaviour

    [Fact]
    public async Task EvaluateAsync_Should_Complete_Synchronously_For_Sync_Tree()
    {
        ValueTask<Result> pending = RuleEngine.EvaluateAsync<Ctx>(new And(Ok("h"), Ok("a"), new Linear(Ok("b"), Ok("c"))), new Ctx());
        ValueTask<Result<int>> pendingT = RuleEngine.EvaluateAsync<Ctx, int>(Ok("a", 7), new Ctx());

        pending.IsCompletedSuccessfully.Should().BeTrue();
        pendingT.IsCompletedSuccessfully.Should().BeTrue();
        (await pending).IsSuccess.Should().BeTrue();
        (await pendingT).Value.Should().Be(7);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Await_Pending_Children_Without_Blocking()
    {
        var first = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        var context = new Ctx();
        IRuleBase<Ctx> rule = new And(
            Ok("h"),
            new Func<Ctx, CancellationToken, ValueTask<Result>>((c, _) => { c.Log.Add("first"); return new ValueTask<Result>(first.Task); }).ToRule(),
            Ok("middle"),
            new Func<Ctx, CancellationToken, ValueTask<Result>>((c, _) => { c.Log.Add("second"); return new ValueTask<Result>(second.Task); }).ToRule());

        ValueTask<Result> pending = RuleEngine.EvaluateAsync(rule, context);

        pending.IsCompleted.Should().BeFalse();
        context.Log.Should().Equal("h", "first");

        first.SetResult(Result.Success());
        await WaitUntil(() => context.Log.Count == 4);
        pending.IsCompleted.Should().BeFalse();
        context.Log.Should().Equal("h", "first", "middle", "second");

        second.SetResult(Error.Validation("late", "late"));
        Result result = await pending;

        result.Errors.Select(e => e.Code).Should().Equal("late");
    }

    [Fact]
    public async Task EvaluateAsync_TResult_Should_Await_Pending_Children_Without_Blocking()
    {
        var gate = new TaskCompletionSource<Result<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var context = new Ctx();
        IRuleBase<Ctx, int> rule = new LinearAsyncT(
            AOk("h", 1),
            new Func<Ctx, CancellationToken, ValueTask<Result<int>>>((c, _) => { c.Log.Add("gate"); return new ValueTask<Result<int>>(gate.Task); }).ToRule());

        ValueTask<Result<int>> pending = RuleEngine.EvaluateAsync(rule, context);
        await WaitUntil(() => context.Log.Count == 2);

        pending.IsCompleted.Should().BeFalse();

        gate.SetResult(42);
        Result<int> result = await pending;

        result.Value.Should().Be(42);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!condition())
            await Task.Delay(5, timeout.Token);
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task EvaluateAsync_Should_Throw_When_Token_Is_Already_Cancelled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var context = new Ctx();

        Func<Task> act = async () => await RuleEngine.EvaluateAsync(new And(Ok("h"), Ok("a")), context, cts.Token);
        Func<Task> actT = async () => await RuleEngine.EvaluateAsync<Ctx, int>(Ok("a", 1), context, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        await actT.Should().ThrowAsync<OperationCanceledException>();
        context.Log.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_Should_Propagate_Cancellation_From_Pending_Async_Rule()
    {
        using var cts = new CancellationTokenSource();
        var context = new Ctx();
        IRuleBase<Ctx> rule = new And(
            Ok("h"),
            new Func<Ctx, CancellationToken, ValueTask<Result>>(async (c, ct) =>
            {
                c.Log.Add("waiting");
                await Task.Delay(Timeout.Infinite, ct);
                return Result.Success();
            }).ToRule(),
            Ok("after"));

        ValueTask<Result> pending = RuleEngine.EvaluateAsync(rule, context, cts.Token);
        await cts.CancelAsync();

        Func<Task> act = async () => await pending;

        await act.Should().ThrowAsync<OperationCanceledException>();
        context.Log.Should().Equal("h", "waiting");
    }

    [Fact]
    public async Task EvaluateAsync_Should_Stop_Before_Next_Rule_When_Cancelled_Midway()
    {
        using var cts = new CancellationTokenSource();
        var context = new Ctx();
        IRuleBase<Ctx, int> rule = new LinearT(
            new Func<Ctx, CancellationToken, Result<int>>((c, _) =>
            {
                c.Log.Add("cancel");
                cts.Cancel();
                return 1;
            }).ToRule(),
            Ok("next", 2));

        Func<Task> act = async () => await RuleEngine.EvaluateAsync(rule, context, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        context.Log.Should().Equal("cancel");
    }

    [Fact]
    public async Task EvaluateAsync_Should_Propagate_Cancellation_Thrown_By_Sync_Rule()
    {
        using var cts = new CancellationTokenSource();
        IRuleBase<Ctx> rule = new Func<Ctx, CancellationToken, Result>((_, ct) =>
        {
            cts.Cancel();
            ct.ThrowIfCancellationRequested();
            return Result.Success();
        }).ToRule();

        Func<Task> act = async () => await RuleEngine.EvaluateAsync(rule, new Ctx(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EvaluateAsync_Should_Convert_Unrelated_Cancellation_To_Error()
    {
        IRuleBase<Ctx> rule = new Func<Ctx, CancellationToken, ValueTask<Result>>((_, _) =>
            throw new OperationCanceledException()).ToRule();

        Result result = await RuleEngine.EvaluateAsync(rule, new Ctx());

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.SimpleAsyncRule");
    }

    #endregion

    #region SynchronizationContext

    private sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = [];

        public override void Post(SendOrPostCallback d, object? state) => _queue.Add((d, state));

        public override void Send(SendOrPostCallback d, object? state) => throw new NotSupportedException();

        public static bool RunOnDedicatedThread(Action<SingleThreadSynchronizationContext> body, TimeSpan timeout)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                var context = new SingleThreadSynchronizationContext();
                SetSynchronizationContext(context);
                try
                {
                    body(context);
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
            })
            { IsBackground = true };

            thread.Start();
            bool finished = thread.Join(timeout);
            if (failure is not null)
                throw new InvalidOperationException("Body failed.", failure);
            return finished;
        }

        public void Pump(Task task)
        {
            task.ContinueWith(_ => _queue.CompleteAdding(), TaskScheduler.Default);
            foreach ((SendOrPostCallback callback, object? state) in _queue.GetConsumingEnumerable())
                callback(state);
            task.GetAwaiter().GetResult();
        }
    }

    private static AndAsync DelayedTree() => new AndAsync(
        new Func<Ctx, CancellationToken, ValueTask<Result>>(async (_, ct) =>
        {
            await Task.Delay(10, ct).ConfigureAwait(false);
            return Result.Success();
        }).ToRule(),
        new LinearAsync(
            new Func<Ctx, CancellationToken, ValueTask<Result>>(async (_, ct) =>
            {
                await Task.Delay(10, ct).ConfigureAwait(false);
                return Result.Success();
            }).ToRule(),
            Ok("sync")),
        new Func<Ctx, CancellationToken, ValueTask<Result>>(async (c, ct) =>
        {
            await Task.Delay(10, ct).ConfigureAwait(false);
            c.Log.Add("last");
            return Error.Validation("done", "done");
        }).ToRule());

    [Fact]
    public void EvaluateAsync_Should_Complete_When_Awaited_On_Single_Threaded_Context()
    {
        Result? result = null;

        bool finished = SingleThreadSynchronizationContext.RunOnDedicatedThread(
            context => context.Pump(Run()),
            TimeSpan.FromSeconds(10));

        finished.Should().BeTrue();
        result!.Value.Errors.Select(e => e.Code).Should().Equal("done");

        async Task Run() => result = await RuleEngine.EvaluateAsync(DelayedTree(), new Ctx());
    }

    [Fact]
    public void EvaluateAsync_Should_Not_Deadlock_When_Blocked_On_Single_Threaded_Context()
    {
        bool completed = false;

        bool finished = SingleThreadSynchronizationContext.RunOnDedicatedThread(
            _ => completed = RuleEngine.EvaluateAsync(DelayedTree(), new Ctx()).AsTask().Wait(TimeSpan.FromSeconds(5)),
            TimeSpan.FromSeconds(10));

        finished.Should().BeTrue();
        completed.Should().BeTrue();
    }

    #endregion
}
