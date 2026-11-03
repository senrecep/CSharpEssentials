using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using OneOf;
using OneOf.Serialization.SystemTextJson;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Serializes a two-case union holding 42 or "forty-two" with System.Text.Json, and deserializes each library's own
/// payload back. Each library has one cached <see cref="JsonSerializerOptions"/>. Parity decodes every payload through the
/// library's own access API and compares the case index and value ("0:42" or "1:forty-two"), never the JSON text.
/// <para>
/// The wire formats differ, so the payloads are not the same length: CSE writes <c>{"Index":0,"Value":42}</c>, the OneOf
/// add-on <c>{"Int32":42}</c>, Dunet <c>{"$type":"Number","Value":42}</c>. OneOf ships no System.Text.Json support; its
/// column is the third-party OneOf.Serialization.SystemTextJson add-on, which reads through a reflection-built converter.
/// CSE deserializes the value as a <see cref="JsonElement"/>, then re-parses its raw text against the case type. Dunet
/// uses System.Text.Json's built-in polymorphism.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class JsonComparison
{
    private const string Serialize = "Serialize";
    private const string Deserialize = "Deserialize";

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/configure-options#reuse-jsonserializeroptions-instances
    private static readonly JsonSerializerOptions CseOptions = new();

    // https://github.com/Liversage/OneOf.Serialization.SystemTextJson/blob/58a64dbed31b10734fe88b51952bb8d14e054c80/README.md#usage
    private static readonly JsonSerializerOptions OneOfOptions = new() { Converters = { new OneOfJsonConverter() } };

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization
    private static readonly JsonSerializerOptions DunetOptions = new();

    private Any<int, string> _cse;
    private OneOf<int, string> _oneOf;
    private IntOrText _dunet = null!;
    private string _cseJson = null!;
    private string _oneOfJson = null!;
    private string _dunetJson = null!;

    [Params(Inputs.IntBranch, Inputs.TextBranch)]
    public string Branch { get; set; } = Inputs.IntBranch;

    [GlobalSetup]
    public void Setup()
    {
        if (Branch == Inputs.IntBranch)
        {
            _cse = Inputs.IntValue;
            _oneOf = Inputs.IntValue;
            _dunet = Inputs.IntValue;
        }
        else
        {
            _cse = Inputs.TextValue;
            _oneOf = Inputs.TextValue;
            _dunet = Inputs.TextValue;
        }

        _cseJson = CSharpEssentials_Serialize();
        _oneOfJson = OneOf_Serialize();
        _dunetJson = Dunet_Serialize();
        string expected = Branch == Inputs.IntBranch ? "0:42" : "1:" + Inputs.TextValue;
        Parity.Same(Serialize,
            ("expected", expected),
            (nameof(CSharpEssentials_Serialize), Decode(JsonSerializer.Deserialize<Any<int, string>>(_cseJson, CseOptions))),
            (nameof(OneOf_Serialize), Decode(JsonSerializer.Deserialize<OneOf<int, string>>(_oneOfJson, OneOfOptions))),
            (nameof(Dunet_Serialize), Decode(JsonSerializer.Deserialize<IntOrText>(_dunetJson, DunetOptions)!)));
        Parity.Same(Deserialize,
            ("expected", expected),
            (nameof(CSharpEssentials_Deserialize), Decode(CSharpEssentials_Deserialize())),
            (nameof(OneOf_Deserialize), Decode(OneOf_Deserialize())),
            (nameof(Dunet_Deserialize), Decode(Dunet_Deserialize())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#type-checking
    [Benchmark(Baseline = true), BenchmarkCategory(Serialize)]
    public string CSharpEssentials_Serialize() => JsonSerializer.Serialize(_cse, CseOptions);

    // https://github.com/Liversage/OneOf.Serialization.SystemTextJson/blob/58a64dbed31b10734fe88b51952bb8d14e054c80/README.md#usage
    [Benchmark, BenchmarkCategory(Serialize)]
    public string OneOf_Serialize() => JsonSerializer.Serialize(_oneOf, OneOfOptions);

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization
    [Benchmark, BenchmarkCategory(Serialize)]
    public string Dunet_Serialize() => JsonSerializer.Serialize(_dunet, DunetOptions);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#type-checking
    [Benchmark(Baseline = true), BenchmarkCategory(Deserialize)]
    public Any<int, string> CSharpEssentials_Deserialize() => JsonSerializer.Deserialize<Any<int, string>>(_cseJson, CseOptions);

    // https://github.com/Liversage/OneOf.Serialization.SystemTextJson/blob/58a64dbed31b10734fe88b51952bb8d14e054c80/README.md#usage
    [Benchmark, BenchmarkCategory(Deserialize)]
    public OneOf<int, string> OneOf_Deserialize() => JsonSerializer.Deserialize<OneOf<int, string>>(_oneOfJson, OneOfOptions);

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization
    [Benchmark, BenchmarkCategory(Deserialize)]
    public IntOrText Dunet_Deserialize() => JsonSerializer.Deserialize<IntOrText>(_dunetJson, DunetOptions)!;

    private static string Decode(Any<int, string> u) =>
        u.Match(first: static v => "0:" + Parity.Of(v), second: static s => "1:" + s).Result!;

    private static string Decode(OneOf<int, string> u) => u.Match(static v => "0:" + Parity.Of(v), static s => "1:" + s);

    private static string Decode(IntOrText u) => u.Match(static n => "0:" + Parity.Of(n.Value), static t => "1:" + t.Value);
}
