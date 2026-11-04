using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Json.Generators;

/// <summary>
/// Turns attributed symbols into equatable models. Symbols the generated code cannot reach are left out; the analyzers report them.
/// </summary>
internal static class PolymorphismModelReader
{
    public static DerivedTypeModel? ReadDerivedType(INamedTypeSymbol type, AttributeData attribute, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (type.IsAbstract || type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || type.IsGenericType || !PolymorphismSymbols.IsReachable(type) ||
            attribute.ConstructorArguments.Length != 1 || attribute.ConstructorArguments[0].Value is not (string or int) ||
            PolymorphismSymbols.HasAttribute(type, PolymorphismSymbols.JsonConverterAttribute))
        {
            return null;
        }

        string[] bases = [.. PolymorphismSymbols.PolymorphicAncestors(type)
            .Where(baseType => !baseType.IsGenericType && PolymorphismSymbols.IsReachable(baseType) && !PolymorphismSymbols.Lists(baseType, type))
            .Select(PolymorphismSymbols.FullyQualified)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)];

        return bases.Length == 0
            ? null
            : new DerivedTypeModel(
                PolymorphismSymbols.FullyQualified(type),
                PolymorphismSymbols.Literal(attribute.ConstructorArguments[0].Value!),
                new EquatableArray<string>(bases));
    }

    public static PropertyModel? ReadProperty(ISymbol member, AttributeData attribute, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ITypeSymbol? propertyType = member switch
        {
            IPropertySymbol property => property.Type,
            IFieldSymbol field => field.Type,
            _ => null,
        };

        if (propertyType is null || member.IsStatic || member.ContainingType is not { IsGenericType: false } declaringType ||
            !PolymorphismSymbols.IsReachable(declaringType) || !PolymorphismSymbols.IsReachable(propertyType) ||
            attribute.ConstructorArguments.Length != 1 || attribute.ConstructorArguments[0].Value is not string name ||
            PolymorphismSymbols.ElementBase(propertyType) is not { } baseType || !PolymorphismSymbols.IsReachable(baseType))
        {
            return null;
        }

        return new PropertyModel(
            PolymorphismSymbols.FullyQualified(declaringType),
            member.MetadataName,
            PolymorphismSymbols.FullyQualified(propertyType),
            PolymorphismSymbols.FullyQualified(baseType),
            PolymorphismSymbols.Literal(name));
    }
}
