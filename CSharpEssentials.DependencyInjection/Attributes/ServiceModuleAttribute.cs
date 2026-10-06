namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Marks an assembly that contains a generated service registry. Emitted by the source generator and read by
/// the generated <c>AddAllServices</c> aggregate of referencing assemblies.
/// </summary>
/// <param name="registryType">The generated registry type.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class ServiceModuleAttribute(Type registryType) : Attribute
{
    /// <summary>
    /// Gets the generated registry type.
    /// </summary>
    public Type RegistryType { get; } = registryType;
}
