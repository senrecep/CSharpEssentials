using System.Collections.Immutable;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Tests.Generators;

public class EnumRulesAnalyzerTests
{
    private static readonly Lazy<AnalyzerAssembly> EnumsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Enums.Generators.dll"));

    private const string Usings = """
        using System;
        using System.Runtime.Serialization;
        using System.Text.Json.Serialization;
        using CSharpEssentials.Enums;

        namespace Sample;

        """;

    [Fact]
    public async Task CSE0002_Should_Report_Members_With_The_Same_Wire_Name_Ignoring_Case()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum]
            public enum Status
            {
                InProgress,
                [JsonStringEnumMemberName("IN_PROGRESS")] Running,
            }
            """);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0002");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Be("'Running' produces the wire name 'IN_PROGRESS', which 'InProgress' produces as well");
        SourceText(diagnostic).Should().Be("JsonStringEnumMemberName(\"IN_PROGRESS\")");
    }

    [Fact]
    public async Task CSE0002_Should_Not_Report_Distinct_Wire_Names()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum]
            public enum Status
            {
                InProgress,
                [EnumMember(Value = "running")] Running,
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("\"in_progress\"", "the wire name of 'InProgress'")]
    [InlineData("\"INPROGRESS\"", "the member name 'InProgress'")]
    [InlineData("\"legacy\"", "an alias of 'InProgress'")]
    public async Task CSE0003_Should_Report_Aliases_That_Collide_With_Another_Member(string alias, string detail)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync($$"""
            [StringEnum]
            public enum Status
            {
                [EnumAlias("legacy")] InProgress,
                [EnumAlias({{alias}})] Done,
            }
            """);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle(static d => d.Id == "CSE0003" && d.GetMessage().Contains("of 'Done' equals")).Subject;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Be($"The alias {alias.Replace("\"", "'", StringComparison.Ordinal)} of 'Done' equals {detail}");
        SourceText(diagnostic).Should().StartWith("EnumAlias(");
    }

    [Fact]
    public async Task CSE0003_Should_Not_Report_An_Alias_Of_The_Own_Member()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum]
            public enum Status
            {
                [EnumAlias("InProgress", "started")] InProgress,
                Done,
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE0004_Should_Report_Every_Fallback_When_There_Are_Several()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum]
            public enum Status
            {
                Active,
                [EnumFallback] Unknown,
                [EnumFallback] Other,
            }
            """);

        diagnostics.Should().HaveCount(2).And.OnlyContain(static d => d.Id == "CSE0004" && d.Severity == DiagnosticSeverity.Error);
        diagnostics.Select(static d => d.GetMessage()).Should().BeEquivalentTo(
            "'Unknown' is marked [EnumFallback] as well as 'Other'; keep one fallback member",
            "'Other' is marked [EnumFallback] as well as 'Unknown'; keep one fallback member");
    }

    [Fact]
    public async Task CSE0004_Should_Not_Report_A_Single_Fallback()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum]
            public enum Status
            {
                Active,
                [EnumFallback] Unknown,
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE0005_Should_Report_Integer_Storage_With_Implicit_Values()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum(Storage = EnumStorage.Integer)]
            public enum Priority
            {
                Low,
                Medium = 5,
                High,
            }
            """);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0005");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().Should().Be(
            "'Sample.Priority' is stored as an integer and 'Low', 'High' have no explicit value; reordering or inserting members changes stored data");
        SourceText(diagnostic).Should().Be("Priority");
    }

    [Fact]
    public async Task CSE0005_Should_Report_Flags_With_Default_Storage()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum, Flags]
            public enum Access
            {
                None = 0,
                Read = 1,
                Write,
            }
            """);

        diagnostics.Should().ContainSingle(static d => d.Id == "CSE0005").Which.GetMessage().Should().Contain("'Write' has no explicit value");
    }

    [Theory]
    [InlineData("[StringEnum(Storage = EnumStorage.Integer)] public enum Priority { Low = 0, High = 1 }")]
    [InlineData("[StringEnum] public enum Priority { Low, High }")]
    [InlineData("[StringEnum(Storage = EnumStorage.String), Flags] public enum Access { None = 0, Read = 1, Write = 2, Admin }")]
    public async Task CSE0005_Should_Not_Report_Explicit_Values_Or_String_Storage(string declaration)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(declaration);

        diagnostics.Should().NotContain(static d => d.Id == "CSE0005");
    }

    [Fact]
    public async Task CSE0006_Should_Report_Flags_Without_A_Zero_Member()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum, Flags]
            public enum Access { Read = 1, Write = 2 }
            """);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0006");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().Should().Be("[Flags] enum 'Sample.Access' has no member with the value 0");
        SourceText(diagnostic).Should().Be("Access");
    }

    [Fact]
    public async Task CSE0006_Should_Not_Report_Flags_With_A_Zero_Member_Or_Plain_Enums()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum, Flags]
            public enum Access { Nothing = 0, Read = 1, Write = 2 }

            [StringEnum]
            public enum Status { Active = 1, Closed = 2 }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE0007_Should_Report_Flag_Values_That_No_Other_Members_Cover()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum, Flags]
            public enum Access { None = 0, Read = 1, Write = 2, Odd = 6, ReadWrite = 3 }
            """);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0007");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().Should().Be("[Flags] member 'Odd' is neither a single bit nor a combination of other members");
        SourceText(diagnostic).Should().Be("Odd");
    }

    [Fact]
    public async Task CSE0007_Should_Not_Report_Single_Bits_And_Combinations()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum, Flags]
            public enum Access { None = 0, Read = 1, Write = 2, Delete = 4, ReadWrite = Read | Write, All = ReadWrite | Delete }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE0008_Should_Report_A_Fallback_On_A_Flags_Enum()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum, Flags]
            public enum Access { [EnumFallback] None = 0, Read = 1 }
            """);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0008");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().StartWith("'None' is marked [EnumFallback] but 'Sample.Access' is a [Flags] enum");
        SourceText(diagnostic).Should().Be("EnumFallback");
    }

    [Fact]
    public async Task CSE0008_Should_Not_Report_A_Fallback_On_A_Plain_Enum()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum]
            public enum Status { [EnumFallback] Unknown, Active }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("[JsonStringEnumMemberName(\"\")] A", "wire name", "", "it is empty")]
    [InlineData("[JsonStringEnumMemberName(\"in progress\")] A", "wire name", "in progress", "it contains whitespace")]
    [InlineData("[EnumMember(Value = \"a,b\")] A", "wire name", "a,b", "it contains a comma, which separates flags")]
    [InlineData("[JsonStringEnumMemberName(\"2fa\")] A", "wire name", "2fa", "it starts with a digit, a sign or a dot, so it is read as a number")]
    [InlineData("[EnumAlias(\"-a\")] A", "alias", "-a", "it starts with a digit, a sign or a dot, so it is read as a number")]
    [InlineData("[EnumAlias(\".a\")] A", "alias", ".a", "it starts with a digit, a sign or a dot, so it is read as a number")]
    public async Task CSE0009_Should_Report_Names_The_Parser_Cannot_Read_Back(string member, string kind, string name, string reason)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync($$"""
            [StringEnum]
            public enum Status { {{member}}, B }
            """);

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0009");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Be($"The {kind} '{name}' of 'A' is invalid because {reason}");
    }

    [Fact]
    public async Task CSE0009_Should_Not_Report_Names_With_Digits_After_The_First_Character()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum]
            public enum Version { [JsonStringEnumMemberName("v2")] V2, [EnumAlias("v_3", "_4")] V3 }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE0010_Should_Report_Enums_Without_StringEnum_In_Public_Members_When_Enabled()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
            """
            using System.Collections.Generic;

            public enum Color { Red }
            public enum Size { Small }
            public enum Shape { Round }

            [StringEnum]
            public enum Status { Active }

            public sealed record Order(Color? Color, Status Status);

            public class Catalog
            {
                public IReadOnlyList<Size> Sizes { get; set; } = [];
                public void Paint(Shape[] shapes) { }
                internal void Hidden(Color color) { }
                private Size Secret { get; set; }
            }

            internal class Internal
            {
                public Color Color { get; set; }
            }
            """,
            new Dictionary<string, ReportDiagnostic> { ["CSE0010"] = ReportDiagnostic.Info });

        diagnostics.Should().OnlyContain(static d => d.Id == "CSE0010" && d.Severity == DiagnosticSeverity.Info);
        diagnostics.Select(static d => d.GetMessage().Split('\'')[1]).Should().BeEquivalentTo(
            "Sample.Color",
            "Sample.Size",
            "Sample.Shape");
    }

    [Fact]
    public async Task CSE0010_Should_Be_Disabled_By_Default()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public enum Color { Red }

            public class Paint
            {
                public Color Color { get; set; }
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData("snake")]
    public async Task CSE0012_Should_Report_An_Invalid_Naming_Property(string value)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
            "[StringEnum] public enum Status { Active }",
            options: new Dictionary<string, string> { ["build_property.CSharpEssentialsEnumNaming"] = value });

        Diagnostic diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("CSE0012");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().Should().StartWith($"The CSharpEssentialsEnumNaming value '{value}' is not valid; use one of ")
            .And.Contain("KebabCaseLower")
            .And.EndWith("SnakeCaseLower is used instead.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("kebabcaselower")]
    [InlineData("PascalCase")]
    public async Task CSE0012_Should_Not_Report_A_Valid_Or_Empty_Naming_Property(string value)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
            "[StringEnum] public enum Status { Active }",
            options: new Dictionary<string, string> { ["build_property.CSharpEssentialsEnumNaming"] = value });

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE0013_Should_Report_Enum_Attributes_Without_StringEnum()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public enum Status { [EnumAlias("old")] Active, [EnumFallback] Unknown }
            """);

        diagnostics.Should().HaveCount(2).And.OnlyContain(static d => d.Id == "CSE0013" && d.Severity == DiagnosticSeverity.Warning);
        diagnostics.Select(static d => d.GetMessage()).Should().BeEquivalentTo(
            "[EnumAlias] on 'Active' is ignored because 'Sample.Status' has no [StringEnum]; only the opt-in reflection fallback reads it",
            "[EnumFallback] on 'Unknown' is ignored because 'Sample.Status' has no [StringEnum]; only the opt-in reflection fallback reads it");
    }

    [Fact]
    public async Task CSE0013_Should_Not_Report_Enum_Attributes_With_StringEnum_Or_Other_Attributes()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            [StringEnum]
            public enum Status { [EnumAlias("old")] Active, [EnumFallback] Unknown }

            public enum Plain { [EnumMember(Value = "a")] A, [Obsolete] B }
            """);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task Rules_Should_Use_The_Project_Naming_For_Wire_Names()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
            """
            [StringEnum]
            public enum Status { InProgress, [JsonStringEnumMemberName("in-progress")] Running }
            """,
            options: new Dictionary<string, string> { ["build_property.CSharpEssentialsEnumNaming"] = "KebabCaseLower" });

        diagnostics.Should().ContainSingle().Which.Id.Should().Be("CSE0002");
    }

    [Theory]
    [InlineData("InProgress, [JsonStringEnumMemberName(\"in_progress\")] Running")]
    [InlineData("A, [EnumFallback] B, [EnumFallback] C")]
    [InlineData("[JsonStringEnumMemberName(\"1st\")] First")]
    [InlineData("A, [EnumAlias(\"a\")] B")]
    public void StringEnumGenerator_Should_Skip_Enums_With_Rule_Errors(string members)
    {
        string source = $$"""
            using CSharpEssentials.Enums;
            using System.Text.Json.Serialization;

            namespace Sample;

            [StringEnum]
            public enum Broken { {{members}} }

            [StringEnum]
            public enum Fine { A }
            """;
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([source], [typeof(StringEnumAttribute).Assembly]);

        GeneratorRun run = GeneratorHarness.Run(compilation, [.. EnumsAssembly.Value.Generators]);

        run.GeneratorDiagnostics.Should().BeEmpty();
        run.OutputDiagnostics.Should().NotContain(static d => d.Severity == DiagnosticSeverity.Error);
        run.Result.GeneratedTrees.Select(static t => Path.GetFileName(t.FilePath))
            .Should().BeEquivalentTo("Sample.FineExtensions.g.cs", "__CSharpEssentialsEnumRegistry.g.cs");
        run.Result.GeneratedTrees.Single(static t => t.FilePath.EndsWith("__CSharpEssentialsEnumRegistry.g.cs", StringComparison.Ordinal))
            .ToString().Should().NotContain("Broken");
    }

    [Fact]
    public void StringEnumGenerator_Should_Still_Generate_Enums_With_Rule_Warnings()
    {
        const string source = """
            using System;
            using CSharpEssentials.Enums;

            namespace Sample;

            [StringEnum, Flags]
            public enum Access { Read = 1, Write = 2, Odd = 12 }
            """;
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([source], [typeof(StringEnumAttribute).Assembly]);

        GeneratorRun run = GeneratorHarness.Run(compilation, [.. EnumsAssembly.Value.Generators]);

        run.Result.GeneratedTrees.Select(static t => Path.GetFileName(t.FilePath))
            .Should().BeEquivalentTo("Sample.AccessExtensions.g.cs", "__CSharpEssentialsEnumRegistry.g.cs");
    }

    private static string SourceText(Diagnostic diagnostic) =>
        diagnostic.Location.SourceTree!.ToString().Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length);

    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string declarations,
        IReadOnlyDictionary<string, ReportDiagnostic>? specificOptions = null,
        IReadOnlyDictionary<string, string>? options = null)
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([], [typeof(StringEnumAttribute).Assembly])
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(Usings + declarations, new CSharpParseOptions(LanguageVersion.Latest), path: "Source.cs"));
        if (specificOptions is not null)
        {
            compilation = compilation.WithOptions(compilation.Options.WithSpecificDiagnosticOptions(specificOptions));
        }

        AnalyzerOptions analyzerOptions = new([], new TestAnalyzerConfigOptionsProvider(options ?? new Dictionary<string, string>()));
        return compilation.WithAnalyzers(EnumsAssembly.Value.Analyzers, analyzerOptions).GetAnalyzerDiagnosticsAsync();
    }
}
