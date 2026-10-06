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
}
