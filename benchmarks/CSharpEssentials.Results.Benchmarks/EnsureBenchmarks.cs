using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// EnsureAsync across source flavours (instance, Task, ValueTask) and predicate flavours (sync, Task, ValueTask).
/// </summary>
[MemoryDiagnoser]
public class EnsureBenchmarks
{
    private Func<int, Task<bool>> _taskPredicate = null!;
#if NET9_0_OR_GREATER
    private Func<int, ValueTask<bool>> _valueTaskPredicate = null!;
#endif

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _taskPredicate = v => BenchmarkSources.TaskBool(v > 0, yield);
#if NET9_0_OR_GREATER
        _valueTaskPredicate = v => BenchmarkSources.ValueTaskBool(v > 0, yield);
#endif
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<int>> HandWritten_TaskSource_TaskPredicate()
    {
        Result<int> result = await BenchmarkSources.TaskOf(Succeed, Yield).ConfigureAwait(false);
        if (result.IsFailure)
            return result;
        return await _taskPredicate(result.Value).ConfigureAwait(false) ? result : BenchmarkSources.Failure;
    }

    [Benchmark]
    public Task<Result<int>> TaskSource_SyncPredicate() =>
        BenchmarkSources.TaskOf(Succeed, Yield).EnsureAsync(static v => v > 0, BenchmarkSources.Failure);

    [Benchmark]
    public Task<Result<int>> TaskSource_TaskPredicate() =>
        BenchmarkSources.TaskOf(Succeed, Yield).EnsureAsync(_taskPredicate, BenchmarkSources.Failure);

    [Benchmark]
    public Task<Result<int>> Instance_TaskPredicate() =>
        BenchmarkSources.Plain(Succeed).EnsureAsync(_taskPredicate, BenchmarkSources.Failure);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<Result<int>> ValueTaskSource_ValueTaskPredicate() =>
        BenchmarkSources.ValueTaskOf(Succeed, Yield).EnsureAsync(_valueTaskPredicate, BenchmarkSources.Failure);

    [Benchmark]
    public ValueTask<Result<int>> Instance_ValueTaskPredicate() =>
        BenchmarkSources.Plain(Succeed).EnsureAsync(_valueTaskPredicate, BenchmarkSources.Failure);
#endif
}
