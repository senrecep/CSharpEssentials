namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>Contexts for each <see cref="RuleInput"/>.</summary>
public static class LoanInputs
{
    public static LoanApplication Pass() => new()
    {
        Age = 30,
        Income = 50000m,
        CreditScore = 700,
        Country = "DE",
        HasDefaults = false,
        Dependents = 2,
        Debt = 10000m,
        EmploymentYears = 5,
        Verified = true,
        Email = "applicant@example.com",
    };

    public static LoanApplication FailAll() => new()
    {
        Age = 16,
        Income = 1000m,
        CreditScore = 300,
        Country = "XX",
        HasDefaults = true,
        Dependents = 9,
        Debt = 99999m,
        EmploymentYears = 0,
        Verified = false,
        Email = "none",
    };

    public static LoanApplication FailFirst()
    {
        LoanApplication app = Pass();
        app.Age = 16;
        return app;
    }

    public static LoanApplication FailLastOfFive()
    {
        LoanApplication app = Pass();
        app.HasDefaults = true;
        return app;
    }

    public static LoanApplication FailLastOfTen()
    {
        LoanApplication app = Pass();
        app.Email = "none";
        return app;
    }

    public static LoanApplication OnlyLastOfFivePasses()
    {
        LoanApplication app = FailAll();
        app.HasDefaults = false;
        return app;
    }
}
