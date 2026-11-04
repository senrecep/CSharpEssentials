using System.Collections;

namespace CSharpEssentials.Enums;

/// <summary>
/// A read-only list over an array that callers cannot cast back to the array. Indexing it through
/// <see cref="IReadOnlyList{T}"/> is a plain interface call, while an array behind the interface pays the array dispatch
/// cost on .NET 9 and older. It is also a read-only <see cref="ICollection{T}"/>, so copies such as <c>ToArray</c> and
/// <c>new List&lt;T&gt;(list)</c> take the <see cref="ICollection{T}.CopyTo"/> path.
/// </summary>
internal sealed class ReadOnlyArray<T>(T[] items) : IReadOnlyList<T>, ICollection<T>
{
    public T this[int index] =>
        (uint)index < (uint)items.Length ? items[index] : throw new ArgumentOutOfRangeException(nameof(index));

    public int Count => items.Length;

    bool ICollection<T>.IsReadOnly => true;

    public bool Contains(T item) => Array.IndexOf(items, item) >= 0;

    public void CopyTo(T[] array, int arrayIndex) => Array.Copy(items, 0, array, arrayIndex, items.Length);

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

    void ICollection<T>.Add(T item) => throw new NotSupportedException("The collection is read-only.");

    void ICollection<T>.Clear() => throw new NotSupportedException("The collection is read-only.");

    bool ICollection<T>.Remove(T item) => throw new NotSupportedException("The collection is read-only.");
}
