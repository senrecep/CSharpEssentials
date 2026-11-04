using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// Self-check run from every [GlobalSetup]: each column must return the same result for the same input, otherwise the
/// benchmark refuses to run.
/// </summary>
internal static class Parity
{
    public static string Of(int value) => value.ToString(CultureInfo.InvariantCulture);

    public static string Of(string? value) => value ?? "<null>";

    public static string Of(Probe probe) =>
        $"{(int)probe.Status}|{Of(probe.MediaType)}|{Hash(probe.Body)}|calls={Of(probe.HandlerCalls)}";

    public static string Of(HttpStatusCode status, byte[] body, int handlerCalls) =>
        $"{(int)status}|{Hash(body)}|calls={Of(handlerCalls)}";

    public static string Problem(Probe probe) =>
        $"{(int)probe.Status}|{Of(probe.MediaType)}|status={StatusMember(probe.Body)}|calls={Of(probe.HandlerCalls)}";

    public static string Hash(byte[] body) => Convert.ToHexString(SHA256.HashData(body));

    public static string Text(byte[] body) => System.Text.Encoding.UTF8.GetString(body);

    public static string StatusMember(byte[] body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.TryGetProperty("status", out JsonElement status) ? status.GetRawText() : "<none>";
    }

    public static int ErrorCount(byte[] body)
    {
        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("errors", out JsonElement errors))
            return 0;
        return errors.ValueKind == JsonValueKind.Array ? errors.GetArrayLength() : errors.EnumerateObject().Count();
    }

    public static void Same(string scenario, params (string Library, string Outcome)[] outcomes)
    {
        (string firstLibrary, string expected) = outcomes[0];
        foreach ((string library, string outcome) in outcomes)
        {
            if (!string.Equals(outcome, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Parity check failed in {scenario}: {firstLibrary} gave '{expected}' but {library} gave '{outcome}'.");
            }
        }
    }

    public static void True(string scenario, bool condition, string detail)
    {
        if (!condition)
            throw new InvalidOperationException($"Parity check failed in {scenario}: {detail}.");
    }
}
