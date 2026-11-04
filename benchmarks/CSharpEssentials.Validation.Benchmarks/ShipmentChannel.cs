using CSharpEssentials.Enums;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>Enum with CSE-generated metadata; the other libraries see it as a plain enum.</summary>
[StringEnum]
public enum ShipmentChannel
{
    /// <summary>Web.</summary>
    Web,

    /// <summary>Store.</summary>
    Store,

    /// <summary>Partner.</summary>
    Partner,
}
