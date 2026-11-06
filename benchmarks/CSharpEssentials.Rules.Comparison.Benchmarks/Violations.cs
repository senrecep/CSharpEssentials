namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>NRules fact the rule actions write to: the names of the rules that fired.</summary>
public sealed class Violations
{
    public List<string> Names { get; } = [];
}
