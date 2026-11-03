using System.Collections;
using System.Globalization;
using System.Text;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

internal static class ErrorParity
{
    public static string Full(Error e) => e.Code + "|" + e.Type + "|" + Core(e.Description, e.Metadata);

    public static string Full(Eo.Error e) => e.Code + "|" + e.Type + "|" + Core(e.Description, e.Metadata);

    public static string Core(Error e) => Core(e.Description, e.Metadata);

    public static string Core(Eo.Error e) => Core(e.Description, e.Metadata);

    public static string Core(Fr.IError e) => Core(e.Message, e.Metadata);

    public static string Has(string text, string part) =>
        text.Contains(part, StringComparison.Ordinal) ? "has:" + part : text;

    public static string Of(Result<int[]> r) => r.IsSuccess ? Values(r.Value) : Fail(r.Errors.Length, r.Errors[0].Description);

    public static string Of(Cfe.Result<int[]> r) => r.IsSuccess ? Values(r.Value) : Fail(r.Error.Split(", ").Length, r.Error.Split(", ")[0]);

    public static string Of(Eo.ErrorOr<List<int>> r) => r.IsError ? Fail(r.Errors.Count, r.FirstError.Description) : Values(r.Value);

    public static string Of(Fr.Result<IEnumerable<int>> r) => r.IsSuccess ? Values(r.Value) : Fail(r.Errors.Count, r.Errors[0].Message);

    public static string Of(Maybe<string> m) => m.HasValue ? "ok:" + m.GetValueOrDefault(string.Empty) : "none";

    public static string Of(Cfe.Maybe<string> m) => m.HasValue ? "ok:" + m.GetValueOrDefault(string.Empty) : "none";

    public static string Found(string? value) => value is null ? "none" : "ok:" + value;

    public static string Found(int? value) => value is { } v ? "ok:" + v.ToString(CultureInfo.InvariantCulture) : "none";

    private static string Values(IEnumerable<int> values)
    {
        int count = 0;
        int sum = 0;
        foreach (int value in values)
        {
            count++;
            sum += value;
        }

        return "ok:" + count.ToString(CultureInfo.InvariantCulture) + ":" + sum.ToString(CultureInfo.InvariantCulture);
    }

    private static string Fail(int count, string message) =>
        "fail:" + count.ToString(CultureInfo.InvariantCulture) + ":" + message;

    private static string Core(string description, IDictionary? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return description + "|-";
        var sb = new StringBuilder(description).Append('|');
        foreach (DictionaryEntry entry in metadata)
            sb.Append(entry.Key).Append('=').Append(Convert.ToString(entry.Value, CultureInfo.InvariantCulture)).Append(';');
        return sb.ToString();
    }
}
