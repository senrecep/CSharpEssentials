namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The row with a unique index on <see cref="Name"/> that the error translation rows violate.</summary>
public sealed class UniqueItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
