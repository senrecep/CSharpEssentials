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

    [Theory]
    [InlineData(LanguageVersion.CSharp7_3, NullableContextOptions.Disable)]
    [InlineData(LanguageVersion.CSharp8, NullableContextOptions.Enable)]
    public void StringEnumGenerator_Should_Emit_Code_Compatible_With_Older_Language_Versions(
        LanguageVersion languageVersion,
        NullableContextOptions nullableOptions)
    {
        const string legacySource = """
            using CSharpEssentials.Enums;

            namespace Sample
            {
                [StringEnum]
                public enum OrderStatus
                {
                    Pending,
                    InProgress,
                    Shipped
                }
            }
            """;
        CSharpParseOptions parseOptions = new(languageVersion);
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([], [typeof(StringEnumAttribute).Assembly]);
        compilation = compilation
            .WithOptions(compilation.Options.WithNullableContextOptions(nullableOptions))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(legacySource, parseOptions, path: "Legacy.cs"));

        GeneratorDriver driver = CSharpGeneratorDriver.Create([.. EnumsAssembly.Value.Generators], parseOptions: parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> generatorDiagnostics);

        driver.GetRunResult().GeneratedTrees.Should().ContainSingle();
        generatorDiagnostics.Should().BeEmpty();
        output.GetDiagnostics().Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void StringEnumGenerator_Should_Return_Cached_Outputs_When_Compilation_Is_Identical()
    {
        CSharpCompilation compilation = CreateCompilation();

        GeneratorDriverRunResult second = IncrementalCaching.RunTwice(compilation, static c => c, [.. EnumsAssembly.Value.Generators]);

        IncrementalCaching.ShouldHaveCachedSourceOutputs(second);
    }

    [Fact]
    public void StringEnumGenerator_Should_Generate_SameNamedEnums_In_DifferentNamespaces()
    {
        const string first = """
            using CSharpEssentials.Enums;

            namespace Orders;

            [StringEnum]
            public enum Status { Open, Closed }
            """;
        const string second = """
            using CSharpEssentials.Enums;

            namespace Users;

            [StringEnum]
            public enum Status { Active, Suspended }
            """;
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([first, second], [typeof(StringEnumAttribute).Assembly]);

        GeneratorRun run = GeneratorHarness.Run(compilation, [.. EnumsAssembly.Value.Generators]);

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        run.Result.GeneratedTrees.Select(static t => Path.GetFileName(t.FilePath))
            .Should().BeEquivalentTo("Orders.StatusExtensions.g.cs", "Users.StatusExtensions.g.cs");
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
