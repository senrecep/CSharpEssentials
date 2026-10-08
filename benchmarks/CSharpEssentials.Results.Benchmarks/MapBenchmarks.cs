using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// MapAsync across source flavours (instance, Task, ValueTask) and handler flavours (sync, Task, ValueTask).
/// </summary>
[MemoryDiagnoser]
public class MapBenchmarks
{
    private Func<int, Task<int>> _taskHandler = null!;
    private Func<int, ValueTask<int>> _valueTaskHandler = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _taskHandler = v => BenchmarkSources.TaskValue(v + 1, yield);
        _valueTaskHandler = v => BenchmarkSources.ValueTaskValue(v + 1, yield);
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<int>> HandWritten_TaskSource_TaskHandler()
    {
        Result<int> result = await BenchmarkSources.TaskOf(Succeed, Yield).ConfigureAwait(false);
        if (result.IsFailure)
            return result.Errors;
        return await _taskHandler(result.Value).ConfigureAwait(false);
    }

    [Benchmark]
    public Task<Result<int>> TaskSource_SyncHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).MapAsync(static v => v + 1);

    [Benchmark]
    public Task<Result<int>> TaskSource_TaskHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).MapAsync(_taskHandler);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_SyncHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).MapAsync(static v => v + 1);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_ValueTaskHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).MapAsync(_valueTaskHandler);

    [Benchmark]
    public Task<Result<int>> Instance_TaskHandler() =>
        BenchmarkSources.Plain(Succeed).MapAsync(_taskHandler);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<Result<int>> Instance_ValueTaskHandler() =>
        BenchmarkSources.Plain(Succeed).MapAsync(_valueTaskHandler);
#endif
}
