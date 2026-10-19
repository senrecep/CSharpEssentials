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
/// different triples never give the same storage key, whatever characters they contain. The scopes of
/// <see cref="IdempotencyScopes"/> start with their kind (<c>user:</c>, <c>tenant:</c>, <c>client:</c>), so none of them
/// is the scope of a global key.
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
    /// <param name="scope">The owner of the key, or <see langword="null"/> for a global key. Must not be empty.</param>
    /// <param name="key">The client's idempotency key. Must not be empty.</param>
    public IdempotencyRecordKey(string @namespace, string? scope, string key)
    {
        Guard.NotNullOrEmpty(@namespace);
        Guard.NotNullOrEmpty(key);
        if (scope is { Length: 0 })
            throw new ArgumentException("A scope cannot be empty; use null for a global key.", nameof(scope));

        Namespace = @namespace;
        Scope = scope;
        Key = key;
    }

    /// <param name="namespace">The caller that owns the key space, for example <c>http</c>. Must not be empty.</param>
    /// <param name="scope">The resolved scope; <see cref="IdempotencyScope.Global"/> gives a global key.</param>
    /// <param name="key">The client's idempotency key. Must not be empty.</param>
    public IdempotencyRecordKey(string @namespace, IdempotencyScope scope, string key)
        : this(@namespace, scope.Value, key)
    {
    }

    /// <summary>The caller that owns the key space, for example <c>http</c>.</summary>
    public string Namespace { get; }

    /// <summary>The owner of the key; <see langword="null"/> for a global key shared by every caller.</summary>
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
    /// <exception cref="ArgumentException">A part is not well-formed UTF-16 (it contains a lone surrogate).</exception>
    public string ToStorageKey(string prefix = DefaultPrefix)
    {
        Guard.NotNull(prefix);

        StringBuilder builder = new(prefix.Length + Namespace.Length + (Scope?.Length ?? 0) + Key.Length + 16);
        builder.Append(prefix);
        LengthPrefixed.Append(builder, Namespace).Append('|');
        LengthPrefixed.Append(builder, Scope ?? GlobalScope).Append('|');
        return LengthPrefixed.Append(builder, Key).ToString();
    }
}
