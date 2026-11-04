using System.ComponentModel;

namespace CSharpEssentials.Json;

/// <summary>
/// Marks an assembly that registers polymorphic types from generated code. Generated code adds it; it is not meant to be used directly.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class JsonPolymorphismModuleAttribute : Attribute
{
    /// <summary>Creates the marker.</summary>
    /// <param name="registry">The generated registry class of the assembly.</param>
    public JsonPolymorphismModuleAttribute(Type registry) => Registry = registry;

    /// <summary>The generated registry class of the assembly.</summary>
    public Type Registry { get; }
}
