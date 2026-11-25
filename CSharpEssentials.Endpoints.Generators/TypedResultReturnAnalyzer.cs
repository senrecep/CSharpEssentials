using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpEssentials.Endpoints.Generators;

/// <summary>
/// Reports a Map* route handler (lambda or method group) that returns <c>Result&lt;T, E1..E3&gt;</c>, or a
/// <c>Task</c> or <c>ValueTask</c> of it, without converting it to an HTTP result (CSE1013).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypedResultReturnAnalyzer : DiagnosticAnalyzer
{
    private const string RouteBuilderExtensions = "Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions";
    private const string ResultNamespace = "CSharpEssentials.ResultPattern";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(EndpointDiagnostics.UnconvertedTypedResult);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? routeBuilderExtensions = start.Compilation.GetTypeByMetadataName(RouteBuilderExtensions);
            INamedTypeSymbol? delegateType = start.Compilation.GetSpecialType(SpecialType.System_Delegate);
            if (routeBuilderExtensions is null)
            {
                return;
            }

            start.RegisterOperationAction(
                operationContext => Analyze(operationContext, routeBuilderExtensions, delegateType),
                OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol routeBuilderExtensions, INamedTypeSymbol delegateType)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        if (!method.Name.StartsWith("Map", StringComparison.Ordinal) ||
            !SymbolEqualityComparer.Default.Equals(method.ContainingType, routeBuilderExtensions))
        {
            return;
        }

        foreach (IArgumentOperation argument in invocation.Arguments)
        {
            if (!SymbolEqualityComparer.Default.Equals(argument.Parameter?.Type, delegateType) ||
                Unwrap(argument.Value) is not IDelegateCreationOperation creation)
            {
                continue;
            }

            ITypeSymbol? returnType = creation.Target switch
            {
                IAnonymousFunctionOperation lambda => lambda.Symbol.ReturnType,
                IMethodReferenceOperation reference => reference.Method.ReturnType,
                _ => null,
            };
            if (TypedResult(returnType) is { } typedResult)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    EndpointDiagnostics.UnconvertedTypedResult,
                    creation.Target.Syntax.GetLocation(),
                    typedResult.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
            }
        }
    }

    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    private static INamedTypeSymbol? TypedResult(ITypeSymbol? type)
    {
        if (type is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } awaitable &&
            awaitable.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
            awaitable.Name is "Task" or "ValueTask")
        {
            type = awaitable.TypeArguments[0];
        }

        return type is INamedTypeSymbol { Name: "Result", Arity: >= 2 and <= 4, TypeKind: TypeKind.Struct } result &&
            result.ContainingNamespace.ToDisplayString() == ResultNamespace
                ? result
                : null;
    }
}
