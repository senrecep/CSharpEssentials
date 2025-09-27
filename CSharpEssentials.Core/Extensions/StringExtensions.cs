using System.Globalization;
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Core;


public static class StringExtensions
{
    private const char _underscore = '_';
    private const char _dash = '-';
    private const char _space = ' ';
    private const int _one = 1;
    private const int _zero = 0;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToPascalCase(this string input) => ConvertCase(input, CaseType.Pascal);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToCamelCase(this string input) => ConvertCase(input, CaseType.Camel);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToKebabCase(this string input) => ConvertCase(input, CaseType.Kebab);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToSnakeCase(this string input) => ConvertCase(input, CaseType.Snake);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToMacroCase(this string input) => ConvertCase(input, CaseType.Macro);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTrainCase(this string input) => ConvertCase(input, CaseType.Train);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTitleCase(this string input) => ConvertCase(input, CaseType.Title);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToUnderscoreCamelCase(this string input) => ConvertCase(input, CaseType.UnderscoreCamel);

    private static string ConvertCase(string input, CaseType caseType)
    {
        ReadOnlySpan<char> value = input;
        bool isFirstCharacter = true;
        bool insertSeparator = caseType is not CaseType.Camel and not CaseType.UnderscoreCamel;

        int extraSize = caseType == CaseType.UnderscoreCamel ? _one : _zero;
        int spanSize = (caseType is CaseType.Pascal or CaseType.Camel or CaseType.UnderscoreCamel)
            ? CalculateSpanSizeForPascalOrCamelCase(value) + extraSize
            : CalculateSpanSizeForKebabOrSnakeCase(value);

        Span<char> newString = stackalloc char[value.Length + spanSize];
        int newIndex = caseType == CaseType.UnderscoreCamel ? _one : _zero;

        if (caseType == CaseType.UnderscoreCamel)
        {
            newString[_zero] = _underscore;
        }

        UnicodeCategory previous;
        UnicodeCategory current = UnicodeCategory.OtherSymbol;

        int startIndex = caseType == CaseType.UnderscoreCamel ? _one : _zero;
        for (int i = startIndex; i < value.Length; i++)
        {
            (previous, current) = (current, char.GetUnicodeCategory(value[i]));
            insertSeparator = insertSeparator.InsertSeparator(previous, current);

            if (!IsSpecialCharacter(current))
            {
                if (insertSeparator && !isFirstCharacter && NeedsSeparator(caseType))
                    newString[newIndex++] = GetSeparator(caseType);

                newString[newIndex] = GetCharacterCase(value[i], caseType, insertSeparator, isFirstCharacter);
                isFirstCharacter = false;
                insertSeparator = false;
                newIndex++;
            }
        }

        return new string(newString[..newIndex]);
    }

    private static char GetCharacterCase(char c, CaseType caseType, bool insertSeparator, bool isFirstCharacter)
    {
        return caseType switch
        {
            CaseType.Macro => char.ToUpperInvariant(c),
            CaseType.Kebab or CaseType.Snake => char.ToLowerInvariant(c),
            CaseType.Camel or CaseType.UnderscoreCamel => (insertSeparator && !isFirstCharacter)
                ? char.ToUpperInvariant(c)
                : char.ToLowerInvariant(c),
            CaseType.Pascal => insertSeparator ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c),
            CaseType.Train => insertSeparator ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c),
            CaseType.Title => insertSeparator ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c),
            _ => throw new ArgumentOutOfRangeException(nameof(caseType), caseType, null)
        };
    }

    private static bool NeedsSeparator(CaseType caseType) =>
        caseType is CaseType.Kebab or CaseType.Snake or CaseType.Macro or CaseType.Train or CaseType.Title;

    private static char GetSeparator(CaseType caseType) => caseType switch
    {
        CaseType.Kebab or CaseType.Train => _dash,
        CaseType.Snake or CaseType.Macro => _underscore,
        CaseType.Title => _space,
        CaseType.Pascal or CaseType.Camel or CaseType.UnderscoreCamel => throw new ArgumentException("No separator needed for this case type"),
        _ => throw new ArgumentException("Invalid case type")
    };


    private static int CalculateSpanSizeForKebabOrSnakeCase(ReadOnlySpan<char> text)
    {
        UnicodeCategory previous = char.GetUnicodeCategory(text[_zero]);
        UnicodeCategory current;
        int skips = IsSpecialCharacter(previous) ? _one : _zero;
        int divs = _zero;
        for (int i = _one; i < text.Length; i++)
        {
            current = char.GetUnicodeCategory(text[i]);
            skips += IsSpecialCharacter(current) ? _one : _zero;
            divs += CheckCategory(previous, current) ? _one : _zero;
            previous = current;
        }
        return divs - skips;
    }


    private static int CalculateSpanSizeForPascalOrCamelCase(ReadOnlySpan<char> text)
    {
        UnicodeCategory current;
        int skips = _zero;
        for (int i = _zero; i < text.Length - _one; i++)
        {
            current = char.GetUnicodeCategory(text[i]);
            skips -= IsSpecialCharacter(current) ? _one : _zero;
        }
        return skips;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckCategory(UnicodeCategory previous, UnicodeCategory current) =>
        previous != current && (current is UnicodeCategory.UppercaseLetter || current is UnicodeCategory.DecimalDigitNumber);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InsertSeparator(this bool insertSeparator, UnicodeCategory previous, UnicodeCategory current) =>
        CheckCategory(previous, current) || insertSeparator;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSpecialCharacter(UnicodeCategory category) =>
    category is not UnicodeCategory.UppercaseLetter
             and not UnicodeCategory.LowercaseLetter
             and not UnicodeCategory.DecimalDigitNumber;
}