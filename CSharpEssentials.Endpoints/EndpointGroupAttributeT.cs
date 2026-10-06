namespace CSharpEssentials.Endpoints;

/// <summary>
/// Places an endpoint or a group under <typeparamref name="TGroup"/>.
/// Equivalent to <see cref="EndpointGroupAttribute"/>; only one of the two may be present on a type.
/// </summary>
/// <typeparam name="TGroup">The group the annotated type is placed under.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EndpointGroupAttribute<TGroup> : Attribute
    where TGroup : IEndpointGroup
{
    /// <summary>
    /// Gets the group type the annotated type is placed under.
    /// </summary>
    public Type GroupType => typeof(TGroup);
}
