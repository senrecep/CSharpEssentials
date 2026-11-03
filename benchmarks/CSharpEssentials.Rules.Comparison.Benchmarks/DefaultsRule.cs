using NRules.Fluent.Dsl;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>NRules rule: records <c>Loan.Defaults</c> for an applicant with defaults.</summary>
public sealed class DefaultsRule : Rule
{
    public override void Define()
    {
        LoanApplication app = null!;
        Violations violations = null!;

        When()
            .Match(() => app, a => a.HasDefaults)
            .Match(() => violations);
        Then()
            .Do(_ => violations.Names.Add("Loan.Defaults"));
    }
}
