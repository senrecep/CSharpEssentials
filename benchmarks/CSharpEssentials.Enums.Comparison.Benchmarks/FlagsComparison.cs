using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Enums.Comparison.Benchmarks.NetEscapadesGenerated;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// <see cref="Permissions"/> flags on the undeclared combination <c>Write | Execute | Delete</c>.
/// <para>
/// HasFlag checks for <see cref="Permissions.Write"/> (true). CSE generates no HasFlag helper, so this category has no CSE
/// column and BCL <c>Enum.HasFlag</c> (a JIT intrinsic, no boxing) is the baseline.
/// </para>
/// <para>
/// Format gives "Write, Execute, Delete" in every compared column. CSE <c>ToOptimizedString</c> and NetEscapades
/// <c>ToStringFast</c> both fall back to <c>Enum.ToString()</c> for a combination that is not a declared member; Enums.NET
/// <c>FormatFlags</c> joins its cached single-flag names. A combination that covers <see cref="Permissions.ReadWrite"/> would
/// differ: BCL prints "ReadWrite, Delete" while Enums.NET prints "Read, Write, Delete", so the input avoids it.
/// <c>CSharpEssentials_ToWireName</c> is different output ("write,execute,delete": wire names of single flags, no space),
/// shown in its own WireName category with no comparison claimed.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class FlagsComparison
{
    private const string HasFlag = "HasFlag";
    private const string Format = "Format";
    private const string WireName = "WireName";

    private Permissions _value;
    private Permissions _flag;

    [GlobalSetup]
    public void Setup()
    {
        _value = Permissions.Write | Permissions.Execute | Permissions.Delete;
        _flag = Permissions.Write;
        Parity.Same(HasFlag,
            ("expected", Parity.Of(true)),
            (nameof(Bcl_HasFlag), Parity.Of(Bcl_HasFlag())),
            (nameof(EnumsNet_HasFlag), Parity.Of(EnumsNet_HasFlag())),
            (nameof(NetEscapades_HasFlag), Parity.Of(NetEscapades_HasFlag())));
        Parity.Same(Format,
            ("expected", "Write, Execute, Delete"),
            (nameof(CSharpEssentials_Format), CSharpEssentials_Format()),
            (nameof(Bcl_Format), Bcl_Format()),
            (nameof(EnumsNet_Format), EnumsNet_Format()),
            (nameof(NetEscapades_Format), NetEscapades_Format()));
        Parity.Same(WireName, ("expected", "write,execute,delete"), (nameof(CSharpEssentials_ToWireName), CSharpEssentials_ToWireName()));
    }

    // https://learn.microsoft.com/dotnet/api/system.enum.hasflag
    [Benchmark(Baseline = true), BenchmarkCategory(HasFlag)]
    public bool Bcl_HasFlag() => _value.HasFlag(_flag);

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/FlagEnums.cs#L301
    [Benchmark, BenchmarkCategory(HasFlag)]
    public bool EnumsNet_HasFlag() => EnumsNET.FlagEnums.HasAllFlags(_value, _flag);

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark, BenchmarkCategory(HasFlag)]
    public bool NetEscapades_HasFlag() => PermissionsFastExtensions.HasFlagFast(_value, _flag);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
    [Benchmark(Baseline = true), BenchmarkCategory(Format)]
    public string CSharpEssentials_Format() => _value.ToOptimizedString();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#flags
    [Benchmark, BenchmarkCategory(WireName)]
    public string CSharpEssentials_ToWireName() => _value.ToWireName();

    // https://learn.microsoft.com/dotnet/api/system.enum.tostring
    [Benchmark, BenchmarkCategory(Format)]
    public string Bcl_Format() => _value.ToString();

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/FlagEnums.cs#L82
    [Benchmark, BenchmarkCategory(Format)]
    public string EnumsNet_Format() => EnumsNET.FlagEnums.FormatFlags(_value);

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark, BenchmarkCategory(Format)]
    public string NetEscapades_Format() => PermissionsFastExtensions.ToStringFast(_value);
}
