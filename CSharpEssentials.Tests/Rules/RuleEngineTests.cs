using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class RuleEngineTests
{
    private sealed class TestContext
    {
        public List<string> ExecutedRules { get; } = [];
    }

    #region Rule Helpers

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

    private sealed class LinearRule(IRule<TestContext> rule, IRuleBase<TestContext>? next) : ILinearRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default) => rule.Evaluate(context, cancellationToken);
        public IRuleBase<TestContext>? Next => next;
    }

    private sealed class LinearRuleT(IRule<TestContext, int> rule, IRuleBase<TestContext, int>? next) : ILinearRule<TestContext, int>
    {
        public Result<int> Evaluate(TestContext context, CancellationToken cancellationToken = default) => rule.Evaluate(context, cancellationToken);
        public IRuleBase<TestContext, int>? Next => next;
    }

    private sealed class LinearAsyncRule(IAsyncRule<TestContext> rule, IRuleBase<TestContext>? next) : ILinearAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => rule.EvaluateAsync(context, cancellationToken);
        public IRuleBase<TestContext>? Next => next;
    }

    private sealed class LinearAsyncRuleT(IAsyncRule<TestContext, int> rule, IRuleBase<TestContext, int>? next) : ILinearAsyncRule<TestContext, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => rule.EvaluateAsync(context, cancellationToken);
        public IRuleBase<TestContext, int>? Next => next;
    }

    private sealed class ConditionalRule(IRule<TestContext> rule, IRuleBase<TestContext>? success, IRuleBase<TestContext>? failure) : IConditionalRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default) => rule.Evaluate(context, cancellationToken);
        public IRuleBase<TestContext>? Success => success;
        public IRuleBase<TestContext>? Failure => failure;
    }

    private sealed class ConditionalRuleT(IRule<TestContext, int> rule, IRuleBase<TestContext, int>? success, IRuleBase<TestContext, int>? failure) : IConditionalRule<TestContext, int>
    {
        public Result<int> Evaluate(TestContext context, CancellationToken cancellationToken = default) => rule.Evaluate(context, cancellationToken);
        public IRuleBase<TestContext, int>? Success => success;
        public IRuleBase<TestContext, int>? Failure => failure;
    }

    private sealed class ConditionalAsyncRule(IAsyncRule<TestContext> rule, IRuleBase<TestContext>? success, IRuleBase<TestContext>? failure) : IConditionalAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => rule.EvaluateAsync(context, cancellationToken);
        public IRuleBase<TestContext>? Success => success;
        public IRuleBase<TestContext>? Failure => failure;
    }

    private sealed class ConditionalAsyncRuleT(IAsyncRule<TestContext, int> rule, IRuleBase<TestContext, int>? success, IRuleBase<TestContext, int>? failure) : IConditionalAsyncRule<TestContext, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => rule.EvaluateAsync(context, cancellationToken);
        public IRuleBase<TestContext, int>? Success => success;
        public IRuleBase<TestContext, int>? Failure => failure;
    }

    private sealed class AndRule(IRule<TestContext> rule, IRuleBase<TestContext>[] rules) : IAndRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default) => rule.Evaluate(context, cancellationToken);
        public IRuleBase<TestContext>[] Rules => rules;
    }

    private sealed class AndRuleT(IRule<TestContext, int> rule, IRuleBase<TestContext, int>[] rules) : IAndRule<TestContext, int>
    {
        public Result<int> Evaluate(TestContext context, CancellationToken cancellationToken = default) => rule.Evaluate(context, cancellationToken);
        public IRuleBase<TestContext, int>[] Rules => rules;
    }

    private sealed class AndAsyncRule(IAsyncRule<TestContext> rule, IRuleBase<TestContext>[] rules) : IAndAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => rule.EvaluateAsync(context, cancellationToken);
        public IRuleBase<TestContext>[] Rules => rules;
    }

    private sealed class AndAsyncRuleT(IAsyncRule<TestContext, int> rule, IRuleBase<TestContext, int>[] rules) : IAndAsyncRule<TestContext, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => rule.EvaluateAsync(context, cancellationToken);
        public IRuleBase<TestContext, int>[] Rules => rules;
    }

    private sealed class OrRule(IRule<TestContext> rule, IRuleBase<TestContext>[] rules) : IOrRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default) => rule.Evaluate(context, cancellationToken);
        public IRuleBase<TestContext>[] Rules => rules;
    }

    private sealed class OrRuleT(IRule<TestContext, int> rule, IRuleBase<TestContext, int>[] rules) : IOrRule<TestContext, int>
    {
        public Result<int> Evaluate(TestContext context, CancellationToken cancellationToken = default) => rule.Evaluate(context, cancellationToken);
        public IRuleBase<TestContext, int>[] Rules => rules;
    }

    private sealed class OrAsyncRule(IAsyncRule<TestContext> rule, IRuleBase<TestContext>[] rules) : IOrAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => rule.EvaluateAsync(context, cancellationToken);
        public IRuleBase<TestContext>[] Rules => rules;
    }

    private sealed class OrAsyncRuleT(IAsyncRule<TestContext, int> rule, IRuleBase<TestContext, int>[] rules) : IOrAsyncRule<TestContext, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => rule.EvaluateAsync(context, cancellationToken);
        public IRuleBase<TestContext, int>[] Rules => rules;
    }

    private sealed class ExplodingRule : IRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Kaboom");
    }

    private sealed class ExplodingRuleT : IRule<TestContext, int>
    {
        public Result<int> Evaluate(TestContext context, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Kaboom");
    }

    private sealed class ExplodingAsyncRule : IAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Kaboom");
    }

    private sealed class ExplodingAsyncRuleT : IAsyncRule<TestContext, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Kaboom");
    }

    private sealed class UnknownRule : IRuleBase<TestContext>;

    private sealed class CancellingAsyncRule(CancellationToken expectedToken) : IAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.Should().Be(expectedToken);
            return ValueTask.FromResult(Result.Success());
        }
    }

    private sealed class CancellingAsyncRuleT(CancellationToken expectedToken) : IAsyncRule<TestContext, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.Should().Be(expectedToken);
            return ValueTask.FromResult(Result.Success(42));
        }
    }

    #endregion

    #region Evaluate - IRule<TContext>

    [Fact]
    public void Evaluate_IRule_Success_ShouldReturnSuccess()
    {
        IRule<TestContext> rule = new SimpleRule(_ => Result.Success());
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IRule_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext> rule = new SimpleRule(_ => Result.Failure(error));
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public void Evaluate_IRule_Exception_ShouldReturnFailure()
    {
        IRule<TestContext> rule = new ExplodingRule();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    #endregion

    #region Evaluate - IAsyncRule<TContext>

    [Fact]
    public void Evaluate_IAsyncRule_Success_ShouldReturnSuccess()
    {
        IAsyncRule<TestContext> rule = new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success()));
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAsyncRule_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        IAsyncRule<TestContext> rule = new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Failure(error)));
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAsyncRule_Exception_ShouldReturnFailure()
    {
        IAsyncRule<TestContext> rule = new ExplodingAsyncRule();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    #endregion

    #region Evaluate - ILinearRule<TContext>

    [Fact]
    public void Evaluate_ILinearRule_Success_WithNext_ShouldExecuteAll()
    {
        TestContext ctx = new();
        IRule<TestContext> second = new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); });
        ILinearRule<TestContext> rule = new LinearRule(
            new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); }),
            second);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Evaluate_ILinearRule_Success_WithNullNext_ShouldReturnSuccess()
    {
        TestContext ctx = new();
        ILinearRule<TestContext> rule = new LinearRule(
            new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); }),
            null);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Evaluate_ILinearRule_Failure_ShouldStopExecution()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        ILinearRule<TestContext> rule = new LinearRule(
            new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Failure(error); }),
            new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); }));
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Evaluate_ILinearRule_Exception_ShouldReturnFailure()
    {
        ILinearRule<TestContext> rule = new LinearRule(new ExplodingRule(), null);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate - ILinearAsyncRule<TContext>

    [Fact]
    public void Evaluate_ILinearAsyncRule_Success_WithNext_ShouldExecuteAll()
    {
        TestContext ctx = new();
        IAsyncRule<TestContext> second = new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); });
        ILinearAsyncRule<TestContext> rule = new LinearAsyncRule(
            new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); }),
            second);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Evaluate_ILinearAsyncRule_Success_WithNullNext_ShouldReturnSuccess()
    {
        TestContext ctx = new();
        ILinearAsyncRule<TestContext> rule = new LinearAsyncRule(
            new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); }),
            null);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Evaluate_ILinearAsyncRule_Failure_ShouldStopExecution()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        ILinearAsyncRule<TestContext> rule = new LinearAsyncRule(
            new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Failure(error)); }),
            new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); }));
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Evaluate_ILinearAsyncRule_Exception_ShouldReturnFailure()
    {
        ILinearAsyncRule<TestContext> rule = new LinearAsyncRule(new ExplodingAsyncRule(), null);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate - IConditionalRule<TContext>

    [Fact]
    public void Evaluate_IConditionalRule_Success_WithSuccessBranch_ShouldExecuteSuccess()
    {
        TestContext ctx = new();
        IConditionalRule<TestContext> rule = new ConditionalRule(
            new SimpleRule(_ => Result.Success()),
            new SimpleRule(c => { c.ExecutedRules.Add("Success"); return Result.Success(); }),
            new SimpleRule(c => { c.ExecutedRules.Add("Failure"); return Result.Success(); }));
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Success");
    }

    [Fact]
    public void Evaluate_IConditionalRule_Success_WithNullSuccess_ShouldReturnConditionResult()
    {
        TestContext ctx = new();
        IConditionalRule<TestContext> rule = new ConditionalRule(
            new SimpleRule(_ => Result.Success()),
            null,
            new SimpleRule(c => { c.ExecutedRules.Add("Failure"); return Result.Success(); }));
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_IConditionalRule_Failure_WithFailureBranch_ShouldExecuteFailure()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IConditionalRule<TestContext> rule = new ConditionalRule(
            new SimpleRule(_ => Result.Failure(error)),
            new SimpleRule(c => { c.ExecutedRules.Add("Success"); return Result.Success(); }),
            new SimpleRule(c => { c.ExecutedRules.Add("Failure"); return Result.Success(); }));
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Failure");
    }

    [Fact]
    public void Evaluate_IConditionalRule_Failure_WithNullFailure_ShouldReturnConditionError()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IConditionalRule<TestContext> rule = new ConditionalRule(
            new SimpleRule(_ => Result.Failure(error)),
            new SimpleRule(_ => Result.Success()),
            null);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public void Evaluate_IConditionalRule_Exception_ShouldReturnFailure()
    {
        IConditionalRule<TestContext> rule = new ConditionalRule(new ExplodingRule(), null, null);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate - IConditionalAsyncRule<TContext>

    [Fact]
    public void Evaluate_IConditionalAsyncRule_Success_WithSuccessBranch_ShouldExecuteSuccess()
    {
        TestContext ctx = new();
        IConditionalAsyncRule<TestContext> rule = new ConditionalAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("Success"); return ValueTask.FromResult(Result.Success()); }),
            new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("Failure"); return ValueTask.FromResult(Result.Success()); }));
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Success");
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRule_Success_WithNullSuccess_ShouldReturnConditionResult()
    {
        IConditionalAsyncRule<TestContext> rule = new ConditionalAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            null,
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())));
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRule_Failure_WithFailureBranch_ShouldExecuteFailure()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IConditionalAsyncRule<TestContext> rule = new ConditionalAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Failure(error))),
            new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("Success"); return ValueTask.FromResult(Result.Success()); }),
            new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("Failure"); return ValueTask.FromResult(Result.Success()); }));
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Failure");
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRule_Failure_WithNullFailure_ShouldReturnConditionError()
    {
        var error = Error.Failure("E1", "Fail");
        IConditionalAsyncRule<TestContext> rule = new ConditionalAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Failure(error))),
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            null);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRule_Exception_ShouldReturnFailure()
    {
        IConditionalAsyncRule<TestContext> rule = new ConditionalAsyncRule(new ExplodingAsyncRule(), null, null);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate - IAndRule<TContext>

    [Fact]
    public void Evaluate_IAndRule_Success_ShouldExecuteAllRules()
    {
        TestContext ctx = new();
        IAndRule<TestContext> rule = new AndRule(
            new SimpleRule(_ => Result.Success()),
            [
                new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); }),
                new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); })
            ]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Evaluate_IAndRule_Failure_ShouldReturnFailure()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IAndRule<TestContext> rule = new AndRule(
            new SimpleRule(_ => Result.Success()),
            [
                new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); }),
                new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Failure(error); })
            ]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Evaluate_IAndRule_MainRuleFailure_ShouldReturnImmediately()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IAndRule<TestContext> rule = new AndRule(
            new SimpleRule(_ => Result.Failure(error)),
            [new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); })]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_IAndRule_Exception_ShouldReturnFailure()
    {
        IAndRule<TestContext> rule = new AndRule(new SimpleRule(_ => Result.Success()), [new ExplodingRule()]);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate - IAndAsyncRule<TContext>

    [Fact]
    public void Evaluate_IAndAsyncRule_Success_ShouldExecuteAllRules()
    {
        TestContext ctx = new();
        IAndAsyncRule<TestContext> rule = new AndAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            [
                new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); }),
                new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); })
            ]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Evaluate_IAndAsyncRule_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        IAndAsyncRule<TestContext> rule = new AndAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            [
                new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
                new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Failure(error)))
            ]);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAndAsyncRule_Exception_ShouldReturnFailure()
    {
        IAndAsyncRule<TestContext> rule = new AndAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            [new ExplodingAsyncRule()]);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate - IOrRule<TContext>

    [Fact]
    public void Evaluate_IOrRule_FirstSuccess_ShouldReturnSuccess()
    {
        TestContext ctx = new();
        IOrRule<TestContext> rule = new OrRule(
            new SimpleRule(_ => Result.Success()),
            [
                new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); }),
                new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); })
            ]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Evaluate_IOrRule_SecondSuccess_ShouldReturnSuccess()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IOrRule<TestContext> rule = new OrRule(
            new SimpleRule(_ => Result.Success()),
            [
                new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Failure(error); }),
                new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Success(); })
            ]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("R1", "R2");
    }

    [Fact]
    public void Evaluate_IOrRule_AllFailure_ShouldReturnFailure()
    {
        TestContext ctx = new();
        var e1 = Error.Failure("E1", "Fail1");
        var e2 = Error.Failure("E2", "Fail2");
        IOrRule<TestContext> rule = new OrRule(
            new SimpleRule(_ => Result.Success()),
            [
                new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Failure(e1); }),
                new SimpleRule(c => { c.ExecutedRules.Add("R2"); return Result.Failure(e2); })
            ]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Evaluate_IOrRule_MainRuleFailure_ShouldReturnImmediately()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IOrRule<TestContext> rule = new OrRule(
            new SimpleRule(_ => Result.Failure(error)),
            [new SimpleRule(c => { c.ExecutedRules.Add("R1"); return Result.Success(); })]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_IOrRule_Exception_ShouldReturnFailure()
    {
        IOrRule<TestContext> rule = new OrRule(new SimpleRule(_ => Result.Success()), [new ExplodingRule()]);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate - IOrAsyncRule<TContext>

    [Fact]
    public void Evaluate_IOrAsyncRule_FirstSuccess_ShouldReturnSuccess()
    {
        TestContext ctx = new();
        IOrAsyncRule<TestContext> rule = new OrAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            [
                new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R1"); return ValueTask.FromResult(Result.Success()); }),
                new SimpleAsyncRule((c, _) => { c.ExecutedRules.Add("R2"); return ValueTask.FromResult(Result.Success()); })
            ]);
        Result result = RuleEngine.Evaluate(rule, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("R1");
    }

    [Fact]
    public void Evaluate_IOrAsyncRule_SecondSuccess_ShouldReturnSuccess()
    {
        var error = Error.Failure("E1", "Fail");
        IOrAsyncRule<TestContext> rule = new OrAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            [
                new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Failure(error))),
                new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success()))
            ]);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IOrAsyncRule_AllFailure_ShouldReturnFailure()
    {
        var e1 = Error.Failure("E1", "Fail1");
        var e2 = Error.Failure("E2", "Fail2");
        IOrAsyncRule<TestContext> rule = new OrAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            [
                new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Failure(e1))),
                new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Failure(e2)))
            ]);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IOrAsyncRule_Exception_ShouldReturnFailure()
    {
        IOrAsyncRule<TestContext> rule = new OrAsyncRule(
            new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success())),
            [new ExplodingAsyncRule()]);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - IRule<TContext, TResult>

    [Fact]
    public void Evaluate_IRuleTResult_Success_ShouldReturnValue()
    {
        IRule<TestContext, int> rule = new SimpleRuleT(_ => Result.Success(42));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Evaluate_IRuleTResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext, int> rule = new SimpleRuleT(_ => Result.Failure<int>(error));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IRuleTResult_Exception_ShouldReturnFailure()
    {
        IRule<TestContext, int> rule = new ExplodingRuleT();
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - IAsyncRule<TContext, TResult>

    [Fact]
    public void Evaluate_IAsyncRuleTResult_Success_ShouldReturnValue()
    {
        IAsyncRule<TestContext, int> rule = new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(99)));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public void Evaluate_IAsyncRuleTResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        IAsyncRule<TestContext, int> rule = new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Failure<int>(error)));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAsyncRuleTResult_Exception_ShouldReturnFailure()
    {
        IAsyncRule<TestContext, int> rule = new ExplodingAsyncRuleT();
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - ILinearRule<TContext, TResult>

    [Fact]
    public void Evaluate_ILinearRuleTResult_Success_WithNext_ShouldReturnLastValue()
    {
        IRule<TestContext, int> second = new SimpleRuleT(_ => Result.Success(20));
        ILinearRule<TestContext, int> rule = new LinearRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            second);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Evaluate_ILinearRuleTResult_Success_WithNullNext_ShouldReturnValue()
    {
        ILinearRule<TestContext, int> rule = new LinearRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            null);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Evaluate_ILinearRuleTResult_Failure_ShouldStopExecution()
    {
        var error = Error.Failure("E1", "Fail");
        ILinearRule<TestContext, int> rule = new LinearRuleT(
            new SimpleRuleT(_ => Result.Failure<int>(error)),
            new SimpleRuleT(_ => Result.Success(20)));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ILinearRuleTResult_Exception_ShouldReturnFailure()
    {
        ILinearRule<TestContext, int> rule = new LinearRuleT(new ExplodingRuleT(), null);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - ILinearAsyncRule<TContext, TResult>

    [Fact]
    public void Evaluate_ILinearAsyncRuleTResult_Success_WithNext_ShouldReturnLastValue()
    {
        IAsyncRule<TestContext, int> second = new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(30)));
        ILinearAsyncRule<TestContext, int> rule = new LinearAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(10))),
            second);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(30);
    }

    [Fact]
    public void Evaluate_ILinearAsyncRuleTResult_Success_WithNullNext_ShouldReturnValue()
    {
        ILinearAsyncRule<TestContext, int> rule = new LinearAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(10))),
            null);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Evaluate_ILinearAsyncRuleTResult_Failure_ShouldStopExecution()
    {
        var error = Error.Failure("E1", "Fail");
        ILinearAsyncRule<TestContext, int> rule = new LinearAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Failure<int>(error))),
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(20))));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ILinearAsyncRuleTResult_Exception_ShouldReturnFailure()
    {
        ILinearAsyncRule<TestContext, int> rule = new LinearAsyncRuleT(new ExplodingAsyncRuleT(), null);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - IConditionalRule<TContext, TResult>

    [Fact]
    public void Evaluate_IConditionalRuleTResult_Success_WithSuccessBranch_ShouldReturnSuccessValue()
    {
        IConditionalRule<TestContext, int> rule = new ConditionalRuleT(
            new SimpleRuleT(_ => Result.Success(0)),
            new SimpleRuleT(_ => Result.Success(42)),
            new SimpleRuleT(_ => Result.Success(-1)));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Evaluate_IConditionalRuleTResult_Success_WithNullSuccess_ShouldReturnConditionValue()
    {
        IConditionalRule<TestContext, int> rule = new ConditionalRuleT(
            new SimpleRuleT(_ => Result.Success(77)),
            null,
            new SimpleRuleT(_ => Result.Success(-1)));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(77);
    }

    [Fact]
    public void Evaluate_IConditionalRuleTResult_Failure_WithFailureBranch_ShouldReturnFailureValue()
    {
        var error = Error.Failure("E1", "Fail");
        IConditionalRule<TestContext, int> rule = new ConditionalRuleT(
            new SimpleRuleT(_ => Result.Failure<int>(error)),
            new SimpleRuleT(_ => Result.Success(42)),
            new SimpleRuleT(_ => Result.Success(-1)));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-1);
    }

    [Fact]
    public void Evaluate_IConditionalRuleTResult_Failure_WithNullFailure_ShouldReturnConditionError()
    {
        var error = Error.Failure("E1", "Fail");
        IConditionalRule<TestContext, int> rule = new ConditionalRuleT(
            new SimpleRuleT(_ => Result.Failure<int>(error)),
            new SimpleRuleT(_ => Result.Success(42)),
            null);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public void Evaluate_IConditionalRuleTResult_Exception_ShouldReturnFailure()
    {
        IConditionalRule<TestContext, int> rule = new ConditionalRuleT(new ExplodingRuleT(), null, null);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - IConditionalAsyncRule<TContext, TResult>

    [Fact]
    public void Evaluate_IConditionalAsyncRuleTResult_Success_WithSuccessBranch_ShouldReturnSuccessValue()
    {
        IConditionalAsyncRule<TestContext, int> rule = new ConditionalAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(0))),
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(42))),
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(-1))));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRuleTResult_Success_WithNullSuccess_ShouldReturnConditionValue()
    {
        IConditionalAsyncRule<TestContext, int> rule = new ConditionalAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(88))),
            null,
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(-1))));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(88);
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRuleTResult_Failure_WithFailureBranch_ShouldReturnFailureValue()
    {
        var error = Error.Failure("E1", "Fail");
        IConditionalAsyncRule<TestContext, int> rule = new ConditionalAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Failure<int>(error))),
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(42))),
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(-1))));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-1);
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRuleTResult_Failure_WithNullFailure_ShouldReturnConditionError()
    {
        var error = Error.Failure("E1", "Fail");
        IConditionalAsyncRule<TestContext, int> rule = new ConditionalAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Failure<int>(error))),
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(42))),
            null);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRuleTResult_Exception_ShouldReturnFailure()
    {
        IConditionalAsyncRule<TestContext, int> rule = new ConditionalAsyncRuleT(new ExplodingAsyncRuleT(), null, null);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - IAndRule<TContext, TResult>

    [Fact]
    public void Evaluate_IAndRuleTResult_Success_ShouldReturnFirstValue()
    {
        IAndRule<TestContext, int> rule = new AndRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            [
                new SimpleRuleT(_ => Result.Success(20)),
                new SimpleRuleT(_ => Result.Success(30))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Evaluate_IAndRuleTResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        IAndRule<TestContext, int> rule = new AndRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            [
                new SimpleRuleT(_ => Result.Success(20)),
                new SimpleRuleT(_ => Result.Failure<int>(error))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAndRuleTResult_MainRuleFailure_ShouldReturnImmediately()
    {
        var error = Error.Failure("E1", "Fail");
        IAndRule<TestContext, int> rule = new AndRuleT(
            new SimpleRuleT(_ => Result.Failure<int>(error)),
            [new SimpleRuleT(_ => Result.Success(20))]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAndRuleTResult_Exception_ShouldReturnFailure()
    {
        IAndRule<TestContext, int> rule = new AndRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            [new ExplodingRuleT()]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - IAndAsyncRule<TContext, TResult>

    [Fact]
    public void Evaluate_IAndAsyncRuleTResult_Success_ShouldReturnFirstValue()
    {
        IAndAsyncRule<TestContext, int> rule = new AndAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(5))),
            [
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(15))),
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(25)))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(15);
    }

    [Fact]
    public void Evaluate_IAndAsyncRuleTResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("E1", "Fail");
        IAndAsyncRule<TestContext, int> rule = new AndAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(5))),
            [
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(15))),
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Failure<int>(error)))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAndAsyncRuleTResult_Exception_ShouldReturnFailure()
    {
        IAndAsyncRule<TestContext, int> rule = new AndAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(5))),
            [new ExplodingAsyncRuleT()]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - IOrRule<TContext, TResult>

    [Fact]
    public void Evaluate_IOrRuleTResult_FirstSuccess_ShouldReturnValue()
    {
        IOrRule<TestContext, int> rule = new OrRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            [
                new SimpleRuleT(_ => Result.Success(20)),
                new SimpleRuleT(_ => Result.Success(30))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Evaluate_IOrRuleTResult_SecondSuccess_ShouldReturnValue()
    {
        var error = Error.Failure("E1", "Fail");
        IOrRule<TestContext, int> rule = new OrRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            [
                new SimpleRuleT(_ => Result.Failure<int>(error)),
                new SimpleRuleT(_ => Result.Success(30))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(30);
    }

    [Fact]
    public void Evaluate_IOrRuleTResult_AllFailure_ShouldReturnFailure()
    {
        var e1 = Error.Failure("E1", "Fail1");
        var e2 = Error.Failure("E2", "Fail2");
        IOrRule<TestContext, int> rule = new OrRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            [
                new SimpleRuleT(_ => Result.Failure<int>(e1)),
                new SimpleRuleT(_ => Result.Failure<int>(e2))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Evaluate_IOrRuleTResult_MainRuleFailure_ShouldReturnImmediately()
    {
        var error = Error.Failure("E1", "Fail");
        IOrRule<TestContext, int> rule = new OrRuleT(
            new SimpleRuleT(_ => Result.Failure<int>(error)),
            [new SimpleRuleT(_ => Result.Success(20))]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IOrRuleTResult_Exception_ShouldReturnFailure()
    {
        IOrRule<TestContext, int> rule = new OrRuleT(
            new SimpleRuleT(_ => Result.Success(10)),
            [new ExplodingRuleT()]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate<TResult> - IOrAsyncRule<TContext, TResult>

    [Fact]
    public void Evaluate_IOrAsyncRuleTResult_FirstSuccess_ShouldReturnValue()
    {
        IOrAsyncRule<TestContext, int> rule = new OrAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(5))),
            [
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(15))),
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(25)))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(15);
    }

    [Fact]
    public void Evaluate_IOrAsyncRuleTResult_SecondSuccess_ShouldReturnValue()
    {
        var error = Error.Failure("E1", "Fail");
        IOrAsyncRule<TestContext, int> rule = new OrAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(5))),
            [
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Failure<int>(error))),
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(25)))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(25);
    }

    [Fact]
    public void Evaluate_IOrAsyncRuleTResult_AllFailure_ShouldReturnFailure()
    {
        var e1 = Error.Failure("E1", "Fail1");
        var e2 = Error.Failure("E2", "Fail2");
        IOrAsyncRule<TestContext, int> rule = new OrAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(5))),
            [
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Failure<int>(e1))),
                new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Failure<int>(e2)))
            ]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IOrAsyncRuleTResult_Exception_ShouldReturnFailure()
    {
        IOrAsyncRule<TestContext, int> rule = new OrAsyncRuleT(
            new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(5))),
            [new ExplodingAsyncRuleT()]);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Evaluate - Unknown and Null

    [Fact]
    public void Evaluate_UnknownRuleType_ShouldReturnNotFoundError()
    {
        IRuleBase<TestContext> rule = new UnknownRule();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Evaluate_NullRule_ShouldThrowNullReferenceException()
    {
        IRuleBase<TestContext>? rule = null;
        Action act = () => RuleEngine.Evaluate(rule!, new TestContext());
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void EvaluateTResult_UnknownRuleType_ShouldReturnNotFoundError()
    {
        IRuleBase<TestContext, int> rule = new UnknownRuleT();
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void EvaluateTResult_NullRule_ShouldThrowNullReferenceException()
    {
        IRuleBase<TestContext, int>? rule = null;
        Action act = () => RuleEngine.Evaluate(rule!, new TestContext());
        act.Should().Throw<NullReferenceException>();
    }

    private sealed class UnknownRuleT : IRuleBase<TestContext, int>;

    #endregion

    #region Cancellation Token Propagation

    [Fact]
    public void Evaluate_IAsyncRule_ShouldPassCancellationToken()
    {
        using CancellationTokenSource cts = new();
        IAsyncRule<TestContext> rule = new CancellingAsyncRule(cts.Token);
        Result result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAsyncRuleTResult_ShouldPassCancellationToken()
    {
        using CancellationTokenSource cts = new();
        IAsyncRule<TestContext, int> rule = new CancellingAsyncRuleT(cts.Token);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Evaluate_ILinearAsyncRule_ShouldPassCancellationToken()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        ILinearAsyncRule<TestContext> rule = new LinearAsyncRule(
            new SimpleAsyncRule((_, ct) => { captured = ct; return ValueTask.FromResult(Result.Success()); }),
            null);
        Result result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(cts.Token);
    }

    [Fact]
    public void Evaluate_IConditionalAsyncRule_ShouldPassCancellationToken()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        IConditionalAsyncRule<TestContext> rule = new ConditionalAsyncRule(
            new SimpleAsyncRule((_, ct) => { captured = ct; return ValueTask.FromResult(Result.Success()); }),
            null,
            null);
        Result result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(cts.Token);
    }

    [Fact]
    public void Evaluate_IAndAsyncRule_ShouldPassCancellationToken()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        IAndAsyncRule<TestContext> rule = new AndAsyncRule(
            new SimpleAsyncRule((_, ct) => { captured = ct; return ValueTask.FromResult(Result.Success()); }),
            []);
        Result result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(cts.Token);
    }

    [Fact]
    public void Evaluate_IOrAsyncRule_ShouldPassCancellationToken()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        IOrAsyncRule<TestContext> rule = new OrAsyncRule(
            new SimpleAsyncRule((_, ct) => { captured = ct; return ValueTask.FromResult(Result.Success()); }),
            [new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success()))]);
        Result result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(cts.Token);
    }

    #endregion

    #region And - Empty Arrays

    [Fact]
    public void And_IRule_Empty_ShouldReturnEmptyRuleArrayError()
    {
        IRule<TestContext>[] rules = [];
        Result result = RuleEngine.And(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    [Fact]
    public void And_IRuleTResult_Empty_ShouldReturnEmptyRuleArrayError()
    {
        IRule<TestContext, int>[] rules = [];
        Result<int> result = RuleEngine.And(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    [Fact]
    public void And_Func_Empty_ShouldReturnEmptyRuleArrayError()
    {
        Func<TestContext, Result>[] rules = [];
        Result result = RuleEngine.And(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    #endregion

    #region Or - Empty Arrays

    [Fact]
    public void Or_IRule_Empty_ShouldReturnEmptyRuleArrayError()
    {
        IRule<TestContext>[] rules = [];
        Result result = RuleEngine.Or(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    [Fact]
    public void Or_IRuleTResult_Empty_ShouldReturnEmptyRuleArrayError()
    {
        IRule<TestContext, int>[] rules = [];
        Result<int> result = RuleEngine.Or(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    [Fact]
    public void Or_Func_Empty_ShouldReturnEmptyRuleArrayError()
    {
        Func<TestContext, Result>[] rules = [];
        Result result = RuleEngine.Or(rules, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    #endregion

    #region Linear - Empty Arrays

    [Fact]
    public void Linear_IRule_Empty_ShouldThrow()
    {
        IRule<TestContext>[] rules = [];
        Action act = () => RuleEngine.Linear(rules, new TestContext());
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Linear_Func_Empty_ShouldThrow()
    {
        Func<TestContext, Result>[] rules = [];
        Action act = () => RuleEngine.Linear(rules, new TestContext());
        act.Should().Throw<Exception>();
    }

    #endregion

    #region If - Bool Condition

    [Fact]
    public void If_BoolCondition_True_ShouldExecuteSuccess()
    {
        TestContext ctx = new();
        IRule<TestContext> success = new SimpleRule(c => { c.ExecutedRules.Add("Success"); return Result.Success(); });
        IRule<TestContext> failure = new SimpleRule(c => { c.ExecutedRules.Add("Failure"); return Result.Success(); });
        Result result = RuleEngine.If(true, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Success");
    }

    [Fact]
    public void If_BoolCondition_False_ShouldExecuteFailure()
    {
        TestContext ctx = new();
        IRule<TestContext> success = new SimpleRule(c => { c.ExecutedRules.Add("Success"); return Result.Success(); });
        IRule<TestContext> failure = new SimpleRule(c => { c.ExecutedRules.Add("Failure"); return Result.Success(); });
        Result result = RuleEngine.If(false, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Failure");
    }

    #endregion

    #region If - Null Branches

    [Fact]
    public void If_IRule_NullSuccess_ShouldReturnConditionResult()
    {
        TestContext ctx = new();
        IRule<TestContext> condition = new SimpleRule(c => { c.ExecutedRules.Add("Cond"); return Result.Success(); });
        IRule<TestContext> failure = new SimpleRule(c => { c.ExecutedRules.Add("Failure"); return Result.Success(); });
        Result result = RuleEngine.If(condition, null!, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Cond");
    }

    [Fact]
    public void If_IRule_NullFailure_ShouldReturnConditionError()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext> condition = new SimpleRule(c => { c.ExecutedRules.Add("Cond"); return Result.Failure(error); });
        IRule<TestContext> success = new SimpleRule(c => { c.ExecutedRules.Add("Success"); return Result.Success(); });
        Result result = RuleEngine.If(condition, success, null!, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Cond");
    }

    #endregion

    #region Evaluate - Func Overloads

    [Fact]
    public void Evaluate_FuncTContextResult_Success_ShouldReturnSuccess()
    {
        Func<TestContext, Result> rule = _ => Result.Success();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_FuncTContextCancellationTokenResult_Success_ShouldReturnSuccess()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        Func<TestContext, CancellationToken, Result> rule = (_, ct) => { captured = ct; return Result.Success(); };
        Result result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be(cts.Token);
    }

    [Fact]
    public async Task Evaluate_FuncAsyncResult_Success_ShouldReturnSuccess()
    {
        Func<TestContext, CancellationToken, ValueTask<Result>> rule = (_, _) => ValueTask.FromResult(Result.Success());
        Result result = await RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_FuncTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, Result<int>> rule = _ => Result.Success(42);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Evaluate_FuncTContextCancellationTokenResultTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, CancellationToken, Result<int>> rule = (_, _) => Result.Success(77);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(77);
    }

    [Fact]
    public async Task Evaluate_FuncAsyncResultTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> rule = (_, _) => ValueTask.FromResult(Result.Success(99));
        Result<int> result = await RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion
}
