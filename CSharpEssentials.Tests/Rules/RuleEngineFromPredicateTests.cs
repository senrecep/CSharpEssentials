using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class RuleEngineFromPredicateTests
{
    private static readonly Error TestError = Error.Failure("Test.Error", "Predicate failed");

    [Fact]
    public void FromPredicate_Should_ReturnSuccess_When_PredicateTrue()
    {
        IRule<int> rule = RuleEngine.FromPredicate<int>(x => x > 0, TestError);
        Result result = RuleEngine.Evaluate(rule, 5);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void FromPredicate_Should_ReturnFailure_When_PredicateFalse()
    {
        IRule<int> rule = RuleEngine.FromPredicate<int>(x => x > 0, TestError);
        Result result = RuleEngine.Evaluate(rule, -1);
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Test.Error");
    }

    [Fact]
    public void FromPredicate_WithFactory_Should_UseContextInError()
    {
        IRule<int> rule = RuleEngine.FromPredicate<int>(
            x => x > 0,
            x => Error.Failure("Negative.Value", $"Value {x} is not positive"));
        Result result = RuleEngine.Evaluate(rule, -5);
        result.IsFailure.Should().BeTrue();
        result.FirstError.Description.Should().Contain("-5");
    }

    [Fact]
    public void FromPredicateAsync_Should_ReturnSuccess_When_PredicateTrue()
    {
        IAsyncRule<int> rule = RuleEngine.FromPredicateAsync<int>(
            async x => { await Task.Yield(); return x > 0; },
            TestError);
        Result result = RuleEngine.Evaluate(rule, 5);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void FromPredicateAsync_Should_ReturnFailure_When_PredicateFalse()
    {
        IAsyncRule<int> rule = RuleEngine.FromPredicateAsync<int>(
            async x => { await Task.Yield(); return x > 0; },
            TestError);
        Result result = RuleEngine.Evaluate(rule, -1);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FromPredicateAsync_WithFactory_Should_UseContextInError()
    {
        IAsyncRule<int> rule = RuleEngine.FromPredicateAsync<int>(
            async x => { await Task.Yield(); return x > 0; },
            x => Error.Failure("Negative.Value", $"Value {x} is not positive"));
        Result result = RuleEngine.Evaluate(rule, -3);
        result.IsFailure.Should().BeTrue();
        result.FirstError.Description.Should().Contain("-3");
    }

    [Fact]
    public void FromPredicate_Should_WorkWithComplexContext()
    {
        IRule<string> rule = RuleEngine.FromPredicate<string>(
            s => s.Length >= 3,
            s => Error.Validation("String.TooShort", $"'{s}' must be at least 3 characters"));
        RuleEngine.Evaluate(rule, "hi").IsFailure.Should().BeTrue();
        RuleEngine.Evaluate(rule, "hello").IsSuccess.Should().BeTrue();
    }
}
