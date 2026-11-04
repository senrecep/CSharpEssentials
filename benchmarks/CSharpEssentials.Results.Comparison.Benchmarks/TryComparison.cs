using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
public class TryComparison
{
    private string _text = string.Empty;
    private Func<int> _parse = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _text = Succeed ? "42" : "x";
        _parse = () => int.Parse(_text, CultureInfo.InvariantCulture);

        Parity.Same(nameof(TryComparison),
            ("Expected", Succeed ? "ok:42" : "fail:1:" + Inputs.Message),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions())),
            (nameof(ErrorOr), Parity.Of(ErrorOr())),
            (nameof(FluentResults), Parity.Of(FluentResults())));
        Result<int> exception = CSharpEssentials_ErrorException();
        Parity.Same(nameof(TryComparison) + "." + nameof(CSharpEssentials_ErrorException),
            ("Expected", Succeed ? "ok:42" : "fail:1:" + nameof(FormatException)),
            (nameof(CSharpEssentials_ErrorException),
                exception.IsSuccess ? Parity.Of(exception) : "fail:" + exception.Errors.Length.ToString(CultureInfo.InvariantCulture) + ":" + exception.Errors[0].Code));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#safe-execution
    [Benchmark(Baseline = true)]
    public Result<int> CSharpEssentials() => Result.Try(_parse, static _ => Inputs.CseError);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#safe-execution
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#converting-from-exceptions
    [Benchmark]
    public Result<int> CSharpEssentials_ErrorException() => Result.Try(_parse, static ex => Error.Exception(ex));

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Try.cs
    [Benchmark]
    public Cfe.Result<int> CSharpFunctionalExtensions() => Cfe.Result.Try(_parse, static _ => Inputs.CfeError);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#replace-throwing-exceptions-with-errorort
    [Benchmark]
    public Eo.ErrorOr<int> ErrorOr()
    {
        try
        {
            return _parse();
        }
        catch (Exception)
        {
            return Inputs.ErrorOrError;
        }
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#try
    [Benchmark]
    public Fr.Result<int> FluentResults() => Fr.Result.Try(_parse, static _ => Inputs.FluentError);
}
