namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>The response of the idempotent replay rows, sized so the JSON has an exact length.</summary>
[Serializable]
public sealed record ReplayPayload(string Id, string Data);
