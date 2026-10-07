using System.Collections.Immutable;
using CSharpEssentials.EntityFrameworkCore.CodeFixes;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.EntityFrameworkCore.QueryFilters;

public sealed class NamedIgnoreQueryFiltersCodeFixTests
{
    private const string SoftDeletableSource = """
        using System.Linq;
        using CSharpEssentials.EntityFrameworkCore;
        using CSharpEssentials.Entity.Interfaces;
        using Microsoft.EntityFrameworkCore;

        namespace Sample;

        public sealed class Document : ISoftDeletableBase
        {
            public bool IsDeleted { get; set; }
        }

        public static class Queries
        {
            public static IQueryable<Document> Run(IQueryable<Document> query) => query.IgnoreQueryFilters();
        }
        """;

    private const string NamesImport = "using CSharpEssentials.EntityFrameworkCore;\n";

    [Fact]
    public async Task CodeFix_Should_Pass_QueryFilterNames_SoftDelete_In_Array_Creation_When_QueryFilterNames_Is_Referenced()
    {
        Project project = await FixAsync(SoftDeletableSource, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);

        (await CodeFixHarness.TextAsync(project, "Source0.cs")).Should()
            .Contain("query.IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete });");
    }

    [Fact]
    public async Task CodeFix_Should_Add_Using_And_Simplify_Name_When_Namespace_Is_Not_Imported()
    {
        string source = SoftDeletableSource.Replace(NamesImport, string.Empty, StringComparison.Ordinal);

        Project project = await FixAsync(source, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);

        string text = await CodeFixHarness.TextAsync(project, "Source0.cs");
        text.Should().Contain("using CSharpEssentials.EntityFrameworkCore;")
            .And.Contain("query.IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete });");
    }

    [Fact]
    public async Task CodeFix_Should_Pass_String_Literal_When_QueryFilterNames_Is_Not_Referenced()
    {
        string source = SoftDeletableSource.Replace(NamesImport, string.Empty, StringComparison.Ordinal);

        Project project = await FixAsync(source, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference);

        (await CodeFixHarness.TextAsync(project, "Source0.cs")).Should().Contain("query.IgnoreQueryFilters(new[] { \"SoftDelete\" });");
    }

    [Fact]
    public async Task CodeFix_Should_Append_Filter_Names_When_Called_As_Static_Method()
    {
        string source = SoftDeletableSource.Replace(
            "query.IgnoreQueryFilters()", "EntityFrameworkQueryableExtensions.IgnoreQueryFilters(query)", StringComparison.Ordinal);

        Project project = await FixAsync(source, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);

        (await CodeFixHarness.TextAsync(project, "Source0.cs")).Should()
            .Contain("EntityFrameworkQueryableExtensions.IgnoreQueryFilters(query, new[] { QueryFilterNames.SoftDelete });");
    }

    [Fact]
    public async Task CodeFix_Should_Compile_When_Language_Version_Predates_Collection_Expressions()
    {
        Project project = await FixAsync(SoftDeletableSource, LanguageVersion.CSharp11, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);

        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
    }

    [Fact]
    public async Task CodeFix_Should_Compile_When_Call_Is_Inside_Expression_Tree()
    {
        string source = SoftDeletableSource
            .Replace("using System.Linq;\n", "using System;\nusing System.Linq;\nusing System.Linq.Expressions;\n", StringComparison.Ordinal)
            .Replace(
                "public static IQueryable<Document> Run(IQueryable<Document> query) => query.IgnoreQueryFilters();",
                "public static Expression<Func<IQueryable<Document>, IQueryable<Document>>> Run() => query => query.IgnoreQueryFilters();",
                StringComparison.Ordinal);

        Project project = await FixAsync(source, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);

        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
        (await CodeFixHarness.TextAsync(project, "Source0.cs")).Should()
            .Contain("query => query.IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete });");
    }

    [Fact]
    public async Task CodeFix_Should_Compile_When_Call_Is_Inside_Subquery()
    {
        string source = SoftDeletableSource.Replace(
            "public static IQueryable<Document> Run(IQueryable<Document> query) => query.IgnoreQueryFilters();",
            "public static IQueryable<Document> Run(IQueryable<Document> query, IQueryable<Document> others) => " +
            "query.Where(document => others.IgnoreQueryFilters().Any(other => other.IsDeleted == document.IsDeleted));",
            StringComparison.Ordinal);

        Project project = await FixAsync(source, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);

        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
        (await CodeFixHarness.TextAsync(project, "Source0.cs")).Should()
            .Contain("others.IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete }).Any(");
    }

