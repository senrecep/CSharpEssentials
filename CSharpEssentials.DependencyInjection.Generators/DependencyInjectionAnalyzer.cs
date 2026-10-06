using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.DependencyInjection.Generators;

/// <summary>
/// Reports invalid service registrations and decorators (CSE2001–CSE2005, CSE2007, CSE2008).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DependencyInjectionAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DependencyInjectionDiagnostics.ServiceNotImplemented,
        DependencyInjectionDiagnostics.DuplicateThrowRegistration,
        DependencyInjectionDiagnostics.RegisteredAsSelf,
        DependencyInjectionDiagnostics.DecoratorWithoutInner,
        DependencyInjectionDiagnostics.DecoratorConstructorCount,
        DependencyInjectionDiagnostics.NotConstructible,
        DependencyInjectionDiagnostics.OpenGenericDecorator);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            if (ServiceTypeInspector.HasAttribute(start.Compilation.Assembly.GetAttributes(), ServiceTypeInspector.ExcludeAttribute))
            {
                return;
            }

            ConcurrentBag<(string TypeName, InspectedRegistration Registration)> registrations = [];
            start.RegisterSymbolAction(symbolContext => AnalyzeType(symbolContext, registrations), SymbolKind.NamedType);
            start.RegisterCompilationEndAction(end => ReportDuplicateThrows(end, registrations));
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, ConcurrentBag<(string TypeName, InspectedRegistration Registration)> registrations)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        ServiceTypeInspection inspection = ServiceTypeInspector.Inspect(type, context.CancellationToken);
        foreach (InspectionIssue issue in inspection.Issues)
        {
            context.ReportDiagnostic(Diagnostic.Create(issue.Descriptor, GetLocation(issue.Attribute, type, context.CancellationToken), issue.Arguments));
        }

        if (inspection.HasErrors)
        {
            return;
        }

        foreach (InspectedRegistration registration in inspection.Registrations)
        {
            registrations.Add((type.ToDisplayString(), registration));
        }
    }

    private static void ReportDuplicateThrows(
        CompilationAnalysisContext context,
        ConcurrentBag<(string TypeName, InspectedRegistration Registration)> registrations)
    {
        List<(string TypeName, InspectedRegistration Registration, string ServiceKey)> entries = [.. registrations
            .SelectMany(static entry => entry.Registration.Model.Services.Select(service => (
                entry.TypeName,
                entry.Registration,
                ServiceKey: service.ServiceType + "|" + (entry.Registration.Model.Key ?? string.Empty))))
            .OrderBy(static entry => entry.TypeName, StringComparer.Ordinal)];

        foreach ((string TypeName, InspectedRegistration Registration, string ServiceKey) entry in entries)
        {
            if (!string.Equals(entry.Registration.Model.Strategy, ServiceTypeInspector.ThrowStrategy, StringComparison.Ordinal))
            {
                continue;
            }

            (string TypeName, InspectedRegistration Registration, string ServiceKey) other = entries.FirstOrDefault(candidate =>
                !ReferenceEquals(candidate.Registration, entry.Registration) &&
                string.Equals(candidate.ServiceKey, entry.ServiceKey, StringComparison.Ordinal));
            if (other.Registration is null)
            {
                continue;
            }

            string key = entry.Registration.Model.Key is { } value ? " with key " + value : string.Empty;
            string service = entry.ServiceKey.Substring(0, entry.ServiceKey.IndexOf('|'));
            context.ReportDiagnostic(Diagnostic.Create(
                DependencyInjectionDiagnostics.DuplicateThrowRegistration,
                GetLocation(entry.Registration.Attribute, null, context.CancellationToken),
                entry.TypeName,
                service.Replace("global::", string.Empty),
                key,
                other.TypeName));
        }
    }

    private static Location GetLocation(AttributeData attribute, INamedTypeSymbol? type, CancellationToken cancellationToken) =>
        attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ??
        type?.Locations.FirstOrDefault() ??
        Location.None;
}
