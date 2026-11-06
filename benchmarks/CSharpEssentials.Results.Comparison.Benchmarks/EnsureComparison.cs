using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Ensure <c>x &gt; 0</c> on a success holding 42 (passes) or -1 (fails and creates the error), so the failure case
/// measures the library turning a success into a failure. ErrorOr uses <c>FailIf</c> with the inverted predicate.
/// FluentResults has no column: it has no Ensure, and its documented alternatives either drop the value
/// (<c>Bind(x =&gt; Result.OkIf(...))</c> returns the non-generic Result) or are a hand-written Bind.
/// </summary>
[MemoryDiagnoser]
public class EnsureComparison
{
    private Result<int> _cse;
    private Cfe.Result<int> _cfe;
    private Eo.ErrorOr<int> _errorOr;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        int value = Succeed ? Inputs.Value : -1;
        _cse = value;
        _cfe = value;
        _errorOr = value;
        Parity.Same(nameof(EnsureComparison),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions())),
            (nameof(ErrorOr), Parity.Of(ErrorOr())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#ensuring
    [Benchmark(Baseline = true)]
    public Result<int> CSharpEssentials() => _cse.Ensure(static x => x > 0, Inputs.CseError);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Ensure.cs
    [Benchmark]
    public Cfe.Result<int> CSharpFunctionalExtensions() => _cfe.Ensure(static x => x > 0, Inputs.CfeError);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#failif
    [Benchmark]
    public Eo.ErrorOr<int> ErrorOr() => _errorOr.FailIf(static x => x <= 0, Inputs.ErrorOrError);
}
