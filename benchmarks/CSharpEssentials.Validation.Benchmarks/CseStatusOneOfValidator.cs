using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>CSE <c>IsOneOf</c> with the allowed set in a static field.</summary>
public sealed class CseStatusOneOfValidator : Validator<StatusRequest>
{
    private static readonly ShipmentStatus[] Allowed = [ShipmentStatus.Pending, ShipmentStatus.Shipped];

    protected override ValueTask Configure(StatusRequest model, RuleContext<StatusRequest> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Status).IsOneOf(Allowed);
        return ValueTask.CompletedTask;
    }
}
