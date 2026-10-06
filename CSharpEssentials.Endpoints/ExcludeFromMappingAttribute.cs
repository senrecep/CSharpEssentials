namespace CSharpEssentials.Endpoints;

/// <summary>
/// Excludes an endpoint, a group (with everything under it) or a whole assembly from endpoint discovery,
/// on both the generated and the reflection path.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class ExcludeFromMappingAttribute : Attribute;
