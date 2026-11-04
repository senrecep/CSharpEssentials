using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.These;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Folds a <c>These&lt;string, int&gt;</c> to an int: Left("warn") gives the error length (4), Right(42) the value (42),
/// Both("warn", 42) the value plus the error length (46). CSE <c>Match</c> takes three delegates; the hand-written row is
/// if/else over <see cref="Outcome"/> in the same branch order as <c>These.Match</c>. No library competitor exists, so the
/// comparison is with hand-written code.
/// <para>
/// One fold takes a nanosecond or two, close to what a single call can resolve, so each row sums the result over
/// <see cref="Inputs.Batch"/> copies of its value and reports the time per fold (<c>OperationsPerInvoke</c>). The returned
/// sum is the expected value times <see cref="Inputs.Batch"/>.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TheseMatchComparison
{
    private const string Category = "Match";

    private These<string, int>[] _these = [];
    private Outcome[] _outcome = [];

    [Params(TheseState.Left, TheseState.Right, TheseState.Both)]
    public TheseState State { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _these = [.. Enumerable.Repeat(TheseInputs.Cse(State), Inputs.Batch)];
        _outcome = [.. Enumerable.Repeat(TheseInputs.HandWritten(State), Inputs.Batch)];
        int expected = State switch
        {
            TheseState.Left => Inputs.Warning.Length,
            TheseState.Right => Inputs.IntValue,
            TheseState.Both => Inputs.IntValue + Inputs.Warning.Length,
            _ => throw new InvalidOperationException(State.ToString()),
        };
        Parity.Same(Category,
            ("expected", Parity.Of(expected * Inputs.Batch)),
            (nameof(CSharpEssentials_Match), Parity.Of(CSharpEssentials_Match())),
            (nameof(HandWritten_Match), Parity.Of(HandWritten_Match())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.These/Readme.MD#match-exhaustive-pattern-match
    [Benchmark(Baseline = true, OperationsPerInvoke = Inputs.Batch), BenchmarkCategory(Category)]
    public int CSharpEssentials_Match()
    {
        int sum = 0;
        foreach (These<string, int> t in _these)
            sum += t.Match(onLeft: static e => e.Length, onRight: static v => v, onBoth: static (e, v) => v + e.Length);
        return sum;
    }

    [Benchmark(OperationsPerInvoke = Inputs.Batch), BenchmarkCategory(Category)]
    public int HandWritten_Match()
    {
        int sum = 0;
        foreach (Outcome o in _outcome)
            sum += Fold(o);
        return sum;
    }

    private static int Fold(Outcome o)
    {
        if (o.HasLeft && !o.HasRight)
            return o.LeftOrDefault!.Length;
        if (!o.HasLeft && o.HasRight)
            return o.RightOrDefault;
        return o.RightOrDefault + o.LeftOrDefault!.Length;
    }
}
