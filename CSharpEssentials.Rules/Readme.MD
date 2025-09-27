# Rule Engine

A powerful and flexible rule engine implementation that supports functional programming patterns in C#. This engine allows you to create and compose complex business rules while maintaining clean and maintainable code.

## Features

- **Multiple Rule Types**:

  - Simple Rules (`IRule<TContext>`, `IAsyncRule<TContext>`)
  - Linear Rules (`ILinearRule<TContext>`, `ILinearAsyncRule<TContext>`)
  - Conditional Rules (`IConditionalRule<TContext>`, `IConditionalAsyncRule<TContext>`)
  - Composite Rules:
    - AND Rules (`IAndRule<TContext>`, `IAndAsyncRule<TContext>`)
    - OR Rules (`IOrRule<TContext>`, `IOrAsyncRule<TContext>`)

- **Functional Approach**: Uses Result type for error handling and composition
- **Async Support**: Full async/await support for all rule types
- **Type Safety**: Generic context support for type-safe rule evaluation
- **Composability**: Rules can be composed and chained together
- **Error Handling**: Built-in error handling and propagation

## Rule Types Explained

### Simple Rules

Basic rules that evaluate a single condition and return a Result. Here are different ways to implement them:

```csharp
// Class implementation
public class AgeValidationRule : IRule<Person>
{
    public Result Evaluate(Person context, CancellationToken cancellationToken = default)
    {
        return context.Age >= 18
            ? Result.Success()
            : Result.Failure(Error.Validation(
                code: "Person.InvalidAge",
                description: "Person must be at least 18 years old"));
    }
}

// Record implementation
public record EmailFormatRule(int MinLength = 5) : IRule<string>
{
    public Result Evaluate(string context, CancellationToken cancellationToken = default)
    {
        var metadata = new ErrorMetadata(new[]
        {
            new KeyValuePair<string, object?>("MinLength", MinLength),
            new KeyValuePair<string, object?>("ActualLength", context.Length)
        });

        return context.Length >= MinLength && context.Contains("@")
            ? Result.Success()
            : Result.Failure(Error.Validation(
                code: "Email.InvalidFormat",
                description: "Invalid email format",
                metadata: metadata));
    }
}

// Readonly struct implementation
public readonly struct PasswordStrengthRule : IRule<string>
{
    public Result Evaluate(string context, CancellationToken cancellationToken = default)
    {
        var validations = new List<(bool isValid, string requirement)>
        {
            (context.Length >= 8, "minimum length of 8"),
            (context.Any(char.IsUpper), "uppercase letter"),
            (context.Any(char.IsLower), "lowercase letter"),
            (context.Any(char.IsDigit), "digit")
        };

        var failedRequirements = validations
            .Where(v => !v.isValid)
            .Select(v => v.requirement)
            .ToList();

        var metadata = new ErrorMetadata("FailedRequirements", failedRequirements);

        return !failedRequirements.Any()
            ? Result.Success()
            : Result.Failure(Error.Validation(
                code: "Password.WeakPassword",
                description: "Password does not meet strength requirements",
                metadata: metadata));
    }
}
```

### Linear Rules

Rules that can be chained in a sequence. Each rule is evaluated only if the previous rule succeeds.

```csharp
// Record struct implementation
public readonly record struct ValidateEmailRule : ILinearRule<UserRegistration>
{
    public IRuleBase<UserRegistration>? Next { get; init; }

    public Result Evaluate(UserRegistration context, CancellationToken cancellationToken = default)
    {
        return context.Email.Contains("@")
            ? Result.Success()
            : Result.Failure(Error.Validation(
                code: "UserRegistration.InvalidEmail",
                description: "Email must contain @ symbol"));
    }
}

// Readonly record implementation
public sealed record ValidatePhoneRule : ILinearRule<UserRegistration>
{
    public IRuleBase<UserRegistration>? Next { get; init; }
    private readonly string _countryCode;

    public ValidatePhoneRule(string countryCode) => _countryCode = countryCode;

    public Result Evaluate(UserRegistration context, CancellationToken cancellationToken = default)
    {
        var metadata = new ErrorMetadata(new[]
        {
            new KeyValuePair<string, object?>("RequiredCountryCode", _countryCode),
            new KeyValuePair<string, object?>("ProvidedPhone", context.Phone)
        });

        return context.Phone.StartsWith(_countryCode)
            ? Result.Success()
            : Result.Failure(Error.Validation(
                code: "UserRegistration.InvalidPhoneNumber",
                description: $"Phone number must start with {_countryCode}",
                metadata: metadata));
    }
}
```

### Conditional Rules

Rules that can branch to different paths based on the evaluation result.

