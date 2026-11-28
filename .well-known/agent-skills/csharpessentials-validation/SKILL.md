---
name: csharpessentials-validation
description: Use when writing model validation that returns Result<T>, or migrating from FluentValidation. Covers Validator<T>.Configure(model, rules, ct), rules.For(() => model.X) with NotEmpty/MaxLength/EmailAddress/GreaterThan/InclusiveBetween/IsDefinedEnum/MinCount, Must/MustAsync, SetValidatorAsync, ForEach/ForEachAsync, CascadeMode, Include, Order, AddValidator/AddValidatorsFromAssembly and ValidateWith/ValidateWithAsync.
---

# CSharpEssentials.Validation

`Configure` receives the live model, so conditions are plain `if`/`switch`. `ValidateAsync` returns `ValueTask<Result<T>>`.

```bash
dotnet add package CSharpEssentials.Validation
```

```csharp
using CSharpEssentials.Validation;             // Validator<T>, IValidator<T>, RuleContext<T>, RuleChain<T,TProp>, CascadeMode, ValidateWith/ValidateWithAsync
using CSharpEssentials.Validation.Validators;  // every built-in rule and SetValidatorAsync (required)
using CSharpEssentials.Validation.Extensions;  // AddValidator, AddValidatorsFromAssembly, AddValidatorsFromAssemblies
using CSharpEssentials.ResultPattern;          // Result<T>
using CSharpEssentials.Errors;                 // Error
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Validation
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

var validator = new CreateOrderValidator();
var order = new CreateOrder(
    CustomerEmail: "bad-email",
    Kind: OrderKind.Business,
    CompanyName: null,
    ShippingAddress: new Address("", "34000"),
    Lines: [new OrderLine("SKU-1", 2), new OrderLine("", 0)]);

Result<CreateOrder> result = await validator.ValidateAsync(order);
foreach (Error error in result.Errors)
    Console.WriteLine($"{error.Code}: {error.Description}");

internal enum OrderKind { Personal, Business }
internal sealed record Address(string? City, string? PostalCode);
internal sealed record OrderLine(string? Sku, int Quantity);
internal sealed record CreateOrder(string? CustomerEmail, OrderKind Kind, string? CompanyName, Address? ShippingAddress, List<OrderLine>? Lines);

internal sealed class AddressValidator : Validator<Address>
{
    protected override ValueTask Configure(Address model, RuleContext<Address> rules, CancellationToken ct = default)
    {
        rules.For(() => model.City).NotEmpty();
        rules.For(() => model.PostalCode).Length(5, 5);
        return ValueTask.CompletedTask;
    }
}

internal sealed class CreateOrderValidator : Validator<CreateOrder>
{
    protected override async ValueTask Configure(CreateOrder model, RuleContext<CreateOrder> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerEmail).NotEmpty().EmailAddress();
        rules.For(() => model.Kind).IsDefinedEnum();
        rules.For(() => model.Lines).NotEmpty().MaxCount(50);

        if (model.Kind == OrderKind.Business)
            rules.For(() => model.CompanyName).NotEmpty().MaxLength(200);

        await rules.For(() => model.ShippingAddress)
            .Must(a => a is not null, "ShippingAddress.Required", "Shipping address is required.")
            .SetValidatorAsync(new AddressValidator(), ct);

        rules.ForEach(() => model.Lines, (line, lineRules) =>
        {
            lineRules.For(() => line.Sku).NotEmpty();
            lineRules.For(() => line.Quantity).GreaterThan(0);
        });
    }
}
```

Output:

```text
CustomerEmail.EmailAddress: 'CustomerEmail' is not a valid email address.
CompanyName.NotEmpty: 'CompanyName' must not be empty.
ShippingAddress.City.NotEmpty: 'ShippingAddress.City' must not be empty.
Lines[1].Sku.NotEmpty: 'Lines[1].Sku' must not be empty.
Lines[1].Quantity.GreaterThan: 'Lines[1].Quantity' must be greater than 0.
```

## Defining a validator

In the snippets below, `command` is a model instance, `ct` a `CancellationToken`, `services` an `IServiceCollection`, and `store` your own async service. Fragments that use `model` and `rules` run inside `Configure`.

