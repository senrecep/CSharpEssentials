# CSharpEssentials.Rules

Composable, functional rule engine for .NET. Define business logic as small, reusable rules and combine them with a fluent API.

## Features

- **Three definition styles** — Class, `Func` field, or inline lambda: all are first-class citizens.
- **No boilerplate conversions** — Pass functions directly to `RuleEngine`; no explicit `.ToRule()` required at call sites.
- **Linear Sequences** — Run rules in order, stop on first failure.
- **Conditional Rules** — If/Then/Else branching.
- **And / Or Logic** — All must pass, or at least one must pass.
- **Async Support** — First-class async rules.
- **Result Integration** — Built on `CSharpEssentials.Results`.

## Installation

```bash
dotnet add package CSharpEssentials.Rules
```

## Rule Definition Styles

There are three ways to define a rule. All styles are interchangeable — mix and match freely when composing.

### 1. Class (recommended for complex or injectable rules)

Implement `IRule<TContext>` when a rule needs constructor-injected dependencies, internal state, or readable naming across a large codebase.

```csharp
public sealed class AgeRule : IRule<UserContext>
{
    public Result Evaluate(UserContext ctx, CancellationToken ct = default) =>
        ctx.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be at least 18.");
}

public sealed class LicenseRule : IRule<UserContext>
{
    private readonly ILicenseRepository _repo;

    public LicenseRule(ILicenseRepository repo) => _repo = repo;

    public Result Evaluate(UserContext ctx, CancellationToken ct = default) =>
        _repo.IsValid(ctx.LicenseId) ? Result.Success() : Error.Validation("License.Invalid", "License not found.");
}
```

### 2. `Func` field (reusable, no class needed)

Declare rules as `Func<TContext, Result>` fields or variables. They can be stored, passed around, and used anywhere an `IRule<TContext>` is expected — with or without an explicit `.ToRule()` call.

```csharp
// Defined once, reused anywhere
Func<UserContext, Result> regionRule = ctx =>
    ctx.IsAllowedRegion ? Result.Success() : Error.Forbidden("Region.Blocked", "Not available in your region.");

// Named method — method group works too
static Result CheckEmail(UserContext ctx) =>
    ctx.Email.Contains('@') ? Result.Success() : Error.Validation("Email.Invalid", "Invalid email address.");
```

### 3. Inline lambda (best for one-off or ad-hoc composition)

Write the rule directly at the call site. No variable, no class — maximum density for simple checks.

```csharp
Result result = RuleEngine.Evaluate(
    (UserContext ctx) => ctx.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be at least 18."),
    userCtx);
```

---

## Passing Rules Without Explicit Conversion

`RuleEngine.Evaluate`, `.And()`, `.Or()`, and `.Next()` all accept raw `Func` delegates. You never need to wrap them manually.

```csharp
var ctx = new UserContext { Age = 20, HasLicense = true, IsAllowedRegion = true };

// Class instance
Result r1 = RuleEngine.Evaluate(new AgeRule(), ctx);

// Func variable — no .ToRule() needed
Result r2 = RuleEngine.Evaluate(regionRule, ctx);

// Inline lambda — no .ToRule() needed
Result r3 = RuleEngine.Evaluate(
    (UserContext c) => c.HasLicense ? Result.Success() : Error.Validation("License.Missing", "License required."),
    ctx);

// Named method group — works directly
Result r4 = RuleEngine.Evaluate(CheckEmail, ctx);
```

---

## Combining Rules

All three styles compose freely. Mix class instances, `Func` variables, and lambdas in the same call.

### And — all must pass (collects all failures)

```csharp
// Using class instances
Result andResult = RuleEngine.Evaluate(
    new IRuleBase<UserContext>[] { new AgeRule(), new LicenseRule(repo) }.And(),
    ctx);

// Using Func array — no .ToRule() needed at all
Result andResult2 = RuleEngine.Evaluate(
    new Func<UserContext, Result>[] { regionRule, CheckEmail }.And(),
    ctx);

// Mixed: class + lambda
Result andResult3 = RuleEngine.Evaluate(
    new IRuleBase<UserContext>[]
    {
        new AgeRule(),
        regionRule.ToRule(),
        ((Func<UserContext, Result>)(c => c.HasLicense ? Result.Success() : Error.Validation("License.Missing", "License required."))).ToRule()
    }.And(),
    ctx);
```

