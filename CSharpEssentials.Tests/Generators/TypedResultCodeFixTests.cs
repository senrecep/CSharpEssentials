using System.Collections.Immutable;
using CSharpEssentials.Errors;
using CSharpEssentials.Results.CodeFixes;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace CSharpEssentials.Tests.Generators;

public class TypedResultCodeFixTests
{
    private const string Header = """
        using System;
        using System.Threading.Tasks;
        using CSharpEssentials.Errors;
        using CSharpEssentials.ResultPattern;

        namespace Sample;

        public sealed record Dto(int Id);
        public sealed record Locked(int Id) : ConflictError("User.Locked", "User is locked.");

        public static class Errs
        {
            public static readonly NotFoundError NotFound = new("User.NotFound", "User not found.");
            public static readonly ConflictError Conflict = new("User.Conflict", "Conflict.");
        }

        """;

    private static readonly Lazy<AnalyzerAssembly> ResultsAssembly = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Results.Generators.dll"));

    [Theory]
    [InlineData("Result<Dto, NotFoundError, NotFoundError>", "Result<Dto, NotFoundError>")]
    [InlineData("Result<Dto, NotFoundError, ConflictError, NotFoundError>", "Result<Dto, NotFoundError, ConflictError>")]
    [InlineData("Result<Dto, NotFoundError, NotFoundError, NotFoundError>", "Result<Dto, NotFoundError>")]
    public async Task CSE5002_CodeFix_Should_Remove_Every_Repeated_Slot(string type, string expected)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, $$"""
            public static class C
            {
                public static {{type}} M() => new Dto(1);
            }
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "CSE5002", new DuplicateSlotCodeFixProvider(), "Remove the repeated error slot");

        (await BodyAsync(fixedProject)).Should().Contain($"public static {expected} M() => new Dto(1);");
        await ShouldBeFixedAsync(fixedProject, "CSE5002");
    }

    [Fact]
    public async Task CSE5002_FixAll_Should_Fix_Every_Type_In_The_Document()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            public static class C
            {
                public static Result<Dto, NotFoundError, NotFoundError> M() => new Dto(1);

                public static Result<Dto, ConflictError, ConflictError> N() => new Dto(2);
            }
            """);
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(project, "CSE5002");

        Project fixedProject = await CodeFixHarness.FixAllAsync(project, diagnostics, new DuplicateSlotCodeFixProvider());

        string body = await BodyAsync(fixedProject);
        body.Should().Contain("Result<Dto, NotFoundError> M()").And.Contain("Result<Dto, ConflictError> N()");
        await ShouldBeFixedAsync(fixedProject, "CSE5002");
    }

    [Fact]
    public async Task CSE5005_CodeFix_Should_Use_The_Kind_Of_The_Base()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            [ErrorKind(ErrorType.Conflict)]
            public sealed record Missing() : NotFoundError("Missing", "Missing.");
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "CSE5005", new ErrorKindCodeFixProvider(), "Change [ErrorKind] to NotFound");

        (await BodyAsync(fixedProject)).Should().Contain("[ErrorKind(ErrorType.NotFound)]\npublic sealed record Missing()");
        await ShouldBeFixedAsync(fixedProject, "CSE5005");
    }

    [Fact]
    public async Task CSE5005_CodeFix_Should_Use_The_Kind_The_Type_Property_Returns()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            [ErrorKind(ErrorType.Conflict)]
            public sealed record Custom() : TypedError("Custom", "Custom.")
            {
                public override ErrorType Type => ErrorType.NotFound;
            }
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "CSE5005", new ErrorKindCodeFixProvider(), "Change [ErrorKind] to NotFound");

        string body = await BodyAsync(fixedProject);
        body.Should().Contain("[ErrorKind(ErrorType.NotFound)]").And.Contain("public override ErrorType Type => ErrorType.NotFound;");
        await ShouldBeFixedAsync(fixedProject, "CSE5005");
    }

    [Fact]
    public async Task CSE5005_CodeFix_Should_Not_Be_Offered_When_The_Attribute_Is_Inherited()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            [ErrorKind(ErrorType.Conflict)]
            public abstract record BaseError() : TypedError("Base", "Base.");

            public sealed record Custom() : BaseError
            {
                public override ErrorType Type => ErrorType.NotFound;
            }
            """);

        IReadOnlyList<CodeAction> actions = await ActionsAsync(project, "CSE5005", new ErrorKindCodeFixProvider());

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5005_CodeFix_Should_Not_Be_Offered_For_The_Base_When_The_Type_Property_Sets_The_Kind()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            [ErrorKind(ErrorType.Conflict)]
            public abstract record BaseError() : TypedError("Base", "Base.")
            {
                public override ErrorType Type => ErrorType.Conflict;
            }

            [ErrorKind(ErrorType.NotFound)]
            public sealed record Custom() : BaseError
            {
                public override ErrorType Type => ErrorType.NotFound;
            }
            """);

        IReadOnlyList<CodeAction> actions = await ActionsAsync(project, "CSE5005", new ErrorKindCodeFixProvider());

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5006_CodeFix_Should_Cast_A_Derived_Error_To_Its_Slot()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            public static class C
            {
                public static object M(Result<Dto, NotFoundError, ConflictError> r) =>
                    r.Ensure(d => d.Id > 0, new Locked(1));
            }
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "CSE5006", new CombinatorSlotCodeFixProvider(), "Cast the error to 'ConflictError'");

        (await BodyAsync(fixedProject)).Should().Contain("r.Ensure(d => d.Id > 0, (ConflictError)new Locked(1));");
        await ShouldBeFixedAsync(fixedProject, "CSE5006");
    }

    [Fact]
    public async Task CSE5006_CodeFix_Should_Drop_Explicit_Type_Arguments_That_Repeat_A_Slot()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            public static class C
            {
                public static object M(Result<Dto, NotFoundError> r) =>
                    r.FailIfAsync<NotFoundError>(d => Task.FromResult(d.Id < 0), Errs.NotFound);
            }
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "CSE5006", new CombinatorSlotCodeFixProvider(), "Remove the explicit type argument");

        (await BodyAsync(fixedProject)).Should().Contain("r.FailIfAsync(d => Task.FromResult(d.Id < 0), Errs.NotFound);");
        await ShouldBeFixedAsync(fixedProject, "CSE5006");
    }

    [Theory]
    [InlineData(
        "public static object M(Result<Dto, NotFoundError> r) => r.BindWiden(d => Name(d));",
        "r.Bind(d => Name(d));")]
    [InlineData(
        "public static object M(Task<Result<Dto, NotFoundError>> r) => r.BindWidenAsync(d => Name(d));",
        "r.BindAsync(d => Name(d));")]
    [InlineData(
        "public static object M(Result<Dto, NotFoundError, ConflictError> r) => r.BindWiden(d => Name(d));",
        "r.Bind<string>(d => Name(d));")]
    public async Task CSE5006_CodeFix_Should_Replace_BindWiden_With_Bind(string member, string expected)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, $$"""
            public static class C
            {
                private static Result<string, NotFoundError> Name(Dto d) => d.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

                {{member}}
            }
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "CSE5006", new CombinatorSlotCodeFixProvider(), "Use " + expected[2..expected.IndexOfAny(['<', '('])] + "<...> and let the inner result widen");

        (await BodyAsync(fixedProject)).Should().Contain(expected);
        await ShouldBeFixedAsync(fixedProject, "CSE5006");
    }

    [Theory]
    [InlineData("public static object M(Result<Dto, NotFoundError, ConflictError> r) => r.Ensure(d => d.Id > 0, (IError)Errs.Conflict);")]
    [InlineData("public static object M(Result<Dto, NotFoundError, ConflictError> r) => r.BindWiden(Name);")]
    [InlineData("public static object M(Result<Dto, InvalidOperationException> r) => r.TryCatch<Dto, Exception>(d => d);")]
    public async Task CSE5006_CodeFix_Should_Not_Be_Offered_When_The_Rewrite_Would_Change_Meaning(string member)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, $$"""
            public static class C
            {
                private static Result<string, NotFoundError> Name(Dto d) => d.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

                {{member}}
            }
            """);

        IReadOnlyList<CodeAction> actions = await ActionsAsync(project, "CSE5006", new CombinatorSlotCodeFixProvider());

        actions.Should().BeEmpty();
    }

    private static Project CreateProject(AdhocWorkspace workspace, string source) =>
        CodeFixHarness.CreateProject(
            workspace,
            GeneratorHarness.CreateCompilation([Header + source], [typeof(Result<int, NotFoundError>).Assembly, typeof(TypedError).Assembly]));

    private static Task<ImmutableArray<Diagnostic>> DiagnosticsAsync(Project project, string id) =>
        CodeFixHarness.GetDiagnosticsAsync(project, ResultsAssembly.Value.Analyzers, id);

    private static async Task<IReadOnlyList<CodeAction>> ActionsAsync(Project project, string id, CodeFixProvider provider)
    {
        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(project, id);
        diagnostics.Should().NotBeEmpty();
        List<CodeAction> actions = [];
        foreach (Diagnostic diagnostic in diagnostics)
            actions.AddRange(await CodeFixHarness.GetActionsAsync(project, diagnostic, provider));

        return actions;
    }

    private static async Task<Project> ApplySingleFixAsync(Project project, string id, CodeFixProvider provider, string title)
    {
        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
        Diagnostic diagnostic = (await DiagnosticsAsync(project, id))[0];
        IReadOnlyList<CodeAction> actions = await CodeFixHarness.GetActionsAsync(project, diagnostic, provider);
        actions.Select(static action => action.Title).Should().Equal(title);
        return await CodeFixHarness.ApplyAsync(project, actions[0]);
    }

    private static async Task<string> BodyAsync(Project project) =>
        (await CodeFixHarness.TextAsync(project, "Source0.cs"))[Header.Length..].ReplaceLineEndings("\n");

    private static async Task ShouldBeFixedAsync(Project project, string id)
    {
        (await DiagnosticsAsync(project, id)).Should().BeEmpty();
        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
    }
}
