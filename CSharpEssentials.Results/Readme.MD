# CSharpEssentials.Results

Functional Result pattern for explicit success/failure handling in .NET. No exceptions for control flow.

## Features

- **Result & Result<T>**: Value-type results with implicit conversions from values and errors.
- **Railway Oriented Programming**: Chain operations with `Then`, `Bind`, `Map`, `Ensure`.
- **Error Aggregation**: Collect multiple validation errors in one result.
- **Async Support**: Full async/await across all extension methods.
- **Safe Execution**: `TryCatch` to wrap exceptions into results.

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

Return the `Error` itself. `Error`, `Error[]`, `List<Error>` and `HashSet<Error>` convert implicitly to `Result` and `Result<T>`, and a `T` value converts to `Result<T>`:

```csharp
// Single error
Result fail = Error.Validation("Input.Invalid", "Input was invalid.");
Result<string> notFound = Error.NotFound("User.NotFound", "User not found.");

// Multiple errors in one failure
Result<int> multiError = new[]
{
    Error.Validation("Name.Empty", "Name is required."),
    Error.Validation("Email.Invalid", "Email is invalid.")
};
```

Implicit conversion is the idiomatic style. Use the explicit factories when the compiler cannot infer the target type, for example with `var`, in a lambda whose return type is inferred, or as a generic argument:

```csharp
var conflict = Result.Failure<string>(Error.Conflict("User.Duplicate", "User already exists."));
Task<Result<int>> pending = Task.FromResult(Result.Success(42));
```

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
Result r3 = Error.NotFound();

Result combined = Result.And(r1, r2, r3); // Failure
```

### Safe Execution

```csharp
Result<int> result = Result.Try(() => int.Parse("not a number"), ex => Error.Exception(ex));
```

`Result.TryAsync` converts exceptions to errors the same way, but when the `CancellationToken` you pass is cancelled it lets the `OperationCanceledException` propagate instead of turning it into an `Error`. An `OperationCanceledException` that did not come from your token (for example an `HttpClient` timeout) is still passed to the error handler.

### Mapping Errors

```csharp
// Per-error mapper: runs once for every error, in order. Never called on success.
Result<User> result = GetUser(id)
    .MapError(e => Error.Failure($"Users.{e.Code}", e.Description));

// Array mapper: replaces the whole error array.
Result<User> single = GetUser(id)
    .MapError(errors => [Error.Failure("Users.Failed", $"{errors.Length} error(s)")]);
```

> **Behaviour change:** `MapError(Func<Error, Error>)` used to map only `FirstError` and drop the rest. It now maps every error and keeps the order.

`MapErrorAsync` takes `Task` or `ValueTask` mappers in the same two shapes. Per-error async mappers run one at a time, in order. The `CancellationToken` is checked before each mapper call; a mapper that is already running is not abandoned, so pass the token into the mapper if it must stop early. An async lambda (`async e => ...`) binds to the `Task` overload.


```csharp
// Instance: async mapper on a Result / Result<T>
Result<User> localized = await result.MapErrorAsync(e => LocalizeAsync(e, ct), ct);

// Task<Result<T>> source: sync or Task mappers
Result<User> mapped = await GetUserAsync(id)
    .MapErrorAsync(e => Error.Failure($"Users.{e.Code}", e.Description));

// ValueTask<Result<T>> source: sync or ValueTask mappers
Result<User> fromCache = await GetCachedUserAsync(id)
    .MapErrorAsync(errors => CollapseAsync(errors));
```

### Async

```csharp
Result<UserDto> result = await FetchUserAsync(id)
    .ThenAsync(user => EnrichAsync(user))
    .EnsureAsync(dto => IsValidAsync(dto), Error.Validation("Invalid", "Not valid."));
```

Every member that returns `Task` or `ValueTask` ends in `Async`: `BindAsync`, `ElseAsync`, `FailIfAsync`, `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `SwitchFirstAsync`, `SwitchLastAsync`, `ThenAsync` and `ThenDoAsync`. The old unsuffixed names on `Task`/`ValueTask` sources (`task.Match(...)`, `task.Then(...)`, `result.Bind(async v => ...)`) still compile as `[Obsolete]` forwarders and will be removed in 7.0. An async lambda passed to `BindAsync` binds to the `Task` overload, and the `CancellationToken` is optional on every `Task` source.

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
| `TraverseAsync(selector, ct)` | Async map then sequence, one item at a time | `Task<Result<TOut[]>>` |
| `SequenceAsync(ct)` | Await already-started tasks, collect all | `Task<Result<T[]>>` |

#### TraverseAsync and SequenceAsync

`TraverseAsync` runs an async selector over a sequence **sequentially**: each item is awaited before the next one starts, so side effects keep their order and a non-thread-safe resource such as a `DbContext` is safe to use. Like `Traverse`, it never short-circuits: every failure is collected and the errors come back in input order.

```csharp
Result<OrderDto[]> orders = await orderIds.TraverseAsync(
    (id, ct) => GetOrderAsync(id, ct),
    cancellationToken);

// Tasks that are already running: every task is awaited, and all errors are collected
Result<int[]> values = await new[] { LoadA(), LoadB(), LoadC() }.SequenceAsync();
```

- Cancellation throws `OperationCanceledException`; it is never turned into an error. The token is checked before each item, and `(item, ct)` selectors receive it.
- Selector exceptions propagate. They are not converted to errors.
- `SequenceAsync` awaits every task, even after a failed `Result` or a faulted task. Only the first exception is rethrown, after all tasks have completed (like `Task.WhenAll`).
- On cancellation `SequenceAsync` stops waiting at once. The remaining tasks of a materialized collection (`ICollection<T>`/`IReadOnlyCollection<T>`) are observed, so a later fault does not surface as an unobserved task exception. Lazy sources are not enumerated further, because that would start new work.
- On .NET 9+ with `LangVersion` 12, untyped `async` lambdas can be ambiguous between the `Task` and `ValueTask` overloads (CS0121), the same as `MapErrorAsync`. Use `LangVersion` 13+ or a typed delegate.
- Overloads: `Func<T, Task<Result<U>>>` and `Func<T, CancellationToken, Task<Result<U>>>` on all targets; the `ValueTask` selector twins are available on .NET 9+. `SequenceAsync` accepts `IEnumerable<Task<Result<T>>>` and `IEnumerable<ValueTask<Result<T>>>`.

### TapIf: Conditional Side Effects

```csharp
Result<int> result = 42;

// Predicate-based tap — action fires only when Success AND predicate is true
result.TapIf(v => v > 10, v => logger.LogInformation("Large value: {Value}", v));

// Async variants
await result.TapIfAsync(v => v > 10, async v => await AuditAsync(v));
await result.TapIfAsync(isEnabled, async v => await TrackAsync(v));

// Works on Task<Result<T>> and ValueTask<Result<T>>
await GetResultAsync().TapIfAsync(v => v > 0, v => Enqueue(v));
```
