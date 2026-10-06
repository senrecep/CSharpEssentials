using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Enums.CodeFixes;

/// <summary>
/// Fixes CSE0006 by inserting <c>None = 0</c> as the first member of a <c>[Flags]</c> enum. Not offered when the enum
/// already has a member named <c>None</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FlagsZeroMemberCodeFixProvider))]
[Shared]
public sealed class FlagsZeroMemberCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE0006";
    private const string MemberName = "None";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root?.FindToken(context.Span.Start).Parent is not EnumDeclarationSyntax declaration ||
            declaration.Members.Any(static member => member.Identifier.ValueText == MemberName))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Add '{MemberName} = 0' to '{declaration.Identifier.ValueText}'",
                _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(declaration, AddNone(declaration)))),
                DiagnosticId),
            context.Diagnostics);
    }

    private static EnumDeclarationSyntax AddNone(EnumDeclarationSyntax declaration)
    {
        EnumMemberDeclarationSyntax none = SyntaxFactory.EnumMemberDeclaration(MemberName)
            .WithEqualsValue(SyntaxFactory.EqualsValueClause(
                SyntaxFactory.Token(SyntaxKind.EqualsToken).WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))));
        if (declaration.Members.Count == 0)
            return declaration.WithMembers(SyntaxFactory.SingletonSeparatedList(none));

        EnumMemberDeclarationSyntax first = declaration.Members[0];
        SyntaxTrivia? endOfLine = declaration.OpenBraceToken.TrailingTrivia.Concat(first.GetLeadingTrivia())
            .Select(static trivia => (SyntaxTrivia?)trivia)
            .FirstOrDefault(static trivia => trivia!.Value.IsKind(SyntaxKind.EndOfLineTrivia));
        SyntaxTriviaList indentation = SyntaxFactory.TriviaList(
            first.GetLeadingTrivia().Reverse().TakeWhile(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)).Reverse());
        SyntaxToken separator = SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(endOfLine ?? SyntaxFactory.Space);

        List<SyntaxNodeOrToken> items = [none.WithLeadingTrivia(indentation), separator, .. declaration.Members.GetWithSeparators()];
        return declaration.WithMembers(SyntaxFactory.SeparatedList<EnumMemberDeclarationSyntax>(items));
    }
}
