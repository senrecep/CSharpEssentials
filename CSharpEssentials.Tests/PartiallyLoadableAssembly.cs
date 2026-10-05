using System.Reflection;
using System.Reflection.Emit;

namespace CSharpEssentials.Tests;

/// <summary>
/// Builds a dynamic assembly whose <see cref="Assembly.GetTypes"/> throws <see cref="ReflectionTypeLoadException"/>,
/// like a proxy assembly that is still emitting types.
/// </summary>
internal static class PartiallyLoadableAssembly
{
    public static AssemblyBuilder Create(string name)
    {
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder module = assembly.DefineDynamicModule(name);
        module.DefineType("Created", TypeAttributes.Public).CreateType();
        // Never created: GetTypes() reports it as unloadable
        module.DefineType("Pending", TypeAttributes.Public);
        return assembly;
    }
}
