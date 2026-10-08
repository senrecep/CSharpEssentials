using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Registers the enum storage convention (design section 11) and the <c>Maybe&lt;T&gt;?</c> column convention of CSharpEssentials.
/// </summary>
public static class ModelConfigurationExtensions
{
    /// <summary>
    /// Stores enums with generated metadata (<see cref="StringEnumAttribute"/>) by <paramref name="conventions"/>: converter,
    /// column facets and check constraint per property. Enums without metadata, and properties with a value converter
    /// configured by the user, are left to EF Core.
    /// </summary>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <param name="conventions">The conventions; <see cref="EnumConventions.Default"/> when <see langword="null"/>.</param>
    /// <param name="existingStorage">
    /// The format the columns hold today. Every handled property without <c>HasEnumStorage</c> or <c>HasLegacyEnumStorage</c>
    /// keeps it, as with <c>HasLegacyEnumStorage</c>, so the first migration after the upgrade is empty. New projects omit it.
    /// </param>
    /// <returns>The same builder.</returns>
    [OverloadResolutionPriority(1)]
    public static ModelConfigurationBuilder ConfigureEnumConventions(
        this ModelConfigurationBuilder configurationBuilder,
        EnumConventions? conventions = null,
        EnumStoredAs? existingStorage = null) =>
        AddConvention(configurationBuilder, conventions, existingStorage, reflectionFallback: null);

    /// <summary>
    /// Same as <see cref="ConfigureEnumConventions(ModelConfigurationBuilder, EnumConventions?, EnumStoredAs?)"/>, but enums
    /// accepted by <see cref="EnumConventions.CanHandle"/> or configured per property without generated metadata are read
    /// through reflection.
    /// </summary>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <param name="conventions">The conventions; <see cref="EnumConventions.Default"/> when <see langword="null"/>.</param>
    /// <param name="existingStorage">The format the columns hold today, see <see cref="ConfigureEnumConventions(ModelConfigurationBuilder, EnumConventions?, EnumStoredAs?)"/>.</param>
    /// <returns>The same builder.</returns>
    [RequiresUnreferencedCode("Enums without generated metadata are read through reflection.")]
    [RequiresDynamicCode("Enums without generated metadata are read through reflection.")]
    public static ModelConfigurationBuilder ConfigureEnumConventionsWithReflection(
        this ModelConfigurationBuilder configurationBuilder,
        EnumConventions? conventions = null,
        EnumStoredAs? existingStorage = null) =>
        AddConvention(configurationBuilder, conventions, existingStorage, static type => EnumMetadata.GetOrCreateWithReflection(type));

    /// <summary>Obsolete: use <see cref="ConfigureEnumConventions(ModelConfigurationBuilder, EnumConventions?, EnumStoredAs?)"/>. The assemblies are ignored.</summary>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <param name="assemblies">Ignored; mapped enum properties are found by the model convention.</param>
    [Obsolete("Use ConfigureEnumConventions(EnumConventions, EnumStoredAs?) instead. Assemblies are no longer scanned.")]
    public static void ConfigureEnumConventions(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies) =>
        configurationBuilder.ConfigureEnumConventions(static _ => { }, assemblies);

    /// <summary>Obsolete: use <see cref="ConfigureEnumConventions(ModelConfigurationBuilder, EnumConventions?, EnumStoredAs?)"/>. The assemblies are ignored.</summary>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <param name="configure">Configures the 4.x options; <c>UseLegacySnakeCase</c> maps to <see cref="EnumStoredAs.LegacySnakeCase"/>.</param>
    /// <param name="assemblies">Ignored; mapped enum properties are found by the model convention.</param>
    [Obsolete("Use ConfigureEnumConventions(EnumConventions, EnumStoredAs?) instead. Assemblies are no longer scanned.")]
    public static void ConfigureEnumConventions(
        this ModelConfigurationBuilder configurationBuilder,
        Action<EnumConventionOptions> configure,
        params Assembly[] assemblies)
    {
        _ = configure ?? throw new ArgumentNullException(nameof(configure));
        _ = assemblies;
        EnumConventionOptions options = new();
        configure(options);
        Predicate<Type> canConvert = options.CanConvert;
        AddConvention(
            configurationBuilder,
            EnumConventions.Default with { CanHandle = type => canConvert(type) },
            options.UseLegacySnakeCase ? EnumStoredAs.LegacySnakeCase : null,
            reflectionFallback: ThrowWithoutMetadata);
    }

    /// <summary>
    /// Maps every public read-write <c>Maybe&lt;T&gt;?</c> property of every entity type and complex type to a nullable
    /// column of <c>T</c>, as <c>HasNullableMaybeConversion</c> does per property. Ignored properties and properties with
    /// a value converter configured by the user are left alone.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Maybe&lt;T&gt;?</c> with a nullable value type <c>T</c> (<c>Maybe&lt;int?&gt;?</c>) is not supported: the
    /// convention skips it and EF Core fails the model build. Ignore such a property or configure your own converter.
    /// </para>
    /// <para>
    /// The CLR properties are read once while the model is built, and each converter type is closed with
    /// <see cref="Type.MakeGenericType"/> once per <c>T</c> and cached. Queries and saves do not use reflection. Under
    /// NativeAOT use a compiled model, so the convention does not run at runtime.
    /// </para>
    /// </remarks>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <returns>The same builder.</returns>
    [RequiresDynamicCode("Closes the converter type for each Maybe<T>? with MakeGenericType while the model is built. Use a compiled model under NativeAOT.")]
    public static ModelConfigurationBuilder ConfigureNullableMaybeConventions(this ModelConfigurationBuilder configurationBuilder)
    {
        _ = configurationBuilder ?? throw new ArgumentNullException(nameof(configurationBuilder));
        configurationBuilder.Conventions.Add(static _ => new NullableMaybeConvention());
        return configurationBuilder;
    }

    private static IEnumInfo ThrowWithoutMetadata(Type enumType) =>
        throw new InvalidOperationException(
            $"Enum '{enumType.FullName}' was selected by the CanConvert predicate but has no generated metadata, so it would be stored as a number instead of a string. " +
            "Mark it [StringEnum] (and keep it public or internal) or use ConfigureEnumConventionsWithReflection.");

    private static ModelConfigurationBuilder AddConvention(
        ModelConfigurationBuilder configurationBuilder,
        EnumConventions? conventions,
        EnumStoredAs? existingStorage,
        Func<Type, IEnumInfo>? reflectionFallback)
    {
        _ = configurationBuilder ?? throw new ArgumentNullException(nameof(configurationBuilder));
        if (existingStorage is { } format)
            _ = EnumPropertyBuilderExtensions.ValidateLegacyFormat(format, nameof(existingStorage));

        EnumConventions resolved = conventions ?? EnumConventions.Default;
        configurationBuilder.Conventions.Add(services => new EnumStorageConvention(
            resolved,
            existingStorage,
            reflectionFallback,
            services.GetService<IDatabaseProvider>()?.Name,
            services.GetRequiredService<ITypeMappingSource>(),
            services.GetService<IRelationalTypeMappingSource>(),
            services.GetService<ISqlGenerationHelper>()));
        return configurationBuilder;
    }
}
