using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class RuleEngineTests
{
    private sealed class TestContext
    {
        public int Value { get; set; }
        public bool IsValid { get; set; }
    }

    #region Basic Evaluation

    [Fact]
    public void Evaluate_WithSuccessfulRule_ShouldReturnSuccess()
    {
        TestContext context = new() { Value = 10 };
        Func<TestContext, Result> rule = _ => Result.Success();

        Result result = RuleEngine.Evaluate(rule, context);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WithFailingRule_ShouldReturnFailure()
    {
        TestContext context = new() { Value = -1 };
        Func<TestContext, Result> rule = ctx =>
            ctx.Value > 0 ? Result.Success() : Error.Validation("INVALID", "Value must be positive");

        Result result = RuleEngine.Evaluate(rule, context);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("INVALID");
    }

    #endregion

    #region And Evaluation

    [Fact]
    public void And_WithAllSuccess_ShouldReturnSuccess()
    {
        TestContext context = new() { Value = 10, IsValid = true };
        Func<TestContext, Result>[] rules =
        [
            ctx => ctx.Value > 0 ? Result.Success() : Error.Validation("VALUE", "Invalid"),
            ctx => ctx.IsValid ? Result.Success() : Error.Validation("VALID", "Invalid")
        ];

        Result result = RuleEngine.And(rules, context);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void And_WithOneFailure_ShouldReturnFailure()
    {
        TestContext context = new() { Value = -1, IsValid = true };
        Func<TestContext, Result>[] rules =
        [
            ctx => ctx.Value > 0 ? Result.Success() : Error.Validation("VALUE", "Invalid"),
            ctx => ctx.IsValid ? Result.Success() : Error.Validation("VALID", "Invalid")
        ];

        Result result = RuleEngine.And(rules, context);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "VALUE");
    }

    #endregion

    #region Or Evaluation

    [Fact]
    public void Or_WithOneSuccess_ShouldReturnSuccess()
    {
        TestContext context = new() { Value = 10, IsValid = false };
        Func<TestContext, Result>[] rules =
        [
            ctx => ctx.Value > 0 ? Result.Success() : Error.Validation("VALUE", "Invalid"),
            ctx => ctx.IsValid ? Result.Success() : Error.Validation("VALID", "Invalid")
        ];

        Result result = RuleEngine.Or(rules, context);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Or_WithAllFailure_ShouldReturnFailure()
    {
        TestContext context = new() { Value = -1, IsValid = false };
        Func<TestContext, Result>[] rules =
        [
            ctx => ctx.Value > 0 ? Result.Success() : Error.Validation("VALUE", "Invalid"),
            ctx => ctx.IsValid ? Result.Success() : Error.Validation("VALID", "Invalid")
        ];

        Result result = RuleEngine.Or(rules, context);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Linear Evaluation

    [Fact]
    public void Linear_WithAllSuccess_ShouldReturnSuccess()
    {
        TestContext context = new() { Value = 0 };
        Func<TestContext, Result>[] rules =
        [
            ctx => { ctx.Value = 1; return Result.Success(); },
            ctx => { ctx.Value = 2; return Result.Success(); },
            ctx => { ctx.Value = 3; return Result.Success(); }
        ];

        Result result = RuleEngine.Linear(rules, context);

        result.IsSuccess.Should().BeTrue();
        context.Value.Should().Be(3);
    }

    [Fact]
    public void Linear_WithFailure_ShouldStopExecution()
    {
        TestContext context = new() { Value = 0 };
        Func<TestContext, Result>[] rules =
        [
            ctx => { ctx.Value = 1; return Result.Success(); },
            ctx => { ctx.Value = 2; return Error.Validation("FAIL", "Failed"); },
            ctx => { ctx.Value = 3; return Result.Success(); }
        ];

        Result result = RuleEngine.Linear(rules, context);

        result.IsFailure.Should().BeTrue();
        context.Value.Should().Be(2); // Should stop after second rule
    }

    #endregion

    #region Exception Handling

    [Fact]
    public void Evaluate_WithExceptionThrowingRule_ShouldReturnFailure()
    {
        TestContext context = new() { Value = 10 };
        Func<TestContext, Result> rule = _ => throw new InvalidOperationException("Test exception");

        Result result = RuleEngine.Evaluate(rule, context);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    #endregion
}

