namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>A way of registering the same N scoped services from a T{N} library with TryAdd semantics.</summary>
public enum TryAddVariant
{
    ManualTryAdd,
    CseGenerated,
    CseReflection,
    Scrutor,
    ManualAdd,
}
