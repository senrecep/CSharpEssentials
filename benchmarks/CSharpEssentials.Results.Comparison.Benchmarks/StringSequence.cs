using System.Collections;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

public sealed class StringSequence : IEnumerable<string>
{
    private readonly string[] _items;

    public StringSequence(string[] items) => _items = items;

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string item in _items)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
