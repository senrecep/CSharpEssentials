using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class ConditionalRuleAdapterTests
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

    #region If with bool condition

    [Fact]
    public void If_BoolCondition_True_ShouldExecuteSuccess()
    {
        TestContext ctx = new();
        IRule<TestContext> success = new TrackingRule("Success", Result.Success());
        IRule<TestContext> failure = new TrackingRule("Failure", Result.Success());
        Result result = RuleEngine.If(true, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Success");
    }

    [Fact]
    public void If_BoolCondition_False_ShouldExecuteFailure()
    {
        TestContext ctx = new();
        IRule<TestContext> success = new TrackingRule("Success", Result.Success());
        IRule<TestContext> failure = new TrackingRule("Failure", Result.Success());
        Result result = RuleEngine.If(false, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Failure");
    }

    #endregion

    #region If with IRuleBase<TContext>

    [Fact]
    public void If_IRule_Success_ShouldExecuteSuccessBranch()
    {
        TestContext ctx = new();
        IRule<TestContext> condition = new TrackingRule("Cond", Result.Success());
        IRule<TestContext> success = new TrackingRule("Success", Result.Success());
        IRule<TestContext> failure = new TrackingRule("Failure", Result.Success());
        Result result = RuleEngine.If(condition, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("Cond", "Success");
        ctx.ExecutedRules.Should().NotContain("Failure");
    }

    [Fact]
    public void If_IRule_Failure_ShouldExecuteFailureBranch()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext> condition = new TrackingRule("Cond", Result.Failure(error));
        IRule<TestContext> success = new TrackingRule("Success", Result.Success());
        IRule<TestContext> failure = new TrackingRule("Failure", Result.Success());
        Result result = RuleEngine.If(condition, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("Cond", "Failure");
        ctx.ExecutedRules.Should().NotContain("Success");
    }

    [Fact]
    public void If_IRule_NullSuccess_ShouldReturnConditionResult()
    {
        TestContext ctx = new();
        IRule<TestContext> condition = new TrackingRule("Cond", Result.Success());
        IRule<TestContext> failure = new TrackingRule("Failure", Result.Success());
        Result result = RuleEngine.If(condition, null!, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("Cond");
    }

    [Fact]
    public void If_IRule_NullFailure_ShouldReturnConditionError()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext> condition = new TrackingRule("Cond", Result.Failure(error));
        IRule<TestContext> success = new TrackingRule("Success", Result.Success());
        Result result = RuleEngine.If(condition, success, null!, ctx);
        result.IsFailure.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainInOrder("Cond");
    }

    [Fact]
    public void If_IRuleTResult_Success_ShouldExecuteSuccessBranch()
    {
        IRule<TestContext, int> condition = new TrackingRuleT("Cond", Result.Success(0));
        IRule<TestContext, int> success = new TrackingRuleT("Success", Result.Success(42));
        IRule<TestContext, int> failure = new TrackingRuleT("Failure", Result.Success(-1));
        Result<int> result = RuleEngine.If(condition, success, failure, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void If_IRuleTResult_Failure_ShouldExecuteFailureBranch()
    {
        var error = Error.Failure("E1", "Fail");
        IRule<TestContext, int> condition = new TrackingRuleT("Cond", Result.Failure<int>(error));
        IRule<TestContext, int> success = new TrackingRuleT("Success", Result.Success(42));
        IRule<TestContext, int> failure = new TrackingRuleT("Failure", Result.Success(-1));
        Result<int> result = RuleEngine.If(condition, success, failure, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-1);
    }

    #endregion

    #region If with Func<TContext, Result>

    [Fact]
    public void If_Func_Success_ShouldExecuteSuccessBranch()
    {
        TestContext ctx = new();
        Func<TestContext, Result> condition = _ => Result.Success();
        Func<TestContext, Result> success = c => { c.ExecutedRules.Add("Success"); return Result.Success(); };
        Func<TestContext, Result> failure = c => { c.ExecutedRules.Add("Failure"); return Result.Success(); };
        Result result = RuleEngine.If(condition, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Success");
    }

    [Fact]
    public void If_Func_Failure_ShouldExecuteFailureBranch()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, Result> condition = _ => Result.Failure(error);
        Func<TestContext, Result> success = c => { c.ExecutedRules.Add("Success"); return Result.Success(); };
        Func<TestContext, Result> failure = c => { c.ExecutedRules.Add("Failure"); return Result.Success(); };
        Result result = RuleEngine.If(condition, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
        ctx.ExecutedRules.Should().ContainSingle().Which.Should().Be("Failure");
    }

    [Fact]
    public void If_FuncTResult_Success_ShouldExecuteSuccessBranch()
    {
        Func<TestContext, Result<int>> condition = _ => Result.Success(0);
        Func<TestContext, Result<int>> success = _ => Result.Success(42);
        Func<TestContext, Result<int>> failure = _ => Result.Success(-1);
        Result<int> result = RuleEngine.If(condition, success, failure, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void If_FuncTResult_Failure_ShouldExecuteFailureBranch()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, Result<int>> condition = _ => Result.Failure<int>(error);
        Func<TestContext, Result<int>> success = _ => Result.Success(42);
        Func<TestContext, Result<int>> failure = _ => Result.Success(-1);
        Result<int> result = RuleEngine.If(condition, success, failure, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-1);
    }

    #endregion

    #region If with Func<TContext, CancellationToken, Result>

    [Fact]
    public void If_FuncWithToken_Success_ShouldPassToken()
    {
        using CancellationTokenSource cts = new();
        CancellationToken capturedCondition = default;
        CancellationToken capturedSuccess = default;
        Func<TestContext, CancellationToken, Result> condition = (_, ct) => { capturedCondition = ct; return Result.Success(); };
        Func<TestContext, CancellationToken, Result> success = (_, ct) => { capturedSuccess = ct; return Result.Success(); };
        Func<TestContext, CancellationToken, Result> failure = (_, _) => Result.Success();
        Result result = RuleEngine.If(condition, success, failure, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        capturedCondition.Should().Be(cts.Token);
        capturedSuccess.Should().Be(cts.Token);
    }

    [Fact]
    public void If_FuncWithTokenTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, CancellationToken, Result<int>> condition = (_, _) => Result.Success(0);
        Func<TestContext, CancellationToken, Result<int>> success = (_, _) => Result.Success(42);
        Func<TestContext, CancellationToken, Result<int>> failure = (_, _) => Result.Success(-1);
        Result<int> result = RuleEngine.If(condition, success, failure, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    #endregion

    #region If with Func<TContext, CancellationToken, ValueTask<Result>>

    [Fact]
    public void If_FuncAsync_Success_ShouldExecuteSuccessBranch()
    {
        TestContext ctx = new();
        Func<TestContext, CancellationToken, ValueTask<Result>> condition = (_, _) => ValueTask.FromResult(Result.Success());
        Func<TestContext, CancellationToken, ValueTask<Result>> success = (_, _) => ValueTask.FromResult(Result.Success());
        Func<TestContext, CancellationToken, ValueTask<Result>> failure = (_, _) => ValueTask.FromResult(Result.Success());
        Result result = RuleEngine.If(condition, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void If_FuncAsync_Failure_ShouldExecuteFailureBranch()
    {
        TestContext ctx = new();
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, CancellationToken, ValueTask<Result>> condition = (_, _) => ValueTask.FromResult(Result.Failure(error));
        Func<TestContext, CancellationToken, ValueTask<Result>> success = (_, _) => ValueTask.FromResult(Result.Success());
        Func<TestContext, CancellationToken, ValueTask<Result>> failure = (_, _) => ValueTask.FromResult(Result.Success());
        Result result = RuleEngine.If(condition, success, failure, ctx);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void If_FuncAsyncTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> condition = (_, _) => ValueTask.FromResult(Result.Success(0));
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> success = (_, _) => ValueTask.FromResult(Result.Success(42));
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> failure = (_, _) => ValueTask.FromResult(Result.Success(-1));
        Result<int> result = RuleEngine.If(condition, success, failure, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void If_FuncAsyncTResult_Failure_ShouldReturnFailureValue()
    {
        var error = Error.Failure("E1", "Fail");
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> condition = (_, _) => ValueTask.FromResult(Result.Failure<int>(error));
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> success = (_, _) => ValueTask.FromResult(Result.Success(42));
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> failure = (_, _) => ValueTask.FromResult(Result.Success(-1));
        Result<int> result = RuleEngine.If(condition, success, failure, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-1);
    }

    #endregion

    #region If with exception

    [Fact]
    public void If_Func_ExceptionInCondition_WithNullFailureBranch_ShouldReturnFailure()
    {
        Func<TestContext, Result> condition = _ => throw new InvalidOperationException("Boom");
        Func<TestContext, Result> success = _ => Result.Success();
        Result result = RuleEngine.If(condition, success, null!, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void If_IRule_ExceptionInCondition_WithNullFailureBranch_ShouldReturnFailure()
    {
        IRule<TestContext> condition = new ExplodingRule();
        IRule<TestContext> success = new TrackingRule("Success", Result.Success());
        Result result = RuleEngine.If(condition, success, null!, new TestContext());
        result.IsFailure.Should().BeTrue();
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
