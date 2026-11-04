using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Json.Generators;

/// <summary>
/// Reads the <c>[assembly: JsonPolymorphismModule(typeof(...))]</c> markers of referenced assemblies, so the generated registry can
/// register them first. Registries whose names collide are dropped; the runtime still finds them through the marker once loaded.
/// </summary>
internal static class ReferencedRegistryReader
{
    private const string RuntimeAssemblyName = "CSharpEssentials.Json";

    public static string[] Read(Compilation compilation, string ownRegistry, CancellationToken cancellationToken)
    {
        List<string> registries = [];
        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ReferencesRuntime(reference))
                continue;

            foreach (AttributeData attribute in reference.GetAttributes())
            {
                if (PolymorphismSymbols.Is(attribute, PolymorphismSymbols.ModuleAttribute) &&
                    attribute.ConstructorArguments.Length == 1 &&
                    attribute.ConstructorArguments[0].Value is INamedTypeSymbol registry &&
                    registry.DeclaredAccessibility == Accessibility.Public)
                {
                    registries.Add(PolymorphismSymbols.FullyQualified(registry));
                }
            }
        }

        return [.. registries
            .GroupBy(static name => name, StringComparer.Ordinal)
            .Where(group => group.Count() == 1 && group.Key != ownRegistry)
            .Select(static group => group.Key)
            .OrderBy(static name => name, StringComparer.Ordinal)];
    }

    private static bool ReferencesRuntime(IAssemblySymbol assembly) =>
        assembly.Modules.Any(static module => module.ReferencedAssemblies.Any(static identity =>
            string.Equals(identity.Name, RuntimeAssemblyName, StringComparison.Ordinal)));
}
