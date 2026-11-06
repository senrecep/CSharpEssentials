using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.These;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Runs <c>Map(v * 2)</c>, then <c>FlatMap(v &gt; 50 ? Right(v + 1) : Left("small"))</c>, then the fold of
/// <see cref="TheseMatchComparison"/> on a <c>These&lt;string, int&gt;</c>: Left("warn") gives 4, Right(42) gives 85,
/// Both("warn", 42) gives Both("warn", 85) and so 89. The hand-written row is if/else over <see cref="Outcome"/>.
/// <para>
/// The hand-written row copies CSE's FlatMap rule for Both on purpose so parity holds: errors are never combined. When
/// the mapper returns Right or Both, the original left is kept and the mapper's left is dropped; when it returns Left,
/// the mapper's left is returned and the original left is dropped. One error is always lost; it is CSE's behavior, not
/// a recommendation.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ThesePipelineComparison
{
    private const string Category = "MapFlatMapMatch";
    private const string Small = "small";

    private These<string, int> _these;
    private Outcome _outcome;

    [Params(TheseState.Left, TheseState.Right, TheseState.Both)]
    public TheseState State { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _these = TheseInputs.Cse(State);
        _outcome = TheseInputs.HandWritten(State);
        int expected = State switch
        {
            TheseState.Left => Inputs.Warning.Length,
            TheseState.Right => Inputs.IntValue * 2 + 1,
            TheseState.Both => Inputs.IntValue * 2 + 1 + Inputs.Warning.Length,
            _ => throw new InvalidOperationException(State.ToString()),
        };
        Parity.Same(Category,
            ("expected", Parity.Of(expected)),
            (nameof(CSharpEssentials_Pipeline), Parity.Of(CSharpEssentials_Pipeline())),
            (nameof(HandWritten_Pipeline), Parity.Of(HandWritten_Pipeline())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.These/Readme.MD#flatmap-chain-these
    [Benchmark(Baseline = true), BenchmarkCategory(Category)]
    public int CSharpEssentials_Pipeline() =>
        _these
            .Map(static v => v * 2)
            .FlatMap(static v => v > 50 ? These<string, int>.Right(v + 1) : These<string, int>.Left(Small))
            .Match(onLeft: static e => e.Length, onRight: static v => v, onBoth: static (e, v) => v + e.Length);

    [Benchmark, BenchmarkCategory(Category)]
    public int HandWritten_Pipeline()
    {
        Outcome o = _outcome;
        if (o.HasLeft && !o.HasRight)
            return o.LeftOrDefault!.Length;

        int mapped = o.RightOrDefault * 2;
        Outcome next = mapped > 50 ? new Outcome(false, true, null, mapped + 1) : new Outcome(true, false, Small, 0);
        if (o.HasLeft && next.HasRight)
            next = new Outcome(true, true, o.LeftOrDefault, next.RightOrDefault);

        if (next.HasLeft && !next.HasRight)
            return next.LeftOrDefault!.Length;
        if (!next.HasLeft && next.HasRight)
            return next.RightOrDefault;
        return next.RightOrDefault + next.LeftOrDefault!.Length;
    }
}
