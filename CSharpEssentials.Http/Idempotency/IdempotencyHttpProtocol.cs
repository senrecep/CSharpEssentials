using CSharpEssentials.Core;

namespace CSharpEssentials.Http;

/// <summary>Wire names of the <c>Idempotency-Key</c> contract shared by the client helpers.</summary>
internal static class IdempotencyHttpProtocol
{
    internal const string KeyHeaderName = "Idempotency-Key";
    internal const string ReplayedHeaderName = "Idempotency-Replayed";
    internal const string CodeMember = "code";
    internal const string InFlightCode = "Idempotency.InFlight";
    internal const string ProblemJsonMediaType = "application/problem+json";
    internal const int MaxProblemBytes = 4096;

    internal static string NewKey() => Guider.NewGuid().ToString("D");

    internal static string Validate(string key, string parameterName)
    {
        if (key.Length == 0)
            throw new ArgumentException("An idempotency key cannot be empty.", parameterName);

        foreach (char character in key)
        {
            if (character is < '!' or > '~' or '"' or '\\')
                throw new ArgumentException("An idempotency key may contain only visible ASCII characters other than '\"' and '\\'.", parameterName);
        }

        return key;
    }

    internal static bool HasKey(HttpRequestMessage request)
    {
        if (!request.Headers.TryGetValues(KeyHeaderName, out IEnumerable<string>? values))
            return false;

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return true;
        }

        return false;
    }

    internal static bool IsReplayed(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues(ReplayedHeaderName, out IEnumerable<string>? values))
            return false;

        foreach (string value in values)
        {
            if (string.Equals(value.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    internal static bool IsSafe(HttpMethod method) =>
        method == HttpMethod.Get
        || method == HttpMethod.Head
        || method == HttpMethod.Put
        || method == HttpMethod.Delete
        || method == HttpMethod.Options;
}
