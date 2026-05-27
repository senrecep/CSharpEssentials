namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    public static Maybe<TValue[]> Sequence<TValue>(this IEnumerable<Maybe<TValue>> source)
    {
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
        source.Select(selector).Sequence();

    public static (TValue[] Values, int NoneCount) Partition<TValue>(this IEnumerable<Maybe<TValue>> source)
    {
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
