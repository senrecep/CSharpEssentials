using System.Text.Json.Serialization;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>The same JSON as <see cref="ArticleDto"/>, with a version the default ETag generator reads.</summary>
public sealed record VersionedArticleDto(int Id, string Title, string Body) : IVersioned
{
    /// <inheritdoc />
    [JsonIgnore]
    public string Version => "17";
}
