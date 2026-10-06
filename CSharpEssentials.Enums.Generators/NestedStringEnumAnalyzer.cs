using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Enums.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestedStringEnumAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CSE0001";

    private const string AttributeName = "CSharpEssentials.Enums.StringEnumAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Nested StringEnum is not generated",
        "Enum '{0}' is nested in '{1}', so no StringEnum extension methods are generated for it; StringEnum extension methods are only generated for top-level enums",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The StringEnum source generator only generates extension methods for enums declared directly in a namespace. Move the enum out of its containing type to get the generated extension methods.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static startContext =>
        {
            INamedTypeSymbol? attributeSymbol = startContext.Compilation.GetTypeByMetadataName(AttributeName);
            if (attributeSymbol is null)
            {
                return;
            }

            startContext.RegisterSymbolAction(
                symbolContext => AnalyzeSymbol(symbolContext, attributeSymbol),
                SymbolKind.NamedType);
        });
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol attributeSymbol)
    {
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum, ContainingType: { } containingType } symbol)
        {
            return;
        }

        bool hasAttribute = symbol.GetAttributes()
            .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol));
        if (!hasAttribute)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            symbol.Locations.FirstOrDefault(),
            symbol.Name,
            containingType.ToDisplayString()));
    }
}
