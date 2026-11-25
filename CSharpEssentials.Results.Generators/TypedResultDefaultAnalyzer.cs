using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpEssentials.Results.Generators;

/// <summary>
/// Reports the places that create a default typed result (CSE5001): <c>default</c>, <c>default(R)</c>, <c>new R()</c>,
/// <c>new R[n]</c>, and a private field or an auto-property only its own type can set, without an initializer, that no code of
/// its type assigns.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypedResultDefaultAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(TypedResultDiagnostics.DefaultResult);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            var symbols = TypedResultSymbols.Create(start.Compilation);
            if (symbols is null)
                return;

            start.RegisterOperationAction(operationContext => AnalyzeDefault(operationContext, symbols), OperationKind.DefaultValue);
            start.RegisterOperationAction(operationContext => AnalyzeCreation(operationContext, symbols), OperationKind.ObjectCreation);
            start.RegisterOperationAction(operationContext => AnalyzeArray(operationContext, symbols), OperationKind.ArrayCreation);
            start.RegisterSymbolStartAction(symbolStart => AnalyzeType(symbolStart, symbols), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeDefault(OperationAnalysisContext context, TypedResultSymbols symbols)
    {
        IOperation operation = context.Operation;
        if (operation.IsImplicit || !symbols.IsTypedResult(operation.Type) || IsCompared(operation))
            return;

        Report(context, operation, operation.Syntax is LiteralExpressionSyntax ? "'default'" : "'default(...)'");
    }

    private static void AnalyzeCreation(OperationAnalysisContext context, TypedResultSymbols symbols)
    {
        var creation = (IObjectCreationOperation)context.Operation;
        if (creation.IsImplicit || !symbols.IsTypedResult(creation.Type) || !creation.Arguments.IsEmpty ||
            creation.Initializer is { Initializers.IsEmpty: false })
        {
            return;
        }

        Report(context, creation, "A parameterless constructor call");
    }

    private static void AnalyzeArray(OperationAnalysisContext context, TypedResultSymbols symbols)
    {
        var creation = (IArrayCreationOperation)context.Operation;
        if (creation.Initializer is not null || creation.Type is not IArrayTypeSymbol array || !symbols.IsTypedResult(array.ElementType) ||
            creation.DimensionSizes.All(static size => size.ConstantValue is { HasValue: true, Value: 0 }))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            TypedResultDiagnostics.DefaultResult,
            creation.Syntax.GetLocation(),
            "An array without an initializer",
            array.ElementType.ToDisplayString()));
    }

    private static void AnalyzeType(SymbolStartAnalysisContext context, TypedResultSymbols symbols)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        var candidates = type.GetMembers().Where(member => IsUninitialized(member, symbols)).ToImmutableArray();
        if (candidates.IsEmpty)
            return;

        ConcurrentDictionary<ISymbol, byte> assigned = new(SymbolEqualityComparer.Default);
        context.RegisterOperationAction(
            operationContext => MarkAssigned(operationContext.Operation, candidates, assigned),
            OperationKind.FieldReference,
            OperationKind.PropertyReference);
        context.RegisterSymbolEndAction(end =>
        {
            foreach (ISymbol member in candidates.Where(member => !assigned.ContainsKey(member)))
            {
                end.ReportDiagnostic(Diagnostic.Create(
                    TypedResultDiagnostics.DefaultResult,
                    member.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None,
                    $"{(member is IFieldSymbol ? "Field" : "Property")} '{member.Name}' without an initializer",
                    MemberType(member).ToDisplayString()));
            }
        });
    }

    private static bool IsUninitialized(ISymbol member, TypedResultSymbols symbols)
    {
        if (member.IsImplicitlyDeclared || member.IsAbstract || member.IsExtern || member.DeclaringSyntaxReferences.IsEmpty)
            return false;

        return member switch
        {
            IFieldSymbol { IsConst: false, IsRequired: false, DeclaredAccessibility: Accessibility.Private } field when symbols.IsTypedResult(field.Type) =>
                member.DeclaringSyntaxReferences.All(static reference => reference.GetSyntax() is VariableDeclaratorSyntax { Initializer: null }),
            IPropertySymbol { IsRequired: false, IsIndexer: false } property when symbols.IsTypedResult(property.Type) && IsSetOnlyByItsType(property) =>
                member.DeclaringSyntaxReferences.All(static reference => IsAutoPropertyWithoutInitializer(reference.GetSyntax())),
            _ => false,
        };
    }

    private static bool IsSetOnlyByItsType(IPropertySymbol property) =>
        property.DeclaredAccessibility == Accessibility.Private || property.SetMethod is null or { DeclaredAccessibility: Accessibility.Private };

    private static bool IsAutoPropertyWithoutInitializer(SyntaxNode syntax) =>
        syntax is PropertyDeclarationSyntax { Initializer: null, ExpressionBody: null, AccessorList: { } accessors } declaration &&
        !declaration.Modifiers.Any(SyntaxKind.PartialKeyword) &&
        accessors.Accessors.All(static accessor => accessor.Body is null && accessor.ExpressionBody is null);

    private static void MarkAssigned(IOperation reference, ImmutableArray<ISymbol> candidates, ConcurrentDictionary<ISymbol, byte> assigned)
    {
        ISymbol member = reference switch
        {
            IFieldReferenceOperation field => field.Field,
            IPropertyReferenceOperation property => property.Property,
            _ => throw new InvalidOperationException(),
        };
        if (!candidates.Contains(member.OriginalDefinition, SymbolEqualityComparer.Default) || !IsWritten(reference))
            return;

        assigned.TryAdd(member.OriginalDefinition, 0);
    }

    private static bool IsWritten(IOperation reference)
    {
        IOperation current = reference;
        while (current.Parent is ITupleOperation tuple)
            current = tuple;

        return current.Parent switch
        {
            IAssignmentOperation assignment => assignment.Target == current,
            IArgumentOperation { Parameter.RefKind: RefKind.Out or RefKind.Ref } => true,
            _ => false,
        };
    }

    private static bool IsCompared(IOperation operation)
    {
        IOperation current = operation;
        while (current.Parent is IConversionOperation conversion)
            current = conversion;

        return current.Parent is IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } ||
            current.Parent is IArgumentOperation { Parent: IInvocationOperation { TargetMethod.Name: "Equals" } };
    }

    private static ITypeSymbol MemberType(ISymbol member) =>
        member is IFieldSymbol field ? field.Type : ((IPropertySymbol)member).Type;

    private static void Report(OperationAnalysisContext context, IOperation operation, string what) =>
        context.ReportDiagnostic(Diagnostic.Create(
            TypedResultDiagnostics.DefaultResult,
            operation.Syntax.GetLocation(),
            what,
            operation.Type!.ToDisplayString()));
}
