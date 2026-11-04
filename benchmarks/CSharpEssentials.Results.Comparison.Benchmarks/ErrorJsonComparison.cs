using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ErrorJsonComparison
{
    private const string SerializeCategory = "Serialize";
    private const string DeserializeCategory = "Deserialize";

    private static readonly JsonSerializerOptions Options = new();

    private Error _cse;
    private Eo.Error _errorOr;
    private string _json = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _cse = Error.Validation(ErrorInputs.Code, Inputs.Message);
        _errorOr = Eo.Error.Validation(ErrorInputs.Code, Inputs.Message);
        _json = CSharpEssentials_Serialize();

        Parity.Same(nameof(ErrorJsonComparison) + "." + SerializeCategory,
            (nameof(CSharpEssentials), _json),
            (nameof(ErrorOr), ErrorOr_Serialize()));
        Parity.Same(nameof(ErrorJsonComparison) + "." + DeserializeCategory,
            ("Expected", "True"),
            (nameof(CSharpEssentials), CSharpEssentials_Deserialize().Equals(_cse).ToString()));
    }

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/configure-options#reuse-jsonserializeroptions-instances
    [Benchmark(Baseline = true), BenchmarkCategory(SerializeCategory)]
    public string CSharpEssentials_Serialize() => JsonSerializer.Serialize(_cse, Options);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#using-systemtextjson
    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/configure-options#reuse-jsonserializeroptions-instances
    [Benchmark, BenchmarkCategory(SerializeCategory)]
    public string ErrorOr_Serialize() => JsonSerializer.Serialize(_errorOr, Options);

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/configure-options#reuse-jsonserializeroptions-instances
    [Benchmark(Baseline = true), BenchmarkCategory(DeserializeCategory)]
    public Error CSharpEssentials_Deserialize() => JsonSerializer.Deserialize<Error>(_json, Options);
}
