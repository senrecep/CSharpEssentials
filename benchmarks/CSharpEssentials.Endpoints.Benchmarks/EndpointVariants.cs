namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>The variants that run on the current target framework.</summary>
public static class EndpointVariants
{
    /// <summary>Every variant that maps endpoints, without <see cref="EndpointVariant.Baseline"/>.</summary>
    public static IReadOnlyList<EndpointVariant> Mapping { get; } =
    [
        EndpointVariant.Plain,
        EndpointVariant.CseGenerated,
        EndpointVariant.CseReflection,
#if NET10_0_OR_GREATER
        EndpointVariant.Carter,
#endif
        EndpointVariant.FastEndpoints,
        EndpointVariant.FastEndpointsSourceGen,
    ];
}
