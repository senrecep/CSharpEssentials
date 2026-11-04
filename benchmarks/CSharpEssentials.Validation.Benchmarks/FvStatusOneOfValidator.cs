using FluentValidation;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>FluentValidation has no subset validator for enums, so this uses <c>Must</c>.</summary>
public sealed class FvStatusOneOfValidator : AbstractValidator<StatusRequest>
{
    private static readonly ShipmentStatus[] Allowed = [ShipmentStatus.Pending, ShipmentStatus.Shipped];

    public FvStatusOneOfValidator()
    {
        RuleFor(x => x.Status).Must(s => Array.IndexOf(Allowed, s) >= 0);
    }
}
