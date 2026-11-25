using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Results.Generators;

/// <summary>
/// Reports a type whose own <c>[ErrorKind]</c> names a different kind than the nearest base type that declares one (CSE5005).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorKindAnalyzer : DiagnosticAnalyzer
{
    private const string ErrorKindAttribute = "CSharpEssentials.Errors.ErrorKindAttribute";

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
            if (attribute is null)
                return;

            start.RegisterSymbolAction(symbolContext => Analyze(symbolContext, attribute), SymbolKind.NamedType);
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

    private static TypedConstant? Kind(INamedTypeSymbol type, INamedTypeSymbol attribute) =>
        type.GetAttributes()
            .Where(data => SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute) && data.ConstructorArguments.Length == 1)
            .Select(static data => (TypedConstant?)data.ConstructorArguments[0])
            .FirstOrDefault();

    private static string Name(TypedConstant kind) =>
        kind.Type?.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(field => field.HasConstantValue && Equals(field.ConstantValue, kind.Value))?.Name
            ?? kind.Value?.ToString()
            ?? "null";
}
