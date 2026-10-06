namespace CSharpEssentials.Endpoints.Generators;

internal sealed class GroupNode(GroupModel? group)
{
    public GroupModel? Group { get; } = group;

    public List<EndpointModel> Endpoints { get; } = [];

    public SortedDictionary<string, GroupNode> Children { get; } = [with(StringComparer.Ordinal)];

    public static GroupNode Build(EquatableArray<EndpointModel> endpoints)
    {
        GroupNode root = new(null);
        foreach (EndpointModel endpoint in endpoints)
        {
            GroupNode node = root;
            foreach (GroupModel group in endpoint.Groups)
            {
                if (!node.Children.TryGetValue(group.SortKey, out GroupNode? child))
                {
                    child = new GroupNode(group);
                    node.Children.Add(group.SortKey, child);
                }

                node = child;
            }

            node.Endpoints.Add(endpoint);
        }

        return root;
    }

    public IEnumerable<EndpointModel> InMappingOrder() =>
        Endpoints.Concat(Children.Values.SelectMany(static child => child.InMappingOrder()));
}
