using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Enums.CodeFixes;

/// <summary>
/// Fixes CSE0005 by giving every enum member without an explicit value the value it has today, so reordering or
/// inserting members no longer changes stored integers.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExplicitEnumValuesCodeFixProvider))]
[Shared]
public sealed class ExplicitEnumValuesCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE0005";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root?.FindToken(context.Span.Start).Parent is not EnumDeclarationSyntax declaration ||
            !declaration.Members.Any(static member => member.EqualsValue is null))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Add explicit values to '{declaration.Identifier.ValueText}'",
                cancellationToken => AddValuesAsync(context.Document, declaration, cancellationToken),
                DiagnosticId),
            context.Diagnostics);
    }

    private static async Task<Document> AddValuesAsync(Document document, EnumDeclarationSyntax declaration, CancellationToken cancellationToken)
    {
        SemanticModel? model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (model is null || root is null)
            return document;

        EnumDeclarationSyntax updated = declaration.ReplaceNodes(
            declaration.Members.Where(static member => member.EqualsValue is null),
            (original, _) => AddValue(original, model, cancellationToken));
        return document.WithSyntaxRoot(root.ReplaceNode(declaration, updated));
    }

    private static EnumMemberDeclarationSyntax AddValue(EnumMemberDeclarationSyntax member, SemanticModel model, CancellationToken cancellationToken)
    {
        if (model.GetDeclaredSymbol(member, cancellationToken) is not IFieldSymbol { HasConstantValue: true, ConstantValue: IFormattable value })
            return member;

        SyntaxToken identifier = member.Identifier;
        ExpressionSyntax expression = SyntaxFactory.ParseExpression(value.ToString(null, CultureInfo.InvariantCulture))
            .WithTrailingTrivia(identifier.TrailingTrivia);
        EqualsValueClauseSyntax equalsValue = SyntaxFactory.EqualsValueClause(
            SyntaxFactory.Token(SyntaxKind.EqualsToken).WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space),
            expression);
        return member.WithIdentifier(identifier.WithTrailingTrivia()).WithEqualsValue(equalsValue);
    }
}