```csharp
// Record implementation with init-only properties
public record UserTypeRule : IConditionalRule<User>
{
    public IRuleBase<User>? Success { get; init; }
    public IRuleBase<User>? Failure { get; init; }
    public decimal PremiumThreshold { get; init; } = 100.0m;

    public Result Evaluate(User context, CancellationToken cancellationToken = default)
    {
        var metadata = new ErrorMetadata(new[]
        {
            new KeyValuePair<string, object?>("RequiredBalance", PremiumThreshold),
            new KeyValuePair<string, object?>("CurrentBalance", context.AccountBalance)
        });

        return context.AccountBalance >= PremiumThreshold
            ? Result.Success()
            : Result.Failure(Error.Validation(
                code: "User.InsufficientBalance",
                description: "Insufficient balance for premium status",
                metadata: metadata));
    }
}

// Readonly struct implementation
public readonly struct AgeGroupRule : IConditionalRule<Person>
{
    public IRuleBase<Person>? Success { get; init; }
    public IRuleBase<Person>? Failure { get; init; }
    private readonly int _adultAge;

    public AgeGroupRule(int adultAge, IRuleBase<Person>? success = null, IRuleBase<Person>? failure = null)
    {
        _adultAge = adultAge;
        Success = success;
        Failure = failure;
    }

    public Result Evaluate(Person context, CancellationToken cancellationToken = default)
    {
        var metadata = new ErrorMetadata(new[]
        {
            new KeyValuePair<string, object?>("RequiredAge", _adultAge),
            new KeyValuePair<string, object?>("ActualAge", context.Age)
        });

        return context.Age >= _adultAge
            ? Result.Success()
            : Result.Failure(Error.Validation(
                code: "Person.Underage",
                description: $"Person must be at least {_adultAge} years old",
                metadata: metadata));
    }
}
```

### Composite Rules

#### AND Rules

All rules must succeed for the composite to succeed.

```csharp
// Record struct implementation
public readonly record struct CompleteProfileValidation : IAndRule<Profile>
{
    public IEnumerable<IRuleBase<Profile>> Rules { get; }

    public CompleteProfileValidation(IEnumerable<IRuleBase<Profile>> rules)
    {
        Rules = rules.ToArray();
    }

    public Result Evaluate(Profile context, CancellationToken cancellationToken = default)
    {
        return Result.Success();
    }
}

// Readonly record implementation
public sealed record UserValidationRules : IAndRule<User>
{
    public IEnumerable<IRuleBase<User>> Rules { get; }

    private UserValidationRules(IEnumerable<IRuleBase<User>> rules)
    {
        Rules = rules.ToArray();
    }

    public static UserValidationRules Create(params IRuleBase<User>[] rules) =>
        new(rules);

    public Result Evaluate(User context, CancellationToken cancellationToken = default)
    {
        return Result.Success();
    }
}
```

#### OR Rules

At least one rule must succeed for the composite to succeed.

```csharp
// Record implementation
public record AuthenticationRule : IOrRule<LoginAttempt>
{
    public IEnumerable<IRuleBase<LoginAttempt>> Rules { get; }

    private AuthenticationRule(IEnumerable<IRuleBase<LoginAttempt>> rules)
    {
        Rules = rules.ToArray();
    }

    public static AuthenticationRule Create(params IRuleBase<LoginAttempt>[] rules) =>
        new(rules);

    public Result Evaluate(LoginAttempt context, CancellationToken cancellationToken = default)
    {
        return Result.Success();
    }
}

// Readonly struct implementation
public readonly struct PaymentValidationRule : IOrRule<Payment>
{
    public IEnumerable<IRuleBase<Payment>> Rules { get; }

    public PaymentValidationRule(params IRuleBase<Payment>[] rules)
    {
        Rules = rules;
    }

    public Result Evaluate(Payment context, CancellationToken cancellationToken = default)
    {
        return Result.Success();
    }
}
```

## Usage

```csharp
// Create rules using different type declarations
var ageRule = new AgeValidationRule();
var emailRule = new EmailFormatRule(MinLength: 8);
var passwordRule = new PasswordStrengthRule();

// Create composite rules
var profileValidation = new CompleteProfileValidation(new[]
{
    new ValidateEmailRule(),
    new ValidatePhoneRule("+90"),
    new PasswordStrengthRule()
});

var authRule = AuthenticationRule.Create(
    new PasswordAuthRule(),
    new TokenAuthRule(),
    new BiometricAuthRule()
);

// Evaluate rules
var result = RuleEngine.Evaluate(ageRule, person);

// Handle the result
if (result.IsSuccess)
{
    // Process success
}
else
{
    // Handle failure
    var error = result.Error;
}
```

## Best Practices

1. Keep rules small and focused on a single responsibility
2. Use async rules when dealing with I/O operations
3. Compose complex rules from simpler ones using AND/OR rules
4. Use meaningful names for your rules that describe their purpose
5. Handle exceptions within rules appropriately
6. Use the Result type consistently for error handling
7. Consider rule caching for expensive operations
8. Prefer immutable implementations using records and readonly structs
9. Use init-only properties where possible to enforce immutability
10. Consider using static factory methods for complex rule creation

## Error Handling

The engine provides built-in error handling through the Result type and includes specific error types for different failure scenarios. All exceptions are caught and converted to failure results with appropriate error messages.
