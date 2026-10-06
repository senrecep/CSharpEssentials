namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Generates the internal <c>AddAllServices</c> aggregate in a library or test project, where it is not generated automatically.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class GenerateServiceAggregateAttribute : Attribute;
