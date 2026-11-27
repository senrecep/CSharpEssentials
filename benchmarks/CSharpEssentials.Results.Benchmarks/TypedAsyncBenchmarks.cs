using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// <c>BindAsync</c> on <c>Result&lt;int, NotFoundError, ConflictError, ValidationError&gt;</c> across the source flavours
/// (instance, Task, ValueTask) and handler flavours (sync, Task, ValueTask) of <see cref="BindBenchmarks"/>, against the same
/// hand-written <c>await</c> plus <c>if</c> baseline written for the typed result. Compare a row with its
/// <see cref="BindBenchmarks"/> twin to see the typed cost over <see cref="Result{TValue}"/>. The rows named after another
/// combinator (Map, Ensure, Tap, MapError, Compensate) measure that combinator's extension on the named source with the named
/// handler. A failure holds the error in the last slot, so <c>MapErrorAsync</c> and <c>CompensateAsync</c> call their handler on
/// it, and on success <c>EnsureAsync</c> and <c>TapAsync</c> call theirs.
/// </summary>
[MemoryDiagnoser]
public class TypedAsyncBenchmarks
{
    private Func<int, Task<Result<int, NotFoundError, ConflictError, ValidationError>>> _taskHandler = null!;
    private Func<int, ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>>> _valueTaskHandler = null!;
    private Func<int, Task<int>> _mapHandler = null!;
    private Func<int, Task<bool>> _predicate = null!;
    private Func<int, Task> _tapHandler = null!;
    private Func<ValidationError, Task<ValidationError>> _mapErrorHandler = null!;
    private Func<ValidationError, Task<Result<int, NotFoundError, ConflictError>>> _compensateHandler = null!;

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
        _predicate = v => BenchmarkSources.TaskBool(v > 0, yield);
        _tapHandler = _ => BenchmarkSources.TaskWork(yield);
        _mapErrorHandler = e => TypedSources.Completed(e, yield);
        _compensateHandler = _ => TypedSources.Completed<Result<int, NotFoundError, ConflictError>>(0, yield);
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
    public Task<Result<int, NotFoundError, ConflictError, ValidationError>> TaskSource_EnsureTaskHandler() =>
        TypedSources.TaskOf(Succeed, Yield).EnsureAsync(_predicate, TypedSources.Missing);

    [Benchmark]
    public Task<Result<int, NotFoundError, ConflictError, ValidationError>> TaskSource_TapTaskHandler() =>
        TypedSources.TaskOf(Succeed, Yield).TapAsync(_tapHandler);

    [Benchmark]
    public Task<Result<int, NotFoundError, ConflictError, ValidationError>> TaskSource_MapErrorTaskHandler() =>
        TypedSources.TaskOf(Succeed, Yield).MapErrorAsync(_mapErrorHandler);

    [Benchmark]
    public Task<Result<int, NotFoundError, ConflictError>> TaskSource_CompensateTaskHandler() =>
        TypedSources.TaskOf(Succeed, Yield).CompensateAsync(_compensateHandler);

    [Benchmark]
    public ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> ValueTaskSource_SyncHandler() =>
        TypedSources.ValueTaskOf(Succeed, Yield).BindAsync(static v => (Result<int, NotFoundError, ConflictError, ValidationError>)(v + 1));

    [Benchmark]
    public ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> ValueTaskSource_MapSyncHandler() =>
        TypedSources.ValueTaskOf(Succeed, Yield).MapAsync(static v => v + 1);

    [Benchmark]
    public ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> ValueTaskSource_EnsureTaskHandler() =>
        TypedSources.ValueTaskOf(Succeed, Yield).EnsureAsync(_predicate, TypedSources.Missing);

    [Benchmark]
    public ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> ValueTaskSource_MapErrorSyncHandler() =>
        TypedSources.ValueTaskOf(Succeed, Yield).MapErrorAsync(static (ValidationError e) => e);

    [Benchmark]
    public ValueTask<Result<int, NotFoundError, ConflictError>> ValueTaskSource_CompensateTaskHandler() =>
        TypedSources.ValueTaskOf(Succeed, Yield).CompensateAsync(_compensateHandler);

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
