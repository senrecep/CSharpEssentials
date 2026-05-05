# CSharpEssentials.Rules

Composable, functional rule engine for .NET. Define business logic as small, reusable rules and combine them with a fluent API.

## Features

- **Simple Rules** — Implement `IRule<TContext>` or use `Func<TContext, Result>.ToRule()`.
- **Linear Sequences** — Run rules in order, stop on first failure.
- **Conditional Rules** — If/Then/Else branching.
- **And / Or Logic** — All must pass, or at least one must pass.
- **Async Support** — First-class async rules.
- **Result Integration** — Built on `CSharpEssentials.Results`.

## Installation

```bash
dotnet add package CSharpEssentials.Rules
```

## Usage

### Defining Rules

```csharp
using CSharpEssentials.Rules;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;

public class AgeRule : IRule<UserContext>
{
    public Result Evaluate(UserContext ctx, CancellationToken ct = default)
    {
        if (ctx.Age < 18)
            return Error.Validation("Age.Underage", "Must be at least 18.");
        return Result.Success();
    }
}

// Or with a Func
IRule<UserContext> licenseRule = (Func<UserContext, Result>)(ctx =>
    ctx.HasLicense ? Result.Success() : Error.Validation("License.Missing", "License required.")).ToRule();
```

### Evaluating Rules

```csharp
var ctx = new UserContext { Age = 20, HasLicense = true };
Result result = RuleEngine.Evaluate(new AgeRule(), ctx);
```

### Combining Rules

```csharp
// All must pass
Result andResult = RuleEngine.And(new[] { new AgeRule(), licenseRule }, ctx);

// At least one must pass
Result orResult = RuleEngine.Or(new[] { passportRule, licenseRule }, ctx);

// Sequence (stop on first failure)
Result linear = RuleEngine.Linear(new[] { step1, step2, step3 }, ctx);

// Conditional
Result conditional = RuleEngine.If(new AgeRule(), new GrantAccessRule(), new DenyAccessRule(), ctx);
```

### Rules with Values

```csharp
IRule<int, string> gradeRule = ((Func<int, Result<string>>)(score =>
{
    if (score >= 90) return "A";
    if (score >= 80) return "B";
    return Error.Validation("Grade", "Failed.");
})).ToRule();

Result<string> grade = gradeRule.Evaluate(85);
```
