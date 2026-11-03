namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The page the plain EF Core offset row returns: the items and the total count.</summary>
public sealed record OffsetPage(IReadOnlyList<FeedItem> Items, int TotalCount);
