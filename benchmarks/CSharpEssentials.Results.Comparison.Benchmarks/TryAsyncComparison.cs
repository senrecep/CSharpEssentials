using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
public class TryAsyncComparison
{
    private Func<Task<int>> _parseAsync = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string text = Succeed ? "42" : "x";
        bool yield = Yield;
        _parseAsync = () => ParseAsync(text, yield);

        Parity.Same(nameof(TryAsyncComparison),
            ("Expected", Succeed ? "ok:42" : "fail:1:" + Inputs.Message),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials().GetAwaiter().GetResult())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions().GetAwaiter().GetResult())),
            (nameof(ErrorOr), Parity.Of(ErrorOr().GetAwaiter().GetResult())),
            (nameof(FluentResults), Parity.Of(FluentResults().GetAwaiter().GetResult())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#safe-execution
    [Benchmark(Baseline = true)]
    public Task<Result<int>> CSharpEssentials() => Result.TryAsync(_parseAsync, static _ => Inputs.CseError);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Try.Task.cs
    [Benchmark]
    public Task<Cfe.Result<int>> CSharpFunctionalExtensions() => Cfe.Result.Try(_parseAsync, static _ => Inputs.CfeError);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#replace-throwing-exceptions-with-errorort
    [Benchmark]
    public async Task<Eo.ErrorOr<int>> ErrorOr()
    {
        try
        {
            return await _parseAsync();
        }
        catch (Exception)
        {
            return Inputs.ErrorOrError;
        }
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#try
    [Benchmark]
    public Task<Fr.Result<int>> FluentResults() => Fr.Result.Try(_parseAsync, static _ => Inputs.FluentError);

    private static async Task<int> ParseAsync(string text, bool yield)
    {
        if (yield)
            await Task.Yield();
        return int.Parse(text, CultureInfo.InvariantCulture);
    }
}
