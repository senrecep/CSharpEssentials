using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Tests.Generators;

public sealed class IsolatedAnalyzerAssemblyLoader(string name) : IAnalyzerAssemblyLoader
{
    private readonly AssemblyLoadContext _loadContext = new(name);

    public void AddDependencyLocation(string fullPath)
    {
    }

    public Assembly LoadFromPath(string fullPath) =>
        _loadContext.LoadFromAssemblyPath(fullPath);
}
