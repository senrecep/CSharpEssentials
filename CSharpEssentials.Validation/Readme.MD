# CSharpEssentials.Validation

High-performance, model-first validation library for C#. It uses no reflection and evaluates rules eagerly, with no expression trees and no deferred builds. Validators return `Result<T>` directly.

## Installation

```bash
dotnet add package CSharpEssentials.Validation
```

## Upgrading to 8.0

- A `!` or `?` before an indexer is left out of the property name and error code, as it already was before a `.`. `rules.For(() => model.Tags![0])` and `rules.For(() => model.Tags?[0])` now report `Tags[0]` instead of `Tags![0]` or `Tags?[0]`, and `model.Map!["k"].City` reports `Map["k"].City`. Update any check that matched the old codes.

## Namespaces

| Namespace | Contents |
|-----------|----------|
| `CSharpEssentials.Validation` | `Validator<T>`, `Validator`, `IValidator<T>`, `RuleContext<T>`, `RuleChain<T, TProp>`, `CascadeMode`, the `ValidateWith` / `ValidateWithAsync` extensions on `Result<T>` |
| `CSharpEssentials.Validation.Validators` | The built-in rules (`NotEmpty`, `MaxLength`, `GreaterThan`, `SetValidatorAsync`, ...). Without this using, `rules.For(...).NotEmpty()` does not compile |
| `CSharpEssentials.Validation.Extensions` | `AddValidator`, `AddValidatorsFromAssembly`, `AddValidatorsFromAssemblies` |

`Result<T>` lives in `CSharpEssentials.ResultPattern` and `Error` in `CSharpEssentials.Errors`.

## Quick start

Define a validator by extending `Validator<T>`:

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

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

> **Sync call site:** `validator.ValidateAsync(command).GetAwaiter().GetResult()` is safe when the validator has no actual async operations (returns a pre-completed `ValueTask`).

### Inline (static) usage

For one-off validations without a dedicated class, use the static `Validator.ValidateAsync`:

```csharp
// Sync delegate — zero heap allocation
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, (m, rules) =>
{
    rules.For(() => m.Email).NotEmpty().EmailAddress();
    rules.For(() => m.Name).NotEmpty().MaxLength(100);
});

// Async delegate — for MustAsync or SetValidatorAsync inside
Result<CreateUserCommand> asyncResult = await Validator.ValidateAsync(command, async (m, rules, ct) =>
{
    rules.For(() => m.Name).NotEmpty();
    await rules.For(() => m.Email)
               .MustAsync(async (email, c) => await _db.IsUniqueAsync(email, c),
                          "Email.NotUnique", "Email is already taken.", ct);
}, cancellationToken);
```

`Validator.ValidateAsync` and `Validator<T>` live in the same file (`Validator.cs`) as separate types: one static utility class and one abstract base class. They are independent; the static form does not use `Validator<T>` internally.

## Built-in validators

### String

| Validator | Null | Description |
|-----------|------|-------------|
| `NotEmpty()` | FAIL | Value must not be null, empty, or whitespace |
| `NotNull()` | FAIL | Value must not be null |
| `MaxLength(n)` | skip | Length ≤ n |
| `MinLength(n)` | skip | Length ≥ n |
| `Length(min, max)` | skip | Length in [min, max] |
| `EmailAddress()` | skip | Valid e-mail format |
| `Matches(pattern)` / `Matches(Regex)` | skip | Matches the regex; a match timeout counts as a failure |
| `Contains(sub)` | skip | Contains substring |
| `StartsWith(prefix)` | skip | Starts with prefix |
| `EndsWith(suffix)` | skip | Ends with suffix |
| `Equal(expected)` | FAIL | Ordinal equality; an overload takes a `StringComparison` |
| `NotEqual(forbidden)` | skip | Not ordinally equal to `forbidden`; an overload takes a `StringComparison` |

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

### Nullable structs (`int?`, `DateTime?`, …)

