namespace CSharpEssentials.Endpoints;

/// <summary>
/// Opts a library or test assembly in to the generated internal <c>MapAllEndpoints</c> aggregate.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class GenerateEndpointAggregateAttribute : Attribute;
