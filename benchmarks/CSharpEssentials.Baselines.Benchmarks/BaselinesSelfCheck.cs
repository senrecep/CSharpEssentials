namespace CSharpEssentials.Baselines.Benchmarks;

/// <summary>Runs every comparison's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class BaselinesSelfCheck
{
    public static void VerifyAll()
    {
        new ClockComparison().Setup();
        new TimeConversionComparison().Setup();
        foreach (int count in new[] { 10, 1000 })
            new CloneComparison { Count = count }.Setup();
    }
}
