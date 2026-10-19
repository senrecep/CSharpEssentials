using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// Deterministic idempotency keys built from business ids, for callers that are handlers rather than people: the same
/// input always gives the same key, so a redelivered message or a retried job reuses its key.
/// <para>
/// The parts are <c>[operation, ...ids]</c>. Each part is written as <c>{utf8ByteLength}:{value}</c> and the parts are
/// joined with <c>|</c>, which is the canonical string; <c>("a", "bc")</c> and <c>("ab", "c")</c> stay different. When
/// every character of the canonical string is allowed (see <see cref="IsValid"/>) and it is at most
/// <see cref="MaxKeyLength"/> characters long, it is the key. Otherwise the key is <c>h:</c> followed by the unpadded
/// base64url SHA-256 of its UTF-8 bytes. The format is shared with the other CSharpEssentials ports through
/// <c>shared/idempotency-key-vectors.json</c>.
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
        CreateUuidV5(UuidNamespace, Canonical(operation, ids));

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

    /// <summary>RFC 9562 UUIDv5 (SHA-1, name-based) of the UTF-8 bytes of <paramref name="name"/> in <paramref name="namespaceId"/>.</summary>
    [SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms",
        Justification = "RFC 9562 defines UUIDv5 as SHA-1; the hash names a key and protects nothing.")]
    internal static Guid CreateUuidV5(Guid namespaceId, string name)
    {
        byte[] namespaceBytes = namespaceId.ToByteArray();
        SwapGuidByteOrder(namespaceBytes);

        using var sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        sha1.AppendData(namespaceBytes);
        sha1.AppendData(LengthPrefixed.StrictUtf8.GetBytes(name));
        byte[] uuid = sha1.GetHashAndReset();

        Array.Resize(ref uuid, 16);
        uuid[6] = (byte)((uuid[6] & 0x0F) | 0x50);
        uuid[8] = (byte)((uuid[8] & 0x3F) | 0x80);
        SwapGuidByteOrder(uuid);
        return new Guid(uuid);
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

    /// <summary>
    /// <see cref="Guid.ToByteArray()"/> and <c>new Guid(byte[])</c> store the first three fields little-endian; RFC 9562
    /// orders all bytes big-endian. Swapping those fields converts in either direction, on every target framework.
    /// </summary>
    private static void SwapGuidByteOrder(byte[] bytes)
    {
        (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
        (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
        (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
        (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
    }
}
