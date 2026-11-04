using CSharpEssentials.Enums;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>Six order states and the <see cref="Unknown"/> fallback; every library writes the same snake_case names.</summary>
[StringEnum]
public enum OrderStatus
{
    /// <summary>Pending.</summary>
    Pending,

    /// <summary>In progress.</summary>
    InProgress,

    /// <summary>On hold.</summary>
    OnHold,

    /// <summary>Shipped.</summary>
    Shipped,

    /// <summary>Delivered.</summary>
    Delivered,

    /// <summary>Cancelled.</summary>
    Cancelled,

    /// <summary>The value read in place of an unknown string.</summary>
    [EnumFallback]
    Unknown,
}