`GreaterThan`, `GreaterThanOrEqualTo`, `LessThan`, `LessThanOrEqualTo`, `InclusiveBetween` and `ExclusiveBetween` are available for nullable value types; `null` is skipped. `NotNull()` fails on `null` and `Null()` fails on a value. `Equal` and `NotEqual` are not available for nullable value types.

Every built-in rule except `Must`, `MustAsync` and `SetValidatorAsync` takes an optional `message` and has an overload that takes a full `Error`, for a custom code: `.NotEmpty(Error.Validation("User.NameRequired", "Name is required."))`.

### Enums (`TEnum`, `TEnum?`)

| Validator | Null | Error code | Description |
|-----------|------|------------|-------------|
| `IsDefinedEnum()` | skip | `{Prop}.IsDefinedEnum` | Value is a defined member; a `[Flags]` enum also accepts any combination of defined flags |
| `IsOneOf(params TEnum[])` | skip | `{Prop}.IsOneOf` | Value equals one of the given members (exact match, flags are not decomposed) |
| `HasOnlyDefinedFlags()` | skip | `{Prop}.HasOnlyDefinedFlags` | Every set bit belongs to a defined member; zero passes |

The description is the `EnumValueError.Message` text that binding and JSON errors use. The examples use `[StringEnum] enum OrderStatus { Pending, InProgress, Approved, Rejected }`:

```csharp
rules.For(() => order.Status).IsDefinedEnum();
// Status.IsDefinedEnum: '42' is not a valid OrderStatus. Allowed values: pending, in_progress, approved, rejected.

rules.For(() => command.Status).IsOneOf(OrderStatus.Approved, OrderStatus.Rejected);
// Status.IsOneOf: 'pending' is not a valid OrderStatus. Allowed values: approved, rejected.
```

- `[StringEnum]` enums use their generated metadata: values are written as wire names (flags as `a,b`), and `IsDefinedEnum`/`HasOnlyDefinedFlags` list the input wire names (the `[EnumFallback]` member is defined but not listed, as in binding errors).
- Enums without metadata never throw: `IsDefinedEnum` follows `Enum.IsDefined` (plus defined flag combinations for `[Flags]` enums), and errors list the C# member names.
- An undefined value is written as its number. Each rule has a `message` overload (code kept) and an `Error` overload.

## CascadeMode

By default (`Stop`), the first failure on a chain stops subsequent rules. Use `Continue` to accumulate all errors for a property:

```csharp
rules.For(() => model.Password)
     .Cascade(CascadeMode.Continue)
     .MinLength(8)
     .Matches(@"[A-Z]", message: "Must contain an uppercase letter.");
```

Call `Cascade` before the rules it should affect. Once a rule on the chain has failed, `Cascade` is a no-op, so a `NotEmpty()` guard that fails stays the only error.

## Custom predicates

```csharp
// Sync
rules.For(() => model.Name)
     .Must(name => name != "admin", "Name.Reserved", "The name 'admin' is reserved.");

// Async
await rules.For(() => model.Email)
           .MustAsync(async (email, ct) => await _db.IsUniqueAsync(email, ct),
                      "Email.NotUnique", "Email is already taken.");
```

## Nested validation

```csharp
await rules.For(() => model.Address!).SetValidatorAsync(new AddressValidator(), ct);
// Error codes are prefixed: "Address.City.NotEmpty", "Address.ZipCode.Matches"
// Note: must be inside an async Configure to use await.
```

## Collection validation

```csharp
rules.ForEach(() => model.Tags, (tag, tagRules) =>
{
    tagRules.For(() => tag.Name).NotEmpty().MaxLength(50);
});
// Error codes: "Tags[0].Name.NotEmpty", "Tags[1].Name.MaxLength"

// Validating the element itself: the code has no property segment
rules.ForEach(() => model.Names, (name, nameRules) => nameRules.For(() => name).NotEmpty());
// Error code: "Names[1].NotEmpty"

// Async items
await rules.ForEachAsync(() => model.Items, async (item, itemRules, c) =>
{
    itemRules.For(() => item.Sku).NotEmpty();
    await itemRules.For(() => item.Sku)
                   .MustAsync(async (sku, token) => await _db.SkuExistsAsync(sku, token), "Sku.NotFound", "SKU not found.", c);
}, ct);
```

