#if NETSTANDARD2_0
using CharText = string;
#else
using CharText = System.ReadOnlySpan<char>;
#endif

namespace CSharpEssentials.Enums;

/// <summary>
/// Strict invariant parsing of <c>-?[0-9]+</c> into the sign-extended bits of <typeparamref name="TEnum"/>.
/// </summary>
internal static class EnumNumberParser<TEnum> where TEnum : struct, Enum
{
    /// <summary>
    /// Whether the token is meant as a number (starts with a digit, a sign or a dot). Such a token is never a name, so a malformed
    /// number is an error even where unknown names map to the fallback member.
    /// </summary>
    public static bool LooksNumeric(CharText text)
    {
        char first = text[0];
        return first is >= '0' and <= '9' or '-' or '+' or '.';
    }

    /// <summary>Parses a numeric token. Returns false for malformed or out of range numbers.</summary>
    public static bool TryParse(CharText text, out ulong raw)
    {
        raw = 0;
        int index = 0;
        bool negative = text[0] == '-';
        if (negative)
            index = 1;
        if (index == text.Length)
            return false;

        ulong magnitude = 0;
        for (; index < text.Length; index++)
        {
            uint digit = (uint)(text[index] - '0');
            if (digit > 9)
                return false;
            if (magnitude > (ulong.MaxValue - digit) / 10)
                return false;
            magnitude = magnitude * 10 + digit;
        }

        if (!negative)
        {
            if (magnitude > EnumTypeTraits<TEnum>.MaxValue)
                return false;
            raw = magnitude;
            return true;
        }

        // "-0" is not canonical, unsigned types have no negative values.
        if (magnitude == 0 || !EnumTypeTraits<TEnum>.IsSigned)
            return false;
        ulong minMagnitude = unchecked(0UL - (ulong)EnumTypeTraits<TEnum>.MinSigned);
        if (magnitude > minMagnitude)
            return false;
        raw = unchecked(0UL - magnitude);
        return true;
    }

    public static bool TryConvert(long number, out ulong raw)
    {
        raw = unchecked((ulong)number);
        if (EnumTypeTraits<TEnum>.IsSigned)
            return number >= EnumTypeTraits<TEnum>.MinSigned && (number < 0 || (ulong)number <= EnumTypeTraits<TEnum>.MaxValue);
        return number >= 0 && (ulong)number <= EnumTypeTraits<TEnum>.MaxValue;
    }

    public static bool TryConvert(ulong number, out ulong raw)
    {
        raw = number;
        return number <= EnumTypeTraits<TEnum>.MaxValue;
    }
}
