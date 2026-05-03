using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class AndRuleAdapterTests
{
    private sealed class TestContext
    {
        public List<string> ExecutedRules { get; } = [];
    }

    private sealed class TrackingRule(string name, Result result) : IRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default)
        {
            context.ExecutedRules.Add(name);
            return result;
        }
    }

    private sealed class TrackingRuleT(string name, Result<int> result) : IRule<TestContext, int>
    {
        public Result<int> Evaluate(TestContext context, CancellationToken cancellationToken = default)
        {
            context.ExecutedRules.Add(name);
            return result;
        }
    }

    private sealed class TrackingAsyncRule(string name, Result result) : IAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default)
        {
            context.ExecutedRules.Add(name);
            return ValueTask.FromResult(result);
        }
    }

    private sealed class TrackingAsyncRuleT(string name, Result<int> result) : IAsyncRule<TestContext, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default)
        {
            context.ExecutedRules.Add(name);
            return ValueTask.FromResult(result);
        }
    }

    #region And via IRule<TContext>[]

    [Fact]
    public void And_IRule_Success_ShouldExecuteAllRules()
    {
        TestContext ctx = new();
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Success()), new TrackingRule("R2", Result.Success())];
        Result result = RuleEngine.And(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void And_IRule_Failure_ShouldReturnFailure()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Success()), new TrackingRule("R2", Result.Failure(error))];
        Result result = RuleEngine.And(rules, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void And_IRule_Empty_ShouldReturnEmptyRuleArrayError()
    {
        IRule<TestContext>[] rules = [];
        Result result = RuleEngine.And(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    [Fact]
    public void And_IRuleTResult_Success_ShouldReturnFirstValue()
    {
        TestContext ctx = new();
        IRule<TestContext, int>[] rules = [new TrackingRuleT("R1", Result.Success(10)), new TrackingRuleT("R2", Result.Success(20))];
        Result<int> result = RuleEngine.And(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void And_IRuleTResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext, int>[] rules = [new TrackingRuleT("R1", Result.Success(10)), new TrackingRuleT("R2", Result.Failure<int>(error))];
        Result<int> result = RuleEngine.And(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void And_IAsyncRule_Success_ShouldExecuteAllRules()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext>[] rules = [new TrackingAsyncRule("R1", Result.Success()), new TrackingAsyncRule("R2", Result.Success())];
        IRuleBase<TestContext> adapter = rules.And();
        Result result = RuleEngine.Evaluate(adapter, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void And_IAsyncRuleTResult_Success_ShouldReturnFirstValue()
    {
        IAsyncRule<TestContext, int>[] rules = [new TrackingAsyncRuleT("R1", Result.Success(5)), new TrackingAsyncRuleT("R2", Result.Success(15))];
        IRuleBase<TestContext, int> adapter = rules.And();
        Result<int> result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region And via Func<TContext, Result>[]

    [Fact]
    public void And_Func_Success_ShouldReturnSuccess()
    {
        TestContext ctx = new();
        Func<TestContext, Result>[] rules =
        [
            c => { c.ExecutedRules.Add("R1"); return Result.Success(); },
            c => { c.ExecutedRules.Add("R2"); return Result.Success(); }
        ];
        Result result = RuleEngine.And(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void And_Func_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, Result>[] rules =
        [
            _ => Result.Success(),
            _ => Result.Failure(error)
        ];
        Result result = RuleEngine.And(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void And_FuncTResult_Success_ShouldReturnFirstValue()
    {
        Func<TestContext, Result<int>>[] rules = [_ => Result.Success(1), _ => Result.Success(2)];
        Result<int> result = RuleEngine.And(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    #endregion

    #region And via Func<TContext, CancellationToken, Result>[]

    [Fact]
    public void And_FuncWithToken_Success_ShouldPassToken()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        Func<TestContext, CancellationToken, Result>[] rules =
        [
            (_, ct) => { captured = ct; return Result.Success(); }
        ];
        Result result = RuleEngine.And(rules, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(cts.Token);
    }

    [Fact]
    public void And_FuncWithTokenTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, CancellationToken, Result<int>>[] rules = [(_, _) => Result.Success(33)];
        Result<int> result = RuleEngine.And(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(33);
    }

    #endregion

    #region And via Func<TContext, CancellationToken, ValueTask<Result>>[]

    [Fact]
    public void And_FuncAsync_Success_ShouldReturnSuccess()
    {
        Func<TestContext, CancellationToken, ValueTask<Result>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Success()),
            (_, _) => ValueTask.FromResult(Result.Success())
        ];
        Result result = RuleEngine.And(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void And_FuncAsync_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, CancellationToken, ValueTask<Result>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Success()),
            (_, _) => ValueTask.FromResult(Result.Failure(error))
        ];
        Result result = RuleEngine.And(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void And_FuncAsyncTResult_Success_ShouldReturnFirstValue()
    {
        Func<TestContext, CancellationToken, ValueTask<Result<int>>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Success(7)),
            (_, _) => ValueTask.FromResult(Result.Success(9))
        ];
        Result<int> result = RuleEngine.And(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
    }

    #endregion

    #region And via IRuleBase<TContext> Evaluate directly

    [Fact]
    public void And_EvaluateAdapterDirectly_Exception_ShouldReturnFailure()
    {
        IRule<TestContext>[] rules = [new ExplodingRule()];
        IRuleBase<TestContext> adapter = rules.And();
        Result result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    #endregion

    #region Helpers

    private sealed class ExplodingRule : IRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Kaboom");
    }

    #endregion
}
