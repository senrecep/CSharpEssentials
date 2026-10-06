namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Value-equatable model of one enum member.
/// </summary>
/// <param name="Name">The C# identifier.</param>
/// <param name="RawValue">The two's complement bits, sign-extended to 64 bits.</param>
/// <param name="DeclaredWireName">The name from <c>[JsonStringEnumMemberName]</c> or <c>[EnumMember(Value)]</c>, if any.</param>
/// <param name="Aliases">The declared aliases.</param>
/// <param name="Description">The description, if any.</param>
/// <param name="IsObsolete">Whether the member is marked <c>[Obsolete]</c>.</param>
/// <param name="IsFallback">Whether the member is marked <c>[EnumFallback]</c>.</param>
internal sealed record EnumMemberModel(
    string Name,
    ulong RawValue,
    string? DeclaredWireName,
    EquatableArray<string> Aliases,
    string? Description,
    bool IsObsolete,
    bool IsFallback);
