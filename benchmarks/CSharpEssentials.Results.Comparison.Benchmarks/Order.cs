namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>Input for the error-aggregation scenario: three independent checks on its fields.</summary>
public sealed record Order(int Id, decimal Total);
