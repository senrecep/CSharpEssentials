using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpEssentials.EntityFrameworkCore.Generators;

/// <summary>
/// Reports calls to the parameterless EF Core <c>IgnoreQueryFilters()</c> when the named overload
/// <c>IgnoreQueryFilters(IReadOnlyCollection&lt;string&gt;)</c> (EF Core 10 and later) is available (CSE3001).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IgnoreQueryFiltersAnalyzer : DiagnosticAnalyzer
{
    private const string QueryableExtensionsMetadataName = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";

    private const string ReadOnlyCollectionMetadataName = "System.Collections.Generic.IReadOnlyCollection`1";

    private const string MethodName = "IgnoreQueryFilters";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(QueryFilterDiagnostics.UnnamedIgnoreQueryFilters);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            IMethodSymbol? unnamed = FindUnnamedOverloadWhenNamedExists(start.Compilation);
            if (unnamed is null)
                return;

            start.RegisterOperationAction(operationContext => AnalyzeInvocation(operationContext, unnamed), OperationKind.Invocation);
        });
    }

    private static IMethodSymbol? FindUnnamedOverloadWhenNamedExists(Compilation compilation)
    {
        INamedTypeSymbol? extensions = compilation.GetTypeByMetadataName(QueryableExtensionsMetadataName);
        INamedTypeSymbol? readOnlyCollection = compilation.GetTypeByMetadataName(ReadOnlyCollectionMetadataName);
        if (extensions is null || readOnlyCollection is null)
            return null;

        IMethodSymbol? unnamed = null;
        bool hasNamed = false;
        foreach (IMethodSymbol method in extensions.GetMembers(MethodName).OfType<IMethodSymbol>())
        {
            if (!method.IsStatic || method.TypeParameters.Length != 1)
                continue;

            if (method.Parameters.Length == 1)
                unnamed = method;
            else if (method.Parameters.Length == 2 && IsStringCollection(method.Parameters[1].Type, readOnlyCollection))
                hasNamed = true;
        }

        return hasNamed ? unnamed : null;
    }

    private static bool IsStringCollection(ITypeSymbol type, INamedTypeSymbol readOnlyCollection) =>
        type is INamedTypeSymbol { TypeArguments.Length: 1 } named &&
        SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, readOnlyCollection) &&
        named.TypeArguments[0].SpecialType == SpecialType.System_String;

    private static void AnalyzeInvocation(OperationAnalysisContext context, IMethodSymbol unnamed)
    {
        if (context.Operation is not IInvocationOperation invocation ||
            !SymbolEqualityComparer.Default.Equals((invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod).OriginalDefinition, unnamed))
        {
            return;
        }

        Location location = invocation.Syntax switch
        {
            InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } => memberAccess.Name.GetLocation(),
            InvocationExpressionSyntax { Expression: MemberBindingExpressionSyntax memberBinding } => memberBinding.Name.GetLocation(),
            _ => invocation.Syntax.GetLocation(),
        };
        string entityType = invocation.TargetMethod.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(QueryFilterDiagnostics.UnnamedIgnoreQueryFilters, location, entityType));
    }
}
