using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using OneOf;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Gets the int out of a two-case union, or -1 when it holds text: 42 or -1. ByCase uses each library's documented
/// per-case access: CSE <c>IsFirst</c>/<c>GetFirst</c>, OneOf <c>TryPickT0</c>, Dunet a C# type pattern on the variant.
/// <para>
/// Semantic differences: OneOf <c>TryPickT0</c> also produces the remainder (the other case), which the others do not.
/// CSE <c>GetFirst</c> unboxes from <c>object</c>. ByRuntimeType holds CSE <c>TryAs</c>, which tests the runtime type of
/// <c>Value</c> rather than the active case (the two differ for a union such as <c>Any&lt;int, int&gt;</c>), so it is
/// compared with nothing and only checked against the expected value.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TryPickComparison
{
    private const string ByCase = "ByCase";
    private const string ByRuntimeType = "ByRuntimeType";

    private Any<int, string> _cse;
    private OneOf<int, string> _oneOf;
    private IntOrText _dunet = null!;

    [Params(Inputs.IntBranch, Inputs.TextBranch)]
    public string Branch { get; set; } = Inputs.IntBranch;

    [GlobalSetup]
    public void Setup()
    {
        if (Branch == Inputs.IntBranch)
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

        string expected = Parity.Of(Branch == Inputs.IntBranch ? Inputs.IntValue : -1);
        Parity.Same(ByCase,
            ("expected", expected),
            (nameof(CSharpEssentials_ByCase), Parity.Of(CSharpEssentials_ByCase())),
            (nameof(OneOf_ByCase), Parity.Of(OneOf_ByCase())),
            (nameof(Dunet_ByCase), Parity.Of(Dunet_ByCase())));
        Parity.Same(ByRuntimeType, ("expected", expected), (nameof(CSharpEssentials_TryAs), Parity.Of(CSharpEssentials_TryAs())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#type-checking
    [Benchmark(Baseline = true), BenchmarkCategory(ByCase)]
    public int CSharpEssentials_ByCase() => _cse.IsFirst ? _cse.GetFirst() : -1;

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md?plain=1#L107
    [Benchmark, BenchmarkCategory(ByCase)]
    public int OneOf_ByCase() => _oneOf.TryPickT0(out int v, out _) ? v : -1;

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#usage
    [Benchmark, BenchmarkCategory(ByCase)]
    public int Dunet_ByCase() => _dunet is IntOrText.Number n ? n.Value : -1;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#type-checking
    [Benchmark, BenchmarkCategory(ByRuntimeType)]
    public int CSharpEssentials_TryAs() => _cse.TryAs<int, int, string>(out int v) ? v : -1;
}
