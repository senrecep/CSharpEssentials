---
name: csharpessentials-validation
description: Use when writing model validation that returns Result<T>. Covers Validator<T> with Configure(model, rules, ct), rules.For(() => model.X).NotEmpty()/MaxLength()/GreaterThan() chains, Must/MustAsync, SetValidatorAsync for nested objects, ForEach/ForEachAsync for collections, native if/switch for conditional rules, Include and Order for composition, AddValidator/AddValidatorsFromAssembly (idempotent), ValidateWith/ValidateWithAsync railway bindings. Also use when migrating from FluentValidation.
---

# CSharpEssentials.Validation

Model-first validation. `Configure` receives the live model, so rules are plain C#: no expression trees, no `When()` DSL. Returns `Result<T>`.

> **Migrating from FluentValidation?** See the [migration guide](#fluentvalidation-migration-guide) at the bottom.

## Installation

```bash
dotnet add package CSharpEssentials.Validation
```

## Namespace

```csharp
using CSharpEssentials.Validation;             // Validator<T>, RuleContext<T>, ValidateWith...
using CSharpEssentials.Validation.Validators;  // NotEmpty, MaxLength, GreaterThan, SetValidatorAsync, ...
using CSharpEssentials.Validation.Extensions;  // AddValidator, AddValidatorsFromAssembly(ies)
```

Without the `Validators` using, `rules.For(...).NotEmpty()` does not compile. `Result<T>` is in `CSharpEssentials.ResultPattern`, `Error` in `CSharpEssentials.Errors`.

---

## Defining a Validator

Extend `Validator<T>` and override `Configure`.

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

Run it directly or through DI:

```csharp
var validator = new CreateUserCommandValidator();
Result<CreateUserCommand> result = await validator.ValidateAsync(command);

if (result.IsFailure)
    foreach (Error error in result.Errors)
        Console.WriteLine($"{error.Code}: {error.Description}"); // "Email.NotEmpty: 'Email' must not be empty."
```

There is no sync `Validate()`. A validator without async work completes synchronously, so `ValidateAsync(...).GetAwaiter().GetResult()` is safe at sync call sites.

### Inline (Static) Usage

```csharp
// Sync delegate
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, (m, rules) =>
{
    rules.For(() => m.Email).NotEmpty().EmailAddress();
    rules.For(() => m.Name).NotEmpty().MaxLength(100);
});

// Async delegate, when MustAsync or SetValidatorAsync is needed
Result<CreateUserCommand> checkedResult = await Validator.ValidateAsync(command, async (m, rules, ct) =>
{
    rules.For(() => m.Name).NotEmpty();
    await rules.For(() => m.Email)
               .MustAsync(async (email, c) => await _db.IsUniqueAsync(email, c),
                          "Email.NotUnique", "Email is already taken.", ct);
}, cancellationToken);
```

The static `Validator` class and the abstract `Validator<T>` are independent types.

---

## Built-in Validators

The fragments below run inside `Configure`, where `model` and `rules` are in scope.

```csharp
// Strings: NotEmpty/NotNull fail on null; all others skip null
rules.For(() => model.Name)
    .NotEmpty()           // null, "" or whitespace fails
    .MinLength(2)
    .MaxLength(100)
    .Length(2, 100)
    .Matches(@"^[A-Za-z ]+$")
    .Contains("a")
    .StartsWith("A")
    .EndsWith("z");

rules.For(() => model.Email).EmailAddress();

// Comparable values (int, decimal, DateTime, …)
rules.For(() => model.Age)
    .GreaterThanOrEqualTo(18)
    .LessThanOrEqualTo(120)
    .InclusiveBetween(18, 65)
    .NotEqual(0);

// Collections (List<T>?, IEnumerable<T>?, T[]?, …)
rules.For(() => model.Tags)
    .NotEmpty()           // null or empty fails
    .MinCount(1)
    .MaxCount(10)
    .CountBetween(1, 10);

// Nullable structs: null is skipped (Equal/NotEqual are not available; use NotNull()/Null())
rules.For(() => model.ExpiresAt).GreaterThan(DateTime.UtcNow);

// Strings also have Equal/NotEqual (ordinal, or pass a StringComparison) and Matches(Regex)
// Enums: IsDefinedEnum(), IsOneOf(...), HasOnlyDefinedFlags()
```

Every validator takes an optional `message`, and most have an overload that takes a full `Error` for a custom code: `.NotEmpty(Error.Validation("User.NameRequired", "Name is required."))`.

---

## CascadeMode

Default is `CascadeMode.Stop`: the first failure stops the chain. Use `Continue` to collect every error for one property.

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
// Sync, with an explicit code
rules.For(() => model.Username)
    .Must(name => name != "admin", "Username.Reserved", "The name 'admin' is reserved.");

// Async: Task<bool> or ValueTask<bool> predicates
await rules.For(() => model.Email)
           .MustAsync(async (email, c) => await _db.IsUniqueAsync(email, c),
                      "Email.NotUnique", "Email is already taken.", ct);
```

`Must(predicate)` without a code produces `"<Property>.Must"` with the predicate text in the message.

---

## Nested Objects

`SetValidatorAsync` skips `null`, so nullable properties need no `!`. Child error codes are prefixed with the property path (`"Address.City.NotEmpty"`). Use it inside an `async` `Configure`.

```csharp
await rules.For(() => model.Address).SetValidatorAsync(new AddressValidator(), ct);
await rules.For(() => model.BillingAddress).SetValidatorAsync(new AddressValidator(), ct); // Address?, null is skipped
```

A `null` intermediate member is tolerated: `rules.For(() => model.Address.City)` with a `null` `Address` treats the value as `null`, so `NotEmpty()` fails and null-tolerant validators are skipped. A `null` collection in `ForEach` produces no errors.

---

## Collection Items

```csharp
rules.ForEach(() => model.Tags, (tag, tagRules) =>
    tagRules.For(() => tag).NotEmpty().MaxLength(50));
// Error codes: "Tags[0].NotEmpty", "Tags[1].MaxLength"

await rules.ForEachAsync(() => model.Items, async (item, itemRules, c) =>
{
    itemRules.For(() => item.Sku).NotEmpty();
    await itemRules.For(() => item.Sku)
                   .MustAsync(async (sku, token) => await _db.SkuExistsAsync(sku, token), "Sku.NotFound", "SKU not found.", c);
}, ct);
```

---

## Native C# Conditional Rules

```csharp
public class OrderValidator : Validator<Order>
{
    protected override ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerId).NotEmpty();

        if (model.OrderType == OrderType.Business)
        {
            rules.For(() => model.CompanyName).NotEmpty().MaxLength(200);
            rules.For(() => model.TaxId).Matches(@"^\d{10}$");
        }
        else
        {
            rules.For(() => model.FirstName).NotEmpty().MaxLength(100);
        }

        switch (model.Country)
        {
            case "TR": rules.For(() => model.NationalId).MinLength(11); break;
            case "US": rules.For(() => model.SSN).Matches(@"^\d{3}-\d{2}-\d{4}$"); break;
        }

        if (!model.AcceptsTerms) return ValueTask.CompletedTask;
        rules.For(() => model.Signature).NotEmpty();
        return ValueTask.CompletedTask;
    }
}
```

`CSharpEssentials.Core` helpers compose too: `model.Coupon.IfNotNull(c => rules.For(() => c.Code).NotEmpty())`, `rules.ForEach(() => model.Items.WhereIf(onlyActive, i => i.IsActive), …)`, `rules.ForEach(() => model.Tags.WithoutNulls(), …)`. The property name in the error code comes from the lambda text. Method calls, `!` and `?` are dropped, so the filtered collections give `Tags[1].NotEmpty` and `Items[0].Sku.NotEmpty`. For the `IfNotNull` form the lambda only sees `c`, so the code is `Code.NotEmpty`; use `rules.For(model.Coupon.Code, "Coupon.Code")` to name the full path.

---

## Custom Rules and Manual Failures

```csharp
public static class EvenValidators
{
    public static RuleChain<T, int> Even<T>(this RuleChain<T, int> chain, string? message = null)
    {
        if (!chain.HasFailed && chain.Value % 2 != 0)
            chain.AddError(Error.Validation($"{chain.PropertyName}.Even", message ?? $"'{chain.PropertyName}' must be even."));
        return chain;
    }
}

