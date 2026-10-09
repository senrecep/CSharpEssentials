using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// TapErrorAsync across source flavours (instance, Task, ValueTask) and handler flavours (Task, ValueTask).
/// </summary>
[MemoryDiagnoser]
public class TapErrorBenchmarks
{
    private Func<Error[], Task> _taskHandler = null!;
#if NET9_0_OR_GREATER
    private Func<Error[], ValueTask> _valueTaskHandler = null!;
#endif

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _taskHandler = _ => BenchmarkSources.TaskWork(yield);
#if NET9_0_OR_GREATER
        _valueTaskHandler = _ => BenchmarkSources.ValueTaskWork(yield);
#endif
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<int>> HandWritten_TaskSource_TaskHandler()
    {
        Result<int> result = await BenchmarkSources.TaskOf(Succeed, Yield).ConfigureAwait(false);
        if (result.IsFailure)
            await _taskHandler(result.Errors).ConfigureAwait(false);
        return result;
    }

    [Benchmark]
    public Task<Result<int>> TaskSource_TaskHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).TapErrorAsync(_taskHandler);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_TaskHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).TapErrorAsync(_taskHandler);

    [Benchmark]
    public Task<Result<int>> Instance_TaskHandler() =>
        BenchmarkSources.Plain(Succeed).TapErrorAsync(_taskHandler);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_ValueTaskHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).TapErrorAsync(_valueTaskHandler);

    [Benchmark]
    public ValueTask<Result<int>> Instance_ValueTaskHandler() =>
        BenchmarkSources.Plain(Succeed).TapErrorAsync(_valueTaskHandler);
#endif
}
