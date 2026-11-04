using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Create a success from a value or a failure from the cached error, through each library's documented implicit
/// conversion. CFE has no implicit string-to-result conversion, so its failure uses <c>Result.Failure&lt;T&gt;(string)</c>.
/// </summary>
[MemoryDiagnoser]
public class CreateComparison
{
    private readonly int _value = Inputs.Value;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup() =>
        Parity.Same(nameof(CreateComparison),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions())),
            (nameof(ErrorOr), Parity.Of(ErrorOr())),
            (nameof(FluentResults), Parity.Of(FluentResults())));

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#returning-failures
    [Benchmark(Baseline = true)]
    public Result<int> CSharpEssentials() => Succeed ? _value : Inputs.CseError;

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#implicit-conversion-1
    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#explicit-construction-success-and-failure
    [Benchmark]
    public Cfe.Result<int> CSharpFunctionalExtensions() => Succeed ? _value : Cfe.Result.Failure<int>(Inputs.CfeError);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#using-implicit-conversion
    [Benchmark]
    public Eo.ErrorOr<int> ErrorOr() => Succeed ? _value : Inputs.ErrorOrError;

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#implicit-conversion-from-t-to-success-result-resultt
    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#implicit-conversion-from-error-to-fail-result-result-or-resultt
    [Benchmark]
    public Fr.Result<int> FluentResults() => Succeed ? _value : Inputs.FluentError;
}