### Or — at least one must pass

```csharp
// Func array — direct, no conversion
Result orResult = RuleEngine.Evaluate(
    new Func<UserContext, Result>[] { regionRule, CheckEmail }.Or(),
    ctx);
```

### Linear — stop on first failure

```csharp
// Class instances
Result linear = RuleEngine.Evaluate(
    new IRule<UserContext>[] { new AgeRule(), new LicenseRule(repo) }.Linear(),
    ctx);

// Func chaining with .Next() — reads like a pipeline
Result pipeline = RuleEngine.Evaluate(
    ((Func<UserContext, Result>)CheckEmail)
        .Next(regionRule)
        .Next(c => c.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be at least 18.")),
    ctx);
```

### Conditional — if/then/else branching

```csharp
// Class instances
Result conditional = RuleEngine.If(
    new AgeRule(),
    success: new GrantAccessRule(),
    failure: new DenyAccessRule(),
    ctx);

// Func lambdas — all three branches inline
Result conditional2 = RuleEngine.If(
    (UserContext c) => c.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be at least 18."),
    success: c => Result.Success(),
    failure: c => Error.Forbidden("Access.Denied", "Access denied."),
    ctx);

// Boolean shorthand
Result conditional3 = RuleEngine.If(
    condition: ctx.IsAllowedRegion,
    success: new GrantAccessRule(),
    failure: new DenyAccessRule(),
    ctx);
```

---

## Rules with Values (`Result<T>`)

Rules can produce a typed result alongside success/failure.

```csharp
// Class form
public sealed class GradeRule : IRule<int, string>
{
    public Result<string> Evaluate(int score, CancellationToken ct = default)
    {
        if (score >= 90) return "A";
        if (score >= 80) return "B";
        return Error.Validation("Grade.Failed", "Score too low.");
    }
}

// Func form — no .ToRule() at call site
Result<string> grade = RuleEngine.Evaluate(
    (int score) => score >= 90 ? Result<string>.Success("A") : Error.Validation("Grade.Failed", "Score too low."),
    85);
```

---

## Integration with Domain Error Hierarchies

Rules integrate naturally with domain-specific `Error` factory classes, keeping validation logic close to domain language:

```csharp
public static class RegistrationErrors
{
    public static readonly Error Underage =
        Error.Validation("Registration.Underage", "Applicant must be at least 18.");

    public static readonly Error NoLicense =
        Error.Validation("Registration.NoLicense", "A valid driver's license is required.");

    public static readonly Error RegionBlocked =
        Error.Forbidden("Registration.RegionBlocked", "Registration is not available in your region.");
}

// Class-based rules referencing the domain error catalogue
public sealed class AgeRule : IRule<ApplicantContext>
{
    public Result Evaluate(ApplicantContext ctx, CancellationToken ct = default) =>
        ctx.Age >= 18 ? Result.Success() : RegistrationErrors.Underage;
}

// Func-based rule — same domain errors, no class needed
Func<ApplicantContext, Result> regionRule =
    ctx => ctx.IsAllowedRegion ? Result.Success() : RegistrationErrors.RegionBlocked;

// Compose: Func array runs all rules and collects all failures
var applicant = new ApplicantContext { Age = 16, HasLicense = false, IsAllowedRegion = true };

Result result = RuleEngine.Evaluate(
    new Func<ApplicantContext, Result>[]
    {
        c => c.Age >= 18 ? Result.Success() : RegistrationErrors.Underage,
        c => c.HasLicense    ? Result.Success() : RegistrationErrors.NoLicense,
        regionRule
    }.And(),
    applicant);

result.Match(
    onSuccess: () => Console.WriteLine("Approved"),
    onError: errors =>
    {
        foreach (Error e in errors)
            Console.WriteLine($"[{e.Type}] {e.Code}: {e.Description}");
    });
// [Validation] Registration.Underage: Applicant must be at least 18.
// [Validation] Registration.NoLicense: A valid driver's license is required.
```

Rules return `Result` / `Result<T>` — the same type used throughout CSharpEssentials — so rule outcomes compose directly with `Then`, `Match`, and other chaining operations.
