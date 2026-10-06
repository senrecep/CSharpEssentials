using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.DependencyInjection.Generators;

/// <summary>
/// Reports invalid service registrations and decorators and captive dependencies (CSE2001–CSE2008).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DependencyInjectionAnalyzer : DiagnosticAnalyzer
{
    private const string Transient = "Transient";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DependencyInjectionDiagnostics.ServiceNotImplemented,
        DependencyInjectionDiagnostics.DuplicateThrowRegistration,
        DependencyInjectionDiagnostics.RegisteredAsSelf,
        DependencyInjectionDiagnostics.DecoratorWithoutInner,
        DependencyInjectionDiagnostics.DecoratorConstructorCount,
        DependencyInjectionDiagnostics.CaptiveDependency,
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
            ConcurrentBag<(string TypeName, InspectedRegistration Registration, IReadOnlyList<ServiceDependency> Dependencies)> consumers = [];
            start.RegisterSymbolAction(symbolContext => AnalyzeType(symbolContext, registrations, consumers), SymbolKind.NamedType);
            start.RegisterCompilationEndAction(end =>
            {
                ReportDuplicateThrows(end, registrations);
                ReportCaptiveDependencies(end, registrations, consumers);
            });
        });
    }

    private static void AnalyzeType(
        SymbolAnalysisContext context,
        ConcurrentBag<(string TypeName, InspectedRegistration Registration)> registrations,
        ConcurrentBag<(string TypeName, InspectedRegistration Registration, IReadOnlyList<ServiceDependency> Dependencies)> consumers)
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

        IReadOnlyList<ServiceDependency>? dependencies = null;
        foreach (InspectedRegistration registration in inspection.Registrations)
        {
            registrations.Add((type.ToDisplayString(), registration));
            if (LifetimeRank(registration.Model.Lifetime) < LifetimeRank(Transient))
            {
                dependencies ??= ServiceDependencyReader.Read(type);
                consumers.Add((type.ToDisplayString(), registration, dependencies));
            }
        }
    }

    private static void ReportCaptiveDependencies(
        CompilationAnalysisContext context,
        ConcurrentBag<(string TypeName, InspectedRegistration Registration)> registrations,
        ConcurrentBag<(string TypeName, InspectedRegistration Registration, IReadOnlyList<ServiceDependency> Dependencies)> consumers)
    {
        ILookup<string, (string TypeName, string Lifetime)> providers = registrations
            .SelectMany(static entry => entry.Registration.Model.Services.Select(service => (
                ServiceKey: service.ServiceType + "|" + entry.Registration.Model.Key,
                entry.TypeName,
                entry.Registration.Model.Lifetime)))
            .OrderBy(static entry => entry.TypeName, StringComparer.Ordinal)
            .ToLookup(static entry => entry.ServiceKey, static entry => (entry.TypeName, entry.Lifetime), StringComparer.Ordinal);

        foreach ((string TypeName, InspectedRegistration Registration, IReadOnlyList<ServiceDependency> Dependencies) consumer in consumers)
        {
            int consumerRank = LifetimeRank(consumer.Registration.Model.Lifetime);
            foreach (ServiceDependency dependency in consumer.Dependencies)
            {
                (string TypeName, string Lifetime) provider = providers[dependency.ServiceType + "|" + dependency.Key]
                    .Concat(dependency.OpenServiceType is { } open ? providers[open + "|" + dependency.Key] : [])
                    .FirstOrDefault(candidate => LifetimeRank(candidate.Lifetime) > consumerRank);
                if (provider.TypeName is null)
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    DependencyInjectionDiagnostics.CaptiveDependency,
                    GetLocation(consumer.Registration.Attribute, null, context.CancellationToken),
                    consumer.Registration.Model.Lifetime,
                    consumer.TypeName,
                    dependency.ServiceType.Replace("global::", string.Empty),
                    dependency.ParameterName,
                    provider.TypeName,
                    provider.Lifetime));
            }
        }
    }

    private static int LifetimeRank(string lifetime) => lifetime switch
    {
        "Singleton" => 0,
        "Scoped" => 1,
        _ => 2,
    };

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
