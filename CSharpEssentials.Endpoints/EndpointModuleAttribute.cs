namespace CSharpEssentials.Endpoints;

/// <summary>
/// Marks an assembly that contains a generated endpoint registry. Emitted by the generator and read at compile time
/// by the aggregate generator of referencing assemblies.
/// </summary>
/// <param name="registryType">The generated registry type.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class EndpointModuleAttribute(Type registryType) : Attribute
{
    /// <summary>
    /// Gets the generated registry type.
    /// </summary>
    public Type RegistryType { get; } = registryType;
}
