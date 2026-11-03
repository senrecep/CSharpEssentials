namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The page the hand-written and MR keyset rows return: the items, the has-next/has-previous flags and the next cursor.</summary>
public sealed record FeedPage(IReadOnlyList<FeedItem> Items, bool HasNext, bool HasPrevious, string? NextCursor)
{
    public static FeedPage FromLimitPlusOne(List<FeedItem> items, int limit, bool hasPrevious)
    {
        bool hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(limit);

        return new FeedPage(items, hasMore, hasPrevious, hasMore ? HandWrittenCursor.Encode(items[^1]) : null);
    }
}
