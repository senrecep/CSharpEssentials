using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Endpoints.Generators;

/// <summary>
/// Generates a per-assembly endpoint registry and, when enabled, an internal aggregate over all referenced registries.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class EndpointsGenerator : IIncrementalGenerator
{
    /// <summary>Tracking name of the per-type discovery step.</summary>
    public const string EndpointTypesStep = "EndpointTypes";

    /// <summary>Tracking name of the collected, ordered endpoint list.</summary>
    public const string CollectedEndpointsStep = "CollectedEndpoints";

    /// <summary>Tracking name of the host (assembly) model.</summary>
    public const string HostStep = "Host";

    /// <summary>Tracking name of the test-project flag.</summary>
    public const string IsTestProjectStep = "IsTestProject";

    private const string IsTestProjectProperty = "build_property.IsTestProject";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EndpointModel> endpoints = context.SyntaxProvider
            .CreateSyntaxProvider(static (node, _) => IsCandidate(node), static (ctx, ct) => CreateModel(ctx, ct))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!)
            .WithTrackingName(EndpointTypesStep);

        IncrementalValueProvider<EquatableArray<EndpointModel>> collected = endpoints
            .Collect()
            .Select(static (models, _) => Order(models))
            .WithTrackingName(CollectedEndpointsStep);

        IncrementalValueProvider<HostModel> host = context.CompilationProvider
            .Select(static (compilation, ct) => CreateHost(compilation, ct))
            .WithTrackingName(HostStep);

        IncrementalValueProvider<bool> isTestProject = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => IsTestProject(provider))
            .WithTrackingName(IsTestProjectStep);

        context.RegisterSourceOutput(collected.Combine(host), static (spc, input) =>
        {
            (EquatableArray<EndpointModel> models, HostModel model) = input;
            if (model.ExcludeAll || models.Count == 0)
            {
                return;
            }

            spc.AddSource($"{model.RegistryName}EndpointRegistry.g.cs", EndpointSourceWriter.WriteRegistry(model.RegistryName, models));
        });

        IncrementalValueProvider<bool> hasOwnRegistry = collected.Select(static (models, _) => models.Count > 0);
        context.RegisterSourceOutput(hasOwnRegistry.Combine(host).Combine(isTestProject), static (spc, input) =>
        {
            ((bool hasEndpoints, HostModel model), bool isTest) = input;
            if (model.DisableAggregate || !(model.GenerateAggregate || model.IsExecutable && !isTest))
            {
                return;
            }

            spc.AddSource(
                $"{model.RegistryName}EndpointAggregate.g.cs",
                EndpointSourceWriter.WriteAggregate(model, hasEndpoints && !model.ExcludeAll));
        });
    }

    private static bool IsCandidate(SyntaxNode node) =>
        node is TypeDeclarationSyntax { BaseList: not null } and not InterfaceDeclarationSyntax;

    private static EndpointModel? CreateModel(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol type ||
            !EndpointSymbolRules.IsEndpoint(type) ||
            !EndpointSymbolRules.IsAccessible(type) ||
            EndpointSymbolRules.IsAbstractOrOpenGeneric(type) ||
            type.IsRefLikeType ||
            EndpointSymbolRules.HasAttribute(type, EndpointSymbolRules.ExcludeAttribute))
        {
            return null;
        }

        GroupChain chain = EndpointSymbolRules.ResolveGroupChain(type);
        if (chain.Status != GroupChainStatus.Valid)
        {
            return null;
        }

        GroupModel[] groups = [.. chain.Groups.Select(static group => new GroupModel(
            EndpointSymbolRules.GetFullyQualifiedName(group),
            EndpointSymbolRules.GetMetadataFullName(group),
            group.Name))];

        return new EndpointModel(
            EndpointSymbolRules.GetFullyQualifiedName(type),
            EndpointSymbolRules.GetMetadataFullName(type),
            new EquatableArray<GroupModel>(groups));
    }

    private static EquatableArray<EndpointModel> Order(ImmutableArray<EndpointModel> models)
    {
        HashSet<string> seen = [with(StringComparer.Ordinal)];
        EndpointModel[] ordered = [.. models
            .Where(model => seen.Add(model.FullyQualifiedName))
            .OrderBy(static model => model.SortKey, StringComparer.Ordinal)];
        return new EquatableArray<EndpointModel>(ordered);
    }

    private static HostModel CreateHost(Compilation compilation, CancellationToken cancellationToken)
    {
        IAssemblySymbol assembly = compilation.Assembly;
        string? customName = assembly.GetAttributes()
            .Where(static attribute => EndpointSymbolRules.IsEndpointsType(attribute.AttributeClass, EndpointSymbolRules.RegistryNameAttribute))
            .Select(static attribute => attribute.ConstructorArguments.FirstOrDefault().Value as string)
            .FirstOrDefault(static name => !string.IsNullOrWhiteSpace(name));

        List<ReferencedRegistry> registries = [];
        HashSet<string> seen = [with(StringComparer.Ordinal)];
        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsFrameworkAssembly(reference.Name) || !ReferencesEndpoints(reference))
            {
                continue;
            }

            foreach (AttributeData attribute in reference.GetAttributes())
            {
                if (EndpointSymbolRules.IsEndpointsType(attribute.AttributeClass, EndpointSymbolRules.ModuleAttribute) &&
                    attribute.ConstructorArguments.FirstOrDefault().Value is INamedTypeSymbol registry)
                {
                    string name = EndpointSymbolRules.GetFullyQualifiedName(registry);
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
            EndpointSymbolRules.HasAttribute(assembly, EndpointSymbolRules.ExcludeAttribute),
            compilation.Options.OutputKind is OutputKind.ConsoleApplication or OutputKind.WindowsApplication,
            EndpointSymbolRules.HasAttribute(assembly, EndpointSymbolRules.GenerateAggregateAttribute),
            EndpointSymbolRules.HasAttribute(assembly, EndpointSymbolRules.DisableAggregateAttribute),
            new EquatableArray<ReferencedRegistry>(ordered));
    }

    private static bool IsFrameworkAssembly(string name) =>
        name.StartsWith("System", StringComparison.Ordinal) ||
        name.StartsWith("Microsoft", StringComparison.Ordinal) ||
        string.Equals(name, "mscorlib", StringComparison.Ordinal) ||
        string.Equals(name, "netstandard", StringComparison.Ordinal);

    private static bool ReferencesEndpoints(IAssemblySymbol assembly) =>
        assembly.Modules.Any(static module => module.ReferencedAssemblies.Any(static identity =>
            string.Equals(identity.Name, EndpointSymbolRules.EndpointsAssemblyName, StringComparison.Ordinal)));

    private static bool IsTestProject(AnalyzerConfigOptionsProvider provider) =>
        provider.GlobalOptions.TryGetValue(IsTestProjectProperty, out string? value) &&
        string.Equals(value?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
}
