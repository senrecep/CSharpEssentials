namespace CSharpEssentials.These;

public static class TheseCollectionExtensions
{
    public static (IReadOnlyList<TError> Lefts, IReadOnlyList<TValue> Rights, IReadOnlyList<(TError, TValue)> Boths)
        Partition<TError, TValue>(this IEnumerable<These<TError, TValue>> source)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
#else
        if (source is null)
            throw new ArgumentNullException(nameof(source));
#endif

        List<TError> lefts = [];
        List<TValue> rights = [];
        List<(TError, TValue)> boths = [];

        foreach (These<TError, TValue> item in source)
        {
            if (item.IsLeft)
                lefts.Add(item.LeftOrDefault!);
            else if (item.IsRight)
                rights.Add(item.RightOrDefault!);
            else
                boths.Add((item.LeftOrDefault!, item.RightOrDefault!));
        }

        return (lefts, rights, boths);
    }
}
