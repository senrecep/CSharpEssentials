using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Per-property settings read by <see cref="ModelConfigurationExtensions.ConfigureEnumConventions(Microsoft.EntityFrameworkCore.ModelConfigurationBuilder, EnumConventions?, EnumStoredAs?)"/>
/// (design section 11.1). They apply to enum, nullable enum and enum collection properties.
/// </summary>
public static class EnumPropertyBuilderExtensions
{
    internal const string StorageAnnotation = "CSharpEssentials:EnumStorage";
    internal const string LegacyStorageAnnotation = "CSharpEssentials:EnumLegacyStorage";
    internal const string CheckConstraintAnnotation = "CSharpEssentials:EnumCheckConstraint";

    /// <summary>Stores the property as <paramref name="storage"/>; wins over <c>existingStorage</c>, <see cref="StringEnumAttribute.Storage"/> and <see cref="EnumConventions.Storage"/>.</summary>
    public static PropertyBuilder<TProperty> HasEnumStorage<TProperty>(this PropertyBuilder<TProperty> builder, EnumStorage storage)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasAnnotation(StorageAnnotation, storage.ToString());
    }

    /// <summary>
    /// Keeps writing the format the column holds today: no check constraint, no column change, tolerant reads (design section 12.3).
    /// </summary>
    public static PropertyBuilder<TProperty> HasLegacyEnumStorage<TProperty>(this PropertyBuilder<TProperty> builder, EnumStoredAs format)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasAnnotation(LegacyStorageAnnotation, ValidateLegacyFormat(format));
    }

    /// <summary>Adds (default) or skips the check constraint of the property.</summary>
    public static PropertyBuilder<TProperty> HasEnumCheckConstraint<TProperty>(this PropertyBuilder<TProperty> builder, bool enabled = true)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasAnnotation(CheckConstraintAnnotation, enabled);
    }

    /// <inheritdoc cref="HasEnumStorage{TProperty}(PropertyBuilder{TProperty}, EnumStorage)"/>
    public static ComplexTypePropertyBuilder<TProperty> HasEnumStorage<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder, EnumStorage storage)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasAnnotation(StorageAnnotation, storage.ToString());
    }

    /// <inheritdoc cref="HasLegacyEnumStorage{TProperty}(PropertyBuilder{TProperty}, EnumStoredAs)"/>
    public static ComplexTypePropertyBuilder<TProperty> HasLegacyEnumStorage<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder, EnumStoredAs format)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasAnnotation(LegacyStorageAnnotation, ValidateLegacyFormat(format));
    }

    /// <inheritdoc cref="HasEnumCheckConstraint{TProperty}(PropertyBuilder{TProperty}, bool)"/>
    public static ComplexTypePropertyBuilder<TProperty> HasEnumCheckConstraint<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder, bool enabled = true)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasAnnotation(CheckConstraintAnnotation, enabled);
    }

    internal static string ValidateLegacyFormat(EnumStoredAs format, string paramName = "format") =>
        format is EnumStoredAs.Text || !Enum.IsDefined(format)
            ? throw new ArgumentOutOfRangeException(paramName, format, $"{nameof(EnumStoredAs)}.{format} is not a write format.")
            : format.ToString();
}
