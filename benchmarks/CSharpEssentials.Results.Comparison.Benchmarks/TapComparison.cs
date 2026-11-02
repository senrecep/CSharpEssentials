using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Tap a side effect (write the value into a field) on a success or failure built in setup. The action captures
/// <c>this</c>, so one cached delegate is shared by every library. ErrorOr uses <c>ThenDo</c>. FluentResults has no
/// column: it has no Tap or equivalent side-effect method.
/// </summary>
[MemoryDiagnoser]
public class TapComparison
{
    private Result<int> _cse;
    private Cfe.Result<int> _cfe;
    private Eo.ErrorOr<int> _errorOr;
    private Action<int> _tap = null!;
    private int _sink;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _tap = x => _sink = x;
        _cse = Inputs.Cse(Succeed);
        _cfe = Inputs.CfeOf(Succeed);
        _errorOr = Inputs.ErrorOr(Succeed);
        Parity.Same(nameof(TapComparison),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials()) + Sink()),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions()) + Sink()),
            (nameof(ErrorOr), Parity.Of(ErrorOr()) + Sink()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#chaining
    [Benchmark(Baseline = true)]
    public Result<int> CSharpEssentials() => _cse.Tap(_tap);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Tap.cs
    [Benchmark]
    public Cfe.Result<int> CSharpFunctionalExtensions() => _cfe.Tap(_tap);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#thendo-and-thendoasync
    [Benchmark]
    public Eo.ErrorOr<int> ErrorOr() => _errorOr.ThenDo(_tap);

    /// <summary>Reads and resets the sink so the parity check also proves the side effect ran exactly on success.</summary>
    private string Sink()
    {
        int sink = _sink;
        _sink = 0;
        return "|sink:" + sink.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
