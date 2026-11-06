namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>A way of decorating the same 10 of the 50 manually registered services.</summary>
public enum DecorationVariant
{
    Manual,
    CseGenerated,
    CseRuntime,
    Scrutor,
}
