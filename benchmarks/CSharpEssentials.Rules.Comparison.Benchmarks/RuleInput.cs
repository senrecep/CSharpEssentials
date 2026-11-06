namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>Which rules the input fails.</summary>
public enum RuleInput
{
    /// <summary>Every rule passes; for an Or tree, only the last rule passes.</summary>
    Pass,

    /// <summary>Only the first rule fails.</summary>
    FailFirst,

    /// <summary>Only the last rule fails; for an Or tree, every rule fails.</summary>
    FailLast,

    /// <summary>Every rule fails.</summary>
    FailAll,
}
