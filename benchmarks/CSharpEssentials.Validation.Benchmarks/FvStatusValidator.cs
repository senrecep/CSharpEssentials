using FluentValidation;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>FluentValidation <c>IsInEnum</c> on a plain enum.</summary>
public sealed class FvStatusValidator : AbstractValidator<StatusRequest>
{
    public FvStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
