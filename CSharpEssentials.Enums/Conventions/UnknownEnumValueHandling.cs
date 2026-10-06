namespace CSharpEssentials.Enums;

/// <summary>
/// What a data read does with a value that is not defined.
/// </summary>
public enum UnknownEnumValueHandling
{
    /// <summary>Fail with an <see cref="EnumValueException"/>.</summary>
    Reject = 0,

    /// <summary>
    /// Map the value to the <see cref="EnumFallbackAttribute"/> member when the enum declares one, otherwise behave like <see cref="Reject"/>.
    /// </summary>
    UseFallback,
}
