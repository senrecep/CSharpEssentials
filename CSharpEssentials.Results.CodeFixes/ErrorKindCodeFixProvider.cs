using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Results.CodeFixes;

/// <summary>
/// Fixes CSE5005 by making the <c>[ErrorKind]</c> of the type name the kind its errors already carry at runtime: the kind of the
/// nearest base that declares one, or the constant its own <c>Type</c> property returns. The runtime behavior does not change.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ErrorKindCodeFixProvider))]
[Shared]
public sealed class ErrorKindCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE5005";
    private const string ErrorKindAttribute = "CSharpEssentials.Errors.ErrorKindAttribute";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            if (await PlanAsync(context.Document, diagnostic.Location.SourceSpan, context.CancellationToken).ConfigureAwait(false) is not { } plan)
                continue;

            Solution solution = context.Document.Project.Solution;
            context.RegisterCodeFix(
                CodeAction.Create(
                    $"Change [ErrorKind] to {plan.Kind}",
                    cancellationToken => RenameAsync(solution, plan, cancellationToken),
                    DiagnosticId),
                diagnostic);
        }
    }

    private static async Task<KindPlan?> PlanAsync(Document document, Microsoft.CodeAnalysis.Text.TextSpan span, CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        SemanticModel model = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;
        INamedTypeSymbol attribute = model.Compilation.GetTypeByMetadataName(ErrorKindAttribute)!;
        SyntaxNode node = root.FindNode(span);
        INamedTypeSymbol type;
        object? runtimeKind;
        if (node is ExpressionSyntax expression && node.FirstAncestorOrSelf<PropertyDeclarationSyntax>() is { } property)
        {
            type = model.GetDeclaredSymbol(property, cancellationToken)!.ContainingType;
            Optional<object?> constant = model.GetConstantValue(expression, cancellationToken);
            if (!constant.HasValue || Inherited(type, attribute) is { } baseKind && !Equals(baseKind.Value, constant.Value))
                return null;

            runtimeKind = constant.Value;
        }
        else if (node is BaseTypeDeclarationSyntax declaration)
        {
            type = model.GetDeclaredSymbol(declaration, cancellationToken)!;
            if (Inherited(type, attribute) is not { } inherited || DeclaresKindProperty(type, inherited.Type))
                return null;

            runtimeKind = inherited.Value;
        }
        else
        {
            return null;
        }

        if (Own(type, attribute) is not { ApplicationSyntaxReference: { } reference } own ||
            await reference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) is not AttributeSyntax { ArgumentList.Arguments: { Count: 1 } arguments } ||
            arguments[0].Expression is not MemberAccessExpressionSyntax access ||
            document.Project.GetDocument(reference.SyntaxTree) is not { } attributeDocument)
        {
            return null;
        }

        ITypeSymbol kindType = own.ConstructorArguments[0].Type!;
        SemanticModel attributeModel = (await attributeDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;
        string? kind = kindType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(field => field.HasConstantValue && Equals(field.ConstantValue, runtimeKind))?.Name;
        bool namesKindMember = attributeModel.GetSymbolInfo(access, cancellationToken).Symbol is IFieldSymbol { ContainingType: { } containing } &&
            SymbolEqualityComparer.Default.Equals(containing, kindType);
        return kind is null || !namesKindMember ? null : new KindPlan(attributeDocument.Id, access, kind);
    }

    private static async Task<Solution> RenameAsync(Solution solution, KindPlan plan, CancellationToken cancellationToken)
    {
        Document document = solution.GetDocument(plan.DocumentId)!;
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        MemberAccessExpressionSyntax renamed = plan.Access.WithName(SyntaxFactory.IdentifierName(plan.Kind).WithTriviaFrom(plan.Access.Name));
        return solution.WithDocumentSyntaxRoot(plan.DocumentId, root.ReplaceNode(plan.Access, renamed));
    }

    private static bool DeclaresKindProperty(INamedTypeSymbol type, ITypeSymbol? kindType) =>
        type.GetMembers("Type").OfType<IPropertySymbol>().Any(property => SymbolEqualityComparer.Default.Equals(property.Type, kindType));

    private static AttributeData? Own(INamedTypeSymbol type, INamedTypeSymbol attribute) =>
        type.GetAttributes().FirstOrDefault(data =>
            SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute) && data.ConstructorArguments.Length == 1);

    private static TypedConstant? Inherited(INamedTypeSymbol type, INamedTypeSymbol attribute)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (Own(current, attribute) is { } data)
                return data.ConstructorArguments[0];
        }

        return null;
    }

    private sealed record KindPlan(DocumentId DocumentId, MemberAccessExpressionSyntax Access, string Kind);
}
