using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Reports enums of this compilation without <c>[StringEnum]</c> that public properties or parameters use (CSE0010, disabled by default).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumUsageAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(StringEnumDiagnostics.EnumWithoutStringEnum);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? attribute = start.Compilation.GetTypeByMetadataName(StringEnumGenerator.AttributeName);
            if (attribute is null)
                return;

            // Positional records declare a property and a constructor parameter at the same location; report it once.
            ConcurrentDictionary<Location, byte> reported = new();
            start.RegisterSymbolAction(
                symbolContext => AnalyzeMember(symbolContext, attribute, reported),
                SymbolKind.Property,
                SymbolKind.Method);
        });
    }

    private static void AnalyzeMember(SymbolAnalysisContext context, INamedTypeSymbol attribute, ConcurrentDictionary<Location, byte> reported)
    {
        ISymbol member = context.Symbol;
        if (member.IsImplicitlyDeclared || !IsVisible(member) || !IsVisible(member.ContainingType))
            return;

        switch (member)
        {
            case IPropertySymbol property:
                Check(context, attribute, reported, property.Type, property, property.Locations);
                foreach (IParameterSymbol parameter in property.Parameters)
                    Check(context, attribute, reported, parameter.Type, property, parameter.Locations);
                break;
            case IMethodSymbol { MethodKind: not (MethodKind.PropertyGet or MethodKind.PropertySet) } method:
                foreach (IParameterSymbol parameter in method.Parameters)
                    Check(context, attribute, reported, parameter.Type, method, parameter.Locations);
                break;
            default:
                break;
        }
    }

    private static void Check(
        SymbolAnalysisContext context,
        INamedTypeSymbol attribute,
        ConcurrentDictionary<Location, byte> reported,
        ITypeSymbol type,
        ISymbol member,
        ImmutableArray<Location> locations)
    {
        if (locations.Length == 0 || !locations[0].IsInSource)
            return;

        foreach (INamedTypeSymbol enumType in Enums(type))
        {
            if (!SymbolEqualityComparer.Default.Equals(enumType.ContainingAssembly, context.Compilation.Assembly) ||
                HasAttribute(enumType, attribute) ||
                !reported.TryAdd(locations[0], 0))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                StringEnumDiagnostics.EnumWithoutStringEnum,
                locations[0],
                enumType.ToDisplayString(),
                member.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
        }
    }

    private static IEnumerable<INamedTypeSymbol> Enums(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                foreach (INamedTypeSymbol inner in Enums(array.ElementType))
                    yield return inner;
                break;
            case INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType:
                yield return enumType;
                break;
            case INamedTypeSymbol { IsGenericType: true } generic:
                foreach (ITypeSymbol argument in generic.TypeArguments)
                {
                    foreach (INamedTypeSymbol inner in Enums(argument))
                        yield return inner;
                }

                break;
            default:
                break;
        }
    }

    private static bool IsVisible(ISymbol? symbol)
    {
        for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
        {
            if (current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal))
                return false;
        }

        return true;
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
