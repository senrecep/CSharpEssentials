using System.Collections.Immutable;
using CSharpEssentials.Json;
using CSharpEssentials.Json.CodeFixes;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace CSharpEssentials.Tests.Generators;

public class JsonPolymorphismCodeFixTests
{
    private static readonly Lazy<AnalyzerAssembly> JsonAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Json.Generators.dll"));

    [Fact]
    public async Task CSE4006_CodeFix_Should_Add_The_Missing_JsonSerializable()
    {
        const string source = """
            using System.Text.Json;
            using System.Text.Json.Serialization;
            using System.Text.Json.Serialization.Metadata;
            using CSharpEssentials.Json;

            namespace Sample;

            [JsonPolymorphic]
            public interface IEvent { }

            [JsonTypeDiscriminator("a")]
            public sealed class A : IEvent { }

            [JsonSerializable(typeof(IEvent))]
            public partial class AppContext : JsonSerializerContext
            {
                public AppContext() : base(null) { }
                protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
                public override JsonTypeInfo? GetTypeInfo(System.Type type) => null;
            }
            """;

        Compilation compilation = GeneratorHarness.CreateCompilation([source], [typeof(JsonTypeDiscriminatorAttribute).Assembly]);
        using AdhocWorkspace workspace = new();
        Project project = CodeFixHarness.CreateProject(workspace, compilation);
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, JsonAssembly.Value.Analyzers, "CSE4006");
        IReadOnlyList<CodeAction> actions = await CodeFixHarness.GetActionsAsync(project, diagnostics.Should().ContainSingle().Subject, new AddJsonSerializableCodeFixProvider());

        Project fixedProject = await CodeFixHarness.ApplyAsync(project, actions.Should().ContainSingle().Subject);

        string text = await CodeFixHarness.TextAsync(fixedProject, "Source0.cs");
        text.Should().Contain("[global::System.Text.Json.Serialization.JsonSerializable(typeof(global::Sample.A))]");
        (await CodeFixHarness.GetDiagnosticsAsync(fixedProject, JsonAssembly.Value.Analyzers, "CSE4006")).Should().BeEmpty();
        (await CodeFixHarness.GetCompilerErrorsAsync(fixedProject)).Should().BeEmpty();
    }

    [Fact]
    public void CSE4006_CodeFix_Should_Fix_Only_CSE4006_And_Support_Batch_Fix_All()
    {
        AddJsonSerializableCodeFixProvider provider = new();

        provider.FixableDiagnosticIds.Should().Equal("CSE4006");
        provider.GetFixAllProvider().Should().BeSameAs(WellKnownFixAllProviders.BatchFixer);
    }

    [Fact]
    public async Task CSE4006_CodeFix_Should_Offer_Nothing_When_Diagnostic_Is_Not_Inside_A_Class()
    {
        const string source = """
            namespace Sample;

            public interface IEvent { }

            public partial class AppContext { }
            """;

        Compilation compilation = GeneratorHarness.CreateCompilation([source], [typeof(JsonTypeDiscriminatorAttribute).Assembly]);
        using AdhocWorkspace workspace = new();
        Project project = CodeFixHarness.CreateProject(workspace, compilation);
        SyntaxTree tree = (await project.Documents.Single().GetSyntaxTreeAsync())!;
        DiagnosticDescriptor descriptor = new("CSE4006", "Missing JsonSerializable", "Missing", "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);
        ImmutableDictionary<string, string?> properties = ImmutableDictionary<string, string?>.Empty.Add("DerivedType", "global::Sample.A");
        Diagnostic OnToken(string text) =>
            Diagnostic.Create(descriptor, Location.Create(tree, new TextSpan(source.IndexOf(text, StringComparison.Ordinal), text.Length)), properties);

        IReadOnlyList<CodeAction> outside = await CodeFixHarness.GetActionsAsync(project, OnToken("IEvent"), new AddJsonSerializableCodeFixProvider());
        IReadOnlyList<CodeAction> inside = await CodeFixHarness.GetActionsAsync(project, OnToken("AppContext"), new AddJsonSerializableCodeFixProvider());

        outside.Should().BeEmpty();
        inside.Should().ContainSingle().Which.Title.Should().Be("Add [JsonSerializable(typeof(Sample.A))]");
    }
}
