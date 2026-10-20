using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// Identifies an idempotency record: the <see cref="Namespace"/> of the caller (for example <c>http</c> or a message
/// consumer), the <see cref="Scope"/> that owns the key (a user, tenant or client) and the client's <see cref="Key"/>.
/// The same key in another namespace or scope is another record.
/// <para>
/// <see cref="ToStorageKey"/> turns the triple into one string for stores that need a flat key:
/// <c>{prefix}{n}:{namespace}|{scope part}|{n}:{key}</c>, where every <c>{n}</c> is the UTF-8 byte length of the value
/// after it. A global key writes <c>global</c> as its scope. The lengths make every part end where it should: two
/// different triples never give the same storage key, whatever characters they contain. The scope <c>global</c> is
/// reserved for global keys: every scope built by the <see cref="IdempotencyScope"/> factories starts with its kind
/// (<c>user:</c>, <c>tenant:</c>, <c>client:</c>, <c>{kind}:</c>) or a length prefix, so none of them is <c>global</c>.
/// </para>
/// </summary>
public sealed record IdempotencyRecordKey
{
    /// <summary>
    /// Default prefix of <see cref="ToStorageKey"/>. The version lets entries of another record format live side by side
    /// with these during a rolling deploy instead of being misread.
    /// </summary>
    public const string DefaultPrefix = "idem:v3:";

    /// <summary>Scope written in the storage key of a global key.</summary>
    internal const string GlobalScope = "global";

    /// <param name="namespace">The caller that owns the key space, for example <c>http</c>. Must not be empty.</param>
    /// <param name="scope">
    /// The owner of the key, built by the <see cref="IdempotencyScope"/> factories; <see langword="null"/> or
    /// <see cref="IdempotencyScope.Global"/> gives a global key.
    /// </param>
    /// <param name="key">The client's idempotency key. Must not be empty.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="namespace"/> or <paramref name="key"/> is <see langword="null"/> or empty, or a part contains an
    /// unpaired surrogate, which has no UTF-8 encoding.
    /// </exception>
    public IdempotencyRecordKey(string @namespace, IdempotencyScope? scope, string key)
        : this(@namespace, scope?.Value, key)
    {
    }

    private IdempotencyRecordKey(string @namespace, string? scope, string key)
    {
        Guard.NotNullOrEmpty(@namespace);
        Guard.NotNullOrEmpty(key);
        Guard.WellFormedUtf16(@namespace);
        Guard.WellFormedUtf16(key);
        if (scope is { Length: 0 })
            throw new ArgumentException("A scope cannot be empty; use null for a global key.", nameof(scope));
        if (scope == GlobalScope)
            throw new ArgumentException("The scope 'global' is reserved for global keys; use null.", nameof(scope));
        if (scope is not null)
            Guard.WellFormedUtf16(scope);

        Namespace = @namespace;
        Scope = scope;
        Key = key;
    }

    /// <summary>
    /// A key from the storage form of its scope (<see cref="IdempotencyScope.Value"/>), for code that reads keys back and
    /// for tests of the storage key encoding. Public code passes an <see cref="IdempotencyScope"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// A part is empty or contains an unpaired surrogate, or <paramref name="scope"/> is <c>global</c>.
    /// </exception>
    internal static IdempotencyRecordKey FromStorageScope(string @namespace, string? scope, string key) => new(@namespace, scope, key);

    /// <summary>The caller that owns the key space, for example <c>http</c>.</summary>
    public string Namespace { get; }

    /// <summary>
    /// The owner of the key in its storage form (<see cref="IdempotencyScope.Value"/>); <see langword="null"/> for a
    /// global key shared by every caller.
    /// </summary>
    public string? Scope { get; }

    /// <summary>The client's idempotency key.</summary>
    public string Key { get; }

    /// <summary><see langword="true"/> when the key has no <see cref="Scope"/>.</summary>
    public bool IsGlobal => Scope is null;

    /// <summary>
    /// The flat storage key <c>{prefix}{n}:{namespace}|{n}:{scope}|{n}:{key}</c>, with <c>global</c> as the scope
    /// of a global key. See the type remarks for why distinct keys never collide.
    /// </summary>
    /// <param name="prefix">Prepended as is; defaults to <see cref="DefaultPrefix"/>.</param>
    public string ToStorageKey(string prefix = DefaultPrefix)
    {
        Guard.NotNull(prefix);

        StringBuilder builder = new(prefix.Length + Namespace.Length + (Scope?.Length ?? 0) + Key.Length + 16);
        builder.Append(prefix);
        LengthPrefixed.Append(builder, Namespace).Append('|');
        LengthPrefixed.Append(builder, Scope ?? GlobalScope).Append('|');
        return LengthPrefixed.Append(builder, Key).ToString();
    }

    /// <summary>
    /// The first 16 hex digits of the SHA-256 hash of the namespace, scope and key, for logs that tell keys apart
    /// without revealing them.
    /// </summary>
    internal string ToLogHash()
    {
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha256.AppendData(LengthPrefixed.StrictUtf8.GetBytes(ToStorageKey(string.Empty)));
        byte[] hash = sha256.GetHashAndReset();
        StringBuilder builder = new(16);
        for (int index = 0; index < 8; index++)
            builder.Append(hash[index].ToString("x2", CultureInfo.InvariantCulture));
        return builder.ToString();
    }
}
