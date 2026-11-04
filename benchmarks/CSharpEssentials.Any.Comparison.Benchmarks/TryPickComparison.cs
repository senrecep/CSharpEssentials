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
/// <para>
/// One read takes well under a nanosecond, below what a single call can resolve, so every row sums the result over
/// <see cref="Inputs.Batch"/> copies of its union and reports the time per read (<c>OperationsPerInvoke</c>). The returned
/// sum is the expected value times <see cref="Inputs.Batch"/>.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TryPickComparison
{
    private const string ByCase = "ByCase";
    private const string ByRuntimeType = "ByRuntimeType";

    private Any<int, string>[] _cse = [];
    private OneOf<int, string>[] _oneOf = [];
    private IntOrText[] _dunet = [];

    [Params(Inputs.IntBranch, Inputs.TextBranch)]
    public string Branch { get; set; } = Inputs.IntBranch;

    [GlobalSetup]
    public void Setup()
    {
        if (Branch == Inputs.IntBranch)
        {
            _cse = Filled<Any<int, string>>(Inputs.IntValue);
            _oneOf = Filled<OneOf<int, string>>(Inputs.IntValue);
            _dunet = Filled<IntOrText>(Inputs.IntValue);
        }
        else
        {
            _cse = Filled<Any<int, string>>(Inputs.TextValue);
            _oneOf = Filled<OneOf<int, string>>(Inputs.TextValue);
            _dunet = Filled<IntOrText>(Inputs.TextValue);
        }

        string expected = Parity.Of((Branch == Inputs.IntBranch ? Inputs.IntValue : -1) * Inputs.Batch);
        Parity.Same(ByCase,
            ("expected", expected),
            (nameof(CSharpEssentials_ByCase), Parity.Of(CSharpEssentials_ByCase())),
            (nameof(OneOf_ByCase), Parity.Of(OneOf_ByCase())),
            (nameof(Dunet_ByCase), Parity.Of(Dunet_ByCase())));
        Parity.Same(ByRuntimeType, ("expected", expected), (nameof(CSharpEssentials_TryAs), Parity.Of(CSharpEssentials_TryAs())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#type-checking
    [Benchmark(Baseline = true, OperationsPerInvoke = Inputs.Batch), BenchmarkCategory(ByCase)]
    public int CSharpEssentials_ByCase()
    {
        int sum = 0;
        foreach (Any<int, string> u in _cse)
            sum += u.IsFirst ? u.GetFirst() : -1;
        return sum;
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md?plain=1#L107
    [Benchmark(OperationsPerInvoke = Inputs.Batch), BenchmarkCategory(ByCase)]
    public int OneOf_ByCase()
    {
        int sum = 0;
        foreach (OneOf<int, string> u in _oneOf)
            sum += u.TryPickT0(out int v, out _) ? v : -1;
        return sum;
    }

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#usage
    [Benchmark(OperationsPerInvoke = Inputs.Batch), BenchmarkCategory(ByCase)]
    public int Dunet_ByCase()
    {
        int sum = 0;
        foreach (IntOrText u in _dunet)
            sum += u is IntOrText.Number n ? n.Value : -1;
        return sum;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#type-checking
    [Benchmark(Baseline = true, OperationsPerInvoke = Inputs.Batch), BenchmarkCategory(ByRuntimeType)]
    public int CSharpEssentials_TryAs()
    {
        int sum = 0;
        foreach (Any<int, string> u in _cse)
            sum += u.TryAs<int, int, string>(out int v) ? v : -1;
        return sum;
    }

    private static T[] Filled<T>(T value) => [.. Enumerable.Repeat(value, Inputs.Batch)];
}
