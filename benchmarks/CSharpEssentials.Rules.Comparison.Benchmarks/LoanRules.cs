namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// The rule sets as <c>Func</c> fields, the trees prebuilt once from them, and the hand-written if/else equivalents.
/// </summary>
public static class LoanRules
{
    public static readonly Func<LoanApplication, Result>[] Five =
    [
        c => c.Age >= 18 ? Result.Success() : LoanErrors.Age,
        c => c.Income >= 30000m ? Result.Success() : LoanErrors.Income,
        c => c.CreditScore >= 650 ? Result.Success() : LoanErrors.CreditScore,
        c => c.Country == "DE" ? Result.Success() : LoanErrors.Country,
        c => !c.HasDefaults ? Result.Success() : LoanErrors.Defaults,
    ];

    public static readonly Func<LoanApplication, Result>[] Ten =
    [
        .. Five,
        c => c.Dependents <= 4 ? Result.Success() : LoanErrors.Dependents,
        c => c.Debt <= 50000m ? Result.Success() : LoanErrors.Debt,
        c => c.EmploymentYears >= 2 ? Result.Success() : LoanErrors.Employment,
        c => c.Verified ? Result.Success() : LoanErrors.Verified,
        c => c.Email.Contains('@') ? Result.Success() : LoanErrors.Email,
    ];

    public static readonly Func<LoanApplication, Result<int>>[] TenGeneric =
    [
        c => c.Age >= 18 ? 1 : LoanErrors.Age,
        c => c.Income >= 30000m ? 2 : LoanErrors.Income,
        c => c.CreditScore >= 650 ? 3 : LoanErrors.CreditScore,
        c => c.Country == "DE" ? 4 : LoanErrors.Country,
        c => !c.HasDefaults ? 5 : LoanErrors.Defaults,
        c => c.Dependents <= 4 ? 6 : LoanErrors.Dependents,
        c => c.Debt <= 50000m ? 7 : LoanErrors.Debt,
        c => c.EmploymentYears >= 2 ? 8 : LoanErrors.Employment,
        c => c.Verified ? 9 : LoanErrors.Verified,
        c => c.Email.Contains('@') ? 10 : LoanErrors.Email,
    ];

    public static readonly IRule<LoanApplication>[] FiveRules = [.. Five.Select(r => r.ToRule())];

    public static readonly IRule<LoanApplication>[] TenRules = [.. Ten.Select(r => r.ToRule())];

    public static readonly IRuleBase<LoanApplication> Linear5 = FiveRules.Linear();

    public static readonly IRuleBase<LoanApplication> Linear10 = TenRules.Linear();

    public static readonly IRuleBase<LoanApplication> And5 = Five.And();

    public static readonly IRuleBase<LoanApplication> And10 = Ten.And();

    public static readonly IRuleBase<LoanApplication> Or5 = Five.Or();

    public static readonly IRuleBase<LoanApplication> Or10 = Ten.Or();

    public static readonly IRuleBase<LoanApplication, int> And10Generic = TenGeneric.And();

    public static readonly IRule<LoanApplication> AgeCondition = FiveRules[0];

    public static readonly IRuleBase<LoanApplication> AfterAge = FiveRules[1..].Linear();

    public static readonly IRuleBase<LoanApplication> Underage = ((Func<LoanApplication, Result>)(_ => LoanErrors.Age)).ToRule();

    public static readonly IAsyncRule<LoanApplication> AsyncDefaults = RuleEngine.FromPredicateAsync<LoanApplication>(
        async c =>
        {
            await Task.Yield();
            return !c.HasDefaults;
        },
        LoanErrors.Defaults);

    public static readonly IRuleBase<LoanApplication> MixedAnd5 =
        new IRuleBase<LoanApplication>[] { FiveRules[0], FiveRules[1], FiveRules[2], FiveRules[3], AsyncDefaults }.And();

    public static Result HandWrittenFive(LoanApplication c)
    {
        if (c.Age < 18)
            return LoanErrors.Age;
        if (c.Income < 30000m)
            return LoanErrors.Income;
        if (c.CreditScore < 650)
            return LoanErrors.CreditScore;
        if (c.Country != "DE")
            return LoanErrors.Country;
        if (c.HasDefaults)
            return LoanErrors.Defaults;
        return Result.Success();
    }

    public static Result HandWrittenTen(LoanApplication c)
    {
        Result first = HandWrittenFive(c);
        if (first.IsFailure)
            return first;
        if (c.Dependents > 4)
            return LoanErrors.Dependents;
        if (c.Debt > 50000m)
            return LoanErrors.Debt;
        if (c.EmploymentYears < 2)
            return LoanErrors.Employment;
        if (!c.Verified)
            return LoanErrors.Verified;
        if (!c.Email.Contains('@'))
            return LoanErrors.Email;
        return Result.Success();
    }

    public static Result HandWrittenOrFive(LoanApplication c)
    {
        if (c.Age >= 18 || c.Income >= 30000m || c.CreditScore >= 650 || c.Country == "DE" || !c.HasDefaults)
            return Result.Success();
        return LoanErrors.AllFive;
    }

    public static Result HandWrittenOrTen(LoanApplication c)
    {
        if (HandWrittenOrFive(c).IsSuccess || c.Dependents <= 4 || c.Debt <= 50000m || c.EmploymentYears >= 2 || c.Verified || c.Email.Contains('@'))
            return Result.Success();
        return LoanErrors.AllTen;
    }

    public static Result<int> HandWrittenAllTen(LoanApplication c)
    {
        List<Error>? errors = c.Age < 18 ? [LoanErrors.Age] : null;
        if (c.Income < 30000m)
            (errors ??= []).Add(LoanErrors.Income);
        if (c.CreditScore < 650)
            (errors ??= []).Add(LoanErrors.CreditScore);
        if (c.Country != "DE")
            (errors ??= []).Add(LoanErrors.Country);
        if (c.HasDefaults)
            (errors ??= []).Add(LoanErrors.Defaults);
        if (c.Dependents > 4)
            (errors ??= []).Add(LoanErrors.Dependents);
        if (c.Debt > 50000m)
            (errors ??= []).Add(LoanErrors.Debt);
        if (c.EmploymentYears < 2)
            (errors ??= []).Add(LoanErrors.Employment);
        if (!c.Verified)
            (errors ??= []).Add(LoanErrors.Verified);
        if (!c.Email.Contains('@'))
            (errors ??= []).Add(LoanErrors.Email);
        return errors is null ? 1 : errors;
    }

    public static async ValueTask<Result> HandWrittenMixedAsync(LoanApplication c)
    {
        if (c.Age < 18)
            return LoanErrors.Age;
        if (c.Income < 30000m)
            return LoanErrors.Income;
        if (c.CreditScore < 650)
            return LoanErrors.CreditScore;
        if (c.Country != "DE")
            return LoanErrors.Country;
        return await AsyncDefaults.EvaluateAsync(c).ConfigureAwait(false);
    }
}
