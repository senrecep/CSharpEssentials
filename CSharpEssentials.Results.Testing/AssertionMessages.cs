using System.Globalization;
using System.Text;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern.Testing;

internal static class AssertionMessages
{
    internal static string Errors(IReadOnlyList<Error> errors)
    {
        var builder = new StringBuilder();
        builder.Append(errors.Count).Append(errors.Count == 1 ? " error:" : " errors:");
        for (int i = 0; i < errors.Count; i++)
        {
            Error error = errors[i];
            builder.AppendLine()
                .Append("  [").Append(i).Append("] ")
                .Append(error.Type).Append(' ')
                .Append(error.Code).Append(": ")
                .Append(error.Description);
            if (error.Metadata is { Count: > 0 } metadata)
                builder.Append(" metadata ").Append(metadata);
        }
        return builder.ToString();
    }

    internal static string Case(object? value)
    {
        if (value is null)
            return "null";
        string name = value.GetType().Name;
        string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        return text.StartsWith(name, StringComparison.Ordinal) ? text : name + " " + text;
    }
}
