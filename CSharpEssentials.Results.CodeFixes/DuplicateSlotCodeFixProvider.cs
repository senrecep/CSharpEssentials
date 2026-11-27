using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Results.CodeFixes;

/// <summary>
/// Fixes CSE5002 by removing every error slot of a typed result type that repeats an earlier one, keeping the first spelling.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DuplicateSlotCodeFixProvider))]
[Shared]
public sealed class DuplicateSlotCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE5002";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() =>
        FixAllProvider.Create(static async (context, document, diagnostics) => await FixAsync(document, diagnostics, context.CancellationToken).ConfigureAwait(false));

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        SemanticModel? model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null || model is null || Find(root, context.Span) is not { } name || KeptIndexes(name, model, context.CancellationToken) is null)
            return;

        Document document = context.Document;
        context.RegisterCodeFix(
            CodeAction.Create(
                "Remove the repeated error slot",
                cancellationToken => FixAsync(document, context.Diagnostics, cancellationToken),
                DiagnosticId),
            context.Diagnostics);
    }

    private static async Task<Document> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SemanticModel? model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (root is null || model is null)
            return document;

        Dictionary<GenericNameSyntax, int[]> kept = [];
        foreach (Diagnostic diagnostic in diagnostics)
        {
            if (Find(root, diagnostic.Location.SourceSpan) is { } name && !kept.ContainsKey(name) && KeptIndexes(name, model, cancellationToken) is { } indexes)
                kept.Add(name, indexes);
        }

        return kept.Count == 0
            ? document
            : document.WithSyntaxRoot(root.ReplaceNodes(kept.Keys, (original, current) => Keep(current, kept[original])));
    }

    private static GenericNameSyntax? Find(SyntaxNode root, Microsoft.CodeAnalysis.Text.TextSpan span) =>
        root.FindNode(span, getInnermostNodeForTie: true).FirstAncestorOrSelf<GenericNameSyntax>() is { } name && name.Span == span ? name : null;

    private static int[]? KeptIndexes(GenericNameSyntax name, SemanticModel model, CancellationToken cancellationToken)
    {
        SeparatedSyntaxList<TypeSyntax> arguments = name.TypeArgumentList.Arguments;
        List<ITypeSymbol> slots = [];
        List<int> kept = [0];
        for (int index = 1; index < arguments.Count; index++)
        {
            if (model.GetTypeInfo(arguments[index], cancellationToken).Type is not { } slot)
                return null;
            if (slots.Contains(slot, SymbolEqualityComparer.Default))
                continue;

            slots.Add(slot);
            kept.Add(index);
        }

        return kept.Count == arguments.Count ? null : [.. kept];
    }

    private static GenericNameSyntax Keep(GenericNameSyntax name, int[] indexes)
    {
        SeparatedSyntaxList<TypeSyntax> arguments = name.TypeArgumentList.Arguments;
        SyntaxToken comma = SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space);
        return name.WithTypeArgumentList(name.TypeArgumentList.WithArguments(SyntaxFactory.SeparatedList(
            indexes.Select(index => arguments[index]),
            Enumerable.Repeat(comma, indexes.Length - 1))));
    }
}
