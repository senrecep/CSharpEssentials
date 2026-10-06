using Microsoft.CodeAnalysis;

namespace CSharpEssentials.DependencyInjection.Generators;

internal static class TypeNames
{
    public static string FullyQualified(ITypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string ForTypeOf(ITypeSymbol type) =>
        type is INamedTypeSymbol named && IsGenericDefinition(named)
            ? FullyQualified(named.ConstructUnboundGenericType())
            : FullyQualified(type);

    public static bool IsGenericDefinition(INamedTypeSymbol type) =>
        type.IsGenericType && SymbolEqualityComparer.Default.Equals(type, type.OriginalDefinition);

    public static string MetadataFullName(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named)
        {
            return type.ToDisplayString();
        }

        List<string> names = [named.MetadataName];
        for (INamedTypeSymbol? containing = named.ContainingType; containing is not null; containing = containing.ContainingType)
        {
            names.Insert(0, containing.MetadataName);
        }

        string name = string.Join("+", names);
        return named.ContainingNamespace is { IsGlobalNamespace: false } ns ? ns.ToDisplayString() + "." + name : name;
    }
}
