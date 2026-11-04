using System.Collections.Immutable;
using CSharpEssentials.Json;
using CSharpEssentials.Json.CodeFixes;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

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
}
