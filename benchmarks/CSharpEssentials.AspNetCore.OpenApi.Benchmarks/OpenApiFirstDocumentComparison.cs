using BenchmarkDotNet.Engines;

namespace CSharpEssentials.AspNetCore.OpenApi.Benchmarks;

/// <summary>
/// Time from a fresh process to the first document: builder, services, Build, mapping, TestServer start, one
/// <c>GET /openapi/v1.json</c>, stop, dispose. Each sample is the first and only invocation in a new process, so JIT,
/// the framework schema store and the CSE per-provider caches are all cold inside the measurement. There is no setup
/// step. The hosts are the ones of <see cref="OpenApiDocumentComparison"/> (<see cref="OpenApiHosts"/>); the self-check
/// builds each once and requires the same document hash as the warm row. The CSE row writes more output (70 operations,
/// enum components, idempotency parts).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 15, warmupCount: 0, iterationCount: 1, id: "cold")]
public class OpenApiFirstDocumentComparison
{
    // https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi
    [Benchmark(Baseline = true)]
    public Task<int> Plain_AddOpenApi() => ApiDocsHost.FirstDocumentAsync(OpenApiHosts.Plain);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.OpenApi/Readme.MD
    [Benchmark]
    public Task<int> CSharpEssentials_AllConventions() => ApiDocsHost.FirstDocumentAsync(OpenApiHosts.AllConventions);
}
