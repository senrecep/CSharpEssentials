using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// Folds a result to an int: the value on success, a number per error otherwise. <see cref="Result{TValue}"/>'s
/// <c>Match</c> is the baseline; the typed rows call <c>Match</c> with one, two and three error slots, and the last two rows
/// fold the three-slot result with a C# switch expression and with <c>TryGetValue</c>. On failure the source holds an error in
/// its last slot.
/// </summary>
[MemoryDiagnoser]
public class TypedMatchBenchmarks
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
        int expected = Result();
        if (Typed_OneError() != expected || Typed_TwoErrors() != expected || Typed_ThreeErrors() != expected ||
            Typed_ThreeErrors_Switch() != expected || Typed_ThreeErrors_TryGetValue() != expected)
        {
            throw new InvalidOperationException($"{nameof(TypedMatchBenchmarks)}: the rows disagree for Succeed={Succeed}.");
        }
    }

    [Benchmark(Baseline = true)]
    public int Result() => _result.Match(static v => v, static errors => errors.Length);

    [Benchmark]
    public int Typed_OneError() => _one.Match(static v => v, static _ => 1);

    [Benchmark]
    public int Typed_TwoErrors() => _two.Match(static v => v, static _ => 2, static _ => 1);

    [Benchmark]
    public int Typed_ThreeErrors() => _three.Match(static v => v, static _ => 3, static _ => 2, static _ => 1);

    [Benchmark]
    public int Typed_ThreeErrors_Switch() => _three switch
    {
        int v => v,
        NotFoundError => 3,
        ConflictError => 2,
        ValidationError => 1,
    };

    [Benchmark]
    public int Typed_ThreeErrors_TryGetValue()
    {
        if (_three.TryGetValue(out int value))
            return value;
        if (_three.TryGetValue(out NotFoundError _))
            return 3;
        return _three.TryGetValue(out ConflictError _) ? 2 : 1;
    }
}