A custom code passed to `Must`/`MustAsync` is used as given; only the built-in codes get the property path.

## Native C# control flow

`Configure` receives the fully resolved model instance, so any C# control flow works inside it without a DSL or special API.

### Conditional rules

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

### Null-guard before nested validation

```csharp
if (model.ShippingAddress is not null)
    await rules.For(() => model.ShippingAddress!).SetValidatorAsync(new AddressValidator(), ct);
```

### Null-tolerant member paths

A `null` intermediate segment in a member path is tolerated: `rules.For(() => model.Address.City)`
with a `null` `Address` does not throw. The property is treated as `null`, so guards like
`NotEmpty()` fire normally and null-tolerant validators (`MaxLength`, `EmailAddress`, …) are skipped.
The same applies to `ForEach` / `ForEachAsync`: a null collection path produces no errors.
(Only paths whose target type can be null are tolerated; for non-nullable value types the
`NullReferenceException` still propagates.)

### ValueTask-based async predicates

`MustAsync` also accepts `ValueTask<bool>` predicates (extension overload in
`CSharpEssentials.Validation`), which avoids the `Task` allocation for ValueTask-based sources.
Async lambdas keep binding to the built-in `Task<bool>` overload, so call sites do not change.

### CSharpEssentials.Core integration

`CSharpEssentials.Core` is transitively available (via `CSharpEssentials.Errors`) and has conditional helpers that work inside validators.

**`IfNotNull`: run rules only when a property has a value.**

```csharp
// native C#:
if (model.Coupon is not null)
    rules.For(() => model.Coupon.Code).NotEmpty();

// with Core helper, one expression:
model.Coupon.IfNotNull(coupon =>
    rules.For(() => coupon.Code).NotEmpty());
// The property name comes from the lambda, so the error code is "Code.NotEmpty", not "Coupon.Code.NotEmpty"
```

**`WhereIf`: filter a collection before iterating.**

```csharp
// Only validate active items
rules.ForEach(
    () => model.Items.WhereIf(model.ValidateActiveOnly, i => i.IsActive),
    (item, itemRules) => itemRules.For(() => item.Sku).NotEmpty());
```

The error code uses the collection name only, for example `Items[0].Sku.NotEmpty`; the filter call is not part of the path. The index counts the filtered sequence.

**`WithoutNulls`: skip null elements in a collection.**

```csharp
rules.ForEach(
    () => model.Tags.WithoutNulls(),
    (tag, tagRules) => tagRules.For(() => tag).NotEmpty().MaxLength(50));
// Error codes look like "Tags[1].NotEmpty": the index counts the filtered sequence
```

**`IsEmpty` / `IsNotEmpty`: readable null/empty guards.**

```csharp
if (model.PromoCode.IsNotEmpty())
    rules.For(() => model.PromoCode).Matches(@"^PROMO-\d{4}$");
```

**`IfTrue` / `IfFalse`: boolean action helpers.**

```csharp
model.IsInternational.IfTrue(() =>
    rules.For(() => model.PassportNumber).NotEmpty());
```

> Install `CSharpEssentials.Core` separately if you are not using the meta-package.

## Multiple validators for one model

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

### Multiple DI validators

When several `IValidator<Order>` implementations are registered, the `ValidationBehavior` in `CSharpEssentials.Mediator` aggregates the results of all of them and removes identical errors.

```csharp
services.AddValidator<Order, BaseOrderValidator>();
services.AddValidator<Order, PaidOrderValidator>();
// Both validators run; errors are merged and deduplicated
```

## Validator ordering

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

public class BusinessRulesValidator(IReadOnlySet<string> blocklist) : Validator<CreateUserCommand>
{
    public override int Order => 1;  // runs after Order=0 group completes

