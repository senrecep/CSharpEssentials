using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.DependencyInjection.Generators;

/// <summary>
/// Reports invalid service registrations and decorators and captive dependencies (CSE2001–CSE2009).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DependencyInjectionAnalyzer : DiagnosticAnalyzer
{
    private const string Transient = "Transient";

    private const string IsTestProjectProperty = "build_property.IsTestProject";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DependencyInjectionDiagnostics.ServiceNotImplemented,
        DependencyInjectionDiagnostics.DuplicateThrowRegistration,
        DependencyInjectionDiagnostics.RegisteredAsSelf,
        DependencyInjectionDiagnostics.DecoratorWithoutInner,
        DependencyInjectionDiagnostics.DecoratorConstructorCount,
        DependencyInjectionDiagnostics.CaptiveDependency,
        DependencyInjectionDiagnostics.NotConstructible,
        DependencyInjectionDiagnostics.OpenGenericDecorator,
        DependencyInjectionDiagnostics.RegistryNameCollision);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            if (ServiceTypeInspector.HasAttribute(start.Compilation.Assembly.GetAttributes(), ServiceTypeInspector.ExcludeAttribute))
            {
                start.RegisterCompilationEndAction(static end => ReportRegistryCollisions(end, hasOwnRegistry: false));
                return;
            }

            ConcurrentBag<(string TypeName, string SortName, int Index, InspectedRegistration Registration)> registrations = [];
            ConcurrentBag<(string TypeName, InspectedRegistration Registration, IReadOnlyList<ServiceDependency> Dependencies)> consumers = [];
            ConcurrentBag<string> ownTypes = [];
            start.RegisterSymbolAction(symbolContext => AnalyzeType(symbolContext, registrations, consumers, ownTypes), SymbolKind.NamedType);
            start.RegisterCompilationEndAction(end =>
            {
                ReportDuplicateThrows(end, registrations);
                ReportCaptiveDependencies(end, registrations, consumers);
                ReportRegistryCollisions(end, hasOwnRegistry: !ownTypes.IsEmpty);
            });
        });
    }

    private static void ReportRegistryCollisions(CompilationAnalysisContext context, bool hasOwnRegistry)
    {
        ImmutableArray<AttributeData> attributes = context.Compilation.Assembly.GetAttributes();
        bool isExecutable = context.Compilation.Options.OutputKind is OutputKind.ConsoleApplication or OutputKind.WindowsApplication;
        bool isTestProject = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(IsTestProjectProperty, out string? value) &&
            string.Equals(value?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
        if (ServiceTypeInspector.HasAttribute(attributes, "DisableServiceAggregateAttribute") ||
            !(ServiceTypeInspector.HasAttribute(attributes, "GenerateServiceAggregateAttribute") || isExecutable && !isTestProject))
        {
            return;
        }

        List<ReferencedRegistry> registries = [.. ReferencedRegistryReader.Read(context.Compilation, context.CancellationToken)];
        if (hasOwnRegistry)
        {
            IAssemblySymbol assembly = context.Compilation.Assembly;
            registries.Add(new ReferencedRegistry(assembly.Name, RegistryNames.RegistryNamespace + RegistryNames.ForAssembly(assembly) + "ServiceRegistry"));
        }

        foreach (IGrouping<string, ReferencedRegistry> collision in ReferencedRegistryReader.FindCollisions(registries))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DependencyInjectionDiagnostics.RegistryNameCollision,
                Location.None,
                string.Join(", ", collision.Select(static registry => "'" + registry.AssemblyName + "'")),
                collision.Key.Replace("global::", string.Empty)));
        }
    }

    private static void AnalyzeType(
        SymbolAnalysisContext context,
        ConcurrentBag<(string TypeName, string SortName, int Index, InspectedRegistration Registration)> registrations,
        ConcurrentBag<(string TypeName, InspectedRegistration Registration, IReadOnlyList<ServiceDependency> Dependencies)> consumers,
        ConcurrentBag<string> ownTypes)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        ServiceTypeInspection inspection = ServiceTypeInspector.Inspect(type, context.CancellationToken);
        if (inspection.ToModel() is not null)
        {
            ownTypes.Add(type.ToDisplayString());
        }

        foreach (InspectionIssue issue in inspection.Issues)
        {
            context.ReportDiagnostic(Diagnostic.Create(issue.Descriptor, GetLocation(issue.Attribute, type, context.CancellationToken), issue.Arguments));
        }

        if (inspection.HasErrors)
        {
            return;
        }

        IReadOnlyList<ServiceDependency>? dependencies = null;
        for (int index = 0; index < inspection.Registrations.Count; index++)
        {
            InspectedRegistration registration = inspection.Registrations[index];
            registrations.Add((type.ToDisplayString(), inspection.SortName, index, registration));
            if (LifetimeRank(registration.Model.Lifetime) < LifetimeRank(Transient))
            {
                dependencies ??= ServiceDependencyReader.Read(type);
                consumers.Add((type.ToDisplayString(), registration, dependencies));
            }
        }
    }

    private static void ReportCaptiveDependencies(
        CompilationAnalysisContext context,
        ConcurrentBag<(string TypeName, string SortName, int Index, InspectedRegistration Registration)> registrations,
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
        ConcurrentBag<(string TypeName, string SortName, int Index, InspectedRegistration Registration)> registrations)
    {
        List<(string TypeName, InspectedRegistration Registration, string ServiceKey)> entries = [.. registrations
            .OrderBy(static entry => entry.SortName, StringComparer.Ordinal)
            .ThenBy(static entry => entry.Index)
            .SelectMany(static entry => entry.Registration.Model.Services.Select(service => (
                entry.TypeName,
                entry.Registration,
                ServiceKey: service.ServiceType + "|" + (entry.Registration.Model.Key ?? string.Empty))))];

        for (int index = 0; index < entries.Count; index++)
        {
            (string TypeName, InspectedRegistration Registration, string ServiceKey) entry = entries[index];
            if (!string.Equals(entry.Registration.Model.Strategy, ServiceTypeInspector.ThrowStrategy, StringComparison.Ordinal))
            {
                continue;
            }

            (string TypeName, InspectedRegistration Registration, string ServiceKey) other = entries.Take(index).FirstOrDefault(candidate =>
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
