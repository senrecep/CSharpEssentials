using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// <c>BindAsync</c> on <c>Result&lt;int, NotFoundError, ConflictError, ValidationError&gt;</c> across the source flavours
/// (instance, Task, ValueTask) and handler flavours (sync, Task, ValueTask) of <see cref="BindBenchmarks"/>, against the same
/// hand-written <c>await</c> plus <c>if</c> baseline written for the typed result. Compare a row with its
/// <see cref="BindBenchmarks"/> twin to see the typed cost over <see cref="Result{TValue}"/>. <c>TaskSource_MapTaskHandler</c>
/// covers the other Task-source families, which share its shape.
/// </summary>
[MemoryDiagnoser]
public class TypedAsyncBenchmarks
{
    private Func<int, Task<Result<int, NotFoundError, ConflictError, ValidationError>>> _taskHandler = null!;
    private Func<int, ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>>> _valueTaskHandler = null!;
    private Func<int, Task<int>> _mapHandler = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool yield = Yield;
        _taskHandler = v => TypedSources.TaskResult(v + 1, yield);
        _valueTaskHandler = v => TypedSources.ValueTaskResult(v + 1, yield);
        _mapHandler = v => TypedSources.TaskValue(v + 1, yield);
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<int, NotFoundError, ConflictError, ValidationError>> HandWritten_TaskSource_TaskHandler()
    {
        Result<int, NotFoundError, ConflictError, ValidationError> result = await TypedSources.TaskOf(Succeed, Yield).ConfigureAwait(false);
        if (!result.TryGetValue(out int value))
            return result;
        return await _taskHandler(value).ConfigureAwait(false);
    }

    [Benchmark]
    public Task<Result<int, NotFoundError, ConflictError, ValidationError>> TaskSource_SyncHandler() =>
        TypedSources.TaskOf(Succeed, Yield).BindAsync(static v => (Result<int, NotFoundError, ConflictError, ValidationError>)(v + 1));

    [Benchmark]
    public Task<Result<int, NotFoundError, ConflictError, ValidationError>> TaskSource_TaskHandler() =>
        TypedSources.TaskOf(Succeed, Yield).BindAsync(_taskHandler);

    [Benchmark]
    public Task<Result<int, NotFoundError, ConflictError, ValidationError>> TaskSource_MapTaskHandler() =>
        TypedSources.TaskOf(Succeed, Yield).MapAsync(_mapHandler);

    [Benchmark]
    public ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> ValueTaskSource_ValueTaskHandler() =>
        TypedSources.ValueTaskOf(Succeed, Yield).BindAsync(_valueTaskHandler);

    [Benchmark]
    public Task<Result<int, NotFoundError, ConflictError, ValidationError>> Instance_TaskHandler() =>
        TypedSources.Three(Succeed).BindAsync(_taskHandler);

    [Benchmark]
    public ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> Instance_ValueTaskHandler() =>
        TypedSources.Three(Succeed).BindAsync(_valueTaskHandler);
}
