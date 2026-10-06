using System.Collections.Immutable;
using CSharpEssentials.Enums;
using CSharpEssentials.Enums.CodeFixes;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace CSharpEssentials.Tests.Generators;

public class EnumCodeFixTests
{
    private static readonly Lazy<AnalyzerAssembly> EnumsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Enums.Generators.dll"));

    private const string Usings = """
        using System;
        using CSharpEssentials.Enums;

        namespace Sample;

        """;

    [Fact]
    public async Task CSE0005_CodeFix_Should_Add_The_Current_Values_And_Keep_Trivia()
    {
        string fixedText = await FixAsync("CSE0005", new ExplicitEnumValuesCodeFixProvider(), """
            [StringEnum(Storage = EnumStorage.Integer)]
            public enum Priority
            {
                /// <summary>Lowest.</summary>
                Low, // first
                Medium = 5,
                High,
                [Obsolete] Legacy
            }
            """);

        fixedText.Should().Be(Usings + """
            [StringEnum(Storage = EnumStorage.Integer)]
            public enum Priority
            {
                /// <summary>Lowest.</summary>
                Low = 0, // first
                Medium = 5,
                High = 6,
                [Obsolete] Legacy = 7
            }
            """);
    }

    [Fact]
    public async Task CSE0005_CodeFix_Should_Write_Negative_And_Unsigned_Values()
    {
        string fixedText = await FixAsync("CSE0005", new ExplicitEnumValuesCodeFixProvider(), """
            [StringEnum(Storage = EnumStorage.Integer)]
            public enum Signed : long { Min = long.MinValue, AfterMin, Minus = -2, MinusOne }

            [StringEnum(Storage = EnumStorage.Integer)]
            public enum Unsigned : ulong { Big = ulong.MaxValue - 1, Max }
            """);

        fixedText.Should().Contain("AfterMin = -9223372036854775807,")
            .And.Contain("MinusOne = -1 }")
            .And.Contain("Max = 18446744073709551615 }");
    }

    [Fact]
    public async Task CSE0005_CodeFix_Should_Fix_Flags_With_Default_Storage()
    {
        string fixedText = await FixAsync("CSE0005", new ExplicitEnumValuesCodeFixProvider(), """
            [StringEnum, Flags]
            public enum Access { None = 0, Read = 1, Write }
            """);

        fixedText.Should().EndWith("public enum Access { None = 0, Read = 1, Write = 2 }");
    }

    [Fact]
    public async Task CSE0006_CodeFix_Should_Insert_None_First_In_A_Multiline_Enum()
    {
        string fixedText = await FixAsync("CSE0006", new FlagsZeroMemberCodeFixProvider(), """
            [StringEnum, Flags]
            public enum Access
            {
                /// <summary>Read.</summary>
                Read = 1,
                Write = 2,
            }
            """);

        fixedText.Should().Be(Usings + """
            [StringEnum, Flags]
            public enum Access
            {
                None = 0,
                /// <summary>Read.</summary>
                Read = 1,
                Write = 2,
            }
            """);
    }

    [Fact]
    public async Task CSE0006_CodeFix_Should_Insert_None_First_In_A_Single_Line_Enum()
    {
        string fixedText = await FixAsync("CSE0006", new FlagsZeroMemberCodeFixProvider(), """
            [StringEnum, Flags]
            public enum Access { Read = 1, Write = 2 }
            """);

        fixedText.Should().EndWith("public enum Access { None = 0, Read = 1, Write = 2 }");
    }

    [Fact]
    public async Task CSE0006_CodeFix_Should_Not_Be_Offered_When_None_Exists()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            [StringEnum, Flags]
            public enum Access { None = 4, Read = 1 }
            """);
        Diagnostic diagnostic = (await CodeFixHarness.GetDiagnosticsAsync(project, EnumsAssembly.Value.Analyzers, "CSE0006")).Should().ContainSingle().Subject;

        IReadOnlyList<CodeAction> actions = await CodeFixHarness.GetActionsAsync(project, diagnostic, new FlagsZeroMemberCodeFixProvider());

        actions.Should().BeEmpty();
    }

    [Fact]
    public void CodeFixes_Should_Fix_Their_Diagnostics_And_Support_Fix_All()
    {
        CodeFixProvider[] providers = [new ExplicitEnumValuesCodeFixProvider(), new FlagsZeroMemberCodeFixProvider()];

        providers.SelectMany(static provider => provider.FixableDiagnosticIds).Should().Equal("CSE0005", "CSE0006");
        providers.Should().OnlyContain(static provider => provider.GetFixAllProvider() == WellKnownFixAllProviders.BatchFixer);
    }

    private static async Task<string> FixAsync(string id, CodeFixProvider provider, string declarations)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, declarations);
        Project fixedProject = project;
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, EnumsAssembly.Value.Analyzers, id);
        diagnostics.Should().NotBeEmpty();
        for (int i = 0; i < diagnostics.Length; i++)
        {
            Diagnostic diagnostic = (await CodeFixHarness.GetDiagnosticsAsync(fixedProject, EnumsAssembly.Value.Analyzers, id))[0];
            CodeAction action = (await CodeFixHarness.GetActionsAsync(fixedProject, diagnostic, provider)).Should().ContainSingle().Subject;
            fixedProject = await CodeFixHarness.ApplyAsync(fixedProject, action);
        }

        (await CodeFixHarness.GetDiagnosticsAsync(fixedProject, EnumsAssembly.Value.Analyzers, id)).Should().BeEmpty();
        (await CodeFixHarness.GetCompilerErrorsAsync(fixedProject)).Should().BeEmpty();
        return await CodeFixHarness.TextAsync(fixedProject, "Source0.cs");
    }

    private static Project CreateProject(AdhocWorkspace workspace, string declarations) =>
        CodeFixHarness.CreateProject(workspace, GeneratorHarness.CreateCompilation([Usings + declarations], [typeof(StringEnumAttribute).Assembly]));
}
