using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class LinearRuleAdapterTests
{
    private sealed class TestContext
    {
        public int Value { get; set; }
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

    private sealed class MutatingRule(string name, int newValue) : IRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default)
        {
            context.ExecutedRules.Add(name);
            context.Value = newValue;
            return Result.Success();
        }
    }

    #region Linear via IRule<TContext>[]

    [Fact]
    public void Linear_IRule_Success_ShouldExecuteAll()
    {
        TestContext ctx = new();
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Success()), new TrackingRule("R2", Result.Success())];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Linear_IRule_Failure_ShouldStopExecution()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Success()), new TrackingRule("R2", Result.Failure(error)), new TrackingRule("R3", Result.Success())];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
        ctx.ExecutedRules.Should().NotContain("R3");
    }

    [Fact]
    public void Linear_IRule_SingleRule_ShouldExecute()
    {
        TestContext ctx = new();
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Success())];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1");
    }

    [Fact]
    public void Linear_IRule_Empty_ShouldThrow()
    {
        IRule<TestContext>[] rules = [];
        Action act = () => RuleEngine.Linear(rules, new TestContext());
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Linear_IRule_Mutation_ShouldPreserveState()
    {
        TestContext ctx = new() { Value = 0 };
        IRule<TestContext>[] rules = [new MutatingRule("R1", 1), new MutatingRule("R2", 2), new MutatingRule("R3", 3)];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.Value.Should().Be(3);
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2", "R3");
    }

    [Fact]
    public void Linear_IRuleTResult_Success_ShouldReturnLastValue()
    {
        TestContext ctx = new();
        IRule<TestContext, int>[] rules = [new TrackingRuleT("R1", Result.Success(10)), new TrackingRuleT("R2", Result.Success(20))];
        Result<int> result = RuleEngine.Linear(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Linear_IRuleTResult_Failure_ShouldStopExecution()
    {
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext, int>[] rules = [new TrackingRuleT("R1", Result.Success(10)), new TrackingRuleT("R2", Result.Failure<int>(error))];
        Result<int> result = RuleEngine.Linear(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Linear via IAsyncRule<TContext>[]

    [Fact]
    public void Linear_IAsyncRule_Success_ShouldExecuteAll()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext>[] rules = [new TrackingAsyncRule("R1", Result.Success()), new TrackingAsyncRule("R2", Result.Success())];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Linear_IAsyncRule_Failure_ShouldStopExecution()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IAsyncRule<TestContext>[] rules = [new TrackingAsyncRule("R1", Result.Success()), new TrackingAsyncRule("R2", Result.Failure(error)), new TrackingAsyncRule("R3", Result.Success())];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
        ctx.ExecutedRules.Should().NotContain("R3");
    }

    [Fact]
    public void Linear_IAsyncRuleTResult_Success_ShouldReturnLastValue()
    {
        IAsyncRule<TestContext, int>[] rules = [new TrackingAsyncRuleT("R1", Result.Success(5)), new TrackingAsyncRuleT("R2", Result.Success(15))];
        Result<int> result = RuleEngine.Linear(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(15);
    }

    #endregion

    #region Linear via Func<TContext, Result>[]

    [Fact]
    public void Linear_Func_Success_ShouldExecuteAll()
    {
        TestContext ctx = new();
        Func<TestContext, Result>[] rules =
        [
            c => { c.ExecutedRules.Add("R1"); return Result.Success(); },
            c => { c.ExecutedRules.Add("R2"); return Result.Success(); }
        ];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Linear_Func_Failure_ShouldStopExecution()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, Result>[] rules =
        [
            c => { c.ExecutedRules.Add("R1"); return Result.Success(); },
            c => { c.ExecutedRules.Add("R2"); return Result.Failure(error); },
            c => { c.ExecutedRules.Add("R3"); return Result.Success(); }
        ];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
        ctx.ExecutedRules.Should().NotContain("R3");
    }

    [Fact]
    public void Linear_FuncTResult_Success_ShouldReturnLastValue()
    {
        Func<TestContext, Result<int>>[] rules = [_ => Result.Success(1), _ => Result.Success(2)];
        Result<int> result = RuleEngine.Linear(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    #endregion

    #region Linear via Func<TContext, CancellationToken, Result>[]

    [Fact]
    public void Linear_FuncWithToken_Success_ShouldPassToken()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        Func<TestContext, CancellationToken, Result>[] rules =
        [
            (_, ct) => { captured = ct; return Result.Success(); },
            (_, _) => Result.Success()
        ];
        Result result = RuleEngine.Linear(rules, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(cts.Token);
    }

    [Fact]
    public void Linear_FuncWithToken_Failure_ShouldStopExecution()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, CancellationToken, Result>[] rules =
        [
            (_, _) => Result.Success(),
            (_, _) => Result.Failure(error),
            (_, _) => Result.Success()
        ];
        Result result = RuleEngine.Linear(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Linear_FuncWithTokenTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, CancellationToken, Result<int>>[] rules = [(_, _) => Result.Success(11), (_, _) => Result.Success(22)];
        Result<int> result = RuleEngine.Linear(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(22);
    }

    #endregion

    #region Linear via Func<TContext, CancellationToken, ValueTask<Result>>[]

    [Fact]
    public void Linear_FuncAsync_Success_ShouldExecuteAll()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, ValueTask<Result>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Success()),
            (_, _) => ValueTask.FromResult(Result.Success())
        ];
        Result result = RuleEngine.Linear(rules, ctx);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Linear_FuncAsync_Failure_ShouldStopExecution()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, CancellationToken, ValueTask<Result>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Success()),
            (_, _) => ValueTask.FromResult(Result.Failure(error)),
            (_, _) => ValueTask.FromResult(Result.Success())
        ];
        Result result = RuleEngine.Linear(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Linear_FuncAsyncTResult_Success_ShouldReturnLastValue()
    {
        Func<TestContext, CancellationToken, ValueTask<Result<int>>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Success(3)),
            (_, _) => ValueTask.FromResult(Result.Success(6))
        ];
        Result<int> result = RuleEngine.Linear(rules, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(6);
    }

    [Fact]
    public void Linear_FuncAsyncTResult_Failure_ShouldStopExecution()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, CancellationToken, ValueTask<Result<int>>>[] rules =
        [
            (_, _) => ValueTask.FromResult(Result.Success(3)),
            (_, _) => ValueTask.FromResult(Result.Failure<int>(error)),
            (_, _) => ValueTask.FromResult(Result.Success(9))
        ];
        Result<int> result = RuleEngine.Linear(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Linear via Evaluate directly with exception

    [Fact]
    public void Linear_EvaluateAdapterDirectly_Exception_ShouldReturnFailure()
    {
        IRule<TestContext>[] rules = [new TrackingRule("R1", Result.Success()), new ExplodingRule()];
        IRuleBase<TestContext> adapter = rules.Linear();
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
