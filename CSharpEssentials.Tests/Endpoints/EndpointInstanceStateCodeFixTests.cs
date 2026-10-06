using System.Collections.Immutable;
using CSharpEssentials.Endpoints.CodeFixes;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace CSharpEssentials.Tests.Endpoints;

public class EndpointInstanceStateCodeFixTests
{
    private const string DiagnosticId = "CSE1004";

    private const string Usings = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Api;

        """;

    public static TheoryData<string> UnsafeSources =>
    [
        """
        public sealed class ReadField : IEndpoint
        {
            private readonly int _count = 1;

            public int Count => _count;

            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/field", () => "field");
        }
        """,
        """
        public sealed class UsedParameter(string name) : IEndpoint
        {
            public override string ToString() => name;

            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/parameter", () => "parameter");
        }
        """,
        """
        public sealed class Created : IEndpoint
        {
            public Created(string name) => System.GC.KeepAlive(name);

            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/created", () => "created");
        }

        public static class Factory
        {
            public static object Create() => new Created("name");
        }
        """,
        """
        public sealed record Positional(string Name) : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/record", () => "record");
        }
        """,
    ];

    [Fact]
    public async Task CodeFix_Should_Remove_Fields_Properties_And_Constructors_When_Only_Used_By_Removed_Members()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            public sealed class WithState : IEndpoint
            {
                private readonly string _name;

                public WithState(string name) => _name = name;

                public string Title { get; set; } = "";

                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/state", () => "state");
            }
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "Remove instance state from 'WithState'");

        (await CodeFixHarness.TextAsync(fixedProject, "Source0.cs")).Should().Be(Usings + """
            public sealed class WithState : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/state", () => "state");
            }
            """);
        await ShouldBeFixedAsync(fixedProject);
    }

    [Fact]
    public async Task CodeFix_Should_Remove_Primary_Constructor_When_Parameters_Are_Unused()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            public sealed class WithPrimary(string name) : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/primary", () => "primary");
            }
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "Remove instance state from 'WithPrimary'");

        (await CodeFixHarness.TextAsync(fixedProject, "Source0.cs")).Should().Be(Usings + """
            public sealed class WithPrimary : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/primary", () => "primary");
            }
            """);
        await ShouldBeFixedAsync(fixedProject);
    }

    [Fact]
    public async Task CodeFix_Should_Remove_State_From_Every_Partial_Declaration()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(
            workspace,
            """
            public sealed partial class Split : IEndpoint
            {
                private int _count;

                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/split", () => "split");
            }
            """,
            """
            namespace Sample.Api;

            public sealed partial class Split
            {
                public Split(int count) => _count = count;
            }
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "Remove instance state from 'Split'");

        (await CodeFixHarness.TextAsync(fixedProject, "Source0.cs")).Should().Be(Usings + """
            public sealed partial class Split : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/split", () => "split");
            }
            """);
        (await CodeFixHarness.TextAsync(fixedProject, "Source1.cs")).Should().Be("""
            namespace Sample.Api;

            public sealed partial class Split
            {
            }
            """);
        await ShouldBeFixedAsync(fixedProject);
    }

    [Theory]
    [MemberData(nameof(UnsafeSources))]
    public async Task CodeFix_Should_Not_Be_Offered_When_State_Is_Used_Outside_Removed_Members(string source)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, source);
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, EndpointCompilations.Generators.Value.Analyzers, DiagnosticId);

        IReadOnlyList<CodeAction> actions = await CodeFixHarness.GetActionsAsync(project, diagnostics.Single(), new EndpointInstanceStateCodeFixProvider());

        actions.Should().BeEmpty();
    }

    private static Project CreateProject(AdhocWorkspace workspace, string source, params string[] additionalSources) =>
        CodeFixHarness.CreateProject(
            workspace,
            EndpointCompilations.Create("Sample.Api", OutputKind.DynamicallyLinkedLibrary, [Usings + source, .. additionalSources]));

    private static async Task<Project> ApplySingleFixAsync(Project project, string title)
    {
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, EndpointCompilations.Generators.Value.Analyzers, DiagnosticId);
        IReadOnlyList<CodeAction> actions = await CodeFixHarness.GetActionsAsync(project, diagnostics.Single(), new EndpointInstanceStateCodeFixProvider());
        actions.Select(static action => action.Title).Should().Equal(title);
        return await CodeFixHarness.ApplyAsync(project, actions[0]);
    }

    private static async Task ShouldBeFixedAsync(Project project)
    {
        (await CodeFixHarness.GetDiagnosticsAsync(project, EndpointCompilations.Generators.Value.Analyzers, DiagnosticId)).Should().BeEmpty();
        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
    }
}
