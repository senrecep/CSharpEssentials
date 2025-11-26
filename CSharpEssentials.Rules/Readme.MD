# CSharpEssentials.Rules

`CSharpEssentials.Rules` is a composable, functional rule engine for .NET. It allows you to define business logic as small, reusable rules and combine them into complex workflows using a fluent API.

## 🚀 Features

- **Composable Logic**: Build complex logic from simple, single-responsibility rules.
- **Flow Control**: Support for **Linear** (sequences), **Conditional** (If/Else), **And**, and **Or** rule combinations.
- **Async Support**: First-class support for asynchronous rules.
- **Result Pattern**: Built on top of `CSharpEssentials.Results` to return success/failure status.
- **Context-Driven**: Rules operate on a shared `TContext` state.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Rules
```

## 🛠 Usage

### 1. Defining Rules

You can implement the `IRule<TContext>` interface or use functional adapters.

**Using Interface:**

```csharp
using CSharpEssentials.Rules;
using CSharpEssentials.ResultPattern;

public class UserContext 
{ 
    public int Age { get; set; }
    public bool HasLicense { get; set; }
}

public class AgeRule : IRule<UserContext>
{
    public Result Evaluate(UserContext context, CancellationToken ct = default)
    {
        if (context.Age < 18)
            return Result.Failure("User.Underage", "User must be at least 18.");
        return Result.Success();
    }
}
```

**Using Func:**

```csharp
Func<UserContext, Result> licenseCheck = ctx => 
    ctx.HasLicense 
        ? Result.Success() 
        : Result.Failure("User.NoLicense", "User must have a license.");

var rule = licenseCheck.ToRule();
```

### 2. Executing Rules

Use the `RuleEngine` to evaluate a rule against a context.

```csharp
var context = new UserContext { Age = 20, HasLicense = true };
var rule = new AgeRule();

Result result = RuleEngine.Evaluate(rule, context);

if (result.IsSuccess)
{
    Console.WriteLine("Rule passed!");
}
```

### 3. Composing Rules (Linear)

Run rules in a sequence. If one fails, execution stops.

```csharp
var complexRule = Rule.Linear(new AgeRule())
                      .Next(licenseCheck.ToRule());

Result result = RuleEngine.Evaluate(complexRule, context);
```

### 4. Conditional Rules

Execute different rules based on the result of a condition.

```csharp
var workflow = Rule.If(new AgeRule(),
    then: new GrantAccessRule(),
    @else: new DenyAccessRule());
```

### 5. And / Or Logic

Combine multiple rules.

- **And**: All rules must pass.
- **Or**: At least one rule must pass.

```csharp
var combined = Rule.And(
    new AgeRule(),
    new CitizenshipRule()
);

var alternative = Rule.Or(
    new PassportRule(),
    new DriverLicenseRule()
);
```
