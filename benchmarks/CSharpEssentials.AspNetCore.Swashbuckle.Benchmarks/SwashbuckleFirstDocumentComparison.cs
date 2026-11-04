using BenchmarkDotNet.Engines;

namespace CSharpEssentials.AspNetCore.Swashbuckle.Benchmarks;

/// <summary>
/// Time from a fresh process to the first document: builder, services, Build, mapping, TestServer start, one
/// <c>GET /swagger/v1/swagger.json</c>, stop, dispose. Each sample is the first and only invocation in a new process, so JIT,
/// the Swashbuckle generator and the CSE per-provider caches are all cold inside the measurement. There is no setup
/// step. The hosts are the ones of <see cref="SwashbuckleDocumentComparison"/> (<see cref="SwashbuckleHosts"/>); the self-check
/// builds each once and requires the same document hash as the warm row. The CSE row writes more output (70 operations,
/// enum components, idempotency parts).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 15, warmupCount: 0, iterationCount: 1, id: "cold")]
public class SwashbuckleFirstDocumentComparison
{
    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v10.3.1/README.md#getting-started
    [Benchmark(Baseline = true)]
    public Task<int> Plain_AddSwaggerGen() => ApiDocsHost.FirstDocumentAsync(SwashbuckleHosts.Plain);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD
    [Benchmark]
    public Task<int> CSharpEssentials_AllConventions() => ApiDocsHost.FirstDocumentAsync(SwashbuckleHosts.AllConventions);
}
