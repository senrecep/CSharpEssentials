using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// Loads one analyzer folder into its own load context. A dependency is taken from the same folder when the file exists
/// there (for example Scriban.dll next to the Immediate generators); everything else, including Microsoft.CodeAnalysis,
/// resolves from the default context so the generator sees the host Roslyn types.
/// </summary>
public sealed class AnalyzerFolderLoader(string folder) : AssemblyLoadContext(Path.GetFileName(folder)), IAnalyzerAssemblyLoader
{
    public void AddDependencyLocation(string fullPath)
    {
    }

    public Assembly LoadFromPath(string fullPath) => LoadFromAssemblyPath(fullPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string candidate = Path.Combine(folder, assemblyName.Name + ".dll");
        return File.Exists(candidate) ? LoadFromAssemblyPath(candidate) : null;
    }
}
