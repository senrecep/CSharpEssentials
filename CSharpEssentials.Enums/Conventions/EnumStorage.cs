namespace CSharpEssentials.Enums;

/// <summary>
/// How an enum value is stored by data layers such as EF Core.
/// </summary>
public enum EnumStorage
{
    /// <summary>Let <see cref="EnumConventions"/> decide.</summary>
    Default = 0,

    /// <summary>The wire name.</summary>
    String,

    /// <summary>The underlying number.</summary>
    Integer,
}
