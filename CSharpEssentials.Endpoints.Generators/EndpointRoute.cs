using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Endpoints.Generators;

internal sealed class EndpointRoute(string groupKey, string method, string pattern, string endpointName, Location location)
{
    public string GroupKey { get; } = groupKey;

    public string Method { get; } = method;

    public string Pattern { get; } = pattern;

    public string EndpointName { get; } = endpointName;

    public Location Location { get; } = location;

    public string Key => GroupKey + "|" + Method + "|" + Pattern.Trim('/').ToUpperInvariant();
}
