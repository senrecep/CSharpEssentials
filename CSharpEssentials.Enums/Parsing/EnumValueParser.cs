#if NETSTANDARD2_0
using CharText = string;
#else
using CharText = System.ReadOnlySpan<char>;
#endif
using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.Enums;

/// <summary>
/// The one parser of the enum contract, over generated metadata (<see cref="EnumMetadata.Get{TEnum}"/>).
/// </summary>
/// <remarks>
/// Lookup order: exact wire name, wire name ignoring case, member name, aliases, then a number of a defined member.
/// <see cref="EnumReadMode.Input"/> applies <see cref="EnumConventions.CaseInsensitive"/>, <see cref="EnumConventions.AcceptMemberNames"/>
/// and <see cref="EnumConventions.AcceptNumbers"/> and never accepts the fallback member. <see cref="EnumReadMode.Data"/> accepts every
/// known spelling and applies <see cref="EnumConventions.UnknownValue"/>. Malformed text (empty, surrounding whitespace, <c>1.0</c>,
/// <c>0x1</c>, <c>+1</c>, <c>-0</c>, out of range, or numeric text longer than 20 characters even when padded with zeros)
/// is always rejected, in JSON strings as well.
/// </remarks>
public static class EnumValueParser
{
    /// <summary>Parses one token.</summary>
    public static bool TryParse<TEnum>(CharText text, EnumReadMode mode, EnumConventions conventions,
        out TEnum value, [NotNullWhen(false)] out EnumValueError? error) where TEnum : struct, Enum =>
        EnumMetadata.Get<TEnum>().TryParse(text, mode, conventions, out value, out error);

    /// <summary>Accepts a number.</summary>
    public static bool TryParseNumber<TEnum>(long number, EnumReadMode mode, EnumConventions conventions,
        out TEnum value, [NotNullWhen(false)] out EnumValueError? error) where TEnum : struct, Enum =>
        EnumMetadata.Get<TEnum>().TryParseNumber(number, mode, conventions, out value, out error);

    /// <summary>Accepts a number.</summary>
    public static bool TryParseNumber<TEnum>(ulong number, EnumReadMode mode, EnumConventions conventions,
        out TEnum value, [NotNullWhen(false)] out EnumValueError? error) where TEnum : struct, Enum =>
        EnumMetadata.Get<TEnum>().TryParseNumber(number, mode, conventions, out value, out error);
}
