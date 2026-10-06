using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Reports member problems of <c>[StringEnum]</c> enums (CSE0002 to CSE0009), an invalid naming property (CSE0012), enum attributes
/// without <c>[StringEnum]</c> (CSE0013), enums the generator cannot give metadata to (CSE0015) and colliding extensions classes (CSE0016).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StringEnumAnalyzer : DiagnosticAnalyzer
{
    private const int IntegerStorage = 2;

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            StringEnumDiagnostics.WireNameCollision,
            StringEnumDiagnostics.AliasCollision,
            StringEnumDiagnostics.MultipleFallbacks,
            StringEnumDiagnostics.ImplicitIntegerValues,
            StringEnumDiagnostics.FlagsWithoutZero,
            StringEnumDiagnostics.InvalidFlagValue,
            StringEnumDiagnostics.FallbackOnFlags,
            StringEnumDiagnostics.InvalidWireName,
            StringEnumDiagnostics.InvalidNamingProperty,
            StringEnumDiagnostics.AttributeWithoutStringEnum,
            StringEnumDiagnostics.MissingMetadata,
            StringEnumDiagnostics.ExtensionsClassCollision);

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

            bool supportsRegistration = start.Compilation is not CSharpCompilation csharp ||
                csharp.LanguageVersion >= LanguageVersion.CSharp9;
            _ = start.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(StringEnumGenerator.NamingProperty, out string? naming);
            int projectNaming = EnumWireNaming.Parse(naming);
            ConcurrentBag<INamedTypeSymbol> generated = [];
            start.RegisterSymbolAction(
                symbolContext => AnalyzeEnum(symbolContext, attribute, supportsRegistration, projectNaming, generated),
                SymbolKind.NamedType);
            start.RegisterCompilationEndAction(end =>
            {
                if (!EnumWireNaming.IsValid(naming))
                {
                    end.ReportDiagnostic(Diagnostic.Create(
                        StringEnumDiagnostics.InvalidNamingProperty,
                        Location.None,
                        naming,
                        EnumWireNaming.ValidNames));
                }

                ReportCollisions(end, generated);
            });
        });
    }

    private static void AnalyzeEnum(
        SymbolAnalysisContext context,
        INamedTypeSymbol attribute,
        bool supportsRegistration,
        int projectNaming,
        ConcurrentBag<INamedTypeSymbol> generated)
    {
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } type)
            return;

        AttributeData? stringEnum = FindAttribute(type, attribute);
        if (stringEnum is null)
        {
            ReportAttributesWithoutStringEnum(context, type);
            return;
        }

        AnalyzeMembers(context, type, stringEnum, projectNaming);

        string reason;
        if (!EnumModelReader.IsReachable(type))
        {
            reason = "it or a containing type is private, protected or file-local, or it is nested in a generic type";
        }
        else
        {
            generated.Add(type);
            if (supportsRegistration)
                return;
            reason = "the project uses a C# version below 9";
        }

        context.ReportDiagnostic(Diagnostic.Create(StringEnumDiagnostics.MissingMetadata, TypeLocation(type), type.ToDisplayString(), reason));
    }

    private static void AnalyzeMembers(SymbolAnalysisContext context, INamedTypeSymbol type, AttributeData stringEnum, int projectNaming)
    {
        List<IFieldSymbol> fields = EnumModelReader.ConstantFields(type);
        bool isSigned = EnumModelReader.IsSigned(type);
        EnumMemberModel[] members = [.. fields.Select(field => EnumModelReader.ReadMember(field, isSigned, context.CancellationToken))];
        bool isFlags = EnumModelReader.HasAttribute(type, EnumModelReader.FlagsAttribute);
        string[] wireNames = StringEnumSourceWriter.WireNames(members, EnumModelReader.GetNamedInt(stringEnum, "Naming"), projectNaming);

        foreach (EnumRuleViolation violation in EnumModelRules.Check(members, wireNames, isFlags))
            context.ReportDiagnostic(CreateDiagnostic(violation, type, fields, context.CancellationToken));

        int storage = EnumModelReader.GetNamedInt(stringEnum, "Storage");
        if (storage == IntegerStorage || storage == 0 && isFlags)
            ReportImplicitValues(context, type, fields);
    }

    private static Diagnostic CreateDiagnostic(EnumRuleViolation violation, INamedTypeSymbol type, List<IFieldSymbol> fields, CancellationToken cancellationToken)
    {
        if (violation.Member < 0)
            return Diagnostic.Create(StringEnumDiagnostics.FlagsWithoutZero, TypeLocation(type), type.ToDisplayString());

        IFieldSymbol field = fields[violation.Member];
        string member = field.Name;
        return violation.Kind switch
        {
            EnumRuleKind.WireNameCollision => Diagnostic.Create(
                StringEnumDiagnostics.WireNameCollision, WireNameLocation(field, cancellationToken), member, violation.Text, violation.Detail),
            EnumRuleKind.AliasCollision => Diagnostic.Create(
                StringEnumDiagnostics.AliasCollision, AttributeLocation(field, EnumModelReader.EnumAliasAttribute, cancellationToken), violation.Text, member, violation.Detail),
            EnumRuleKind.MultipleFallbacks => Diagnostic.Create(
                StringEnumDiagnostics.MultipleFallbacks, AttributeLocation(field, EnumModelReader.EnumFallbackAttribute, cancellationToken), member, violation.Detail),
            EnumRuleKind.FallbackOnFlags => Diagnostic.Create(
                StringEnumDiagnostics.FallbackOnFlags, AttributeLocation(field, EnumModelReader.EnumFallbackAttribute, cancellationToken), member, type.ToDisplayString()),
            EnumRuleKind.InvalidFlagValue => Diagnostic.Create(StringEnumDiagnostics.InvalidFlagValue, MemberLocation(field), member),
            EnumRuleKind.FlagsWithoutZero => Diagnostic.Create(StringEnumDiagnostics.FlagsWithoutZero, TypeLocation(type), type.ToDisplayString()),
            EnumRuleKind.InvalidWireName when violation.Alias >= 0 => Diagnostic.Create(
                StringEnumDiagnostics.InvalidWireName, AttributeLocation(field, EnumModelReader.EnumAliasAttribute, cancellationToken), "alias", violation.Text, member, violation.Detail),
            EnumRuleKind.InvalidWireName => Diagnostic.Create(
                StringEnumDiagnostics.InvalidWireName, WireNameLocation(field, cancellationToken), "wire name", violation.Text, member, violation.Detail),
            _ => throw new ArgumentOutOfRangeException(nameof(violation), violation.Kind, "Unknown enum rule."),
        };
    }

    private static void ReportImplicitValues(SymbolAnalysisContext context, INamedTypeSymbol type, List<IFieldSymbol> fields)
    {
        List<string> implicitMembers = [];
        foreach (IFieldSymbol field in fields)
        {
            foreach (SyntaxReference reference in field.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax(context.CancellationToken) is EnumMemberDeclarationSyntax { EqualsValue: null })
                    implicitMembers.Add(field.Name);
            }
        }

        if (implicitMembers.Count == 0)
            return;

        string list = "'" + string.Join("', '", implicitMembers) + "'";
        context.ReportDiagnostic(Diagnostic.Create(
            StringEnumDiagnostics.ImplicitIntegerValues,
            TypeLocation(type),
            type.ToDisplayString(),
            list + (implicitMembers.Count == 1 ? " has" : " have")));
    }

    private static void ReportAttributesWithoutStringEnum(SymbolAnalysisContext context, INamedTypeSymbol type)
    {
        foreach (IFieldSymbol field in EnumModelReader.ConstantFields(type))
        {
            foreach (AttributeData data in field.GetAttributes())
            {
                string? name = data.AttributeClass?.ToDisplayString();
                if (name is not (EnumModelReader.EnumAliasAttribute or EnumModelReader.EnumFallbackAttribute))
                    continue;

                Location location = data.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? MemberLocation(field);
                context.ReportDiagnostic(Diagnostic.Create(
                    StringEnumDiagnostics.AttributeWithoutStringEnum,
                    location,
                    data.AttributeClass!.Name.Replace("Attribute", string.Empty),
                    field.Name,
                    type.ToDisplayString()));
            }
        }
    }

    private static void ReportCollisions(CompilationAnalysisContext context, ConcurrentBag<INamedTypeSymbol> generated)
    {
        IEnumerable<IGrouping<string, INamedTypeSymbol>> collisions = generated
            .GroupBy(static type => type.ContainingNamespace.ToDisplayString() + "." + EnumModelReader.ExtensionsClassName(type), StringComparer.Ordinal)
            .Where(static group => group.Count() > 1);
        foreach (IGrouping<string, INamedTypeSymbol> group in collisions)
        {
            INamedTypeSymbol[] types = [.. group.OrderBy(static type => type.ToDisplayString(), StringComparer.Ordinal)];
            foreach (INamedTypeSymbol type in types)
            {
                string others = string.Join("', '", types.Where(other => !SymbolEqualityComparer.Default.Equals(other, type)).Select(static other => other.ToDisplayString()));
                context.ReportDiagnostic(Diagnostic.Create(
                    StringEnumDiagnostics.ExtensionsClassCollision,
                    TypeLocation(type),
                    type.ToDisplayString(),
                    EnumModelReader.ExtensionsClassName(type),
                    others));
            }
        }
    }

    private static Location WireNameLocation(IFieldSymbol field, CancellationToken cancellationToken)
    {
        Location? json = FindAttributeLocation(field, EnumModelReader.JsonMemberNameAttribute, cancellationToken);
        return json ?? AttributeLocation(field, EnumModelReader.EnumMemberAttribute, cancellationToken);
    }

    private static Location AttributeLocation(IFieldSymbol field, string metadataName, CancellationToken cancellationToken) =>
        FindAttributeLocation(field, metadataName, cancellationToken) ?? MemberLocation(field);

    private static Location? FindAttributeLocation(IFieldSymbol field, string metadataName, CancellationToken cancellationToken)
    {
        foreach (AttributeData data in field.GetAttributes())
        {
            if (data.AttributeClass?.ToDisplayString() == metadataName && data.ApplicationSyntaxReference is { } reference)
                return reference.GetSyntax(cancellationToken).GetLocation();
        }

        return null;
    }

    private static Location MemberLocation(IFieldSymbol field) => field.Locations.Length > 0 ? field.Locations[0] : Location.None;

    private static Location TypeLocation(INamedTypeSymbol type) => type.Locations.Length > 0 ? type.Locations[0] : Location.None;

    private static AttributeData? FindAttribute(INamedTypeSymbol type, INamedTypeSymbol attribute)
    {
        foreach (AttributeData data in type.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute))
                return data;
        }

        return null;
    }
}
