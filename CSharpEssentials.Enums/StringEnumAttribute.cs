namespace CSharpEssentials.Enums;

/// <summary>
/// Generates metadata, wire names and extension methods for the enum.
/// </summary>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public sealed class StringEnumAttribute : Attribute
{
    /// <summary>The naming of the wire names; <see cref="EnumNaming.Default"/> uses the project setting.</summary>
    public EnumNaming Naming { get; set; }

    /// <summary>The storage of the enum; <see cref="EnumStorage.Default"/> lets <see cref="EnumConventions"/> decide.</summary>
    public EnumStorage Storage { get; set; }
}
