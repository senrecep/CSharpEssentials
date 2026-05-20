---
name: csharpessentials-validation
description: Use when writing model validation with Result<T> integration — Validator<T> base class, rules.For().NotEmpty()/.MaxLength()/.GreaterThan() chains, SetValidator for nested objects, ForEach for collections, native C# if/switch for conditional rules. Also use when migrating FROM FluentValidation — this skill contains a full side-by-side migration guide.
---

# CSharpEssentials.Validation

High-performance, model-first validation. Zero reflection at runtime — no expression trees, no deferred builds. Returns `Result<T>` natively.

> **Migrating from FluentValidation?** Jump to [Migration Guide](#fluent-validation-migration-guide) at the bottom.

## Installation

```bash
dotnet add package CSharpEssentials.Validation
```

## Namespace

```csharp
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
```

---

## Defining a Validator

Extend `Validator<T>` and override `Configure`. The model instance is available directly — no expression trees, no lambdas over `x =>`.

```csharp
public class CreateUserCommandValidator : Validator<CreateUserCommand>
{
    protected override ValueTask Configure(CreateUserCommand model, RuleContext<CreateUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        rules.For(() => model.Name).NotEmpty().MaxLength(100);
        rules.For(() => model.Age).GreaterThan(0).LessThan(120);
        return ValueTask.CompletedTask;
    }
}
```

Invoke directly or via DI:

```csharp
var validator = new CreateUserCommandValidator();
Result<CreateUserCommand> result = await validator.ValidateAsync(command);
// or sync: validator.ValidateAsync(command).GetAwaiter().GetResult()

if (result.IsFailure)
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.Code}: {error.Description}");
        // e.g. "Email.NotEmpty: 'Email' must not be empty."
```

### Inline (Static) Usage

For one-off validations without a dedicated class, use the static `Validator.ValidateAsync`:

```csharp
// Sync delegate — zero heap allocation
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, (m, rules) =>
{
    rules.For(() => m.Email).NotEmpty().EmailAddress();
    rules.For(() => m.Name).NotEmpty().MaxLength(100);
});

// Async delegate — when MustAsync or SetValidatorAsync is needed
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, async (m, rules, ct) =>
{
    rules.For(() => m.Name).NotEmpty();
    await rules.For(() => m.Email)
               .MustAsync(async (email, c) => await _db.IsUniqueAsync(email, c),
                          "Email.NotUnique", "Email is already taken.", c);
}, cancellationToken);
```

`Validator.ValidateAsync` (static utility class) and `Validator<T>` (abstract base class) are two separate types defined in the same file. They are fully independent — the static form does not delegate to `Validator<T>` internally.

---

## String Validators

```csharp
rules.For(() => model.Name)
    .NotEmpty()           // fails on null / "" / whitespace
    .NotNull()            // fails on null only
    .MinLength(2)         // < 2 chars fails
    .MaxLength(100)       // > 100 chars fails
    .Length(2, 100)       // outside [2,100] fails
    .EmailAddress()       // invalid email format fails
    .Matches(@"^\d+$")    // regex mismatch fails
    .Contains("@")        // substring absent fails
    .StartsWith("Mr")     // prefix mismatch fails
    .EndsWith(".com");    // suffix mismatch fails
```

Null behaviour: `NotEmpty` / `NotNull` fail on null. All other string validators **skip** null (no error).

---

## Comparable Validators (`int`, `decimal`, `DateTime`, …)

```csharp
rules.For(() => model.Age)
    .GreaterThan(0)
    .GreaterThanOrEqualTo(18)
    .LessThan(150)
    .LessThanOrEqualTo(120)
    .InclusiveBetween(18, 65)
    .ExclusiveBetween(0, 100)
    .Equal(42)
    .NotEqual(0);
```

---

## Collection Validators

Works with any nullable collection type: `List<T>?`, `IEnumerable<T>?`, `IList<T>?`, `IReadOnlyList<T>?`, `T[]?`, and any type implementing `IEnumerable`.

```csharp
rules.For(() => model.Tags)      // Tags can be List<string>?, IEnumerable<string>?, string[]?, etc.
    .NotEmpty()        // null or empty collection fails
    .NotNull()         // null fails
    .MinCount(1)       // fewer than 1 element fails
    .MaxCount(10)      // more than 10 elements fails
    .CountBetween(1, 10);
```

---

## Nullable Struct Validators (`int?`, `DateTime?`, …)

All comparable validators work on nullable value types — `null` is silently skipped.

```csharp
rules.For(() => model.ExpiresAt).GreaterThan(DateTime.UtcNow);
// expiresAt == null → no error; expiresAt < now → error
```

---

## CascadeMode

Default (`Stop`): first failure stops the chain. Switch to `Continue` to collect all errors for a property.

```csharp
rules.For(() => model.Password)
    .Cascade(CascadeMode.Continue)
    .MinLength(8)
    .Matches(@"[A-Z]", message: "Must contain an uppercase letter.")
    .Matches(@"[0-9]", message: "Must contain a digit.");
```

---

## Custom Predicates

```csharp
// Sync
rules.For(() => model.Username)
    .Must(name => name != "admin", "Username.Reserved", "The name 'admin' is reserved.");

// Async
await rules.For(() => model.Email)
           .MustAsync(async (email, ct) => await _db.IsUniqueAsync(email, ct),
                      "Email.NotUnique", "Email is already taken.");
```

---

## Nested Object Validation

Works with both non-nullable and nullable reference type properties — no null-forgiving operator needed.
`SetValidatorAsync` skips `null` values automatically.

```csharp
// Non-nullable property
await rules.For(() => model.Address).SetValidatorAsync(new AddressValidator(), ct);

// Nullable reference type property — works directly, no ! required
await rules.For(() => model.BillingAddress).SetValidatorAsync(new AddressValidator(), ct);
// model.BillingAddress is Address? — null is silently skipped

// Error codes are prefixed: "Address.City.NotEmpty", "Address.ZipCode.Matches"
// Note: must be inside an async Configure to use await.
```

---

## Collection Item Validation

```csharp
rules.ForEach(() => model.Tags, (tag, tagRules) =>
    tagRules.For(() => tag).NotEmpty().MaxLength(50));
// Error codes: "Tags[0].NotEmpty", "Tags[1].MaxLength"
```

Async variant:

```csharp
await rules.ForEachAsync(() => model.Items, async (item, itemRules, ct) =>
{
    itemRules.For(() => item.Sku).NotEmpty();
    await itemRules.For(() => item.Sku)
                   .MustAsync(async (sku, c) => await _db.SkuExistsAsync(sku, c), "Sku.NotFound", "SKU not found.");
}, ct);
```

---

## Native C# Conditional Rules

`Configure` receives the live model — any C# control flow works directly. No `When()`/`Unless()` DSL needed.

```csharp
protected override ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
{
    // Always-on
    rules.For(() => model.CustomerId).NotEmpty();

    // if / else
    if (model.OrderType == OrderType.Business)
    {
        rules.For(() => model.CompanyName).NotEmpty().MaxLength(200);
        rules.For(() => model.TaxId).Matches(@"^\d{10}$");
    }
    else
    {
        rules.For(() => model.FirstName).NotEmpty().MaxLength(100);
    }

    // switch
    switch (model.Country)
    {
        case "TR": rules.For(() => model.NationalId).MinLength(11); break;
        case "US": rules.For(() => model.SSN).Matches(@"^\d{3}-\d{2}-\d{4}$"); break;
    }

    // early return
    if (!model.AcceptsTerms) return ValueTask.CompletedTask;
    rules.For(() => model.Signature).NotEmpty();
    return ValueTask.CompletedTask;
}
```

---

## CSharpEssentials.Core Integration

`CSharpEssentials.Core` is transitively available (via `CSharpEssentials.Errors`). Its conditional helpers compose naturally inside `Configure`.

```csharp
// IfNotNull — run rules only when property is non-null
model.Coupon.IfNotNull(coupon =>
    rules.For(() => coupon.Code).NotEmpty().Matches(@"^COUP-\d{6}$"));

// IfTrue / IfFalse — boolean action helpers
model.IsInternational.IfTrue(() =>
    rules.For(() => model.PassportNumber).NotEmpty());

// IsNotEmpty — readable null/empty guard
if (model.PromoCode.IsNotEmpty())
    rules.For(() => model.PromoCode).Matches(@"^PROMO-\d{4}$");

// WhereIf — conditional collection filtering before ForEach
rules.ForEach(
    () => model.Items.WhereIf(model.OnlyActiveItems, i => i.IsActive),
    (item, itemRules) => itemRules.For(() => item.Sku).NotEmpty());

// WithoutNulls — skip null elements in a collection
rules.ForEach(
    () => model.Tags.WithoutNulls(),
    (tag, tagRules) => tagRules.For(() => tag).NotEmpty().MaxLength(50));
```

---

## Validator Composition (Include)

Use `rules.Include()` to merge another validator's rules in-line. All errors are merged into the same `Result<T>`.

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
        rules.For(() => model.PaymentReference).NotEmpty();
    }
}
```

---

## DI Registration

```csharp
// Single validator
services.AddValidator<CreateUserCommand, CreateUserCommandValidator>();

