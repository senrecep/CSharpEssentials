namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>The entity of <see cref="EnumStorageContext"/>: one column on the current wire name format, one kept on the 3.x format.</summary>
public sealed class Shipment
{
    public int Id { get; set; }

    public OrderStatus Status { get; set; }

    public OrderStatus LegacyStatus { get; set; }
}
