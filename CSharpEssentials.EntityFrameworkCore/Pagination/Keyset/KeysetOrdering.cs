using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>
/// The ordered keys of a keyset pagination, for example
/// <c>new KeysetOrdering&lt;Order&gt;().Descending(x =&gt; x.CreatedAt).Descending(x =&gt; x.Id)</c>.
/// The keys together must be unique, so end with a unique tie-breaker such as the primary key. Keys are validated
/// when they are added: they must be non-nullable property or field accesses of a supported type.
/// Instances are immutable and can be cached and shared.
/// </summary>
public sealed class KeysetOrdering<T>
{
    private readonly KeysetKey<T>[] _keys;

    /// <summary>Creates an empty ordering; add keys with <see cref="Ascending{TKey}"/> and <see cref="Descending{TKey}"/>.</summary>
    public KeysetOrdering() : this([])
    {
    }

    private KeysetOrdering(KeysetKey<T>[] keys)
    {
        _keys = keys;
        KeyTypes = [.. keys.Select(key => key.KeyType)];
        Fingerprint = CreateFingerprint(keys);
    }

    /// <summary>Number of keys.</summary>
    public int Count => _keys.Length;

    internal IReadOnlyList<KeysetKey<T>> Keys => _keys;

    internal IReadOnlyList<Type> KeyTypes { get; }

    internal string Fingerprint { get; }

    /// <summary>Returns a new ordering with <paramref name="key"/> appended in ascending order.</summary>
    /// <exception cref="ArgumentException">The key is nullable, of an unsupported type, not a member access, or already added.</exception>
    public KeysetOrdering<T> Ascending<TKey>(Expression<Func<T, TKey>> key) => Add(key, KeysetDirection.Ascending);

    /// <summary>Returns a new ordering with <paramref name="key"/> appended in descending order.</summary>
    /// <exception cref="ArgumentException">The key is nullable, of an unsupported type, not a member access, or already added.</exception>
    public KeysetOrdering<T> Descending<TKey>(Expression<Func<T, TKey>> key) => Add(key, KeysetDirection.Descending);

    private KeysetOrdering<T> Add<TKey>(Expression<Func<T, TKey>> key, KeysetDirection direction)
    {
        var created = TypedKeysetKey<T, TKey>.Create(key, direction);
        if (_keys.Any(existing => existing.Path == created.Path))
            throw new ArgumentException($"Keyset key '{created.Path}' is already part of the ordering.", nameof(key));

        return new KeysetOrdering<T>([.. _keys, created]);
    }

    private static string CreateFingerprint(KeysetKey<T>[] keys)
    {
        string definition = typeof(T).FullName + "|" + string.Join(
            '|',
            keys.Select(key => $"{key.Path}:{key.KeyType.FullName}:{(key.Direction == KeysetDirection.Ascending ? 'a' : 'd')}"));
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(definition));
        return Base64Url.Encode(hash.AsSpan(0, 6));
    }
}
