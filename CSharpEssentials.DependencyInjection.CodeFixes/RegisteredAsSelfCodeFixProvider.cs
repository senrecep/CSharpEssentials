using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.DependencyInjection.CodeFixes;

/// <summary>
/// Fixes CSE2003 by passing one of the implemented interfaces as the service type, or by setting <c>As = ServiceAs.Self</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegisteredAsSelfCodeFixProvider))]
[Shared]
public sealed class RegisteredAsSelfCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE2003";

    private const string ServiceAsMetadataName = "CSharpEssentials.DependencyInjection.ServiceAs";

    private static readonly string[] ExcludedInterfaces =
    [
        "System.IDisposable",
        "System.IAsyncDisposable",
        "System.IEquatable`1",
        "System.IComparable`1",
        "System.Collections.IEnumerable",
    ];

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
            root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>() is not { Parent.Parent: TypeDeclarationSyntax declaration } attribute ||
            model.GetDeclaredSymbol(declaration, context.CancellationToken) is not INamedTypeSymbol type)
        {
            return;
        }

        Document document = context.Document;
        int position = attribute.SpanStart;
        foreach (INamedTypeSymbol service in GetServiceInterfaces(type))
        {
            string name = (type.IsGenericType ? service.ConstructUnboundGenericType() : service).ToMinimalDisplayString(model, position);
            AttributeArgumentSyntax argument = SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(name)));
            context.RegisterCodeFix(
                CodeAction.Create(
                    $"Register as '{name}'",
                    _ => Task.FromResult(AddArgument(document, root, attribute, argument, first: true)),
                    DiagnosticId + ":" + name),
                context.Diagnostics);
        }

        if (model.Compilation.GetTypeByMetadataName(ServiceAsMetadataName) is { } serviceAs)
        {
            AttributeArgumentSyntax argument = SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals("As"),
                null,
                SyntaxFactory.ParseExpression(serviceAs.ToMinimalDisplayString(model, position) + ".Self"));
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Register as self (As = ServiceAs.Self)",
                    _ => Task.FromResult(AddArgument(document, root, attribute, argument, first: false)),
                    DiagnosticId + ":Self"),
                context.Diagnostics);
        }
    }

    private static Document AddArgument(Document document, SyntaxNode root, AttributeSyntax attribute, AttributeArgumentSyntax argument, bool first)
    {
        IEnumerable<AttributeArgumentSyntax> existing = attribute.ArgumentList?.Arguments ?? default;
        AttributeArgumentSyntax[] arguments = first ? [argument, .. existing] : [.. existing, argument];
        SyntaxToken separator = SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space);
        AttributeArgumentListSyntax list = SyntaxFactory.AttributeArgumentList(
            SyntaxFactory.SeparatedList(arguments, Enumerable.Repeat(separator, arguments.Length - 1)));
        return document.WithSyntaxRoot(root.ReplaceNode(attribute, attribute.WithArgumentList(list)));
    }

    private static IEnumerable<INamedTypeSymbol> GetServiceInterfaces(INamedTypeSymbol type) =>
        type.AllInterfaces
            .Where(static candidate => !ExcludedInterfaces.Contains(MetadataFullName(candidate.OriginalDefinition), StringComparer.Ordinal))
            .Select(candidate => MapToService(type, candidate))
            .OfType<INamedTypeSymbol>()
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .OrderBy(static candidate => MetadataFullName(candidate), StringComparer.Ordinal)
            .ThenBy(static candidate => candidate.ToDisplayString(), StringComparer.Ordinal);

    private static INamedTypeSymbol? MapToService(INamedTypeSymbol type, INamedTypeSymbol candidate)
    {
        if (!type.IsGenericType)
        {
            return candidate;
        }

        return candidate.IsGenericType &&
            candidate.TypeArguments.SequenceEqual(type.TypeParameters, static (argument, parameter) => SymbolEqualityComparer.Default.Equals(argument, parameter))
                ? candidate.OriginalDefinition
                : null;
    }

    private static string MetadataFullName(INamedTypeSymbol type) =>
        type.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace
            ? containingNamespace.ToDisplayString() + "." + type.MetadataName
            : type.MetadataName;
}
