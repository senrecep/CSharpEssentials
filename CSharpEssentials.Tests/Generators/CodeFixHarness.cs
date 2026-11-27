using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Tests.Generators;

public static class CodeFixHarness
{
    public static Project CreateProject(AdhocWorkspace workspace, Compilation compilation)
    {
        ProjectInfo info = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Default,
            compilation.AssemblyName!,
            compilation.AssemblyName!,
            LanguageNames.CSharp,
            compilationOptions: compilation.Options,
            parseOptions: compilation.SyntaxTrees.First().Options,
            metadataReferences: compilation.References);
        Project project = workspace.AddProject(info);
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            project = project.AddDocument(tree.FilePath, tree.GetText(), filePath: tree.FilePath).Project;
        }

        return project;
    }

    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Project project, ImmutableArray<DiagnosticAnalyzer> analyzers, string id)
    {
        Compilation compilation = (await project.GetCompilationAsync())!;
        ImmutableArray<Diagnostic> diagnostics = await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
        return [.. diagnostics.Where(diagnostic => diagnostic.Id == id).OrderBy(static diagnostic => diagnostic.Location.SourceSpan.Start)];
    }

    public static async Task<IReadOnlyList<CodeAction>> GetActionsAsync(Project project, Diagnostic diagnostic, CodeFixProvider provider)
    {
        Document document = project.GetDocument(diagnostic.Location.SourceTree)!;
        List<CodeAction> actions = [];
        CodeFixContext context = new(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(context);
        return actions;
    }

    public static async Task<Project> ApplyAsync(Project project, CodeAction action)
    {
        ImmutableArray<CodeActionOperation> operations = await action.GetOperationsAsync(CancellationToken.None);
        Solution solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        return solution.GetProject(project.Id)!;
    }

    public static async Task<string> TextAsync(Project project, string filePath)
    {
        Document document = project.Documents.Single(document => document.FilePath == filePath);
        return (await document.GetTextAsync()).ToString();
    }

    public static async Task<ImmutableArray<Diagnostic>> GetCompilerErrorsAsync(Project project)
    {
        Compilation compilation = (await project.GetCompilationAsync())!;
        return [.. compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
    }

    public static async Task<Project> FixAllAsync(Project project, ImmutableArray<Diagnostic> diagnostics, CodeFixProvider provider)
    {
        Document document = project.GetDocument(diagnostics[0].Location.SourceTree)!;
        CodeAction first = (await GetActionsAsync(project, diagnostics[0], provider))[0];
        FixAllContext context = new(
            document,
            provider,
            FixAllScope.Document,
            first.EquivalenceKey,
            provider.FixableDiagnosticIds,
            new FixedDiagnosticProvider(diagnostics),
            CancellationToken.None);
        CodeAction fixAll = (await provider.GetFixAllProvider()!.GetFixAsync(context))!;
        return await ApplyAsync(project, fixAll);
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
