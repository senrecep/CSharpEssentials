using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Json.Generators;

/// <summary>
/// Symbol queries shared by the generator and the analyzers.
/// </summary>
internal static class PolymorphismSymbols
{
    public const string TypeDiscriminatorAttribute = "CSharpEssentials.Json.JsonTypeDiscriminatorAttribute";

    public const string PolymorphicPropertyAttribute = "CSharpEssentials.Json.JsonPolymorphicPropertyAttribute";

    public const string RegistryType = "CSharpEssentials.Json.JsonPolymorphism";

    public const string ModuleAttribute = "CSharpEssentials.Json.JsonPolymorphismModuleAttribute";

    public const string JsonPolymorphicAttribute = "System.Text.Json.Serialization.JsonPolymorphicAttribute";

    public const string JsonDerivedTypeAttribute = "System.Text.Json.Serialization.JsonDerivedTypeAttribute";

    public const string JsonConverterAttribute = "System.Text.Json.Serialization.JsonConverterAttribute";

    public const string JsonPropertyNameAttribute = "System.Text.Json.Serialization.JsonPropertyNameAttribute";

    public const string DefaultPropertyName = "$type";

    public static bool Is(AttributeData attribute, string metadataName) =>
        attribute.AttributeClass?.ToDisplayString() == metadataName;

    public static bool HasAttribute(ISymbol symbol, string metadataName) =>
        symbol.GetAttributes().Any(attribute => Is(attribute, metadataName));

    /// <summary>A base System.Text.Json treats as polymorphic: it carries <c>[JsonPolymorphic]</c> or <c>[JsonDerivedType]</c>.</summary>
    public static bool IsPolymorphicBase(ITypeSymbol type) =>
        type is INamedTypeSymbol && (HasAttribute(type, JsonPolymorphicAttribute) || HasAttribute(type, JsonDerivedTypeAttribute));

    /// <summary>The <c>[JsonDerivedType]</c> entries of a base: the derived type and its discriminator (null when none is given).</summary>
    public static IEnumerable<(ITypeSymbol Type, object? Value)> DeclaredDerivedTypes(INamedTypeSymbol baseType)
    {
        foreach (AttributeData attribute in baseType.GetAttributes())
        {
            if (!Is(attribute, JsonDerivedTypeAttribute) || attribute.ConstructorArguments.Length == 0 ||
                attribute.ConstructorArguments[0].Value is not ITypeSymbol derived)
            {
                continue;
            }

            object? value = attribute.ConstructorArguments.Length > 1 ? attribute.ConstructorArguments[1].Value : null;
            yield return (derived, value);
        }
    }

    public static bool Lists(INamedTypeSymbol baseType, ITypeSymbol derived) =>
        DeclaredDerivedTypes(baseType).Any(entry => SymbolEqualityComparer.Default.Equals(entry.Type, derived));

    public static string DiscriminatorPropertyName(INamedTypeSymbol baseType)
    {
        foreach (AttributeData attribute in baseType.GetAttributes())
        {
            if (!Is(attribute, JsonPolymorphicAttribute))
                continue;

            foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
            {
                if (argument.Key == "TypeDiscriminatorPropertyName" && argument.Value.Value is string name)
                    return name;
            }
        }

        return DefaultPropertyName;
    }

    /// <summary>The base classes and interfaces of <paramref name="type"/> that are polymorphic bases.</summary>
    public static IEnumerable<INamedTypeSymbol> PolymorphicAncestors(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (IsPolymorphicBase(current))
                yield return current;
        }

        foreach (INamedTypeSymbol contract in type.AllInterfaces)
        {
            if (IsPolymorphicBase(contract))
                yield return contract;
        }
    }

    /// <summary>The value of <c>[JsonTypeDiscriminator]</c>: a <see cref="string"/> or an <see cref="int"/>, or null when absent.</summary>
    public static object? Discriminator(ISymbol type)
    {
        foreach (AttributeData attribute in type.GetAttributes())
        {
            if (Is(attribute, TypeDiscriminatorAttribute) && attribute.ConstructorArguments.Length == 1)
                return attribute.ConstructorArguments[0].Value;
        }

        return null;
    }

    public static string Literal(object value) =>
        value is int number
            ? number.ToString(CultureInfo.InvariantCulture)
            : SymbolDisplay.FormatLiteral((string)value, quote: true);

    public static string Display(object value) =>
        value is int number ? number.ToString(CultureInfo.InvariantCulture) : (string)value;

    public static string FullyQualified(ITypeSymbol type) =>
        type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <summary>The runtime <c>Type.FullName</c> of a non-generic type: nested types are joined with <c>+</c>.</summary>
    public static string ClrFullName(INamedTypeSymbol type)
    {
        List<string> names = [type.MetadataName];
        for (INamedTypeSymbol? containing = type.ContainingType; containing is not null; containing = containing.ContainingType)
        {
            names.Insert(0, containing.MetadataName);
        }

        string name = string.Join("+", names);
        return type.ContainingNamespace is { IsGlobalNamespace: false } ns ? ns.ToDisplayString() + "." + name : name;
    }

    /// <summary>
    /// Whether a namespace-level generated class can name the type: no private, protected or file-local type in the chain, no type
    /// parameter, and every type argument reachable too.
    /// </summary>
    public static bool IsReachable(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                return IsReachable(array.ElementType);
            case INamedTypeSymbol named:
                for (INamedTypeSymbol? current = named; current is not null; current = current.ContainingType)
                {
                    if (current.IsFileLocal || current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal))
                        return false;
                }

                return named.TypeArguments.All(IsReachable);
            default:
                return false;
        }
    }

    /// <summary>
    /// The polymorphic base a property type holds: the type itself, or the element type of an array, a <c>Nullable&lt;T&gt;</c>,
    /// a dictionary value or an <c>IEnumerable&lt;T&gt;</c>. Null when none of them is a polymorphic base.
    /// </summary>
    public static INamedTypeSymbol? ElementBase(ITypeSymbol type)
    {
        if (IsPolymorphicBase(type))
            return (INamedTypeSymbol)type;

        if (type is IArrayTypeSymbol array)
            return ElementBase(array.ElementType);

        if (type is not INamedTypeSymbol named || type.SpecialType == SpecialType.System_String)
            return null;

        if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            return ElementBase(named.TypeArguments[0]);

        INamedTypeSymbol[] all = named.TypeKind == TypeKind.Interface ? [named, .. named.AllInterfaces] : [.. named.AllInterfaces];
        INamedTypeSymbol? dictionary = all.FirstOrDefault(static contract =>
            IsGeneric(contract, "System.Collections.Generic.IDictionary<TKey, TValue>") ||
            IsGeneric(contract, "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>"));
        if (dictionary is not null)
            return ElementBase(dictionary.TypeArguments[1]);

        INamedTypeSymbol? enumerable = all.FirstOrDefault(static contract => contract.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);
        return enumerable is null ? null : ElementBase(enumerable.TypeArguments[0]);
    }

    private static bool IsGeneric(INamedTypeSymbol type, string definition) =>
        type.IsGenericType && type.OriginalDefinition.ToDisplayString() == definition;
}
