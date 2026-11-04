using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

/// <summary>
/// Finds derived types and per-property discriminator names by reflection, for <see cref="JsonPolymorphismExtensions.AddPolymorphismWithReflection"/>.
/// </summary>
[RequiresUnreferencedCode(JsonPolymorphismExtensions.ReflectionMessage)]
[RequiresDynamicCode(JsonPolymorphismExtensions.ReflectionMessage)]
internal sealed class PolymorphismScanner(Assembly[] trustedAssemblies)
{
    private readonly ConcurrentDictionary<Type, (Type Type, object Value)[]> _derivedTypes = new();

    public (Type Type, object Value)[] FindDerivedTypes(Type baseType) => _derivedTypes.GetOrAdd(baseType, Scan);

    public JsonConverter? CreatePropertyConverter(MemberInfo member, Type propertyType)
    {
        if (member.GetCustomAttribute<JsonPolymorphicPropertyAttribute>() is not { } attribute ||
            ElementType(propertyType) is not { } baseType ||
            !IsPolymorphicBase(baseType))
        {
            return null;
        }

        Type converterType = typeof(PolymorphicPropertyConverter<>).MakeGenericType(propertyType);
        return Activator.CreateInstance(converterType, baseType, attribute.TypeDiscriminatorPropertyName) as JsonConverter;
    }

    private (Type Type, object Value)[] Scan(Type baseType)
    {
        IEnumerable<Assembly> assemblies = trustedAssemblies.Length > 0
            ? trustedAssemblies
            : AppDomain.CurrentDomain.GetAssemblies().Where(static assembly => !PolymorphicTypeScan.IsFrameworkAssembly(assembly));

        List<(Type Type, object Value)> found = [];
        foreach (Assembly assembly in assemblies.Distinct())
        {
            foreach (Type type in PolymorphicTypeScan.GetLoadableTypes(assembly))
            {
                if (IsCandidate(baseType, type))
                    found.Add((type, type.GetCustomAttribute<JsonTypeDiscriminatorAttribute>(false)?.TypeDiscriminator ?? type.FullName ?? type.Name));
            }
        }

        return [.. found.OrderBy(static entry => entry.Type.FullName, StringComparer.Ordinal)];
    }

    private static bool IsCandidate(Type baseType, Type type) =>
        type != baseType &&
        baseType.IsAssignableFrom(type) &&
        !type.IsAbstract &&
        !type.IsInterface &&
        !type.ContainsGenericParameters &&
        !type.IsByRefLike &&
        !type.IsDefined(typeof(JsonConverterAttribute), false) &&
        !PolymorphicTypeScan.IsFrameworkAssembly(type.Assembly);

    private static bool IsPolymorphicBase(Type type) =>
        type.IsDefined(typeof(JsonPolymorphicAttribute), false) || type.IsDefined(typeof(JsonDerivedTypeAttribute), false);

    private static Type? ElementType(Type type)
    {
        if (IsPolymorphicBase(type))
            return type;

        if (type.IsArray)
            return type.GetElementType() is { } element ? ElementType(element) : null;

        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return ElementType(underlying);

        Type[] interfaces = type.IsInterface ? [type, .. type.GetInterfaces()] : type.GetInterfaces();
        Type? dictionary = Array.Find(interfaces, static candidate => IsGeneric(candidate, typeof(IDictionary<,>)) || IsGeneric(candidate, typeof(IReadOnlyDictionary<,>)));
        if (dictionary is not null)
            return ElementType(dictionary.GetGenericArguments()[1]);

        Type? enumerable = Array.Find(interfaces, static candidate => IsGeneric(candidate, typeof(IEnumerable<>)));
        return enumerable is not null && enumerable.GetGenericArguments()[0] != typeof(char)
            ? ElementType(enumerable.GetGenericArguments()[0])
            : null;
    }

    private static bool IsGeneric(Type type, Type definition) => type.IsGenericType && type.GetGenericTypeDefinition() == definition;
}
