using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class OrRuleAdapterTests
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

    #region Or via IRule<TContext>[]

    [Fact]
    public void Or_IRule_FirstSuccess_ShouldExecuteOnlyFirst()
    {
        TestContext ctx = new();
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Success()), new TrackingRule("R2", Result.Success())];
        Result result = RuleEngine.Or(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Or_IRule_SecondSuccess_ShouldExecuteUntilSuccess()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Failure(error)), new TrackingRule("R2", Result.Success())];
        Result result = RuleEngine.Or(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Or_IRule_AllFailure_ShouldReturnAllErrors()
    {
        TestContext ctx = new();
        var e1 = Error.Failure("E1", "Fail1");
        var e2 = Error.Failure("E2", "Fail2");
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Failure(e1)), new TrackingRule("R2", Result.Failure(e2))];
        Result result = RuleEngine.Or(rules, ctx);
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Or_IRule_Empty_ShouldReturnEmptyRuleArrayError()
    {
        IRule<TestContext>[] rules = [];
        Result result = RuleEngine.Or(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    [Fact]
    public void Or_IRuleTResult_FirstSuccess_ShouldReturnValue()
    {
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext, int>[] rules = [new TrackingRuleT("R1", Result.Success(10)), new TrackingRuleT("R2", Result.Failure<int>(error))];
        Result<int> result = RuleEngine.Or(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Or_IRuleTResult_SecondSuccess_ShouldReturnValue()
    {
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext, int>[] rules = [new TrackingRuleT("R1", Result.Failure<int>(error)), new TrackingRuleT("R2", Result.Success(20))];
        Result<int> result = RuleEngine.Or(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Or_IAsyncRule_Success_ShouldReturnSuccess()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext>[] rules = [new TrackingAsyncRule("R1", Result.Success()), new TrackingAsyncRule("R2", Result.Success())];
        IRuleBase<TestContext> adapter = rules.Or();
        Result result = RuleEngine.Evaluate(adapter, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Or_IAsyncRuleTResult_SecondSuccess_ShouldReturnValue()
    {
        var error = Error.Failure("E1", "Fail");
        IAsyncRule<TestContext, int>[] rules = [new TrackingAsyncRuleT("R1", Result.Failure<int>(error)), new TrackingAsyncRuleT("R2", Result.Success(42))];
        IRuleBase<TestContext, int> adapter = rules.Or();
        Result<int> result = RuleEngine.Evaluate(adapter, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    #endregion

    #region Or via Func<TContext, Result>[]

    [Fact]
    public void Or_Func_FirstSuccess_ShouldReturnSuccess()
    {
        TestContext ctx = new();
        Func<TestContext, Result>[] rules =
        [
            c => { c.ExecutedRules.Add("R1"); return Result.Success(); },
            c => { c.ExecutedRules.Add("R2"); return Result.Success(); }
        ];
        Result result = RuleEngine.Or(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Or_Func_SecondSuccess_ShouldReturnSuccess()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, Result>[] rules =
        [
            _ => Result.Failure(error),
            _ => Result.Success()
        ];
        Result result = RuleEngine.Or(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Or_Func_AllFailure_ShouldReturnFailure()
    {
        var e1 = Error.Failure("E1", "Fail1");
        var e2 = Error.Failure("E2", "Fail2");
        Func<TestContext, Result>[] rules = [_ => Result.Failure(e1), _ => Result.Failure(e2)];
        Result result = RuleEngine.Or(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Or_FuncTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, Result<int>>[] rules = [_ => Result.Success(100), _ => Result.Success(200)];
        Result<int> result = RuleEngine.Or(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(100);
    }

    #endregion

    #region Or via Func<TContext, CancellationToken, Result>[]

    [Fact]
    public void Or_FuncWithToken_SecondSuccess_ShouldReturnSuccess()
    {
        using CancellationTokenSource cts = new();
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, CancellationToken, Result>[] rules =
        [
            (_, _) => Result.Failure(error),
            (_, _) => Result.Success()
        ];
        Result result = RuleEngine.Or(rules, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Or_FuncWithTokenTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, CancellationToken, Result<int>>[] rules = [(_, _) => Result.Success(55)];
        Result<int> result = RuleEngine.Or(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(55);
    }

    #endregion

    #region Or via Func<TContext, CancellationToken, ValueTask<Result>>[]

    [Fact]
    public void Or_FuncAsync_FirstSuccess_ShouldReturnSuccess()
    {
        Func<TestContext, CancellationToken, ValueTask<Result>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Success()),
            (_, _) => ValueTask.FromResult(Result.Success())
        ];
        Result result = RuleEngine.Or(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Or_FuncAsync_AllFailure_ShouldReturnFailure()
    {
        var e1 = Error.Failure("E1", "Fail1");
        var e2 = Error.Failure("E2", "Fail2");
        Func<TestContext, CancellationToken, ValueTask<Result>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Failure(e1)),
            (_, _) => ValueTask.FromResult(Result.Failure(e2))
        ];
        Result result = RuleEngine.Or(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Or_FuncAsyncTResult_SecondSuccess_ShouldReturnValue()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, CancellationToken, ValueTask<Result<int>>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Failure<int>(error)),
            (_, _) => ValueTask.FromResult(Result.Success(66))
        ];
        Result<int> result = RuleEngine.Or(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(66);
    }

    #endregion

    #region Or via IRuleBase<TContext> Evaluate directly

    [Fact]
    public void Or_EvaluateAdapterDirectly_Exception_ShouldReturnFailure()
    {
        IRule<TestContext>[] rules = [new ExplodingRule()];
        IRuleBase<TestContext> adapter = rules.Or();
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
