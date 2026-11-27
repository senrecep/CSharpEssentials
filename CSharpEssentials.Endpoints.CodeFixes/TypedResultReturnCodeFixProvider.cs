using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CSharpEssentials.Endpoints.CodeFixes;

/// <summary>
/// Fixes CSE1013 by calling <c>ToHttpResult()</c> on what a lambda route handler returns, and adds
/// <c>using CSharpEssentials.AspNetCore;</c> when the extension is not in scope. It is offered only when
/// <c>CSharpEssentials.AspNetCore</c> is referenced, and not for method groups or lambdas with an explicit return type.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypedResultReturnCodeFixProvider))]
[Shared]
public sealed class TypedResultReturnCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE1013";
    private const string ExtensionsNamespace = "CSharpEssentials.AspNetCore";
    private const string Extensions = ExtensionsNamespace + ".HttpResultExtensions";
    private const string ToHttpResult = "ToHttpResult";

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
        if (root is null || model?.Compilation.GetTypeByMetadataName(Extensions) is null)
            return;

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            if (Find(root, diagnostic.Location.SourceSpan) is not { } lambda || Targets(lambda, model, context.CancellationToken) is null)
                continue;

            Document document = context.Document;
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Convert with ToHttpResult()",
                    cancellationToken => FixAsync(document, ImmutableArray.Create(diagnostic), cancellationToken),
                    DiagnosticId),
                diagnostic);
        }
    }

    private static async Task<Document> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SemanticModel? model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (root is null || model?.Compilation.GetTypeByMetadataName(Extensions) is null)
            return document;

        ExpressionSyntax[] targets =
        [
            .. diagnostics
                .Select(diagnostic => Find(root, diagnostic.Location.SourceSpan))
                .OfType<LambdaExpressionSyntax>()
                .Distinct()
                .SelectMany(lambda => Targets(lambda, model, cancellationToken) ?? []),
        ];
        if (targets.Length == 0)
            return document;

        bool inScope = targets.All(target => model.LookupSymbols(
            target.SpanStart, model.GetTypeInfo(target, cancellationToken).Type, ToHttpResult, includeReducedExtensionMethods: true).Any());
        SyntaxNode converted = root.ReplaceNodes(targets, static (_, current) => Convert(current));
        return document.WithSyntaxRoot(inScope || converted is not CompilationUnitSyntax unit ? converted : AddUsing(unit));
    }

    private static LambdaExpressionSyntax? Find(SyntaxNode root, TextSpan span) =>
        root.FindNode(span, getInnermostNodeForTie: true) is LambdaExpressionSyntax lambda && lambda.Span == span ? lambda : null;

    private static ExpressionSyntax[]? Targets(LambdaExpressionSyntax lambda, SemanticModel model, CancellationToken cancellationToken)
    {
        if (lambda is ParenthesizedLambdaExpressionSyntax { ReturnType: not null } ||
            model.GetSymbolInfo(lambda, cancellationToken).Symbol is not IMethodSymbol { ReturnType: { } returnType } method)
        {
            return null;
        }

        ITypeSymbol returned = method.IsAsync && returnType is INamedTypeSymbol { TypeArguments.Length: 1 } awaitable ? awaitable.TypeArguments[0] : returnType;
        ExpressionSyntax?[] returns = lambda.ExpressionBody is { } body
            ? [body]
            : [.. lambda.Block?.DescendantNodes(static node => node is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax))
                .OfType<ReturnStatementSyntax>()
                .Select(static statement => statement.Expression) ?? []];
        ExpressionSyntax[] targets = [.. returns.OfType<ExpressionSyntax>()];
        return targets.Length > 0 && targets.Length == returns.Length && targets.All(target =>
            SymbolEqualityComparer.Default.Equals(model.GetTypeInfo(target, cancellationToken).Type, returned))
            ? targets
            : null;
    }

    private static InvocationExpressionSyntax Convert(ExpressionSyntax expression)
    {
        ExpressionSyntax receiver = expression.WithoutTrivia();
        if (receiver is not (InvocationExpressionSyntax or MemberAccessExpressionSyntax or SimpleNameSyntax or ElementAccessExpressionSyntax or
            ParenthesizedExpressionSyntax or ObjectCreationExpressionSyntax))
        {
            receiver = SyntaxFactory.ParenthesizedExpression(receiver);
        }

        return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, receiver, SyntaxFactory.IdentifierName(ToHttpResult)))
            .WithTriviaFrom(expression);
    }

    private static CompilationUnitSyntax AddUsing(CompilationUnitSyntax unit)
    {
        SyntaxList<UsingDirectiveSyntax> usings = unit.Usings;
        UsingDirectiveSyntax directive = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ExtensionsNamespace))
            .WithTrailingTrivia(usings.Count > 0 ? usings[0].GetTrailingTrivia() : SyntaxFactory.TriviaList(SyntaxFactory.LineFeed));
        int index = usings.TakeWhile(static existing =>
            existing.Alias is null && existing.StaticKeyword.IsKind(SyntaxKind.None) && existing.Name is { } name &&
            (name.ToString().StartsWith("System", StringComparison.Ordinal) || string.CompareOrdinal(name.ToString(), ExtensionsNamespace) < 0)).Count();
        if (index == 0 && usings.Count > 0)
        {
            directive = directive.WithLeadingTrivia(usings[0].GetLeadingTrivia());
            usings = usings.Replace(usings[0], usings[0].WithLeadingTrivia());
        }

        return unit.WithUsings(usings.Insert(index, directive));
    }
}
