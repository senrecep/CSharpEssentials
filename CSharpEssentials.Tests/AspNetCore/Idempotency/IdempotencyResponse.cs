namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

internal sealed record IdempotencyResponse(
    int StatusCode,
    string Body,
    IReadOnlyDictionary<string, string[]> Headers)
{
    public bool Replayed => Header("Idempotency-Replayed") == "true";

    public string? Header(string name) =>
        Headers.TryGetValue(name, out string[]? values) ? string.Join(",", values) : null;

    public static async Task<IdempotencyResponse> ReadAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        Dictionary<string, string[]> headers = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers.Concat(response.Content.Headers))
            headers[header.Key] = [.. header.Value];
        return new IdempotencyResponse((int)response.StatusCode, body, headers);
    }
}
