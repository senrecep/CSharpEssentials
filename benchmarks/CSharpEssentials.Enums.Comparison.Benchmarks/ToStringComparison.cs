using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Enums.Comparison.Benchmarks.NetEscapadesGenerated;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// Formats a <see cref="Color"/> as its member name: a defined value (<see cref="Color.Indigo"/>, the seventh of eight
/// switch arms) and an undefined one (<c>(Color)99</c>). Every column returns "Indigo" and "99".
/// For the undefined value CSE <c>ToOptimizedString</c> falls back to <c>Enum.ToString()</c>, like the BCL column; both box
/// the enum (24 B on 64-bit) and return the runtime's cached "99". Enums.NET <c>AsString</c> and NetEscapades
/// <c>ToStringFast</c> format the unboxed underlying int, so they return the same cached string without allocating.
/// <c>CSharpEssentials_ToWireName</c> is different work (the snake_case wire name "indigo"), shown in its own WireName
/// category with no comparison claimed; it throws for an undefined value, so it has no row in that category.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ToStringComparison
{
    private const string Defined = "Defined";
    private const string Undefined = "Undefined";
    private const string WireName = "WireName";

    private Color _defined;
    private Color _undefined;

    [GlobalSetup]
    public void Setup()
    {
        _defined = Color.Indigo;
        _undefined = (Color)99;
        Parity.Same(Defined,
            (nameof(CSharpEssentials_Defined), CSharpEssentials_Defined()),
            (nameof(Bcl_Defined), Bcl_Defined()),
            (nameof(EnumsNet_Defined), EnumsNet_Defined()),
            (nameof(NetEscapades_Defined), NetEscapades_Defined()));
        Parity.Same(Undefined,
            (nameof(CSharpEssentials_Undefined), CSharpEssentials_Undefined()),
            (nameof(Bcl_Undefined), Bcl_Undefined()),
            (nameof(EnumsNet_Undefined), EnumsNet_Undefined()),
            (nameof(NetEscapades_Undefined), NetEscapades_Undefined()));
        Parity.Same(WireName, ("expected", "indigo"), (nameof(CSharpEssentials_ToWireName), CSharpEssentials_ToWireName()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
    [Benchmark(Baseline = true), BenchmarkCategory(Defined)]
    public string CSharpEssentials_Defined() => _defined.ToOptimizedString();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#wire-names
    [Benchmark, BenchmarkCategory(WireName)]
    public string CSharpEssentials_ToWireName() => _defined.ToWireName();

    // https://learn.microsoft.com/dotnet/api/system.enum.tostring
    [Benchmark, BenchmarkCategory(Defined)]
    public string Bcl_Defined() => _defined.ToString();

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L691
    [Benchmark, BenchmarkCategory(Defined)]
    public string EnumsNet_Defined() => EnumsNET.Enums.AsString(_defined);

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark, BenchmarkCategory(Defined)]
    public string NetEscapades_Defined() => ColorFastExtensions.ToStringFast(_defined);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
    [Benchmark(Baseline = true), BenchmarkCategory(Undefined)]
    public string CSharpEssentials_Undefined() => _undefined.ToOptimizedString();

    // https://learn.microsoft.com/dotnet/api/system.enum.tostring
    [Benchmark, BenchmarkCategory(Undefined)]
    public string Bcl_Undefined() => _undefined.ToString();

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L691
    [Benchmark, BenchmarkCategory(Undefined)]
    public string EnumsNet_Undefined() => EnumsNET.Enums.AsString(_undefined);

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark, BenchmarkCategory(Undefined)]
    public string NetEscapades_Undefined() => ColorFastExtensions.ToStringFast(_undefined);
}
