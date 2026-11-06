using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Enums.Comparison.Benchmarks.NetEscapadesGenerated;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// Parses a <see cref="Status"/> member name. The valid input is <see cref="Status.PendingApproval"/>, the 11th of 20
/// members (NetEscapades compares names one by one in declaration order); the invalid input is a non-numeric unknown name.
/// Each method returns the parsed value as an int, or -1 when parsing fails.
/// <para>
/// CaseSensitive parses "PendingApproval" or "Unknown". The generated helpers have no case-sensitive parse; the runtime
/// parser does with <c>CaseInsensitive = false</c>, so <c>CSharpEssentials_CaseSensitive</c> (the baseline) calls
/// <c>EnumValueParser.TryParse</c> in <c>EnumReadMode.Input</c> with those conventions: wire name exact, then member name
/// exact (the second probe hits). <c>CSharpEssentials_IsDefinedName</c> is the generated case-sensitive member-name check,
/// which returns only a bool; parity compares its bool with the others' success.
/// IgnoreCase parses "pendingapproval" or "unknown" with the generated <c>TryParseWire</c> (the baseline there).
/// </para>
/// <para>
/// Semantics differences, kept out of the inputs on purpose: CSE parsing also accepts snake_case wire names, aliases and
/// the number of a defined member, and rejects undefined numbers ("99"), whitespace and comma lists; <c>TryParseWire</c>
/// always ignores case. BCL and Enums.NET accept any number in range and trim whitespace; BCL also parses "A, B" lists on
/// any enum, Enums.NET only on [Flags] enums; NetEscapades accepts any int and parses neither whitespace nor lists.
/// <c>TryParseWire</c> lookup path for these inputs: wire name exact, wire name ignore-case, then member name ignore-case
/// (the third probe hits). An invalid input misses every lookup and builds an <c>EnumValueError</c> that is discarded, so
/// the CSE invalid rows allocate by design.
/// </para>
/// <para>
/// On net8.0 CSE runs its netstandard2.1 build (the package has no net8 target), whose name lookup is a
/// <c>Dictionary&lt;string, ...&gt;</c> keyed by <c>span.ToString()</c>: every probe allocates a copy of the input
/// (<c>TryParseWire</c> makes 3 on the valid input and 4 on the invalid one), while net9.0+ uses an allocation-free
/// <c>FrozenDictionary</c> alternate lookup.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ParseComparison
{
    private const string CaseSensitive = "CaseSensitive";
    private const string IgnoreCase = "IgnoreCase";

    private static readonly EnumConventions CaseSensitiveInput = EnumConventions.Default with { CaseInsensitive = false };

    private string _exact = null!;
    private string _lower = null!;

    [Params(true, false)]
    public bool Valid { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _exact = Valid ? "PendingApproval" : "Unknown";
        _lower = Valid ? "pendingapproval" : "unknown";
        int expected = Valid ? (int)Status.PendingApproval : -1;
        Parity.Same(CaseSensitive,
            ("expected", Parity.Of(expected)),
            (nameof(CSharpEssentials_CaseSensitive), Parity.Of(CSharpEssentials_CaseSensitive())),
            (nameof(Bcl_CaseSensitive), Parity.Of(Bcl_CaseSensitive())),
            (nameof(EnumsNet_CaseSensitive), Parity.Of(EnumsNet_CaseSensitive())),
            (nameof(NetEscapades_CaseSensitive), Parity.Of(NetEscapades_CaseSensitive())));
        Parity.Same(CaseSensitive + " success",
            ("expected", Parity.Of(Valid)),
            (nameof(CSharpEssentials_IsDefinedName), Parity.Of(CSharpEssentials_IsDefinedName())));
        Parity.Same(IgnoreCase,
            ("expected", Parity.Of(expected)),
            (nameof(CSharpEssentials_IgnoreCase), Parity.Of(CSharpEssentials_IgnoreCase())),
            (nameof(Bcl_IgnoreCase), Parity.Of(Bcl_IgnoreCase())),
            (nameof(EnumsNet_IgnoreCase), Parity.Of(EnumsNet_IgnoreCase())),
            (nameof(NetEscapades_IgnoreCase), Parity.Of(NetEscapades_IgnoreCase())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#conventions-and-runtime-api
    [Benchmark(Baseline = true), BenchmarkCategory(CaseSensitive)]
    public int CSharpEssentials_CaseSensitive() =>
        EnumValueParser.TryParse(_exact, EnumReadMode.Input, CaseSensitiveInput, out Status v, out _) ? (int)v : -1;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
    [Benchmark, BenchmarkCategory(CaseSensitive)]
    public bool CSharpEssentials_IsDefinedName() => StatusExtensions.IsDefined(_exact);

    // https://learn.microsoft.com/dotnet/api/system.enum.tryparse
    [Benchmark, BenchmarkCategory(CaseSensitive)]
    public int Bcl_CaseSensitive() => Enum.TryParse(_exact, ignoreCase: false, out Status v) ? (int)v : -1;

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L1370
    [Benchmark, BenchmarkCategory(CaseSensitive)]
    public int EnumsNet_CaseSensitive() => EnumsNET.Enums.TryParse(_exact, ignoreCase: false, out Status v) ? (int)v : -1;

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark, BenchmarkCategory(CaseSensitive)]
    public int NetEscapades_CaseSensitive() => StatusFastExtensions.TryParse(_exact, out Status v) ? (int)v : -1;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
    [Benchmark(Baseline = true), BenchmarkCategory(IgnoreCase)]
    public int CSharpEssentials_IgnoreCase() => StatusExtensions.TryParseWire(_lower, out Status v) ? (int)v : -1;

    // https://learn.microsoft.com/dotnet/api/system.enum.tryparse
    [Benchmark, BenchmarkCategory(IgnoreCase)]
    public int Bcl_IgnoreCase() => Enum.TryParse(_lower, ignoreCase: true, out Status v) ? (int)v : -1;

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L1370
    [Benchmark, BenchmarkCategory(IgnoreCase)]
    public int EnumsNet_IgnoreCase() => EnumsNET.Enums.TryParse(_lower, ignoreCase: true, out Status v) ? (int)v : -1;

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark, BenchmarkCategory(IgnoreCase)]
    public int NetEscapades_IgnoreCase() => StatusFastExtensions.TryParse(_lower, out Status v, ignoreCase: true) ? (int)v : -1;
}
