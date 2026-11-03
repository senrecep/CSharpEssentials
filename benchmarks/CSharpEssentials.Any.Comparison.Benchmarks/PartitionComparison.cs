using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using OneOf;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Splits 1000 two-case unions (even indexes hold the int index, odd indexes hold a preallocated string, 500/500) into an
/// <c>int[]</c> and a <c>string[]</c>, the shape CSE <c>Partition</c> returns. Every source is typed as
/// <c>IEnumerable&lt;T&gt;</c> over an array built in setup, because that is what CSE <c>Partition</c> takes. Parity
/// compares the counts, the int sum and the total string length.
/// <para>
/// Neither OneOf nor Dunet ships a partition, so their rows are hand-written loops over each library's documented access
/// API: OneOf <c>TryPickT0</c>, Dunet's stateful <c>Match</c> (documented for cutting allocations). The same loop over CSE
/// <c>IsFirst</c>/<c>GetFirst</c> is a reference row that isolates what the library method adds on top of the loop.
/// All loops fill unsized lists and copy them to arrays, as CSE <c>Partition</c> does.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class PartitionComparison
{
    private const string Category = "Partition";

    private IEnumerable<Any<int, string>> _cse = null!;
    private IEnumerable<OneOf<int, string>> _oneOf = null!;
    private IEnumerable<IntOrText> _dunet = null!;

    [GlobalSetup]
    public void Setup()
    {
        var cse = new Any<int, string>[Inputs.Count];
        var oneOf = new OneOf<int, string>[Inputs.Count];
        var dunet = new IntOrText[Inputs.Count];
        var expectedInts = new List<int>();
        var expectedTexts = new List<string>();
        for (int i = 0; i < Inputs.Count; i++)
        {
            if (Inputs.IsInt(i))
            {
                cse[i] = i;
                oneOf[i] = i;
                dunet[i] = i;
                expectedInts.Add(i);
            }
            else
            {
                cse[i] = Inputs.Texts[i];
                oneOf[i] = Inputs.Texts[i];
                dunet[i] = Inputs.Texts[i];
                expectedTexts.Add(Inputs.Texts[i]);
            }
        }

        _cse = cse;
        _oneOf = oneOf;
        _dunet = dunet;
        Parity.Same(Category,
            ("expected", Parity.Of([.. expectedInts], [.. expectedTexts])),
            (nameof(CSharpEssentials_Partition), Of(CSharpEssentials_Partition())),
            (nameof(CSharpEssentials_HandLoop), Of(CSharpEssentials_HandLoop())),
            (nameof(OneOf_HandLoop), Of(OneOf_HandLoop())),
            (nameof(Dunet_StatefulMatch), Of(Dunet_StatefulMatch())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(Category)]
    public (int[] First, string[] Second) CSharpEssentials_Partition() => _cse.Partition();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#type-checking
    [Benchmark, BenchmarkCategory(Category)]
    public (int[] First, string[] Second) CSharpEssentials_HandLoop()
    {
        var ints = new List<int>();
        var texts = new List<string>();
        foreach (Any<int, string> item in _cse)
        {
            if (item.IsFirst)
                ints.Add(item.GetFirst());
            else
                texts.Add(item.GetSecond());
        }

        return (ints.ToArray(), texts.ToArray());
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md?plain=1#L107
    [Benchmark, BenchmarkCategory(Category)]
    public (int[] First, string[] Second) OneOf_HandLoop()
    {
        var ints = new List<int>();
        var texts = new List<string>();
        foreach (OneOf<int, string> item in _oneOf)
        {
            if (item.TryPickT0(out int v, out string s))
                ints.Add(v);
            else
                texts.Add(s);
        }

        return (ints.ToArray(), texts.ToArray());
    }

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#stateful-matching
    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/src/Dunet.Generator/UnionGeneration/UnionSourceBuilder.cs#L150-L171
    [Benchmark, BenchmarkCategory(Category)]
    public (int[] First, string[] Second) Dunet_StatefulMatch()
    {
        (List<int> Ints, List<string> Texts) state = (new List<int>(), new List<string>());
        foreach (IntOrText item in _dunet)
            item.Match(state, static (st, n) => st.Ints.Add(n.Value), static (st, t) => st.Texts.Add(t.Value));

        return (state.Ints.ToArray(), state.Texts.ToArray());
    }

    private static string Of((int[] First, string[] Second) result) => Parity.Of(result.First, result.Second);
}
