namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>The prebuilt five-rule tree <see cref="SyncTreeComparison"/> evaluates.</summary>
public enum TreeShape
{
    /// <summary><c>Linear()</c>: stop at the first failure.</summary>
    Linear5,

    /// <summary>Non-generic <c>And()</c>: stop at the first failure.</summary>
    And5,

    /// <summary><c>Or()</c>: stop at the first success, aggregate when all fail.</summary>
    Or5,
}
