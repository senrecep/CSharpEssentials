namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>Runs every comparison's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class EnumsSelfCheck
{
    public static void VerifyAll()
    {
        new ToStringComparison().Setup();
        new GetValuesComparison().Setup();
        new FlagsComparison().Setup();
        new DescriptionComparison().Setup();
        foreach (bool flag in new[] { true, false })
        {
            new ParseComparison { Valid = flag }.Setup();
            new IsDefinedComparison { Defined = flag }.Setup();
        }
    }
}
