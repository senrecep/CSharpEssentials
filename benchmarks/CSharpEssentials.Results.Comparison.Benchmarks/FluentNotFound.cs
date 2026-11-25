namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>The FluentResults not-found error, a subclass as its README designs custom errors.</summary>
// https://github.com/altmann/FluentResults/blob/v4.0/README.md#designing-errors-and-success-messages
public sealed class FluentNotFound(string message) : Fr.Error(message);