    protected override ValueTask Configure(CreateUserCommand model, RuleContext<CreateUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).Must(email => !blocklist.Contains(email!), "Email.Blocked", "Email is blocked.");
        return ValueTask.CompletedTask;
    }
}
```

Execution rules:
- Validators with the same `Order` run concurrently within their group.
- Groups with a lower `Order` run sequentially before groups with a higher `Order`.
- All groups execute regardless of earlier failures. Errors from all groups are accumulated and deduplicated.
- Default `Order` is `0`, so all validators without an override run concurrently in one group.

## DI registration

```csharp
using CSharpEssentials.Validation.Extensions;

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

`AddValidatorsFromAssembly` / `AddValidatorsFromAssemblies` discover validators by reflection and are marked `[RequiresUnreferencedCode]`, so the trim analyzer flags their call sites. `AddValidator<T, TValidator>()` keeps the validator constructor through trimming and is safe in trimmed and Native AOT apps.

Registration uses `TryAddEnumerable`, so registering the same validator twice (for example, `AddValidator` and an assembly scan) adds it once.

Validators with no scoped dependencies may be registered as `Singleton`. Validators that inject scoped services (e.g. `DbContext`, `ICurrentUser`) must be `Scoped`.

## Mediator pipeline integration

With `CSharpEssentials.Mediator`, requests are validated automatically before handlers run:

```csharp
services.AddMediatorValidationBehavior();
// or
services.AddMediatorBehaviors(); // registers all behaviors
```

`ValidationBehavior` is registered as `Scoped`. It runs all registered `IValidator<TRequest>` implementations, groups them by `Order`, and returns `Result.Failure` if any errors are found for a `Result` / `Result<T>` handler (any other response type gets an `EnhancedValidationException`). The handler is never invoked. See the [Mediator readme](../CSharpEssentials.Mediator/Readme.MD) for the validation modes and failure observers.

**Exception isolation:** If a validator throws a non-`OperationCanceledException` exception, `ValidationBehavior` catches it and converts it to a `Result.Failure` error (code `"Validator.Exception"`). `OperationCanceledException` always propagates; cancellation is never swallowed.

## Custom validators and manual failures

A custom rule is an extension method on `RuleChain<T, TProp>`. Check `HasFailed` first and report with `AddError`:

```csharp
using CSharpEssentials.Errors;
using CSharpEssentials.Validation;

public static class EvenValidators
{
    public static RuleChain<T, int> Even<T>(this RuleChain<T, int> chain, string? message = null)
    {
        if (!chain.HasFailed && chain.Value % 2 != 0)
            chain.AddError(Error.Validation($"{chain.PropertyName}.Even", message ?? $"'{chain.PropertyName}' must be even."));
        return chain;
    }
}

rules.For(() => model.Quantity).GreaterThan(0).Even();
```

`RuleContext<T>` also accepts failures that belong to no single property, and a pre-evaluated value:

```csharp
rules.AddFailure("Order.Total", "The total does not match the line items."); // or AddFailure(Error)
rules.For(model.Name, nameof(model.Name)).NotEmpty(); // no lambda, no path parsing: for hot paths
```

## Railway validation bindings

`ValidateWith` (synchronous) and `ValidateWithAsync` (asynchronous) add model validation to railway-oriented programming (ROP) pipelines.

These extension methods are available on `Result<T>`, `Task<Result<T>>`, and `ValueTask<Result<T>>`. If the source result is a failure, they short-circuit without running the validator and pass the errors down the chain. If it is a success, the validator runs against the value.

### 1. Purely synchronous pipeline (`ValidateWith`)

`ValidateWith` with a synchronous inline delegate validates without an asynchronous `ValueTask` or task context, so the railway pipeline stays fully synchronous:

```csharp
Result<User> result = GetUserCommand() // returns Result<CreateUserCommand>
    .ValidateWith((cmd, rules) =>
    {
        rules.For(() => cmd.Email).NotEmpty().EmailAddress();
        rules.For(() => cmd.Age).GreaterThan(18);
    })
    .Map(cmd => new User(cmd)); // returns Result<User> synchronously
```