rules.AddFailure("Order.Total", "The total does not match the line items."); // model-level failure
rules.For(model.Name, nameof(model.Name)).NotEmpty();                         // pre-evaluated value, no lambda
```

---

## Composition and Ordering

`Include` merges another validator's rules into the same result:

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

When several `IValidator<T>` are registered for one model, the Mediator `ValidationBehavior` groups them by `Order`:

```csharp
public class BusinessRulesValidator(IReadOnlySet<string> blocklist) : Validator<CreateUserCommand>
{
    public override int Order => 1; // runs after the Order = 0 group

    protected override ValueTask Configure(CreateUserCommand model, RuleContext<CreateUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).Must(email => !blocklist.Contains(email!), "Email.Blocked", "Email is blocked.");
        return ValueTask.CompletedTask;
    }
}
```

- Validators with the same `Order` run concurrently; lower groups run before higher groups.
- All groups run regardless of earlier failures; errors are merged and deduplicated.

---

## DI Registration

```csharp
services.AddValidator<CreateUserCommand, CreateUserCommandValidator>();
services.AddValidatorsFromAssembly(typeof(CreateUserCommandValidator).Assembly);
services.AddValidatorsFromAssemblies([typeof(CreateUserCommandValidator).Assembly, typeof(OrderValidator).Assembly]);
```

- Default lifetime is `Scoped`; pass a `ServiceLifetime` to change it.
- Registration uses `TryAddEnumerable` (4.0): calling `AddValidator` or `AddValidatorsFromAssembly` twice does not register the same validator twice. Different validators for the same model are all kept.

---

## Railway Bindings

`ValidateWith` (sync) and `ValidateWithAsync` work on `Result<T>`, `Task<Result<T>>` and `ValueTask<Result<T>>`. A failed source result short-circuits.

```csharp
Result<CreateUserCommand> validated = GetUserCommand()
    .ValidateWith((cmd, rules) => rules.For(() => cmd.Email).NotEmpty().EmailAddress());

