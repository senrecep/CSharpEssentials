using System.Globalization;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// Reads a <see cref="DateTime"/>. Iso is an ISO 8601 timestamp with a millisecond fraction, which every row reads to the
/// same UTC value. LateList is <c>14-Mar-2024</c>, a format near the end of CSE's fixed list; the built-in converter
/// rejects it, so it has no row there, and the documented fallback converter reads it through <c>DateTime.Parse</c>
/// culture heuristics instead of a format list. Parity compares ticks and <see cref="DateTimeKind"/>.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class DateTimeComparison
{
    private const string Iso = "Iso";
    private const string LateList = "LateList";

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#multi-format-datetime
    private static readonly JsonSerializerOptions CseOptions = new() { Converters = { new MultiFormatDateTimeConverterFactory() } };

    // https://learn.microsoft.com/dotnet/standard/datetime/system-text-json-support#support-for-the-iso-8601-12019-format
    private static readonly JsonSerializerOptions BclOptions = new();

    // https://learn.microsoft.com/dotnet/standard/datetime/system-text-json-support#use-datetimeoffsetparse-as-a-fallback
    private static readonly JsonSerializerOptions FallbackOptions = new() { Converters = { new FallbackDateTimeConverter() } };

    [GlobalSetup]
    public void Setup()
    {
        Parity.Same(Iso,
            ("expected", Of(new DateTime(2024, 3, 14, 15, 30, 45, 123, DateTimeKind.Utc))),
            (nameof(CSharpEssentials_Iso), Of(CSharpEssentials_Iso())),
            (nameof(Bcl_Iso), Of(Bcl_Iso())),
            (nameof(BclFallback_Iso), Of(BclFallback_Iso())));
        Parity.Same(LateList,
            ("expected", Of(new DateTime(2024, 3, 14, 0, 0, 0, DateTimeKind.Utc))),
            (nameof(CSharpEssentials_LateList), Of(CSharpEssentials_LateList())),
            (nameof(BclFallback_LateList), Of(BclFallback_LateList())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#multi-format-datetime
    [Benchmark(Baseline = true), BenchmarkCategory(Iso)]
    public DateTime CSharpEssentials_Iso() => JsonSerializer.Deserialize<DateTime>(Inputs.IsoJson, CseOptions);

    // https://learn.microsoft.com/dotnet/standard/datetime/system-text-json-support#support-for-the-iso-8601-12019-format
    [Benchmark, BenchmarkCategory(Iso)]
    public DateTime Bcl_Iso() => JsonSerializer.Deserialize<DateTime>(Inputs.IsoJson, BclOptions);

    // https://learn.microsoft.com/dotnet/standard/datetime/system-text-json-support#use-datetimeoffsetparse-as-a-fallback
    [Benchmark, BenchmarkCategory(Iso)]
    public DateTime BclFallback_Iso() => JsonSerializer.Deserialize<DateTime>(Inputs.IsoJson, FallbackOptions);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#multi-format-datetime
    [Benchmark(Baseline = true), BenchmarkCategory(LateList)]
    public DateTime CSharpEssentials_LateList() => JsonSerializer.Deserialize<DateTime>(Inputs.LateListJson, CseOptions);

    // https://learn.microsoft.com/dotnet/standard/datetime/system-text-json-support#use-datetimeoffsetparse-as-a-fallback
    [Benchmark, BenchmarkCategory(LateList)]
    public DateTime BclFallback_LateList() => JsonSerializer.Deserialize<DateTime>(Inputs.LateListJson, FallbackOptions);

    private static string Of(DateTime value) =>
        value.Ticks.ToString(CultureInfo.InvariantCulture) + ":" + value.Kind.ToString();
}
