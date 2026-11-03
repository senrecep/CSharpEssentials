namespace CSharpEssentials.Http.Comparison.Benchmarks;

public sealed class SearchQuery
{
    public string Q { get; init; } = string.Empty;

    public int Page { get; init; }

    public string Include { get; init; } = string.Empty;
}
