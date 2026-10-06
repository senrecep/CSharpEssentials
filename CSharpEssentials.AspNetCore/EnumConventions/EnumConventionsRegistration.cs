using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// What <see cref="EnumConventionsExtensions.AddEnumConventions"/> registered: the conventions, how enum metadata is found
/// (generated only, or with the reflection opt-in) and how a rejected value becomes an <see cref="Error"/>.
/// </summary>
internal sealed class EnumConventionsRegistration
{
    private readonly Func<Type, IEnumInfo>? _reflectionFallback;
    private readonly Func<EnumWireFormat, EnumConverterFactory> _createConverterFactory;

    internal EnumConventionsRegistration(
        EnumConventions conventions,
        Func<Type, IEnumInfo>? reflectionFallback,
        Func<EnumWireFormat, EnumConverterFactory> createConverterFactory)
    {
        Conventions = conventions;
        _reflectionFallback = reflectionFallback;
        _createConverterFactory = createConverterFactory;
    }

    public EnumConventions Conventions { get; }

    /// <summary>Set by <see cref="EnumConventionsBuilder.ConfigureErrors"/>; <see langword="null"/> uses <see cref="DefaultError"/>.</summary>
    public Func<EnumValueError, string, Error>? ErrorFactory { get; set; }

    /// <summary>The JSON converter factory for request bodies and responses, with the given output format.</summary>
    public EnumConverterFactory CreateConverterFactory(EnumWireFormat writeAs) => _createConverterFactory(writeAs);

    public Error CreateError(EnumValueError error, string key) =>
        ErrorFactory is { } factory ? factory(error, key) : DefaultError(error, key);

    /// <summary>
    /// The metadata of an enum the conventions handle, or <see langword="null"/> for an enum left to the framework (the same
    /// selection as <see cref="EnumConverterFactory.CanConvert"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The enum is marked <see cref="StringEnumAttribute"/> but has no generated metadata and there is no reflection opt-in.
    /// </exception>
    public IEnumInfo? Resolve(Type enumType)
    {
        if (EnumMetadata.TryGet(enumType, out IEnumInfo? info))
            return Conventions.CanHandle(enumType) ? info : null;
        if (_reflectionFallback is not null)
            return Conventions.CanHandle(enumType) ? _reflectionFallback(enumType) : null;
        if (!enumType.IsDefined(typeof(StringEnumAttribute), inherit: false))
            return null;

        throw new InvalidOperationException(
            $"Enum '{enumType.FullName}' is marked [StringEnum] but has no generated metadata. Rebuild its project with the " +
            "CSharpEssentials.Enums 5.0 generator (C# 9 or newer) and make the enum and its containing types public or internal " +
            "(not private, protected, file-local or nested in a generic type).");
    }

    private static Error DefaultError(EnumValueError error, string key) =>
        Error.Validation(code: key, description: error.Message);
}
