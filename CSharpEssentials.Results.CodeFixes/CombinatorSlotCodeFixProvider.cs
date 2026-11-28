using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;

namespace CSharpEssentials.Results.CodeFixes;

/// <summary>
/// Fixes CSE5006 so the combinator keeps the error set of its receiver. <c>Ensure</c> and <c>FailIf</c> (and their async forms)
/// get their error cast to the one slot it converts to, which picks the overload that keeps the slots. <c>BindWiden</c> and
/// <c>BindWidenAsync</c> become <c>Bind</c> and <c>BindAsync</c>, so the inner result widens into the existing slots. The fix is
/// offered only when the rewritten call binds to an overload that returns the receiver's error types.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CombinatorSlotCodeFixProvider))]
[Shared]
public sealed class CombinatorSlotCodeFixProvider : CodeFixProvider
{
    private const string DiagnosticId = "CSE5006";
    private const string ResultNamespace = "CSharpEssentials.ResultPattern";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
        SemanticModel model = (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false))!;

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is not InvocationExpressionSyntax invocation ||
                invocation.Span != diagnostic.Location.SourceSpan ||
                Rewrite(invocation, model, context.CancellationToken) is not { } rewrite)
            {
                continue;
            }

            Document document = context.Document;
            context.RegisterCodeFix(
                CodeAction.Create(
                    rewrite.Title,
                    _ => Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(invocation, rewrite.Replacement))),
                    DiagnosticId),
                diagnostic);
        }
    }

    private static Rewritten? Rewrite(InvocationExpressionSyntax invocation, SemanticModel model, CancellationToken cancellationToken)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax access ||
            model.GetOperation(invocation, cancellationToken) is not IInvocationOperation operation ||
            Unwrap(Receiver(operation)) is not { } receiver ||
            Unwrap(operation.TargetMethod.ReturnType) is not { } result)
        {
            return null;
        }

        Rewritten? rewritten = operation.TargetMethod.Name switch
        {
            "Ensure" or "FailIf" or "EnsureAsync" or "FailIfAsync" => CastError(invocation, access, operation, receiver, model),
            "BindWiden" or "BindWidenAsync" => UseBind(invocation, access, operation, receiver, result),
            _ => null,
        };
        return rewritten is not null && KeepsSlots(model, invocation, rewritten.Replacement, receiver, result, cancellationToken) ? rewritten : null;
    }

    private static Rewritten? CastError(
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax access,
        IInvocationOperation operation,
        INamedTypeSymbol receiver,
        SemanticModel model)
    {
        if (operation.Arguments.FirstOrDefault(static argument => argument.Parameter?.Name == "error") is not { Syntax: ArgumentSyntax argument, Value.Type: { } error })
            return null;

        ITypeSymbol[] slots = [.. receiver.TypeArguments.Skip(1).Where(slot => Converts(model.Compilation, error, slot)).Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)];
        if (slots.Length != 1)
            return null;

        ITypeSymbol slot = slots[0];
        bool exact = SymbolEqualityComparer.Default.Equals(error, slot);
        ArgumentSyntax replaced = exact ? argument : argument.WithExpression(Cast(slot, argument.Expression));
        InvocationExpressionSyntax rewritten = invocation
            .WithArgumentList(invocation.ArgumentList.ReplaceNode(argument, replaced))
            .WithExpression(access.WithName(SyntaxFactory.IdentifierName(access.Name.Identifier)));
        return new Rewritten(
            exact ? "Remove the explicit type argument" : $"Cast the error to '{slot.ToMinimalDisplayString(model, invocation.SpanStart)}'",
            rewritten);
    }

    private static Rewritten UseBind(
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax access,
        IInvocationOperation operation,
        INamedTypeSymbol receiver,
        INamedTypeSymbol result)
    {
        ITypeSymbol success = result.TypeArguments[0];
        IEnumerable<ITypeSymbol> typeArguments = !operation.TargetMethod.IsExtensionMethod ? [success] : [.. receiver.TypeArguments, success];
        string name = operation.TargetMethod.Name == "BindWiden" ? "Bind" : "BindAsync";
        GenericNameSyntax bind = SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(name),
            SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments.Select(TypeName))))
            .WithTriviaFrom(access.Name)
            .WithAdditionalAnnotations(Simplifier.Annotation);
        return new Rewritten($"Use {name}<...> and let the inner result widen", invocation.WithExpression(access.WithName(bind)));
    }

    private static bool KeepsSlots(
        SemanticModel model,
        InvocationExpressionSyntax original,
        InvocationExpressionSyntax rewritten,
        INamedTypeSymbol receiver,
        INamedTypeSymbol result,
        CancellationToken cancellationToken)
    {
        if (model.GetSpeculativeSymbolInfo(original.SpanStart, rewritten, SpeculativeBindingOption.BindAsExpression).Symbol is not IMethodSymbol method ||
            Unwrap(method.ReturnType) is not { } bound ||
            !SymbolEqualityComparer.Default.Equals(Wrapper(method.ReturnType), Wrapper(model.GetOperation(original, cancellationToken)?.Type)))
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(bound.TypeArguments[0], result.TypeArguments[0]) &&
            bound.TypeArguments.Skip(1).SequenceEqual(receiver.TypeArguments.Skip(1), SymbolEqualityComparer.Default);
    }

    private static bool Converts(Compilation compilation, ITypeSymbol source, ITypeSymbol destination)
    {
        if (SymbolEqualityComparer.Default.Equals(source, destination))
            return true;

        Microsoft.CodeAnalysis.CSharp.Conversion conversion = ((CSharpCompilation)compilation).ClassifyConversion(source, destination);
        return conversion.IsImplicit && !conversion.IsUserDefined && (conversion.IsReference || conversion.IsBoxing);
    }

    private static CastExpressionSyntax Cast(ITypeSymbol type, ExpressionSyntax expression)
    {
        return SyntaxFactory.CastExpression(
                TypeName(type),
                SyntaxFactory.ParenthesizedExpression(expression.WithoutTrivia()).WithAdditionalAnnotations(Simplifier.Annotation))
            .WithTriviaFrom(expression)
            .WithAdditionalAnnotations(Simplifier.Annotation);
    }

    private static TypeSyntax TypeName(ITypeSymbol type) =>
        SyntaxFactory.ParseTypeName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).WithAdditionalAnnotations(Simplifier.Annotation);

    private static ITypeSymbol? Receiver(IInvocationOperation operation) =>
        operation.Instance?.Type ?? (operation.Arguments.IsEmpty ? null : operation.Arguments[0].Value.Type);

    private static ITypeSymbol? Wrapper(ITypeSymbol? type) =>
        type is INamedTypeSymbol { Name: "Task" or "ValueTask", TypeArguments.Length: 1 } task ? task.OriginalDefinition : null;

    private static INamedTypeSymbol? Unwrap(ITypeSymbol? type)
    {
        if (type is INamedTypeSymbol { Name: "Task" or "ValueTask", TypeArguments.Length: 1 } task &&
            task.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks")
        {
            type = task.TypeArguments[0];
        }

        return type is INamedTypeSymbol { Name: "Result", Arity: >= 2 and <= 4 } named && named.ContainingNamespace.ToDisplayString() == ResultNamespace
            ? named
            : null;
    }

    private sealed record Rewritten(string Title, InvocationExpressionSyntax Replacement);
}
