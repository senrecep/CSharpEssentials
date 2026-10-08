using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// ThenAsync is an alias of BindAsync; these rows show the alias costs the same as the operation it forwards to.
/// </summary>
[MemoryDiagnoser]
public class ThenBenchmarks
{
    private Func<int, Task<Result<int>>> _taskHandler = null!;
    private Func<int, ValueTask<Result<int>>> _valueTaskHandler = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _taskHandler = v => BenchmarkSources.TaskResult(v + 1, yield);
        _valueTaskHandler = v => BenchmarkSources.ValueTaskResult(v + 1, yield);
    }

    [Benchmark(Baseline = true)]
    public Task<Result<int>> TaskSource_BindAsync() =>
        BenchmarkSources.TaskOf(Succeed, Yield).BindAsync(_taskHandler);

    [Benchmark]
    public Task<Result<int>> TaskSource_ThenAsync() =>
        BenchmarkSources.TaskOf(Succeed, Yield).ThenAsync(_taskHandler);

    [Benchmark]
    public Task<Result<int>> Instance_ThenAsync_TaskHandler() =>
        BenchmarkSources.Plain(Succeed).ThenAsync(_taskHandler);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_BindAsync() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).BindAsync(_valueTaskHandler);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_ThenAsync() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).ThenAsync(_valueTaskHandler);

    [Benchmark]
    public ValueTask<Result<int>> Instance_ThenAsync_ValueTaskHandler() =>
        BenchmarkSources.Plain(Succeed).ThenAsync(_valueTaskHandler);
#endif
}
