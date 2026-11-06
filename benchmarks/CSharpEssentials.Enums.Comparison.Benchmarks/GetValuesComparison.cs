using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Enums.Comparison.Benchmarks.NetEscapadesGenerated;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// Gets all <see cref="Status"/> values (summed as ints) or names (summed lengths), so the work cannot be eliminated.
/// Every column walks the returned collection with an indexed <c>for</c> loop. BCL, CSE and NetEscapades return a new
/// array on every call; Enums.NET returns a cached <c>IReadOnlyList</c> (a <c>foreach</c> over it would allocate a boxed
/// enumerator, which the <c>for</c> loop avoids).
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class GetValuesComparison
{
    private const string Values = "GetValues";
    private const string Names = "GetNames";

    [GlobalSetup]
    public void Setup()
    {
        Parity.Same(Values,
            (nameof(CSharpEssentials_Values), Parity.Of(CSharpEssentials_Values())),
            (nameof(Bcl_Values), Parity.Of(Bcl_Values())),
            (nameof(EnumsNet_Values), Parity.Of(EnumsNet_Values())),
            (nameof(NetEscapades_Values), Parity.Of(NetEscapades_Values())));
        Parity.Same(Names,
            (nameof(CSharpEssentials_Names), Parity.Of(CSharpEssentials_Names())),
            (nameof(Bcl_Names), Parity.Of(Bcl_Names())),
            (nameof(EnumsNet_Names), Parity.Of(EnumsNet_Names())),
            (nameof(NetEscapades_Names), Parity.Of(NetEscapades_Names())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
    [Benchmark(Baseline = true), BenchmarkCategory(Values)]
    public int CSharpEssentials_Values() => Sum(StatusExtensions.GetValues());

    // https://learn.microsoft.com/dotnet/api/system.enum.getvalues
    [Benchmark, BenchmarkCategory(Values)]
    public int Bcl_Values() => Sum(Enum.GetValues<Status>());

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L284
    [Benchmark, BenchmarkCategory(Values)]
    public int EnumsNet_Values() => Sum(EnumsNET.Enums.GetValues<Status>());

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark, BenchmarkCategory(Values)]
    public int NetEscapades_Values() => Sum(StatusFastExtensions.GetValues());

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
    [Benchmark(Baseline = true), BenchmarkCategory(Names)]
    public int CSharpEssentials_Names() => Sum(StatusExtensions.GetNames());

    // https://learn.microsoft.com/dotnet/api/system.enum.getnames
    [Benchmark, BenchmarkCategory(Names)]
    public int Bcl_Names() => Sum(Enum.GetNames<Status>());

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L273
    [Benchmark, BenchmarkCategory(Names)]
    public int EnumsNet_Names() => Sum(EnumsNET.Enums.GetNames<Status>());

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark, BenchmarkCategory(Names)]
    public int NetEscapades_Names() => Sum(StatusFastExtensions.GetNames());

    private static int Sum(Status[] values)
    {
        int sum = 0;
        for (int i = 0; i < values.Length; i++)
        {
            sum += (int)values[i];
        }

        return sum;
    }

    private static int Sum(IReadOnlyList<Status> values)
    {
        int sum = 0;
        for (int i = 0; i < values.Count; i++)
        {
            sum += (int)values[i];
        }

        return sum;
    }

    private static int Sum(string[] names)
    {
        int sum = 0;
        for (int i = 0; i < names.Length; i++)
        {
            sum += names[i].Length;
        }

        return sum;
    }

    private static int Sum(IReadOnlyList<string> names)
    {
        int sum = 0;
        for (int i = 0; i < names.Count; i++)
        {
            sum += names[i].Length;
        }

        return sum;
    }
}
