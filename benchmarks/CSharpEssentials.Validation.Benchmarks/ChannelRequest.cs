using System.ComponentModel.DataAnnotations;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>One <see cref="ShipmentChannel"/> property, so DataAnnotations checks one property like the other rows.</summary>
public sealed class ChannelRequest
{
    [EnumDataType(typeof(ShipmentChannel))]
    public ShipmentChannel Channel { get; set; }
}
