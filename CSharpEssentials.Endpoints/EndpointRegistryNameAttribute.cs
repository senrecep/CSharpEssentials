namespace CSharpEssentials.Endpoints;

/// <summary>
/// Overrides the generated registry name of an assembly.
/// <c>[assembly: EndpointRegistryName("Apps")]</c> generates <c>AppsEndpointRegistry.MapAppsEndpoints</c>.
/// </summary>
/// <param name="name">The registry name. It is sanitized like an assembly name.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class EndpointRegistryNameAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the registry name.
    /// </summary>
    public string Name { get; } = name;
}
