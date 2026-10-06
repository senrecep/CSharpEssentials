namespace CSharpEssentials.Enums;

/// <summary>
/// Thrown when an enum value cannot be read or written: an undefined value created in code, or an invalid value in a data read.
/// </summary>
/// <param name="error">The rejected value.</param>
public sealed class EnumValueException(EnumValueError error) : Exception(error?.Message)
{
    /// <summary>The rejected value.</summary>
    public EnumValueError Error { get; } = error ?? throw new ArgumentNullException(nameof(error));
}
