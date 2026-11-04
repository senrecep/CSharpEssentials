using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CSharpEssentials.Json;

/// <summary>
/// One copy of the caller's options per base type and discriminator property name, in which that base uses the property name.
/// </summary>
internal static class PolymorphicPropertyShadows
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, ConcurrentDictionary<(Type, string), JsonSerializerOptions>> Shadows = [];

    internal static JsonSerializerOptions Get(JsonSerializerOptions options, Type baseType, string propertyName) =>
        Shadows.GetValue(options, static _ => new ConcurrentDictionary<(Type, string), JsonSerializerOptions>())
            .GetOrAdd((baseType, propertyName), static (key, source) => Create(source, key.Item1, key.Item2), options);

    private static JsonSerializerOptions Create(JsonSerializerOptions source, Type baseType, string propertyName)
    {
        IJsonTypeInfoResolver resolver = source.TypeInfoResolver
            ?? throw new InvalidOperationException("The serializer options have no TypeInfoResolver.");
        return new JsonSerializerOptions(source)
        {
            TypeInfoResolver = resolver.WithAddedModifier(typeInfo => Rename(typeInfo, baseType, propertyName)),
        };
    }

    private static void Rename(JsonTypeInfo typeInfo, Type baseType, string propertyName)
    {
        if (typeInfo.Type == baseType && typeInfo.PolymorphismOptions is { } polymorphism)
            polymorphism.TypeDiscriminatorPropertyName = propertyName;

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (property.CustomConverter is IPolymorphicPropertyConverter converter &&
                converter.BaseType == baseType &&
                string.Equals(converter.TypeDiscriminatorPropertyName, propertyName, StringComparison.Ordinal))
            {
                property.CustomConverter = null;
            }
        }
    }
}
