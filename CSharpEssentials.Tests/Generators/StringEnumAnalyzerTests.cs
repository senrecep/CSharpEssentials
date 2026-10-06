using System.Collections.Immutable;
using System.Globalization;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Generators;

public class StringEnumAnalyzerTests
{
    private static readonly Lazy<AnalyzerAssembly> EnumsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Enums.Generators.dll"));

    private const string Source = """
        using CSharpEssentials.Enums;

        namespace Sample
        {
            [StringEnum]
            public enum Reachable { A }

            public enum Plain { A }

            public class Order
            {
                [StringEnum]
                internal enum Nested { A }

                [StringEnum]
                private enum Hidden { A }

                [StringEnum]
                protected enum Guarded { A }

                private class Inner
                {
                    [StringEnum]
                    public enum InPrivate { A }
                }
            }

            public class Generic<T>
            {
                [StringEnum]
                public enum InGeneric { A }
            }
        }

        [StringEnum]
        file enum FileLocal { A }
        """;

    [Fact]
    public async Task CSE0015_Should_Report_Only_StringEnums_The_Generator_Cannot_Reach()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(Source, LanguageVersion.Latest);

        diagnostics.Should().OnlyContain(static d => d.Id == "CSE0015" && d.Severity == DiagnosticSeverity.Warning);
        diagnostics.Select(static d => d.GetMessage(CultureInfo.InvariantCulture).Split('\'')[1]).Should().BeEquivalentTo(
            "Sample.Order.Hidden",
            "Sample.Order.Guarded",
            "Sample.Order.Inner.InPrivate",
            "Sample.Generic<T>.InGeneric",
            "FileLocal");
        diagnostics.Should().OnlyContain(static d => d.GetMessage(CultureInfo.InvariantCulture).Contains("throws instead of writing numbers"));
    }

    [Theory]
    [InlineData(LanguageVersion.CSharp7_3)]
    [InlineData(LanguageVersion.CSharp8)]
    public async Task CSE0015_Should_Report_Every_StringEnum_Below_CSharp_9(LanguageVersion languageVersion)
    {
        const string legacySource = """
            using CSharpEssentials.Enums;

            namespace Sample
            {
                [StringEnum]
                public enum OrderStatus { Pending }

                public enum Plain { A }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(legacySource, languageVersion);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0015");
        diagnostic.GetMessage(CultureInfo.InvariantCulture).Should().Contain("Sample.OrderStatus").And.Contain("below 9");
    }

    [Fact]
    public async Task CSE0015_Should_Not_Report_Reachable_StringEnums_In_CSharp_9()
    {
        const string reachable = """
            using CSharpEssentials.Enums;

            namespace Sample;

            [StringEnum]
            public enum OrderStatus { Pending }

            public static class Holder
            {
                [StringEnum]
                internal enum Kind { A }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(reachable, LanguageVersion.CSharp10);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE0016_Should_Report_StringEnums_Whose_Extensions_Classes_Collide()
    {
        const string colliding = """
            using CSharpEssentials.Enums;

            namespace Sample;

            public class Order
            {
                [StringEnum]
                public enum State { A }
            }

            [StringEnum]
            public enum Order_State { B }

            [StringEnum]
            public enum Order_Kind { C }

            public enum Order_Plain { D }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(colliding, LanguageVersion.Latest);

        diagnostics.Should().OnlyContain(static d => d.Id == "CSE0016" && d.Severity == DiagnosticSeverity.Error);
        diagnostics.Select(static d => d.GetMessage(CultureInfo.InvariantCulture)).Should().BeEquivalentTo(
            "'Sample.Order.State' generates the extensions class 'Order_StateExtensions', which 'Sample.Order_State' generates as well; rename one of the enums or its containing type",
            "'Sample.Order_State' generates the extensions class 'Order_StateExtensions', which 'Sample.Order.State' generates as well; rename one of the enums or its containing type");
    }

    [Fact]
    public async Task CSE0016_Should_Not_Report_The_Same_Name_In_Different_Namespaces()
    {
        const string separate = """
            using CSharpEssentials.Enums;

            namespace Orders
            {
                public class Order
                {
                    [StringEnum]
                    public enum State { A }
                }
            }

            namespace Legacy
            {
                [StringEnum]
                public enum Order_State { B }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(separate, LanguageVersion.Latest);

        diagnostics.Should().BeEmpty();
    }

    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, LanguageVersion languageVersion)
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([], [typeof(StringEnumAttribute).Assembly])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(languageVersion), path: "Source.cs"));
        return AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, EnumsAssembly.Value.Analyzers);
    }
}
