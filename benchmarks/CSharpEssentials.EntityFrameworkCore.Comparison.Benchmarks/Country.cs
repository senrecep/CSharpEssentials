namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The table <c>MigrateDataAsync</c> syncs, keyed by <see cref="Code"/>.</summary>
public sealed class Country
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
