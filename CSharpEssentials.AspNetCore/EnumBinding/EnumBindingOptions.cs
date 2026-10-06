using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The 4.x options of <see cref="EnumBindingExtensions.AddEnumBinding"/>, forwarded to the enum conventions.
/// </summary>
[Obsolete("Use services.AddEnumConventions(c => c with { ... }) and EnumConventionsBuilder.ConfigureErrors.")]
public sealed class EnumBindingOptions
{
    /// <summary>
    /// Selects the enum types whose values are bound (<see cref="EnumConventions.CanHandle"/>). Defaults to enums with
    /// generated metadata (<see cref="EnumMetadata.IsRegistered"/>, enums marked <c>[StringEnum]</c>).
    /// </summary>
    public Predicate<Type> CanBind { get; set; } = EnumMetadata.IsRegistered;

    /// <summary>
    /// Only <see langword="null"/> or <see cref="JsonNamingPolicy.SnakeCaseLower"/> is supported; naming is set on the enum
    /// with <c>[StringEnum(Naming = ...)]</c>.
    /// </summary>
    public JsonNamingPolicy? NamingPolicy { get; set; }

    /// <summary>
    /// Whether the numeric value of a defined member is accepted (<see cref="EnumConventions.AcceptNumbers"/>).
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool AllowIntegerValues { get; set; } = true;

    /// <summary>
    /// Creates the error of a rejected value from the key, the enum type and the allowed values.
    /// <see langword="null"/> (default) uses the error of the enum conventions.
    /// </summary>
    public Func<string, Type, IReadOnlyList<string>, Error>? ErrorFactory { get; set; }
}
