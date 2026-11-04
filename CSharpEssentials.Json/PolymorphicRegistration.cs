using System.ComponentModel;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

/// <summary>
/// One generated registration: a derived type with its discriminator, or a property with its own discriminator property name.
/// Generated code creates it; it is not meant to be used directly.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PolymorphicRegistration
{
    private PolymorphicRegistration(Type owner, Type? derived, object? typeDiscriminator, string? memberName, Func<JsonConverter>? converterFactory)
    {
        Owner = owner;
        Derived = derived;
        TypeDiscriminator = typeDiscriminator;
        MemberName = memberName;
        ConverterFactory = converterFactory;
    }

    internal Type Owner { get; }

    internal Type? Derived { get; }

    internal object? TypeDiscriminator { get; }

    internal string? MemberName { get; }

    internal Func<JsonConverter>? ConverterFactory { get; }

    /// <summary>A derived type of <paramref name="baseType"/> with a string discriminator.</summary>
    public static PolymorphicRegistration DerivedType(Type baseType, Type derivedType, string typeDiscriminator) =>
        new(
            baseType ?? throw new ArgumentNullException(nameof(baseType)),
            derivedType ?? throw new ArgumentNullException(nameof(derivedType)),
            typeDiscriminator ?? throw new ArgumentNullException(nameof(typeDiscriminator)),
            null,
            null);

    /// <summary>A derived type of <paramref name="baseType"/> with an integer discriminator.</summary>
    public static PolymorphicRegistration DerivedType(Type baseType, Type derivedType, int typeDiscriminator) =>
        new(
            baseType ?? throw new ArgumentNullException(nameof(baseType)),
            derivedType ?? throw new ArgumentNullException(nameof(derivedType)),
            typeDiscriminator,
            null,
            null);

    /// <summary>A property or field of <paramref name="declaringType"/> with the converter that applies its discriminator property name.</summary>
    public static PolymorphicRegistration Property(Type declaringType, string memberName, Func<JsonConverter> converterFactory) =>
        new(
            declaringType ?? throw new ArgumentNullException(nameof(declaringType)),
            null,
            null,
            memberName ?? throw new ArgumentNullException(nameof(memberName)),
            converterFactory ?? throw new ArgumentNullException(nameof(converterFactory)));
}
