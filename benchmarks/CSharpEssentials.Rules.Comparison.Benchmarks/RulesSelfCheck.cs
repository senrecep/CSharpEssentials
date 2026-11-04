namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>Runs every class's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class RulesSelfCheck
{
    public static void VerifyAll()
    {
        new MixedAsyncTreeComparison().Setup();
        new ShortCircuitComparison().Setup();
        foreach (int count in new[] { 5, 10 })
            new LinearBuildComparison { Count = count }.Setup();
        foreach (RuleInput input in new[] { RuleInput.Pass, RuleInput.FailLast })
        {
            new ConditionalComparison { Input = input }.Setup();
            new ReferenceEngineComparison { Input = input }.Setup();
            foreach (TreeShape shape in new[] { TreeShape.Linear5, TreeShape.And5, TreeShape.Or5 })
                new SyncTreeComparison { Shape = shape, Input = input }.Setup();
        }
    }
}
