using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// ElseAsync with a value fallback across source flavours (instance, Task, ValueTask) and handler flavours (sync, Task, ValueTask).
/// </summary>
[MemoryDiagnoser]
public class ElseBenchmarks
{
    private Func<Error[], Task<int>> _taskFallback = null!;
#if NET9_0_OR_GREATER
    private Func<Error[], ValueTask<int>> _valueTaskFallback = null!;
#endif

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _taskFallback = errors => BenchmarkSources.TaskValue(errors.Length, yield);
#if NET9_0_OR_GREATER
        _valueTaskFallback = errors => BenchmarkSources.ValueTaskValue(errors.Length, yield);
#endif
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<int>> HandWritten_TaskSource_TaskHandler()
    {
        Result<int> result = await BenchmarkSources.TaskOf(Succeed, Yield).ConfigureAwait(false);
        return result.IsSuccess ? result : await _taskFallback(result.Errors).ConfigureAwait(false);
    }

    [Benchmark]
    public Task<Result<int>> TaskSource_SyncHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).ElseAsync(static errors => errors.Length);

    [Benchmark]
    public Task<Result<int>> TaskSource_TaskHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).ElseAsync(_taskFallback);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_SyncHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).ElseAsync(static errors => errors.Length);

    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_TaskHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).ElseAsync(_taskFallback);

    [Benchmark]
    public Task<Result<int>> Instance_TaskHandler() =>
        BenchmarkSources.Plain(Succeed).ElseAsync(_taskFallback);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_ValueTaskHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).ElseAsync(_valueTaskFallback);

    [Benchmark]
    public ValueTask<Result<int>> Instance_ValueTaskHandler() =>
        BenchmarkSources.Plain(Succeed).ElseAsync(_valueTaskFallback);
#endif
}
