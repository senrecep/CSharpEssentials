using System.Text.Json.Serialization;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Hand-written baseline for the These comparison: the same four fields and JSON names as <c>These&lt;string, int&gt;</c>,
/// handled with plain if/else and no delegates. A value tuple cannot stand in for it, because System.Text.Json skips
/// tuple fields unless <c>IncludeFields</c> is set and would write <c>{}</c>.
/// </summary>
public readonly record struct Outcome(
    [property: JsonPropertyName("isLeft")] bool HasLeft,
    [property: JsonPropertyName("isRight")] bool HasRight,
    [property: JsonPropertyName("left")] string? LeftOrDefault,
    [property: JsonPropertyName("right")] int RightOrDefault);
