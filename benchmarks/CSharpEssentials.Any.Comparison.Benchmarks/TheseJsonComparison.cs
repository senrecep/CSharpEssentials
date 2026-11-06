using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.These;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Serializes a <c>These&lt;string, int&gt;</c> in each state with System.Text.Json and deserializes it back, against
/// the hand-written <see cref="Outcome"/>, which carries the same <c>[JsonPropertyName]</c>s. The wire format is identical
/// (<c>{"isLeft":true,"isRight":true,"left":"warn","right":42}</c> for Both), so parity asserts the two JSON strings
/// are equal and compares the decoded flags and payloads. Both use one cached default <see cref="JsonSerializerOptions"/>.
/// CSE binds through a private <c>[JsonConstructor]</c>; <see cref="Outcome"/> through its public record constructor.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TheseJsonComparison
{
    private const string Serialize = "Serialize";
    private const string Deserialize = "Deserialize";

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/configure-options#reuse-jsonserializeroptions-instances
    private static readonly JsonSerializerOptions Options = new();

    private These<string, int> _these;
    private Outcome _outcome;
    private string _theseJson = null!;
    private string _outcomeJson = null!;

    [Params(TheseState.Left, TheseState.Right, TheseState.Both)]
    public TheseState State { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _these = TheseInputs.Cse(State);
        _outcome = TheseInputs.HandWritten(State);
        _theseJson = CSharpEssentials_Serialize();
        _outcomeJson = HandWritten_Serialize();
        Parity.Same(Serialize, (nameof(CSharpEssentials_Serialize), _theseJson), (nameof(HandWritten_Serialize), _outcomeJson));
        Parity.Same(Deserialize,
            ("expected", Decode(_outcome)),
            (nameof(CSharpEssentials_Deserialize), Decode(CSharpEssentials_Deserialize())),
            (nameof(HandWritten_Deserialize), Decode(HandWritten_Deserialize())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.These/Readme.MD#features
    [Benchmark(Baseline = true), BenchmarkCategory(Serialize)]
    public string CSharpEssentials_Serialize() => JsonSerializer.Serialize(_these, Options);

    [Benchmark, BenchmarkCategory(Serialize)]
    public string HandWritten_Serialize() => JsonSerializer.Serialize(_outcome, Options);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.These/Readme.MD#features
    [Benchmark(Baseline = true), BenchmarkCategory(Deserialize)]
    public These<string, int> CSharpEssentials_Deserialize() => JsonSerializer.Deserialize<These<string, int>>(_theseJson, Options);

    [Benchmark, BenchmarkCategory(Deserialize)]
    public Outcome HandWritten_Deserialize() => JsonSerializer.Deserialize<Outcome>(_outcomeJson, Options);

    private static string Decode(These<string, int> t) => Decode(new Outcome(t.HasLeft, t.HasRight, t.LeftOrDefault, t.RightOrDefault));

    private static string Decode(Outcome o) =>
        $"{Parity.Of(o.HasLeft ? 1 : 0)}{Parity.Of(o.HasRight ? 1 : 0)}:{Parity.Of(o.LeftOrDefault)}:{Parity.Of(o.RightOrDefault)}";
}
