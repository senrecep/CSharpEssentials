using System.Globalization;
using System.Text;

namespace CSharpEssentials.Enums;

/// <summary>
/// The wire naming algorithms. The generator has an identical copy; both are pinned against <c>JsonNamingPolicy</c> by tests.
/// </summary>
internal static class EnumNameConverter
{
    private enum SeparatorState
    {
        NotStarted,
        UppercaseLetter,
        LowercaseLetterOrDigit,
        SpaceSeparator,
    }

    public static string Convert(string name, EnumNaming naming) => naming switch
    {
        EnumNaming.SnakeCaseUpper => ToSeparated(name, '_', lowercase: false),
        EnumNaming.KebabCaseLower => ToSeparated(name, '-', lowercase: true),
        EnumNaming.KebabCaseUpper => ToSeparated(name, '-', lowercase: false),
        EnumNaming.CamelCase => ToCamelCase(name),
        EnumNaming.PascalCase => name,
        EnumNaming.Default or EnumNaming.SnakeCaseLower => ToSeparated(name, '_', lowercase: true),
        _ => ToSeparated(name, '_', lowercase: true),
    };

    /// <summary>Port of the <c>System.Text.Json</c> separator naming policy.</summary>
    public static string ToSeparated(string name, char separator, bool lowercase)
    {
        StringBuilder sb = new(name.Length + 4);
        SeparatorState state = SeparatorState.NotStarted;
        for (int i = 0; i < name.Length; i++)
        {
            char current = name[i];
            UnicodeCategory category = char.GetUnicodeCategory(current);
            if (category == UnicodeCategory.UppercaseLetter)
            {
                if (state is SeparatorState.LowercaseLetterOrDigit or SeparatorState.SpaceSeparator ||
                    state == SeparatorState.UppercaseLetter && i + 1 < name.Length && char.IsLower(name[i + 1]))
                {
                    sb.Append(separator);
                }

                sb.Append(lowercase ? char.ToLowerInvariant(current) : current);
                state = SeparatorState.UppercaseLetter;
            }
            else if (category is UnicodeCategory.LowercaseLetter or UnicodeCategory.DecimalDigitNumber)
            {
                if (state == SeparatorState.SpaceSeparator)
                    sb.Append(separator);
                sb.Append(!lowercase && category == UnicodeCategory.LowercaseLetter ? char.ToUpperInvariant(current) : current);
                state = SeparatorState.LowercaseLetterOrDigit;
            }
            else if (category == UnicodeCategory.SpaceSeparator)
            {
                if (state != SeparatorState.NotStarted)
                    state = SeparatorState.SpaceSeparator;
            }
            else
            {
                sb.Append(current);
                state = SeparatorState.NotStarted;
            }
        }

        return sb.ToString();
    }

    /// <summary>Port of <c>JsonNamingPolicy.CamelCase</c>.</summary>
    public static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return name;

        char[] chars = name.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == 1 && !char.IsUpper(chars[i]))
                break;

            bool hasNext = i + 1 < chars.Length;
            if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
            {
                if (chars[i + 1] == ' ')
                    chars[i] = char.ToLowerInvariant(chars[i]);
                break;
            }

            chars[i] = char.ToLowerInvariant(chars[i]);
        }

        return new string(chars);
    }

    /// <summary>The 3.x/4.x <c>ToSnakeCase()</c> algorithm (<c>HTTPResponse</c> is <c>httpresponse</c>), read as an alias.</summary>
    public static string ToLegacySnakeCase(string name)
    {
        StringBuilder sb = new(name.Length + 4);
        UnicodeCategory previous = UnicodeCategory.OtherSymbol;
        bool isFirst = true;
        foreach (char c in name)
        {
            UnicodeCategory current = char.GetUnicodeCategory(c);
            if (!IsSpecial(current))
            {
                bool insertSeparator = previous != current &&
                    (current is UnicodeCategory.UppercaseLetter or UnicodeCategory.DecimalDigitNumber || IsSpecial(previous) && !IsSpecial(current));
                if (insertSeparator && !isFirst)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
                isFirst = false;
            }

            previous = current;
        }

        return sb.ToString();
    }

    private static bool IsSpecial(UnicodeCategory category) =>
        category is not UnicodeCategory.UppercaseLetter and not UnicodeCategory.LowercaseLetter and not UnicodeCategory.DecimalDigitNumber;
}
