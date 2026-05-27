namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    private static IEnumerable<T> EnsureSource<T>(IEnumerable<T> source)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
#else
        if (source is null)
            throw new ArgumentNullException(nameof(source));
#endif

        return source;
    }

    private static Func<TSource, TResult> EnsureSelector<TSource, TResult>(Func<TSource, TResult> selector)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(selector);
#else
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));
#endif

        return selector;
    }

    public static Maybe<TValue[]> Sequence<TValue>(this IEnumerable<Maybe<TValue>> source)
    {
        source = EnsureSource(source);
        List<TValue> values = [];

        foreach (Maybe<TValue> maybe in source)
        {
            if (maybe.HasNoValue)
                return Maybe<TValue[]>.None;

            values.Add(maybe.Value);
        }

        return values.ToArray();
    }

    public static Maybe<TOut[]> Traverse<TSource, TOut>(this IEnumerable<TSource> source, Func<TSource, Maybe<TOut>> selector) =>
        EnsureSource(source).Select(EnsureSelector(selector)).Sequence();

    public static (TValue[] Values, int NoneCount) Partition<TValue>(this IEnumerable<Maybe<TValue>> source)
    {
        source = EnsureSource(source);
        List<TValue> values = [];
        int noneCount = 0;

        foreach (Maybe<TValue> maybe in source)
        {
            if (maybe.HasValue)
                values.Add(maybe.Value);
            else
                noneCount++;
        }

        return (values.ToArray(), noneCount);
    }
}
