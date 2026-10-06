namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Excludes a class, or a whole assembly, from attribute-based registration and decoration.
/// </summary>
/// <remarks>
/// On a class, the class is neither registered nor applied as a decorator. On an assembly, no registry is
/// generated and <c>AddServicesFromAssemblies</c> skips the assembly.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class ExcludeFromRegistrationAttribute : Attribute;
