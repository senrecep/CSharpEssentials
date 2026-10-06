namespace CSharpEssentials.Enums;

/// <summary>
/// A rejected enum value. Every layer turns it into its own error type without changing the content.
/// </summary>
/// <param name="EnumType">The enum type.</param>
/// <param name="Value">The rejected text or number; <see langword="null"/> when there was no value.</param>
/// <param name="AllowedValues">The wire names a caller may send.</param>
/// <param name="Path">The location of the value (JSON path, parameter name or column), when known.</param>
public sealed record EnumValueError(Type EnumType, string? Value, IReadOnlyList<string> AllowedValues, string? Path)
{
    /// <summary>
    /// <c>'bogus' is not a valid OrderStatus. Allowed values: pending, pending_approval.</c>
    /// </summary>
    public string Message =>
        AllowedValues.Count == 0
            ? $"'{Value}' is not a valid {EnumType.Name}."
            : $"'{Value}' is not a valid {EnumType.Name}. Allowed values: {string.Join(", ", AllowedValues)}.";
}
