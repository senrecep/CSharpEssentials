using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ErrorEqualityComparison
{
    private const string EqualsCategory = "Equals";
    private const string HashCategory = "GetHashCode";
    private const string HashSetCategory = "HashSet";
    private const int Distinct = 10;
    private const int Repeats = 10;

    private Error _cseA;
    private Error _cseB;
    private Eo.Error _errorOrA;
    private Eo.Error _errorOrB;
    private Error[] _cseHundred = [];
    private Eo.Error[] _errorOrHundred = [];

    [Params(false, true)]
    public bool WithMetadata { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _cseA = Cse(ErrorInputs.Code);
        _cseB = Cse(ErrorInputs.Code);
        _errorOrA = ErrorOrOf(ErrorInputs.Code);
        _errorOrB = ErrorOrOf(ErrorInputs.Code);
        string[] codes = ErrorInputs.Codes(Distinct);
        _cseHundred = new Error[Distinct * Repeats];
        _errorOrHundred = new Eo.Error[Distinct * Repeats];
        for (int i = 0; i < _cseHundred.Length; i++)
        {
            _cseHundred[i] = Cse(codes[i % Distinct]);
            _errorOrHundred[i] = ErrorOrOf(codes[i % Distinct]);
        }

        Check(EqualsCategory, "True", CSharpEssentials_Equals().ToString(), ErrorOr_Equals().ToString());
        Check(HashCategory, "True",
            (CSharpEssentials_GetHashCode() == _cseB.GetHashCode()).ToString(),
            (ErrorOr_GetHashCode() == _errorOrB.GetHashCode()).ToString());
        Check(HashSetCategory, "10", Count(CSharpEssentials_HashSet()), Count(ErrorOr_HashSet()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#creating-errors
    [Benchmark(Baseline = true), BenchmarkCategory(EqualsCategory)]
    public bool CSharpEssentials_Equals() => _cseA.Equals(_cseB);

    // https://github.com/amantinband/error-or/blob/v2.1.1/src/Errors/Error.cs
    [Benchmark, BenchmarkCategory(EqualsCategory)]
    public bool ErrorOr_Equals() => _errorOrA.Equals(_errorOrB);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#creating-errors
    [Benchmark(Baseline = true), BenchmarkCategory(HashCategory)]
    public int CSharpEssentials_GetHashCode() => _cseA.GetHashCode();

    // https://github.com/amantinband/error-or/blob/v2.1.1/src/Errors/Error.cs
    [Benchmark, BenchmarkCategory(HashCategory)]
    public int ErrorOr_GetHashCode() => _errorOrA.GetHashCode();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#creating-errors
    [Benchmark(Baseline = true), BenchmarkCategory(HashSetCategory)]
    public int CSharpEssentials_HashSet() => new HashSet<Error>(_cseHundred).Count;

    // https://github.com/amantinband/error-or/blob/v2.1.1/src/Errors/Error.cs
    [Benchmark, BenchmarkCategory(HashSetCategory)]
    public int ErrorOr_HashSet() => new HashSet<Eo.Error>(_errorOrHundred).Count;

    private static string Count(int count) => count.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static void Check(string category, string expected, string cse, string errorOr) =>
        Parity.Same(nameof(ErrorEqualityComparison) + "." + category,
            ("Expected", expected),
            (nameof(CSharpEssentials), cse),
            (nameof(ErrorOr), errorOr));

    private Error Cse(string code) =>
        Error.Validation(ErrorInputs.Copy(code), ErrorInputs.Copy(Inputs.Message), WithMetadata ? ErrorInputs.CseMetadata() : null);

    private Eo.Error ErrorOrOf(string code) =>
        Eo.Error.Validation(ErrorInputs.Copy(code), ErrorInputs.Copy(Inputs.Message), WithMetadata ? ErrorInputs.ErrorOrMetadata() : null);
}
