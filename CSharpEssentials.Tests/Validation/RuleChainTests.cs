using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class RuleChainTests
{
    private sealed record Model(string? Text, int Number);
    private sealed record OuterModel(Model? Inner);

    // -------------------------------------------------------------------------
    // Must — sync predicate
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Must_ShouldAddError_WhenPredicateReturnsFalse()
    {
        Model model = new("hello", 5);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Text).Must(v => v == "world", "Text.Wrong", "Must be 'world'."));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Text.Wrong");
    }

    [Fact]
    public async Task Must_ShouldNotAddError_WhenPredicateReturnsTrue()
    {
        Model model = new("world", 5);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Text).Must(v => v == "world", "Text.Wrong", "Must be 'world'."));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Must_ShouldSkipSubsequentChecks_AfterFirstFailureOnSameChain()
    {
        Model model = new(null, 5);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Text)
                .NotNull()
                .Must(v => v == "world", "Text.Wrong", "Must be 'world'."));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Text.NotNull");
    }

    [Fact]
    public async Task Must_ShouldAccumulateErrors_AcrossDifferentProperties()
    {
        Model model = new(null, -1);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
        {
            rules.For(() => m.Text).NotNull();
            rules.For(() => m.Number).GreaterThan(0);
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    // -------------------------------------------------------------------------
    // MustAsync — async predicate
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MustAsync_ShouldAddError_WhenAsyncPredicateReturnsFalse()
    {
        Model model = new("hello", 5);

        Result<Model> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.For(() => m.Text).MustAsync(
                async (v, _) => await Task.FromResult(v == "world"),
                "Text.Wrong",
                "Must be 'world'.",
                ct));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Text.Wrong");
    }

    [Fact]
    public async Task MustAsync_ShouldNotAddError_WhenAsyncPredicateReturnsTrue()
    {
        Model model = new("world", 5);

        Result<Model> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.For(() => m.Text).MustAsync(
                async (v, _) => await Task.FromResult(v == "world"),
                "Text.Wrong",
                "Must be 'world'.",
                ct));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // PropertyName extraction
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PropertyName_ShouldBeExtractedFromLambda()
    {
        string capturedName = string.Empty;
        Model model = new("test", 5);

        await Validator.ValidateAsync(model, (m, rules) =>
        {
            RuleChain<Model, string?> chain = rules.For(() => m.Text);
            capturedName = chain.PropertyName;
        });

        capturedName.Should().Be("Text");
    }

    [Fact]
    public async Task PropertyName_ShouldExtractNestedPath()
    {
        string capturedName = string.Empty;
        OuterModel outer = new(new Model("test", 5));

        await Validator.ValidateAsync(outer, (m, rules) =>
        {
            if (m.Inner is not null)
            {
                RuleChain<OuterModel, string?> chain = rules.For(() => m.Inner.Text);
                capturedName = chain.PropertyName;
            }
        });

        capturedName.Should().Be("Inner.Text");
    }

    // -------------------------------------------------------------------------
    // AddError — cascade & manual
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddError_ShouldOnlyRecordFirstError_OnSameChain()
    {
        Model model = new(null, 0);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
        {
            RuleChain<Model, string?> chain = rules.For(() => m.Text);
            chain.AddError(Error.Validation("First", "First error."));
            chain.AddError(Error.Validation("Second", "This should be ignored."));
        });

        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("First");
    }

    [Fact]
    public async Task RuleContext_AddFailure_ShouldAddError_WithoutPropertyChain()
    {
        Model model = new("test", 5);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.AddFailure("Model.Invalid", "Cross-field validation failed."));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Model.Invalid");
    }

    [Fact]
    public async Task HasFailed_ShouldBeFalse_BeforeAnyError()
    {
        bool hasFailed = true;
        Model model = new("test", 5);

        await Validator.ValidateAsync(model, (m, rules) =>
        {
            RuleChain<Model, string?> chain = rules.For(() => m.Text);
            hasFailed = chain.HasFailed;
        });

        hasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HasFailed_ShouldBeTrue_AfterAddError()
    {
        bool hasFailed = false;
        Model model = new(null, 0);

        await Validator.ValidateAsync(model, (m, rules) =>
        {
            RuleChain<Model, string?> chain = rules.For(() => m.Text).NotNull();
            hasFailed = chain.HasFailed;
        });

        hasFailed.Should().BeTrue();
    }
}
