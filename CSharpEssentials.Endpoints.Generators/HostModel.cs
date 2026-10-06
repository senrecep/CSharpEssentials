namespace CSharpEssentials.Endpoints.Generators;

internal sealed record HostModel(
    string RegistryName,
    bool ExcludeAll,
    bool IsExecutable,
    bool GenerateAggregate,
    bool DisableAggregate,
    EquatableArray<ReferencedRegistry> ReferencedRegistries);
