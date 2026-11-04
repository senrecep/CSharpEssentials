using FluentValidation;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>FluentValidation rules for <see cref="WideRequest"/>.</summary>
public sealed class FvWideRequestValidator : AbstractValidator<WideRequest>
{
    public FvWideRequestValidator()
    {
        RuleFor(x => x.F1).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F2).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F3).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F4).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F5).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F6).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F7).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F8).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F9).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F10).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F11).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F12).NotEmpty().MaximumLength(100);
    }
}
