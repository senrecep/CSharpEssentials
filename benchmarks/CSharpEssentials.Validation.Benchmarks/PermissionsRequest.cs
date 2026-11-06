using System.ComponentModel.DataAnnotations;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>One <see cref="ShipmentPermissions"/> property, so DataAnnotations checks one property like the other rows.</summary>
public sealed class PermissionsRequest
{
    [EnumDataType(typeof(ShipmentPermissions))]
    public ShipmentPermissions Permissions { get; set; }
}
