namespace CSharpEssentials.Enums;

/// <summary>
/// The output format of an enum value.
/// </summary>
public enum EnumWireFormat
{
    /// <summary>The wire name (default).</summary>
    String = 0,

    /// <summary>The underlying number, for legacy consumers.</summary>
    Number,
}
