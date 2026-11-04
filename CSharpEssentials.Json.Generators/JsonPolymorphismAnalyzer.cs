using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Json.Generators;

/// <summary>
/// Reports problems of <c>[JsonTypeDiscriminator]</c> types, derived types of polymorphic bases and <c>[JsonPolymorphicProperty]</c>
/// members (CSE4001 to CSE4005 and CSE4007 to CSE4011).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class JsonPolymorphismAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            JsonPolymorphismDiagnostics.DuplicateDiscriminator,
            JsonPolymorphismDiagnostics.ListedOnBase,
            JsonPolymorphismDiagnostics.NotConcrete,
            JsonPolymorphismDiagnostics.NameConflict,
            JsonPolymorphismDiagnostics.MissingDiscriminator,
            JsonPolymorphismDiagnostics.OwnConverter,
            JsonPolymorphismDiagnostics.NoPolymorphicBase,
            JsonPolymorphismDiagnostics.PropertyNotPolymorphic,
            JsonPolymorphismDiagnostics.Unreachable,
            JsonPolymorphismDiagnostics.NotListed);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            if (start.Compilation.GetTypeByMetadataName(PolymorphismSymbols.TypeDiscriminatorAttribute) is null)
                return;

            ConcurrentBag<ValueUse> uses = [];
            start.RegisterSymbolAction(symbolContext => AnalyzeType(symbolContext, uses), SymbolKind.NamedType);
            start.RegisterSymbolAction(AnalyzeMember, SymbolKind.Property, SymbolKind.Field);
            start.RegisterCompilationEndAction(end => ReportDuplicates(end, uses));
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, ConcurrentBag<ValueUse> uses)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        Location location = type.Locations.FirstOrDefault() ?? Location.None;
        if (PolymorphismSymbols.IsPolymorphicBase(type))
            AnalyzeBase(context, type, uses);

        object? value = PolymorphismSymbols.Discriminator(type);
        bool isConcrete = !type.IsAbstract && type.TypeKind is TypeKind.Class or TypeKind.Struct && !type.IsGenericType;
        if (value is not null && !isConcrete)
        {
            context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.NotConcrete, location, Name(type)));
            return;
        }

        INamedTypeSymbol[] ancestors = [.. PolymorphismSymbols.PolymorphicAncestors(type).Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)];
        if (ancestors.Length == 0)
        {
            if (value is not null)
                context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.NoPolymorphicBase, location, Name(type)));
            return;
        }

        if (!isConcrete)
            return;

        if (value is not null && !PolymorphismSymbols.IsReachable(type))
            context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.Unreachable, location, Name(type)));

        bool hasConverter = PolymorphismSymbols.HasAttribute(type, PolymorphismSymbols.JsonConverterAttribute);
        if (hasConverter)
            context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.OwnConverter, location, Name(type), Name(ancestors[0])));

        foreach (INamedTypeSymbol ancestor in ancestors)
        {
            AnalyzeDerived(context, type, ancestor, value, hasConverter, location, uses);
        }
    }

    private static void AnalyzeDerived(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        INamedTypeSymbol ancestor,
        object? value,
        bool hasConverter,
        Location location,
        ConcurrentBag<ValueUse> uses)
    {
        (ITypeSymbol Type, object? Value)[] declared = [.. PolymorphismSymbols.DeclaredDerivedTypes(ancestor)];
        (ITypeSymbol Type, object? Value)? listed = declared.Any(entry => SymbolEqualityComparer.Default.Equals(entry.Type, type))
            ? declared.First(entry => SymbolEqualityComparer.Default.Equals(entry.Type, type))
            : null;
        if (listed is { } entry)
        {
            if (value is not null)
                context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.ListedOnBase, location, Name(type), Name(ancestor)));
            if (entry.Value is not null)
                uses.Add(new ValueUse(ancestor, entry.Value, type));
            return;
        }

        if (hasConverter)
            return;

        if (value is not null)
        {
            uses.Add(new ValueUse(ancestor, value, type));
            ReportNameConflicts(context, ancestor, type, location);
        }
        else if (declared.Length == 0)
        {
            string clrName = PolymorphismSymbols.ClrFullName(type);
            context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.MissingDiscriminator, location, Name(type), Name(ancestor), clrName));
            uses.Add(new ValueUse(ancestor, clrName, type));
            ReportNameConflicts(context, ancestor, type, location);
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.NotListed, location, Name(type), Name(ancestor)));
        }
    }

    private static void AnalyzeBase(SymbolAnalysisContext context, INamedTypeSymbol baseType, ConcurrentBag<ValueUse> uses)
    {
        foreach ((ITypeSymbol derived, object? value) in PolymorphismSymbols.DeclaredDerivedTypes(baseType))
        {
            if (value is not null && derived is INamedTypeSymbol named)
                uses.Add(new ValueUse(baseType, value, named));
        }

        ReportNameConflicts(context, baseType, baseType, baseType.Locations.FirstOrDefault() ?? Location.None);
    }

    private static void ReportNameConflicts(SymbolAnalysisContext context, INamedTypeSymbol baseType, INamedTypeSymbol type, Location location)
    {
        string propertyName = PolymorphismSymbols.DiscriminatorPropertyName(baseType);
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (member.IsStatic || member.DeclaredAccessibility != Accessibility.Public || member is not (IPropertySymbol { IsIndexer: false } or IFieldSymbol { IsImplicitlyDeclared: false }))
                    continue;

                string jsonName = member.GetAttributes()
                    .Where(static attribute => PolymorphismSymbols.Is(attribute, PolymorphismSymbols.JsonPropertyNameAttribute))
                    .Select(static attribute => attribute.ConstructorArguments.FirstOrDefault().Value as string)
                    .FirstOrDefault(static name => name is not null) ?? member.Name;
                if (string.Equals(jsonName, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.NameConflict, location, propertyName, Name(baseType), member.Name, Name(type)));
                    return;
                }
            }
        }
    }

    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol member = context.Symbol;
        AttributeData? attribute = member.GetAttributes().FirstOrDefault(static a => PolymorphismSymbols.Is(a, PolymorphismSymbols.PolymorphicPropertyAttribute));
        ITypeSymbol? type = member switch
        {
            IPropertySymbol property => property.Type,
            IFieldSymbol field => field.Type,
            _ => null,
        };
        if (attribute is null || type is null)
            return;

        Location location = member.Locations.FirstOrDefault() ?? Location.None;
        if (PolymorphismSymbols.ElementBase(type) is not { } baseType)
        {
            context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.PropertyNotPolymorphic, location, member.Name, type.ToDisplayString()));
            return;
        }

        INamedTypeSymbol declaring = member.ContainingType;
        ITypeSymbol? unreachable = new[] { declaring, type, baseType }
            .FirstOrDefault(candidate => !PolymorphismSymbols.IsReachable(candidate) || candidate is INamedTypeSymbol { IsGenericType: true } && SymbolEqualityComparer.Default.Equals(candidate, declaring));
        if (unreachable is not null)
            context.ReportDiagnostic(Diagnostic.Create(JsonPolymorphismDiagnostics.Unreachable, location, unreachable.ToDisplayString()));
    }

    private static void ReportDuplicates(CompilationAnalysisContext context, ConcurrentBag<ValueUse> uses)
    {
        IEnumerable<IGrouping<(string, string), ValueUse>> groups = uses
            .GroupBy(static use => (Name(use.BaseType), Key(use.Value)));
        foreach (IGrouping<(string, string), ValueUse> group in groups.OrderBy(static g => g.Key.Item1, StringComparer.Ordinal).ThenBy(static g => g.Key.Item2, StringComparer.Ordinal))
        {
            INamedTypeSymbol[] types = [.. group
                .Select(static use => use.Type)
                .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
                .OrderBy(static type => type.ToDisplayString(), StringComparer.Ordinal)];
            for (int i = 1; i < types.Length; i++)
            {
                Location location = types[i].Locations.FirstOrDefault(static l => l.IsInSource) ?? Location.None;
                context.ReportDiagnostic(Diagnostic.Create(
                    JsonPolymorphismDiagnostics.DuplicateDiscriminator,
                    location,
                    Name(types[0]),
                    Name(types[i]),
                    PolymorphismSymbols.Display(group.First().Value),
                    group.Key.Item1));
            }
        }
    }

    private static string Key(object value) => (value is int ? "i:" : "s:") + PolymorphismSymbols.Display(value);

    private static string Name(ITypeSymbol type) => type.ToDisplayString();

    private sealed record ValueUse(INamedTypeSymbol BaseType, object Value, INamedTypeSymbol Type);
}
