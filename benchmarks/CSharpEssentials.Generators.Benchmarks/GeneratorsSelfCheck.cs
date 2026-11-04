namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// Runs every comparison's [GlobalSetup] parity check for every Files value without BenchmarkDotNet, then prints the
/// incremental step report for every generator row. Throws on the first mismatch.
/// </summary>
public static class GeneratorsSelfCheck
{
    public static void VerifyAll()
    {
        foreach (int files in new[] { 200, 1000 })
        {
            DependencyInjectionGeneratorComparison dependencyInjection = new() { Files = files };
            dependencyInjection.Setup();
            Report(nameof(DependencyInjectionGeneratorComparison), files, dependencyInjection.Rows());

            EnumsGeneratorComparison enums = new() { Files = files };
            enums.Setup();
            Report(nameof(EnumsGeneratorComparison), files, enums.Rows());

            new QueryFilterAnalyzerComparison { Files = files }.Setup();

            EndpointsGeneratorComparison endpoints = new() { Files = files };
            endpoints.Setup();
            Report(nameof(EndpointsGeneratorComparison), files, endpoints.Rows());
        }
    }

    private static void Report(string scenario, int files, IEnumerable<GeneratorRow> rows)
    {
        foreach (GeneratorRow row in rows)
        {
            IncrementalReport.Print(scenario, files, row);
        }
    }
}
