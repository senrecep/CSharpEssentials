using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// BindAsync across source flavours (instance, Task, ValueTask) and handler flavours (sync, Task, ValueTask).
/// </summary>
[MemoryDiagnoser]
public class BindBenchmarks
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
    public async Task<Result<int>> HandWritten_TaskSource_TaskHandler()
    {
        Result<int> result = await BenchmarkSources.TaskOf(Succeed, Yield).ConfigureAwait(false);
        if (result.IsFailure)
            return result;
        return await _taskHandler(result.Value).ConfigureAwait(false);
    }

    [Benchmark]
    public Task<Result<int>> TaskSource_SyncHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).BindAsync(static v => Result<int>.Success(v + 1));

    [Benchmark]
    public Task<Result<int>> TaskSource_TaskHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).BindAsync(_taskHandler);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_ValueTaskHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).BindAsync(_valueTaskHandler);

    [Benchmark]
    public Task<Result<int>> Instance_TaskHandler() =>
        BenchmarkSources.Plain(Succeed).BindAsync(_taskHandler);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<Result<int>> Instance_ValueTaskHandler() =>
        BenchmarkSources.Plain(Succeed).BindAsync(_valueTaskHandler);
#endif
}
