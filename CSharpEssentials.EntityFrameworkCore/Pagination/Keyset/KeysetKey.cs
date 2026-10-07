using System.Linq.Expressions;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

internal abstract class KeysetKey<T>(LambdaExpression selector, string path, KeysetDirection direction)
{
    public LambdaExpression Selector { get; } = selector;

    public string Path { get; } = path;

    public KeysetDirection Direction { get; } = direction;

    public Type KeyType => Selector.ReturnType;

    public abstract object GetValue(T item);

    public abstract Expression CreateValue(object value);

    public abstract IOrderedQueryable<T> ApplyOrder(IQueryable<T> source, bool first, bool descending);
}
