# CSharpEssentials.Validation

High-performance, model-first validation library for C#. Zero-reflection, eager evaluation — no expression trees, no deferred builds. Returns `Result<T>` natively.

## Installation

```bash
dotnet add package CSharpEssentials.Validation
```

## Quick Start

Define a validator by extending `Validator<T>`:

```csharp
using CSharpEssentials.Validation;

public class CreateUserCommandValidator : Validator<CreateUserCommand>
{
    protected override ValueTask Configure(CreateUserCommand model, RuleContext<CreateUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        rules.For(() => model.Name).NotEmpty().MaxLength(100);
        rules.For(() => model.Age).GreaterThan(0);
        return ValueTask.CompletedTask;
    }
}
```

Invoke directly or inject via DI:

```csharp
var validator = new CreateUserCommandValidator();
Result<CreateUserCommand> result = await validator.ValidateAsync(command);
// Sync call site: validator.ValidateAsync(command).GetAwaiter().GetResult()

if (result.IsFailure)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.Code}: {error.Description}");
        // e.g. "Email.NotEmpty: 'Email' must not be empty."
}
```

> **Sync call site:** `validator.ValidateAsync(command).GetAwaiter().GetResult()` — safe when the validator has no actual async operations (returns a pre-completed `ValueTask`).

### Inline (Static) Usage

For one-off validations without a dedicated class, use the static `Validator.ValidateAsync`:

```csharp
// Sync delegate — zero heap allocation
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, (m, rules) =>
{
    rules.For(() => m.Email).NotEmpty().EmailAddress();
    rules.For(() => m.Name).NotEmpty().MaxLength(100);
});

// Async delegate — for MustAsync or SetValidatorAsync inside
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, async (m, rules, ct) =>
{
    rules.For(() => m.Name).NotEmpty();
    await rules.For(() => m.Email)
               .MustAsync(async (email, c) => await _db.IsUniqueAsync(email, c),
                          "Email.NotUnique", "Email is already taken.", c);
}, cancellationToken);
```

`Validator.ValidateAsync` and `Validator<T>` live in the same file (`Validator.cs`) as separate types — one static utility class, one abstract base class. They are independent; the static form does not use `Validator<T>` internally.

## Built-in Validators

### String

| Validator | Null | Description |
|-----------|------|-------------|
| `NotEmpty()` | FAIL | Value must not be null, empty, or whitespace |
| `NotNull()` | FAIL | Value must not be null |
| `MaxLength(n)` | skip | Length ≤ n |
| `MinLength(n)` | skip | Length ≥ n |
| `Length(min, max)` | skip | Length in [min, max] |
| `EmailAddress()` | skip | Valid e-mail format |
| `Matches(pattern)` | skip | Matches regex pattern |
| `Contains(sub)` | skip | Contains substring |
| `StartsWith(prefix)` | skip | Starts with prefix |
| `EndsWith(suffix)` | skip | Ends with suffix |

### Comparable (`int`, `DateTime`, `decimal`, …)

| Validator | Null | Description |
|-----------|------|-------------|
| `GreaterThan(n)` | skip | Value > n |
| `GreaterThanOrEqualTo(n)` | skip | Value ≥ n |
| `LessThan(n)` | skip | Value < n |
| `LessThanOrEqualTo(n)` | skip | Value ≤ n |
| `InclusiveBetween(min, max)` | skip | min ≤ value ≤ max |
| `ExclusiveBetween(min, max)` | skip | min < value < max |
| `Equal(expected)` | FAIL | Equals expected |
| `NotEqual(forbidden)` | skip | Not equal to forbidden |

### Collections (`IEnumerable<T>`)

| Validator | Null | Description |
|-----------|------|-------------|
| `NotEmpty()` | FAIL | Collection must not be null or empty |
| `NotNull()` | FAIL | Collection must not be null |
| `MinCount(n)` | skip | At least n elements |
| `MaxCount(n)` | skip | At most n elements |
| `CountBetween(min, max)` | skip | Element count in [min, max] |

