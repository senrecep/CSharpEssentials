using System.Text.Json;
using CSharpEssentials.Json;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Options for <see cref="EnumBindingExtensions.UseEnumBinding"/>.
/// </summary>
public sealed class EnumBindingOptions
{
    /// <summary>
    /// Selects the enum types whose query and route values are normalized.
    /// Defaults to <see cref="StringEnumNaming.IsStringEnum"/> (enums marked with <c>[StringEnum]</c>).
    /// </summary>
    public Predicate<Type> CanBind { get; set; } = StringEnumNaming.IsStringEnum;

    /// <summary>
    /// The naming policy of the accepted string form. <see langword="null"/> uses
    /// <see cref="StringEnumNaming.DefaultPolicy"/>, the same default as the JSON converter.
    /// </summary>
    public JsonNamingPolicy? NamingPolicy { get; set; }

    /// <summary>
    /// Whether the numeric value of a defined member is accepted. Undefined numbers are always rejected.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool AllowIntegerValues { get; set; } = true;
}