```csharp
public sealed class CreateUserValidator : Validator<CreateUser>
{
    protected override ValueTask Configure(CreateUser model, RuleContext<CreateUser> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        rules.For(() => model.Name).NotEmpty().MaxLength(100);
        rules.For(() => model.Age).InclusiveBetween(18, 120);
        return ValueTask.CompletedTask;   // sync Configure must return this
    }
}

Result<CreateUser> result = await new CreateUserValidator().ValidateAsync(command, ct);
```

Inline, without a class:

```csharp
Result<CreateUser> r1 = await Validator.ValidateAsync(command, (m, rules) => rules.For(() => m.Name).NotEmpty());
Result<CreateUser> r2 = await Validator.ValidateAsync(command, async (m, rules, token) =>
    await rules.For(() => m.Email).MustAsync((e, t) => store.IsFreeAsync(e, t), "Email.Taken", "Email is taken.", token), ct);
```

## Built-in rules

All take an optional `message` and have an overload that takes an `Error` (custom code). Default code: `{Property}.{Rule}`.

```csharp
// string?: NotEmpty and NotNull fail on null; the rest skip null
rules.For(() => model.Name).NotEmpty().NotNull().MinLength(2).MaxLength(100).Length(2, 100)
    .Matches("^[A-Za-z ]+$").Contains("a").StartsWith("A").EndsWith("z")
    .Equal("Ada", StringComparison.OrdinalIgnoreCase).NotEqual("admin");
rules.For(() => model.Email).EmailAddress();

// IComparable<T> values
rules.For(() => model.Age).GreaterThan(0).GreaterThanOrEqualTo(18).LessThan(130).LessThanOrEqualTo(120)
    .InclusiveBetween(18, 120).ExclusiveBetween(0, 130).NotEqual(42);

// Nullable<T>: comparisons skip null; NotNull() and Null(); no Equal/NotEqual
rules.For(() => model.ExpiresAt).NotNull().GreaterThan(DateTime.UtcNow);

// IEnumerable reference types: NotEmpty and NotNull fail on null; counts skip null
rules.For(() => model.Tags).NotEmpty().MinCount(1).MaxCount(10).CountBetween(1, 10);

// Enums (TEnum and TEnum?)
rules.For(() => model.Role).IsDefinedEnum().IsOneOf(Role.User, Role.Admin).HasOnlyDefinedFlags();

// Custom code
rules.For(() => model.Name).NotEmpty(Error.Validation("User.NameRequired", "Name is required."));
```

There is no `NotNull()` for other reference types (`Address?`). Use `Must(a => a is not null, code, message)`.

## Cascade

```csharp
rules.For(() => model.Password)
    .Cascade(CascadeMode.Continue)   // default Stop: first failure ends the chain
    .MinLength(8)
    .Matches("[A-Z]", message: "Needs an uppercase letter.")
    .Matches("[0-9]", message: "Needs a digit.");
```

## Custom predicates

```csharp
rules.For(() => model.Name).Must(n => n != "admin", "Name.Reserved", "Name is reserved.");
rules.For(() => model.Age).Must(a => a % 2 == 0);   // code "Age.Must", message includes the lambda text

await rules.For(() => model.Email)
    .MustAsync((email, token) => store.IsFreeAsync(email, token), "Email.Taken", "Email is taken.", ct);
```

## Nested objects and collections

```csharp
await rules.For(() => model.Address).SetValidatorAsync(new AddressValidator(), ct);  // null skipped; codes "Address.City.NotEmpty"

rules.ForEach(() => model.Lines, (line, lineRules) =>
    lineRules.For(() => line.Sku).NotEmpty());                                         // "Lines[0].Sku.NotEmpty"

await rules.ForEachAsync(() => model.Lines, async (line, lineRules, token) =>
    await lineRules.For(() => line.Sku).MustAsync((s, t) => store.SkuExistsAsync(s, t), "Sku.Unknown", "Unknown SKU.", token), ct);
```

## Custom rules and manual errors

```csharp
public static class AgeRules
{
    public static RuleChain<T, int> Even<T>(this RuleChain<T, int> chain, string? message = null)
    {
        if (!chain.HasFailed && chain.Value % 2 != 0)
            chain.AddError($"{chain.PropertyName}.Even", message ?? $"'{chain.PropertyName}' must be even.");
        return chain;
    }
}

rules.AddFailure("Order.Total", "Total does not match the lines.");   // model-level, no field metadata
rules.For(model.Name, "Profile.DisplayName").MaxLength(50);          // value + explicit name, no lambda
```

