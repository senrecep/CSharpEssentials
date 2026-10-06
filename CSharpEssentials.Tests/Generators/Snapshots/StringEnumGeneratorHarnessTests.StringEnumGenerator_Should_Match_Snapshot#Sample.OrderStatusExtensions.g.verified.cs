//HintName: Sample.OrderStatusExtensions.g.cs
#nullable enable
namespace Sample;
public static class OrderStatusExtensions
{
    public const string PendingSnakeCase = "pending";
    public const string PendingKebabCase = "pending";
    public const string InProgressSnakeCase = "in_progress";
    public const string InProgressKebabCase = "in-progress";
    public const string ShippedSnakeCase = "shipped";
    public const string ShippedKebabCase = "shipped";

    public static string ToOptimizedString(this Sample.OrderStatus value) => value switch
    {
        Sample.OrderStatus.Pending => nameof(Sample.OrderStatus.Pending),
        Sample.OrderStatus.InProgress => nameof(Sample.OrderStatus.InProgress),
        Sample.OrderStatus.Shipped => nameof(Sample.OrderStatus.Shipped),
        _ => value.ToString()
    };

    public static string ToSnakeCase(this Sample.OrderStatus value) => value switch
    {
        Sample.OrderStatus.Pending => "pending",
        Sample.OrderStatus.InProgress => "in_progress",
        Sample.OrderStatus.Shipped => "shipped",
        _ => ToSnakeCaseFallback(value.ToOptimizedString())
    };

    public static string ToKebabCase(this Sample.OrderStatus value) => value switch
    {
        Sample.OrderStatus.Pending => "pending",
        Sample.OrderStatus.InProgress => "in-progress",
        Sample.OrderStatus.Shipped => "shipped",
        _ => ToKebabCaseFallback(value.ToOptimizedString())
    };

    public static string ToLowerCase(this Sample.OrderStatus value) => value switch
    {
        Sample.OrderStatus.Pending => nameof(Sample.OrderStatus.Pending).ToLowerInvariant(),
        Sample.OrderStatus.InProgress => nameof(Sample.OrderStatus.InProgress).ToLowerInvariant(),
        Sample.OrderStatus.Shipped => nameof(Sample.OrderStatus.Shipped).ToLowerInvariant(),
        _ => value.ToString().ToLowerInvariant()
    };

    public static string ToUpperCase(this Sample.OrderStatus value) => value switch
    {
        Sample.OrderStatus.Pending => nameof(Sample.OrderStatus.Pending).ToUpperInvariant(),
        Sample.OrderStatus.InProgress => nameof(Sample.OrderStatus.InProgress).ToUpperInvariant(),
        Sample.OrderStatus.Shipped => nameof(Sample.OrderStatus.Shipped).ToUpperInvariant(),
        _ => value.ToString().ToUpperInvariant()
    };

    public static bool IsDefined(string name) => name switch
    {
        nameof(Sample.OrderStatus.Pending) => true,
        nameof(Sample.OrderStatus.InProgress) => true,
        nameof(Sample.OrderStatus.Shipped) => true,
        _ => false
    };

    public static bool TryParse(string? name, out Sample.OrderStatus value)
    {
        switch (name)
        {
            case string s when s.Equals(nameof(Sample.OrderStatus.Pending), global::System.StringComparison.Ordinal):
                value = Sample.OrderStatus.Pending;
                return true;
            case string s when s.Equals(nameof(Sample.OrderStatus.InProgress), global::System.StringComparison.Ordinal):
                value = Sample.OrderStatus.InProgress;
                return true;
            case string s when s.Equals(nameof(Sample.OrderStatus.Shipped), global::System.StringComparison.Ordinal):
                value = Sample.OrderStatus.Shipped;
                return true;
            case string s when int.TryParse(s, out var numericValue):
                value = (Sample.OrderStatus)numericValue;
                return true;
            default:
                value = (Sample.OrderStatus)0;
                return false;
        }
    }

    public static Sample.OrderStatus Parse(string? name) =>
        TryParse(name, out var value) ? value : ThrowValueNotFound(name);

    private static Sample.OrderStatus ThrowValueNotFound(string? name) =>
        throw new global::System.ArgumentException($"Requested value '{name}' was not found.");

    public static string[] GetNames() =>
        [nameof(Sample.OrderStatus.Pending), nameof(Sample.OrderStatus.InProgress), nameof(Sample.OrderStatus.Shipped)];

    public static Sample.OrderStatus[] GetValues() =>
        [Sample.OrderStatus.Pending, Sample.OrderStatus.InProgress, Sample.OrderStatus.Shipped];

    public static int AsUnderlyingType(this Sample.OrderStatus value) => (int)value;

    private static string ToSnakeCaseFallback(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        global::System.Text.StringBuilder sb = new(input.Length + 4);
        global::System.Globalization.UnicodeCategory previous = global::System.Globalization.UnicodeCategory.OtherSymbol;
        bool isFirst = true;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            global::System.Globalization.UnicodeCategory current = char.GetUnicodeCategory(c);
            bool insertSeparator = CheckCategory(previous, current);
            bool isSpecial = IsSpecialCharacter(current);
            if (!isSpecial)
            {
                if (insertSeparator && !isFirst) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
                isFirst = false;
            }
            previous = current;
        }
        return sb.ToString();
    }

    private static string ToKebabCaseFallback(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        global::System.Text.StringBuilder sb = new(input.Length + 4);
        global::System.Globalization.UnicodeCategory previous = global::System.Globalization.UnicodeCategory.OtherSymbol;
        bool isFirst = true;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            global::System.Globalization.UnicodeCategory current = char.GetUnicodeCategory(c);
            bool insertSeparator = CheckCategory(previous, current);
            bool isSpecial = IsSpecialCharacter(current);
            if (!isSpecial)
            {
                if (insertSeparator && !isFirst) sb.Append('-');
                sb.Append(char.ToLowerInvariant(c));
                isFirst = false;
            }
            previous = current;
        }
        return sb.ToString();
    }

    private static bool CheckCategory(global::System.Globalization.UnicodeCategory previous, global::System.Globalization.UnicodeCategory current) =>
        previous != current && (current is global::System.Globalization.UnicodeCategory.UppercaseLetter || current is global::System.Globalization.UnicodeCategory.DecimalDigitNumber || IsSpecialCharacter(previous) && !IsSpecialCharacter(current));

    private static bool IsSpecialCharacter(global::System.Globalization.UnicodeCategory category) =>
        category is not global::System.Globalization.UnicodeCategory.UppercaseLetter
             and not global::System.Globalization.UnicodeCategory.LowercaseLetter
             and not global::System.Globalization.UnicodeCategory.DecimalDigitNumber;
}
