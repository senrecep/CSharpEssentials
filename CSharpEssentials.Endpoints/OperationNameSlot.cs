namespace CSharpEssentials.Endpoints;

internal sealed class OperationNameSlot(Type endpointType, string methodSuffix)
{
    public Type EndpointType { get; } = endpointType;

    public string MethodSuffix { get; } = methodSuffix;

    public string Name { get; set; } = string.Empty;
}
