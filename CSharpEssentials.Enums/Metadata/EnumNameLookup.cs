namespace CSharpEssentials.Enums;

/// <summary>
/// A read-only string table with span lookups (allocation free on netstandard2.1 and later).
/// </summary>
/// <remarks>
/// A hand-rolled table instead of <c>FrozenDictionary</c>: its alternate lookup uses generic virtual methods, which make a
/// Native AOT app carry the runtime type loader.
/// </remarks>
internal sealed class EnumNameLookup<TValue> where TValue : class
{
#if NETSTANDARD2_0
    private readonly Dictionary<string, TValue> _items;
#else
    private readonly bool _ignoreCase;
    private readonly bool _fullHash;
    private readonly int _shift;
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
#if NETSTANDARD2_0
        _items = map;
#else
        _ignoreCase = ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase);
        if (!_ignoreCase && !ReferenceEquals(comparer, StringComparer.Ordinal))
            throw new ArgumentException("Only StringComparer.Ordinal and StringComparer.OrdinalIgnoreCase are supported.", nameof(comparer));

        int count = map.Count;
        int bits = 1;
        while (1 << bits < count * 2)
        {
            bits++;
        }

        HashSet<int> sampled = [];
        foreach (string key in map.Keys)
        {
            if (!sampled.Add(Hash(key.AsSpan())))
            {
                _fullHash = true;
                break;
            }
        }

        _shift = 32 - bits;
        _buckets = new int[1 << bits];
        _hashes = new int[count];
        _next = new int[count];
        _keys = new string[count];
        _values = new TValue[count];
        int index = 0;
        foreach (KeyValuePair<string, TValue> entry in map)
        {
            int hash = Hash(entry.Key.AsSpan());
            ref int bucket = ref _buckets[Bucket(hash)];
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
#else
    public TValue? Find(ReadOnlySpan<char> key)
    {
        int hash = Hash(key);
        for (int i = _buckets[Bucket(hash)] - 1; i >= 0; i = _next[i])
        {
            if (_hashes[i] == hash && (_ignoreCase
                    ? key.Equals(_keys[i].AsSpan(), StringComparison.OrdinalIgnoreCase)
                    : key.SequenceEqual(_keys[i].AsSpan())))
                return _values[i];
        }

        return null;
    }

    private int Bucket(int hash) => (int)(unchecked((uint)hash * 2654435769u) >> _shift);

    /// <remarks>
    /// Hashes the length and five sampled chars (first, last and three in between), or every char when two keys of the table share
    /// a sampled hash, so names that differ only between the samples (<c>Error_001_Code</c>, <c>Error_002_Code</c>) never chain.
    /// Ignoring case, ASCII chars are folded with <c>| 0x20</c> and every non-ASCII char hashes alike: OrdinalIgnoreCase never
    /// equates a non-ASCII char with an ASCII one and never changes the length, so keys that compare equal hash equal.
    /// </remarks>
    private int Hash(ReadOnlySpan<char> key)
    {
        int length = key.Length;
        int hash = length;
        if (_fullHash)
        {
            foreach (char c in key)
            {
                hash = unchecked(hash * 31 + Sample(c));
            }

            return hash;
        }

        if (length == 0)
            return 0;

        hash = unchecked(hash * 31 + Sample(key[0]));
        hash = unchecked(hash * 31 + Sample(key[length >> 2]));
        hash = unchecked(hash * 31 + Sample(key[length >> 1]));
        hash = unchecked(hash * 31 + Sample(key[length - 1 - (length >> 2)]));
        return unchecked(hash * 31 + Sample(key[length - 1]));
    }

    private int Sample(char c) => _ignoreCase ? Fold(c) : c;

    private static int Fold(int c) => c >= 0x80 ? 0x80 : c | 0x20;
#endif
}