    [Fact]
    public async Task FixAll_Should_Fix_Every_Call_In_Document()
    {
        string source = SoftDeletableSource.Replace(
            "public static IQueryable<Document> Run(IQueryable<Document> query) => query.IgnoreQueryFilters();",
            """
            public static IQueryable<Document> Run(IQueryable<Document> query) => query.IgnoreQueryFilters();

                public static IQueryable<Document> RunStatic(IQueryable<Document> query) => EntityFrameworkQueryableExtensions.IgnoreQueryFilters(query);

                public static IQueryable<Document> RunNested(IQueryable<Document> query, IQueryable<Document> others) =>
                    query.IgnoreQueryFilters().Where(document => others.IgnoreQueryFilters().Any());
            """,
            StringComparison.Ordinal);
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, source, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, QueryFilterAnalysis.Analyzers, QueryFilterAnalysis.DiagnosticId);

        Project fixedProject = await FixAllAsync(project, diagnostics);

        diagnostics.Should().HaveCount(4);
        (await CodeFixHarness.GetCompilerErrorsAsync(fixedProject)).Should().BeEmpty();
        (await CodeFixHarness.GetDiagnosticsAsync(fixedProject, QueryFilterAnalysis.Analyzers, QueryFilterAnalysis.DiagnosticId)).Should().BeEmpty();
    }

    [Fact]
    public async Task CodeFix_Should_Name_The_Helpers_That_Register_The_Named_Filter()
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, SoftDeletableSource, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);
        Diagnostic diagnostic = await SingleDiagnosticAsync(project);

        IReadOnlyList<CodeAction> actions = await CodeFixHarness.GetActionsAsync(project, diagnostic, new NamedIgnoreQueryFiltersCodeFixProvider());

        actions.Should().ContainSingle().Which.Title.Should()
            .Be("Ignore only the named 'SoftDelete' filter (requires HasSoftDeleteQueryFilter/ApplyNamedSoftDeleteQueryFilter)");
    }

    [Fact]
    public async Task CodeFix_Should_Clear_CSE3001_Without_Compiler_Errors()
    {
        Project project = await FixAsync(SoftDeletableSource, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);

        (await CodeFixHarness.GetCompilerErrorsAsync(project)).Should().BeEmpty();
        (await CodeFixHarness.GetDiagnosticsAsync(project, QueryFilterAnalysis.Analyzers, QueryFilterAnalysis.DiagnosticId)).Should().BeEmpty();
    }

    [Fact]
    public async Task CodeFix_Should_Not_Be_Offered_When_Entity_Is_Not_Soft_Deletable()
    {
        string source = SoftDeletableSource.Replace(" : ISoftDeletableBase", string.Empty, StringComparison.Ordinal);
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, source, LanguageVersion.Latest, QueryFilterAnalysis.EfCore10Reference, QueryFilterAnalysis.QueryFilterNamesReference);
        Diagnostic diagnostic = await SingleDiagnosticAsync(project);

        IReadOnlyList<CodeAction> actions = await CodeFixHarness.GetActionsAsync(project, diagnostic, new NamedIgnoreQueryFiltersCodeFixProvider());

        actions.Should().BeEmpty();
    }

    private static async Task<Project> FixAsync(string source, LanguageVersion languageVersion, params MetadataReference[] references)
    {
        using AdhocWorkspace workspace = new();
        Project project = CreateProject(workspace, source, languageVersion, references);
        Diagnostic diagnostic = await SingleDiagnosticAsync(project);
        CodeAction action = (await CodeFixHarness.GetActionsAsync(project, diagnostic, new NamedIgnoreQueryFiltersCodeFixProvider())).Should().ContainSingle().Subject;
        return await CodeFixHarness.ApplyAsync(project, action);
    }

    private static async Task<Project> FixAllAsync(Project project, ImmutableArray<Diagnostic> diagnostics)
    {
        NamedIgnoreQueryFiltersCodeFixProvider provider = new();
        Document document = project.GetDocument(diagnostics[0].Location.SourceTree)!;
        CodeAction firstAction = (await CodeFixHarness.GetActionsAsync(project, diagnostics[0], provider)).Should().ContainSingle().Subject;
        FixAllContext context = new(
            document,
            provider,
            FixAllScope.Document,
            firstAction.EquivalenceKey,
            [QueryFilterAnalysis.DiagnosticId],
            new FixedDiagnosticProvider(diagnostics),
            CancellationToken.None);

        CodeAction fixAll = (await provider.GetFixAllProvider().GetFixAsync(context))!;
        return await CodeFixHarness.ApplyAsync(project, fixAll);
    }

    private static Project CreateProject(AdhocWorkspace workspace, string source, LanguageVersion languageVersion, params MetadataReference[] references) =>
        CodeFixHarness.CreateProject(workspace, QueryFilterAnalysis.CreateCompilation(source, languageVersion, references));

    private static async Task<Diagnostic> SingleDiagnosticAsync(Project project)
    {
        ImmutableArray<Diagnostic> diagnostics = await CodeFixHarness.GetDiagnosticsAsync(project, QueryFilterAnalysis.Analyzers, QueryFilterAnalysis.DiagnosticId);
        return diagnostics.Should().ContainSingle().Subject;
    }

    private sealed class FixedDiagnosticProvider(ImmutableArray<Diagnostic> diagnostics) : FixAllContext.DiagnosticProvider
    {
        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<Diagnostic>>([]);

        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
    }
}
