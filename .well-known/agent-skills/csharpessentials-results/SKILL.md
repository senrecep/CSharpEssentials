---
name: csharpessentials-results
description: Use when handling operation outcomes without exceptions — Result and Result<T> for success/failure, railway-oriented chaining with Then/ThenAsync/Ensure, Match for consumption, and Result.And/Or for combining multiple results.
---

# CSharpEssentials.Results

`Result` and `Result<T>` model operation outcomes as values. No exceptions for control flow.

## Installation

```bash
dotnet add package CSharpEssentials.Results
```

## Namespace

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;
```

## Creating Results

```csharp
// Success
Result ok      = Result.Success();
Result<int> v  = Result.Success(42);

// Failure — explicit factory
Result fail    = Result.Failure(Error.Validation("Input.Invalid", "Input was invalid."));
Result<string> fail1 = Result.Failure<string>(Error.NotFound("User.NotFound", "User not found."));
Result<string> fail2 = Result<string>.Failure(Error.Conflict("User.Duplicate", "Duplicate."));

// Multiple errors in one failure
Result<int> multi = Result.Failure<int>(
    Error.Validation("Name.Empty", "Name is required."),
    Error.Validation("Email.Invalid", "Email is invalid."));

// Implicit conversions — shorthand
Result<User> r = user;                        // T → Result<T>
Result<User> r = Error.NotFound("...", "..."); // Error → Result<T>
```

## Checking the Result

```csharp
if (result.IsFailure)
    return result.FirstError;  // Error (first in list)

if (result.IsSuccess)
    return result.Value;       // T — safe only after IsSuccess check
```

## Chaining — railway-oriented

```csharp
// Then: transform value, short-circuits on failure
Result<int> result = Parse("5")
    .Then(n => n * 2)
    .Then(n => n + 10);

// ThenAsync: async chain
Result<OrderConfirmation> placed = await GetUserAsync(id)
    .ThenAsync(user => ValidateOrderAsync(user, order))
    .ThenAsync(order => ChargePaymentAsync(order));

// Ensure: guard condition — adds error if predicate fails
Result<int> ensured = Result.Success(50)
    .Ensure(v => v > 0,   Error.Validation("Range", "Must be positive."))
    .Ensure(v => v < 100, Error.Validation("Range", "Must be less than 100."));

// EnsureAsync
Result<User> validated = await GetUserAsync(id)
    .EnsureAsync(u => IsActiveAsync(u), Error.Validation("User.Inactive", "Account is inactive."));
```

## Consuming — Match

```csharp
string msg = result.Match(
    onSuccess: value  => $"OK: {value}",
    onError:   errors => $"Failed: {errors[0].Description}");  // errors is Error[]

// Async match
await result.MatchAsync(
    onSuccess: async value  => await SendConfirmationAsync(value),
    onError:   async errors => await LogErrorsAsync(errors));
```

## Combining Results

```csharp
// And — all must pass (short-circuits on first failure)
Result combined = Result.And(r1, r2, r3);

// Or — first success wins
Result any = Result.Or(r1, r2, r3);
```

## Safe Execution

```csharp
// Wrap exception → Result
Result<int> safe = Result.Try(() => int.Parse(input), ex => Error.Exception(ex));

// Async
Result<Data> data = await Result.TryAsync(() => _db.GetAsync(id), ex => Error.Exception(ex));
```

## Best Practices

- Never access `.Value` without checking `.IsSuccess` first
- `onError` in `Match` receives `Error[]` (array) — not a single `Error`
- `Then()` short-circuits: once a failure occurs, subsequent `Then()` calls are skipped
- Prefer `Result.Failure<T>` over the implicit `Error → Result<T>` conversion when self-documentation matters
- Use `Ensure()` to add guard conditions without breaking the chain
