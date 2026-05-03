using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class SimpleAdapterTests
{
    private sealed class TestContext
    {
        public List<string> ExecutedRules { get; } = [];
    }

    private sealed class ThrowingRule : IRule<TestContext>
    {
        public Result Evaluate(TestContext context, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Rule explosion");
    }

    private sealed class ThrowingAsyncRule : IAsyncRule<TestContext>
    {
        public ValueTask<Result> EvaluateAsync(TestContext context, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Async rule explosion");
    }

    #region SimpleRuleAdapter via ToRule + Evaluate

    [Fact]
    public void Evaluate_FuncTContextResult_Success_ShouldReturnSuccess()
    {
        Func<TestContext, Result> rule = _ => Result.Success();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_FuncTContextResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("CODE", "Message");
        Func<TestContext, Result> rule = _ => Result.Failure(error);
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("CODE");
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
    public async Task Evaluate_FuncTContextCancellationTokenValueTaskResult_Success_ShouldReturnSuccess()
    {
        Func<TestContext, CancellationToken, ValueTask<Result>> rule = (_, _) => ValueTask.FromResult(Result.Success());
        Result result = await RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Evaluate_FuncTContextCancellationTokenValueTaskResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("CODE", "Message");
        Func<TestContext, CancellationToken, ValueTask<Result>> rule = (_, _) => ValueTask.FromResult(Result.Failure(error));
        Result result = await RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_FuncTContextResult_Exception_ShouldReturnFailure()
    {
        Func<TestContext, Result> rule = _ => throw new InvalidOperationException("Boom");
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task Evaluate_FuncTContextCancellationTokenValueTaskResult_Exception_ShouldReturnFailure()
    {
        Func<TestContext, CancellationToken, ValueTask<Result>> rule = (_, _) => throw new InvalidOperationException("Boom");
        Result result = await RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region SimpleRuleAdapter TResult via ToRule + Evaluate

    [Fact]
    public void Evaluate_FuncTContextResultTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, Result<int>> rule = _ => Result.Success(42);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Evaluate_FuncTContextResultTResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("CODE", "Message");
        Func<TestContext, Result<int>> rule = _ => Result.Failure<int>(error);
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("CODE");
    }

    [Fact]
    public void Evaluate_FuncTContextCancellationTokenResultTResult_Success_ShouldReturnValue()
    {
        using CancellationTokenSource cts = new();
        CancellationToken captured = default;
        Func<TestContext, CancellationToken, Result<int>> rule = (_, ct) => { captured = ct; return Result.Success(42); };
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext(), cts.Token);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        captured.Should().Be(cts.Token);
    }

    [Fact]
    public async Task Evaluate_FuncTContextCancellationTokenValueTaskResultTResult_Success_ShouldReturnValue()
    {
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> rule = (_, _) => ValueTask.FromResult(Result.Success(99));
        Result<int> result = await RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public async Task Evaluate_FuncTContextCancellationTokenValueTaskResultTResult_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("CODE", "Message");
        Func<TestContext, CancellationToken, ValueTask<Result<int>>> rule = (_, _) => ValueTask.FromResult(Result.Failure<int>(error));
        Result<int> result = await RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IRuleBaseTContext_Success_ShouldReturnSuccess()
    {
        IRule<TestContext> rule = new SimpleRule(_ => Result.Success());
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IRuleBaseTContext_Failure_ShouldReturnFailure()
    {
        var error = Error.Failure("CODE", "Message");
        IRule<TestContext> rule = new SimpleRule(_ => Result.Failure(error));
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IRuleBaseTContext_Exception_ShouldReturnFailure()
    {
        IRule<TestContext> rule = new ThrowingRule();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Evaluate_IRuleBaseTContextTResult_Success_ShouldReturnValue()
    {
        IRule<TestContext, int> rule = new SimpleRuleT(_ => Result.Success(77));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(77);
    }

    [Fact]
    public void Evaluate_IAsyncRuleTContext_Success_ShouldReturnSuccess()
    {
        IAsyncRule<TestContext> rule = new SimpleAsyncRule((_, _) => ValueTask.FromResult(Result.Success()));
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAsyncRuleTContext_Exception_ShouldReturnFailure()
    {
        IAsyncRule<TestContext> rule = new ThrowingAsyncRule();
        Result result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IAsyncRuleTContextTResult_Success_ShouldReturnValue()
    {
        IAsyncRule<TestContext, int> rule = new SimpleAsyncRuleT((_, _) => ValueTask.FromResult(Result.Success(88)));
        Result<int> result = RuleEngine.Evaluate(rule, new TestContext());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(88);
    }

    #endregion

    #region Helpers

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

    #endregion
}
