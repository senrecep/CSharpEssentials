using NRules.Fluent.Dsl;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>NRules rule: records <c>Loan.Income</c> for an income below 30000.</summary>
public sealed class IncomeRule : Rule
{
    public override void Define()
    {
        LoanApplication app = null!;
        Violations violations = null!;

        When()
            .Match(() => app, a => a.Income < 30000m)
            .Match(() => violations);
        Then()
            .Do(_ => violations.Names.Add("Loan.Income"));
    }
}
