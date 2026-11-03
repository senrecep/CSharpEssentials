namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>Runs the [GlobalSetup] checks of the naming and empty-prefix group benchmarks, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class EndpointsExtSelfCheck
{
    public static void VerifyAll()
    {
        new OperationNamingBenchmarks().Setup().GetAwaiter().GetResult();
        OperationNameReadBenchmarks read = new();
        read.Setup();
        read.Cleanup();
        new EmptyPrefixGroupBenchmarks().Setup().GetAwaiter().GetResult();
    }
}
