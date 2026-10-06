using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Endpoints.Generators;

internal static class ReferencedRegistryReader
{
    public static IReadOnlyList<ReferencedRegistry> Read(Compilation compilation, CancellationToken cancellationToken)
    {
        List<ReferencedRegistry> registries = [];
        HashSet<ReferencedRegistry> seen = [];
        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsFrameworkAssembly(reference.Name) || !ReferencesEndpoints(reference))
            {
                continue;
            }

            foreach (AttributeData attribute in reference.GetAttributes())
            {
                if (EndpointSymbolRules.IsEndpointsType(attribute.AttributeClass, EndpointSymbolRules.ModuleAttribute) &&
                    attribute.ConstructorArguments.FirstOrDefault().Value is INamedTypeSymbol registry)
                {
                    ReferencedRegistry entry = new(reference.Name, EndpointSymbolRules.GetFullyQualifiedName(registry));
                    if (seen.Add(entry))
                    {
                        registries.Add(entry);
                    }
                }
            }
        }

        return [.. registries
            .OrderBy(static registry => registry.AssemblyName, StringComparer.Ordinal)
            .ThenBy(static registry => registry.FullyQualifiedName, StringComparer.Ordinal)];
    }

    public static IEnumerable<IGrouping<string, ReferencedRegistry>> FindCollisions(IReadOnlyList<ReferencedRegistry> registries) =>
        registries
            .GroupBy(static registry => registry.FullyQualifiedName, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1);

    private static bool IsFrameworkAssembly(string name) =>
        name.StartsWith("System", StringComparison.Ordinal) ||
        name.StartsWith("Microsoft", StringComparison.Ordinal) ||
        string.Equals(name, "mscorlib", StringComparison.Ordinal) ||
        string.Equals(name, "netstandard", StringComparison.Ordinal);

    private static bool ReferencesEndpoints(IAssemblySymbol assembly) =>
        assembly.Modules.Any(static module => module.ReferencedAssemblies.Any(static identity =>
            string.Equals(identity.Name, EndpointSymbolRules.EndpointsAssemblyName, StringComparison.Ordinal)));
}
