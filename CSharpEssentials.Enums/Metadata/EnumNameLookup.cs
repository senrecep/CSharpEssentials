#if NET9_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace CSharpEssentials.Enums;

/// <summary>
/// A read-only string table with span lookups (allocation free on netstandard2.1 and net9.0+).
/// </summary>
internal sealed class EnumNameLookup<TValue> where TValue : class
{
#if NET9_0_OR_GREATER
    private readonly FrozenDictionary<string, TValue>.AlternateLookup<ReadOnlySpan<char>> _spanLookup;
#elif NETSTANDARD2_0
    private readonly Dictionary<string, TValue> _items;
#else
    private readonly bool _ignoreCase;
    private readonly int[] _buckets;
    private readonly int[] _hashes;
    private readonly int[] _next;
    private readonly string[] _keys;
    private readonly TValue[] _values;
#endif

    public EnumNameLookup(IEnumerable<KeyValuePair<string, TValue>> items, StringComparer comparer)
    {
        Dictionary<string, TValue> map = [with(comparer)];
        foreach (KeyValuePair<string, TValue> item in items)
        {
            // First entry wins, so the lookup order of the caller decides collisions.
            if (!map.ContainsKey(item.Key))
                map.Add(item.Key, item.Value);
        }
#if NET9_0_OR_GREATER
        _spanLookup = map.ToFrozenDictionary(comparer).GetAlternateLookup<ReadOnlySpan<char>>();
#elif NETSTANDARD2_0
        _items = map;
#else
        _ignoreCase = ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase);
        if (!_ignoreCase && !ReferenceEquals(comparer, StringComparer.Ordinal))
            throw new ArgumentException("Only StringComparer.Ordinal and StringComparer.OrdinalIgnoreCase are supported.", nameof(comparer));

        int count = map.Count;
        _buckets = new int[Math.Max(1, count * 2)];
        _hashes = new int[count];
        _next = new int[count];
        _keys = new string[count];
        _values = new TValue[count];
        int index = 0;
        foreach (KeyValuePair<string, TValue> entry in map)
        {
            int hash = Hash(entry.Key.AsSpan());
            ref int bucket = ref _buckets[(uint)hash % (uint)_buckets.Length];
            _hashes[index] = hash;
            _keys[index] = entry.Key;
            _values[index] = entry.Value;
            _next[index] = bucket - 1;
            bucket = index + 1;
            index++;
        }
#endif
    }

#if NETSTANDARD2_0
    public TValue? Find(string key) => _items.TryGetValue(key, out TValue? value) ? value : null;
#elif NET9_0_OR_GREATER
    public TValue? Find(ReadOnlySpan<char> key) => _spanLookup.TryGetValue(key, out TValue? value) ? value : null;
#else
    public TValue? Find(ReadOnlySpan<char> key)
    {
        int hash = Hash(key);
        StringComparison comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        for (int i = _buckets[(uint)hash % (uint)_buckets.Length] - 1; i >= 0; i = _next[i])
        {
            if (_hashes[i] == hash && key.Equals(_keys[i].AsSpan(), comparison))
                return _values[i];
        }

        return null;
    }

    /// <remarks>
    /// Ignoring case, ASCII letters are folded and every non-ASCII char hashes alike: OrdinalIgnoreCase never equates a
    /// non-ASCII char with an ASCII one and never changes the length, so keys that compare equal hash equal.
    /// </remarks>
    private int Hash(ReadOnlySpan<char> key)
    {
        int hash = key.Length;
        foreach (char c in key)
        {
            int folded = c;
            if (_ignoreCase)
                folded = c >= 0x80 ? 0x80 : char.ToUpperInvariant(c);
            hash = unchecked(hash * 31 + folded);
        }

        return hash;
    }
#endif
}
