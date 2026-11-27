using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// The synchronous chain Map <c>x * 2</c>, Bind <c>x + 1</c>, Bind <c>x + 1</c> on <see cref="Result{TValue}"/> (baseline) and on
/// the typed result with one, two and three error slots. On failure the source holds an error in its last slot, which every
/// step passes through. The rows show what the typed layout and slot dispatch cost against the classic result.
/// </summary>
[MemoryDiagnoser]
public class TypedBindBenchmarks
{
    private Result<int> _result;
    private Result<int, NotFoundError> _one;
    private Result<int, NotFoundError, ConflictError> _two;
    private Result<int, NotFoundError, ConflictError, ValidationError> _three;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _result = BenchmarkSources.Plain(Succeed);
        _one = TypedSources.One(Succeed);
        _two = TypedSources.Two(Succeed);
        _three = TypedSources.Three(Succeed);
    }

    [Benchmark(Baseline = true)]
    public Result<int> Result() => _result
        .Map(static x => x * 2)
        .Bind(static x => (Result<int>)(x + 1))
        .Bind(static x => (Result<int>)(x + 1));

    [Benchmark]
    public Result<int, NotFoundError> Typed_OneError() => _one
        .Map(static x => x * 2)
        .Bind(static x => (Result<int, NotFoundError>)(x + 1))
        .Bind(static x => (Result<int, NotFoundError>)(x + 1));

    [Benchmark]
    public Result<int, NotFoundError, ConflictError> Typed_TwoErrors() => _two
        .Map(static x => x * 2)
        .Bind(static x => (Result<int, NotFoundError, ConflictError>)(x + 1))
        .Bind(static x => (Result<int, NotFoundError, ConflictError>)(x + 1));

    [Benchmark]
    public Result<int, NotFoundError, ConflictError, ValidationError> Typed_ThreeErrors() => _three
        .Map(static x => x * 2)
        .Bind(static x => (Result<int, NotFoundError, ConflictError, ValidationError>)(x + 1))
        .Bind(static x => (Result<int, NotFoundError, ConflictError, ValidationError>)(x + 1));
}
