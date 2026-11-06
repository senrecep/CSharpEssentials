using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using OneOf;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Folds a two-case union to an int: the int case returns the value (42), the text case returns the string length (9 for
/// "forty-two"). ToInt uses each library's function form; Switch uses its action form, which writes the same value into
/// one static sink that the method returns. The unions are built in setup, so only dispatch is measured.
/// <para>
/// Semantic differences: CSE handlers are optional and an unmatched case returns <c>NotExecuted</c> (no compile-time
/// exhaustiveness), while OneOf and Dunet require every handler. CSE casts from <c>object</c>, which unboxes on the int
/// case, and throws when <c>Value</c> is null; its <c>Match</c> returns <c>AnyActionResult&lt;int&gt;</c>, read through
/// <c>.Result</c>. Dunet dispatches through a virtual call on its record class.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class MatchComparison
{
    private const string ToInt = "ToInt";
    private const string SwitchCategory = "Switch";

    private static int s_sink;

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

        int expected = Branch == Inputs.IntBranch ? Inputs.IntValue : Inputs.TextValue.Length;
        Parity.Same(ToInt,
            ("expected", Parity.Of(expected)),
            (nameof(CSharpEssentials_ToInt), Parity.Of(CSharpEssentials_ToInt())),
            (nameof(OneOf_ToInt), Parity.Of(OneOf_ToInt())),
            (nameof(Dunet_ToInt), Parity.Of(Dunet_ToInt())));
        Parity.Same(SwitchCategory,
            ("expected", Parity.Of(expected)),
            (nameof(CSharpEssentials_Switch), Parity.Of(CSharpEssentials_Switch())),
            (nameof(OneOf_Switch), Parity.Of(OneOf_Switch())),
            (nameof(Dunet_Switch), Parity.Of(Dunet_Switch())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#pattern-matching
    [Benchmark(Baseline = true), BenchmarkCategory(ToInt)]
    public int CSharpEssentials_ToInt() => _cse.Match(first: static v => v, second: static s => s.Length).Result;

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md#matching
    [Benchmark, BenchmarkCategory(ToInt)]
    public int OneOf_ToInt() => _oneOf.Match(static v => v, static s => s.Length);

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#match-method
    [Benchmark, BenchmarkCategory(ToInt)]
    public int Dunet_ToInt() => _dunet.Match(static n => n.Value, static t => t.Value.Length);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#pattern-matching
    [Benchmark(Baseline = true), BenchmarkCategory(SwitchCategory)]
    public int CSharpEssentials_Switch()
    {
        _cse.Switch(first: static v => Sink(v), second: static s => Sink(s.Length));
        return s_sink;
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md?plain=1#L97
    [Benchmark, BenchmarkCategory(SwitchCategory)]
    public int OneOf_Switch()
    {
        _oneOf.Switch(static v => Sink(v), static s => Sink(s.Length));
        return s_sink;
    }

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#match-method
    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/src/Dunet.Generator/UnionGeneration/UnionSourceBuilder.cs#L106-L124
    [Benchmark, BenchmarkCategory(SwitchCategory)]
    public int Dunet_Switch()
    {
        _dunet.Match(static n => Sink(n.Value), static t => Sink(t.Value.Length));
        return s_sink;
    }

    private static void Sink(int value) => s_sink = value;
}
