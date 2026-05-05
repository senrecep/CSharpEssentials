using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class ExtensionTests
{
    private sealed class TestContext
    {
        public List<string> ExecutedRules { get; } = [];
    }

    private sealed class SimpleRule(Func<TestContext, Result> func) : IRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default) => func(context);
    }

    private sealed class SimpleRuleT(Func<TestContext, Result<int>> func) : IRule<TestContext, int>
    {
        public Result<int> Evaluate(TestContext context, CancellationToken cancellationToken = default) => func(context);
    }

    private sealed class SimpleAsyncRule(Func<TestContext, CancellationToken, ValueTask<Result>> func) : IAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => func(context, cancellationToken);
    }

    private sealed class SimpleAsyncRuleT(Func<TestContext, CancellationToken, ValueTask<Result<int>>> func) : IAsyncRule<TestContext, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => func(context, cancellationToken);
    }

    #region ToRule

    [Fact]
    public void ToRule_FuncTContextResult_ShouldCreateEvaluableRule()
    {
        Func<TestContext, Result> func = _ => Result.Success();
        IRule<TestContext> rule = func.ToRule();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToRule_FuncTContextCancellationTokenResult_ShouldCreateEvaluableRule()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        Func<TestContext, CancellationToken, Result> func = (_, ct) => { captured = ct; return Result.Success(); };
        IRule<TestContext> rule = func.ToRule();
        Result result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(cts.Token);
    }

    [Fact]
    public void ToRule_FuncTContextCancellationTokenValueTaskResult_ShouldCreateEvaluableAsyncRule()
    {
        Func<TestContext, CancellationToken, ValueTask<Result>> func = (_, _) => ValueTask.FromResult(Result.Success());
        IAsyncRule<TestContext> rule = func.ToRule();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToRule_FuncTContextResultTResult_ShouldCreateEvaluableRuleT()
    {
        Func<TestContext, Result<int>> func = _ => Result.Success(42);
        IRule<TestContext, int> rule = func.ToRule();
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToRule_FuncTContextCancellationTokenResultTResult_ShouldCreateEvaluableRuleT()
    {
        Func<TestContext, CancellationToken, Result<int>> func = (_, _) => Result.Success(42);
        IRule<TestContext, int> rule = func.ToRule();
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToRule_FuncTContextCancellationTokenValueTaskResultTResult_ShouldCreateEvaluableAsyncRuleT()
    {
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> func = (_, _) => ValueTask.FromResult(Result.Success(99));
        IAsyncRule<TestContext, int> rule = func.ToRule();
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion

    #region Next - IRule<TContext> to ...

    [Fact]
    public void Next_IRule_To_IRule_ShouldChain()
    {
        TestContext ctx = new();
        IRule<TestContext> first = new SimpleRule(_ => { ctx.ExecutedRules.Add("R1"); return Result.Success(); });
        IRule<TestContext> second = new SimpleRule(_ => { ctx.ExecutedRules.Add("R2"); return Result.Success(); });
        IRule<TestContext> chain = first.Next(second);
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_IRule_To_Func_ShouldChain()
    {
        TestContext ctx = new();
        IRule<TestContext> first = new SimpleRule(_ => { ctx.ExecutedRules.Add("R1"); return Result.Success(); });
        IRule<TestContext> chain = first.Next(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_IRule_To_FuncWithToken_ShouldChain()
    {
        TestContext ctx = new();
        IRule<TestContext> first = new SimpleRule(_ => { ctx.ExecutedRules.Add("R1"); return Result.Success(); });
        IRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_IRule_To_FuncAsync_ShouldChain()
    {
        TestContext ctx = new();
        IRule<TestContext> first = new SimpleRule(_ => { ctx.ExecutedRules.Add("R1"); return Result.Success(); });
        IRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    #endregion

    #region Next - IAsyncRule<TContext> to ...

    [Fact]
    public void Next_IAsyncRule_To_IRule_ShouldChain()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext> first = new SimpleAsyncRule((_, _) => { ctx.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); });
        IRule<TestContext> second = new SimpleRule(_ => { ctx.ExecutedRules.Add("R2"); return Result.Success(); });
        IAsyncRule<TestContext> chain = first.Next(second);
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_IAsyncRule_To_Func_ShouldChain()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext> first = new SimpleAsyncRule((_, _) => { ctx.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); });
        IAsyncRule<TestContext> chain = first.Next(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_IAsyncRule_To_FuncWithToken_ShouldChain()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext> first = new SimpleAsyncRule((_, _) => { ctx.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); });
        IAsyncRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_IAsyncRule_To_FuncAsync_ShouldChain()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext> first = new SimpleAsyncRule((_, _) => { ctx.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); });
        IAsyncRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    #endregion

    #region Next - Func<TContext, Result> to ...

    [Fact]
    public void Next_Func_To_IRule_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, Result> first = c => { c.ExecutedRules.Add("R1"); return Result.Success(); };
        IRule<TestContext> second = new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        IRule<TestContext> chain = first.Next(second);
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_Func_To_Func_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, Result> first = c => { c.ExecutedRules.Add("R1"); return Result.Success(); };
        IRule<TestContext> chain = first.Next(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_Func_To_FuncWithToken_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, Result> first = c => { c.ExecutedRules.Add("R1"); return Result.Success(); };
        IRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_Func_To_FuncAsync_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, Result> first = c => { c.ExecutedRules.Add("R1"); return Result.Success(); };
        IRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    #endregion

    #region Next - Func<TContext, CancellationToken, Result> to ...

    [Fact]
    public void Next_FuncWithToken_To_IRule_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, Result> first = (c, _) => { c.ExecutedRules.Add("R1"); return Result.Success(); };
        IRule<TestContext> second = new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        IRule<TestContext> chain = first.Next(second);
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_FuncWithToken_To_Func_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, Result> first = (c, _) => { c.ExecutedRules.Add("R1"); return Result.Success(); };
        IRule<TestContext> chain = first.Next(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_FuncWithToken_To_FuncWithToken_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, Result> first = (c, _) => { c.ExecutedRules.Add("R1"); return Result.Success(); };
        IRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_FuncWithToken_To_FuncAsync_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, Result> first = (c, _) => { c.ExecutedRules.Add("R1"); return Result.Success(); };
        IRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    #endregion

    #region Next - Func<TContext, CancellationToken, ValueTask<Result>> to ...

    [Fact]
    public void Next_FuncAsync_To_IRule_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, ValueTask<Result>> first = (c, _) => { ctx.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); };
        IRule<TestContext> second = new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        IAsyncRule<TestContext> chain = first.Next(second);
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_FuncAsync_To_Func_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, ValueTask<Result>> first = (c, _) => { ctx.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); };
        IAsyncRule<TestContext> chain = first.Next(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_FuncAsync_To_FuncWithToken_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, ValueTask<Result>> first = (c, _) => { ctx.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); };
        IAsyncRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Next_FuncAsync_To_FuncAsync_ShouldChain()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, ValueTask<Result>> first = (c, _) => { ctx.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); };
        IAsyncRule<TestContext> chain = first.Next((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    #endregion

    #region Next - TResult variants

    [Fact]
    public void Next_IRuleTResult_To_IRuleTResult_ShouldChain()
    {
        IRule<TestContext, int> first = new SimpleRuleT(_ => Result.Success(1));
        IRule<TestContext, int> second = new SimpleRuleT(_ => Result.Success(2));
        IRule<TestContext, int> chain = first.Next(second);
        Result<int> result = RuleEngine.Evaluate(chain, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public void Next_IRuleTResult_To_FuncTResult_ShouldChain()
    {
        IRule<TestContext, int> first = new SimpleRuleT(_ => Result.Success(1));
        IRule<TestContext, int> chain = first.Next(_ => Result.Success(2));
        Result<int> result = RuleEngine.Evaluate(chain, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public void Next_IAsyncRuleTResult_To_IRuleTResult_ShouldChain()
    {
        IAsyncRule<TestContext, int> first = new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(1)));
        IRule<TestContext, int> second = new SimpleRuleT(_ => Result.Success(2));
        IAsyncRule<TestContext, int> chain = first.Next(second);
        Result<int> result = RuleEngine.Evaluate(chain, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public void Next_FuncTResult_To_FuncTResult_ShouldChain()
    {
        Func<TestContext, Result<int>> first = _ => Result.Success(1);
        IRule<TestContext, int> chain = first.Next(_ => Result.Success(2));
        Result<int> result = RuleEngine.Evaluate(chain, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public void Next_FuncWithTokenTResult_To_FuncWithTokenTResult_ShouldChain()
    {
        Func<TestContext, CancellationToken, Result<int>> first = (_, _) => Result.Success(1);
        IRule<TestContext, int> chain = first.Next((_, _) => Result.Success(2));
        Result<int> result = RuleEngine.Evaluate(chain, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public void Next_FuncAsyncTResult_To_FuncAsyncTResult_ShouldChain()
    {
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> first = (_, _) => ValueTask.FromResult(Result.Success(1));
        IAsyncRule<TestContext, int> chain = first.Next((_, _) => ValueTask.FromResult(Result.Success(2)));
        Result<int> result = RuleEngine.Evaluate(chain, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    #endregion

    #region And extension on arrays

    [Fact]
    public void Extension_And_IRuleArray_ShouldCreateAdapter()
    {
        IRule<TestContext>[] rules = [new SimpleRule(_ => Result.Success()), new SimpleRule(_ => Result.Success())];
        IRuleBase<TestContext> adapter = rules.And();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Extension_And_FuncArray_ShouldCreateAdapter()
    {
        Func<TestContext, Result>[] rules = [_ => Result.Success(), _ => Result.Success()];
        IRuleBase<TestContext> adapter = rules.And();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Extension_And_FuncWithTokenArray_ShouldCreateAdapter()
    {
        Func<TestContext, CancellationToken, Result>[] rules = [(_, _) => Result.Success(), (_, _) => Result.Success()];
        IRuleBase<TestContext> adapter = rules.And();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Extension_And_FuncAsyncArray_ShouldCreateAdapter()
    {
        Func<TestContext, CancellationToken, ValueTask<Result>>[] rules = [(_, _) => ValueTask.FromResult(Result.Success()), (_, _) => ValueTask.FromResult(Result.Success())];
        IRuleBase<TestContext> adapter = rules.And();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Or extension on arrays

    [Fact]
    public void Extension_Or_IRuleArray_ShouldCreateAdapter()
    {
        IRule<TestContext>[] rules = [new SimpleRule(_ => Result.Success()), new SimpleRule(_ => Result.Success())];
        IRuleBase<TestContext> adapter = rules.Or();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Extension_Or_FuncArray_ShouldCreateAdapter()
    {
        Func<TestContext, Result>[] rules = [_ => Result.Success(), _ => Result.Success()];
        IRuleBase<TestContext> adapter = rules.Or();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Extension_Or_FuncWithTokenArray_ShouldCreateAdapter()
    {
        Func<TestContext, CancellationToken, Result>[] rules = [(_, _) => Result.Success(), (_, _) => Result.Success()];
        IRuleBase<TestContext> adapter = rules.Or();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Extension_Or_FuncAsyncArray_ShouldCreateAdapter()
    {
        Func<TestContext, CancellationToken, ValueTask<Result>>[] rules = [(_, _) => ValueTask.FromResult(Result.Success()), (_, _) => ValueTask.FromResult(Result.Success())];
        IRuleBase<TestContext> adapter = rules.Or();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Linear extension on arrays

    [Fact]
    public void Extension_Linear_IRuleArray_ShouldCreateChain()
    {
        TestContext ctx = new();
        IRule<TestContext>[] rules = [new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); }), new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); })];
        IRuleBase<TestContext> chain = rules.Linear();
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Extension_Linear_IAsyncRuleArray_ShouldCreateChain()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext>[] rules = [new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); }), new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); })];
        IRuleBase<TestContext> chain = rules.Linear();
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Extension_Linear_FuncArray_ShouldCreateChain()
    {
        TestContext ctx = new();
        Func<TestContext, Result>[] rules = [c => { c.ExecutedRules.Add("R1"); return Result.Success(); }, c => { c.ExecutedRules.Add("R2"); return Result.Success(); }];
        IRuleBase<TestContext> chain = rules.Select(r => r.ToRule()).ToArray().Linear();
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    #endregion

    #region Complex chain building

    [Fact]
    public void Extension_ComplexChain_IRule_Next_Func_ShouldExecuteBoth()
    {
        TestContext ctx = new();
        IRule<TestContext> chain = new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); })
            .Next(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Extension_ComplexChain_Func_Next_FuncAsync_ShouldExecuteBoth()
    {
        TestContext ctx = new();
        IRule<TestContext> chain = ((Func<TestContext, Result>)(c => { c.ExecutedRules.Add("R1"); return Result.Success(); }))
            .Next((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); });
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Extension_ComplexChain_ThreeRulesViaLinear_ShouldExecuteAll()
    {
        TestContext ctx = new();
        IRule<TestContext>[] rules =
        [
            new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); }),
            new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); }),
            new SimpleRule(c => { c.ExecutedRules.Add("R3"); return Result.Success(); })
        ];
        IRuleBase<TestContext> chain = rules.Linear();
        Result result = RuleEngine.Evaluate(chain, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2", "R3");
    }

    [Fact]
    public void Extension_ComplexChain_TResult_ShouldExecuteAll()
    {
        IRule<TestContext, int> chain = new SimpleRuleT(_ => Result.Success(1))
            .Next(_ => Result.Success(2))
            .Next((_, _) => Result.Success(3));
        Result<int> result = RuleEngine.Evaluate(chain, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(3);
    }

    #endregion
}
