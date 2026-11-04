using NRules.Fluent.Dsl;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>NRules rule: records <c>Loan.Country</c> for an country other than DE.</summary>
public sealed class CountryRule : Rule
{
    public override void Define()
    {
        LoanApplication app = null!;
        Violations violations = null!;

        When()
            .Match(() => app, a => a.Country != "DE")
            .Match(() => violations);
        Then()
            .Do(_ => violations.Names.Add("Loan.Country"));
    }
}
