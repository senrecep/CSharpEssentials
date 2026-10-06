namespace CSharpEssentials.Enums;

/// <summary>
/// Metadata of one enum member.
/// </summary>
public interface IEnumMemberInfo
{
    /// <summary>The C# identifier (<c>PendingApproval</c>).</summary>
    string MemberName { get; }

    /// <summary>The canonical string written by every layer (<c>pending_approval</c>).</summary>
    string WireName { get; }

    /// <summary>Declared aliases, then the legacy 3.x snake case name when it differs from the wire name. Read, never written.</summary>
    IReadOnlyList<string> Aliases { get; }

    /// <summary>The <c>[Description]</c> text or the XML summary of the member.</summary>
    string? Description { get; }

    /// <summary>Whether the member is marked <see cref="ObsoleteAttribute"/>.</summary>
    bool IsObsolete { get; }

    /// <summary>Whether the member is the <see cref="EnumFallbackAttribute"/> member.</summary>
    bool IsFallback { get; }

    /// <summary>The two's complement bits of the value, sign-extended to 64 bits.</summary>
    ulong RawValue { get; }

    /// <summary>The value as an invariant number of the underlying type (<c>-1</c>, <c>18446744073709551615</c>).</summary>
    string NumericText { get; }
}
