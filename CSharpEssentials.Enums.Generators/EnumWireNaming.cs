using System.Globalization;
using System.Text;

namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Copy of the runtime <c>EnumNameConverter</c> (the generator cannot reference the runtime assembly). Both copies are pinned against
/// <c>JsonNamingPolicy</c> by tests.
/// </summary>
internal static class EnumWireNaming
{
    public const int Default = 0;
    public const int SnakeCaseLower = 1;
    public const int SnakeCaseUpper = 2;
    public const int KebabCaseLower = 3;
    public const int KebabCaseUpper = 4;
    public const int CamelCase = 5;
    public const int PascalCase = 6;

    private static readonly string[] Names =
        ["Default", "SnakeCaseLower", "SnakeCaseUpper", "KebabCaseLower", "KebabCaseUpper", "CamelCase", "PascalCase"];

    private enum SeparatorState
    {
        NotStarted,
        UppercaseLetter,
        LowercaseLetterOrDigit,
        SpaceSeparator,
    }

    /// <summary>Parses the MSBuild property value; unknown or empty values are <see cref="Default"/>.</summary>
    public static int Parse(string? value)
    {
        if (value is null)
            return Default;

        string trimmed = value.Trim();
        for (int i = 0; i < Names.Length; i++)
        {
            if (string.Equals(Names[i], trimmed, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return Default;
    }

    /// <summary>The effective naming: the enum attribute, then the project setting, then <see cref="SnakeCaseLower"/>.</summary>
    public static int Resolve(int enumNaming, int projectNaming)
    {
        if (enumNaming is > Default and <= PascalCase)
            return enumNaming;
        return projectNaming is > Default and <= PascalCase ? projectNaming : SnakeCaseLower;
    }

    public static string Convert(string name, int naming)
    {
        if (naming == SnakeCaseUpper)
            return ToSeparated(name, '_', lowercase: false);
        if (naming == KebabCaseLower)
            return ToSeparated(name, '-', lowercase: true);
        if (naming == KebabCaseUpper)
            return ToSeparated(name, '-', lowercase: false);
        if (naming == CamelCase)
            return ToCamelCase(name);
        if (naming == PascalCase)
            return name;
        return ToSeparated(name, '_', lowercase: true);
    }

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

    /// <summary>The 3.x/4.x <c>ToSnakeCase()</c> algorithm (<c>HTTPResponse</c> is <c>httpresponse</c>).</summary>
    public static string ToLegacySnakeCase(string name) => ToLegacySeparated(name, '_');

    /// <summary>The 4.x <c>ToKebabCase()</c> algorithm.</summary>
    public static string ToLegacyKebabCase(string name) => ToLegacySeparated(name, '-');

    private static string ToLegacySeparated(string name, char separator)
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
                    sb.Append(separator);
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
