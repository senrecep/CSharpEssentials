using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

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

    /// <summary>Tracking name of the collected explicit endpoint names.</summary>
    public const string ReservedNamesStep = "ReservedNames";

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

        IncrementalValueProvider<EquatableArray<string>> reservedNames = context.SyntaxProvider
            .CreateSyntaxProvider(static (node, _) => EndpointNameReader.IsCandidate(node), static (ctx, ct) => ReadNames(ctx, ct))
            .Where(static names => names.Count > 0)
            .Collect()
            .Select(static (names, _) => Merge(names))
            .WithTrackingName(ReservedNamesStep);

        IncrementalValueProvider<bool> isTestProject = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => IsTestProject(provider))
            .WithTrackingName(IsTestProjectStep);

        context.RegisterSourceOutput(collected.Combine(host).Combine(reservedNames), static (spc, input) =>
        {
            ((EquatableArray<EndpointModel> models, HostModel model), EquatableArray<string> names) = input;
            if (model.ExcludeAll || models.Count == 0)
            {
                return;
            }

            spc.AddSource($"{model.RegistryName}EndpointRegistry.g.cs", EndpointSourceWriter.WriteRegistry(model.RegistryName, models, names));
        });

        IncrementalValueProvider<bool> hasOwnRegistry = collected.Select(static (models, _) => models.Count > 0);
        context.RegisterSourceOutput(hasOwnRegistry.Combine(host).Combine(isTestProject).Combine(reservedNames), static (spc, input) =>
        {
            (((bool hasEndpoints, HostModel model), bool isTest), EquatableArray<string> names) = input;
            if (model.DisableAggregate || !(model.GenerateAggregate || model.IsExecutable && !isTest))
            {
                return;
            }

            spc.AddSource(
                $"{model.RegistryName}EndpointAggregate.g.cs",
                EndpointSourceWriter.WriteAggregate(model, hasEndpoints && !model.ExcludeAll, names));
        });
    }

    private static bool IsCandidate(SyntaxNode node) =>
        node is TypeDeclarationSyntax { BaseList: not null } and not InterfaceDeclarationSyntax;

    private static EndpointModel? CreateModel(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol type ||
            EndpointSymbolRules.GetGeneratedChain(type) is not { } chain)
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

    private static EquatableArray<string> ReadNames(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetOperation(context.Node, cancellationToken) is not IInvocationOperation invocation)
        {
            return default;
        }

        string[] names = [.. EndpointNameReader.Read(invocation)
            .Select(static use => use.Name)
            .OfType<string>()];
        return new EquatableArray<string>(names);
    }

    private static EquatableArray<string> Merge(ImmutableArray<EquatableArray<string>> names)
    {
        string[] merged = [.. names
            .SelectMany(static group => group)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)];
        return new EquatableArray<string>(merged);
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

        IReadOnlyList<ReferencedRegistry> registries = ReferencedRegistryReader.Read(compilation, cancellationToken);
        HashSet<string> colliding = [.. ReferencedRegistryReader.FindCollisions(registries).Select(static group => group.Key)];
        ReferencedRegistry[] ordered = [.. registries.Where(registry => !colliding.Contains(registry.FullyQualifiedName))];

        return new HostModel(
            RegistryNames.ForAssembly(assembly),
            EndpointSymbolRules.HasAttribute(assembly, EndpointSymbolRules.ExcludeAttribute),
            compilation.Options.OutputKind is OutputKind.ConsoleApplication or OutputKind.WindowsApplication,
            EndpointSymbolRules.HasAttribute(assembly, EndpointSymbolRules.GenerateAggregateAttribute),
            EndpointSymbolRules.HasAttribute(assembly, EndpointSymbolRules.DisableAggregateAttribute),
            new EquatableArray<ReferencedRegistry>(ordered));
    }

    private static bool IsTestProject(AnalyzerConfigOptionsProvider provider) =>
        provider.GlobalOptions.TryGetValue(IsTestProjectProperty, out string? value) &&
        string.Equals(value?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
}
