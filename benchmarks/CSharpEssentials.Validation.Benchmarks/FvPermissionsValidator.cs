using FluentValidation;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>FluentValidation <c>IsInEnum</c> on a flags enum (accepts combinations of defined flags).</summary>
public sealed class FvPermissionsValidator : AbstractValidator<PermissionsRequest>
{
    public FvPermissionsValidator()
    {
        RuleFor(x => x.Permissions).IsInEnum();
    }
}
