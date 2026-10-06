using CSharpEssentials.Tests.Fixtures.OpenApiSample;

namespace CSharpEssentials.Tests.Enums.CrossLayer;

/// <summary>The EF Core entity of the cross-layer golden table: one property per <see cref="CrossLayerRow.Shape"/>.</summary>
public sealed class CrossLayerRecord
{
    public int Id { get; set; }
    public GoldenPlain Plain { get; set; }
    public GoldenOrderStatus Status { get; set; }
    public GoldenNaming Naming { get; set; }
    public GoldenPermissions Permissions { get; set; }
    public GoldenPriority Priority { get; set; }
    public GoldenOrderStatus? OptionalStatus { get; set; }
}
