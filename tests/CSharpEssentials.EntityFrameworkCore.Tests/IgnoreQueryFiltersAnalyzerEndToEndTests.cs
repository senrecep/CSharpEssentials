using System.Collections.Immutable;
using System.Reflection;
using CSharpEssentials.Entity.Interfaces;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Tests;

// The CSE3001 tests in CSharpEssentials.Tests run against a stub of the EF Core 10 overloads; this runs the packaged analyzer against
// the real EF Core 10 assembly.
public sealed class IgnoreQueryFiltersAnalyzerEndToEndTests
{
    private const string AnalyzerTypeName = "CSharpEssentials.EntityFrameworkCore.Generators.IgnoreQueryFiltersAnalyzer";

    private const string Source = """
        using System.Linq;
        using CSharpEssentials.EntityFrameworkCore;
        using CSharpEssentials.Entity.Interfaces;
        using Microsoft.EntityFrameworkCore;

        namespace Sample;

        public sealed class Document : ISoftDeletableBase
        {
            public bool IsDeleted { get; set; }
        }

        public static class Queries
        {
            private static readonly string[] SoftDeleteOnly = [QueryFilterNames.SoftDelete];

            public static IQueryable<Document> All(IQueryable<Document> query) => query.IgnoreQueryFilters();

            public static IQueryable<Document> Named(IQueryable<Document> query) => query.IgnoreQueryFilters(SoftDeleteOnly);

            public static IQueryable<Document> Helper(IQueryable<Document> query) => query.IgnoreSoftDeleteQueryFilter();
        }
        """;

    [Fact]
    public async Task Analyzer_Should_Report_Only_The_Parameterless_Call_When_Compiled_Against_EF_Core_10()
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Sample",
            [CSharpSyntaxTree.ParseText(Source, new CSharpParseOptions(LanguageVersion.Latest))],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers([LoadAnalyzer()])
            .GetAllDiagnosticsAsync();

        diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        Diagnostic diagnostic = diagnostics.Where(static diagnostic => diagnostic.Id == "CSE3001").Should().ContainSingle().Subject;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Info);
        (await diagnostic.Location.SourceTree!.GetTextAsync()).Lines[diagnostic.Location.GetLineSpan().StartLinePosition.Line]
            .ToString().Should().Contain("All(");
    }

    private static DiagnosticAnalyzer LoadAnalyzer()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Analyzers", "CSharpEssentials.EntityFrameworkCore.Generators.dll");
        Type analyzerType = Assembly.LoadFrom(path).GetType(AnalyzerTypeName, throwOnError: true)!;
        return (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType)!;
    }

    private static IEnumerable<MetadataReference> References()
    {
        string[] platform = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        return platform
            .Where(static path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal) ||
                                  Path.GetFileName(path) is "mscorlib.dll" or "netstandard.dll")
            .Append(typeof(EntityFrameworkQueryableExtensions).Assembly.Location)
            .Append(typeof(ISoftDeletableBase).Assembly.Location)
            .Append(typeof(QueryFilterNames).Assembly.Location)
            .Distinct(StringComparer.Ordinal)
            .Select(static path => MetadataReference.CreateFromFile(path));
    }
}
