namespace CSharpEssentials.Enums;

/// <summary>
/// Metadata of one enum type.
/// </summary>
public interface IEnumInfo
{
    /// <summary>The enum type.</summary>
    Type EnumType { get; }

    /// <summary>The underlying integral type.</summary>
    Type UnderlyingType { get; }

    /// <summary>Whether the enum is marked <see cref="FlagsAttribute"/>.</summary>
    bool IsFlags { get; }

    /// <summary>The bitwise OR of every member value.</summary>
    ulong DefinedMask { get; }

    /// <summary>The storage from <see cref="StringEnumAttribute.Storage"/>; <see cref="EnumStorage.Default"/> when unset.</summary>
    EnumStorage Storage { get; }

    /// <summary>The members in declaration order.</summary>
    IReadOnlyList<IEnumMemberInfo> Members { get; }

    /// <summary>The <see cref="EnumFallbackAttribute"/> member, if any.</summary>
    IEnumMemberInfo? Fallback { get; }

    /// <summary>The wire names in declaration order, without aliases.</summary>
    IReadOnlyList<string> WireNames { get; }

    /// <summary>Dispatches to typed code.</summary>
    TResult Accept<TResult>(IEnumInfoVisitor<TResult> visitor);
}
