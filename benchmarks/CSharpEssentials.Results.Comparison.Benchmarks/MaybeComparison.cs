using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// CSE <c>Maybe&lt;T&gt;</c> against CFE <c>Maybe&lt;T&gt;</c>: From/None, Map, Bind, Where, GetValueOrDefault and
/// Match, each in its own category over a Some(42) or None built in setup. ErrorOr and FluentResults have no column:
/// neither has a Maybe/Option type.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class MaybeComparison
{
    private const string Create = "Create";
    private const string MapCategory = "Map";
    private const string BindCategory = "Bind";
    private const string WhereCategory = "Where";
    private const string ValueOrDefault = "GetValueOrDefault";
    private const string MatchCategory = "Match";

    private int _value;
    private Maybe<int> _cse;
    private Cfe.Maybe<int> _cfe;

    [Params(true, false)]
    public bool HasValue { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _value = Inputs.Value;
        _cse = CSharpEssentials_Create();
        _cfe = CSharpFunctionalExtensions_Create();
        Check(Create, Parity.Of(_cse), Parity.Of(_cfe));
        Check(MapCategory, Parity.Of(CSharpEssentials_Map()), Parity.Of(CSharpFunctionalExtensions_Map()));
        Check(BindCategory, Parity.Of(CSharpEssentials_Bind()), Parity.Of(CSharpFunctionalExtensions_Bind()));
        Check(WhereCategory, Parity.Of(CSharpEssentials_Where()), Parity.Of(CSharpFunctionalExtensions_Where()));
        Check(ValueOrDefault, Parity.Of(CSharpEssentials_GetValueOrDefault()), Parity.Of(CSharpFunctionalExtensions_GetValueOrDefault()));
        Check(MatchCategory, Parity.Of(CSharpEssentials_Match()), Parity.Of(CSharpFunctionalExtensions_Match()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#creating-maybe
    [Benchmark(Baseline = true), BenchmarkCategory(Create)]
    public Maybe<int> CSharpEssentials_Create() => HasValue ? Maybe<int>.From(_value) : Maybe<int>.None;

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#explicit-construction
    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#noneno-value
    [Benchmark, BenchmarkCategory(Create)]
    public Cfe.Maybe<int> CSharpFunctionalExtensions_Create() => HasValue ? Cfe.Maybe<int>.From(_value) : Cfe.Maybe<int>.None;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#functional-chaining
    [Benchmark(Baseline = true), BenchmarkCategory(MapCategory)]
    public Maybe<int> CSharpEssentials_Map() => _cse.Map(static x => x * 2);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#map
    [Benchmark, BenchmarkCategory(MapCategory)]
    public Cfe.Maybe<int> CSharpFunctionalExtensions_Map() => _cfe.Map(static x => x * 2);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#functional-chaining
    [Benchmark(Baseline = true), BenchmarkCategory(BindCategory)]
    public Maybe<int> CSharpEssentials_Bind() => _cse.Bind(static x => Maybe<int>.From(x + 1));

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#bind
    [Benchmark, BenchmarkCategory(BindCategory)]
    public Cfe.Maybe<int> CSharpFunctionalExtensions_Bind() => _cfe.Bind(static x => Cfe.Maybe<int>.From(x + 1));

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#filtering-fallbacks-and-side-effects
    [Benchmark(Baseline = true), BenchmarkCategory(WhereCategory)]
    public Maybe<int> CSharpEssentials_Where() => _cse.Where(static x => x > 0);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#where
    [Benchmark, BenchmarkCategory(WhereCategory)]
    public Cfe.Maybe<int> CSharpFunctionalExtensions_Where() => _cfe.Where(static x => x > 0);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#reading-the-value
    [Benchmark(Baseline = true), BenchmarkCategory(ValueOrDefault)]
    public int CSharpEssentials_GetValueOrDefault() => _cse.GetValueOrDefault(-1);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#getvalueordefault
    [Benchmark, BenchmarkCategory(ValueOrDefault)]
    public int CSharpFunctionalExtensions_GetValueOrDefault() => _cfe.GetValueOrDefault(-1);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD#matching
    [Benchmark(Baseline = true), BenchmarkCategory(MatchCategory)]
    public int CSharpEssentials_Match() => _cse.Match(static x => x, static () => -1);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#match
    [Benchmark, BenchmarkCategory(MatchCategory)]
    public int CSharpFunctionalExtensions_Match() => _cfe.Match(static x => x, static () => -1);

    private static void Check(string category, string cse, string cfe) =>
        Parity.Same(nameof(MaybeComparison) + "." + category,
            (nameof(CSharpEssentials), cse),
            (nameof(CSharpFunctionalExtensions), cfe));
}
