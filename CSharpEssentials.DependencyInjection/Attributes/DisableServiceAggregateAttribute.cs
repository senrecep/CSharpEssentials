namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Suppresses the internal <c>AddAllServices</c> aggregate that is generated automatically in executable projects.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class DisableServiceAggregateAttribute : Attribute;