## Composition, order and DI

```csharp
public sealed class PaidOrderValidator : Validator<Order>
{
    public override int Order => 1;   // Mediator ValidationBehavior runs lower Order groups first

    protected override async ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        await Include(new OrderValidator(), model, rules, ct);   // merge another validator's rules
        rules.For(() => model.PaymentReference).NotEmpty();
    }
}

services.AddValidator<CreateUser, CreateUserValidator>();                       // Scoped by default
services.AddValidator<Order, OrderValidator>(ServiceLifetime.Singleton);
services.AddValidatorsFromAssembly(typeof(CreateUserValidator).Assembly);      // reflection; not for trimmed/AOT apps
```

Registration uses `TryAddEnumerable`, so repeated calls do not duplicate a validator.

## Railway helpers

```csharp
Result<CreateUser> parsed = command;
Result<CreateUser> a = parsed.ValidateWith((m, rules) => rules.For(() => m.Name).NotEmpty());  // sync, Result<T> only
Result<CreateUser> b = await parsed.ValidateWithAsync(new CreateUserValidator());             // Result<T>, Task<Result<T>>, ValueTask<Result<T>>
```

A failed source result is returned unchanged.

## Rules

- Add `using CSharpEssentials.Validation.Validators;` or `NotEmpty()` and the other rules do not resolve.
- There is no sync `Validate()`. Call `ValidateAsync`. A validator with no async work completes synchronously.
- `MustAsync`, `SetValidatorAsync` and `ForEachAsync` return a `ValueTask`. Always `await` them, and mark `Configure` `async`. Do not chain sync rules after `MustAsync(...)` without awaiting.
- A sync `Configure` ends with `return ValueTask.CompletedTask;`.
- Use `rules.For(() => model.X)` with a lambda. `rules.For(model.X)` without a name does not compile. The two-argument form is `rules.For(value, "Name")`.
- Names are `MinLength`/`MaxLength` (not `MinimumLength`), `GreaterThanOrEqualTo`, `InclusiveBetween`, `MinCount`/`MaxCount`/`CountBetween`, `IsDefinedEnum`.
- No `When`/`Unless`/`RuleSet`/`WithMessage`/`WithErrorCode`. Use `if`, separate validators, `message:` and an `Error` argument.
- Built-in rules, `Must`, `MustAsync` and `AddError(code, message)` add `field` metadata. `AddFailure` and rules given an `Error` do not. `Error` equality compares metadata, so in tests compare `Code` and `Description`.
- Inside `if (model.X is not null)`, write `rules.For<string?>(() => model.X)`. Without the type argument the narrowed `string` chain gives warning CS8620 on string rules.
- `Matches(string)` caches each pattern forever. Pass fixed patterns or a `Regex`.
- `Validator<T>` with a scoped dependency (`DbContext`) must stay `Scoped`.
- With `CSharpEssentials.Mediator`, `services.AddMediatorValidationBehavior()` runs the registered validators before the handler. See the `csharpessentials-mediator` skill.

## FluentValidation mapping

| FluentValidation | CSharpEssentials.Validation |
|---|---|
| `AbstractValidator<T>`, constructor rules | `Validator<T>`, `Configure(model, rules, ct)` |
| `RuleFor(x => x.Name)` | `rules.For(() => model.Name)` |
| `When(...)` / `Unless(...)` | `if` |
| `RuleForEach(x => x.Tags)` | `rules.ForEach(() => model.Tags, (tag, r) => r.For(() => tag)...)` |
| `SetValidator(v)` | `await rules.For(() => model.Address).SetValidatorAsync(v, ct)` |
| `Include(v)` | `await Include(v, model, rules, ct)` |
| `MinimumLength` / `MaximumLength` | `MinLength` / `MaxLength` |
| `.WithMessage("...")` | `message: "..."` |
| `.WithErrorCode("X")` | `.NotEmpty(Error.Validation("X", "..."))` |
| `Validate(x).IsValid` | `(await ValidateAsync(x)).IsSuccess` |
| `AddValidatorsFromAssemblyContaining<T>()` | `AddValidatorsFromAssembly(typeof(T).Assembly)` |

## Diagnostics

This package reports no analyzer diagnostics.
