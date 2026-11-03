using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using OneOf;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Each library's default <c>ToString()</c> of a two-case union holding 42 or "forty-two". The outputs differ, so this is
/// not a ranking: every row sits in its own category with no baseline and is checked only against its own literal.
/// CSE returns the JSON of the value (<c>42</c>, <c>"forty-two"</c> with quotes) through reflection-based
/// System.Text.Json; OneOf returns <c>System.Int32: 42</c> / <c>System.String: forty-two</c>; Dunet returns the record
/// <c>ToString</c>, <c>Number { Value = 42 }</c> / <c>Text { Value = forty-two }</c>.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ToStringComparison
{
    private const string Cse = "Cse";
    private const string OneOfCategory = "OneOf";
    private const string Dunet = "Dunet";

    private Any<int, string> _cse;
    private OneOf<int, string> _oneOf;
    private IntOrText _dunet = null!;

    [Params(Inputs.IntBranch, Inputs.TextBranch)]
    public string Branch { get; set; } = Inputs.IntBranch;

    [GlobalSetup]
    public void Setup()
    {
        bool isInt = Branch == Inputs.IntBranch;
        if (isInt)
        {
            _cse = Inputs.IntValue;
            _oneOf = Inputs.IntValue;
            _dunet = Inputs.IntValue;
        }
        else
        {
            _cse = Inputs.TextValue;
            _oneOf = Inputs.TextValue;
            _dunet = Inputs.TextValue;
        }

        Parity.Same(Cse, ("expected", isInt ? "42" : "\"forty-two\""), (nameof(CSharpEssentials_ToString), CSharpEssentials_ToString()));
        Parity.Same(OneOfCategory, ("expected", isInt ? "System.Int32: 42" : "System.String: forty-two"), (nameof(OneOf_ToString), OneOf_ToString()));
        Parity.Same(Dunet, ("expected", isInt ? "Number { Value = 42 }" : "Text { Value = forty-two }"), (nameof(Dunet_ToString), Dunet_ToString()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#type-checking
    [Benchmark, BenchmarkCategory(Cse)]
    public string CSharpEssentials_ToString() => _cse.ToString();

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/OneOf/OneOfT1.generated.cs
    [Benchmark, BenchmarkCategory(OneOfCategory)]
    public string OneOf_ToString() => _oneOf.ToString();

    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display
    [Benchmark, BenchmarkCategory(Dunet)]
    public string Dunet_ToString() => _dunet.ToString();
}
