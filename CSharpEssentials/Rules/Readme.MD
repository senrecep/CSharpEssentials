# Rule Engine

A powerful and flexible rule engine implementation that supports both object-oriented and functional programming patterns in C#. This engine allows you to create and compose complex business rules while maintaining clean and maintainable code.

## Features

- **Multiple Rule Types**:

  - Simple Rules (`IRule<TContext>`, `IAsyncRule<TContext>`)
  - Linear Rules (`ILinearRule<TContext>`, `ILinearAsyncRule<TContext>`)
  - Conditional Rules (`IConditionalRule<TContext>`, `IConditionalAsyncRule<TContext>`)
  - Composite Rules:
    - AND Rules (`IAndRule<TContext>`, `IAndAsyncRule<TContext>`)
    - OR Rules (`IOrRule<TContext>`, `IOrAsyncRule<TContext>`)

- **Multiple Programming Paradigms**:
  - Object-Oriented Approach with class-based rules
  - Functional Programming with lambda expressions
  - Extension methods for converting functions to rules
  - Fluent API for rule composition

- **Type Safety**: Generic context and result type support for type-safe rule evaluation
- **Async Support**: Full async/await support for all rule types
- **Composability**: Rules can be composed and chained together
- **Error Handling**: Built-in error handling and propagation using Result type
- **Performance**: Aggressive inlining for extension methods

## Comparing OOP and Functional Approaches

The engine supports both object-oriented and functional programming styles, allowing you to choose the most appropriate approach for your use case. Here's a real-world example showing the same validation implemented in both styles:

```csharp
// The domain model
internal sealed record Person(string Name, int Age);

// Object-Oriented Approach
internal readonly record struct PersonRule : IAndRule<Person>
{
    public IRuleBase<Person>[] Rules => [new NameRule(3, 50), new AgeRule(0, 120)];

    public Result Evaluate(Person context, CancellationToken cancellationToken = default) =>
        context is not null;
}

internal readonly record struct NameRule(int Min, int Max) : IRule<Person>
{
    public Result Evaluate(Person context, CancellationToken cancellationToken = default) =>
        !string.IsNullOrEmpty(context.Name) && context.Name.Length < Max && context.Name.Length > Min;
}

internal readonly record struct AgeRule(int Min, int Max) : IRule<Person>
{
    public Result Evaluate(Person context, CancellationToken cancellationToken = default) =>
        context.Age >= Min && context.Age < Max;
}

// Using OOP approach
var person = new Person("John", 30);
var rule = new PersonRule();
Result result = RuleEngine.Evaluate(rule, person);

// Functional Approach - Same validation using lambda expressions
Result result2 = RuleEngine.And(
    rules: [
        input => input is not null,
        input => input.Age > 0 && input.Age < 120,
        input => !string.IsNullOrEmpty(input.Name) && input.Name.Length > 3 && input.Name.Length < 50
    ],
    context: person
);
```

### When to Use Each Approach

#### Object-Oriented Approach Benefits
- Better for complex business rules that require state or configuration
- Provides better encapsulation and reusability
- Makes rules more maintainable when logic is complex
- Allows for dependency injection and testability
- Ideal for rules that are used across multiple parts of the application
- Better for rules that require documentation and versioning

#### Functional Approach Benefits
- More concise for simple validations
- Reduces boilerplate code
- Easier to write inline
- Great for one-off validations
- More flexible for quick changes
- Better for rules that are specific to one use case
- Easier to compose and modify on the fly

### Choosing the Right Approach

1. **Use Object-Oriented Approach When**:
   - Rules are complex and require multiple steps
   - Rules need to be reused across the application
   - Rules require configuration or dependencies
   - Rules need to maintain state
   - Rules require extensive unit testing
   - Rules are part of your domain model

2. **Use Functional Approach When**:
   - Rules are simple and straightforward
   - Rules are specific to one use case
   - Rules don't require state or configuration
   - Rules are unlikely to change or need maintenance
   - Quick prototyping is needed
   - Rules are composed at runtime

3. **Mix Both Approaches When**:
   - Some rules are complex while others are simple
   - You need to combine existing class-based rules with quick validations
   - Different team members have different preferences
   - Different parts of the application have different requirements

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

// Functional implementation
var ageValidationRule = (Person person) => person.Age >= 18
    ? Result.Success()
    : Result.Failure(Error.Validation(
        code: "Person.InvalidAge",
        description: "Person must be at least 18 years old"));

// Using the functional rule
var result = RuleEngine.Evaluate(ageValidationRule, person);
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

