namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// A problem in the members of a <c>[StringEnum]</c> enum that <see cref="EnumModelRules"/> finds.
/// </summary>
internal enum EnumRuleKind
{
    /// <summary>CSE0002: two members produce the same wire name.</summary>
    WireNameCollision,

    /// <summary>CSE0003: an alias equals a wire name, member name or alias of another member.</summary>
    AliasCollision,

    /// <summary>CSE0004: more than one <c>[EnumFallback]</c> member.</summary>
    MultipleFallbacks,

    /// <summary>CSE0006: a <c>[Flags]</c> enum without a zero member.</summary>
    FlagsWithoutZero,

    /// <summary>CSE0007: a <c>[Flags]</c> member that is neither a single bit nor a combination of other members.</summary>
    InvalidFlagValue,

    /// <summary>CSE0008: <c>[EnumFallback]</c> on a <c>[Flags]</c> enum.</summary>
    FallbackOnFlags,

    /// <summary>CSE0009: a wire name or alias the parser cannot read back.</summary>
    InvalidWireName,
}
