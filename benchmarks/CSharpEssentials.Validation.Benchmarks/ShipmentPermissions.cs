namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>Plain flags enum.</summary>
[Flags]
public enum ShipmentPermissions
{
    /// <summary>None.</summary>
    None = 0,

    /// <summary>Read.</summary>
    Read = 1,

    /// <summary>Write.</summary>
    Write = 2,

    /// <summary>Delete.</summary>
    Delete = 4,
}
