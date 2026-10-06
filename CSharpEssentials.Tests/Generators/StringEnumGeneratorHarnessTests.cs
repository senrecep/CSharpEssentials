using System.Collections.Immutable;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Generators;

public class StringEnumGeneratorHarnessTests
{
    private const string Source = """
        using CSharpEssentials.Enums;

        namespace Sample;

        [StringEnum]
        public enum OrderStatus
        {
            Pending,
            InProgress,
            Shipped
        }
        """;

    private static readonly Lazy<AnalyzerAssembly> EnumsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Enums.dll"));

    [Fact]
    public Task StringEnumGenerator_Should_Match_Snapshot()
    {
        GeneratorRun run = GeneratorHarness.Run(CreateCompilation(), [.. EnumsAssembly.Value.Generators]);

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        return Verify(run.Driver);
    }

    [Fact]
    public void StringEnumGenerator_Should_Return_Cached_Outputs_When_Compilation_Is_Identical()
    {
        CSharpCompilation compilation = CreateCompilation();

        GeneratorDriverRunResult second = IncrementalCaching.RunTwice(compilation, static c => c, [.. EnumsAssembly.Value.Generators]);

        IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
    }

    [Fact]
    public async Task AnalyzerHarness_Should_Report_NestedStringEnum_Diagnostic()
    {
        const string nestedSource = """
            using CSharpEssentials.Enums;

            namespace Sample;

            public class Container
            {
                [StringEnum]
                public enum Color { Red, Green }
            }
            """;
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([nestedSource], [typeof(StringEnumAttribute).Assembly]);

        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EnumsAssembly.Value.Analyzers);

        diagnostics.Should().ContainSingle().Which.Id.Should().Be("CSE0001");
    }

    private static CSharpCompilation CreateCompilation() =>
        GeneratorHarness.CreateCompilation([Source], [typeof(StringEnumAttribute).Assembly]);
}
