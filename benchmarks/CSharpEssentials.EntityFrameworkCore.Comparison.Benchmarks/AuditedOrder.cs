using CSharpEssentials.Entity;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The written entity of the SaveChanges rows: audit fields and the domain event list come from <see cref="EntityBase{TId}"/>.</summary>
public sealed class AuditedOrder : EntityBase<int>
{
    public string Name { get; set; } = string.Empty;

    public long Amount { get; set; }
}
