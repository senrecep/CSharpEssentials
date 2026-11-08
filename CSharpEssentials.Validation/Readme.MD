# CSharpEssentials.Validation

Model-first validation for C#. Rules are plain code that runs against the model on every call: no expression trees, no reflection on the validation path, no deferred rule building. Validators return `Result<T>`.

## Installation

```bash
dotnet add package CSharpEssentials.Validation
```

## Namespaces

| Namespace | Contents |
|-----------|----------|
| `CSharpEssentials.Validation` | `Validator<T>`, `Validator`, `IValidator<T>`, `RuleContext<T>`, `RuleChain<T, TProp>`, `CascadeMode`, the `ValidateWith` / `ValidateWithAsync` extensions on `Result<T>` |
| `CSharpEssentials.Validation.Validators` | The built-in rules (`NotEmpty`, `MaxLength`, `GreaterThan`, `SetValidatorAsync`, ...). Without this using, `rules.For(...).NotEmpty()` does not compile |
| `CSharpEssentials.Validation.Extensions` | `AddValidator`, `AddValidatorsFromAssembly`, `AddValidatorsFromAssemblies` |

`Result<T>` lives in `CSharpEssentials.ResultPattern` and `Error` in `CSharpEssentials.Errors`.

## Quick start

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

```csharp
var validator = new CreateUserCommandValidator();
Result<CreateUserCommand> result = await validator.ValidateAsync(command);

if (result.IsFailure)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.Code}: {error.Description}");
        // e.g. "Email.NotEmpty: 'Email' must not be empty."
}
```

A sync call site can use `validator.ValidateAsync(command).GetAwaiter().GetResult()` when the validator does no real async work (its `Configure` returns a completed `ValueTask`).

### Inline (static) usage

For one-off validations without a dedicated class, use the static `Validator.ValidateAsync`:

```csharp
// Sync delegate: returns a completed ValueTask (no Task; the rules allocate as in a validator class)
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, (m, rules) =>
{
    rules.For(() => m.Email).NotEmpty().EmailAddress();
    rules.For(() => m.Name).NotEmpty().MaxLength(100);
});

// Async delegate: for MustAsync or SetValidatorAsync inside
Result<CreateUserCommand> asyncResult = await Validator.ValidateAsync(command, async (m, rules, ct) =>
{
    rules.For(() => m.Name).NotEmpty();
    await rules.For(() => m.Email)
               .MustAsync(async (email, c) => await _db.IsUniqueAsync(email, c),
                          "Email.NotUnique", "Email is already taken.", ct);
}, cancellationToken);
```

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

### Comparable (`int`, `DateTime`, `decimal`, ...)

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

### Nullable structs (`int?`, `DateTime?`, ...)

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

`Configure` receives the model instance, so any C# control flow works inside it.

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

        // Early return: remaining rules skipped when Terms not accepted
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
`NotEmpty()` fire normally and null-tolerant validators (`MaxLength`, `EmailAddress`, ...) are skipped.
The same applies to `ForEach` / `ForEachAsync`: a null collection path produces no errors.
(Only paths whose target type can be null are tolerated; for non-nullable value types the
`NullReferenceException` still propagates.)

### ValueTask-based async predicates

`MustAsync` also accepts `ValueTask<bool>` predicates (extension overload in
`CSharpEssentials.Validation`), which avoids the `Task` allocation for ValueTask-based sources.
Async lambdas keep binding to the built-in `Task<bool>` overload, so call sites do not change.

### CSharpEssentials.Core integration

`CSharpEssentials.Core` comes in through `CSharpEssentials.Errors`. Its conditional helpers work inside validators.

`IfNotNull` runs rules only when a property has a value:

```csharp
// native C#:
if (model.Coupon is not null)
    rules.For(() => model.Coupon.Code).NotEmpty();

// with Core helper, one expression:
model.Coupon.IfNotNull(coupon =>
    rules.For(() => coupon.Code).NotEmpty());
// The property name comes from the lambda, so the error code is "Code.NotEmpty", not "Coupon.Code.NotEmpty"
```

`WhereIf` filters a collection before iterating:

```csharp
// Only validate active items
rules.ForEach(
    () => model.Items.WhereIf(model.ValidateActiveOnly, i => i.IsActive),
    (item, itemRules) => itemRules.For(() => item.Sku).NotEmpty());
```

The error code uses the collection name only, for example `Items[0].Sku.NotEmpty`; the filter call is not part of the path. The index counts the filtered sequence.

`WithoutNulls` skips null elements:

```csharp
rules.ForEach(
    () => model.Tags.WithoutNulls(),
    (tag, tagRules) => tagRules.For(() => tag).NotEmpty().MaxLength(50));
// Error codes look like "Tags[1].NotEmpty": the index counts the filtered sequence
```

`IsEmpty` / `IsNotEmpty` are null-or-empty guards:

```csharp
if (model.PromoCode.IsNotEmpty())
    rules.For(() => model.PromoCode).Matches(@"^PROMO-\d{4}$");
```

`IfTrue` / `IfFalse` run an action on a boolean:

```csharp
model.IsInternational.IfTrue(() =>
    rules.For(() => model.PassportNumber).NotEmpty());
```

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
    public override int Order => 0;  // default, runs first

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

If a validator throws a non-`OperationCanceledException` exception, `ValidationBehavior` catches it and converts it to a `Result.Failure` error (code `"Validator.Exception"`). `OperationCanceledException` always propagates.

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

## Railway integration

`ValidateWith` and `ValidateWithAsync` plug a validator into a `Result<T>` pipeline. A failed source result skips the validator and passes its errors on; a successful one is validated.

