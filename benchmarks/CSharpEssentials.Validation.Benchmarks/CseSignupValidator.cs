using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>CSE rules for <see cref="SignupRequest"/>.</summary>
public sealed class CseSignupValidator : Validator<SignupRequest>
{
    protected override ValueTask Configure(SignupRequest model, RuleContext<SignupRequest> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty().MaxLength(100);
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        rules.For(() => model.Age).InclusiveBetween(1, 149);
        rules.For(() => model.Tags).NotEmpty();
        return ValueTask.CompletedTask;
    }
}
