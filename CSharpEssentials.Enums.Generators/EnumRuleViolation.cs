namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// One problem found by <see cref="EnumModelRules"/>.
/// </summary>
/// <param name="Kind">The problem.</param>
/// <param name="Member">The index of the member the problem is reported on, -1 for the enum itself.</param>
/// <param name="Alias">The index of the alias the problem is about, -1 for the wire name or the member.</param>
/// <param name="Text">The wire name or alias the problem is about.</param>
/// <param name="Detail">The other members, what the alias collides with, or why the name is invalid.</param>
internal readonly record struct EnumRuleViolation(EnumRuleKind Kind, int Member, int Alias, string Text, string Detail)
{
    /// <summary>Whether the generator skips the enum because of this problem.</summary>
    public bool IsError => Kind is not (EnumRuleKind.FlagsWithoutZero or EnumRuleKind.InvalidFlagValue);
}
