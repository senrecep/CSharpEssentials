using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Idempotency;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Reads an <c>Idempotency-Key</c> header value the way the idempotency middleware does. Pass the header to a command
/// through <see cref="TryParse(string?, out string?)"/>, so the command sees the same key as the middleware: a client
/// that sends the key as a structured-field string (<c>"abc"</c>) is deduplicated as <c>abc</c> in both layers.
/// </summary>
public static class IdempotencyKeyHeader
{
    /// <summary>
    /// Unquotes a structured-field string (<c>"abc"</c>) and validates the key with <see cref="IdempotencyKeys.IsValid"/>
    /// and <see cref="IdempotencyKeys.MaxKeyLength"/>. See <see cref="TryParse(string?, int, out string?)"/>.
    /// </summary>
    /// <param name="value">One <c>Idempotency-Key</c> header value.</param>
    /// <param name="key">The key, without the quotes, when the value is valid.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> carries a valid key.</returns>
    public static bool TryParse(string? value, [NotNullWhen(true)] out string? key) =>
        TryParse(value, IdempotencyKeys.MaxKeyLength, out key);

    /// <summary>
    /// Unquotes a structured-field string (<c>"abc"</c>) and validates the key with <see cref="IdempotencyKeys.IsValid"/>.
    /// An escape sequence can only produce <c>"</c> or <c>\</c>, which keys may not contain, so a value with a backslash
    /// or an inner quote is rejected whether or not its quoting is well formed. <paramref name="maxKeyLength"/> applies
    /// to the unquoted key; pass <see cref="IdempotencyOptions.MaxKeyLength"/> when it is not the default.
    /// </summary>
    /// <param name="value">One <c>Idempotency-Key</c> header value.</param>
    /// <param name="maxKeyLength">The longest key accepted.</param>
    /// <param name="key">The key, without the quotes, when the value is valid.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> carries a valid key.</returns>
    public static bool TryParse(string? value, int maxKeyLength, [NotNullWhen(true)] out string? key)
    {
        string? candidate = value is { Length: >= 2 } && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;
        if (IdempotencyKeys.IsValid(candidate, maxKeyLength))
        {
            key = candidate;
            return true;
        }

        key = null;
        return false;
    }
}
