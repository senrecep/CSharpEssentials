using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class RuleEngineAggregationTests
{
    private static readonly Error A = Error.Validation("A", "a");
    private static readonly Error B = Error.Validation("B", "b");
    private static readonly Error C = Error.Validation("C", "c");
    private static readonly Error D = Error.Validation("D", "d");

    private sealed class Ctx
    {
        public List<string> Ran { get; } = [];
    }

    private sealed class EmptyOrRule : IOrRule<Ctx>
    {
        public IRuleBase<Ctx>[] Rules => [];
        public Result Evaluate(Ctx context, CancellationToken cancellationToken = default) => Result.Success();
    }

    private sealed class EmptyOrRuleT : IOrRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => [];
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default) => 0;
    }

    private sealed class EmptyAndRuleT : IAndRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => [];
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default) => 0;
    }

    private sealed class OrAsyncRuleT(IRuleBase<Ctx, int>[] rules) : IOrAsyncRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => new(0);
    }

    private sealed class AndAsyncRuleT(IRuleBase<Ctx, int>[] rules) : IAndAsyncRule<Ctx, int>
    {
        public IRuleBase<Ctx, int>[] Rules => rules;
        public ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default) => new(0);
    }

    private static Func<Ctx, Result> Fail(string name, params Error[] errors) => c =>
    {
        c.Ran.Add(name);
        return Result.Failure(errors);
    };

    private static Func<Ctx, Result> Pass(string name) => c =>
    {
        c.Ran.Add(name);
        return Result.Success();
    };

    private static Func<Ctx, Result<int>> FailT(string name, params Error[] errors) => c =>
    {
        c.Ran.Add(name);
        return Result<int>.Failure(errors);
    };

    private static Func<Ctx, Result<int>> PassT(string name, int value) => c =>
    {
        c.Ran.Add(name);
        return value;
    };

    private static Result Run(IRuleBase<Ctx> rule, Ctx ctx, bool viaEvaluateAsync) =>
        viaEvaluateAsync ? RuleEngine.EvaluateAsync(rule, ctx).AsTask().GetAwaiter().GetResult() : RuleEngine.Evaluate(rule, ctx);

    private static Result<int> Run(IRuleBase<Ctx, int> rule, Ctx ctx, bool viaEvaluateAsync) =>
        viaEvaluateAsync ? RuleEngine.EvaluateAsync(rule, ctx).AsTask().GetAwaiter().GetResult() : RuleEngine.Evaluate(rule, ctx);

    private static string[] Codes(Error[] errors) => [.. errors.Select(e => e.Code)];

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Or_AllFail_ShouldAggregateEveryErrorInChildOrder(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx> rule = new[] { Fail("R1", A, B), Fail("R2", C), Fail("R3", D) }.Or();

        Result result = Run(rule, ctx, viaEvaluateAsync);

        Codes(result.Errors).Should().Equal("A", "B", "C", "D");
        ctx.Ran.Should().Equal("R1", "R2", "R3");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Or_SingleFailingChild_ShouldReturnItsErrors(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx> rule = new[] { Fail("R1", A, B) }.Or();

        Result result = Run(rule, ctx, viaEvaluateAsync);

        Codes(result.Errors).Should().Equal("A", "B");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Or_PassAfterFailures_ShouldSucceedAndStop(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx> rule = new[] { Fail("R1", A, B), Fail("R2", C), Pass("R3"), Fail("R4", D) }.Or();

        Result result = Run(rule, ctx, viaEvaluateAsync);

        result.IsSuccess.Should().BeTrue();
        ctx.Ran.Should().Equal("R1", "R2", "R3");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void And_SeveralFailures_ShouldStopAtFirstFailureAndReturnOnlyItsErrors(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx> rule = new[] { Pass("R1"), Fail("R2", A, B), Fail("R3", C), Pass("R4") }.And();

        Result result = Run(rule, ctx, viaEvaluateAsync);

        Codes(result.Errors).Should().Equal("A", "B");
        ctx.Ran.Should().Equal("R1", "R2");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void And_AllPass_ShouldSucceed(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx> rule = new[] { Pass("R1"), Pass("R2") }.And();

        Run(rule, ctx, viaEvaluateAsync).IsSuccess.Should().BeTrue();
        ctx.Ran.Should().Equal("R1", "R2");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GenericOr_AllFail_ShouldAggregateEveryErrorInChildOrder(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx, int> rule = new[] { FailT("R1", A, B), FailT("R2", C), FailT("R3", D) }.Or();

        Result<int> result = Run(rule, ctx, viaEvaluateAsync);

        Codes(result.Errors).Should().Equal("A", "B", "C", "D");
        ctx.Ran.Should().Equal("R1", "R2", "R3");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GenericOr_PassAfterFailures_ShouldReturnPassingValueAndStop(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx, int> rule = new[] { FailT("R1", A), PassT("R2", 42), PassT("R3", 7) }.Or();

        Result<int> result = Run(rule, ctx, viaEvaluateAsync);

        result.Value.Should().Be(42);
        ctx.Ran.Should().Equal("R1", "R2");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GenericAnd_SeveralFailures_ShouldRunEveryChildAndAggregateInOrder(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx, int> rule = new[] { PassT("R1", 1), FailT("R2", A, B), PassT("R3", 3), FailT("R4", C), FailT("R5", D) }.And();

        Result<int> result = Run(rule, ctx, viaEvaluateAsync);

        Codes(result.Errors).Should().Equal("A", "B", "C", "D");
        ctx.Ran.Should().Equal("R1", "R2", "R3", "R4", "R5");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GenericAnd_FirstChildFails_ShouldStillRunEveryChild(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx, int> rule = new[] { FailT("R1", A), PassT("R2", 2), FailT("R3", B, C) }.And();

        Result<int> result = Run(rule, ctx, viaEvaluateAsync);

        Codes(result.Errors).Should().Equal("A", "B", "C");
        ctx.Ran.Should().Equal("R1", "R2", "R3");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GenericAnd_AllPass_ShouldReturnFirstChildValue(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx, int> rule = new[] { PassT("R1", 7), PassT("R2", 8), PassT("R3", 9) }.And();

        Result<int> result = Run(rule, ctx, viaEvaluateAsync);

        result.Value.Should().Be(7);
        ctx.Ran.Should().Equal("R1", "R2", "R3");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GenericAsyncComposites_ShouldAggregateLikeSyncComposites(bool viaEvaluateAsync)
    {
        Ctx ctx = new();
        IRuleBase<Ctx, int>[] children = [FailT("R1", A, B).ToRule(), PassT("R2", 2).ToRule(), FailT("R3", C).ToRule()];

        Result<int> or = Run(new OrAsyncRuleT(children), ctx, viaEvaluateAsync);
        Result<int> and = Run(new AndAsyncRuleT(children), ctx, viaEvaluateAsync);

        or.IsSuccess.Should().BeTrue();
        or.Value.Should().Be(2);
        Codes(and.Errors).Should().Equal("A", "B", "C");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyCustomOr_ShouldReturnWrappedEvaluateError(bool viaEvaluateAsync)
    {
        Result result = Run(new EmptyOrRule(), new Ctx(), viaEvaluateAsync);
        Result<int> generic = Run(new EmptyOrRuleT(), new Ctx(), viaEvaluateAsync);

        result.FirstError.Code.Should().Be("RuleEngine.Evaluate.OrRule");
        generic.FirstError.Code.Should().Be("RuleEngine.Evaluate.OrRule");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyCustomGenericAnd_ShouldReturnEmptyRuleArrayError(bool viaEvaluateAsync)
    {
        Result<int> result = Run(new EmptyAndRuleT(), new Ctx(), viaEvaluateAsync);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("EMPTY_RULE_ARRAY");
    }
}
