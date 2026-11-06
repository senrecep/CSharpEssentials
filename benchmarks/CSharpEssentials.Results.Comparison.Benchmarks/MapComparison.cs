using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Map <c>x => x * 2</c> over a success or failure built in setup, so only the operation is timed. ErrorOr maps with
/// the <c>Then(Func&lt;T, TNext&gt;)</c> overload.
/// </summary>
[MemoryDiagnoser]
public class MapComparison
{
    private Result<int> _cse;
    private Cfe.Result<int> _cfe;
    private Eo.ErrorOr<int> _errorOr;
    private Fr.Result<int> _fluent = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _cse = Inputs.Cse(Succeed);
        _cfe = Inputs.CfeOf(Succeed);
        _errorOr = Inputs.ErrorOr(Succeed);
        _fluent = Inputs.Fluent(Succeed);
        Parity.Same(nameof(MapComparison),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions())),
            (nameof(ErrorOr), Parity.Of(ErrorOr())),
            (nameof(FluentResults), Parity.Of(FluentResults())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#chaining
    [Benchmark(Baseline = true)]
    public Result<int> CSharpEssentials() => _cse.Map(static x => x * 2);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#map-1
    [Benchmark]
    public Cfe.Result<int> CSharpFunctionalExtensions() => _cfe.Map(static x => x * 2);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#then-1
    [Benchmark]
    public Eo.ErrorOr<int> ErrorOr() => _errorOr.Then(static x => x * 2);

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#converting-and-transformation
    [Benchmark]
    public Fr.Result<int> FluentResults() => _fluent.Map(static x => x * 2);
}
