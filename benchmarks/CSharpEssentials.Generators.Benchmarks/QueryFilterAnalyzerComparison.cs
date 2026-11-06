using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// The CSE3001 analyzer from CSharpEssentials.EntityFrameworkCore.Generators (an analyzer, not a generator; no competitor)
/// against a no-op invocation analyzer with the same registration shape. The project has 50 query files with 150
/// IgnoreQueryFilters calls (100 unnamed, which CSE3001 reports, and 50 named) plus filler, against a source stub of the
/// EF Core 10 API, not the EF Core assembly. Each call builds a fresh compilation and a fresh CompilationWithAnalyzers,
/// because results are cached per instance. Analysis is single-threaded for low noise.
/// <para>
/// Compilation runs the analyzer over the whole project. Document runs it over one edited file
/// (GetAnalyzerSemanticDiagnosticsAsync on Query025.cs after a method-body edit), the per-document IDE path after a keystroke.
/// Analyzers have no incremental state, so there is no incremental row. The no-op rows bind every method body to build
/// operations, which both rows share; the difference is CSE's own cost.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class QueryFilterAnalyzerComparison
{
    public const int Queries = 50;

    public const int EditedQuery = 25;

    private const string CompilationCategory = "Compilation";
    private const string DocumentCategory = "Document";

    private const string Stub = """
        using System.Collections.Generic;
        using System.Linq;

        namespace Microsoft.EntityFrameworkCore;

        public static class EntityFrameworkQueryableExtensions
        {
            public static IQueryable<T> IgnoreQueryFilters<T>(this IQueryable<T> source) where T : class => source;

            public static IQueryable<T> IgnoreQueryFilters<T>(this IQueryable<T> source, IReadOnlyCollection<string> filterKeys) where T : class => source;
        }
        """;

    private static readonly CompilationWithAnalyzersOptions s_options = new(
        new AnalyzerOptions([]),
        onAnalyzerException: null,
        concurrentAnalysis: false,
        logAnalyzerExecutionTime: false);

    private static readonly ImmutableArray<DiagnosticAnalyzer> s_noOp = [new NoOpInvocationAnalyzer()];

    private ProjectShape _shape = null!;
    private CSharpCompilation _original = null!;

    [Params(200, 1000)]
    public int Files { get; set; } = 200;

    [GlobalSetup]
    public void Setup()
    {
        string edited = $"Query{EditedQuery:000}";
        ImmutableArray<(string Name, string Text)> features =
        [
            ("EfStub", Stub),
            .. Enumerable.Range(1, Queries).Select(static n => ($"Query{n:000}", Query($"{n:000}", 0))),
        ];
        _shape = ProjectShape.Create(Files, features, edited, Query($"{EditedQuery:000}", 0), Query($"{EditedQuery:000}", 1));
        _original = _shape.CreateCompilation();

        string where = $"{nameof(QueryFilterAnalyzerComparison)} (Files={Files})";
        GeneratorParity.NoErrors(where, _original);
        Expect(where + " " + nameof(CSharpEssentials_Compilation), Queries * 2, Compilation(GeneratorSet.CseEntityFrameworkCore), "CSE3001");
        Expect(where + " " + nameof(CSharpEssentials_Document), 2, Document(GeneratorSet.CseEntityFrameworkCore), "CSE3001");
        Expect(where + " " + nameof(NoOpAnalyzer_Compilation), 0, Compilation(s_noOp), "BENCH0001");
        NoOpRan(where + " " + nameof(NoOpAnalyzer_Compilation));
        Expect(where + " " + nameof(NoOpAnalyzer_Document), 0, Document(s_noOp), "BENCH0001");
        Expect(where + " " + nameof(CSharpEssentials_Compilation), Queries * 2, CSharpEssentials_Compilation());
        Expect(where + " " + nameof(CSharpEssentials_Document), 2, CSharpEssentials_Document());
        Expect(where + " " + nameof(NoOpAnalyzer_Compilation), 0, NoOpAnalyzer_Compilation());
        Expect(where + " " + nameof(NoOpAnalyzer_Document), 0, NoOpAnalyzer_Document());
    }

    // https://learn.microsoft.com/dotnet/api/microsoft.codeanalysis.diagnostics.compilationwithanalyzers
    [Benchmark, BenchmarkCategory(CompilationCategory)]
    public async Task<int> NoOpAnalyzer_Compilation() => (await Compilation(s_noOp).ConfigureAwait(false)).Length;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#named-query-filters-ef-core-10
    [Benchmark(Baseline = true), BenchmarkCategory(CompilationCategory)]
    public async Task<int> CSharpEssentials_Compilation() => (await Compilation(GeneratorSet.CseEntityFrameworkCore).ConfigureAwait(false)).Length;

    // https://learn.microsoft.com/dotnet/api/microsoft.codeanalysis.diagnostics.compilationwithanalyzers
    [Benchmark, BenchmarkCategory(DocumentCategory)]
    public async Task<int> NoOpAnalyzer_Document() => (await Document(s_noOp).ConfigureAwait(false)).Length;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#named-query-filters-ef-core-10
    [Benchmark(Baseline = true), BenchmarkCategory(DocumentCategory)]
    public async Task<int> CSharpEssentials_Document() => (await Document(GeneratorSet.CseEntityFrameworkCore).ConfigureAwait(false)).Length;

    public Task<ImmutableArray<Diagnostic>> Compilation(ImmutableArray<DiagnosticAnalyzer> analyzers) =>
        new CompilationWithAnalyzers(_shape.CreateCompilation(), analyzers, s_options).GetAnalyzerDiagnosticsAsync();

    public Task<ImmutableArray<Diagnostic>> Document(ImmutableArray<DiagnosticAnalyzer> analyzers)
    {
        Compilation edited = _original.ReplaceSyntaxTree(_shape.Original, _shape.Edited);
        return new CompilationWithAnalyzers(edited, analyzers, s_options)
            .GetAnalyzerSemanticDiagnosticsAsync(edited.GetSemanticModel(_shape.Edited), filterSpan: null, CancellationToken.None);
    }

    private void NoOpRan(string where)
    {
        CompilationWithAnalyzersOptions timed = new(new AnalyzerOptions([]), onAnalyzerException: null, concurrentAnalysis: false, logAnalyzerExecutionTime: true);
        CompilationWithAnalyzers analyzers = new(_shape.CreateCompilation(), s_noOp, timed);
        _ = analyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        int actions = analyzers.GetAnalyzerTelemetryInfoAsync(s_noOp[0], CancellationToken.None).GetAwaiter().GetResult().OperationActionsCount;
        if (actions != 1)
        {
            throw new InvalidOperationException($"Parity check failed in {where}: the no-op analyzer registered {actions} operation actions, expected 1.");
        }
    }

    private static void Expect(string where, int count, Task<ImmutableArray<Diagnostic>> run, string id)
    {
        ImmutableArray<Diagnostic> diagnostics = run.GetAwaiter().GetResult();
        if (diagnostics.Length != count || diagnostics.Any(d => d.Id != id))
        {
            throw new InvalidOperationException(
                $"Parity check failed in {where}: expected {count} {id} diagnostics, got {diagnostics.Length} ({string.Join(", ", diagnostics.Select(static d => d.Id).Distinct())}).");
        }
    }

    private static void Expect(string where, int count, Task<int> run)
    {
        int actual = run.GetAwaiter().GetResult();
        if (actual != count)
        {
            throw new InvalidOperationException($"Parity check failed in {where}: the benchmark method returned {actual}, expected {count}.");
        }
    }

    private static string Query(string id, int added) => $$"""
        using Microsoft.EntityFrameworkCore;

        namespace Bench.App.Queries;

        public sealed class Order{{id}}
        {
            public int Id { get; set; }
        }

        public interface IQuery{{id}}
        {
            int Run(IQueryable<Order{{id}}> orders);
        }

        public sealed class Query{{id}} : IQuery{{id}}
        {
            private static readonly string[] SoftDeleteOnly = ["SoftDelete"];

            public int Run(IQueryable<Order{{id}}> orders)
            {
                int all = orders.IgnoreQueryFilters().Count();
                int again = orders.IgnoreQueryFilters().Count();
                int named = orders.IgnoreQueryFilters(SoftDeleteOnly).Count();
                return all + again + named + {{added}};
            }
        }
        """;
}
