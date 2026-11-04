using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>CSE <c>IsDefinedEnum</c> on a plain enum.</summary>
public sealed class CseStatusValidator : Validator<StatusRequest>
{
    protected override ValueTask Configure(StatusRequest model, RuleContext<StatusRequest> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Status).IsDefinedEnum();
        return ValueTask.CompletedTask;
    }
}
