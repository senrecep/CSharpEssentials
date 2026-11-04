namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The 10,000-row feed table of the paging rows. Ten rows share each timestamp, so the id tie-breaker matters.</summary>
internal static class FeedSeed
{
    public const int Rows = 10_000;

    private static readonly DateTime Start = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Insert(BenchDatabase database)
    {
        using PlainContext context = database.Plain();
        context.Set<FeedItem>().AddRange(Enumerable.Range(0, Rows).Select(static i => new FeedItem
        {
            Id = i + 1,
            PublishedAt = Start.AddTicks(TimeSpan.TicksPerMinute * (i / 10)),
            Title = $"item {i}",
        }));
        context.SaveChanges();
    }
}
