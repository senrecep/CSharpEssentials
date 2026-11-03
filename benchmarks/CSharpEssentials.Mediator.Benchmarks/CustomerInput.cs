namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Which <see cref="CreateCustomer"/> a <see cref="ValidationFanOutBenchmarks"/> row sends.</summary>
public enum CustomerInput
{
    /// <summary><c>("a", "a@b.c", 30)</c>: passes every validator; the handler returns Age + 1.</summary>
    Valid,

    /// <summary><c>("", "", 0)</c>: fails every validator; the handler never runs.</summary>
    Invalid,
}
