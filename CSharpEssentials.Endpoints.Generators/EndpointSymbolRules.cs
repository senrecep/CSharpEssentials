using System.Text;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Endpoints.Generators;

internal static class EndpointSymbolRules
{
    public const string Namespace = "CSharpEssentials.Endpoints";

    public const string EndpointsAssemblyName = "CSharpEssentials.Endpoints";

    public const string EndpointInterface = "IEndpoint";

    public const string GroupInterface = "IEndpointGroup";

    public const string GroupAttribute = "EndpointGroupAttribute";

    public const string ExcludeAttribute = "ExcludeFromMappingAttribute";

    public const string RegistryNameAttribute = "EndpointRegistryNameAttribute";

    public const string ModuleAttribute = "EndpointModuleAttribute";

    public const string GenerateAggregateAttribute = "GenerateEndpointAggregateAttribute";

    public const string DisableAggregateAttribute = "DisableEndpointAggregateAttribute";

    public static bool IsEndpoint(INamedTypeSymbol type) => Implements(type, EndpointInterface);

    public static bool IsGroup(INamedTypeSymbol type) => Implements(type, GroupInterface);

    public static bool IsEndpointsType(INamedTypeSymbol? symbol, string name) =>
        symbol is { ContainingType: null } &&
        string.Equals(symbol.Name, name, StringComparison.Ordinal) &&
        string.Equals(symbol.ContainingNamespace?.ToDisplayString(), Namespace, StringComparison.Ordinal);

    public static bool HasAttribute(ISymbol symbol, string attributeName) =>
        symbol.GetAttributes().Any(attribute => IsEndpointsType(attribute.AttributeClass, attributeName));

    public static bool IsExcluded(INamedTypeSymbol type) =>
        HasAttribute(type, ExcludeAttribute) || HasAttribute(type.ContainingAssembly, ExcludeAttribute);

    public static bool IsAccessible(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.IsFileLocal ||
                current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsAbstractOrOpenGeneric(INamedTypeSymbol type)
    {
        if (type.IsAbstract)
        {
            return true;
        }

        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.TypeParameters.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<GroupTarget> GetGroupTargets(INamedTypeSymbol type)
    {
        List<GroupTarget> targets = [];
        foreach (AttributeData attribute in type.GetAttributes())
        {
            INamedTypeSymbol? attributeClass = attribute.AttributeClass;
            if (!IsEndpointsType(attributeClass, GroupAttribute))
            {
                continue;
            }

            ITypeSymbol? target = attributeClass!.Arity == 1
                ? attributeClass.TypeArguments[0]
                : attribute.ConstructorArguments.FirstOrDefault().Value as ITypeSymbol;
            targets.Add(new GroupTarget(attribute, target));
        }

        return targets;
    }

    public static bool IsValidGroupTarget(ITypeSymbol? target) =>
        target is INamedTypeSymbol group &&
        group.TypeKind is not TypeKind.Error &&
        !group.IsUnboundGenericType &&
        IsGroup(group) &&
        !IsAbstractOrOpenGeneric(group);

    public static GroupChain ResolveGroupChain(INamedTypeSymbol type)
    {
        List<INamedTypeSymbol> groups = [];
        HashSet<INamedTypeSymbol> visited = [with(SymbolEqualityComparer.Default), type];
        INamedTypeSymbol current = type;
        while (true)
        {
            IReadOnlyList<GroupTarget> targets = GetGroupTargets(current);
            if (targets.Count == 0)
            {
                return new GroupChain(GroupChainStatus.Valid, groups);
            }

            if (targets.Count > 1)
            {
                return new GroupChain(GroupChainStatus.MultipleGroupAttributes, groups);
            }

            if (!IsValidGroupTarget(targets[0].Type))
            {
                return new GroupChain(GroupChainStatus.InvalidGroupTarget, groups);
            }

            var group = (INamedTypeSymbol)targets[0].Type!;
            if (!visited.Add(group))
            {
                return new GroupChain(GroupChainStatus.Cycle, groups);
            }

            if (!IsAccessible(group))
            {
                return new GroupChain(GroupChainStatus.InaccessibleGroup, groups);
            }

            if (HasAttribute(group, ExcludeAttribute))
            {
                return new GroupChain(GroupChainStatus.SkippedGroup, groups);
            }

            groups.Insert(0, group);
            current = group;
        }
    }

    public static string GetFullyQualifiedName(INamedTypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string GetMetadataFullName(INamedTypeSymbol type)
    {
        StringBuilder builder = new(type.MetadataName);
        for (INamedTypeSymbol? container = type.ContainingType; container is not null; container = container.ContainingType)
        {
            builder.Insert(0, '+').Insert(0, container.MetadataName);
            type = container;
        }

        INamespaceSymbol? ns = type.ContainingNamespace;
        if (ns is { IsGlobalNamespace: false })
        {
            builder.Insert(0, '.').Insert(0, ns.ToDisplayString());
        }

        return builder.ToString();
    }

    private static bool Implements(INamedTypeSymbol type, string interfaceName) =>
        type.AllInterfaces.Any(candidate => IsEndpointsType(candidate, interfaceName));
}
