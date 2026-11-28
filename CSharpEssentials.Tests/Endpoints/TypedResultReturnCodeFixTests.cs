using System.Collections.Immutable;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Endpoints.CodeFixes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace CSharpEssentials.Tests.Endpoints;

public class TypedResultReturnCodeFixTests
{
    private const string DiagnosticId = "CSE1013";

    private const string Usings = """
        using System.Threading.Tasks;
        using CSharpEssentials.Errors;
        using CSharpEssentials.ResultPattern;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Routing;

        """;

    private const string Declarations = """
        namespace Sample.Api;

        public sealed record OrderDto(int Id);

        public static class Orders
        {
            public static Result<OrderDto, NotFoundError> Get(int id) => new OrderDto(id);

            public static Task<Result<OrderDto, NotFoundError, ConflictError>> GetAsync(int id) =>
                Task.FromResult<Result<OrderDto, NotFoundError, ConflictError>>(new OrderDto(id));
        }

        """;

    [Theory]
    [InlineData("(int id) => Orders.Get(id)", "(int id) => Orders.Get(id).ToHttpResult()")]
    [InlineData("(int id) => Orders.GetAsync(id)", "(int id) => Orders.GetAsync(id).ToHttpResult()")]
    [InlineData("async (int id) => await Orders.GetAsync(id)", "async (int id) => (await Orders.GetAsync(id)).ToHttpResult()")]
    [InlineData(
        "(int id) => { if (id < 0) { return Orders.Get(0); } return Orders.Get(id); }",
        "(int id) => { if (id < 0) { return Orders.Get(0).ToHttpResult(); } return Orders.Get(id).ToHttpResult(); }")]
    public async Task CodeFix_Should_Convert_What_The_Handler_Returns(string handler, string expected)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, "using CSharpEssentials.AspNetCore;\n" + Usings, Api(handler));

        Project fixedProject = await ApplySingleFixAsync(project);

        (await TextAsync(fixedProject)).Should().Be("using CSharpEssentials.AspNetCore;\n" + Usings + Declarations + Api(expected));
        await ShouldBeFixedAsync(fixedProject);
    }

    [Fact]
    public async Task CodeFix_Should_Add_The_Using_When_The_Extension_Is_Not_In_Scope()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, Usings, Api("(int id) => Orders.Get(id)"));

        Project fixedProject = await ApplySingleFixAsync(project);

        (await TextAsync(fixedProject)).Should().StartWith("""
            using System.Threading.Tasks;
            using CSharpEssentials.AspNetCore;
            using CSharpEssentials.Errors;
            """);
        await ShouldBeFixedAsync(fixedProject);
    }

    [Fact]
    public async Task FixAll_Should_Convert_Every_Handler_And_Add_One_Using()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, Usings, """
            public static class Api
            {
                public static void Map(IEndpointRouteBuilder app)
                {
                    app.MapGet("/a/{id}", (int id) => Orders.Get(id));
                    app.MapGet("/b/{id}", (int id) => Orders.GetAsync(id));
                }
            }
            """);
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, EndpointCompilations.Generators.Value.Analyzers, DiagnosticId);

        Project fixedProject = await CodeFixHarness.FixAllAsync(project, diagnostics, new TypedResultReturnCodeFixProvider());

        string text = await TextAsync(fixedProject);
        text.Split("using CSharpEssentials.AspNetCore;").Should().HaveCount(2);
        text.Should().Contain("Orders.Get(id).ToHttpResult()").And.Contain("Orders.GetAsync(id).ToHttpResult()");
        await ShouldBeFixedAsync(fixedProject);
    }

    [Theory]
    [InlineData("Handler")]
    [InlineData("Result<OrderDto, NotFoundError> (int id) => Orders.Get(id)")]
    public async Task CodeFix_Should_Not_Be_Offered_For_Method_Groups_Or_Explicit_Return_Types(string handler)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, Usings, Api(handler));

        IReadOnlyList<CodeAction> actions = await ActionsAsync(project);

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task CodeFix_Should_Not_Be_Offered_Without_CSharpEssentials_AspNetCore()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, Usings, Api("(int id) => Orders.Get(id)"), withAspNetCore: false);

        IReadOnlyList<CodeAction> actions = await ActionsAsync(project);

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task FixAll_Should_Leave_Handlers_With_An_Explicit_Return_Type_Alone()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, Usings, """
            public static class Api
            {
                public static void Map(IEndpointRouteBuilder app)
                {
                    app.MapGet("/a/{id}", (int id) => Orders.Get(id));
                    app.MapGet("/b/{id}", Result<OrderDto, NotFoundError> (int id) => Orders.Get(id));
                }
            }
            """);
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, EndpointCompilations.Generators.Value.Analyzers, DiagnosticId);

        Project fixedProject = await CodeFixHarness.FixAllAsync(project, diagnostics, new TypedResultReturnCodeFixProvider());

        string text = await TextAsync(fixedProject);
        text.Should().Contain("(int id) => Orders.Get(id).ToHttpResult()").And.Contain("Result<OrderDto, NotFoundError> (int id) => Orders.Get(id));");
    }

    [Fact]
    public async Task FixAll_Should_Not_Change_The_Document_When_No_Handler_Can_Be_Converted()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, Usings, Api("Result<OrderDto, NotFoundError> (int id) => Orders.Get(id)"));
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, EndpointCompilations.Generators.Value.Analyzers, DiagnosticId);

        Project fixedProject = await CodeFixHarness.FixAllAsync(project, diagnostics, new TypedResultReturnCodeFixProvider(), DiagnosticId);

        (await TextAsync(fixedProject)).Should().Be(await TextAsync(project));
    }

    [Fact]
    public async Task CodeFix_Should_Ignore_The_Returns_Of_Nested_Lambdas_And_Local_Functions()
    {
        using AdhocWorkspace workspace = new();
        const string handler = "(int id) => { System.Func<int> one = () => { return 1; }; int Two() { return 2; } return Orders.Get(id + one() + Two()); }";
        Project project = CreateProject(workspace, "using CSharpEssentials.AspNetCore;\n" + Usings, Api(handler));

        Project fixedProject = await ApplySingleFixAsync(project);

        (await TextAsync(fixedProject)).Should()
            .Contain("return 1; }; int Two() { return 2; } return Orders.Get(id + one() + Two()).ToHttpResult(); }");
    }

    [Fact]
    public async Task CodeFix_Should_Not_Be_Offered_When_A_Return_Is_Not_The_Result_Itself()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, Usings, Api("(int id) => { if (id < 0) { return new OrderDto(0); } return Orders.Get(id); }"));

        IReadOnlyList<CodeAction> actions = await ActionsAsync(project);

        actions.Should().BeEmpty();
    }

    [Theory]
    [InlineData("// Header\n", "using Microsoft.AspNetCore.Builder;", "using Sys = System;\n")]
    [InlineData("", "using Sys = System;", "using Microsoft.AspNetCore.Builder;\n")]
    [InlineData("", "using static System.Math;", "using Microsoft.AspNetCore.Builder;\n")]
    public async Task CodeFix_Should_Put_The_Using_First_When_No_Using_Sorts_Before_It(string header, string first, string rest)
    {
        using AdhocWorkspace workspace = new();
        const string needed = "using Microsoft.AspNetCore.Routing;\nusing System.Threading.Tasks;\nusing CSharpEssentials.Errors;\nusing CSharpEssentials.ResultPattern;\n";
        string usings = header + first + "\n" + rest + needed;
        Project project = CreateProject(workspace, usings, Api("(int id) => Orders.Get(id)"));

        Project fixedProject = await ApplySingleFixAsync(project);

        (await TextAsync(fixedProject)).Should().StartWith(header + "using CSharpEssentials.AspNetCore;\n" + first + "\n" + rest + needed);
        await ShouldBeFixedAsync(fixedProject);
    }

    private static string Api(string handler) => $$"""
        public static class Api
        {
            private static Result<OrderDto, NotFoundError> Handler(int id) => Orders.Get(id);

            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/orders/{id}", {{handler}});
        }
        """;

    private static Project CreateProject(AdhocWorkspace workspace, string usings, string source, bool withAspNetCore = true)
    {
        Type[] types = withAspNetCore
            ? [typeof(Result<,>), typeof(IError), typeof(HttpResultExtensions), typeof(CSharpEssentials.Core.HttpCodes)]
            : [typeof(Result<,>), typeof(IError), typeof(CSharpEssentials.Core.HttpCodes)];
        MetadataReference[] references = [.. types.Select(static type => (MetadataReference)MetadataReference.CreateFromFile(type.Assembly.Location))];
        return CodeFixHarness.CreateProject(
            workspace,
            EndpointCompilations.Create("Sample.Api", OutputKind.DynamicallyLinkedLibrary, references, usings + Declarations + source));
    }

    private static async Task<IReadOnlyList<CodeAction>> ActionsAsync(Project project)
    {
        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
        Diagnostic diagnostic = (await CodeFixHarness.GetDiagnosticsAsync(project, EndpointCompilations.Generators.Value.Analyzers, DiagnosticId)).Should().ContainSingle().Subject;
        return await CodeFixHarness.GetActionsAsync(project, diagnostic, new TypedResultReturnCodeFixProvider());
    }

    private static async Task<Project> ApplySingleFixAsync(Project project)
    {
        IReadOnlyList<CodeAction> actions = await ActionsAsync(project);
        actions.Select(static action => action.Title).Should().Equal("Convert with ToHttpResult()");
        return await CodeFixHarness.ApplyAsync(project, actions[0]);
    }

    private static async Task<string> TextAsync(Project project) =>
        (await CodeFixHarness.TextAsync(project, "Source0.cs")).ReplaceLineEndings("\n");

    private static async Task ShouldBeFixedAsync(Project project)
    {
        (await CodeFixHarness.GetDiagnosticsAsync(project, EndpointCompilations.Generators.Value.Analyzers, DiagnosticId)).Should().BeEmpty();
        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
    }
}
