using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using CSharpEssentials.Core;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// Deterministic idempotency keys built from business ids, for callers that are handlers rather than people: the same
/// input always gives the same key, so a redelivered message or a retried job reuses its key.
/// <para>
/// The parts are <c>[operation, ...ids]</c>. Each part is written as <c>{utf8ByteLength}:{value}</c> and the parts are
/// joined with <c>|</c>, which is the canonical string; <c>("a", "bc")</c> and <c>("ab", "c")</c> stay different. When
/// every character of the canonical string is allowed (see <see cref="IsValid"/>) and it is at most
/// <see cref="MaxKeyLength"/> characters long, it is the key. Otherwise the key is <c>h:</c> followed by the unpadded
/// base64url SHA-256 of its UTF-8 bytes. The vectors in <c>tests/vectors/idempotency-key-vectors.json</c> pin this
/// format.
/// </para>
/// </summary>
public static class IdempotencyKeys
{
    /// <summary>Longest key returned by <see cref="For"/>; also the default limit of <see cref="IsValid"/>.</summary>
    public const int MaxKeyLength = 255;

    /// <summary>Operation used by <see cref="FromMessage"/>.</summary>
    public const string MessageOperation = "message";

    private const string HashedKeyPrefix = "h:";

    /// <summary>The RFC 9562 namespace of <see cref="ToUuidV5"/>: <c>9c8fbd17-da5f-4f32-a4bf-651721368d5e</c>.</summary>
    public static Guid UuidNamespace { get; } = new("9c8fbd17-da5f-4f32-a4bf-651721368d5e");

    /// <summary>The key for <paramref name="operation"/> on <paramref name="ids"/>, in order.</summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="operation"/> is <see langword="null"/> or empty, an id is <see langword="null"/>, or a part is not
    /// well-formed UTF-16 (it contains a lone surrogate).
    /// </exception>
    public static string For(string operation, params string[] ids)
    {
        string canonical = Canonical(operation, ids);
        return IsValid(canonical) ? canonical : Hash(canonical);
    }

    /// <summary>
    /// The key of message <paramref name="messageId"/> handled by <paramref name="consumer"/>:
    /// <c>For("message", consumer, messageId)</c>. Two consumers of the same message get different keys.
    /// </summary>
    public static string FromMessage(string messageId, string consumer) => For(MessageOperation, consumer, messageId);

    /// <summary>
    /// The RFC 9562 UUIDv5 of the canonical string of <paramref name="operation"/> and <paramref name="ids"/> in
    /// <see cref="UuidNamespace"/>, for providers that require a UUID key. The canonical string is hashed whether or not
    /// <see cref="For"/> would fall back to a hashed key.
    /// </summary>
    /// <exception cref="ArgumentException">The same cases as <see cref="For"/>.</exception>
    public static Guid ToUuidV5(string operation, params string[] ids) =>
        Guider.NewGuidV5(UuidNamespace, Canonical(operation, ids));

    /// <summary>
    /// <see langword="true"/> when <paramref name="key"/> is 1 to <paramref name="maxLength"/> characters long and every
    /// character is printable ASCII (U+0021 to U+007E) other than <c>"</c> and <c>\</c>.
    /// </summary>
    public static bool IsValid([NotNullWhen(true)] string? key, int maxLength = MaxKeyLength)
    {
        if (key is null || key.Length == 0 || key.Length > maxLength)
            return false;

        foreach (char character in key)
        {
            if (character is < '!' or > '~' or '"' or '\\')
                return false;
        }

        return true;
    }

    private static string Canonical(string operation, string[] ids)
    {
        Guard.NotNullOrEmpty(operation);
        Guard.NotNull(ids);

        StringBuilder builder = new(operation.Length + 8);
        LengthPrefixed.Append(builder, operation);
        foreach (string? id in ids)
        {
            if (id is null)
                throw new ArgumentException("An idempotency key id cannot be null.", nameof(ids));
            LengthPrefixed.Append(builder.Append('|'), id);
        }

        return builder.ToString();
    }

    private static string Hash(string canonical)
    {
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha256.AppendData(LengthPrefixed.StrictUtf8.GetBytes(canonical));
        string base64 = Convert.ToBase64String(sha256.GetHashAndReset());
        return HashedKeyPrefix + base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

}
