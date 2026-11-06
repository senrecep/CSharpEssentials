using NRules.Fluent.Dsl;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>NRules rule: records <c>Loan.CreditScore</c> for an credit score below 650.</summary>
public sealed class CreditScoreRule : Rule
{
    public override void Define()
    {
        LoanApplication app = null!;
        Violations violations = null!;

        When()
            .Match(() => app, a => a.CreditScore < 650)
            .Match(() => violations);
        Then()
            .Do(_ => violations.Names.Add("Loan.CreditScore"));
    }
}
