using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class LinearRuleTests
{
    private sealed class TestContext
    {
        public int Value { get; set; }
        public List<string> ExecutedRules { get; } = [];
    }

    [Fact]
    public void Linear_WithAllSuccessRules_ShouldReturnSuccess()
    {
        TestContext context = new() { Value = 0 };
        Func<TestContext, Result>[] rules =
        [
            ctx => { ctx.ExecutedRules.Add("Rule1"); return Result.Success(); },
            ctx => { ctx.ExecutedRules.Add("Rule2"); return Result.Success(); },
            ctx => { ctx.ExecutedRules.Add("Rule3"); return Result.Success(); }
        ];

        Result result = RuleEngine.Linear(rules, context);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Linear_WithFailingRule_ShouldReturnFailure()
    {
        TestContext context = new() { Value = 0 };
        var testError = Error.Failure("TEST", "Failed at Rule2");
        Func<TestContext, Result>[] rules =
        [
            ctx => { ctx.ExecutedRules.Add("Rule1"); return Result.Success(); },
            ctx => { ctx.ExecutedRules.Add("Rule2"); return Result.Failure(testError); },
            ctx => { ctx.ExecutedRules.Add("Rule3"); return Result.Success(); }
        ];

        Result result = RuleEngine.Linear(rules, context);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Linear_WithCancellationToken_ShouldWork()
    {
        TestContext context = new();
        CancellationToken token = CancellationToken.None;
        Func<TestContext, CancellationToken, Result>[] rules =
        [
            (ctx, _) => { ctx.ExecutedRules.Add("Rule1"); return Result.Success(); },
            (ctx, _) => { ctx.ExecutedRules.Add("Rule2"); return Result.Success(); }
        ];

        Result result = RuleEngine.Linear(rules, context, token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Linear_WithSingleRule_ShouldWork()
    {
        TestContext context = new() { Value = 10 };
        Func<TestContext, Result>[] rules =
        [
            ctx => { ctx.ExecutedRules.Add("Rule1"); return Result.Success(); }
        ];

        Result result = RuleEngine.Linear(rules, context);

        result.IsSuccess.Should().BeTrue();
    }
}
