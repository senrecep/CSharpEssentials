using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// TraverseAsync over <see cref="Count"/> items against a hand-written sequential loop. On failure the middle item fails.
/// </summary>
[MemoryDiagnoser]
public class TraverseBenchmarks
{
    private int[] _items = [];
    private Func<int, Task<Result<int>>> _taskSelector = null!;
#if NET9_0_OR_GREATER
    private Func<int, ValueTask<Result<int>>> _valueTaskSelector = null!;
#endif

    [Params(16, 256)]
    public int Count { get; set; }

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _items = new int[Count];
        for (int i = 0; i < _items.Length; i++)
            _items[i] = i;

        bool yield = Yield;
        int failAt = Succeed ? -1 : Count / 2;
        _taskSelector = v => v == failAt ? Task.FromResult<Result<int>>(BenchmarkSources.Failure) : BenchmarkSources.TaskResult(v, yield);
#if NET9_0_OR_GREATER
        _valueTaskSelector = v => v == failAt ? new ValueTask<Result<int>>(BenchmarkSources.Failure) : BenchmarkSources.ValueTaskResult(v, yield);
#endif
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<int[]>> HandWritten_SequentialLoop()
    {
        int[] values = new int[_items.Length];
        for (int i = 0; i < _items.Length; i++)
        {
            Result<int> result = await _taskSelector(_items[i]).ConfigureAwait(false);
            if (result.IsFailure)
                return result.Errors;
            values[i] = result.Value;
        }
        return values;
    }

    [Benchmark]
    public Task<Result<int[]>> TraverseAsync_TaskSelector() =>
        _items.TraverseAsync(_taskSelector);

#if NET9_0_OR_GREATER
    [Benchmark]
    public ValueTask<Result<int[]>> TraverseAsync_ValueTaskSelector() =>
        _items.TraverseAsync(_valueTaskSelector);
#endif

    [Benchmark]
    public Task<Result<int[]>> SequenceAsync_Tasks()
    {
        var pending = new Task<Result<int>>[_items.Length];
        for (int i = 0; i < _items.Length; i++)
            pending[i] = _taskSelector(_items[i]);
        return pending.SequenceAsync();
    }
}
