using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Core;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// Draws K items from 1000 distinct integers (K is 1, 50 or 99 percent of them). The output is random, so parity checks
/// invariants: K items, all from the source, and distinct for the Unique rows. CSE and the
/// <see cref="RandomNumberGenerator"/> row use a cryptographic RNG; the <see cref="Random.Shared"/> and hand-written
/// Fisher-Yates rows do not. WithReplacement samples with replacement, so it does not do the same job and has no baseline.
/// </summary>
[SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "The non-cryptographic Random rows are the comparison being measured.")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class RandomItemsComparison
{
    private const string Unique = "Unique";
    private const string WithReplacement = "WithReplacement";

    private readonly int[] _source = Inputs.Source;
    private int _count;

    [Params(1, 50, 99)]
    public int Percent { get; set; } = 1;

    [GlobalSetup]
    public void Setup()
    {
        _count = Inputs.SourceCount * Percent / 100;
        string unique = string.Create(CultureInfo.InvariantCulture, $"n={_count};distinct=True;subset=True");
        Parity.Same(Unique + " " + Parity.Of(Percent),
            ("expected", unique),
            (nameof(CSharpEssentials_Unique), Of(CSharpEssentials_Unique(), true)),
            (nameof(Bcl_Unique), Of(Bcl_Unique(), true)),
            (nameof(BclShared_Unique), Of(BclShared_Unique(), true)),
            (nameof(HandWritten_Unique), Of(HandWritten_Unique(), true)));
        Parity.Same(WithReplacement + " " + Parity.Of(Percent),
            ("expected", string.Create(CultureInfo.InvariantCulture, $"n={_count};subset=True")),
            (nameof(Bcl_WithReplacement), Of(Bcl_WithReplacement(), false)));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(Unique)]
    public int[] CSharpEssentials_Unique() => _source.GetRandomItems(_count);

    // https://learn.microsoft.com/dotnet/api/system.security.cryptography.randomnumbergenerator.shuffle
    [Benchmark, BenchmarkCategory(Unique)]
    public int[] Bcl_Unique()
    {
        int[] copy = (int[])_source.Clone();
        RandomNumberGenerator.Shuffle(copy.AsSpan());
        return copy[.._count];
    }

    // https://learn.microsoft.com/dotnet/api/system.random.shuffle
    [Benchmark, BenchmarkCategory(Unique)]
    public int[] BclShared_Unique()
    {
        int[] copy = (int[])_source.Clone();
        Random.Shared.Shuffle(copy);
        return copy[.._count];
    }

    [Benchmark, BenchmarkCategory(Unique)]
    public int[] HandWritten_Unique()
    {
        int[] copy = (int[])_source.Clone();
        for (int i = 0; i < _count; i++)
        {
            int j = Random.Shared.Next(i, copy.Length);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }

        return copy[.._count];
    }

    // https://learn.microsoft.com/dotnet/api/system.random.getitems
    [Benchmark, BenchmarkCategory(WithReplacement)]
    public int[] Bcl_WithReplacement() => Random.Shared.GetItems(_source, _count);

    private static string Of(int[] items, bool unique)
    {
        bool subset = items.All(static v => (uint)v < Inputs.SourceCount);
        return unique
            ? string.Create(CultureInfo.InvariantCulture, $"n={items.Length};distinct={items.Distinct().Count() == items.Length};subset={subset}")
            : string.Create(CultureInfo.InvariantCulture, $"n={items.Length};subset={subset}");
    }
}
