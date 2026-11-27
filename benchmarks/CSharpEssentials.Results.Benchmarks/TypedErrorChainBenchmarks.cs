using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// The synchronous error-side chain MapError, Recover and Compensate on <see cref="Result{TValue}"/> (baseline) and on the typed
/// result with one, two and three error slots. On failure the source holds an error in its last slot: MapError maps that
/// error, the slot-selecting step in the middle passes it through, and the last step turns it into the success value 0. On
/// success every step passes the value through. The one-slot chain has no middle step, because Recover on one slot returns
/// the value itself. The two- and three-slot rows run MapError, Recover and Compensate on two and three slots, which switch
/// over three or more tags.
/// </summary>
[MemoryDiagnoser]
public class TypedErrorChainBenchmarks
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
        .MapError(static (Error e) => e)
        .Recover(ErrorType.NotFound, static (Error _) => 0)
        .Compensate(static _ => 0);

    [Benchmark]
    public Result<int, NotFoundError> Typed_OneError() => _one
        .MapError(static (NotFoundError e) => e)
        .Compensate(static (NotFoundError _) => 0);

    [Benchmark]
    public Result<int, ConflictError> Typed_TwoErrors() => _two
        .MapError(static (ConflictError e) => e)
        .Recover(static (NotFoundError _) => 0)
        .Compensate(static (ConflictError _) => 0);

    [Benchmark]
    public Result<int, ConflictError> Typed_ThreeErrors() => _three
        .MapError(static (ValidationError e) => e)
        .Compensate(static (NotFoundError _) => (Result<int, ConflictError, ValidationError>)0)
        .Recover(static (ValidationError _) => 0);
}
