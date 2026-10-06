namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Overrides the name used for the generated registry of the assembly:
/// <c>[assembly: ServiceRegistryName("Billing")]</c> generates <c>AddBillingServices</c>.
/// </summary>
/// <param name="name">The registry name.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class ServiceRegistryNameAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the registry name.
    /// </summary>
    public string Name { get; } = name;
}
