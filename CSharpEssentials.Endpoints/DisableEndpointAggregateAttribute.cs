namespace CSharpEssentials.Endpoints;

/// <summary>
/// Suppresses the internal <c>MapAllEndpoints</c> aggregate that is generated automatically for <c>Exe</c> and <c>WinExe</c> assemblies.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class DisableEndpointAggregateAttribute : Attribute;
