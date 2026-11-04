namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>Rule context: the first five properties feed the five-rule set, all ten feed the ten-rule set.</summary>
public sealed class LoanApplication
{
    public int Age { get; set; }

    public decimal Income { get; set; }

    public int CreditScore { get; set; }

    public string Country { get; set; } = "";

    public bool HasDefaults { get; set; }

    public int Dependents { get; set; }

    public decimal Debt { get; set; }

    public int EmploymentYears { get; set; }

    public bool Verified { get; set; }

    public string Email { get; set; } = "";
}
