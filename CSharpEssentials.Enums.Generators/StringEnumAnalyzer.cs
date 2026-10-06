using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Reports <c>[StringEnum]</c> enums that the enum generator cannot give metadata to (CSE0015).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StringEnumAnalyzer : DiagnosticAnalyzer
{
    private const string AttributeName = "CSharpEssentials.Enums.StringEnumAttribute";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(StringEnumDiagnostics.MissingMetadata);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? attribute = start.Compilation.GetTypeByMetadataName(AttributeName);
            if (attribute is null)
                return;

            bool supportsRegistration = start.Compilation is not CSharpCompilation csharp ||
                csharp.LanguageVersion >= LanguageVersion.CSharp9;
            start.RegisterSymbolAction(symbolContext => AnalyzeEnum(symbolContext, attribute, supportsRegistration), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeEnum(SymbolAnalysisContext context, INamedTypeSymbol attribute, bool supportsRegistration)
    {
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } type || !HasAttribute(type, attribute))
            return;

        string reason;
        if (!EnumModelReader.IsReachable(type))
            reason = "it or a containing type is private, protected or file-local, or it is nested in a generic type";
        else if (!supportsRegistration)
            reason = "the project uses a C# version below 9";
        else
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            StringEnumDiagnostics.MissingMetadata,
            type.Locations.Length > 0 ? type.Locations[0] : Location.None,
            type.ToDisplayString(),
            reason));
    }

    private static bool HasAttribute(INamedTypeSymbol type, INamedTypeSymbol attribute)
    {
        foreach (AttributeData data in type.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute))
                return true;
        }

        return false;
    }
}
