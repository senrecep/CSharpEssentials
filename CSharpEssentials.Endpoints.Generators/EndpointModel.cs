namespace CSharpEssentials.Endpoints.Generators;

internal sealed record EndpointModel(string FullyQualifiedName, string SortKey, EquatableArray<GroupModel> Groups);
