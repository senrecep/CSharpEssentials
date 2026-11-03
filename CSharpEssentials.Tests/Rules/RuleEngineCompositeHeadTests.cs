using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public sealed class RuleEngineCompositeHeadTests
{
    private sealed class Ctx
    {
        public List<string> Log { get; } = [];
    }

    private sealed class Leaf(string name, bool success = true, int value = 0) : IRule<Ctx>, IRule<Ctx, int>, IAsyncRule<Ctx>, IAsyncRule<Ctx, int>
    {
        Result IRule<Ctx>.Evaluate(Ctx context, CancellationToken cancellationToken)
        {
            context.Log.Add(name);
            return success ? Result.Success() : Error.Validation(name, name);
        }

        Result<int> IRule<Ctx, int>.Evaluate(Ctx context, CancellationToken cancellationToken)
        {
            context.Log.Add(name);
            return success ? value : Error.Validation(name, name);
        }

        ValueTask<Result> IAsyncRule<Ctx>.EvaluateAsync(Ctx context, CancellationToken cancellationToken) =>
            new(((IRule<Ctx>)this).Evaluate(context, cancellationToken));

        ValueTask<Result<int>> IAsyncRule<Ctx, int>.EvaluateAsync(Ctx context, CancellationToken cancellationToken) =>
            new(((IRule<Ctx, int>)this).Evaluate(context, cancellationToken));
    }

    private sealed class AndAsync(bool throws, params IRuleBase<Ctx>[] rules) : IAndAsyncRule<Ctx>
    {
        public IRuleBase<Ctx>[] Rules => rules;
        public async ValueTask<Result> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return throws ? throw new InvalidOperationException("head") : Result.Success();
        }
    }

    private sealed class OrAsync(bool throws, params IRuleBase<Ctx>[] rules) : IOrAsyncRule<Ctx>
    {
        public IRuleBase<Ctx>[] Rules => rules;
        public async ValueTask<Result> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return throws ? throw new InvalidOperationException("head") : Result.Success();
        }
    }

    private sealed class ThrowingConditional(IRuleBase<Ctx> success) : IConditionalRule<Ctx>
    {
        public IRuleBase<Ctx>? Success => success;
        public IRuleBase<Ctx>? Failure => null;
        public Result Evaluate(Ctx context, CancellationToken cancellationToken = default) => throw new InvalidOperationException("head");
    }

    private sealed class ThrowingAnd(params IRuleBase<Ctx>[] rules) : IAndRule<Ctx>
    {
        public IRuleBase<Ctx>[] Rules => rules;
        public Result Evaluate(Ctx context, CancellationToken cancellationToken = default) => throw new InvalidOperationException("head");
    }

    private sealed class AndAsyncT(bool throws, params IRuleBase<Ctx, int>[] rules) : IAndAsyncRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public async ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return throws ? throw new InvalidOperationException("head") : 0;
        }
    }

    private sealed class OrAsyncT(Func<Result<int>> head, params IRuleBase<Ctx, int>[] rules) : IOrAsyncRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public async ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return head();
        }
    }

    private sealed class ThrowingConditionalT(IRuleBase<Ctx, int> success) : IConditionalRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>? Success => success;
        public IRuleBase<Ctx, int>? Failure => null;
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default) => throw new InvalidOperationException("head");
    }

    private sealed class ThrowingOrT(params IRuleBase<Ctx, int>[] rules) : IOrRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default) => throw new InvalidOperationException("head");
    }

    private sealed class CancellingRuleT(CancellationTokenSource cts) : IRule<Ctx, int>
    {
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default)
        {
            cts.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return 1;
        }
    }

    [Fact]
    public void Evaluate_Should_Return_AndAsync_Error_When_Async_And_Head_Throws()
    {
        var ctx = new Ctx();

        Result result = RuleEngine.Evaluate(new AndAsync(true, new Leaf("child")), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.AndAsyncRule");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_Should_Return_OrAsync_Error_When_Async_Or_Head_Throws()
    {
        var ctx = new Ctx();

        Result result = RuleEngine.Evaluate(new OrAsync(true, new Leaf("child")), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.OrAsyncRule");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_Should_Return_Conditional_Error_When_Sync_Conditional_Head_Throws()
    {
        var ctx = new Ctx();

        Result result = await RuleEngine.EvaluateAsync(new ThrowingConditional(new Leaf("success")), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.ConditionalRule");
        result.FirstError.Description.Should().Be("head");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_Should_Return_And_Error_When_Sync_And_Head_Throws()
    {
        var ctx = new Ctx();

        Result result = await RuleEngine.EvaluateAsync(new ThrowingAnd(new Leaf("child")), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.AndRule");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_Generic_Should_Return_AndAsync_Error_When_Async_And_Head_Throws()
    {
        var ctx = new Ctx();

        Result<int> result = RuleEngine.Evaluate(new AndAsyncT(true, new Leaf("child", value: 1)), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.AndAsyncRule");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_Generic_Should_Return_Head_Failure_Without_Children_When_Async_Or_Head_Fails()
    {
        var ctx = new Ctx();

        Result<int> result = RuleEngine.Evaluate(new OrAsyncT(() => Error.Validation("head", "head"), new Leaf("child", value: 1)), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("head");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_Generic_Should_Return_First_Successful_Child_When_Async_Or_Head_Succeeds()
    {
        var ctx = new Ctx();

        Result<int> result = RuleEngine.Evaluate(
            new OrAsyncT(() => 0, new Leaf("a", success: false), new Leaf("b", value: 7)),
            ctx);

        result.Value.Should().Be(7);
        ctx.Log.Should().Equal("a", "b");
    }

    [Fact]
    public void Evaluate_Generic_Should_Return_OrAsync_Error_When_Async_Or_Head_Throws()
    {
        var ctx = new Ctx();

        Result<int> result = RuleEngine.Evaluate(new OrAsyncT(() => throw new InvalidOperationException("head"), new Leaf("child")), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.OrAsyncRule");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_Generic_Should_Return_Conditional_Error_When_Sync_Conditional_Head_Throws()
    {
        var ctx = new Ctx();

        Result<int> result = await RuleEngine.EvaluateAsync(new ThrowingConditionalT(new Leaf("success", value: 1)), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.ConditionalRule");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_Generic_Should_Return_Or_Error_When_Sync_Or_Head_Throws()
    {
        var ctx = new Ctx();

        Result<int> result = await RuleEngine.EvaluateAsync(new ThrowingOrT(new Leaf("child", value: 1)), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("RuleEngine.Evaluate.OrRule");
        ctx.Log.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_Generic_Should_Throw_When_Sync_Rule_Observes_Caller_Cancellation()
    {
        using var cts = new CancellationTokenSource();

        Func<Task> act = async () => await RuleEngine.EvaluateAsync(new CancellingRuleT(cts), new Ctx(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
