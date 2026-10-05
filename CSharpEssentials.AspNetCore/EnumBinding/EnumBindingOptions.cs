using System.Text.Json;
using CSharpEssentials.Errors;
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

    /// <summary>
    /// Creates the error of an invalid value from the query/route key, the enum type and the accepted names.
    /// <see langword="null"/> (default) creates <c>Error.Validation(code: key, description: "'key' must be one of: ...")</c>.
    /// Use it to normalize the code (for example <c>"validation.status"</c>) or localize the message.
    /// </summary>
    public Func<string, Type, IReadOnlyList<string>, Error>? ErrorFactory { get; set; }
}