// All validators in an assembly
services.AddValidatorsFromAssembly(typeof(CreateUserCommandValidator).Assembly);

// Multiple assemblies
services.AddValidatorsFromAssemblies([
    typeof(CreateUserCommandValidator).Assembly,
    typeof(UpdateProductValidator).Assembly
]);
```

Default lifetime: `Scoped`. Override with the `lifetime` parameter.

Multiple `IValidator<T>` registrations for the same `T` are supported — `ValidationBehavior` aggregates and deduplicates results from all of them automatically.

---

## Validator Ordering

Override `Order` on `Validator<T>` to control execution sequence when multiple validators are registered for the same model.

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

Rules:
- Validators sharing the same `Order` run **concurrently** within their group.
- Groups with lower `Order` run **sequentially before** groups with higher `Order`.
- All groups execute regardless of earlier failures — errors from all groups are accumulated and deduplicated.
- Default `Order` is `0` — validators with no override all run concurrently in a single group.

---

## Mediator Pipeline Integration

```csharp
services.AddMediatorValidationBehavior();
// or register all behaviors:
services.AddMediatorBehaviors();
```

Validation runs before the handler. On failure, the handler is never invoked. Errors are surfaced based on the handler return type: `Result` and `Result<T>` handlers receive `Result.Failure` directly; handlers with any other return type cause `EnhancedValidationException` to be thrown (caught by `GlobalExceptionHandler`). See `csharpessentials-mediator` skill for full pipeline docs.

**Exception isolation:** If a validator throws a non-`OperationCanceledException` exception, `ValidationBehavior` catches it and converts it to `Error.Exception("Validator.Exception", ex)` — the pipeline never rethrows validator bugs. `OperationCanceledException` always propagates so cancellation is respected.

---

## Best Practices

- Chain guard validators first (`NotEmpty` / `NotNull`) — stop mode prevents subsequent validators from running on null/empty values.
- Use native `if` / `switch` for conditional rules — no DSL required.
- Use `SetValidator` for nested objects; use `ForEach` for collections.
- Validators with no scoped dependencies may be registered as `Singleton`. Validators that inject scoped services (e.g. `DbContext`, `ICurrentUser`) must be registered as `Scoped`. `ValidationBehavior` is always `Scoped` — it adapts to the lifetime of injected validators.
- Use `CascadeMode.Continue` only when the client needs all errors for a field simultaneously (e.g. password strength rules).

---

## FluentValidation Migration Guide

### 1 — Swap the Package

```bash
# Remove
dotnet remove package FluentValidation
dotnet remove package FluentValidation.DependencyInjectionExtensions

