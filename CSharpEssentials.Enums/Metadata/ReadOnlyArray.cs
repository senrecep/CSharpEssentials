using System.Collections;

namespace CSharpEssentials.Enums;

/// <summary>
/// A read-only list over an array that callers cannot cast back to the array. Indexing it through
/// <see cref="IReadOnlyList{T}"/> is a plain interface call, while an array behind the interface pays the array dispatch
/// cost on .NET 9 and older.
/// </summary>
internal sealed class ReadOnlyArray<T>(T[] items) : IReadOnlyList<T>
{
    public T this[int index] =>
        (uint)index < (uint)items.Length ? items[index] : throw new ArgumentOutOfRangeException(nameof(index));

    public int Count => items.Length;

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
}
