namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>A way of registering the same N scoped services.</summary>
public enum RegistrationVariant
{
    Baseline,
    Manual,
    CseGenerated,
    CseReflection,
    Scrutor,
}
