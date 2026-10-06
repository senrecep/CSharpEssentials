namespace CSharpEssentials.Enums;

/// <summary>
/// Extra spellings of an enum member. Aliases are read, never written.
/// </summary>
/// <param name="aliases">The extra spellings.</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class EnumAliasAttribute(params string[] aliases) : Attribute
{
    /// <summary>The extra spellings.</summary>
    public IReadOnlyList<string> Aliases { get; } = aliases ?? [];
}
