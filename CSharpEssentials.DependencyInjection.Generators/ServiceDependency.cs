namespace CSharpEssentials.DependencyInjection.Generators;

internal sealed class ServiceDependency(string parameterName, string serviceType, string? openServiceType, string? key, bool inheritsKey)
{
    public string ParameterName { get; } = parameterName;

    public string ServiceType { get; } = serviceType;

    public string? OpenServiceType { get; } = openServiceType;

    public string? Key { get; } = key;

    public bool InheritsKey { get; } = inheritsKey;
}
