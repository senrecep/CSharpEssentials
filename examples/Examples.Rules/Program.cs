using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;

Applicant adult = new("Ada", Age: 30, HasLicense: true, Country: "TR");
Applicant minor = new("Bo", Age: 16, HasLicense: false, Country: "XX");

// 1. Three ways to write a rule: a class, a Func, a predicate
IRule<Applicant> ageRule = new MinimumAgeRule(18);
Func<Applicant, Result> licenseRule = a => a.HasLicense ? Result.Success() : ApplicantErrors.NoLicense;
IRule<Applicant> countryRule = RuleEngine.FromPredicate<Applicant>(
    a => a.Country is "TR" or "DE",
    a => Error.Forbidden("Applicant.Country", $"Country '{a.Country}' is not supported."));

Console.WriteLine("--- Single rules ---");
Print("class", RuleEngine.Evaluate(ageRule, minor));
Print("func", RuleEngine.Evaluate(licenseRule, minor));
Print("lambda", RuleEngine.Evaluate((Applicant a) => a.Name.Length > 1 ? Result.Success() : Error.Validation("Name.Short", "Name is too short."), adult));
Print("predicate", RuleEngine.Evaluate(countryRule, minor));

// 2. And: all must pass. Result rules stop at the first failure
Console.WriteLine("--- And ---");
IRuleBase<Applicant> all = new IRuleBase<Applicant>[] { ageRule, licenseRule.ToRule(), countryRule }.And();
Print("And adult", RuleEngine.Evaluate(all, adult));
Print("And minor", RuleEngine.Evaluate(all, minor));

// 3. Or: one must pass. When all fail, every error comes back
Console.WriteLine("--- Or ---");
IRuleBase<Applicant> any = new IRuleBase<Applicant>[] { ageRule, licenseRule.ToRule() }.Or();
Print("Or minor", RuleEngine.Evaluate(any, minor));

// 4. Linear and Next: run in order, stop at the first failure
Console.WriteLine("--- Linear / Next ---");
IRuleBase<Applicant> chain = new IRule<Applicant>[] { ageRule, countryRule }.Linear();
Print("Linear minor", RuleEngine.Evaluate(chain, minor));
IRule<Applicant> next = ageRule.Next(licenseRule).Next(countryRule);
Print("Next adult", RuleEngine.Evaluate(next, adult));
Print("Linear empty", RuleEngine.Evaluate(Array.Empty<IRule<Applicant>>().Linear(), adult));

// 5. Static shortcuts take the array and the context together
Console.WriteLine("--- Static shortcuts ---");
Func<Applicant, Result>[] checks = [licenseRule, a => a.Age >= 18 ? Result.Success() : ApplicantErrors.Underage];
Print("RuleEngine.And", RuleEngine.And(checks, minor));
Print("RuleEngine.Or", RuleEngine.Or(checks, adult));
Print("RuleEngine.Linear", RuleEngine.Linear(checks, minor));

// 6. If: pick a branch from a rule or a bool
Console.WriteLine("--- If ---");
Print("If(rule)", RuleEngine.If(
    ageRule,
    success: new GrantRule(),
    failure: RuleEngine.FromPredicate<Applicant>(_ => false, a => Error.Forbidden("Access.Denied", $"{a.Name} is too young.")),
    minor));
Print("If(bool)", RuleEngine.If(adult.HasLicense, new GrantRule(), ageRule, adult));
Print("If(funcs)", RuleEngine.If(
    (Applicant a) => a.Age >= 18 ? Result.Success() : Error.False,
    success: a => Result.Success(),
    failure: a => ApplicantErrors.Underage,
    minor));

// 7. Rules that return a value
Console.WriteLine("--- Result<T> rules ---");
Func<int, Result<string>>[] graders =
[
    score => score >= 90 ? "A" : Error.Validation("Grade.NotA", "Below 90."),
    score => score >= 80 ? "B" : Error.Validation("Grade.NotB", "Below 80."),
];
PrintValue("Or (first success)", RuleEngine.Or(graders, 85));
PrintValue("And (all checked)", RuleEngine.And(graders, 70));
Func<int, Result<string>>[] passing = [s => "first", s => "second"];
PrintValue("And (first value)", RuleEngine.And(passing, 0));

// 8. Async rules with EvaluateAsync
Console.WriteLine("--- Async ---");
HashSet<string> takenNames = ["admin", "root"];
IAsyncRule<string> uniqueName = RuleEngine.FromPredicateAsync<string>(
    async name => { await Task.Yield(); return !takenNames.Contains(name); },
    name => Error.Conflict("Name.Taken", $"'{name}' is already taken."));
IAsyncRule<string> notEmpty = ((Func<string, CancellationToken, ValueTask<Result>>)((name, ct) =>
    ValueTask.FromResult(name.Length > 0 ? Result.Success() : Error.Validation("Name.Empty", "Name is required.")))).ToRule();
IRuleBase<string> nameRules = new IRuleBase<string>[] { notEmpty, uniqueName, new UniqueNameRule(takenNames) }.And();
Print("EvaluateAsync 'ada'", await RuleEngine.EvaluateAsync(nameRules, "ada"));
Print("EvaluateAsync 'admin'", await RuleEngine.EvaluateAsync(nameRules, "admin"));

// 9. A throwing rule becomes an Unexpected error
Console.WriteLine("--- Exceptions ---");
Result thrown = RuleEngine.Evaluate((Applicant a) => throw new InvalidOperationException("boom"), adult);
Console.WriteLine($"{thrown.FirstError.Code} [{thrown.FirstError.Type}]");

// 10. A custom linear rule: Evaluate runs first, Next runs only on success
Console.WriteLine("--- Custom ILinearRule ---");
Print("custom linear", RuleEngine.Evaluate(new HasNameThen(ageRule), minor));

static void Print(string label, Result result) =>
    Console.WriteLine(result.IsSuccess
        ? $"{label}: success"
        : $"{label}: {string.Join("; ", result.Errors.Select(e => e.Code))}");

static void PrintValue<T>(string label, Result<T> result) =>
    Console.WriteLine(result.IsSuccess
        ? $"{label}: success ({result.Value})"
        : $"{label}: {string.Join("; ", result.Errors.Select(e => e.Code))}");

internal sealed record Applicant(string Name, int Age, bool HasLicense, string Country);

internal static class ApplicantErrors
{
    public static readonly Error Underage = Error.Validation("Applicant.Underage", "Applicant must be at least 18.");
    public static readonly Error NoLicense = Error.Validation("Applicant.NoLicense", "A driver's license is required.");
}

internal sealed class MinimumAgeRule(int minimum) : IRule<Applicant>
{
    public Result Evaluate(Applicant context, CancellationToken cancellationToken = default) =>
        context.Age >= minimum ? Result.Success() : ApplicantErrors.Underage;
}

internal sealed class GrantRule : IRule<Applicant>
{
    public Result Evaluate(Applicant context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"  access granted to {context.Name}");
        return Result.Success();
    }
}

internal sealed class UniqueNameRule(HashSet<string> taken) : IAsyncRule<string>
{
    public async ValueTask<Result> EvaluateAsync(string context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return taken.Contains(context) ? Error.Conflict("Name.Reserved", "Name is reserved.") : Result.Success();
    }
}

internal sealed class HasNameThen(IRuleBase<Applicant> next) : ILinearRule<Applicant>
{
    public IRuleBase<Applicant>? Next { get; } = next;

    public Result Evaluate(Applicant context, CancellationToken cancellationToken = default) =>
        string.IsNullOrWhiteSpace(context.Name) ? Error.Validation("Name.Empty", "Name is required.") : Result.Success();
}
