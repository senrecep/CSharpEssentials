using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.These;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Splits 1000 <c>These&lt;string, int&gt;</c> values (index % 3: 0 is Left(text), 1 is Right(index), 2 is Both(text,
/// index); 334/333/333) into lefts, rights and boths, returned as three lists, the shape CSE <c>Partition</c> returns.
/// Both sources are typed as <c>IEnumerable&lt;T&gt;</c> over an array built in setup. The hand-written row is a loop
/// with flag checks over <see cref="Outcome"/> into three unsized lists. Parity compares the counts, the sum of the right
/// and both values, and the total length of the left and both texts.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ThesePartitionComparison
{
    private const string Category = "Partition";

    private IEnumerable<These<string, int>> _these = null!;
    private IEnumerable<Outcome> _outcomes = null!;

    [GlobalSetup]
    public void Setup()
    {
        var these = new These<string, int>[Inputs.Count];
        var outcomes = new Outcome[Inputs.Count];
        var lefts = new List<string>();
        var rights = new List<int>();
        var boths = new List<(string, int)>();
        for (int i = 0; i < Inputs.Count; i++)
        {
            string text = Inputs.Texts[i];
            switch (i % 3)
            {
                case 0:
                    these[i] = These<string, int>.Left(text);
                    outcomes[i] = new Outcome(true, false, text, 0);
                    lefts.Add(text);
                    break;
                case 1:
                    these[i] = These<string, int>.Right(i);
                    outcomes[i] = new Outcome(false, true, null, i);
                    rights.Add(i);
                    break;
                default:
                    these[i] = These<string, int>.Both(text, i);
                    outcomes[i] = new Outcome(true, true, text, i);
                    boths.Add((text, i));
                    break;
            }
        }

        _these = these;
        _outcomes = outcomes;
        Parity.Same(Category,
            ("expected", Parity.Of(lefts, rights, boths)),
            (nameof(CSharpEssentials_Partition), Of(CSharpEssentials_Partition())),
            (nameof(HandWritten_Partition), Of(HandWritten_Partition())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.These/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(Category)]
    public (IReadOnlyList<string> Lefts, IReadOnlyList<int> Rights, IReadOnlyList<(string, int)> Boths) CSharpEssentials_Partition() =>
        _these.Partition();

    [Benchmark, BenchmarkCategory(Category)]
    public (IReadOnlyList<string> Lefts, IReadOnlyList<int> Rights, IReadOnlyList<(string, int)> Boths) HandWritten_Partition()
    {
        var lefts = new List<string>();
        var rights = new List<int>();
        var boths = new List<(string, int)>();
        foreach (Outcome o in _outcomes)
        {
            if (o.HasLeft && o.HasRight)
                boths.Add((o.LeftOrDefault!, o.RightOrDefault));
            else if (o.HasLeft)
                lefts.Add(o.LeftOrDefault!);
            else
                rights.Add(o.RightOrDefault);
        }

        return (lefts, rights, boths);
    }

    private static string Of((IReadOnlyList<string> Lefts, IReadOnlyList<int> Rights, IReadOnlyList<(string, int)> Boths) result) =>
        Parity.Of(result.Lefts, result.Rights, result.Boths);
}
