using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public sealed class NextOverloadTests
{
    private sealed class Ctx
    {
        public List<string> Log { get; } = [];
    }

    private static readonly Func<Ctx, Result<int>> SyncNext = c =>
    {
        c.Log.Add("next");
        return 2;
    };

    private static readonly Func<Ctx, CancellationToken, Result<int>> TokenNext = (c, _) =>
    {
        c.Log.Add("next");
        return 2;
    };

    private static readonly Func<Ctx, CancellationToken, ValueTask<Result<int>>> AsyncNext = (c, _) =>
    {
        c.Log.Add("next");
        return new ValueTask<Result<int>>(2);
    };

    private static Func<Ctx, Result<int>> SyncHead(bool success) => c =>
    {
        c.Log.Add("head");
        return success ? 1 : Error.Validation("head", "head");
    };

    private static Func<Ctx, CancellationToken, Result<int>> TokenHead(bool success) => (c, _) =>
    {
        c.Log.Add("head");
        return success ? 1 : Error.Validation("head", "head");
    };

    private static Func<Ctx, CancellationToken, ValueTask<Result<int>>> AsyncHead(bool success) => (c, _) =>
    {
        c.Log.Add("head");
        Result<int> result = success ? 1 : Error.Validation("head", "head");
        return new ValueTask<Result<int>>(result);
    };

    private static IRuleBase<Ctx, int> NextRule => SyncNext.ToRule();

    public static TheoryData<string> Chains => new()
    {
        "rule+async", "asyncRule+async", "asyncRule+token", "asyncRule+sync",
        "sync+rule", "sync+sync", "sync+token", "sync+async",
        "token+rule", "token+sync", "token+token", "token+async",
        "async+rule", "async+async", "async+sync", "async+token"
    };

    private static IRuleBase<Ctx, int> Build(string chain, bool headSucceeds) => chain switch
    {
        "rule+async" => SyncHead(headSucceeds).ToRule().Next(AsyncNext),
        "asyncRule+async" => AsyncHead(headSucceeds).ToRule().Next(AsyncNext),
        "asyncRule+token" => AsyncHead(headSucceeds).ToRule().Next(TokenNext),
        "asyncRule+sync" => AsyncHead(headSucceeds).ToRule().Next(SyncNext),
        "sync+rule" => SyncHead(headSucceeds).Next(NextRule),
        "sync+sync" => SyncHead(headSucceeds).Next(SyncNext),
        "sync+token" => SyncHead(headSucceeds).Next(TokenNext),
        "sync+async" => SyncHead(headSucceeds).Next(AsyncNext),
        "token+rule" => TokenHead(headSucceeds).Next(NextRule),
        "token+sync" => TokenHead(headSucceeds).Next(SyncNext),
        "token+token" => TokenHead(headSucceeds).Next(TokenNext),
        "token+async" => TokenHead(headSucceeds).Next(AsyncNext),
        "async+rule" => AsyncHead(headSucceeds).Next(NextRule),
        "async+async" => AsyncHead(headSucceeds).Next(AsyncNext),
        "async+sync" => AsyncHead(headSucceeds).Next(SyncNext),
        "async+token" => AsyncHead(headSucceeds).Next(TokenNext),
        _ => throw new ArgumentOutOfRangeException(nameof(chain))
    };

    [Theory]
    [MemberData(nameof(Chains))]
    public void Evaluate_Should_Return_Next_Value_When_Head_Succeeds(string chain)
    {
        var ctx = new Ctx();

        Result<int> result = RuleEngine.Evaluate(Build(chain, headSucceeds: true), ctx);

        result.Value.Should().Be(2);
        ctx.Log.Should().Equal("head", "next");
    }

    [Theory]
    [MemberData(nameof(Chains))]
    public async Task EvaluateAsync_Should_Return_Head_Failure_Without_Next_When_Head_Fails(string chain)
    {
        var ctx = new Ctx();

        Result<int> result = await RuleEngine.EvaluateAsync(Build(chain, headSucceeds: false), ctx);

        result.Errors.Select(e => e.Code).Should().Equal("head");
        ctx.Log.Should().Equal("head");
    }
}
