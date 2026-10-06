#if NET9_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace CSharpEssentials.Enums;

/// <summary>
/// A read-only string table with span lookups (allocation free on net9.0+).
/// </summary>
internal sealed class EnumNameLookup<TValue> where TValue : class
{
#if NET9_0_OR_GREATER
    private readonly FrozenDictionary<string, TValue>.AlternateLookup<ReadOnlySpan<char>> _spanLookup;
#else
    private readonly Dictionary<string, TValue> _items;
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
#else
        _items = map;
#endif
    }

#if NETSTANDARD2_0
    public TValue? Find(string key) => _items.TryGetValue(key, out TValue? value) ? value : null;
#elif NET9_0_OR_GREATER
    public TValue? Find(ReadOnlySpan<char> key) => _spanLookup.TryGetValue(key, out TValue? value) ? value : null;
#else
    public TValue? Find(ReadOnlySpan<char> key) => _items.TryGetValue(key.ToString(), out TValue? value) ? value : null;
#endif
}
