namespace CSharpEssentials.Endpoints;

/// <summary>
/// Endpoint metadata that identifies the <see cref="IEndpoint"/> type an endpoint was mapped from.
/// </summary>
/// <param name="endpointType">The endpoint type.</param>
public sealed class EndpointTypeMetadata(Type endpointType)
{
    /// <summary>
    /// Gets the endpoint type.
    /// </summary>
    public Type EndpointType { get; } = endpointType;
}
