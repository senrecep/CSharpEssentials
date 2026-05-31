# CSharpEssentials.Results

Functional Result pattern for explicit success/failure handling in .NET. No exceptions for control flow.

## Features

- **Result & Result<T>** — Value-type results with implicit conversions from values and errors.
- **Railway Oriented Programming** — Chain operations with `Then`, `Bind`, `Map`, `Ensure`.
- **Error Aggregation** — Collect multiple validation errors in one result.
- **Async Support** — Full async/await across all extension methods.
- **Safe Execution** — `TryCatch` to wrap exceptions into results.

## Installation

```bash
dotnet add package CSharpEssentials.Results
```

## Usage

### Creating Results

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;

public Result<int> GetUser(int id)
{
    if (id <= 0)
        return Error.Validation("Id.Invalid", "ID must be positive.");

    return 42; // implicit conversion to Result<int>
}

public Result DeleteUser(int id)
{
    if (id <= 0)
        return Error.Validation("Id.Invalid", "ID must be positive.");

    return Result.Success();
}
```

### Returning Failures

Use `Result.Failure` or `Result<TValue>.Failure` to explicitly construct a failed result without throwing an exception:

```csharp
// Non-generic failure
Result fail = Result.Failure(Error.Validation("Input.Invalid", "Input was invalid."));

// Generic failure — no value, just an error
Result<string> fail1 = Result.Failure<string>(Error.NotFound("User.NotFound", "User not found."));
Result<string> fail2 = Result<string>.Failure(Error.Conflict("User.Duplicate", "User already exists."));

// Multiple errors in one failure
Result<int> multiError = Result.Failure<int>(
    Error.Validation("Name.Empty", "Name is required."),
    Error.Validation("Email.Invalid", "Email is invalid."));

// Implicit conversion from Error (shorthand)
Result<string> implicit1 = Error.Unauthorized("Token.Expired", "Token has expired.");
```

All three styles are equivalent — choose based on readability preference. `Result.Failure<T>` is the most explicit and self-documenting.

### Chaining

```csharp
Result<int> Parse(string input)
{
    if (int.TryParse(input, out int n))
        return n;
    return Error.Validation("Parse", "Not a number.");
}

Result<int> result = Parse("5")
    .Then(n => n * 2)
    .Then(n => n + 10);
```

### Ensuring

```csharp
Result<int> ensured = Result.Success(50)
    .Ensure(v => v > 0, Error.Validation("Range", "Must be positive."))
    .Ensure(v => v < 100, Error.Validation("Range", "Must be less than 100."));
```

### Matching

```csharp
string message = GetUser(10).Match(
    onSuccess: user => $"User: {user}",
    onError: errors => $"Failed: {errors[0].Description}");
```

### Combining Results

```csharp
var r1 = Result.Success();
var r2 = Result.Success();
var r3 = Result.Failure(Error.NotFound());

Result combined = Result.And(r1, r2, r3); // Failure
```

### Safe Execution

```csharp
Result<int> result = Result.Try(() => 10 / 0, ex => Error.Exception(ex));
```

### Async

```csharp
Result<int> result = await FetchUserAsync(id)
    .ThenAsync(user => EnrichAsync(user))
    .EnsureAsync(v => IsValidAsync(v), Error.Validation("Invalid", "Not valid."));
```

### Collection Extensions

Batch operations on sequences of results:

```csharp
// Collect ALL errors from multiple results
Result validation = new[] { ValidateName(input), ValidateEmail(input), ValidateAge(input) }
    .CombineAll();

// Map each item and collect — success array or all errors
Result<OrderDto[]> orders = orderIds.Traverse(id => GetOrder(id));

// Split into successes and errors without short-circuiting
var (succeeded, failed) = results.Partition();

// Stop at first failure
Result pipeline = new[] { CheckStock(), ReserveItem(), CreateOrder() }
    .FirstFailureOrSuccesses();
```

| Method | Strategy | Returns |
|--------|----------|---------|
| `CombineAll()` | Collect all errors | `Result` or `Result<T[]>` |
| `Sequence()` | Collect all | `Result<T[]>` with all values or all errors |
| `Traverse(selector)` | Map then sequence | `Result<TOut[]>` |
| `Partition()` | Split | `(T[] Successes, Error[] Errors)` |
| `FirstFailureOrSuccesses()` | Short-circuit | `Result` or `Result<T[]>` |

### TapIf — Conditional Side Effects

```csharp
Result<int> result = Result<int>.Success(42);

// Predicate-based tap — action fires only when Success AND predicate is true
result.TapIf(v => v > 10, v => logger.Log($"Large value: {v}"));

// Bool condition
result.TapIf(featureEnabled, v => Audit(v));

// Async variants
await result.TapIfAsync(v => v > 10, async v => await AuditAsync(v));
await result.TapIfAsync(isEnabled, async v => await TrackAsync(v));

// Works on Task<Result<T>> and ValueTask<Result<T>>
await GetResultAsync().TapIfAsync(v => v > 0, v => Enqueue(v));
```