### 2. Class-based validator (`ValidateWithAsync`)

Pass a concrete `IValidator<T>` instance to run validations as part of an asynchronous pipeline:

```csharp
Result<User> result = await GetUserCommand() // returns Result<CreateUserCommand>
    .ValidateWithAsync(new CreateUserCommandValidator())
    .BindAsync(async cmd => await _userService.CreateUserAsync(cmd)); // ValueTask chain: await the Task-returning call
```

### 3. Inline delegate validation (`ValidateWithAsync`)

For one-off validations inside an asynchronous pipeline, pass a synchronous or asynchronous inline delegate:

```csharp
// Synchronous inline delegate
Result<User> result = await GetUserCommand()
    .ValidateWithAsync((cmd, rules) =>
    {
        rules.For(() => cmd.Email).NotEmpty().EmailAddress();
    })
    .BindAsync(async cmd => await _userService.CreateUserAsync(cmd)); // ValueTask chain: await the Task-returning call

// Asynchronous inline delegate (e.g. database unique check)
Result<User> asyncResult = await GetUserCommand()
    .ValidateWithAsync(async (cmd, rules, ct) =>
    {
        rules.For(() => cmd.Email).NotEmpty();
        await rules.For(() => cmd.Username)
                   .MustAsync(async (name, c) => await _db.IsUniqueAsync(name, c), 
                              "Username.Taken", "Username is already taken.", ct);
    })
    .BindAsync(async cmd => await _userService.CreateUserAsync(cmd)); // ValueTask chain: await the Task-returning call
```

## FluentValidation comparison

> **Benchmarks:** the 23 FluentValidation scenarios (construction, invalid and valid paths, collections, async,
> nested, cascade), plus baselines against FluentValidation, DataAnnotations and Microsoft.Extensions.Validation,
> the enum validators, the `For` overloads and the assembly scan, are described in [BENCHMARKS.md](./BENCHMARKS.md).
> Numbers are pending a re-run on FluentValidation 12.2.0.

| | CSharpEssentials.Validation | FluentValidation |
|---|---|---|
| **Rule definition** | `ValueTask Configure(model, rules, ct)`: model is the live instance; sync validators `return ValueTask.CompletedTask` | Constructor: `RuleFor(x => x.Name)` stores expressions |
| **Conditional rules** | Native `if`/`else`/`switch`, no DSL | `.When(x => ..., () => { ... })` block or per-rule `.When()` chain |
| **Conditional style** | Standard C#, no extra API to learn | Dedicated `When`, `Unless`, `WhenAsync`, `UnlessAsync` methods |
| **Property name** | Extracted from `CallerArgumentExpression` at compile time | Extracted from `Expression<Func<T,TProperty>>` at compile time |
| **Reflection** | None: no expression trees, no reflection at runtime | Expression trees at rule-definition time; compiled to delegates |
| **AOT / NativeAOT** | No expression trees or reflection on the validation path (`AddValidatorsFromAssembly` scans assemblies by reflection; use `AddValidator` in trimmed or AOT apps) | Expression tree compilation can cause issues |
| **Async rules** | `MustAsync`, `ForEachAsync`, `SetValidatorAsync`; `ValidateAsync` returns `ValueTask<Result<T>>` | `MustAsync`, `WhenAsync`; `ValidateAsync` returns `Task<ValidationResult>` |
| **Result type** | Returns `Result<T>` natively (railway-oriented) | Returns `ValidationResult`; separate mapping needed |
| **Error type** | Structured `Error` (code, description, type, metadata) | `ValidationFailure` (PropertyName, ErrorMessage, …) |
| **DI lifetime** | Scoped by default; `Singleton`-safe | Typically `Singleton` (stateless by design) |

### Why we chose this approach

FluentValidation's constructor-based design forces rules to be registered without a model instance. As a result:

