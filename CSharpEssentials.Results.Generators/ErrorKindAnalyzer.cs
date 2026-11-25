using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Results.Generators;

/// <summary>
/// Reports a type whose own <c>[ErrorKind]</c> names a different kind than the nearest base type that declares one, and a
/// <c>Type</c> property that returns a constant kind other than the <c>[ErrorKind]</c> of its type (CSE5005).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorKindAnalyzer : DiagnosticAnalyzer
{
    private const string ErrorKindAttribute = "CSharpEssentials.Errors.ErrorKindAttribute";
    private const string ErrorType = "CSharpEssentials.Errors.ErrorType";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(TypedResultDiagnostics.ErrorKindMismatch);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? attribute = start.Compilation.GetTypeByMetadataName(ErrorKindAttribute);
            INamedTypeSymbol? errorType = start.Compilation.GetTypeByMetadataName(ErrorType);
            if (attribute is null || errorType is null)
                return;

            start.RegisterSymbolAction(symbolContext => Analyze(symbolContext, attribute), SymbolKind.NamedType);
            start.RegisterSyntaxNodeAction(nodeContext => AnalyzeTypeProperty(nodeContext, attribute, errorType), SyntaxKind.PropertyDeclaration);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol attribute)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        TypedConstant? own = Kind(type, attribute);
        if (own is null)
            return;

        for (INamedTypeSymbol? baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (Kind(baseType, attribute) is not { } inherited)
                continue;

            if (!Equals(inherited.Value, own.Value.Value))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    TypedResultDiagnostics.ErrorKindMismatch,
                    type.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None,
                    type.ToDisplayString(),
                    Name(own.Value),
                    baseType.ToDisplayString(),
                    Name(inherited)));
            }

            return;
        }
    }

    private static void AnalyzeTypeProperty(SyntaxNodeAnalysisContext context, INamedTypeSymbol attribute, INamedTypeSymbol errorType)
    {
        var declaration = (PropertyDeclarationSyntax)context.Node;
        if (declaration.Identifier.ValueText != "Type" || Body(declaration) is not { } body ||
            context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not { } property ||
            !SymbolEqualityComparer.Default.Equals(property.Type, errorType))
        {
            return;
        }

        Optional<object?> value = context.SemanticModel.GetConstantValue(body, context.CancellationToken);
        if (!value.HasValue || Nearest(property.ContainingType, attribute) is not { } kind || Equals(kind.Value, value.Value))
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            TypedResultDiagnostics.ErrorKindMismatch,
            body.GetLocation(),
            property.ContainingType.ToDisplayString(),
            Name(kind),
            property.ToDisplayString(),
            Name(kind.Type, value.Value)));
    }

    private static ExpressionSyntax? Body(PropertyDeclarationSyntax declaration) =>
        declaration.ExpressionBody?.Expression ??
        declaration.AccessorList?.Accessors.SingleOrDefault(static accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration))?.ExpressionBody?.Expression;

    private static TypedConstant? Nearest(INamedTypeSymbol type, INamedTypeSymbol attribute)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (Kind(current, attribute) is { } kind)
                return kind;
        }

        return null;
    }

    private static TypedConstant? Kind(INamedTypeSymbol type, INamedTypeSymbol attribute) =>
        type.GetAttributes()
            .Where(data => SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute) && data.ConstructorArguments.Length == 1)
            .Select(static data => (TypedConstant?)data.ConstructorArguments[0])
            .FirstOrDefault();

    private static string Name(TypedConstant kind) => Name(kind.Type, kind.Value);

    private static string Name(ITypeSymbol? type, object? value) =>
        type?.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(field => field.HasConstantValue && Equals(field.ConstantValue, value))?.Name
            ?? value?.ToString()
            ?? "null";
}