// Functional implementation using Linear composition
var result = RuleEngine.Linear(
    rules: [
        input => input > 0,
        input => input < 100,
        input => input % 5 == 0,
        input => input % 7 == 0
    ],
    context: 35);
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

// Functional implementation using If composition
var result = RuleEngine.If(
    rule: input => input > 10,
    success: Rule1,
    failure: Rule2,
    context: 6);

static Result Rule1(int input)
{
    Console.WriteLine($"Rule1: {input}");
    return Result.Success();
}

static Result Rule2(int input)
{
    Console.WriteLine($"Rule2: {input}");
    return Result.Success();
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

// Functional implementation using And composition
var result = RuleEngine.And(
    rules: [
        input => input > 0,
        input => input < 20,
        input => input % 5 == 0,
        input => input % 7 == 0
    ],
    context: 35);
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

// Functional implementation using Or composition
var result = RuleEngine.Or(
    rules: [
        input => input > 100,
        input => input < 0
    ],
    context: 50);
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

### Mixing OOP and Functional Approaches

The engine allows you to mix and match OOP and functional approaches freely. Here are some examples:

#### Using Functions in Class-Based Rules

```csharp
// Using functions in Next property of LinearRule
public readonly record struct ValidateEmailRule : ILinearRule<UserRegistration>
{
    private static readonly Func<UserRegistration, Result> _nextRule = ur => ur.Email.Length > 5 && ur.Email.Contains('@');

    public IRuleBase<UserRegistration> Next => _nextRule.ToRule();

    public Result Evaluate(UserRegistration context, CancellationToken cancellationToken = default) =>
        !string.IsNullOrEmpty(context.Email);
}

// Using functions in Rules property of AndRule
internal readonly record struct UserValidationRule : IAndRule<User>
{
    private static readonly Func<User, Result> _nameRuleFunc = user => user.Name.Length > 3;
    private static readonly Func<User, Result> _emailRuleFunc = user => user.Email.Length > 5 && user.Email.Contains('@');

    private static readonly IRuleBase<User> _nameRule = _nameRuleFunc.ToRule();
    private static readonly IRuleBase<User> _emailRule = _emailRuleFunc.ToRule();

    public IRuleBase<User>[] Rules =>
    [
        _nameRule,
        _emailRule,
    ];

    public Result Evaluate(User context, CancellationToken cancellationToken = default) =>
        context is not null;
}
```

#### Using Class-Based Rules in Functional Composition

```csharp
// Mixing class-based and functional rules in And composition
var result = RuleEngine.And(
    rules: [
        new AgeValidationRule(),           // Class-based rule
        new EmailFormatRule(MinLength: 5),  // Class-based rule with parameters
        user => user.Name.Length > 3,      // Function-based rule
        async (user, token) => await ValidateAddressAsync(user, token)  // Async function
    ],
    context: user
);

// Using class-based rules in Linear composition
var result2 = RuleEngine.Linear(
    rules: [
        new UserExistsRule(),              // Class-based rule
        user => user.IsActive,             // Function-based rule
        new PermissionCheckRule("admin"),   // Class-based rule with parameters
        async (user, token) => await CheckQuotaAsync(user, token)  // Async function
    ],
    context: user
);

// Mixing in Conditional rules
var result3 = RuleEngine.If(
    rule: new PremiumUserRule(),           // Class-based condition
    success: user => ApplyDiscount(user),  // Function-based success handler
    failure: new StandardPricingRule(),    // Class-based failure handler
    context: user
);
```

#### Converting Between Approaches

Any function can be converted to a rule type and vice versa:

```csharp
// Converting functions to rule types
Func<User, Result> userValidation = user => user.Age >= 18;
IRule<User> rule1 = userValidation.ToRule();

// Using converted rules in class-based rules
public readonly record struct ComplexValidation : IAndRule<User>
{
    public IEnumerable<IRuleBase<User>> Rules => 
    [
        userValidation.ToRule(),           // Converted function
        new AgeValidationRule(),           // Class-based rule
        user => user.Name.Length > 3       // Inline function
    ];
}

// Using class-based rules in functional composition
var ageRule = new AgeValidationRule();
var emailRule = new EmailFormatRule(5);

var result = RuleEngine.And(
    rules: [
        ageRule,                           // Class-based rule
        user => user.IsActive,             // Function-based rule
        emailRule,                         // Class-based rule
        async (user, token) => await ValidateAsync(user, token)  // Async function
    ],
    context: user
);
```

This flexibility allows you to:
- Use the most appropriate approach for each rule
- Gradually migrate between approaches
- Mix synchronous and asynchronous rules
- Combine simple and complex validations
- Reuse existing rules in both paradigms
- Create hybrid validation strategies

The engine treats all rules equally regardless of their implementation approach, making it seamless to combine different styles based on your needs.

## 🚀 Performance Optimizations

1. **Record Struct Usage**
```csharp
// ❌ Bad: Using class
public class UserRule : IRule<User> { }

// ✅ Good: Using record struct
public readonly record struct UserRule : IRule<User> { }
```

2. **Static Error Definitions**
```csharp
// ❌ Bad: Creating new error each time
return Error.Validation("USER.NOT_ADULT", "User is not adult");

// ✅ Good: Static error definitions
public static class UserErrors
{
    public static readonly Error NotAdult = Error.Validation(
        "USER.NOT_ADULT",
        "User is not adult"
    );
}
```

3. **Lazy Evaluation**
```csharp
// Lazy evaluation of rules
public IRuleBase<Order>[] Rules => 
    _rules ??= [
        new OrderAmountRule(),
        new StockAvailabilityRule()
    ];
```

4. **Immutable Design**
```csharp
// ❌ Bad: Mutable state
public class UserRule : IRule<User> 
{
    private int _minimumAge;
    public void SetMinimumAge(int age) => _minimumAge = age;
}

// ✅ Good: Immutable design
public readonly record struct UserRule : IRule<User> 
{
    private readonly int _minimumAge;
    public UserRule(int minimumAge) => _minimumAge = minimumAge;
}
```

## 🛠 Error Handling Best Practices

1. **Centralized Error Management**
```csharp
public static class OrderErrors
{
    public static Error InsufficientStock(int required, int available) => Error.Validation(
        code: "ORDER.INSUFFICIENT_STOCK",
        description: "Not enough items in stock",
        metadata: new ErrorMetadata(
            ("RequiredQuantity", required),
            ("AvailableQuantity", available)
        )
    );

    public static Error InvalidAmount(decimal minimum, decimal actual) => Error.Validation(
        code: "ORDER.INVALID_AMOUNT",
        description: "Order amount is below minimum",
        metadata: new ErrorMetadata(
            ("MinimumAmount", minimum),
            ("ActualAmount", actual)
        )
    );
}
```

2. **Rich Error Details**
```csharp
public Result Evaluate(Order context)
{
    if (context.Amount < MinimumAmount)
        return OrderErrors.InvalidAmount(MinimumAmount, context.Amount);

    if (context.Quantity > AvailableStock)
        return OrderErrors.InsufficientStock(context.Quantity, AvailableStock);

    return Result.Success();
}
```

## 🎯 Real World Examples

### E-Commerce Validation
```csharp
public readonly record struct OrderValidationRule : IAndRule<Order>
{
    private readonly IStockService _stockService;
    private readonly IPaymentService _paymentService;

    public OrderValidationRule(IStockService stockService, IPaymentService paymentService)
    {
        _stockService = stockService;
        _paymentService = paymentService;
    }

    public IRuleBase<Order>[] Rules =>
    [
        new OrderAmountRule(minimumAmount: 50),           // Minimum order amount
        new StockAvailabilityRule(_stockService),         // Stock check
        new PaymentMethodValidationRule(_paymentService), // Payment method
        new ShippingAddressRule(),                        // Shipping address
        new UserValidationRule()                          // User status
    ];
}
```

### Payment Processing
```csharp
public readonly record struct PaymentMethodRule : IOrRule<Payment>
{
    public IRuleBase<Payment>[] Rules =>
    [
        new CreditCardRule(),     // Credit card validation
        new BankTransferRule(),   // Bank transfer validation
        new CryptoRule()          // Cryptocurrency validation
    ];
}
```

## 📚 Advanced Usage

### Rule Decorators
```csharp
public record struct LoggingRuleDecorator<T> : IRule<T>
{
    private readonly IRule<T> _rule;
    private readonly ILogger _logger;

    public LoggingRuleDecorator(IRule<T> rule, ILogger logger)
    {
        _rule = rule;
        _logger = logger;
    }

    public Result Evaluate(T context)
    {
        _logger.LogInformation("Starting rule evaluation: {RuleType}", _rule.GetType().Name);
        var result = _rule.Evaluate(context);
        _logger.LogInformation("Rule evaluation completed: {Success}", result.IsSuccess);
        return result;
    }
}
```
