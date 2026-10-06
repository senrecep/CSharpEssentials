using System.Reflection;

namespace CSharpEssentials.DependencyInjection;

internal sealed class RegisteredAssemblyMarker(Assembly assembly)
{
    public Assembly Assembly { get; } = assembly;
}
