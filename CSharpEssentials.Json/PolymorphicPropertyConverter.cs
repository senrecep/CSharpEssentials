using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CSharpEssentials.Json;

/// <summary>
/// Serializes one property through System.Text.Json's polymorphism with another discriminator property name for one base type.
/// Created by generated code and by <see cref="JsonPolymorphismExtensions.AddPolymorphismWithReflection"/>; it is not meant to be used directly.
/// </summary>
/// <typeparam name="TProperty">The declared type of the property.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PolymorphicPropertyConverter<TProperty> : JsonConverter<TProperty>, IPolymorphicPropertyConverter
{
    /// <summary>Creates the converter.</summary>
    /// <param name="baseType">The polymorphic base whose discriminator property is renamed.</param>
    /// <param name="typeDiscriminatorPropertyName">The discriminator property name used inside this property.</param>
    public PolymorphicPropertyConverter(Type baseType, string typeDiscriminatorPropertyName)
    {
        BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
        TypeDiscriminatorPropertyName = typeDiscriminatorPropertyName ?? throw new ArgumentNullException(nameof(typeDiscriminatorPropertyName));
    }

    /// <inheritdoc />
    public Type BaseType { get; }

    /// <inheritdoc />
    public string TypeDiscriminatorPropertyName { get; }

    /// <inheritdoc />
    public override TProperty? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize(ref reader, GetTypeInfo(options));

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TProperty value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, GetTypeInfo(options));

    private JsonTypeInfo<TProperty> GetTypeInfo(JsonSerializerOptions options)
    {
        JsonSerializerOptions shadow = PolymorphicPropertyShadows.Get(options, BaseType, TypeDiscriminatorPropertyName);
#if NET11_0_OR_GREATER
        return shadow.GetTypeInfo<TProperty>();
#else
        return (JsonTypeInfo<TProperty>)shadow.GetTypeInfo(typeof(TProperty));
#endif
    }
}