### Nullable Structs (`int?`, `DateTime?`, …)

All comparable validators are available for nullable value types — `null` is silently skipped.

## CascadeMode

By default (`Stop`), the first failure on a chain stops subsequent rules. Use `Continue` to accumulate all errors for a property:

```csharp
rules.For(() => model.Password)
     .Cascade(CascadeMode.Continue)
     .MinLength(8)
     .Matches(@"[A-Z]", message: "Must contain an uppercase letter.");
```

## Custom Predicates

```csharp
// Sync
rules.For(() => model.Name)
     .Must(name => name != "admin", "Name.Reserved", "The name 'admin' is reserved.");

// Async
await rules.For(() => model.Email)
           .MustAsync(async (email, ct) => await _db.IsUniqueAsync(email, ct),
                      "Email.NotUnique", "Email is already taken.");
```

## Nested Validation

```csharp
await rules.For(() => model.Address!).SetValidatorAsync(new AddressValidator(), ct);
// Error codes are prefixed: "Address.City.NotEmpty", "Address.ZipCode.Matches"
// Note: must be inside an async Configure to use await.
```

## Collection Validation

```csharp
rules.ForEach(() => model.Tags, (tag, tagRules) =>
{
    tagRules.For(() => tag.Name).NotEmpty().MaxLength(50);
});
// Error codes: "Tags[0].Name.NotEmpty", "Tags[1].Name.MaxLength"
```

## Native C# Control Flow

`Configure` receives the fully-resolved model instance, so **any C# control flow** works inside it — no DSL, no special API.

### Conditional Rules

```csharp
public class OrderValidator : Validator<Order>
{
    protected override ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        // Always-on rules
        rules.For(() => model.CustomerId).NotEmpty();

        // if / else branching
        if (model.OrderType == OrderType.Business)
        {
            rules.For(() => model.CompanyName).NotEmpty().MaxLength(200);
            rules.For(() => model.TaxId).NotEmpty().Matches(@"^\d{10}$");
        }
        else
        {
            rules.For(() => model.FirstName).NotEmpty().MaxLength(100);
            rules.For(() => model.LastName).NotEmpty().MaxLength(100);
        }

        // switch expression
        switch (model.Country)
        {
            case "TR":
                rules.For(() => model.NationalId).NotEmpty().MinLength(11);
                break;
            case "US":
                rules.For(() => model.SSN).NotEmpty().Matches(@"^\d{3}-\d{2}-\d{4}$");
                break;
        }

        // Early return — remaining rules skipped when Terms not accepted
        if (!model.AcceptsTerms) return ValueTask.CompletedTask;
        rules.For(() => model.Signature).NotEmpty();
        return ValueTask.CompletedTask;
    }
}
```

### Null-Guard Before Nested Validation

```csharp
if (model.ShippingAddress is not null)
    await rules.For(() => model.ShippingAddress!).SetValidatorAsync(new AddressValidator(), ct);
```

### CSharpEssentials.Core Integration

`CSharpEssentials.Core` is transitively available (via `CSharpEssentials.Errors`) and ships a rich set of conditional helpers that compose naturally with validators.

**`IfNotNull` — run rules only when a property has a value:**

```csharp
// native C#:
if (model.Coupon is not null)
    rules.For(() => model.Coupon.Code).NotEmpty();

// with Core helper — same behaviour, one expression:
model.Coupon.IfNotNull(coupon =>
    rules.For(() => coupon.Code).NotEmpty());
```

**`WhereIf` — filter a collection before iterating:**

```csharp
// Only validate active items
rules.ForEach(
    () => model.Items.WhereIf(model.ValidateActiveOnly, i => i.IsActive),
    (item, itemRules) => itemRules.For(() => item.Sku).NotEmpty());
```

**`WithoutNulls` — skip null elements in a collection:**

```csharp
rules.ForEach(
    () => model.Tags.WithoutNulls(),
    (tag, tagRules) => tagRules.For(() => tag).NotEmpty().MaxLength(50));
```

