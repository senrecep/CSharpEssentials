using CSharpEssentials.Enums;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>Four single bits and <see cref="None"/>.</summary>
[Flags]
[StringEnum]
public enum Permissions
{
    /// <summary>No permission.</summary>
    None = 0,

    /// <summary>Read.</summary>
    Read = 1,

    /// <summary>Write.</summary>
    Write = 2,

    /// <summary>Delete.</summary>
    Delete = 4,

    /// <summary>Share.</summary>
    Share = 8,
}
