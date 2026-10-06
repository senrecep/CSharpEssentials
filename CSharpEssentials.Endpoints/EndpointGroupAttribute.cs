namespace CSharpEssentials.Endpoints;

/// <summary>
/// Places an endpoint or a group under the group type <see cref="GroupType"/>.
/// </summary>
/// <param name="groupType">A non-abstract, non-generic type that implements <see cref="IEndpointGroup"/>.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EndpointGroupAttribute(Type groupType) : Attribute
{
    /// <summary>
    /// Gets the group type the annotated type is placed under.
    /// </summary>
    public Type GroupType { get; } = groupType;
}
