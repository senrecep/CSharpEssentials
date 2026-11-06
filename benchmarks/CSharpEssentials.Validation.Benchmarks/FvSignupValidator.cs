using FluentValidation;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>FluentValidation rules for <see cref="SignupRequest"/>.</summary>
public sealed class FvSignupValidator : AbstractValidator<SignupRequest>
{
    public FvSignupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(1, 149);
        RuleFor(x => x.Tags).NotEmpty();
    }
}
