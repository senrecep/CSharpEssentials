using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// Serializes a dog typed as its abstract base and reads it back. CSE and System.Text.Json each get their own hierarchy
/// of the same shape: a pending CSE change leaves <c>[JsonPolymorphic]</c> types to System.Text.Json, so a shared type
/// would then measure System.Text.Json twice. The discriminator equals the CSE full type name and the class names have equal length,
/// so both payloads have the same shape and length, with <c>$type</c> first. CSE finds derived types by scanning the loaded
/// assemblies once (warmed in setup) and is reflection-based; System.Text.Json needs the declared derived-type list.
/// The CSE rows add <c>PolymorphicJsonConverterFactory</c> to plain Web options, not the Readme's <c>ConvertToJson()</c>/<c>DefaultOptions</c>,
/// which also turns on IgnoreCycles, relaxed escaping and ignore-nulls. Both rows then share the same base options,
/// and the number is the factory's cost alone.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class PolymorphicComparison
{
    private const string Serialize = "Serialize";
    private const string Deserialize = "Deserialize";
    private const string Expected = "Dog:Rex:Labrador";

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#polymorphic-serialization
    private static readonly JsonSerializerOptions CseOptions =
        new(JsonSerializerDefaults.Web) { Converters = { new PolymorphicJsonConverterFactory() } };

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/polymorphism#customize-the-type-discriminator-name
    private static readonly JsonSerializerOptions StjOptions = new(JsonSerializerDefaults.Web);

    private readonly CseAnimal _cse = new CseDog { Name = "Rex", Breed = "Labrador" };
    private readonly StjAnimal _stj = new StjDog { Name = "Rex", Breed = "Labrador" };
    private string _cseJson = null!;
    private string _stjJson = null!;

    [GlobalSetup]
    public void Setup()
    {
        _cseJson = CSharpEssentials_Serialize();
        _stjJson = Stj_Serialize();
        Parity.Same(Serialize + " length", (nameof(CSharpEssentials_Serialize), Parity.Of(_cseJson.Length)), (nameof(Stj_Serialize), Parity.Of(_stjJson.Length)));
        Parity.Same(Serialize,
            ("expected", Expected),
            (nameof(CSharpEssentials_Serialize), Decode(JsonSerializer.Deserialize<CseAnimal>(_cseJson, CseOptions)!)),
            (nameof(Stj_Serialize), Decode(JsonSerializer.Deserialize<StjAnimal>(_stjJson, StjOptions)!)));
        Parity.Same(Deserialize,
            ("expected", Expected),
            (nameof(CSharpEssentials_Deserialize), Decode(CSharpEssentials_Deserialize())),
            (nameof(Stj_Deserialize), Decode(Stj_Deserialize())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#polymorphic-serialization
    [Benchmark(Baseline = true), BenchmarkCategory(Serialize)]
    public string CSharpEssentials_Serialize() => JsonSerializer.Serialize(_cse, CseOptions);

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/polymorphism#customize-the-type-discriminator-name
    [Benchmark, BenchmarkCategory(Serialize)]
    public string Stj_Serialize() => JsonSerializer.Serialize(_stj, StjOptions);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#polymorphic-serialization
    [Benchmark(Baseline = true), BenchmarkCategory(Deserialize)]
    public CseAnimal CSharpEssentials_Deserialize() => JsonSerializer.Deserialize<CseAnimal>(_cseJson, CseOptions)!;

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/polymorphism#customize-the-type-discriminator-name
    [Benchmark, BenchmarkCategory(Deserialize)]
    public StjAnimal Stj_Deserialize() => JsonSerializer.Deserialize<StjAnimal>(_stjJson, StjOptions)!;

    private static string Decode(CseAnimal animal) => animal switch
    {
        CseDog dog => "Dog:" + dog.Name + ":" + dog.Breed,
        CseCat cat => "Cat:" + cat.Name + ":" + Parity.Of(cat.Lives),
        _ => "<other>",
    };

    private static string Decode(StjAnimal animal) => animal switch
    {
        StjDog dog => "Dog:" + dog.Name + ":" + dog.Breed,
        StjCat cat => "Cat:" + cat.Name + ":" + Parity.Of(cat.Lives),
        _ => "<other>",
    };
}