```csharp
// Synchronous inline delegate: the pipeline stays synchronous
Result<User> user = GetUserCommand() // Result<CreateUserCommand>
    .ValidateWith((cmd, rules) =>
    {
        rules.For(() => cmd.Email).NotEmpty().EmailAddress();
        rules.For(() => cmd.Age).GreaterThan(18);
    })
    .Map(cmd => new User(cmd));

// Class-based validator in an async pipeline
Result<User> created = await GetUserCommand()
    .ValidateWithAsync(new CreateUserCommandValidator(), ct)
    .BindAsync(async cmd => await _userService.CreateUserAsync(cmd)); // ValueTask chain: await the Task-returning call

// Asynchronous inline delegate (e.g. a database unique check)
Result<User> unique = await GetUserCommand()
    .ValidateWithAsync(async (cmd, rules, c) =>
    {
        rules.For(() => cmd.Email).NotEmpty();
        await rules.For(() => cmd.Username)
                   .MustAsync(async (name, token) => await _db.IsUniqueAsync(name, token),
                              "Username.Taken", "Username is already taken.", c);
    }, ct)
    .BindAsync(async cmd => await _userService.CreateUserAsync(cmd));

// Works on Task<Result<T>> and ValueTask<Result<T>> too
Result<Order> order = await GetOrderAsync(id).ValidateWithAsync(new OrderValidator(), ct);
```

| Overload | Source type |
|----------|-------------|
| `result.ValidateWith(configure)` | `Result<T>` → `Result<T>` (sync) |
| `result.ValidateWithAsync(validator, ct)` | `Result<T>` → `ValueTask<Result<T>>` |
| `result.ValidateWithAsync(configure)` | `Result<T>` → `ValueTask<Result<T>>` (sync delegate) |
| `result.ValidateWithAsync(asyncConfigure, ct)` | `Result<T>` → `ValueTask<Result<T>>` (`(model, rules, ct) => ValueTask` delegate) |
| `resultTask.ValidateWithAsync(validator \| configure \| asyncConfigure, ct)` | `Task<Result<T>>` → `ValueTask<Result<T>>` |
| `resultValueTask.ValidateWithAsync(validator \| configure \| asyncConfigure, ct)` | `ValueTask<Result<T>>` → `ValueTask<Result<T>>` |

## Comparison with FluentValidation

Benchmarks against FluentValidation 12.2.0, DataAnnotations and Microsoft.Extensions.Validation (validation path, enum validators, the `For` overloads, construction and the assembly scan) are in [BENCHMARKS.md](./BENCHMARKS.md), losses included.

| | CSharpEssentials.Validation | FluentValidation |
|---|---|---|
| Rule definition | `ValueTask Configure(model, rules, ct)` runs on every call with the live instance; sync validators `return ValueTask.CompletedTask` | Constructor: `RuleFor(x => x.Name)` stores expressions once |
| Conditional rules | Plain `if` / `else` / `switch` | `When`, `Unless`, `WhenAsync`, `UnlessAsync` |
| Property name | `CallerArgumentExpression`, at compile time | `Expression<Func<T, TProperty>>`, read when the rules are built |
| Reflection | None on the validation path | Expression trees, compiled to delegates at construction |
| Trimming / Native AOT | Validation path has no expression trees or reflection; `AddValidatorsFromAssembly` scans by reflection, so use `AddValidator` | Not annotated for trimming |
| Async rules | `MustAsync`, `ForEachAsync`, `SetValidatorAsync`; `ValidateAsync` returns `ValueTask<Result<T>>` | `MustAsync`, `WhenAsync`; `ValidateAsync` returns `Task<ValidationResult>` |
| Result | `Result<T>` with structured `Error` (code, description, type, metadata) | `ValidationResult` of `ValidationFailure` (PropertyName, ErrorMessage, ...); mapping to `Result<T>` is manual |
| DI lifetime | Scoped by default; singleton when the validator has no scoped dependencies | Typically singleton |
| Rule sets, message API | None; branch in code and pass `message` or an `Error` | `RuleSet`, `WithMessage`, `WithName`, `WithErrorCode`, global cascade settings |

The cost of running `Configure` per call is a new `RuleContext<T>` per `ValidateAsync`; `Validator<T>` instances hold no state. FluentValidation builds its rules once per instance, so its construction is expensive and its validate path cheaper in allocation on some valid inputs (see BENCHMARKS.md).

## Dependencies

- `CSharpEssentials.Results` (brings `CSharpEssentials.Errors`, `CSharpEssentials.Json`, and through Errors `CSharpEssentials.Core` and `CSharpEssentials.Enums`)
- `Microsoft.Extensions.DependencyInjection.Abstractions`

## Upgrading to 8.0

- A `!` or `?` before an indexer is left out of the property name and error code, as it already was before a `.`. `rules.For(() => model.Tags![0])` and `rules.For(() => model.Tags?[0])` now report `Tags[0]` instead of `Tags![0]` or `Tags?[0]`, and `model.Map!["k"].City` reports `Map["k"].City`. Update any check that matched the old codes.
- `ValidateWithAsync` on `Task<Result<T>>` and `ValueTask<Result<T>>` waits on the task through Core `WithCancellation`, which is `Task.WaitAsync` on the .NET 9, 10 and 11 targets. When the token is cancelled while it waits, it throws `TaskCanceledException`, a subclass of `OperationCanceledException`, instead of `OperationCanceledException` itself. A `catch (OperationCanceledException)` still catches it; an exact type check no longer matches. The `netstandard2.1` asset is unchanged.
