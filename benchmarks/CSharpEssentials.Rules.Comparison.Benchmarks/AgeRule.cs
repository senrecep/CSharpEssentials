using NRules.Fluent.Dsl;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>NRules rule: records <c>Loan.Age</c> for an age below 18.</summary>
public sealed class AgeRule : Rule
{
    public override void Define()
    {
        LoanApplication app = null!;
        Violations violations = null!;

        When()
            .Match(() => app, a => a.Age < 18)
            .Match(() => violations);
        Then()
            .Do(_ => violations.Names.Add("Loan.Age"));
    }
}
