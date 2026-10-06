using Microsoft.CodeAnalysis;

namespace CSharpEssentials.DependencyInjection.Generators;

internal static class ReferencedRegistryReader
{
    private const string RuntimeAssemblyName = "CSharpEssentials.DependencyInjection";

    public static IReadOnlyList<ReferencedRegistry> Read(Compilation compilation, CancellationToken cancellationToken)
    {
        List<ReferencedRegistry> registries = [];
        HashSet<ReferencedRegistry> seen = [];
        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsFrameworkAssembly(reference.Name) || !ReferencesRuntime(reference))
            {
                continue;
            }

            foreach (AttributeData attribute in reference.GetAttributes())
            {
                if (ServiceTypeInspector.IsAttribute(attribute.AttributeClass, "ServiceModuleAttribute") &&
                    attribute.ConstructorArguments.FirstOrDefault().Value is INamedTypeSymbol registry)
                {
                    ReferencedRegistry entry = new(reference.Name, TypeNames.FullyQualified(registry));
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

    private static bool ReferencesRuntime(IAssemblySymbol assembly) =>
        assembly.Modules.Any(static module => module.ReferencedAssemblies.Any(static identity =>
            string.Equals(identity.Name, RuntimeAssemblyName, StringComparison.Ordinal)));
}
