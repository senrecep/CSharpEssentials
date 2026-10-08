using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// TapAsync across source flavours, comparing the synchronous <see cref="Action{T}"/> handler with the awaited
/// <c>Func&lt;T, Task&gt;</c> and <c>Func&lt;T, ValueTask&gt;</c> handlers.
/// </summary>
[MemoryDiagnoser]
public class TapBenchmarks
{
    private Action<int> _action = null!;
    private Func<int, Task> _taskHandler = null!;
    private Func<int, ValueTask> _valueTaskHandler = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _action = v => Sink = v;
        _taskHandler = _ => BenchmarkSources.TaskWork(yield);
        _valueTaskHandler = _ => BenchmarkSources.ValueTaskWork(yield);
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<int>> HandWritten_TaskSource_TaskHandler()
    {
        Result<int> result = await BenchmarkSources.TaskOf(Succeed, Yield).ConfigureAwait(false);
        if (result.IsSuccess)
            await _taskHandler(result.Value).ConfigureAwait(false);
        return result;
    }

    [Benchmark]
    public Task<Result<int>> TaskSource_Action() =>
        BenchmarkSources.TaskOf(Succeed, Yield).TapAsync(_action);

    [Benchmark]
    public Task<Result<int>> TaskSource_TaskHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).TapAsync(_taskHandler);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_Action() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).TapAsync(_action);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_ValueTaskHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).TapAsync(_valueTaskHandler);

    [Benchmark]
    public Task<Result<int>> Instance_TaskHandler() =>
        BenchmarkSources.Plain(Succeed).TapAsync(_taskHandler);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<Result<int>> Instance_ValueTaskHandler() =>
        BenchmarkSources.Plain(Succeed).TapAsync(_valueTaskHandler);
#endif

    public int Sink { get; private set; }
}
