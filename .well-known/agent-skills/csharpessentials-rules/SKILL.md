---
name: csharpessentials-rules
description: Use when composing business validation logic. Define rules as classes, Func fields, or inline lambdas; combine with .And()/.Or()/.Linear()/.Next(); evaluate with RuleEngine.Evaluate(), or await RuleEngine.EvaluateAsync() when the tree contains async rules; branch with RuleEngine.If().
---

# CSharpEssentials.Rules

Composable rule engine for .NET. Define business logic as small rules in any style and combine them freely.

## Installation

```bash
dotnet add package CSharpEssentials.Rules
```

## Namespaces

```csharp
using CSharpEssentials.Rules;        // IRule<T>, RuleEngine
using CSharpEssentials.ResultPattern; // Result, Result<T>
using CSharpEssentials.Errors;       // Error
```

---

## Three Definition Styles

All styles are interchangeable; mix and match freely when composing.

### 1. Class (injectable, unit-testable)

```csharp
public sealed class AgeRule : IRule<UserContext>
{
    public Result Evaluate(UserContext ctx, CancellationToken ct = default) =>
        ctx.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be at least 18.");
}

// With constructor injection
public sealed class LicenseRule : IRule<UserContext>
{
    private readonly ILicenseRepository _repo;
    public LicenseRule(ILicenseRepository repo) => _repo = repo;

    public Result Evaluate(UserContext ctx, CancellationToken ct = default) =>
        _repo.IsValid(ctx.LicenseId) ? Result.Success() : Error.Validation("License.Invalid", "License not found.");
}
```

### 2. Func field (reusable, no class needed)

```csharp
Func<UserContext, Result> regionRule = ctx =>
    ctx.IsAllowedRegion ? Result.Success() : Error.Forbidden("Region.Blocked", "Not available in your region.");

static Result CheckEmail(UserContext ctx) =>
    ctx.Email.Contains('@') ? Result.Success() : Error.Validation("Email.Invalid", "Invalid email.");
```

### 3. Inline lambda (one-off, maximum density)

```csharp
Result r = RuleEngine.Evaluate(
    (UserContext ctx) => ctx.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be 18+."),
    userCtx);
```

---

## Evaluating a Single Rule

```csharp
// Any style passes directly — no .ToRule() needed
Result r1 = RuleEngine.Evaluate(new AgeRule(), ctx);
Result r2 = RuleEngine.Evaluate(regionRule, ctx);   // Func variable
Result r3 = RuleEngine.Evaluate(CheckEmail, ctx);   // method group
Result r4 = RuleEngine.Evaluate(
    (UserContext c) => c.HasLicense ? Result.Success() : Error.Validation("License.Missing", "Required."),
    ctx);
```

---

## Combining Rules

Compose first using extension methods on arrays, then evaluate with `RuleEngine.Evaluate`.

### And: all must pass

And: all must pass. `Result` rules stop at the first failure and return its errors; `Result<T>` rules evaluate every child and aggregate all errors.

```csharp
// Class instances
Result andResult = RuleEngine.Evaluate(
    new IRuleBase<UserContext>[] { new AgeRule(), new LicenseRule(repo) }.And(),
    ctx);

// Func array — no .ToRule() needed
Result andResult2 = RuleEngine.Evaluate(
    new Func<UserContext, Result>[] { regionRule, CheckEmail }.And(),
    ctx);

// Mixed: class + lambda
Result andResult3 = RuleEngine.Evaluate(
    new IRuleBase<UserContext>[]
    {
        new AgeRule(),
        regionRule.ToRule(),
        ((Func<UserContext, Result>)(c => c.HasLicense ? Result.Success() : Error.Validation("License.Missing", "Required."))).ToRule()
    }.And(),
    ctx);
```

### Or: at least one must pass

```csharp
Result orResult = RuleEngine.Evaluate(
    new Func<UserContext, Result>[] { regionRule, CheckEmail }.Or(),
    ctx);
```

`Or` succeeds as soon as one rule passes. When every rule fails, the result carries the errors of all of them.

### Linear: stop on first failure

```csharp
// Class instances
Result linear = RuleEngine.Evaluate(
    new IRule<UserContext>[] { new AgeRule(), new LicenseRule(repo) }.Linear(),
    ctx);

// .Next() runs the second rule only when the first one passes
Result pipeline = RuleEngine.Evaluate(
    ((Func<UserContext, Result>)CheckEmail).Next(regionRule),
    ctx);

// Longer sequences: keep chaining Next, or use Linear()
Result longer = RuleEngine.Evaluate(
    ((Func<UserContext, Result>)CheckEmail).Next(regionRule).Next(c => c.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be 18+")),
    ctx);
```

`Next` links one rule to the rule after it. `a.Next(b).Next(c)` appends `c` to the end of the chain, same as `a.Next(b.Next(c))` or an array with `.Linear()`: a failing `b` stops the chain.

### Static shortcuts

`RuleEngine.And`, `RuleEngine.Or` and `RuleEngine.Linear` take the array and the context directly:

