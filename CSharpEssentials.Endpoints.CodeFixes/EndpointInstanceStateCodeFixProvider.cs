using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;

namespace CSharpEssentials.Endpoints.CodeFixes;

/// <summary>
/// Fixes CSE1004 by removing the instance fields, auto-properties and constructors of an endpoint type
/// when nothing outside the removed declarations references them.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EndpointInstanceStateCodeFixProvider))]
[Shared]
public sealed class EndpointInstanceStateCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE1004";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        SemanticModel? model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (root?.FindToken(context.Span.Start).Parent is not TypeDeclarationSyntax declaration ||
            model?.GetDeclaredSymbol(declaration, context.CancellationToken) is not INamedTypeSymbol { IsRecord: false } type)
        {
            return;
        }

        Solution solution = context.Document.Project.Solution;
        IReadOnlyList<SyntaxNode>? removals = await FindRemovalsAsync(type, solution, context.CancellationToken).ConfigureAwait(false);
        if (removals is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Remove instance state from '{type.Name}'",
                cancellationToken => RemoveAsync(solution, removals, cancellationToken),
                DiagnosticId),
            context.Diagnostics);
    }

    private static async Task<IReadOnlyList<SyntaxNode>?> FindRemovalsAsync(INamedTypeSymbol type, Solution solution, CancellationToken cancellationToken)
    {
        List<SyntaxNode> removals = [];
        List<ISymbol> removedSymbols = [];
        foreach (ISymbol member in type.GetMembers())
        {
            ISymbol? removed = member switch
            {
                IFieldSymbol { IsStatic: false, IsConst: false } field => field.AssociatedSymbol ?? field,
                IMethodSymbol { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: false, Parameters.Length: > 0 } constructor => constructor,
                _ => null,
            };
            if (removed is null)
            {
                continue;
            }

            if (!TryAddDeclarations(removed, removals, removedSymbols, cancellationToken))
            {
                return null;
            }
        }

        if (removals.Any(node => solution.GetDocumentId(node.SyntaxTree) is null))
        {
            return null;
        }

        foreach (ISymbol symbol in removedSymbols)
        {
            IEnumerable<ReferencedSymbol> references = await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken).ConfigureAwait(false);
            if (references.SelectMany(static reference => reference.Locations).Any(location => !IsInside(location.Location, removals)))
            {
                return null;
            }
        }

        return [.. removals.Distinct().OrderBy(static node => node is ParameterListSyntax)];
    }

    private static bool TryAddDeclarations(ISymbol symbol, List<SyntaxNode> removals, List<ISymbol> removedSymbols, CancellationToken cancellationToken)
    {
        if (symbol is not (IFieldSymbol or IPropertySymbol or IMethodSymbol) || symbol.DeclaringSyntaxReferences.IsEmpty)
        {
            return false;
        }

        removedSymbols.Add(symbol);
        foreach (SyntaxReference reference in symbol.DeclaringSyntaxReferences)
        {
            switch (reference.GetSyntax(cancellationToken))
            {
                case VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax field }:
                    removals.Add(field);
                    break;
                case PropertyDeclarationSyntax property:
                    removals.Add(property);
                    break;
                case ConstructorDeclarationSyntax constructor:
                    removals.Add(constructor);
                    break;
                case TypeDeclarationSyntax { ParameterList: { } parameters } declaration
                    when declaration.BaseList?.Types.Any(static baseType => baseType is PrimaryConstructorBaseTypeSyntax) != true:
                    removals.Add(parameters);
                    removedSymbols.AddRange(((IMethodSymbol)symbol).Parameters);
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    private static bool IsInside(Location location, IReadOnlyList<SyntaxNode> removals) =>
        removals.Any(node => node.SyntaxTree == location.SourceTree && node.FullSpan.Contains(location.SourceSpan));

    private static async Task<Solution> RemoveAsync(Solution solution, IReadOnlyList<SyntaxNode> removals, CancellationToken cancellationToken)
    {
        SolutionEditor editor = new(solution);
        foreach (SyntaxNode node in removals)
        {
            DocumentEditor documentEditor = await editor.GetDocumentEditorAsync(solution.GetDocumentId(node.SyntaxTree), cancellationToken).ConfigureAwait(false);
            if (node is ParameterListSyntax { Parent: TypeDeclarationSyntax declaration })
            {
                documentEditor.ReplaceNode(declaration, static (current, _) => RemoveParameterList((TypeDeclarationSyntax)current));
            }
            else
            {
                documentEditor.RemoveNode(node);
            }
        }

        return editor.GetChangedSolution();
    }

    private static TypeDeclarationSyntax RemoveParameterList(TypeDeclarationSyntax declaration) =>
        declaration
            .WithParameterList(null)
            .WithIdentifier(declaration.Identifier.WithTrailingTrivia(declaration.ParameterList?.GetTrailingTrivia() ?? default));
}
