using Microsoft.CodeAnalysis;

namespace CSharpEssentials.DependencyInjection.Generators;

internal static class ServiceDependencyReader
{
    public static IReadOnlyList<ServiceDependency> Read(INamedTypeSymbol type)
    {
        if (type.IsGenericType || type.InstanceConstructors.Length != 1)
        {
            return [];
        }

        List<ServiceDependency> dependencies = [];
        foreach (IParameterSymbol parameter in type.InstanceConstructors[0].Parameters)
        {
            if (parameter.GetAttributes().Any(static attribute => ServiceTypeInspector.IsContainerAttribute(attribute, "ServiceKeyAttribute")))
            {
                continue;
            }

            AttributeData? fromKeyed = parameter.GetAttributes()
                .FirstOrDefault(static attribute => ServiceTypeInspector.IsContainerAttribute(attribute, "FromKeyedServicesAttribute"));
            string? key = fromKeyed is { ConstructorArguments.Length: 1 } && !fromKeyed.ConstructorArguments[0].IsNull
                ? ConstantFormatter.Format(fromKeyed.ConstructorArguments[0])
                : null;
            string? openServiceType = parameter.Type is INamedTypeSymbol { IsGenericType: true } named && !TypeNames.IsGenericDefinition(named)
                ? TypeNames.FullyQualified(named.OriginalDefinition.ConstructUnboundGenericType())
                : null;
            dependencies.Add(new ServiceDependency(parameter.Name, TypeNames.FullyQualified(parameter.Type), openServiceType, key));
        }

        return dependencies;
    }
}