```csharp
Result all = RuleEngine.And(new Func<UserContext, Result>[] { regionRule, CheckEmail }, ctx);
Result seq = RuleEngine.Linear(new IRule<UserContext>[] { new AgeRule(), new LicenseRule(repo) }, ctx);
```

Every combinator also exists for rules with values (`IRule<TContext, TResult>`) and for async rules.

### Conditional: if/then/else branching

```csharp
// Rule as condition
Result conditional = RuleEngine.If(
    new AgeRule(),
    success: new GrantAccessRule(),
    failure: new DenyAccessRule(),
    ctx);

// Lambda branches
Result conditional2 = RuleEngine.If(
    (UserContext c) => c.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be 18+"),
    success: c => Result.Success(),
    failure: c => Error.Forbidden("Access.Denied", "Access denied."),
    ctx);

// Bool shorthand
Result conditional3 = RuleEngine.If(
    condition: ctx.IsAllowedRegion,
    success: new GrantAccessRule(),
    failure: new DenyAccessRule(),
    ctx);
```

---

## Rules with Values (Result<T>)

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

// Inline form — no class needed
Result<string> grade = RuleEngine.Evaluate(
    (int score) => score >= 90 ? Result<string>.Success("A") : Error.Validation("Grade.Failed", "Score too low."),
    85);
```

---

## Domain Error Hierarchies

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

// Func rules referencing domain error catalogue
Func<ApplicantContext, Result> regionRule =
    ctx => ctx.IsAllowedRegion ? Result.Success() : RegistrationErrors.RegionBlocked;

// Compose with And: Result rules stop at the first failure and return its errors
Result result = RuleEngine.Evaluate(
    new Func<ApplicantContext, Result>[]
    {
        c => c.Age >= 18   ? Result.Success() : RegistrationErrors.Underage,
        c => c.HasLicense  ? Result.Success() : RegistrationErrors.NoLicense,
        regionRule
    }.And(),
    applicant);

result.Switch(
    onSuccess: () => Console.WriteLine("Approved"),
    onFailure: errors =>
    {
        foreach (Error e in errors)
            Console.WriteLine($"[{e.Type}] {e.Code}: {e.Description}");
    });
```

---

## Predicate Factories

Create `IRule<T>` or `IAsyncRule<T>` directly from a predicate; no class needed.

```csharp
// Static error
IRule<int> positive = RuleEngine.FromPredicate<int>(
    x => x > 0,
    Error.Validation("Value.Negative", "Must be positive"));

// Error factory — error message references the failing context
IRule<string> minLength = RuleEngine.FromPredicate<string>(
    s => s.Length >= 3,
    s => Error.Validation("String.TooShort", $"'{s}' must be at least 3 characters"));

// Async predicate (e.g., DB uniqueness check)
IAsyncRule<string> unique = RuleEngine.FromPredicateAsync<string>(
    async s => await _db.IsUniqueAsync(s),
    s => Error.Conflict("Name.Taken", $"'{s}' is already taken"));

// Sync-only tree
Result r = RuleEngine.Evaluate(positive, 42);

// Tree with an async rule: await EvaluateAsync
Result composed = await RuleEngine.EvaluateAsync(
    new IRuleBase<string>[] { minLength, unique }.And(), "alice", ct);
```

---

## Async Evaluation

Async rules implement `IAsyncRule<TContext>` (`ValueTask<Result> EvaluateAsync(context, ct)`), come from `FromPredicateAsync`, or are a `Func<TContext, CancellationToken, ValueTask<Result>>` converted with `.ToRule()`.

`RuleEngine.EvaluateAsync(rule, context, ct)` returns `ValueTask<Result>` / `ValueTask<Result<T>>` and awaits every `IAsyncRule` in the tree.

- Same results, short-circuiting and error order as `Evaluate`; composite children run sequentially.
- Sync-only trees complete synchronously, with no task allocation.
- Cancelling `ct` throws `OperationCanceledException` instead of returning an error (`Evaluate` throws too unless an enclosing sync rule returns it as an error `Result`; it stops waiting for a pending async rule).
- Rules must observe `ct`: the token is checked before each rule, but an in-flight rule is awaited, never abandoned.
- `Evaluate` blocks on async rules (deadlock risk under a `SynchronizationContext`); use `EvaluateAsync` for any tree with async rules.

---

## Best Practices

- And: all must pass. `Result` rules stop at the first failure and return its errors; `Result<T>` rules evaluate every child and aggregate all errors. `array.Linear()` stops at the **first** failure
- Use `.Linear()` on an array for sequences of three or more rules; `.Next()` links one rule to the next and can be chained as `a.Next(b).Next(c)`
- No explicit `.ToRule()` needed when passing `Func<>` to `RuleEngine.Evaluate` or to `.And()/.Or()` on `Func[]`
- Group domain errors in static classes so rules read like domain language
- Test each `IRule<T>` in isolation: `Evaluate(context)` → assert Result. No mocking needed
- Use class rules when the rule needs DI; use `Func` fields when it doesn't
- Await `RuleEngine.EvaluateAsync` whenever the tree contains an `IAsyncRule`
