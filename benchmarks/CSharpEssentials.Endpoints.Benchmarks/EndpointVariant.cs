namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// A way of mapping the same endpoints. Carter 10 targets net10.0 only, so its rows exist on net10.0 and later.
/// </summary>
public enum EndpointVariant
{
    Baseline,
    Plain,
    CseGenerated,
    CseReflection,
    Carter,
    FastEndpoints,
    FastEndpointsSourceGen,
}
