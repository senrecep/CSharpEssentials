using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Tests.Generators;

public sealed class AnalyzerAssembly
{
    private readonly AnalyzerFileReference _reference;

    private AnalyzerAssembly(AnalyzerFileReference reference) => _reference = reference;

    public static AnalyzerAssembly Load(string fileName) =>
        new(new AnalyzerFileReference(
            Path.Combine(AppContext.BaseDirectory, "Analyzers", fileName),
            new IsolatedAnalyzerAssemblyLoader(fileName)));

    public ImmutableArray<ISourceGenerator> Generators => _reference.GetGenerators(LanguageNames.CSharp);

    public ImmutableArray<DiagnosticAnalyzer> Analyzers => _reference.GetAnalyzers(LanguageNames.CSharp);
}
