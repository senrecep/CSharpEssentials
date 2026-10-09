using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class NextChainTests
{
    private sealed class Ctx
    {
        public List<string> Executed { get; } = [];
    }

    private sealed class SyncRule(string name, Result result) : IRule<Ctx>
    {
        public Result Evaluate(Ctx context, CancellationToken cancellationToken = default)
        {
            context.Executed.Add(name);
            return result;
        }
    }

    private sealed class AsyncRule(string name, Result result) : IAsyncRule<Ctx>
    {
        public ValueTask<Result> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default)
        {
            context.Executed.Add(name);
            return ValueTask.FromResult(result);
        }
    }

    private sealed class SyncRuleT(string name, Result<int> result) : IRule<Ctx, int>
    {
        public Result<int> Evaluate(Ctx context, CancellationToken cancellationToken = default)
        {
            context.Executed.Add(name);
            return result;
        }
    }

    private sealed class AsyncRuleT(string name, Result<int> result) : IAsyncRule<Ctx, int>
    {
        public ValueTask<Result<int>> EvaluateAsync(Ctx context, CancellationToken cancellationToken = default)
        {
            context.Executed.Add(name);
            return ValueTask.FromResult(result);
        }
    }

    private static readonly Error ErrorB = Error.Failure("B.Failed", "b failed");
    private static readonly Error ErrorC = Error.Failure("C.Failed", "c failed");

    [Fact]
    public void Next_IRule_Should_FailWithBError_When_ThreeRuleChainHasFailingMiddle()
    {
        Ctx ctx = new();
        IRule<Ctx> chain = new SyncRule("a", Result.Success()).Next(new SyncRule("b", Result.Failure(ErrorB))).Next(new SyncRule("c", Result.Success()));

        Result result = RuleEngine.Evaluate(chain, ctx);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    [Fact]
    public void Next_IRule_Should_RunAllInOrder_When_ThreeRuleChainSucceeds()
    {
        Ctx ctx = new();
        IRule<Ctx> chain = new SyncRule("a", Result.Success()).Next(new SyncRule("b", Result.Success())).Next(new SyncRule("c", Result.Success()));

        Result result = RuleEngine.Evaluate(chain, ctx);

        result.IsSuccess.Should().BeTrue();
        ctx.Executed.Should().Equal("a", "b", "c");
    }

    [Fact]
    public void Next_IRule_Should_FailWithCError_When_FourRuleChainHasFailingTail()
    {
        Ctx ctx = new();
        IRule<Ctx> chain = new SyncRule("a", Result.Success())
            .Next(new SyncRule("b", Result.Success()))
            .Next(new SyncRule("c", Result.Failure(ErrorC)))
            .Next(new SyncRule("d", Result.Success()));

        Result result = RuleEngine.Evaluate(chain, ctx);

        result.FirstError.Code.Should().Be("C.Failed");
        ctx.Executed.Should().Equal("a", "b", "c");
    }

    [Fact]
    public void Next_IRule_Should_MatchNestedAndLinearForms_When_Chained()
    {
        Ctx chained = new();
        Ctx nested = new();
        Ctx linear = new();
        IRule<Ctx>[] Build() => [new SyncRule("a", Result.Success()), new SyncRule("b", Result.Failure(ErrorB)), new SyncRule("c", Result.Success())];
        IRule<Ctx>[] r1 = Build();
        IRule<Ctx>[] r2 = Build();
        IRule<Ctx>[] r3 = Build();

        Result viaChain = RuleEngine.Evaluate(r1[0].Next(r1[1]).Next(r1[2]), chained);
        Result viaNested = RuleEngine.Evaluate(r2[0].Next(r2[1].Next(r2[2])), nested);
        Result viaLinear = RuleEngine.Evaluate(r3.Linear(), linear);

        viaChain.FirstError.Code.Should().Be(viaNested.FirstError.Code).And.Be(viaLinear.FirstError.Code);
        chained.Executed.Should().Equal(nested.Executed).And.Equal(linear.Executed);
    }

    [Fact]
    public void Next_IRule_Should_AppendToNestedChain_When_NextIsAlreadyLinked()
    {
        Ctx ctx = new();
        IRule<Ctx> chain = new SyncRule("a", Result.Success())
            .Next(new SyncRule("b", Result.Failure(ErrorB)).Next(new SyncRule("c", Result.Success())))
            .Next(new SyncRule("d", Result.Success()));

        Result result = RuleEngine.Evaluate(chain, ctx);

        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    [Fact]
    public async Task Next_IRule_Should_FailWithBError_When_EvaluatedAsync()
    {
        Ctx ctx = new();
        IRule<Ctx> chain = new SyncRule("a", Result.Success()).Next(new SyncRule("b", Result.Failure(ErrorB))).Next(new SyncRule("c", Result.Success()));

        Result result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    [Fact]
    public void Next_IRuleTResult_Should_FailWithBError_When_ThreeRuleChainHasFailingMiddle()
    {
        Ctx ctx = new();
        IRule<Ctx, int> chain = new SyncRuleT("a", Result.Success(1)).Next(new SyncRuleT("b", Result.Failure<int>(ErrorB))).Next(new SyncRuleT("c", Result.Success(3)));

        Result<int> result = RuleEngine.Evaluate(chain, ctx);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    [Fact]
    public void Next_IRuleTResult_Should_ReturnLastValue_When_ThreeRuleChainSucceeds()
    {
        Ctx ctx = new();
        IRule<Ctx, int> chain = new SyncRuleT("a", Result.Success(1)).Next(new SyncRuleT("b", Result.Success(2))).Next(new SyncRuleT("c", Result.Success(3)));

        Result<int> result = RuleEngine.Evaluate(chain, ctx);

        result.Value.Should().Be(3);
        ctx.Executed.Should().Equal("a", "b", "c");
    }

    [Fact]
    public async Task Next_IAsyncRule_Should_FailWithBError_When_ThreeRuleChainHasFailingMiddle()
    {
        Ctx ctx = new();
        IAsyncRule<Ctx> chain = new AsyncRule("a", Result.Success()).Next(new AsyncRule("b", Result.Failure(ErrorB))).Next(new AsyncRule("c", Result.Success()));

        Result result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    [Fact]
    public async Task Next_IAsyncRule_Should_RunAllInOrder_When_ThreeRuleChainSucceeds()
    {
        Ctx ctx = new();
        IAsyncRule<Ctx> chain = new AsyncRule("a", Result.Success()).Next(new AsyncRule("b", Result.Success())).Next(new AsyncRule("c", Result.Success()));

        Result result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.IsSuccess.Should().BeTrue();
        ctx.Executed.Should().Equal("a", "b", "c");
    }

    [Fact]
    public async Task Next_IAsyncRule_Should_MatchNestedAndLinearForms_When_Chained()
    {
        Ctx chained = new();
        Ctx nested = new();
        Ctx linear = new();
        IAsyncRule<Ctx>[] Build() => [new AsyncRule("a", Result.Success()), new AsyncRule("b", Result.Failure(ErrorB)), new AsyncRule("c", Result.Success())];
        IAsyncRule<Ctx>[] r1 = Build();
        IAsyncRule<Ctx>[] r2 = Build();
        IAsyncRule<Ctx>[] r3 = Build();

        Result viaChain = await RuleEngine.EvaluateAsync(r1[0].Next(r1[1]).Next(r1[2]), chained);
        Result viaNested = await RuleEngine.EvaluateAsync(r2[0].Next(r2[1].Next(r2[2])), nested);
        Result viaLinear = await RuleEngine.EvaluateAsync(r3.Linear(), linear);

        viaChain.FirstError.Code.Should().Be(viaNested.FirstError.Code).And.Be(viaLinear.FirstError.Code);
        chained.Executed.Should().Equal(nested.Executed).And.Equal(linear.Executed);
    }

    [Fact]
    public async Task Next_IAsyncRuleTResult_Should_FailWithBError_When_ThreeRuleChainHasFailingMiddle()
    {
        Ctx ctx = new();
        IAsyncRule<Ctx, int> chain = new AsyncRuleT("a", Result.Success(1)).Next(new AsyncRuleT("b", Result.Failure<int>(ErrorB))).Next(new AsyncRuleT("c", Result.Success(3)));

        Result<int> result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    [Fact]
    public async Task Next_IAsyncRuleTResult_Should_ReturnLastValue_When_ThreeRuleChainSucceeds()
    {
        Ctx ctx = new();
        IAsyncRule<Ctx, int> chain = new AsyncRuleT("a", Result.Success(1)).Next(new AsyncRuleT("b", Result.Success(2))).Next(new AsyncRuleT("c", Result.Success(3)));

        Result<int> result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.Value.Should().Be(3);
        ctx.Executed.Should().Equal("a", "b", "c");
    }

    [Fact]
    public void Next_Func_Should_FailWithBError_When_ThreeFuncChainHasFailingMiddle()
    {
        Ctx ctx = new();
        Func<Ctx, Result> a = c => { c.Executed.Add("a"); return Result.Success(); };
        Func<Ctx, Result> b = c => { c.Executed.Add("b"); return Result.Failure(ErrorB); };
        Func<Ctx, Result> third = c => { c.Executed.Add("c"); return Result.Success(); };

        Result result = RuleEngine.Evaluate(a.Next(b).Next(third), ctx);

        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    private sealed class BareRule : IRuleBase<Ctx>;

    private sealed class BareRuleT : IRuleBase<Ctx, int>;

    [Fact]
    public async Task Next_IRule_Should_FailWithBError_When_MiddleRuleIsAsync()
    {
        Ctx ctx = new();
        IRule<Ctx> chain = new SyncRule("a", Result.Success()).Next(new AsyncRule("b", Result.Failure(ErrorB))).Next(new SyncRule("c", Result.Success()));

        Result result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    [Fact]
    public async Task Next_IRule_Should_AppendToNestedAsyncChain_When_NextIsLinkedAsyncRule()
    {
        Ctx ctx = new();
        IRule<Ctx> chain = new SyncRule("a", Result.Success())
            .Next(new AsyncRule("b", Result.Success()).Next(new AsyncRule("c", Result.Failure(ErrorC))))
            .Next(new SyncRule("d", Result.Success()));

        Result result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.FirstError.Code.Should().Be("C.Failed");
        ctx.Executed.Should().Equal("a", "b", "c");
    }

    [Fact]
    public async Task Next_IAsyncRule_Should_AppendToNestedSyncChain_When_NextIsLinkedSyncRule()
    {
        Ctx ctx = new();
        IAsyncRule<Ctx> chain = new AsyncRule("a", Result.Success())
            .Next(new SyncRule("b", Result.Success()).Next(new SyncRule("c", Result.Failure(ErrorC))))
            .Next(new AsyncRule("d", Result.Success()));

        Result result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.FirstError.Code.Should().Be("C.Failed");
        ctx.Executed.Should().Equal("a", "b", "c");
    }

    [Fact]
    public void Next_IRule_Should_ReportMissingEngineSupport_When_TailIsNotARule()
    {
        Ctx ctx = new();
        IRule<Ctx> chain = new SyncRule("a", Result.Success()).Next(new BareRule()).Next(new SyncRule("c", Result.Success()));

        Result result = RuleEngine.Evaluate(chain, ctx);

        result.IsFailure.Should().BeTrue();
        ctx.Executed.Should().Equal("a");
    }

    [Fact]
    public void Next_IRuleTResult_Should_AppendToNestedChain_When_NextIsAlreadyLinked()
    {
        Ctx ctx = new();
        IRule<Ctx, int> chain = new SyncRuleT("a", Result.Success(1))
            .Next(new SyncRuleT("b", Result.Failure<int>(ErrorB)).Next(new SyncRuleT("c", Result.Success(3))))
            .Next(new SyncRuleT("d", Result.Success(4)));

        Result<int> result = RuleEngine.Evaluate(chain, ctx);

        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b");
    }

    [Fact]
    public async Task Next_IRuleTResult_Should_AppendToMixedChain_When_TailIsAsyncOrLinkedAsync()
    {
        Ctx ctx = new();
        IRule<Ctx, int> chain = new SyncRuleT("a", Result.Success(1))
            .Next(new AsyncRuleT("b", Result.Success(2)).Next(new AsyncRuleT("c", Result.Success(3))))
            .Next(new AsyncRuleT("d", Result.Failure<int>(ErrorB)));

        Result<int> result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b", "c", "d");
    }

    [Fact]
    public async Task Next_IAsyncRuleTResult_Should_AppendToMixedChain_When_NextIsLinkedOrSync()
    {
        Ctx ctx = new();
        IAsyncRule<Ctx, int> chain = new AsyncRuleT("a", Result.Success(1))
            .Next(new SyncRuleT("b", Result.Success(2)).Next(new SyncRuleT("c", Result.Success(3))))
            .Next(new SyncRuleT("d", Result.Failure<int>(ErrorC)));

        Result<int> result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.FirstError.Code.Should().Be("C.Failed");
        ctx.Executed.Should().Equal("a", "b", "c", "d");
    }

    [Fact]
    public async Task Next_IAsyncRuleTResult_Should_AppendToNestedAsyncChain_When_NextIsAlreadyLinked()
    {
        Ctx ctx = new();
        IAsyncRule<Ctx, int> chain = new AsyncRuleT("a", Result.Success(1))
            .Next(new AsyncRuleT("b", Result.Success(2)).Next(new AsyncRuleT("c", Result.Success(3))))
            .Next(new AsyncRuleT("d", Result.Failure<int>(ErrorB)));

        Result<int> result = await RuleEngine.EvaluateAsync(chain, ctx);

        result.FirstError.Code.Should().Be("B.Failed");
        ctx.Executed.Should().Equal("a", "b", "c", "d");
    }

    [Fact]
    public void Next_IRuleTResult_Should_ReportMissingEngineSupport_When_TailIsNotARule()
    {
        Ctx ctx = new();
        IRule<Ctx, int> chain = new SyncRuleT("a", Result.Success(1)).Next(new BareRuleT()).Next(new SyncRuleT("c", Result.Success(3)));

        Result<int> result = RuleEngine.Evaluate(chain, ctx);

        result.IsFailure.Should().BeTrue();
        ctx.Executed.Should().Equal("a");
    }
}
