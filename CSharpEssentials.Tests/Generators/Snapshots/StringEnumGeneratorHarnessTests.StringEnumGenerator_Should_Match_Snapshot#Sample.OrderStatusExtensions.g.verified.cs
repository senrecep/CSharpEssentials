//HintName: Sample.OrderStatusExtensions.g.cs
#nullable enable
namespace Sample
{
    public static class OrderStatusExtensions
    {
        public const string PendingSnakeCase = "pending";
        public const string PendingKebabCase = "pending";
        public const string InProgressSnakeCase = "in_progress";
        public const string InProgressKebabCase = "in-progress";
        public const string ShippedSnakeCase = "shipped";
        public const string ShippedKebabCase = "shipped";

        public static string ToOptimizedString(this Sample.OrderStatus value)
        {
            switch (value)
            {
                case Sample.OrderStatus.Pending:
                    return nameof(Sample.OrderStatus.Pending);
                case Sample.OrderStatus.InProgress:
                    return nameof(Sample.OrderStatus.InProgress);
                case Sample.OrderStatus.Shipped:
                    return nameof(Sample.OrderStatus.Shipped);
                default:
                    return value.ToString();
            }
        }

        public static string ToSnakeCase(this Sample.OrderStatus value)
        {
            switch (value)
            {
                case Sample.OrderStatus.Pending:
                    return "pending";
                case Sample.OrderStatus.InProgress:
                    return "in_progress";
                case Sample.OrderStatus.Shipped:
                    return "shipped";
                default:
                    return ToSnakeCaseFallback(value.ToOptimizedString());
            }
        }

        public static string ToKebabCase(this Sample.OrderStatus value)
        {
            switch (value)
            {
                case Sample.OrderStatus.Pending:
                    return "pending";
                case Sample.OrderStatus.InProgress:
                    return "in-progress";
                case Sample.OrderStatus.Shipped:
                    return "shipped";
                default:
                    return ToKebabCaseFallback(value.ToOptimizedString());
            }
        }

        public static string ToLowerCase(this Sample.OrderStatus value)
        {
            switch (value)
            {
                case Sample.OrderStatus.Pending:
                    return nameof(Sample.OrderStatus.Pending).ToLowerInvariant();
                case Sample.OrderStatus.InProgress:
                    return nameof(Sample.OrderStatus.InProgress).ToLowerInvariant();
                case Sample.OrderStatus.Shipped:
                    return nameof(Sample.OrderStatus.Shipped).ToLowerInvariant();
                default:
                    return value.ToString().ToLowerInvariant();
            }
        }

        public static string ToUpperCase(this Sample.OrderStatus value)
        {
            switch (value)
            {
                case Sample.OrderStatus.Pending:
                    return nameof(Sample.OrderStatus.Pending).ToUpperInvariant();
                case Sample.OrderStatus.InProgress:
                    return nameof(Sample.OrderStatus.InProgress).ToUpperInvariant();
                case Sample.OrderStatus.Shipped:
                    return nameof(Sample.OrderStatus.Shipped).ToUpperInvariant();
                default:
                    return value.ToString().ToUpperInvariant();
            }
        }

        public static bool IsDefined(string name)
        {
            switch (name)
            {
                case nameof(Sample.OrderStatus.Pending):
                case nameof(Sample.OrderStatus.InProgress):
                case nameof(Sample.OrderStatus.Shipped):
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryParse(string? name, out Sample.OrderStatus value)
        {
            switch (name)
            {
                case nameof(Sample.OrderStatus.Pending):
                    value = Sample.OrderStatus.Pending;
                    return true;
                case nameof(Sample.OrderStatus.InProgress):
                    value = Sample.OrderStatus.InProgress;
                    return true;
                case nameof(Sample.OrderStatus.Shipped):
                    value = Sample.OrderStatus.Shipped;
                    return true;
            }

            int numericValue;
            if (name != null && int.TryParse(name, out numericValue))
            {
                value = (Sample.OrderStatus)numericValue;
                return true;
            }

            value = (Sample.OrderStatus)0;
            return false;
        }

        public static Sample.OrderStatus Parse(string? name)
        {
            Sample.OrderStatus value;
            if (TryParse(name, out value))
            {
                return value;
            }

            throw new global::System.ArgumentException("Requested value '" + name + "' was not found.");
        }

        public static string[] GetNames()
        {
            return new string[] { nameof(Sample.OrderStatus.Pending), nameof(Sample.OrderStatus.InProgress), nameof(Sample.OrderStatus.Shipped) };
        }

        public static Sample.OrderStatus[] GetValues()
        {
            return new Sample.OrderStatus[] { Sample.OrderStatus.Pending, Sample.OrderStatus.InProgress, Sample.OrderStatus.Shipped };
        }

        public static int AsUnderlyingType(this Sample.OrderStatus value)
        {
            return (int)value;
        }

        private static string ToSnakeCaseFallback(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            global::System.Text.StringBuilder sb = new global::System.Text.StringBuilder(input.Length + 4);
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
            global::System.Text.StringBuilder sb = new global::System.Text.StringBuilder(input.Length + 4);
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

        private static bool CheckCategory(global::System.Globalization.UnicodeCategory previous, global::System.Globalization.UnicodeCategory current)
        {
            return previous != current && (current == global::System.Globalization.UnicodeCategory.UppercaseLetter || current == global::System.Globalization.UnicodeCategory.DecimalDigitNumber || IsSpecialCharacter(previous) && !IsSpecialCharacter(current));
        }

        private static bool IsSpecialCharacter(global::System.Globalization.UnicodeCategory category)
        {
            return category != global::System.Globalization.UnicodeCategory.UppercaseLetter
                && category != global::System.Globalization.UnicodeCategory.LowercaseLetter
                && category != global::System.Globalization.UnicodeCategory.DecimalDigitNumber;
        }
    }
}
