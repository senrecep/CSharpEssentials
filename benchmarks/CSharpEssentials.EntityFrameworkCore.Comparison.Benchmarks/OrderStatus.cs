using CSharpEssentials.Enums;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// The stored enum of the converter rows. <see cref="Level2Check"/> is <c>level_2_check</c> in the 3.x snake case format,
/// which is neither its wire name (<c>level2_check</c>) nor its member name, so the legacy input differs from the wire input.
/// </summary>
[StringEnum]
public enum OrderStatus
{
    Pending,
    InTransit,
    Delivered,
    Cancelled,
    Level2Check,
}
