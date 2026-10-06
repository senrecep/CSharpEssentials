using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.DependencyInjection.Generators;

/// <summary>
/// Generates a per-assembly service registry and, when enabled, an internal aggregate over all referenced registries.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ServiceRegistrationGenerator : IIncrementalGenerator
{
    /// <summary>Tracking name of the per-type discovery step.</summary>
    public const string ServiceTypesStep = "ServiceTypes";

    /// <summary>Tracking name of the collected, ordered service type list.</summary>
    public const string CollectedServicesStep = "CollectedServices";

    /// <summary>Tracking name of the host (assembly) model.</summary>
    public const string HostStep = "Host";

    /// <summary>Tracking name of the test-project flag.</summary>
    public const string IsTestProjectStep = "IsTestProject";

    private const string AttributePrefix = ServiceTypeInspector.AttributeNamespace + ".";

    private const string IsTestProjectProperty = "build_property.IsTestProject";

    private const string RuntimeAssemblyName = "CSharpEssentials.DependencyInjection";

    private static readonly string[] AttributeMetadataNames =
    [
        AttributePrefix + "RegisterScopedAttribute",
        AttributePrefix + "RegisterScopedAttribute`1",
        AttributePrefix + "RegisterSingletonAttribute",
        AttributePrefix + "RegisterSingletonAttribute`1",
        AttributePrefix + "RegisterTransientAttribute",
        AttributePrefix + "RegisterTransientAttribute`1",
        AttributePrefix + "DecoratesAttribute",
        AttributePrefix + "DecoratesAttribute`1",
    ];

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<ServiceTypeModel>>? all = null;
        foreach (string metadataName in AttributeMetadataNames)
        {
            IncrementalValueProvider<ImmutableArray<ServiceTypeModel>> models = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    metadataName,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, ct) => ServiceTypeInspector.Inspect((INamedTypeSymbol)ctx.TargetSymbol, ct).ToModel())
                .Where(static model => model is not null)
                .Select(static (model, _) => model!)
                .WithTrackingName(ServiceTypesStep)
                .Collect();
            all = all is null
                ? models
                : all.Value.Combine(models).Select(static (pair, _) => pair.Left.AddRange(pair.Right));
        }

        IncrementalValueProvider<EquatableArray<ServiceTypeModel>> collected = all!.Value
            .Select(static (models, _) => Order(models))
            .WithTrackingName(CollectedServicesStep);

        IncrementalValueProvider<HostModel> host = context.CompilationProvider
            .Select(static (compilation, ct) => CreateHost(compilation, ct))
            .WithTrackingName(HostStep);

        IncrementalValueProvider<bool> isTestProject = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => IsTestProject(provider))
            .WithTrackingName(IsTestProjectStep);

        context.RegisterSourceOutput(collected.Combine(host), static (spc, input) =>
        {
            (EquatableArray<ServiceTypeModel> models, HostModel model) = input;
            if (model.ExcludeAll || models.Count == 0)
            {
                return;
            }

            spc.AddSource($"{model.RegistryName}ServiceRegistry.g.cs", ServiceSourceWriter.WriteRegistry(model.RegistryName, models));
        });

        IncrementalValueProvider<bool> hasOwnRegistry = collected.Select(static (models, _) => models.Count > 0);
        context.RegisterSourceOutput(hasOwnRegistry.Combine(host).Combine(isTestProject), static (spc, input) =>
        {
            ((bool hasServices, HostModel model), bool isTest) = input;
            if (model.DisableAggregate || !(model.GenerateAggregate || model.IsExecutable && !isTest))
            {
                return;
            }

            spc.AddSource(
                $"{model.RegistryName}ServiceAggregate.g.cs",
                ServiceSourceWriter.WriteAggregate(model, hasServices && !model.ExcludeAll));
        });
    }

    private static EquatableArray<ServiceTypeModel> Order(ImmutableArray<ServiceTypeModel> models)
    {
        HashSet<string> seen = [with(StringComparer.Ordinal)];
        ServiceTypeModel[] ordered = [.. models
            .Where(model => seen.Add(model.SortName))
            .OrderBy(static model => model.SortName, StringComparer.Ordinal)];
        return new EquatableArray<ServiceTypeModel>(ordered);
    }

    private static HostModel CreateHost(Compilation compilation, CancellationToken cancellationToken)
    {
        IAssemblySymbol assembly = compilation.Assembly;
        ImmutableArray<AttributeData> attributes = assembly.GetAttributes();
        string? customName = attributes
            .Where(static attribute => ServiceTypeInspector.IsAttribute(attribute.AttributeClass, "ServiceRegistryNameAttribute"))
            .Select(static attribute => attribute.ConstructorArguments.FirstOrDefault().Value as string)
            .FirstOrDefault(static name => !string.IsNullOrWhiteSpace(name));

        List<ReferencedRegistry> registries = [];
        HashSet<string> seen = [with(StringComparer.Ordinal)];
        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsFrameworkAssembly(reference.Name) || !ReferencesRuntime(reference))
            {
                continue;
            }

            foreach (AttributeData attribute in reference.GetAttributes())
            {
                if (ServiceTypeInspector.IsAttribute(attribute.AttributeClass, "ServiceModuleAttribute") &&
                    attribute.ConstructorArguments.FirstOrDefault().Value is INamedTypeSymbol registry)
                {
                    string name = TypeNames.FullyQualified(registry);
                    if (seen.Add(name))
                    {
                        registries.Add(new ReferencedRegistry(reference.Name, name));
                    }
                }
            }
        }

        ReferencedRegistry[] ordered = [.. registries
            .OrderBy(static registry => registry.AssemblyName, StringComparer.Ordinal)
            .ThenBy(static registry => registry.FullyQualifiedName, StringComparer.Ordinal)];

        return new HostModel(
            RegistryNames.Sanitize(customName ?? assembly.Name),
            ServiceTypeInspector.HasAttribute(attributes, ServiceTypeInspector.ExcludeAttribute),
            compilation.Options.OutputKind is OutputKind.ConsoleApplication or OutputKind.WindowsApplication,
            ServiceTypeInspector.HasAttribute(attributes, "GenerateServiceAggregateAttribute"),
            ServiceTypeInspector.HasAttribute(attributes, "DisableServiceAggregateAttribute"),
            new EquatableArray<ReferencedRegistry>(ordered));
    }

    private static bool IsFrameworkAssembly(string name) =>
        name.StartsWith("System", StringComparison.Ordinal) ||
        name.StartsWith("Microsoft", StringComparison.Ordinal) ||
        string.Equals(name, "mscorlib", StringComparison.Ordinal) ||
        string.Equals(name, "netstandard", StringComparison.Ordinal);

    private static bool ReferencesRuntime(IAssemblySymbol assembly) =>
        assembly.Modules.Any(static module => module.ReferencedAssemblies.Any(static identity =>
            string.Equals(identity.Name, RuntimeAssemblyName, StringComparison.Ordinal)));

    private static bool IsTestProject(AnalyzerConfigOptionsProvider provider) =>
        provider.GlobalOptions.TryGetValue(IsTestProjectProperty, out string? value) &&
        string.Equals(value?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
}
