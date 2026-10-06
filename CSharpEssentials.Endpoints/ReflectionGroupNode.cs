namespace CSharpEssentials.Endpoints;

internal sealed class ReflectionGroupNode(Type? groupType)
{
    public Type? GroupType { get; } = groupType;

    public List<Type> Endpoints { get; } = [];

    public SortedDictionary<string, ReflectionGroupNode> Children { get; } = [with(StringComparer.Ordinal)];

    public IEnumerable<Type> InMappingOrder() =>
        Endpoints.Concat(Children.Values.SelectMany(static child => child.InMappingOrder()));
}
