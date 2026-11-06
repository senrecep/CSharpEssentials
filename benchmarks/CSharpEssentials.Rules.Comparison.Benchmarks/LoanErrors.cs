namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>Domain error catalogue, one static <see cref="Error"/> per rule (the Readme's domain error hierarchy pattern).</summary>
public static class LoanErrors
{
    public static readonly Error Age = Error.Validation("Loan.Age", "Applicant must be at least 18.");
    public static readonly Error Income = Error.Validation("Loan.Income", "Income must be at least 30000.");
    public static readonly Error CreditScore = Error.Validation("Loan.CreditScore", "Credit score must be at least 650.");
    public static readonly Error Country = Error.Validation("Loan.Country", "Loans are offered in DE only.");
    public static readonly Error Defaults = Error.Validation("Loan.Defaults", "Applicant has defaults.");
    public static readonly Error Dependents = Error.Validation("Loan.Dependents", "At most 4 dependents.");
    public static readonly Error Debt = Error.Validation("Loan.Debt", "Debt must be at most 50000.");
    public static readonly Error Employment = Error.Validation("Loan.Employment", "At least 2 years of employment.");
    public static readonly Error Verified = Error.Validation("Loan.Verified", "Applicant must be verified.");
    public static readonly Error Email = Error.Validation("Loan.Email", "Email must contain '@'.");

    public static readonly Error[] AllFive = [Age, Income, CreditScore, Country, Defaults];
    public static readonly Error[] AllTen = [Age, Income, CreditScore, Country, Defaults, Dependents, Debt, Employment, Verified, Email];
}
