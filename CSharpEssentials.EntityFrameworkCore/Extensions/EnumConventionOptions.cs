using CSharpEssentials.Enums;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Options for <see cref="ModelConfigurationExtensions.ConfigureEnumConventions(Microsoft.EntityFrameworkCore.ModelConfigurationBuilder, Action{EnumConventionOptions}, System.Reflection.Assembly[])"/>.
/// </summary>
[Obsolete("Use EnumConventions with ConfigureEnumConventions(EnumConventions, EnumStoredAs?) instead.")]
public sealed class EnumConventionOptions
{
    /// <summary>
    /// Which enum types are stored as strings. Default: enums marked with <see cref="StringEnumAttribute"/>.
    /// </summary>
    public Predicate<Type> CanConvert { get; set; } = CSharpEssentials.Json.StringEnumNaming.IsStringEnum;

    /// <summary>
    /// When <see langword="true"/>, values are written in the 3.x format (Core <c>ToSnakeCase</c>) instead of the
    /// JSON name. Default: <see langword="false"/>.
    /// </summary>
    public bool UseLegacySnakeCase { get; set; }
}
