using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Json.Generators;

/// <summary>
/// Reports a <c>JsonSerializerContext</c> that lists a polymorphic base but not a <c>[JsonTypeDiscriminator]</c> type of the same
/// compilation that derives from it (CSE4006).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class JsonSerializerContextAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic property that holds the fully qualified missing type.</summary>
    public const string DerivedTypeProperty = "DerivedType";

    private const string ContextType = "System.Text.Json.Serialization.JsonSerializerContext";

    private const string SerializableAttribute = "System.Text.Json.Serialization.JsonSerializableAttribute";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(JsonPolymorphismDiagnostics.MissingFromContext);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? contextType = start.Compilation.GetTypeByMetadataName(ContextType);
            if (contextType is null || start.Compilation.GetTypeByMetadataName(PolymorphismSymbols.TypeDiscriminatorAttribute) is null)
                return;

            ConcurrentBag<INamedTypeSymbol> contexts = [];
            ConcurrentBag<INamedTypeSymbol> valued = [];
            start.RegisterSymbolAction(symbolContext =>
            {
                var type = (INamedTypeSymbol)symbolContext.Symbol;
                if (DerivesFrom(type, contextType) && PolymorphismSymbols.HasAttribute(type, SerializableAttribute))
                    contexts.Add(type);
                if (PolymorphismSymbols.Discriminator(type) is not null && !type.IsAbstract && type.TypeKind is TypeKind.Class or TypeKind.Struct)
                    valued.Add(type);
            }, SymbolKind.NamedType);
            start.RegisterCompilationEndAction(end => Report(end, contexts, valued));
        });
    }

    private static void Report(CompilationAnalysisContext context, ConcurrentBag<INamedTypeSymbol> contexts, ConcurrentBag<INamedTypeSymbol> valued)
    {
        INamedTypeSymbol[] derivedTypes = [.. valued.OrderBy(static type => type.ToDisplayString(), StringComparer.Ordinal)];
        foreach (INamedTypeSymbol serializerContext in contexts)
        {
            HashSet<ITypeSymbol> listed = new(serializerContext.GetAttributes()
                .Where(static attribute => PolymorphismSymbols.Is(attribute, SerializableAttribute))
                .Select(static attribute => attribute.ConstructorArguments.FirstOrDefault().Value)
                .OfType<ITypeSymbol>(), SymbolEqualityComparer.Default);
            foreach (INamedTypeSymbol baseType in listed.OfType<INamedTypeSymbol>().Where(PolymorphismSymbols.IsPolymorphicBase).OrderBy(static type => type.ToDisplayString(), StringComparer.Ordinal))
            {
                foreach (INamedTypeSymbol derived in derivedTypes)
                {
                    if (listed.Contains(derived) || PolymorphismSymbols.Lists(baseType, derived) ||
                        !PolymorphismSymbols.PolymorphicAncestors(derived).Contains(baseType, SymbolEqualityComparer.Default))
                    {
                        continue;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                        JsonPolymorphismDiagnostics.MissingFromContext,
                        serializerContext.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None,
                        ImmutableDictionary<string, string?>.Empty.Add(DerivedTypeProperty, PolymorphismSymbols.FullyQualified(derived)),
                        serializerContext.ToDisplayString(),
                        baseType.ToDisplayString(),
                        derived.ToDisplayString()));
                }
            }
        }
    }

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
        }

        return false;
    }
}
