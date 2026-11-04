using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpEssentials.Json.CodeFixes;

/// <summary>
/// Fixes CSE4006 by adding <c>[JsonSerializable(typeof(Derived))]</c> to the <c>JsonSerializerContext</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddJsonSerializableCodeFixProvider))]
[Shared]
public sealed class AddJsonSerializableCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE4006";
    private const string DerivedTypeProperty = "DerivedType";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root?.FindToken(context.Span.Start).Parent?.FirstAncestorOrSelf<ClassDeclarationSyntax>() is not { } declaration)
            return;

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            if (!diagnostic.Properties.TryGetValue(DerivedTypeProperty, out string? derivedType) || derivedType is null)
                continue;

            context.RegisterCodeFix(
                CodeAction.Create(
                    $"Add [JsonSerializable(typeof({derivedType.Replace("global::", string.Empty)}))]",
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(declaration, AddAttribute(declaration, derivedType)))),
                    DiagnosticId + derivedType),
                diagnostic);
        }
    }

    private static ClassDeclarationSyntax AddAttribute(ClassDeclarationSyntax declaration, string derivedType)
    {
        AttributeSyntax attribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("global::System.Text.Json.Serialization.JsonSerializable"),
            SyntaxFactory.ParseAttributeArgumentList("(typeof(" + derivedType + "))"));
        AttributeListSyntax list = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
            .WithAdditionalAnnotations(Formatter.Annotation);
        return declaration.AddAttributeLists(list).WithAdditionalAnnotations(Formatter.Annotation);
    }
}
