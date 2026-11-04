using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// Writes and reads <c>Read | Write</c>. Not like-for-like: the wire formats differ. CSE writes the JSON array
/// <c>["read","write"]</c>; System.Text.Json and Macross write the comma string <c>"read, write"</c>. Each row reads its
/// own payload, and parity compares the decoded value, never the text. The setup also pins each payload.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EnumFlagsComparison
{
    private const string FlagsWrite = "FlagsWrite";
    private const string FlagsRead = "FlagsRead";
    private const Permissions Value = Permissions.Read | Permissions.Write;

    private string _cseJson = null!;
    private string _bclJson = null!;
    private string _macrossJson = null!;

    [GlobalSetup]
    public void Setup()
    {
        _cseJson = CSharpEssentials_FlagsWrite();
        _bclJson = Bcl_FlagsWrite();
        _macrossJson = Macross_FlagsWrite();
        Parity.Same(FlagsWrite + " CSE payload", ("expected", "[\"read\",\"write\"]"), (nameof(CSharpEssentials_FlagsWrite), _cseJson));
        Parity.Same(FlagsWrite + " comma payload",
            ("expected", "\"read, write\""),
            (nameof(Bcl_FlagsWrite), _bclJson),
            (nameof(Macross_FlagsWrite), _macrossJson));
        Parity.Same(FlagsRead,
            ("expected", Value.ToString()),
            (nameof(CSharpEssentials_FlagsRead), CSharpEssentials_FlagsRead().ToString()),
            (nameof(Bcl_FlagsRead), Bcl_FlagsRead().ToString()),
            (nameof(Macross_FlagsRead), Macross_FlagsRead().ToString()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    [Benchmark(Baseline = true), BenchmarkCategory(FlagsWrite)]
    public string CSharpEssentials_FlagsWrite() => JsonSerializer.Serialize(Value, JsonOptionsSet.CseData);

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings
    [Benchmark, BenchmarkCategory(FlagsWrite)]
    public string Bcl_FlagsWrite() => JsonSerializer.Serialize(Value, JsonOptionsSet.Bcl);

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    [Benchmark, BenchmarkCategory(FlagsWrite)]
    public string Macross_FlagsWrite() => JsonSerializer.Serialize(Value, JsonOptionsSet.Macross);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    [Benchmark(Baseline = true), BenchmarkCategory(FlagsRead)]
    public Permissions CSharpEssentials_FlagsRead() => JsonSerializer.Deserialize<Permissions>(_cseJson, JsonOptionsSet.CseData);

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings
    [Benchmark, BenchmarkCategory(FlagsRead)]
    public Permissions Bcl_FlagsRead() => JsonSerializer.Deserialize<Permissions>(_bclJson, JsonOptionsSet.Bcl);

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    [Benchmark, BenchmarkCategory(FlagsRead)]
    public Permissions Macross_FlagsRead() => JsonSerializer.Deserialize<Permissions>(_macrossJson, JsonOptionsSet.Macross);
}