# Add
dotnet add package CSharpEssentials.Validation
```

### 2 — Validator Class

| FluentValidation | CSharpEssentials.Validation |
|---|---|
| `AbstractValidator<T>` | `Validator<T>` |
| Constructor + `RuleFor(x => x.Name)` | `Configure(T model, RuleContext<T> rules)` override |
| `IValidator<T>` | `IValidator<T>` (same interface name, different namespace) |

```csharp
// FluentValidation — BEFORE
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MaxLength(100);
        RuleFor(x => x.Age).GreaterThan(0);
    }
}

// CSharpEssentials.Validation — AFTER
public class CreateUserValidator : Validator<CreateUserCommand>
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

### 3 — Conditional Rules

```csharp
// FluentValidation — BEFORE
When(x => x.IsInternational, () =>
{
    RuleFor(x => x.PassportNumber).NotEmpty();
    RuleFor(x => x.VisaType).NotEmpty();
});

// CSharpEssentials.Validation — AFTER (plain C#)
if (model.IsInternational)
{
    rules.For(() => model.PassportNumber).NotEmpty();
    rules.For(() => model.VisaType).NotEmpty();
}
```

```csharp
// FluentValidation per-rule When — BEFORE
RuleFor(x => x.DriversLicense).NotEmpty().When(x => x.Age >= 18);

// CSharpEssentials.Validation — AFTER
if (model.Age >= 18)
    rules.For(() => model.DriversLicense).NotEmpty();
```

### 4 — Nested Object Validation

```csharp
// FluentValidation — BEFORE
RuleFor(x => x.Address).SetValidator(new AddressValidator());

// CSharpEssentials.Validation — AFTER (inside async Configure)
await rules.For(() => model.Address).SetValidatorAsync(new AddressValidator(), ct);
// Works for both Address and Address? — null is skipped automatically, no ! needed
```

### 5 — Collection Item Validation

```csharp
// FluentValidation — BEFORE
RuleForEach(x => x.Tags).NotEmpty().MaximumLength(50);

// CSharpEssentials.Validation — AFTER
rules.ForEach(() => model.Tags, (tag, tagRules) =>
    tagRules.For(() => tag).NotEmpty().MaxLength(50));
```

### 6 — Validator Composition (Include)

```csharp
// FluentValidation — BEFORE (inside constructor)
Include(new BaseValidator());

// CSharpEssentials.Validation — AFTER (inside async Configure)
await Include(new BaseValidator(), model, rules, ct);
```

