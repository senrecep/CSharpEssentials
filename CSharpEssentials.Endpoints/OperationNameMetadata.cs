using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints;

internal sealed class OperationNameMetadata(OperationNameRegistry registry, OperationNameSlot slot) : IEndpointNameMetadata, IRouteNameMetadata
{
    public string EndpointName => registry.Resolve(slot);

    public string RouteName => EndpointName;

    public override string ToString() => EndpointName;
}
