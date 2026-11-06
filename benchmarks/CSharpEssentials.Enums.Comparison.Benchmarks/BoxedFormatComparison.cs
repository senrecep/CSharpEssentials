using System.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// Formats an enum value the caller holds only as an <see cref="object"/>. In category Format the value is boxed once in setup, not per call.
/// Category Format: <see cref="Shade.Indigo"/> as "Indigo" (PascalCase wire names, so CSE writes the member name like the
/// BCL and Enums.NET). <c>CSharpEssentials_HandleAll</c> is a CSE configuration row with the same output: its
/// <c>CanHandle</c> accepts every type, which isolates the cost of the default registry check. <c>CSharpEssentials_Typed</c>
/// is a reference row only: the unboxed generic call, which shows what boxing plus the visitor cost.
/// Category FormatMany: a boxed <c>Shade[]</c> of all eight members into one list. CSE has <c>TryFormatMany</c>; the BCL and
/// Enums.NET have no collection formatter, so their rows are hand-written loops around their documented single-value call.
/// Every FormatMany row returns the entry count plus the summed lengths.
/// <para>
/// NetEscapades.EnumGenerators has no column: its generated methods take the typed enum only, never an <see cref="object"/>.
/// There is no undefined-value row: CSE throws <see cref="EnumValueException"/> for an undefined value while the BCL
/// returns the number ("99"), so the outputs differ.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class BoxedFormatComparison
{
    private const string Single = "Format";
    private const string Many = "FormatMany";

    private object _boxed = null!;
    private Shade _typed;
    private object _boxedMany = null!;
    private List<string> _list = null!;
    private EnumConventions _handleAll = null!;

    [GlobalSetup]
    public void Setup()
    {
        _boxed = Shade.Indigo;
        _typed = Shade.Indigo;
        _boxedMany = new[] { Shade.Red, Shade.Orange, Shade.Yellow, Shade.Green, Shade.Cyan, Shade.Blue, Shade.Indigo, Shade.Violet };
        _list = [];
        _handleAll = EnumConventions.Default with { CanHandle = static _ => true };

        Parity.Same(Single,
            ("Expected", "Indigo"),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(CSharpEssentials_HandleAll), Parity.Of(CSharpEssentials_HandleAll())),
            (nameof(CSharpEssentials_Typed), Parity.Of(CSharpEssentials_Typed())),
            (nameof(Bcl), Parity.Of(Bcl())),
            (nameof(EnumsNet), Parity.Of(EnumsNet())));
        string[] names = Enum.GetNames<Shade>();
        Parity.Same(Many,
            ("Expected", Parity.Of(names.Length + names.Sum(static name => name.Length)) + " " + string.Join(",", names)),
            (nameof(CSharpEssentials_Many), ManyOutcome(CSharpEssentials_Many())),
            (nameof(Bcl_Many), ManyOutcome(Bcl_Many())),
            (nameof(EnumsNet_Many), ManyOutcome(EnumsNet_Many())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Parsing/EnumValueFormatter.cs#L22-L38
    [Benchmark(Baseline = true), BenchmarkCategory(Single)]
    public string CSharpEssentials() => EnumValueFormatter.Format(_boxed, EnumConventions.Default);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Parsing/EnumValueFormatter.cs#L22-L38
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#conventions-and-runtime-api
    [Benchmark, BenchmarkCategory(Single)]
    public string CSharpEssentials_HandleAll() => EnumValueFormatter.Format(_boxed, _handleAll);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#conventions-and-runtime-api
    [Benchmark, BenchmarkCategory(Single)]
    public string CSharpEssentials_Typed() => EnumValueFormatter.Format(_typed, EnumWireFormat.String);

    // https://learn.microsoft.com/dotnet/api/system.enum.tostring
    [Benchmark, BenchmarkCategory(Single)]
    public string Bcl() => _boxed.ToString()!;

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L3439
    [Benchmark, BenchmarkCategory(Single)]
    public string EnumsNet() => EnumsNET.Enums.AsString(_boxed.GetType(), _boxed);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Parsing/EnumValueFormatter.cs#L61-L101
    [Benchmark(Baseline = true), BenchmarkCategory(Many)]
    public int CSharpEssentials_Many()
    {
        _list.Clear();
        _ = EnumValueFormatter.TryFormatMany(_boxedMany, EnumConventions.Default, _list);
        return Total(_list);
    }

    // https://learn.microsoft.com/dotnet/api/system.enum.tostring
    [Benchmark, BenchmarkCategory(Many)]
    public int Bcl_Many()
    {
        _list.Clear();
        foreach (object item in (IEnumerable)_boxedMany)
        {
            _list.Add(item.ToString()!);
        }

        return Total(_list);
    }

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L3439
    [Benchmark, BenchmarkCategory(Many)]
    public int EnumsNet_Many()
    {
        _list.Clear();
        foreach (object item in (IEnumerable)_boxedMany)
        {
            _list.Add(EnumsNET.Enums.AsString(item.GetType(), item));
        }

        return Total(_list);
    }

    private string ManyOutcome(int total) => Parity.Of(total) + " " + string.Join(",", _list);

    private static int Total(List<string> values)
    {
        int total = values.Count;
        for (int i = 0; i < values.Count; i++)
        {
            total += values[i].Length;
        }

        return total;
    }
}
