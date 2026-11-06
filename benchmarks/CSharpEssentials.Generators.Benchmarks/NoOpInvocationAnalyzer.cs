using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// The floor for the query-filter analyzer rows: the same registration shape as CSE3001 (an invocation operation action
/// registered at compilation start) with an empty callback. It declares one enabled descriptor that it never reports,
/// because an analyzer without enabled diagnostics is skipped by the driver.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoOpInvocationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        "BENCH0001",
        "Benchmark no-op",
        "Benchmark no-op",
        "Benchmark",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start => start.RegisterOperationAction(static _ => { }, OperationKind.Invocation));
    }
}