Result<CreateUserCommand> viaValidator = await GetUserCommand()
    .ValidateWithAsync(new CreateUserCommandValidator());
```

---

## Mediator Pipeline Integration

```csharp
services.AddMediatorValidationBehavior();
// or all behaviors:
services.AddMediatorBehaviors();
```

Validation runs before the handler; on failure the handler is not invoked. `Result` / `Result<T>` handlers get the failure as their return value; other return types get an `EnhancedValidationException`, which the AspNetCore `GlobalExceptionHandler` turns into a 400 problem response. A validator that throws (other than `OperationCanceledException`) becomes a `"Validator.Exception"` error. See the `csharpessentials-mediator` skill.

---

## Best Practices

- Put guards (`NotEmpty` / `NotNull`) first; the default stop mode skips the rest of the chain on null or empty values.
- Use `if` / `switch` for conditional rules.
- Use `SetValidatorAsync` for nested objects and `ForEach` for collections.
- Validators without scoped dependencies can be `Singleton`; validators that inject `DbContext` or other scoped services must be `Scoped`.
- Use `CascadeMode.Continue` only when the client needs every error for a field (password strength).

---

## FluentValidation Migration Guide

### 1. Swap the package

```bash
dotnet remove package FluentValidation
dotnet remove package FluentValidation.DependencyInjectionExtensions
dotnet add package CSharpEssentials.Validation
```

### 2. Map the concepts

| FluentValidation | CSharpEssentials.Validation |
|---|---|
| `AbstractValidator<T>` + constructor | `Validator<T>` + `override ValueTask Configure(T model, RuleContext<T> rules, CancellationToken ct)` |
| `RuleFor(x => x.Name)` | `rules.For(() => model.Name)` |
| `When(x => cond, () => { … })` / `.When(...)` / `.Unless(...)` | `if (cond) { … }` |
| `RuleForEach(x => x.Tags).NotEmpty()` | `rules.ForEach(() => model.Tags, (tag, r) => r.For(() => tag).NotEmpty())` |
| `.SetValidator(new AddressValidator())` | `await rules.For(() => model.Address).SetValidatorAsync(new AddressValidator(), ct)` |
| `Include(new BaseValidator())` | `await Include(new BaseValidator(), model, rules, ct)` |
| `MinimumLength(n)` / `MaximumLength(n)` | `MinLength(n)` / `MaxLength(n)` |
| `Must(pred)` / `MustAsync(pred)` | `Must(pred, code, message)` / `await MustAsync(pred, code, message, ct)` |
| `.WithMessage("…")` | `message:` argument, e.g. `.NotEmpty(message: "…")` |
| `.WithErrorCode("X")` | Pass an `Error`: `.NotEmpty(Error.Validation("X", "…"))` |
| `validator.Validate(x)` → `ValidationResult.IsValid` / `.Errors` | `await validator.ValidateAsync(x)` → `Result<T>.IsFailure` / `.Errors` (`Code`, `Description`) |
| `services.AddValidatorsFromAssemblyContaining<T>()` | `services.AddValidatorsFromAssembly(typeof(T).Assembly)` |
| `RuleSet` | Separate validators, or composition with `Include` |
| `ValidationContext` / root context data | Constructor injection on the validator |

Unchanged names: `NotEmpty`, `NotNull`, `Length`, `EmailAddress`, `Matches`, `GreaterThan`, `LessThan`, `InclusiveBetween`, `ExclusiveBetween`, `Equal`, `NotEqual`, `Cascade(CascadeMode.Continue)`.

### 3. Example

```csharp
// FluentValidation:
//   RuleFor(x => x.Email).NotEmpty().EmailAddress();
//   RuleFor(x => x.DriversLicense).NotEmpty().When(x => x.Age >= 18);
//   RuleFor(x => x.Address).SetValidator(new AddressValidator());
public class ApplicantValidator : Validator<Applicant>
{
    protected override async ValueTask Configure(Applicant model, RuleContext<Applicant> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();

        if (model.Age >= 18)
            rules.For(() => model.DriversLicense).NotEmpty();

        await rules.For(() => model.Address).SetValidatorAsync(new AddressValidator(), ct);
    }
}
```

### Pitfalls

- Error codes are generated as `"<Property>.<Validator>"` (`"Email.NotEmpty"`). Use the `Error` overloads or `Must(pred, code, message)` for custom codes.
- Sync validators must `return ValueTask.CompletedTask;`. Use `async ValueTask` when awaiting `SetValidatorAsync`, `MustAsync`, `ForEachAsync` or `Include`.
- Custom `IRuleBuilderOptions<T, TProperty>` extensions become extensions on `RuleChain<T, TProp>`.
