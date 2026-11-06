namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>Eight contiguous members with PascalCase wire names, so CSE's wire name equals the member name.</summary>
[StringEnum(Naming = EnumNaming.PascalCase)]
public enum Shade
{
    /// <summary>Red.</summary>
    Red = 0,

    /// <summary>Orange.</summary>
    Orange = 1,

    /// <summary>Yellow.</summary>
    Yellow = 2,

    /// <summary>Green.</summary>
    Green = 3,

    /// <summary>Cyan.</summary>
    Cyan = 4,

    /// <summary>Blue.</summary>
    Blue = 5,

    /// <summary>Indigo.</summary>
    Indigo = 6,

    /// <summary>Violet.</summary>
    Violet = 7,
}
