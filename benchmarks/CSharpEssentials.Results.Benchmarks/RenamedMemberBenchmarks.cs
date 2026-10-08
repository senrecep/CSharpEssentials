using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// The obsolete pre-6.4 names against their <c>Async</c> replacements; the forwarders should add no measurable cost.
/// CS0618 is silenced for this file in .editorconfig because calling the obsolete names is the point.
/// </summary>
[MemoryDiagnoser]
public class RenamedMemberBenchmarks
{
    private Func<int, Task<Result<int>>> _taskHandler = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _taskHandler = v => BenchmarkSources.TaskResult(v + 1, yield);
    }

    [Benchmark(Baseline = true)]
    public Task<int> TaskSource_MatchAsync() =>
        BenchmarkSources.TaskOf(Succeed, Yield).MatchAsync(static v => v, static errors => errors.Length);

    [Benchmark]
    public Task<int> TaskSource_Match_Obsolete() =>
        BenchmarkSources.TaskOf(Succeed, Yield).Match(static v => v, static errors => errors.Length);

    [Benchmark]
    public Task<Result<int>> Instance_BindAsync() =>
        BenchmarkSources.Plain(Succeed).BindAsync(_taskHandler);

    [Benchmark]
    public Task<Result<int>> Instance_Bind_Obsolete() =>
        BenchmarkSources.Plain(Succeed).Bind(_taskHandler);
}
