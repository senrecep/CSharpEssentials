using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Tests.DependencyInjection.Generation;

internal sealed class ServiceGeneratorOptionsProvider(IReadOnlyDictionary<string, string> globalValues) : AnalyzerConfigOptionsProvider
{
    private static readonly AnalyzerConfigOptions Empty = new ServiceGeneratorGlobalOptions(new Dictionary<string, string>());

    public override AnalyzerConfigOptions GlobalOptions { get; } = new ServiceGeneratorGlobalOptions(globalValues);

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Empty;
}
