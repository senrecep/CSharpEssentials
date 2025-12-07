using System.Globalization;

namespace CSharpEssentials.Core;


public static class StringExtensions
{
    private const char Underscore = '_';
    private const char Dash = '-';
    private const char Space = ' ';
    private const int One = 1;
    private const int Zero = 0;

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

    public static string ToPascalCase(this string input) => ConvertCase(input, CaseType.Pascal);
    public static string ToCamelCase(this string input) => ConvertCase(input, CaseType.Camel);
    public static string ToKebabCase(this string input) => ConvertCase(input, CaseType.Kebab);
    public static string ToSnakeCase(this string input) => ConvertCase(input, CaseType.Snake);
    public static string ToMacroCase(this string input) => ConvertCase(input, CaseType.Macro);
    public static string ToTrainCase(this string input) => ConvertCase(input, CaseType.Train);
    public static string ToTitleCase(this string input) => ConvertCase(input, CaseType.Title);
    public static string ToUnderscoreCamelCase(this string input) => ConvertCase(input, CaseType.UnderscoreCamel);

    private static string ConvertCase(string input, CaseType caseType)
    {
        ReadOnlySpan<char> value = input;
        bool isFirstCharacter = true;
        bool insertSeparator = caseType is not CaseType.Camel and not CaseType.UnderscoreCamel;

        int extraSize = caseType == CaseType.UnderscoreCamel ? One : Zero;
        int spanSize = caseType is CaseType.Pascal or CaseType.Camel or CaseType.UnderscoreCamel
            ? CalculateSpanSizeForPascalOrCamelCase(value) + extraSize
            : CalculateSpanSizeForKebabOrSnakeCase(value);

        Span<char> newString = stackalloc char[value.Length + spanSize];
        int newIndex = caseType == CaseType.UnderscoreCamel ? One : Zero;

        if (caseType == CaseType.UnderscoreCamel)
        {
            newString[Zero] = Underscore;
        }

        UnicodeCategory current = UnicodeCategory.OtherSymbol;

        int startIndex = caseType == CaseType.UnderscoreCamel ? One : Zero;
        for (int i = startIndex; i < value.Length; i++)
        {
            UnicodeCategory previous = current;
            current = char.GetUnicodeCategory(value[i]);
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

#if NET8_0_OR_GREATER
        return new string(newString[..newIndex]);
#else
        return new(newString.Slice(0, newIndex));
#endif
    }

    private static char GetCharacterCase(char c, CaseType caseType, bool insertSeparator, bool isFirstCharacter)
    {
        return caseType switch
        {
            CaseType.Macro => char.ToUpperInvariant(c),
            CaseType.Kebab or CaseType.Snake => char.ToLowerInvariant(c),
            CaseType.Camel or CaseType.UnderscoreCamel => insertSeparator && !isFirstCharacter
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
        CaseType.Kebab or CaseType.Train => Dash,
        CaseType.Snake or CaseType.Macro => Underscore,
        CaseType.Title => Space,
        CaseType.Pascal or CaseType.Camel or CaseType.UnderscoreCamel => throw new ArgumentException("No separator needed for this case type"),
        _ => throw new ArgumentException("Invalid case type")
    };


    private static int CalculateSpanSizeForKebabOrSnakeCase(ReadOnlySpan<char> text)
    {
        UnicodeCategory previous = char.GetUnicodeCategory(text[Zero]);
        int skips = IsSpecialCharacter(previous) ? One : Zero;
        int divs = Zero;
        for (int i = One; i < text.Length; i++)
        {
            UnicodeCategory current = char.GetUnicodeCategory(text[i]);
            skips += IsSpecialCharacter(current) ? One : Zero;
            divs += CheckCategory(previous, current) ? One : Zero;
            previous = current;
        }
        return divs - skips;
    }


    private static int CalculateSpanSizeForPascalOrCamelCase(ReadOnlySpan<char> text)
    {
        int skips = Zero;
        for (int i = Zero; i < text.Length - One; i++)
        {
            UnicodeCategory current = char.GetUnicodeCategory(text[i]);
            skips -= IsSpecialCharacter(current) ? One : Zero;
        }
        return skips;
    }
    private static bool CheckCategory(UnicodeCategory previous, UnicodeCategory current) =>
        previous != current && (current is UnicodeCategory.UppercaseLetter || current is UnicodeCategory.DecimalDigitNumber);

    private static bool InsertSeparator(this bool insertSeparator, UnicodeCategory previous, UnicodeCategory current) =>
        CheckCategory(previous, current) || insertSeparator;

    private static bool IsSpecialCharacter(UnicodeCategory category) =>
    category is not UnicodeCategory.UppercaseLetter
             and not UnicodeCategory.LowercaseLetter
             and not UnicodeCategory.DecimalDigitNumber;
}