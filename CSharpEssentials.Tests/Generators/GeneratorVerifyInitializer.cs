using System.Runtime.CompilerServices;

namespace CSharpEssentials.Tests.Generators;

public static class GeneratorVerifyInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
        Verifier.DerivePathInfo(static (sourceFile, _, type, method) =>
            new PathInfo(Path.Combine(Path.GetDirectoryName(sourceFile)!, "Snapshots"), type.Name, method.Name));
    }
}
