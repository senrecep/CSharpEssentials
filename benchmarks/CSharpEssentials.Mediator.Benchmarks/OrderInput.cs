namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Which <see cref="CreateOrder"/> a <see cref="BehaviorBenchmarks"/> row sends.</summary>
public enum OrderInput
{
    /// <summary>Passes validation; the handler returns Quantity + 1.</summary>
    Valid,

    /// <summary>Fails both rules; the handler never runs.</summary>
    Invalid,
}
