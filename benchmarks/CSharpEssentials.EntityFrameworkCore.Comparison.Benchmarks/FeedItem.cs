namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// The paged row. It does not derive from <c>EntityBase</c>: that type already has a <c>DateTimeOffset CreatedAt</c>,
/// which SQLite cannot order by, so the keyset key is <see cref="PublishedAt"/>.
/// </summary>
public sealed class FeedItem
{
    public int Id { get; set; }

    public DateTime PublishedAt { get; set; }

    public string Title { get; set; } = string.Empty;
}
