namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>Runs the [GlobalSetup] parity check of the boxed-format and reflection-info comparisons, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class EnumsExtSelfCheck
{
    public static void VerifyAll()
    {
        new BoxedFormatComparison().Setup();
        new ReflectionInfoComparison().Setup();
    }
}
