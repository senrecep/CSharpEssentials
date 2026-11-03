namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>Runs every comparison's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class AnySelfCheck
{
    public static void VerifyAll()
    {
        new ConstructComparison().Setup();
        new PartitionComparison().Setup();
        new ThesePartitionComparison().Setup();
        foreach (string branch in new[] { Inputs.IntBranch, Inputs.TextBranch })
        {
            new MatchComparison { Branch = branch }.Setup();
            new TryPickComparison { Branch = branch }.Setup();
            new JsonComparison { Branch = branch }.Setup();
            new ToStringComparison { Branch = branch }.Setup();
        }

        foreach (TheseState state in Enum.GetValues<TheseState>())
        {
            new TheseMatchComparison { State = state }.Setup();
            new ThesePipelineComparison { State = state }.Setup();
            new TheseJsonComparison { State = state }.Setup();
        }
    }
}
