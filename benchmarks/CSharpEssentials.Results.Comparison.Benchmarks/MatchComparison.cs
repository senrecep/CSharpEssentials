using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Match a success or failure built in setup to an int (-1 on failure) and to a string (the first error's message on
/// failure). FluentResults has no Match; its column uses the README's documented processing idiom
/// (<c>IsSuccess</c>/<c>Value</c>, <c>Errors[0].Message</c>), which does no work beyond the library's own properties.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class MatchComparison
{
    private const string ToInt = "ToInt";
    private const string ToText = "ToString";

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
        Parity.Same(nameof(MatchComparison) + "." + ToInt,
            (nameof(CSharpEssentials_ToInt), Parity.Of(CSharpEssentials_ToInt())),
            (nameof(CSharpFunctionalExtensions_ToInt), Parity.Of(CSharpFunctionalExtensions_ToInt())),
            (nameof(ErrorOr_ToInt), Parity.Of(ErrorOr_ToInt())),
            (nameof(FluentResults_ToInt), Parity.Of(FluentResults_ToInt())));
        Parity.Same(nameof(MatchComparison) + "." + ToText,
            (nameof(CSharpEssentials_ToString), Parity.Of(CSharpEssentials_ToString())),
            (nameof(CSharpFunctionalExtensions_ToString), Parity.Of(CSharpFunctionalExtensions_ToString())),
            (nameof(ErrorOr_ToString), Parity.Of(ErrorOr_ToString())),
            (nameof(FluentResults_ToString), Parity.Of(FluentResults_ToString())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#matching
    [Benchmark(Baseline = true), BenchmarkCategory(ToInt)]
    public int CSharpEssentials_ToInt() => _cse.Match(static v => v, static _ => -1);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Match.cs
    [Benchmark, BenchmarkCategory(ToInt)]
    public int CSharpFunctionalExtensions_ToInt() => _cfe.Match(static v => v, static _ => -1);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#match-1
    [Benchmark, BenchmarkCategory(ToInt)]
    public int ErrorOr_ToInt() => _errorOr.Match(static v => v, static _ => -1);

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#processing-a-result
    [Benchmark, BenchmarkCategory(ToInt)]
    public int FluentResults_ToInt() => _fluent.IsSuccess ? _fluent.Value : -1;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#matching
    [Benchmark(Baseline = true), BenchmarkCategory(ToText)]
    public string CSharpEssentials_ToString() =>
        _cse.Match(static v => v.ToString(CultureInfo.InvariantCulture), static errors => errors[0].Description);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Match.cs
    [Benchmark, BenchmarkCategory(ToText)]
    public string CSharpFunctionalExtensions_ToString() =>
        _cfe.Match(static v => v.ToString(CultureInfo.InvariantCulture), static error => error);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#match-1
    [Benchmark, BenchmarkCategory(ToText)]
    public string ErrorOr_ToString() =>
        _errorOr.Match(static v => v.ToString(CultureInfo.InvariantCulture), static errors => errors[0].Description);

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#processing-a-result
    [Benchmark, BenchmarkCategory(ToText)]
    public string FluentResults_ToString() =>
        _fluent.IsSuccess ? _fluent.Value.ToString(CultureInfo.InvariantCulture) : _fluent.Errors[0].Message;
}
