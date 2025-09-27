using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Core;

public static class CollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ICollection<T> IfAdd<T>(this ICollection<T> collection, bool condition, T item)
    {
        if (condition)
            collection.Add(item);
        return collection;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ICollection<T> IfAddRange<T>(this ICollection<T> collection, bool condition, params IEnumerable<T> items)
    {
        if (condition)
            foreach (T? item in items)
                collection.Add(item);
        return collection;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T? item in source)
        {
            action(item);
            yield return item;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IQueryable<TSource> WhereIf<TSource>(
        this IQueryable<TSource> source,
        bool condition,
        Expression<Func<TSource, bool>> predicate) =>
            condition ? source.Where(predicate) : source;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TSource> WhereIf<TSource>(
        this IEnumerable<TSource> source,
        bool condition,
        Func<TSource, bool> predicate) =>
            condition ? source.Where(predicate) : source;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TSource> WhereIf<TSource>(
        this IEnumerable<TSource> source,
        bool condition,
        Func<TSource, int, bool> predicate) =>
            condition ? source.Where(predicate) : source;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TSource> WithoutNulls<TSource>(
        this IEnumerable<TSource?> source) =>
           source.Where(item => item is not null)!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TSource> WithoutNulls<TSource, TProperty>(
        this IEnumerable<TSource?> source,
        Expression<Func<TSource, TProperty?>> propertySelector)
    {
        Func<TSource, TProperty?> propertyFunc = propertySelector.Compile();
        return source
            .WithoutNulls()
            .Where(item => propertyFunc(item) is not null);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasSameElements<T>(this IEnumerable<T> src, IEnumerable<T> dest) =>
        src.ToHashSet().SetEquals(dest.ToHashSet());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllTrue(this IEnumerable<bool> list)
    {
        foreach (bool item in list)
            if (item.IsFalse())
                return false;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllFalse(this IEnumerable<bool> list)
    {
        foreach (bool item in list)
            if (item.IsTrue())
                return false;
        return true;
    }

}
