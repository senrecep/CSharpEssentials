using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Tests.Generators;

public static class AnalyzerHarness
{
    public static Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers) =>
        compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();

    public static Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(
        Compilation compilation,
        ImmutableArray<DiagnosticAnalyzer> analyzers,
        AnalyzerConfigOptionsProvider optionsProvider) =>
        compilation.WithAnalyzers(analyzers, new AnalyzerOptions([], optionsProvider)).GetAnalyzerDiagnosticsAsync();
}
