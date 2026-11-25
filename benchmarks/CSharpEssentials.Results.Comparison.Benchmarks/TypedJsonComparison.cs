using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using OneOf.Serialization.SystemTextJson;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Serializes a four-case result holding 42 or the cached not-found error with System.Text.Json, and deserializes each
/// library's own payload back. Each library has one cached <see cref="JsonSerializerOptions"/>. Parity decodes every payload
/// through the library's own access API and compares the status code, never the JSON text.
/// <para>
/// The wire formats differ: CSE writes <c>{"case":"NotFoundError","value":{...}}</c>, the OneOf add-on
/// <c>{"NotFoundError":{...}}</c>, Dunet <c>{"$type":"Missing","Error":{...}}</c>. OneOf ships no System.Text.Json support; its
/// column is the third-party OneOf.Serialization.SystemTextJson add-on. Dunet uses System.Text.Json's built-in polymorphism.
/// <see cref="Result{TValue}"/>, ErrorOr, CFE and FluentResults have no System.Text.Json converter for their results, so they
/// have no column.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TypedJsonComparison
{
    private const string Serialize = "Serialize";
    private const string Deserialize = "Deserialize";

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/configure-options#reuse-jsonserializeroptions-instances
    private static readonly JsonSerializerOptions CseOptions = new();

    // https://github.com/Liversage/OneOf.Serialization.SystemTextJson/blob/58a64dbed31b10734fe88b51952bb8d14e054c80/README.md#usage
    private static readonly JsonSerializerOptions OneOfOptions = new() { Converters = { new OneOfJsonConverter() } };

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization
    private static readonly JsonSerializerOptions DunetOptions = new();

    private Typed _typed;
    private TypedOneOf _oneOf;
    private OrderOutcome _dunet = null!;
    private string _typedJson = null!;
    private string _oneOfJson = null!;
    private string _dunetJson = null!;

    [Params(TypedInputs.Success, TypedInputs.NotFound)]
    public string Case { get; set; } = TypedInputs.Success;

    [GlobalSetup]
    public void Setup()
    {
        _typed = TypedInputs.Cse(Case);
        _oneOf = TypedInputs.OneOf(Case);
        _dunet = TypedInputs.Dunet(Case);
        _typedJson = CSharpEssentials_Typed_Serialize();
        _oneOfJson = OneOf_Serialize();
        _dunetJson = Dunet_Serialize();
        string expected = Parity.Of(TypedInputs.Expected(Case));
        Parity.Same(Serialize,
            ("expected", expected),
            (nameof(CSharpEssentials_Typed_Serialize), Parity.Of(TypedInputs.Read(JsonSerializer.Deserialize<Typed>(_typedJson, CseOptions)))),
            (nameof(OneOf_Serialize), Parity.Of(TypedInputs.Read(JsonSerializer.Deserialize<TypedOneOf>(_oneOfJson, OneOfOptions)))),
            (nameof(Dunet_Serialize), Parity.Of(TypedInputs.Read(JsonSerializer.Deserialize<OrderOutcome>(_dunetJson, DunetOptions)!))));
        Parity.Same(Deserialize,
            ("expected", expected),
            (nameof(CSharpEssentials_Typed_Deserialize), Parity.Of(TypedInputs.Read(CSharpEssentials_Typed_Deserialize()))),
            (nameof(OneOf_Deserialize), Parity.Of(TypedInputs.Read(OneOf_Deserialize()))),
            (nameof(Dunet_Deserialize), Parity.Of(TypedInputs.Read(Dunet_Deserialize()))));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#json
    [Benchmark(Baseline = true), BenchmarkCategory(Serialize)]
    public string CSharpEssentials_Typed_Serialize() => JsonSerializer.Serialize(_typed, CseOptions);

    // https://github.com/Liversage/OneOf.Serialization.SystemTextJson/blob/58a64dbed31b10734fe88b51952bb8d14e054c80/README.md#usage
    [Benchmark, BenchmarkCategory(Serialize)]
    public string OneOf_Serialize() => JsonSerializer.Serialize(_oneOf, OneOfOptions);

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization
    [Benchmark, BenchmarkCategory(Serialize)]
    public string Dunet_Serialize() => JsonSerializer.Serialize(_dunet, DunetOptions);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#json
    [Benchmark(Baseline = true), BenchmarkCategory(Deserialize)]
    public Typed CSharpEssentials_Typed_Deserialize() => JsonSerializer.Deserialize<Typed>(_typedJson, CseOptions);

    // https://github.com/Liversage/OneOf.Serialization.SystemTextJson/blob/58a64dbed31b10734fe88b51952bb8d14e054c80/README.md#usage
    [Benchmark, BenchmarkCategory(Deserialize)]
    public TypedOneOf OneOf_Deserialize() => JsonSerializer.Deserialize<TypedOneOf>(_oneOfJson, OneOfOptions);

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization
    [Benchmark, BenchmarkCategory(Deserialize)]
    public OrderOutcome Dunet_Deserialize() => JsonSerializer.Deserialize<OrderOutcome>(_dunetJson, DunetOptions)!;
}