- You cannot write `if (model.X) { ... }` in the constructor, because the model doesn't exist yet.
- The `When()` DSL exists only to work around this limitation.
- Every developer must learn an extra API layer that reimplements what C# `if` already provides.

CSharpEssentials.Validation passes `model` directly into `Configure`, so it needs no `When`/`Unless` at all. Branching is plain C# that anyone can read, step through with standard tooling and refactor.

The trade-off is a new `RuleContext<T>` per `ValidateAsync` call. That is intentional: the context is a small, short-lived object, and it keeps the design thread-safe and its allocations predictable. `Validator<T>` instances themselves are stateless and can be registered as `Singleton` when they have no scoped dependencies. Validators that inject scoped services (e.g. `DbContext`) should be registered as `Scoped`.

### FluentValidation advantages

- Mature ecosystem, wide community adoption
- Rich `.WithMessage`, `.WithName`, `.WithErrorCode` fluent API
- Built-in rule sets (`RuleSet`) for named validation scenarios
- `.Cascade(CascadeMode.StopOnFirstFailure)` configurable globally

### FluentValidation disadvantages

- Cannot use plain `if` for conditional rules without DSL
- `ValidationResult` is not railway-oriented and requires manual `Result<T>` mapping
- Expression trees limit Native AOT compatibility
- `When()` blocks create implicit coupling between rule registration and condition evaluation

## Railway integration

`ValidateWith` / `ValidateWithAsync` plug validators directly into `Result<T>` pipelines. If the result is already a failure, the validator is skipped entirely.

```csharp
// Named validator in a pipeline
Result<CreateUserCommand> result = await ParseCommand(input)
    .ValidateWithAsync(new CreateUserCommandValidator(), ct);

// Inline validation — no dedicated class
Result<CreateUserCommand> inlineResult = await ParseCommand(input)
    .ValidateWithAsync((m, rules) =>
    {
        rules.For(() => m.Email).NotEmpty().EmailAddress();
        rules.For(() => m.Name).NotEmpty().MaxLength(100);
    });

// Works on Task<Result<T>> and ValueTask<Result<T>>
Result<Order> order = await GetOrderAsync(id)
    .ValidateWithAsync(new OrderValidator(), ct);
```

| Overload | Source type |
|----------|-------------|
| `result.ValidateWith(configure)` | `Result<T>` → `Result<T>` (sync) |
| `result.ValidateWithAsync(validator, ct)` | `Result<T>` → `ValueTask<Result<T>>` |
| `result.ValidateWithAsync(configure)` | `Result<T>` → `ValueTask<Result<T>>` (sync delegate) |
| `result.ValidateWithAsync(asyncConfigure, ct)` | `Result<T>` → `ValueTask<Result<T>>` (`(model, rules, ct) => ValueTask` delegate) |
| `resultTask.ValidateWithAsync(validator \| configure \| asyncConfigure, ct)` | `Task<Result<T>>` → `ValueTask<Result<T>>` |
| `resultValueTask.ValidateWithAsync(validator \| configure \| asyncConfigure, ct)` | `ValueTask<Result<T>>` → `ValueTask<Result<T>>` |

## Dependencies

- `CSharpEssentials.Results` (brings `CSharpEssentials.Errors`, `CSharpEssentials.Json`, and through Errors `CSharpEssentials.Core` and `CSharpEssentials.Enums`)
- `Microsoft.Extensions.DependencyInjection.Abstractions`

## Upgrading to 8.0

- `ValidateWithAsync` on `Task<Result<T>>` and `ValueTask<Result<T>>` waits on the task through Core `WithCancellation`, which is `Task.WaitAsync` on the .NET 9, 10 and 11 targets. When the token is cancelled while it waits, it throws `TaskCanceledException`, a subclass of `OperationCanceledException`, instead of `OperationCanceledException` itself. A `catch (OperationCanceledException)` still catches it; an exact type check no longer matches. The `netstandard2.1` asset is unchanged.
