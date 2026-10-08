using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// MatchAsync across source flavours (instance, Task, ValueTask) and handler flavours (sync, Task, ValueTask).
/// </summary>
[MemoryDiagnoser]
public class MatchBenchmarks
{
    private Func<int, Task<int>> _taskOnSuccess = null!;
    private Func<Error[], Task<int>> _taskOnFailure = null!;
#if NET9_0_OR_GREATER
    private Func<int, ValueTask<int>> _valueTaskOnSuccess = null!;
    private Func<Error[], ValueTask<int>> _valueTaskOnFailure = null!;
#endif

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _taskOnSuccess = v => BenchmarkSources.TaskValue(v, yield);
        _taskOnFailure = errors => BenchmarkSources.TaskValue(errors.Length, yield);
#if NET9_0_OR_GREATER
        _valueTaskOnSuccess = v => BenchmarkSources.ValueTaskValue(v, yield);
        _valueTaskOnFailure = errors => BenchmarkSources.ValueTaskValue(errors.Length, yield);
#endif
    }

    [Benchmark(Baseline = true)]
    public async Task<int> HandWritten_TaskSource_TaskHandler()
    {
        Result<int> result = await BenchmarkSources.TaskOf(Succeed, Yield).ConfigureAwait(false);
        return result.IsSuccess
            ? await _taskOnSuccess(result.Value).ConfigureAwait(false)
            : await _taskOnFailure(result.Errors).ConfigureAwait(false);
    }

    [Benchmark]
    public Task<int> TaskSource_SyncHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).MatchAsync(static v => v, static errors => errors.Length);

    [Benchmark]
    public Task<int> TaskSource_TaskHandler() =>
        BenchmarkSources.TaskOf(Succeed, Yield).MatchAsync(_taskOnSuccess, _taskOnFailure);

    [Benchmark]
    public ValueTask<int> ValueTaskSource_SyncHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).MatchAsync(static v => v, static errors => errors.Length);

    [Benchmark]
    public Task<int> Instance_TaskHandler() =>
        BenchmarkSources.Plain(Succeed).MatchAsync(_taskOnSuccess, _taskOnFailure);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<int> ValueTaskSource_ValueTaskHandler() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).MatchAsync(_valueTaskOnSuccess, _valueTaskOnFailure);

    [Benchmark]
    public ValueTask<int> Instance_ValueTaskHandler() =>
        BenchmarkSources.Plain(Succeed).MatchAsync(_valueTaskOnSuccess, _valueTaskOnFailure);
#endif
}
