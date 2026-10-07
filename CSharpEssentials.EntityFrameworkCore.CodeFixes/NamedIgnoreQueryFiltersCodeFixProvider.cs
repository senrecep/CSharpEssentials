using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace CSharpEssentials.EntityFrameworkCore.CodeFixes;

/// <summary>
/// Fixes CSE3001 by passing the soft-delete filter name to <c>IgnoreQueryFilters</c>:
/// <c>IgnoreQueryFilters(new[] { QueryFilterNames.SoftDelete })</c>, or the <c>"SoftDelete"</c> literal when <c>QueryFilterNames</c> is not
/// referenced. Offered only when the queried entity type implements <c>ISoftDeletableBase</c>, so soft delete is the only filter the
/// call can be meant to drop.
/// </summary>
/// <remarks>
/// The fix emits <c>new[] { ... }</c> rather than a collection expression: EF Core 10.0.x compiles the query again on every execution
/// when the keys come from a collection expression or a <c>List</c>, and collection expressions are not allowed inside expression trees
/// (CS9175). The named key ignores only a filter registered under that name, so the title names the helpers that register it.
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NamedIgnoreQueryFiltersCodeFixProvider))]
[Shared]
public sealed class NamedIgnoreQueryFiltersCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE3001";

    private const string QueryFilterNamesMetadataName = "CSharpEssentials.EntityFrameworkCore.QueryFilterNames";

    private const string SoftDeletableMetadataName = "CSharpEssentials.Entity.Interfaces.ISoftDeletableBase";

    private const string SoftDeleteField = "SoftDelete";

    private const string Title =
        "Ignore only the named 'SoftDelete' filter (requires HasSoftDeleteQueryFilter/ApplyNamedSoftDeleteQueryFilter)";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        SemanticModel? model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null || model is null ||
            root.FindNode(context.Span, getInnermostNodeForTie: true).FirstAncestorOrSelf<InvocationExpressionSyntax>() is not { } invocation ||
            model.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol { TypeArguments.Length: 1 } method ||
            !CanAddFilterNames(invocation, method) ||
            !IsSoftDeletable(model.Compilation, method.TypeArguments[0]))
        {
            return;
        }

        ArgumentSyntax argument = SyntaxFactory.Argument(FilterNamesExpression(model.Compilation));
        Document document = context.Document;
        context.RegisterCodeFix(
            CodeAction.Create(
                Title,
                cancellationToken => AddArgumentAsync(document, root, invocation, argument, cancellationToken),
                DiagnosticId + ":" + SoftDeleteField),
            context.Diagnostics);
    }

    private static bool CanAddFilterNames(InvocationExpressionSyntax invocation, IMethodSymbol method) =>
        method.MethodKind == MethodKind.ReducedExtension && invocation.ArgumentList.Arguments.Count == 0 ||
        method.MethodKind == MethodKind.Ordinary && invocation.ArgumentList.Arguments.Count == 1;

    private static bool IsSoftDeletable(Compilation compilation, ITypeSymbol entityType) =>
        compilation.GetTypeByMetadataName(SoftDeletableMetadataName) is { } softDeletable &&
        compilation.ClassifyCommonConversion(entityType, softDeletable).IsImplicit;

    private static ImplicitArrayCreationExpressionSyntax FilterNamesExpression(Compilation compilation)
    {
        ExpressionSyntax filterName = HasQueryFilterNames(compilation)
            ? SyntaxFactory.ParseExpression("global::" + QueryFilterNamesMetadataName + "." + SoftDeleteField)
                .WithAdditionalAnnotations(Simplifier.Annotation, Simplifier.AddImportsAnnotation)
            : SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(SoftDeleteField));
        return SyntaxFactory.ImplicitArrayCreationExpression(
            SyntaxFactory.InitializerExpression(
                SyntaxKind.ArrayInitializerExpression,
                SyntaxFactory.SingletonSeparatedList(filterName)));
    }

    private static bool HasQueryFilterNames(Compilation compilation) =>
        compilation.GetTypeByMetadataName(QueryFilterNamesMetadataName) is { DeclaredAccessibility: Accessibility.Public } names &&
        names.GetMembers(SoftDeleteField).OfType<IFieldSymbol>().Any(static field => field.IsConst);

    private static async Task<Document> AddArgumentAsync(
        Document document,
        SyntaxNode root,
        InvocationExpressionSyntax invocation,
        ArgumentSyntax argument,
        CancellationToken cancellationToken)
    {
        ArgumentSyntax[] arguments = [.. invocation.ArgumentList.Arguments, argument];
        SyntaxToken separator = SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space);
        ArgumentListSyntax list = invocation.ArgumentList.WithArguments(
            SyntaxFactory.SeparatedList(arguments, Enumerable.Repeat(separator, arguments.Length - 1)));
        Document changed = document.WithSyntaxRoot(root.ReplaceNode(invocation, invocation.WithArgumentList(list)));
        changed = await ImportAdder.AddImportsAsync(changed, Simplifier.AddImportsAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);
        changed = await Simplifier.ReduceAsync(changed, Simplifier.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await Formatter.FormatAsync(changed, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
