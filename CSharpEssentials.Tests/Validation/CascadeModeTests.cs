using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class CascadeModeTests
{
    private sealed record M(string? Email, int Age);

    // =========================================================================
    // Default: Stop
    // =========================================================================

    [Fact]
    public async Task Stop_ShouldHaltChain_AfterFirstFailure()
    {
        // MinLength fails → MaxLength and EmailAddress are skipped → only 1 error
        Result<M> result = await Validator.ValidateAsync(new M("ab", 0), (m, rules) =>
            rules.For(() => m.Email)
                .NotEmpty()
                .MinLength(5)
                .MaxLength(100)
                .EmailAddress());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Email.MinLength");
    }

    [Fact]
    public async Task Stop_GuardFires_ShouldProduceOnlyGuardError()
    {
        // null → NotEmpty fails → everything skipped
        Result<M> result = await Validator.ValidateAsync(new M(null, 0), (m, rules) =>
            rules.For(() => m.Email)
                .NotEmpty()
                .MinLength(5)
                .MaxLength(100)
                .EmailAddress());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Email.NotEmpty");
    }

    // =========================================================================
    // Continue: accumulate all errors
    // =========================================================================

    [Fact]
    public async Task Continue_ShouldAccumulateAllErrors_WhenMultipleValidatorsFail()
    {
        // "ab" passes NotEmpty, fails MinLength AND EmailAddress → 2 errors
        Result<M> result = await Validator.ValidateAsync(new M("ab", 0), (m, rules) =>
            rules.For(() => m.Email)
                .NotEmpty()
                .Cascade(CascadeMode.Continue)
                .MinLength(5)
                .MaxLength(100)
                .EmailAddress());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Code == "Email.MinLength");
        result.Errors.Should().Contain(e => e.Code == "Email.EmailAddress");
    }

    [Fact]
    public async Task Continue_GuardFires_ShouldStillStop_WhenGuardFails()
    {
        // null → NotEmpty fires (guard) → Cascade(Continue) is a no-op → chain stays stopped
        Result<M> result = await Validator.ValidateAsync(new M(null, 0), (m, rules) =>
            rules.For(() => m.Email)
                .NotEmpty()
                .Cascade(CascadeMode.Continue)
                .MinLength(5)
                .MaxLength(100)
                .EmailAddress());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Email.NotEmpty");
    }

    [Fact]
    public async Task Continue_ShouldPass_WhenAllValidatorsPass()
    {
        Result<M> result = await Validator.ValidateAsync(new M("user@example.com", 0), (m, rules) =>
            rules.For(() => m.Email)
                .NotEmpty()
                .Cascade(CascadeMode.Continue)
                .MinLength(5)
                .MaxLength(100)
                .EmailAddress());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Continue_ShouldReportOnlyMaxLengthError_WhenOnlyMaxLengthFails()
    {
        // Long but valid-format email — only MaxLength fails
        string longEmail = new string('a', 50) + "@example.com";

        Result<M> result = await Validator.ValidateAsync(new M(longEmail, 0), (m, rules) =>
            rules.For(() => m.Email)
                .NotEmpty()
                .Cascade(CascadeMode.Continue)
                .MinLength(5)
                .MaxLength(20)
                .EmailAddress());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Email.MaxLength");
    }

    // =========================================================================
    // Continue on comparable / numeric chains
    // =========================================================================

    private sealed record Range(int Value);

    [Fact]
    public async Task Continue_Comparable_ShouldAccumulateErrors_FromMultipleConstraints()
    {
        // Value=0 fails GreaterThan(0) AND Must predicate
        Result<Range> result = await Validator.ValidateAsync(new Range(0), (m, rules) =>
            rules.For(() => m.Value)
                .Cascade(CascadeMode.Continue)
                .GreaterThan(0)
                .LessThan(100)
                .Must(v => v % 2 == 0, "Value.Even", "Value must be even."));

        // GreaterThan fails AND Must(even) should run too
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Value.GreaterThan");
        // LessThan passes (0 < 100), Must(even) passes (0 % 2 == 0)
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task Continue_Comparable_TwoFailures()
    {
        // Value=200 fails LessThan(100) AND Must(even check against >150)
        Result<Range> result = await Validator.ValidateAsync(new Range(201), (m, rules) =>
            rules.For(() => m.Value)
                .Cascade(CascadeMode.Continue)
                .GreaterThan(0)
                .LessThan(100)
                .Must(v => v % 2 == 0, "Value.Even", "Value must be even."));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Code == "Value.LessThan");
        result.Errors.Should().Contain(e => e.Code == "Value.Even");
    }

    // =========================================================================
    // Stop → Continue → Stop is not supported (Continue is one-way per chain)
    // =========================================================================

    [Fact]
    public async Task Continue_MultipleChains_AreIndependent()
    {
        // Two independent For() chains — each has its own cascade mode
        Result<M> result = await Validator.ValidateAsync(new M(null, -5), (m, rules) =>
        {
            rules.For(() => m.Email)
                .NotEmpty();   // Stop mode: null → 1 error

            rules.For(() => m.Age)
                .Cascade(CascadeMode.Continue)
                .GreaterThan(0)
                .LessThan(150);  // -5 fails GreaterThan but passes LessThan → 1 error
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Code == "Email.NotEmpty");
        result.Errors.Should().Contain(e => e.Code == "Age.GreaterThan");
    }
}
