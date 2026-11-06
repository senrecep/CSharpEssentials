namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>The body of the idempotent replay rows; <see cref="SerializableAttribute"/> per IdempotentAPI's Step 2.</summary>
[Serializable]
public sealed record CreateOrder(string OrderId, int Amount, string Currency);
