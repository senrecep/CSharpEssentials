using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Tests.Enums;

public class NestedStringEnumAnalyzerTests
{
    private static readonly Lazy<ImmutableArray<DiagnosticAnalyzer>> Analyzers = new(LoadAnalyzers);

    [Fact]
    public async Task Analyzer_Should_Report_CSE0001_When_StringEnum_Is_Nested_In_Class()
    {
        const string source = """
            using CSharpEssentials.Enums;

            namespace Sample;

            public class Container
            {
                [StringEnum]
                public enum Color { Red, Green }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0001");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Info);
        diagnostic.GetMessage(CultureInfo.InvariantCulture).Should().Contain("'Color'").And.Contain("Sample.Container").And.Contain("top-level");
        LocationText(diagnostic).Should().Be("Color");
    }

    [Fact]
    public async Task Analyzer_Should_Report_CSE0001_When_StringEnum_Is_Nested_In_Struct()
    {
        const string source = """
            using CSharpEssentials.Enums;

            namespace Sample;

            public struct Container
            {
                [StringEnum]
                internal enum Status { Active, Inactive }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(d => d.Id == "CSE0001").Subject;
        diagnostic.GetMessage(CultureInfo.InvariantCulture).Should().Contain("'Status'");
        LocationText(diagnostic).Should().Be("Status");
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_When_StringEnum_Is_TopLevel()
    {
        const string source = """
            using CSharpEssentials.Enums;

            namespace Sample;

            [StringEnum]
            public enum Color { Red, Green }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task Analyzer_Should_Not_Report_When_Nested_Enum_Has_No_StringEnum_Attribute()
    {
        const string source = """
            namespace Sample;

            public class Container
            {
                public enum Color { Red, Green }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().BeEmpty();
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        string runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        MetadataReference[] references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(typeof(StringEnumAttribute).Assembly.Location)
        ];

        CSharpCompilation compilation = CSharpCompilation.Create(
            "NestedStringEnumAnalyzerTests",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

        return await compilation
            .WithAnalyzers(Analyzers.Value)
            .GetAnalyzerDiagnosticsAsync();
    }

    private static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzers()
    {
        string analyzerPath = Path.Combine(AppContext.BaseDirectory, "Analyzers", "CSharpEssentials.Enums.Generators.dll");
        AnalyzerFileReference reference = new(analyzerPath, new IsolatedAnalyzerAssemblyLoader());

        ImmutableArray<DiagnosticAnalyzer> analyzers = reference.GetAnalyzers(LanguageNames.CSharp);
        analyzers.Should().ContainSingle(a => a.SupportedDiagnostics.Any(d => d.Id == "CSE0001"));
        return analyzers;
    }

    private static string LocationText(Diagnostic diagnostic) =>
        diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);

    private sealed class IsolatedAnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
    {
        private readonly AssemblyLoadContext _loadContext = new(nameof(NestedStringEnumAnalyzerTests));

        public void AddDependencyLocation(string fullPath)
        {
        }

        public Assembly LoadFromPath(string fullPath) =>
            _loadContext.LoadFromAssemblyPath(fullPath);
    }
}
