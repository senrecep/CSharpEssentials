using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CSharpEssentials.Tests.Generators;

public static partial class GeneratorVerifyInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
        Verifier.DerivePathInfo(static (sourceFile, _, type, method) =>
            new PathInfo(Path.Combine(Path.GetDirectoryName(sourceFile)!, "Snapshots"), type.Name, method.Name));
        VerifierSettings.ScrubLinesWithReplace(static line => GeneratedCodeVersion().Replace(line, "$1\"{version}\""));
    }

    [GeneratedRegex("(GeneratedCode\\(\"CSharpEssentials\\.[A-Za-z.]+\\.Generators\", )\"[0-9.]+\"")]
    private static partial Regex GeneratedCodeVersion();
}
