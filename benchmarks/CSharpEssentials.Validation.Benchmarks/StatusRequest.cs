using System.ComponentModel.DataAnnotations;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>One <see cref="ShipmentStatus"/> property, so DataAnnotations checks one property like the other rows.</summary>
public sealed class StatusRequest
{
    [EnumDataType(typeof(ShipmentStatus))]
    public ShipmentStatus Status { get; set; }
}
