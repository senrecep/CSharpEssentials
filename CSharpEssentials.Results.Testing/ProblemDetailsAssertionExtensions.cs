using System.Globalization;
using System.Text.Json;

namespace CSharpEssentials.ResultPattern.Testing;

/// <summary>
/// Assertions for HTTP responses that carry an RFC 9457 problem details body, such as the ones
/// <c>CSharpEssentials.AspNetCore</c> writes. They read the body as JSON, so they need no ASP.NET Core reference.
/// A failed assertion throws <see cref="ResultAssertionException"/> whose message shows the request, the status and the body.
/// </summary>
public static class ProblemDetailsAssertionExtensions
{
    private const string ProblemMediaType = "application/problem+json";
    private const string ErrorsKey = "errors";
    private const string ErrorCodesKey = "errorCodes";

    /// <summary>
    /// Asserts that the response is a problem details response and returns its body.
    /// </summary>
    /// <param name="response">The response under test.</param>
    /// <param name="status">The expected status code, or <see langword="null"/> to accept any.</param>
    /// <param name="code">
    /// An error code the problem must carry, or <see langword="null"/> to skip the check. It is looked up in
    /// <c>errorCodes</c> and in the <c>errors</c> field, whether that is a list of <c>{ code, description }</c> or an
    /// object keyed by code.
    /// </param>
    /// <returns>The parsed body, which stays valid after the response is disposed.</returns>
    /// <exception cref="ResultAssertionException">The response is not a problem, or has another status, or lacks the code.</exception>
    public static async Task<JsonElement> ShouldBeProblemAsync(this HttpResponseMessage response, int? status = null, string? code = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
#else
        if (response is null)
            throw new ArgumentNullException(nameof(response));
#endif

        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        string? mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, ProblemMediaType, StringComparison.OrdinalIgnoreCase))
            throw Fail(response, body, "Expected content type " + ProblemMediaType + ", but it was " + (mediaType ?? "missing") + ".");
        if (status is { } expected && (int)response.StatusCode != expected)
            throw Fail(response, body, string.Format(CultureInfo.InvariantCulture, "Expected status {0}, but it was {1}.", expected, (int)response.StatusCode));

        JsonElement problem = Parse(response, body);
        if (code is not null && !Codes(problem).Contains(code, StringComparer.Ordinal))
            throw Fail(response, body, "Expected the problem to carry error code " + code + ".");
        return problem;
    }

    /// <summary>
    /// Asserts that the response is a 400 problem details response whose <c>errors</c> field has every given key,
    /// and returns its body. The keys are the field paths or codes of an object-shaped <c>errors</c>
    /// (<c>ValidationErrorsFormat.Dictionary</c> or <c>ByField</c>) or the codes of a list-shaped one
    /// (<c>ValidationErrorsFormat.List</c>).
    /// </summary>
    /// <param name="response">The response under test.</param>
    /// <param name="errorKeys">The keys that must be present in <c>errors</c>, compared ordinally.</param>
    /// <returns>The parsed body, which stays valid after the response is disposed.</returns>
    /// <exception cref="ResultAssertionException">The response is not a 400 problem, or a key is missing.</exception>
    public static async Task<JsonElement> ShouldBeValidationProblemAsync(this HttpResponseMessage response, params string[] errorKeys)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(errorKeys);
#else
        if (errorKeys is null)
            throw new ArgumentNullException(nameof(errorKeys));
#endif

        JsonElement problem = await response.ShouldBeProblemAsync(400).ConfigureAwait(false);
        if (!problem.TryGetProperty(ErrorsKey, out JsonElement errors))
            throw Fail(response, problem.GetRawText(), "Expected the problem to have an errors field.");

        string[] keys = [.. ErrorKeys(errors)];
        string[] missing = [.. errorKeys.Where(key => !keys.Contains(key, StringComparer.Ordinal))];
        if (missing.Length > 0)
            throw Fail(response, problem.GetRawText(), "Expected errors to have the keys " + string.Join(", ", missing) + ", but it has " + (keys.Length == 0 ? "none" : string.Join(", ", keys)) + ".");
        return problem;
    }

    private static JsonElement Parse(HttpResponseMessage response, string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw Fail(response, body, "Expected the problem body to be a JSON object.");
            return document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw Fail(response, body, "Expected the problem body to be JSON.", exception);
        }
    }

    private static IEnumerable<string> Codes(JsonElement problem)
    {
        if (problem.TryGetProperty(ErrorCodesKey, out JsonElement codes) && codes.ValueKind == JsonValueKind.Array)
            foreach (JsonElement code in codes.EnumerateArray())
                if (code.ValueKind == JsonValueKind.String)
                    yield return code.GetString()!;
        if (problem.TryGetProperty(ErrorsKey, out JsonElement errors))
            foreach (string key in ErrorKeys(errors))
                yield return key;
    }

    private static IEnumerable<string> ErrorKeys(JsonElement errors)
    {
        if (errors.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in errors.EnumerateObject())
                yield return property.Name;
        }
        else if (errors.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement error in errors.EnumerateArray())
                if (error.ValueKind == JsonValueKind.Object
                    && error.TryGetProperty("code", out JsonElement code)
                    && code.ValueKind == JsonValueKind.String)
                    yield return code.GetString()!;
        }
    }

    private static ResultAssertionException Fail(HttpResponseMessage response, string body, string expectation, Exception? inner = null)
    {
        string message = string.Format(
            CultureInfo.InvariantCulture,
            "{0}{1}{2} {3} returned {4} {5} with body:{1}{6}",
            expectation,
            Environment.NewLine,
            response.RequestMessage?.Method.Method ?? "(no request)",
            response.RequestMessage?.RequestUri?.ToString() ?? string.Empty,
            (int)response.StatusCode,
            response.StatusCode,
            body.Length == 0 ? "(empty)" : body);
        return inner is null ? new ResultAssertionException(message) : new ResultAssertionException(message, inner);
    }
}
