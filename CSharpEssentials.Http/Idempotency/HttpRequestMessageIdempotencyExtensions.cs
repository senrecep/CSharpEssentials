namespace CSharpEssentials.Http;

/// <summary>Writes the <c>Idempotency-Key</c> header of a request without any message handler.</summary>
public static class HttpRequestMessageIdempotencyExtensions
{
    /// <summary>
    /// Sets the <c>Idempotency-Key</c> header of <paramref name="request"/>, replacing any previous value, and returns
    /// the key. When <paramref name="key"/> is <see langword="null"/> a new key is generated (a UUIDv7 on .NET 9 and
    /// later, a random UUID on netstandard2.1). Pass a deterministic key, for example from
    /// <c>IdempotencyKeys.For(...)</c>, when the same logical operation can be started again.
    /// Retries that resend the same <see cref="HttpRequestMessage"/> keep the key.
    /// </summary>
    /// <param name="request">The request to mark.</param>
    /// <param name="key">The key to send, or <see langword="null"/> to generate one.</param>
    /// <returns>The key that was written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="key"/> is empty or contains a character other than visible ASCII, or contains <c>"</c> or <c>\</c>.
    /// </exception>
    public static string SetIdempotencyKey(this HttpRequestMessage request, string? key = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request is null)
            throw new ArgumentNullException(nameof(request));
#endif

        string value = key is null ? IdempotencyHttpProtocol.NewKey() : IdempotencyHttpProtocol.Validate(key, nameof(key));
        request.Headers.Remove(IdempotencyHttpProtocol.KeyHeaderName);
        request.Headers.TryAddWithoutValidation(IdempotencyHttpProtocol.KeyHeaderName, value);
        return value;
    }
}
