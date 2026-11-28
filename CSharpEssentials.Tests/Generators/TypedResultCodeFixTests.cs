using System.Collections.Immutable;
using CSharpEssentials.Errors;
using CSharpEssentials.Results.CodeFixes;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

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
    public async Task CSE5005_CodeFix_Should_Not_Be_Offered_For_The_Type_Property_When_The_Base_Declares_Another_Kind()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            [ErrorKind(ErrorType.Conflict)]
            public abstract record BaseError() : TypedError("Base", "Base.");

            [ErrorKind(ErrorType.Conflict)]
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

    [Theory]
    [InlineData("public static Result<Dto, NotFoundError> M() => new Dto(1);", "Result<Dto, NotFoundError>")]
    [InlineData("public static Result<Dto, NotFoundError, NotFoundError> M() => new Dto(1);", "Dto")]
    [InlineData("public static Result<Dto, NotFoundError, NotFoundError> M() => new Dto(1);", "public static")]
    public async Task CSE5002_CodeFix_Should_Not_Be_Offered_When_The_Diagnostic_Is_Not_On_A_Repeating_Typed_Result(string member, string marker)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, $"public static class C\n{{\n    {member}\n}}");

        IReadOnlyList<CodeAction> actions = await SyntheticActionsAsync(project, "CSE5002", new DuplicateSlotCodeFixProvider(), marker);

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5002_FixAll_Should_Not_Change_The_Document_When_No_Type_Repeats_A_Slot()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, "public static class C\n{\n    public static Result<Dto, NotFoundError> M() => new Dto(1);\n}");
        ImmutableArray<Diagnostic> diagnostics = [await SyntheticAsync(project, "CSE5002", "Result<Dto, NotFoundError>")];

        Project fixedProject = await CodeFixHarness.FixAllAsync(project, diagnostics, new DuplicateSlotCodeFixProvider(), "CSE5002");

        (await BodyAsync(fixedProject)).Should().Be(await BodyAsync(project));
    }

    [Fact]
    public async Task CSE5005_FixAll_Should_Fix_Every_Type_In_The_Document()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            [ErrorKind(ErrorType.Conflict)]
            public sealed record Missing() : NotFoundError("Missing", "Missing.");

            [ErrorKind(ErrorType.Failure)]
            public sealed record Taken() : ConflictError("Taken", "Taken.");
            """);
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(project, "CSE5005");

        Project fixedProject = await CodeFixHarness.FixAllAsync(project, diagnostics, new ErrorKindCodeFixProvider());

        string body = await BodyAsync(fixedProject);
        body.Should().Contain("[ErrorKind(ErrorType.NotFound)]\npublic sealed record Missing()").And.Contain("[ErrorKind(ErrorType.Conflict)]\npublic sealed record Taken()");
        await ShouldBeFixedAsync(fixedProject, "CSE5005");
    }

    [Fact]
    public async Task CSE5005_CodeFix_Should_Use_The_Kind_Of_The_Nearest_Base_That_Declares_One()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            public abstract record Root() : NotFoundError("Root", "Root.");

            [ErrorKind(ErrorType.NotFound)]
            public abstract record Middle() : Root;

            public abstract record Bare() : Middle;

            [ErrorKind(ErrorType.Conflict)]
            public sealed record Leaf() : Bare;
            """);

        Project fixedProject = await ApplySingleFixAsync(project, "CSE5005", new ErrorKindCodeFixProvider(), "Change [ErrorKind] to NotFound");

        (await BodyAsync(fixedProject)).Should().Contain("[ErrorKind(ErrorType.NotFound)]\npublic sealed record Leaf()");
    }

    [Theory]
    [InlineData("public sealed record Custom() : TypedError(\"Custom\", \"Custom.\") { public override ErrorType Type => ErrorType.NotFound; }", "[ErrorKind(default(ErrorType))]", "ErrorType.NotFound")]
    [InlineData("public sealed record Custom() : TypedError(\"Custom\", \"Custom.\") { public override ErrorType Type => ErrorType.NotFound; }", "[ErrorKind(Kinds.Conflict)]", "ErrorType.NotFound")]
    [InlineData("public sealed record Custom() : TypedError(\"Custom\", \"Custom.\") { public override ErrorType Type => (ErrorType)999; }", "[ErrorKind(ErrorType.Conflict)]", "(ErrorType)999")]
    public async Task CSE5005_CodeFix_Should_Not_Be_Offered_When_The_Attribute_Does_Not_Name_A_Kind_It_Can_Rewrite(string type, string attribute, string marker)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, $$"""
            public static class Kinds
            {
                public const ErrorType Conflict = ErrorType.Conflict;
            }

            {{attribute}}
            {{type}}
            """);

        IReadOnlyList<CodeAction> actions = await SyntheticActionsAsync(project, "CSE5005", new ErrorKindCodeFixProvider(), marker);

        actions.Should().BeEmpty();
    }

    [Theory]
    [InlineData("public int M() => 1 + 2;", "1 + 2")]
    [InlineData("public int M() => 1 + 2;", "public int M()")]
    public async Task CSE5005_CodeFix_Should_Not_Be_Offered_Outside_A_Type_Or_A_Type_Property(string member, string marker)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, $"public sealed class Plain\n{{\n    {member}\n}}");

        IReadOnlyList<CodeAction> actions = await SyntheticActionsAsync(project, "CSE5005", new ErrorKindCodeFixProvider(), marker);

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5005_CodeFix_Should_Not_Be_Offered_When_The_Type_Property_Is_Not_A_Constant()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            [ErrorKind(ErrorType.Conflict)]
            public sealed record Custom() : TypedError("Custom", "Custom.")
            {
                public override ErrorType Type => Pick();

                private static ErrorType Pick() => ErrorType.NotFound;
            }
            """);

        IReadOnlyList<CodeAction> actions = await SyntheticActionsAsync(project, "CSE5005", new ErrorKindCodeFixProvider(), "Pick()");

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task CSE5006_FixAll_Should_Fix_Every_Call_In_The_Document()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, """
            public static class C
            {
                public static object M(Result<Dto, NotFoundError, ConflictError> r) => r.Ensure(d => d.Id > 0, new Locked(1));

                public static object N(Result<Dto, NotFoundError, ConflictError> r) => r.FailIf(d => d.Id < 0, new Locked(2));
            }
            """);
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(project, "CSE5006");

        Project fixedProject = await CodeFixHarness.FixAllAsync(project, diagnostics, new CombinatorSlotCodeFixProvider());

        string body = await BodyAsync(fixedProject);
        body.Should().Contain("r.Ensure(d => d.Id > 0, (ConflictError)new Locked(1))").And.Contain("r.FailIf(d => d.Id < 0, (ConflictError)new Locked(2))");
        await ShouldBeFixedAsync(fixedProject, "CSE5006");
    }

    [Theory]
    [InlineData("Name(d)", "Name(d)")]
    [InlineData("d.Id.ToString()", "d.Id.ToString()")]
    [InlineData("r.Map(x => x.Id)", "r.Map(x => x.Id)")]
    [InlineData("r.Match(x => x.Id, _ => 0)", "r.Match(x => x.Id, _ => 0)")]
    [InlineData("C.Make()", "C.Make()")]
    public async Task CSE5006_CodeFix_Should_Not_Be_Offered_For_A_Call_That_Is_Not_A_Slot_Combinator(string call, string marker)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, $$"""
            public static class C
            {
                private static Result<string, NotFoundError> Name(Dto d) => d.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

                public static Result<Dto, NotFoundError> Make() => new Dto(1);

                public static object M(Result<Dto, NotFoundError> r)
                {
                    Dto d = new(1);
                    return {{call}};
                }
            }
            """);

        IReadOnlyList<CodeAction> actions = await SyntheticActionsAsync(project, "CSE5006", new CombinatorSlotCodeFixProvider(), marker);

        actions.Should().BeEmpty();
    }

    private static async Task<Diagnostic> SyntheticAsync(Project project, string id, string marker)
    {
        SyntaxTree tree = (await project.Documents.Single().GetSyntaxTreeAsync())!;
        int start = tree.GetText().ToString().IndexOf(marker, Header.Length, StringComparison.Ordinal);
        start.Should().BeGreaterThan(0, "the marker must appear in the sample source");
        DiagnosticDescriptor descriptor = new(id, "Sample", "Sample", "Sample", DiagnosticSeverity.Warning, isEnabledByDefault: true);
        return Diagnostic.Create(descriptor, Location.Create(tree, new TextSpan(start, marker.Length)));
    }

    private static async Task<IReadOnlyList<CodeAction>> SyntheticActionsAsync(Project project, string id, CodeFixProvider provider, string marker)
    {
        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
        return await CodeFixHarness.GetActionsAsync(project, await SyntheticAsync(project, id, marker), provider);
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
