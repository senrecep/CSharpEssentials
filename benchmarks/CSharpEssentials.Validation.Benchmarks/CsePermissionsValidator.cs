using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>CSE <c>HasOnlyDefinedFlags</c> on a plain flags enum.</summary>
public sealed class CsePermissionsValidator : Validator<PermissionsRequest>
{
    protected override ValueTask Configure(PermissionsRequest model, RuleContext<PermissionsRequest> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Permissions).HasOnlyDefinedFlags();
        return ValueTask.CompletedTask;
    }
}
