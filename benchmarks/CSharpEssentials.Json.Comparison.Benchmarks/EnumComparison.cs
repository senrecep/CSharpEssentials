using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// Writes and reads a snake_case string enum with System.Text.Json. Every library writes the same wire names, so Write
/// compares the JSON text and the read categories compare the decoded value. InvalidThrow reads an unknown string in each
/// library's strict mode; the outcome is the same <see cref="JsonException"/> (CSE throws a subclass), but the message and
/// payload each library builds differ, and the exception cost dominates. InvalidFallback has no BCL row: the built-in
/// converter has no fallback member.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EnumComparison
{
    private const string Write = "Write";
    private const string Read = "Read";
    private const string ReadNumber = "ReadNumber";
    private const string InvalidThrow = "InvalidThrow";
    private const string InvalidFallback = "InvalidFallback";

    [GlobalSetup]
    public void Setup()
    {
        Parity.Same(Write,
            ("expected", Inputs.ValuesJson),
            (nameof(CSharpEssentials_Write), CSharpEssentials_Write()),
            (nameof(Bcl_Write), Bcl_Write()),
            (nameof(Macross_Write), Macross_Write()));
        Parity.Same(Read,
            ("expected", string.Join(",", Inputs.Values)),
            (nameof(CSharpEssentials_Read), string.Join(",", CSharpEssentials_Read())),
            (nameof(Bcl_Read), string.Join(",", Bcl_Read())),
            (nameof(Macross_Read), string.Join(",", Macross_Read())));
        Parity.Same(ReadNumber,
            ("expected", nameof(OrderStatus.OnHold)),
            (nameof(CSharpEssentials_ReadNumber), CSharpEssentials_ReadNumber().ToString()),
            (nameof(Bcl_ReadNumber), Bcl_ReadNumber().ToString()),
            (nameof(Macross_ReadNumber), Macross_ReadNumber().ToString()));
        Parity.Same(InvalidThrow,
            ("expected", Parity.Of(-1)),
            (nameof(CSharpEssentials_InvalidThrow), Parity.Of(CSharpEssentials_InvalidThrow())),
            (nameof(Bcl_InvalidThrow), Parity.Of(Bcl_InvalidThrow())),
            (nameof(Macross_InvalidThrow), Parity.Of(Macross_InvalidThrow())));
        Parity.Same(InvalidFallback,
            ("expected", nameof(OrderStatus.Unknown)),
            (nameof(CSharpEssentials_InvalidFallback), CSharpEssentials_InvalidFallback().ToString()),
            (nameof(Macross_InvalidFallback), Macross_InvalidFallback().ToString()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    [Benchmark(Baseline = true), BenchmarkCategory(Write)]
    public string CSharpEssentials_Write() => JsonSerializer.Serialize(Inputs.Values, JsonOptionsSet.CseData);

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings
    [Benchmark, BenchmarkCategory(Write)]
    public string Bcl_Write() => JsonSerializer.Serialize(Inputs.Values, JsonOptionsSet.Bcl);

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    [Benchmark, BenchmarkCategory(Write)]
    public string Macross_Write() => JsonSerializer.Serialize(Inputs.Values, JsonOptionsSet.Macross);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    [Benchmark(Baseline = true), BenchmarkCategory(Read)]
    public OrderStatus[] CSharpEssentials_Read() => JsonSerializer.Deserialize<OrderStatus[]>(Inputs.ValuesJson, JsonOptionsSet.CseData)!;

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings
    [Benchmark, BenchmarkCategory(Read)]
    public OrderStatus[] Bcl_Read() => JsonSerializer.Deserialize<OrderStatus[]>(Inputs.ValuesJson, JsonOptionsSet.Bcl)!;

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    [Benchmark, BenchmarkCategory(Read)]
    public OrderStatus[] Macross_Read() => JsonSerializer.Deserialize<OrderStatus[]>(Inputs.ValuesJson, JsonOptionsSet.Macross)!;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    [Benchmark(Baseline = true), BenchmarkCategory(ReadNumber)]
    public OrderStatus CSharpEssentials_ReadNumber() => JsonSerializer.Deserialize<OrderStatus>(Inputs.NumberJson, JsonOptionsSet.CseData);

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings
    [Benchmark, BenchmarkCategory(ReadNumber)]
    public OrderStatus Bcl_ReadNumber() => JsonSerializer.Deserialize<OrderStatus>(Inputs.NumberJson, JsonOptionsSet.Bcl);

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    [Benchmark, BenchmarkCategory(ReadNumber)]
    public OrderStatus Macross_ReadNumber() => JsonSerializer.Deserialize<OrderStatus>(Inputs.NumberJson, JsonOptionsSet.Macross);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    [Benchmark(Baseline = true), BenchmarkCategory(InvalidThrow)]
    public int CSharpEssentials_InvalidThrow() => ReadOrMinusOne(JsonOptionsSet.CseInput);

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings
    [Benchmark, BenchmarkCategory(InvalidThrow)]
    public int Bcl_InvalidThrow() => ReadOrMinusOne(JsonOptionsSet.Bcl);

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    [Benchmark, BenchmarkCategory(InvalidThrow)]
    public int Macross_InvalidThrow() => ReadOrMinusOne(JsonOptionsSet.Macross);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    [Benchmark(Baseline = true), BenchmarkCategory(InvalidFallback)]
    public OrderStatus CSharpEssentials_InvalidFallback() => JsonSerializer.Deserialize<OrderStatus>(Inputs.UnknownJson, JsonOptionsSet.CseData);

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    [Benchmark, BenchmarkCategory(InvalidFallback)]
    public OrderStatus Macross_InvalidFallback() => JsonSerializer.Deserialize<OrderStatus>(Inputs.UnknownJson, JsonOptionsSet.MacrossFallback);

    private static int ReadOrMinusOne(JsonSerializerOptions options)
    {
        try
        {
            return (int)JsonSerializer.Deserialize<OrderStatus>(Inputs.UnknownJson, options);
        }
        catch (JsonException)
        {
            return -1;
        }
    }
}
