using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Endpoints.Generators;

internal sealed class EndpointNameUse(string? name, bool isEndpointName, bool isRouteName, Location location)
{
    public string? Name { get; } = name;

    public bool IsEndpointName { get; } = isEndpointName;

    public bool IsRouteName { get; } = isRouteName;

    public Location Location { get; } = location;
}
