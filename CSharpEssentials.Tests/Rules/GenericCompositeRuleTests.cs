using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class GenericCompositeRuleTests
{
    private static readonly Error Failure = Error.Failure("E1", "Failed");

    private static Func<int, Result<int>> Pass(int value) => _ => value;

    private static Func<int, Result<int>> Fail() => _ => Failure;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task And_ValueTypeResults_Should_ReturnTheFirstValue_When_AllSucceed(bool viaEvaluateAsync)
    {
        IRuleBase<int, int> rule = new[] { Pass(0), Pass(2) }.And();

        Result<int> result = viaEvaluateAsync ? await RuleEngine.EvaluateAsync(rule, 1) : RuleEngine.Evaluate(rule, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Or_ValueTypeResults_Should_ReturnTheFirstSuccess(bool viaEvaluateAsync)
    {
        IRuleBase<int, int> rule = new[] { Fail(), Pass(0) }.Or();

        Result<int> result = viaEvaluateAsync ? await RuleEngine.EvaluateAsync(rule, 1) : RuleEngine.Evaluate(rule, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Or_Should_ReturnEmptyRuleArrayError_When_Empty(bool viaEvaluateAsync)
    {
        IRuleBase<int, int> rule = Array.Empty<Func<int, Result<int>>>().Or();

        Result<int> result = viaEvaluateAsync ? await RuleEngine.EvaluateAsync(rule, 1) : RuleEngine.Evaluate(rule, 1);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("EMPTY_RULE_ARRAY");
    }

    [Fact]
    public void And_Evaluate_Should_RunTheWholeRule_When_CalledDirectly()
    {
        var rule = (IAndRule<int, int>)new[] { Pass(3), Fail() }.And();

        Result<int> result = rule.Evaluate(1);

        result.FirstError.Should().Be(Failure);
    }

    [Fact]
    public void Or_Evaluate_Should_RunTheWholeRule_When_CalledDirectly()
    {
        var rule = (IOrRule<int, int>)new[] { Fail(), Pass(5) }.Or();

        Result<int> result = rule.Evaluate(1);

        result.Value.Should().Be(5);
    }
}