### 7 — DI Registration

```csharp
// FluentValidation — BEFORE
services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

// CSharpEssentials.Validation — AFTER
services.AddValidatorsFromAssembly(typeof(CreateUserValidator).Assembly);
```

### 8 — Reading Validation Errors

```csharp
// FluentValidation — BEFORE
ValidationResult result = validator.Validate(command);
if (!result.IsValid)
    foreach (var failure in result.Errors)
        Console.WriteLine($"{failure.PropertyName}: {failure.ErrorMessage}");

// CSharpEssentials.Validation — AFTER
Result<CreateUserCommand> result = await validator.ValidateAsync(command);
// or sync: validator.ValidateAsync(command).GetAwaiter().GetResult()
if (result.IsFailure)
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.Code}: {error.Description}");
        // Code = "Email.NotEmpty"  Description = "'Email' must not be empty."
```

### 9 — Async Validation

```csharp
// FluentValidation — BEFORE
ValidationResult result = await validator.ValidateAsync(command, ct);

// CSharpEssentials.Validation — AFTER
Result<CreateUserCommand> result = await validator.ValidateAsync(command, ct);
// (no separate ValidateAsync needed — same method for sync and async validators)
```

### 10 — CascadeMode

```csharp
// FluentValidation per-rule — BEFORE
RuleFor(x => x.Password)
    .Cascade(CascadeMode.Continue)
    .MinimumLength(8)
    .Matches("[A-Z]");

// CSharpEssentials.Validation — AFTER (same concept)
rules.For(() => model.Password)
    .Cascade(CascadeMode.Continue)
    .MinLength(8)
    .Matches(@"[A-Z]");
```

### 11 — Validator Name Changes

| FluentValidation | CSharpEssentials.Validation | Notes |
|---|---|---|
| `NotEmpty()` | `NotEmpty()` | identical |
| `NotNull()` | `NotNull()` | identical |
| `MinimumLength(n)` | `MinLength(n)` | renamed |
| `MaximumLength(n)` | `MaxLength(n)` | renamed |
| `Length(min, max)` | `Length(min, max)` | identical |
| `EmailAddress()` | `EmailAddress()` | identical |
| `Matches(pattern)` | `Matches(pattern)` | identical |
| `GreaterThan(n)` | `GreaterThan(n)` | identical |
| `LessThan(n)` | `LessThan(n)` | identical |
| `InclusiveBetween(a,b)` | `InclusiveBetween(a,b)` | identical |
| `ExclusiveBetween(a,b)` | `ExclusiveBetween(a,b)` | identical |
| `Equal(v)` | `Equal(v)` | identical |
| `NotEqual(v)` | `NotEqual(v)` | identical |
| `Must(predicate)` | `Must(predicate, code, msg)` | requires explicit error code |
| `MustAsync(predicate)` | `MustAsync(predicate, code, msg)` | requires explicit error code |
| `SetValidator(v)` | `await SetValidatorAsync(v, ct)` | async; inside async Configure |
| `RuleForEach(…)` | `rules.ForEach(…)` | different shape — see §5 |
| `.When(condition)` | native `if (condition)` | no DSL needed |
| `.WithMessage(msg)` | `NotEmpty(message: "…")` | pass message as parameter |

### Common Pitfalls During Migration

- **`When()` blocks**: Delete them and replace with plain `if`. The model is available directly.
- **`.WithMessage()`**: Pass the custom message as the last parameter of the validator method: `.NotEmpty(message: "Custom message")`.
- **`.WithErrorCode()`**: Error codes in CSharpEssentials are auto-generated as `"PropertyName.ValidatorName"` (e.g. `"Email.NotEmpty"`). There is no `.WithErrorCode()` — use `.Must(pred, "MyCode", "My message")` for custom codes.
- **`IRuleBuilder` extensions**: Custom validators written as `IRuleBuilderOptions<T, TProperty>` extensions must be rewritten as `RuleChain<T, TProp>` extensions.
- **`ValidationContext`**: No equivalent — `Configure` receives the model directly. Contextual data should be passed via constructor injection on the validator.
- **`RuleSet`**: No built-in equivalent. Use validator composition (`Include`) or separate validator classes per scenario.
- **No sync `Validate()`**: Use `ValidateAsync().GetAwaiter().GetResult()` at sync call sites. This is safe when the validator has no actual async operations (returns a pre-completed `ValueTask`).
- **`Configure` returns `ValueTask`**: Sync-only validators must `return ValueTask.CompletedTask;`. Async validators use `async ValueTask` and can `await` freely.
- **`SetValidator` removed**: Use `await SetValidatorAsync(validator, ct)` inside an `async ValueTask Configure(...)`.
- **`Include` must be awaited**: `await Include(new BaseValidator(), model, rules, ct)` inside async Configure.
