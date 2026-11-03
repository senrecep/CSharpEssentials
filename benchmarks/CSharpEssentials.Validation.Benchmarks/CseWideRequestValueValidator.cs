using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>CSE rules for <see cref="WideRequest"/> with the <c>For(value, name)</c> overload.</summary>
public sealed class CseWideRequestValueValidator : Validator<WideRequest>
{
    protected override ValueTask Configure(WideRequest model, RuleContext<WideRequest> rules, CancellationToken ct = default)
    {
        rules.For(model.F1, nameof(model.F1)).NotEmpty().MaxLength(100);
        rules.For(model.F2, nameof(model.F2)).NotEmpty().MaxLength(100);
        rules.For(model.F3, nameof(model.F3)).NotEmpty().MaxLength(100);
        rules.For(model.F4, nameof(model.F4)).NotEmpty().MaxLength(100);
        rules.For(model.F5, nameof(model.F5)).NotEmpty().MaxLength(100);
        rules.For(model.F6, nameof(model.F6)).NotEmpty().MaxLength(100);
        rules.For(model.F7, nameof(model.F7)).NotEmpty().MaxLength(100);
        rules.For(model.F8, nameof(model.F8)).NotEmpty().MaxLength(100);
        rules.For(model.F9, nameof(model.F9)).NotEmpty().MaxLength(100);
        rules.For(model.F10, nameof(model.F10)).NotEmpty().MaxLength(100);
        rules.For(model.F11, nameof(model.F11)).NotEmpty().MaxLength(100);
        rules.For(model.F12, nameof(model.F12)).NotEmpty().MaxLength(100);
        return ValueTask.CompletedTask;
    }
}