**`IsEmpty` / `IsNotEmpty` — readable null/empty guards:**

```csharp
if (model.PromoCode.IsNotEmpty())
    rules.For(() => model.PromoCode).Matches(@"^PROMO-\d{4}$");
```

**`IfTrue` / `IfFalse` — boolean action helpers:**

```csharp
model.IsInternational.IfTrue(() =>
    rules.For(() => model.PassportNumber).NotEmpty());
```

> Install `CSharpEssentials.Core` separately if you are not using the meta-package.

## Multiple Validators for One Model

Use `Include` to compose base validators, or inject several `IValidator<T>` implementations for the same model type.

### Composition with `Include`

```csharp
public class BaseOrderValidator : Validator<Order>
{
    protected override ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerId).NotEmpty();
        rules.For(() => model.Items).NotEmpty();
        return ValueTask.CompletedTask;
    }
}

public class PaidOrderValidator : Validator<Order>
{
    protected override async ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        await Include(new BaseOrderValidator(), model, rules, ct);
        // additional rules for paid orders
        rules.For(() => model.PaymentReference).NotEmpty();
    }
}
```

`Include` runs the included validator in-line and merges all errors into the same `Result<T>`.

### Multiple DI Validators

When several `IValidator<Order>` implementations are registered, the `ValidationBehavior` in `CSharpEssentials.Mediator` aggregates results from **all** of them, deduplicating identical errors automatically.

```csharp
services.AddValidator<Order, BaseOrderValidator>();
services.AddValidator<Order, PaidOrderValidator>();
// Both validators run; errors are merged and deduplicated
```

## Validator Ordering

Override `Order` on `Validator<T>` to control execution sequence when multiple validators are registered for the same model type.

```csharp
public class FormatValidator : Validator<CreateUserCommand>
{
    public override int Order => 0;  // default — runs first

    protected override ValueTask Configure(CreateUserCommand model, RuleContext<CreateUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        return ValueTask.CompletedTask;
    }
}

public class BusinessRulesValidator : Validator<CreateUserCommand>
{
    public override int Order => 1;  // runs after Order=0 group completes

    protected override ValueTask Configure(CreateUserCommand model, RuleContext<CreateUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).Must(email => !_blocklist.Contains(email), "Email.Blocked", "Email is blocked.");
        return ValueTask.CompletedTask;
    }
}
```

**Execution rules:**
- Validators sharing the same `Order` run **concurrently** within their group.
- Groups with lower `Order` run **sequentially before** groups with higher `Order`.
- All groups execute regardless of earlier failures — errors from all groups are accumulated and deduplicated.
- Default `Order` is `0` — all validators without an override run concurrently in one group.

## DI Registration

```csharp
// Register a single validator
services.AddValidator<CreateUserCommand, CreateUserCommandValidator>();

// Scan and register all validators from an assembly
services.AddValidatorsFromAssembly(typeof(CreateUserCommandValidator).Assembly);

// Multiple assemblies
services.AddValidatorsFromAssemblies([
    typeof(CreateUserCommandValidator).Assembly,
    typeof(UpdateProductValidator).Assembly
]);
```

Default lifetime: `Scoped`. Pass a `ServiceLifetime` parameter to override.

Validators with no scoped dependencies may be registered as `Singleton`. Validators that inject scoped services (e.g. `DbContext`, `ICurrentUser`) must be `Scoped`.

## Mediator Pipeline Integration

Use with `CSharpEssentials.Mediator` for automatic validation before handlers run:

```csharp
services.AddMediatorValidationBehavior();
// or
services.AddMediatorBehaviors(); // registers all behaviors
```

`ValidationBehavior` is registered as `Scoped`. It runs all registered `IValidator<TRequest>` implementations, groups them by `Order`, and returns `Result.Failure` if any errors are found — the handler is never invoked.

