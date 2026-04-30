using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class OrRuleTests
{
    private sealed class TestContext
    {
        public int Value { get; set; }
        public List<string> ExecutedRules { get; } = [];
    }

    [Fact]
    public void Or_WithAllSuccessRules_ShouldReturnFirstSuccess()
    {
        TestContext context = new() { Value = 10 };
        Func<TestContext, Result>[] rules =
        [
            ctx => { ctx.ExecutedRules.Add("Rule1"); return Result.Success(); },
            ctx => { ctx.ExecutedRules.Add("Rule2"); return Result.Success(); },
            ctx => { ctx.ExecutedRules.Add("Rule3"); return Result.Success(); }
        ];

        Result result = RuleEngine.Or(rules, context);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Or_WithOneSuccessRule_ShouldReturnSuccess()
    {
        TestContext context = new() { Value = 10 };
        var error1 = Error.Failure("ERR1", "Error 1");
        var error2 = Error.Failure("ERR2", "Error 2");
        Func<TestContext, Result>[] rules =
        [
            ctx => { ctx.ExecutedRules.Add("Rule1"); return Result.Failure(error1); },
            ctx => { ctx.ExecutedRules.Add("Rule2"); return Result.Success(); },
            ctx => { ctx.ExecutedRules.Add("Rule3"); return Result.Failure(error2); }
        ];

        Result result = RuleEngine.Or(rules, context);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Or_WithAllFailingRules_ShouldReturnFailure()
    {
        TestContext context = new() { Value = 10 };
        var error1 = Error.Failure("ERR1", "Error 1");
        var error2 = Error.Failure("ERR2", "Error 2");
        var error3 = Error.Failure("ERR3", "Error 3");
        Func<TestContext, Result>[] rules =
        [
            _ => Result.Failure(error1),
            _ => Result.Failure(error2),
            _ => Result.Failure(error3)
        ];

        Result result = RuleEngine.Or(rules, context);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void Or_WithCancellationToken_ShouldWork()
    {
        TestContext context = new();
        CancellationToken token = CancellationToken.None;
        Func<TestContext, CancellationToken, Result>[] rules =
        [
            (ctx, _) => { ctx.ExecutedRules.Add("Rule1"); return Result.Success(); },
            (ctx, _) => { ctx.ExecutedRules.Add("Rule2"); return Result.Failure(Error.Failure("ERR", "Error")); }
        ];

        Result result = RuleEngine.Or(rules, context, token);

        result.IsSuccess.Should().BeTrue();
    }
}

