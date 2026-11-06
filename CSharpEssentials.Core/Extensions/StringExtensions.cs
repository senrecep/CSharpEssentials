using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Core;


public static class StringExtensions
{
    private const char Underscore = '_';
    private const char Dash = '-';
    private const char Space = ' ';
    private const int One = 1;
    private const int Zero = 0;
    private const int StackAllocThreshold = 512;

    private enum CaseType
    {
        Pascal,
        Camel,
        UnderscoreCamel,
        Kebab,
        Snake,
        Macro,
        Train,
        Title
    }

    public static string ToPascalCase(this string input, CultureInfo? culture = null) => ConvertCase(input, CaseType.Pascal, culture);
    public static string ToCamelCase(this string input, CultureInfo? culture = null) => ConvertCase(input, CaseType.Camel, culture);
    public static string ToKebabCase(this string input, CultureInfo? culture = null) => ConvertCase(input, CaseType.Kebab, culture);
    public static string ToSnakeCase(this string input, CultureInfo? culture = null) => ConvertCase(input, CaseType.Snake, culture);
    public static string ToMacroCase(this string input, CultureInfo? culture = null) => ConvertCase(input, CaseType.Macro, culture);
    public static string ToTrainCase(this string input, CultureInfo? culture = null) => ConvertCase(input, CaseType.Train, culture);
    public static string ToTitleCase(this string input, CultureInfo? culture = null) => ConvertCase(input, CaseType.Title, culture);
    public static string ToUnderscoreCamelCase(this string input, CultureInfo? culture = null) => ConvertCase(input, CaseType.UnderscoreCamel, culture);

    private static string ConvertCase(string input, CaseType caseType, CultureInfo? culture = null)
    {
        if (input.Length == Zero)
            return string.Empty;

        ReadOnlySpan<char> value = input.AsSpan();
        int bufferLength = NeedsSeparator(caseType) ? value.Length * 2 : value.Length + One;
        char[]? rented = bufferLength <= StackAllocThreshold ? null : ArrayPool<char>.Shared.Rent(bufferLength);
        Span<char> newString = rented is null ? stackalloc char[bufferLength] : rented;
        try
        {
            int newIndex = WriteCase(value, newString, caseType, culture);

#if NET8_0_OR_GREATER
            return new string(newString[..newIndex]);
#elif NETSTANDARD2_1
            return new string(newString.Slice(0, newIndex));
#else
            return new string(newString.Slice(0, newIndex).ToArray());
#endif
        }
        finally
        {
            if (rented is not null)
                ArrayPool<char>.Shared.Return(rented);
        }
    }

    private static int WriteCase(ReadOnlySpan<char> value, Span<char> newString, CaseType caseType, CultureInfo? culture)
    {
        bool needsSeparator = NeedsSeparator(caseType);
        char separator = needsSeparator ? GetSeparator(caseType) : default;
        bool upperAlways = caseType == CaseType.Macro;
        bool upperAtWordStart = caseType is not CaseType.Kebab and not CaseType.Snake and not CaseType.Macro;
        bool upperFirstCharacter = caseType is CaseType.Pascal or CaseType.Train or CaseType.Title;
        bool isFirstCharacter = true;
        bool insertSeparator = caseType is not CaseType.Camel and not CaseType.UnderscoreCamel;
        int newIndex = Zero;

        if (caseType == CaseType.UnderscoreCamel)
            newString[newIndex++] = Underscore;

        UnicodeCategory current = UnicodeCategory.OtherSymbol;

        for (int i = Zero; i < value.Length; i++)
        {
            UnicodeCategory previous = current;
            current = GetCategory(value[i]);
            insertSeparator = insertSeparator.InsertSeparator(previous, current);

            if (IsSpecialCharacter(current))
                continue;

            if (insertSeparator && !isFirstCharacter && needsSeparator)
                newString[newIndex++] = separator;

            bool upper = upperAlways || insertSeparator && upperAtWordStart && (!isFirstCharacter || upperFirstCharacter);
            newString[newIndex++] = upper ? ToUpper(value[i], culture) : ToLower(value[i], culture);
            isFirstCharacter = false;
            insertSeparator = false;
        }

        return newIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char ToUpper(char c, CultureInfo? culture)
    {
        if (culture is not null)
            return char.ToUpper(c, culture);
        if (c >= 128)
            return char.ToUpperInvariant(c);
        return c is >= 'a' and <= 'z' ? (char)(c - 32) : c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char ToLower(char c, CultureInfo? culture)
    {
        if (culture is not null)
            return char.ToLower(c, culture);
        if (c >= 128)
            return char.ToLowerInvariant(c);
        return c is >= 'A' and <= 'Z' ? (char)(c + 32) : c;
    }

    private static bool NeedsSeparator(CaseType caseType) =>
        caseType is CaseType.Kebab or CaseType.Snake or CaseType.Macro or CaseType.Train or CaseType.Title;

    private static char GetSeparator(CaseType caseType) => caseType switch
    {
        CaseType.Kebab or CaseType.Train => Dash,
        CaseType.Snake or CaseType.Macro => Underscore,
        CaseType.Title => Space,
        CaseType.Pascal or CaseType.Camel or CaseType.UnderscoreCamel => throw new ArgumentException("No separator needed for this case type"),
        _ => throw new ArgumentException("Invalid case type")
    };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UnicodeCategory GetCategory(char c)
    {
        if (c >= 128)
            return char.GetUnicodeCategory(c);
        if (c is >= 'a' and <= 'z')
            return UnicodeCategory.LowercaseLetter;
        if (c is >= 'A' and <= 'Z')
            return UnicodeCategory.UppercaseLetter;
        return c is >= '0' and <= '9' ? UnicodeCategory.DecimalDigitNumber : UnicodeCategory.OtherSymbol;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckCategory(UnicodeCategory previous, UnicodeCategory current) =>
        previous != current && (current is UnicodeCategory.UppercaseLetter or UnicodeCategory.TitlecaseLetter or UnicodeCategory.DecimalDigitNumber
            || IsSpecialCharacter(previous) && !IsSpecialCharacter(current));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InsertSeparator(this bool insertSeparator, UnicodeCategory previous, UnicodeCategory current) =>
        CheckCategory(previous, current) || insertSeparator;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSpecialCharacter(UnicodeCategory category) =>
    category is not UnicodeCategory.UppercaseLetter
             and not UnicodeCategory.LowercaseLetter
             and not UnicodeCategory.TitlecaseLetter
             and not UnicodeCategory.ModifierLetter
             and not UnicodeCategory.OtherLetter
             and not UnicodeCategory.NonSpacingMark
             and not UnicodeCategory.SpacingCombiningMark
             and not UnicodeCategory.DecimalDigitNumber;
}