**Exception isolation:** If a validator throws a non-`OperationCanceledException` exception, `ValidationBehavior` catches it and converts it to a `Result.Failure` error (code `"Validator.Exception"`). `OperationCanceledException` always propagates — cancellation is never swallowed.

See [CSharpEssentials.Mediator](../CSharpEssentials.Mediator/Readme.MD) for full pipeline documentation.

## FluentValidation Comparison

> **Benchmarks:** Head-to-head numbers across 23 scenarios (construction, invalid path, valid path,
> collections, async, nested, cascade) are documented in [BENCHMARKS.md](./BENCHMARKS.md).
> Key findings: validator construction is **776× faster**, invalid paths are **5–8× faster** with
> **3–5× less memory**, complex/large-collection valid paths beat FV on both time and memory,
> and large collections (50 items) run **3× faster** with **27% less allocation**.

| | CSharpEssentials.Validation | FluentValidation |
|---|---|---|
| **Rule definition** | `ValueTask Configure(model, rules, ct)` — model is the live instance; sync validators `return ValueTask.CompletedTask` | Constructor — `RuleFor(x => x.Name)` stores expressions |
| **Conditional rules** | Native `if`/`else`/`switch` — no DSL | `.When(x => ..., () => { ... })` block or per-rule `.When()` chain |
| **Conditional style** | Standard C# — zero extra API to learn | Dedicated `When`, `Unless`, `WhenAsync`, `UnlessAsync` methods |
| **Property name** | Extracted from `CallerArgumentExpression` at compile time | Extracted from `Expression<Func<T,TProperty>>` at compile time |
| **Reflection** | Zero — no expression trees, no reflection at runtime | Expression trees at rule-definition time; compiled to delegates |
| **AOT / NativeAOT** | Fully compatible | Expression tree compilation can cause issues |
| **Async rules** | `MustAsync`, `ForEachAsync`, `SetValidatorAsync`; `ValidateAsync` returns `ValueTask<Result<T>>` | `MustAsync`, `WhenAsync`; `ValidateAsync` returns `Task<ValidationResult>` |
| **Result type** | Returns `Result<T>` natively (railway-oriented) | Returns `ValidationResult` — separate mapping needed |
| **Error type** | Structured `Error` (code, description, type, metadata) | `ValidationFailure` (PropertyName, ErrorMessage, …) |
| **DI lifetime** | Scoped by default; `Singleton`-safe | Typically `Singleton` (stateless by design) |

### Why We Chose This Approach

FluentValidation's constructor-based design forces rule registration **without a model instance**. This means:

- You cannot write `if (model.X) { ... }` in the constructor — the model doesn't exist yet.
- The `When()` DSL exists solely to compensate for this limitation.
- Every developer must learn an extra API layer that simply reimplements what C# `if` already provides.

By passing `model` directly into `Configure`, CSharpEssentials.Validation eliminates the need for `When`/`Unless` entirely. Branching logic is written in idiomatic C#, readable by anyone, debuggable with standard tooling, and refactorable without ceremony.

The trade-off: a new `RuleContext<T>` is created per `ValidateAsync` call. This is intentional: the context is a small, short-lived object that keeps the design thread-safe and allocation-predictable. `Validator<T>` instances themselves are stateless and can be registered as `Singleton` when they have no scoped dependencies. Validators that inject scoped services (e.g. `DbContext`) should be registered as `Scoped`.

### FluentValidation Advantages

- Mature ecosystem, wide community adoption
- Rich `.WithMessage`, `.WithName`, `.WithErrorCode` fluent API
- Built-in rule sets (`RuleSet`) for named validation scenarios
- `.Cascade(CascadeMode.StopOnFirstFailure)` configurable globally

### FluentValidation Disadvantages

- Cannot use plain `if` for conditional rules without DSL
- `ValidationResult` is not railway-oriented — requires manual `Result<T>` mapping
- Expression trees limit Native AOT compatibility
- `When()` blocks create implicit coupling between rule registration and condition evaluation

## Dependencies

- `CSharpEssentials.Results`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
