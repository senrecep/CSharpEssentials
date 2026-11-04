using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace CSharpEssentials.Json;

/// <summary>
/// Adds generated and scanned derived types to bases that System.Text.Json already treats as polymorphic, and sets the per-property
/// converters of <see cref="JsonPolymorphicPropertyAttribute"/>.
/// </summary>
internal sealed class PolymorphismModifier(IJsonTypeInfoResolver inner, PolymorphismScanner? scanner)
{
    public void Modify(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind == JsonTypeInfoKind.None)
        {
            if (typeInfo.Converter is IClrTypeNameConverter)
                typeInfo.PolymorphismOptions = null;
            return;
        }

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        ApplyPropertyConverters(typeInfo);
        if (typeInfo.PolymorphismOptions is { } polymorphism)
            AddDerivedTypes(typeInfo, polymorphism);
    }

    private void ApplyPropertyConverters(JsonTypeInfo typeInfo)
    {
        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (property.CustomConverter is not null || property.AttributeProvider is not MemberInfo { DeclaringType: { } declaringType } member)
                continue;

            property.CustomConverter = JsonPolymorphism.GetProperty(declaringType, member.Name)?.ConverterFactory?.Invoke()
                ?? scanner?.CreatePropertyConverter(member, property.PropertyType);
        }
    }

    private void AddDerivedTypes(JsonTypeInfo typeInfo, JsonPolymorphismOptions polymorphism)
    {
        bool open = polymorphism.DerivedTypes.Count == 0;
        Dictionary<object, Type> values = [];
        HashSet<Type> types = [];
        foreach (JsonDerivedType declared in polymorphism.DerivedTypes)
        {
            types.Add(declared.DerivedType);
            if (declared.TypeDiscriminator is { } value)
                values[value] = declared.DerivedType;
        }

        foreach (PolymorphicRegistration registration in JsonPolymorphism.GetDerivedTypes(typeInfo.Type))
        {
            Type derived = registration.Derived!;
            if (types.Contains(derived) || !IsInResolver(derived, typeInfo))
                continue;

            Add(typeInfo, polymorphism, values, types, derived, registration.TypeDiscriminator!);
        }

        if (scanner is not null && open)
        {
            foreach ((Type derived, object value) in scanner.FindDerivedTypes(typeInfo.Type))
            {
                if (types.Contains(derived) || !CanConfigure(derived, typeInfo))
                    continue;

                Add(typeInfo, polymorphism, values, types, derived, value);
            }
        }

        if (polymorphism.DerivedTypes.Count == 0)
        {
            throw new InvalidOperationException(
                $"Polymorphic base '{typeInfo.Type.FullName}' has no derived types. Add [JsonDerivedType] on it, " +
                "[JsonTypeDiscriminator] on a derived type, or use AddPolymorphismWithReflection.");
        }
    }

    private static void Add(
        JsonTypeInfo typeInfo,
        JsonPolymorphismOptions polymorphism,
        Dictionary<object, Type> values,
        HashSet<Type> types,
        Type derived,
        object value)
    {
        if (values.TryGetValue(value, out Type? existing))
        {
            throw new InvalidOperationException(
                $"The type discriminator '{value}' of polymorphic base '{typeInfo.Type.FullName}' is used by both '{Describe(existing)}' " +
                $"and '{Describe(derived)}'. Give each derived type its own [JsonTypeDiscriminator] value.");
        }

        polymorphism.DerivedTypes.Add(value is int number ? new JsonDerivedType(derived, number) : new JsonDerivedType(derived, (string)value));
        values.Add(value, derived);
        types.Add(derived);
    }

    private static string Describe(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";

    private bool IsInResolver(Type derived, JsonTypeInfo typeInfo) =>
        inner is DefaultJsonTypeInfoResolver || inner.GetTypeInfo(derived, typeInfo.Options) is not null;

    private bool CanConfigure(Type derived, JsonTypeInfo typeInfo)
    {
        if (!IsInResolver(derived, typeInfo))
            return false;

        try
        {
            _ = typeInfo.Options.GetTypeInfo(derived);
            return true;
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or ArgumentException)
        {
            return false;
        }
    }
}
