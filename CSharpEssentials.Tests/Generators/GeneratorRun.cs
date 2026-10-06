using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Tests.Generators;

public sealed record GeneratorRun(
    GeneratorDriver Driver,
    GeneratorDriverRunResult Result,
    Compilation OutputCompilation,
    ImmutableArray<Diagnostic> GeneratorDiagnostics)
{
    public ImmutableArray<Diagnostic> OutputDiagnostics => OutputCompilation.GetDiagnostics();
}
