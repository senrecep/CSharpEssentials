using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// One options provider for every row. The global options are the defaults each package's build props expose, plus the
/// RootNamespace the SDK makes visible to the compiler for every project; per-tree
/// options are empty, so CSharpEssentialsEnumNaming and the Injectio options stay at their package defaults.
/// </summary>
public sealed class BenchOptionsProvider : AnalyzerConfigOptionsProvider
{
    public static BenchOptionsProvider Instance { get; } = new();

    private BenchOptionsProvider()
    {
    }

    public override AnalyzerConfigOptions GlobalOptions { get; } = new BenchConfigOptions(new Dictionary<string, string>(AnalyzerConfigOptions.KeyComparer)
    {
        ["build_property.IsTestProject"] = "false",
        ["build_property.RootNamespace"] = ProjectShape.AssemblyName,
        ["build_property.EnumGenerator_EnumMetadataSource"] = "EnumMemberAttribute",
        ["build_property.EnumGenerator_ForceExtensionMembers"] = "false",
        ["build_property.EnumGenerator_ForceInternal"] = "false",
    });

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => BenchConfigOptions.Empty;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => BenchConfigOptions.Empty;
}
