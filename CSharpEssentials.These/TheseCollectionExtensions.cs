namespace CSharpEssentials.These;

public static class TheseCollectionExtensions
{
    public static (IReadOnlyList<TError> Lefts, IReadOnlyList<TValue> Rights, IReadOnlyList<(TError, TValue)> Boths)
        Partition<TError, TValue>(this IEnumerable<These<TError, TValue>> source)
    {
        List<TError> lefts = [];
        List<TValue> rights = [];
        List<(TError, TValue)> boths = [];

        foreach (These<TError, TValue> item in source)
        {
            item.Match(
                onLeft: e => { lefts.Add(e); return 0; },
                onRight: a => { rights.Add(a); return 0; },
                onBoth: (e, a) => { boths.Add((e, a)); return 0; });
        }

        return (lefts, rights, boths);
    }
}
