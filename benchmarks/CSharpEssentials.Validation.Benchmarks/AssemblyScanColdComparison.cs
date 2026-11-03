using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// The same scan as <see cref="AssemblyScanComparison"/>, timed as the first invocation in a fresh process: JIT, assembly loading
/// and each library's reflection over the assembly are inside the time. The Allocated column comes from BenchmarkDotNet's
/// extra-stats call, which runs the method a second time in the same process, so it shows warm allocations. There is no setup
/// step that could warm a variant first; <see cref="AssemblyScanComparison"/> runs the self-check.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 15, warmupCount: 0, iterationCount: 1, id: "cold")]
public class AssemblyScanColdComparison
{
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#di-registration
    [Benchmark(Baseline = true)]
    public int CSharpEssentials() => AssemblyScanComparison.CseServices().Count;

    // https://docs.fluentvalidation.net/en/latest/di.html#automatic-registration
    [Benchmark]
    public int FluentValidation() => AssemblyScanComparison.FvServices().Count;
}
