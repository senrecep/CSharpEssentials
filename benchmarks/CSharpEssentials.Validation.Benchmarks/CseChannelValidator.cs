using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>CSE <c>IsDefinedEnum</c> on a <c>[StringEnum]</c> enum (generated metadata).</summary>
public sealed class CseChannelValidator : Validator<ChannelRequest>
{
    protected override ValueTask Configure(ChannelRequest model, RuleContext<ChannelRequest> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Channel).IsDefinedEnum();
        return ValueTask.CompletedTask;
    }
}
