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

Result combined = Result.And(r1, r2, r3); // Failure: returns r3's errors; Result.And stops at the first failure
Result all = new[] { r1, r2, r3 }.CombineAll(); // Failure: collects the errors of every failing result
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

Every member that returns `Task` or `ValueTask` ends in `Async`: `BindAsync`, `ElseAsync`, `FailIfAsync`, `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `SwitchFirstAsync`, `SwitchLastAsync`, `ThenAsync` and `ThenDoAsync`. The old unsuffixed names on `Task`/`ValueTask` sources (`task.Match(...)`, `task.Then(...)`) and the instance `Bind` overloads that take a `Task`/`ValueTask` function still compile as `[Obsolete]` forwarders and will be removed in 7.0; the `Bind` members on `IResult`/`IResult<T>` are obsolete too. Use `BindAsync` for async lambdas: `result.Bind(async v => ...)` is ambiguous (CS0121). An untyped async lambda binds to the `Task` overload of `BindAsync` through `OverloadResolutionPriority`, which needs C# 13 or later; on C# 12, type the lambda or pass a `Func<..., Task<Result<T>>>` local. The `CancellationToken` is optional on every `Task` source.

#### Async overload matrix

The source decides the handler flavour. A `Task<Result>` source takes sync and `Task` handlers, a `ValueTask<Result>` source takes sync and `ValueTask` handlers, and an instance `Result`/`Result<T>` takes `Task` handlers and, on .NET 9+, `ValueTask` handlers. Handlers that already took a `Task` on a `ValueTask` source are kept.

| Operation | `Result` / `Result<T>` instance | `Task<Result>` / `Task<Result<T>>` source | `ValueTask<Result>` / `ValueTask<Result<T>>` source |
|---|---|---|---|
| `MapAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `ValueTask` |
| `BindAsync` | `Task`, `ValueTask` | sync, `Task` | sync, `ValueTask` |
| `TapAsync` | `Task`, `ValueTask`¹ | `Action`, `Task` | `Action`, `ValueTask` |
| `EnsureAsync` (`Result<T>`) | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `MapErrorAsync` | `Task`, `ValueTask` | sync, `Task` | sync, `ValueTask` |
| `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `SwitchAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `SwitchFirstAsync`, `SwitchLastAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `ElseAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `FailIfAsync` (`Result<T>`) | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `TapErrorAsync`, `TapErrorFirstAsync`, `ElseDoAsync`, `ElseDoFirstAsync` | `Task`, `ValueTask`¹ | `Task` | `Task`, `ValueTask`¹ |
| `CompensateAsync`, `CompensateFirstAsync` | `Task`, `ValueTask`¹ | `Task` | `Task`, `ValueTask`¹ |
| `ThenEnsureAsync` (`Result<T>`) | `Task`, `ValueTask`¹ | `Task` | `Task`, `ValueTask`¹ |
| `FinallyAsync` | | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `Result.TryAsync` (static) | `Task`, `ValueTask`¹ | | |
| `ThenAsync`, `ThenDoAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `TapIfAsync` (`Result<T>`) | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `TraverseAsync` (`IEnumerable<T>`) | `Task` selector, `ValueTask` selector¹ | | |

¹ .NET 9+ assets only. The `netstandard2.1` asset (.NET 8, older hosts) does not have these overloads.

`ThenAsync` stays an alias of `BindAsync`. Nothing crosses flavours: a `Task<Result>` source has no `ValueTask` handler, and the matrix adds no new `Task` handlers to `ValueTask` sources.

```csharp
// Task source: Task handlers, awaited in order
Result<OrderDto> order = await LoadOrderAsync(id)
    .EnsureAsync(o => o.Lines.Count > 0, Error.Validation("Order.Empty", "Order has no lines."))
    .TapAsync(async o => await _audit.WriteAsync(o.Id, ct))
    .MapAsync(o => new OrderDto(o));

// ValueTask source: sync or ValueTask handlers
Result<Price> price = await GetCachedProductAsync(sku)
    .MapAsync(p => _pricing.QuoteAsync(p, ct));   // Func<Product, ValueTask<Price>>
```

On .NET 9+, a lambda that returns `ValueTask` binds the `ValueTask` handler, and an untyped `async` lambda binds the `Task` handler through `OverloadResolutionPriority`. That needs C# 13 or later. With `LangVersion` 12 on a .NET 9+ target, an untyped `async` lambda is ambiguous (CS0121) on every operation marked ¹, including existing ones such as `result.MatchAsync(async ...)` and `vt.ThenAsync(async ...)`. Move to C# 13, or type the handler: `(Func<int, Task<string>>)(async v => ...)` or a local function. Two shapes are ambiguous on every target and language version, so type the handler there too:

- `result.MapErrorAsync(async e => ...)` when the body compiles for both `Error` and `Error[]`, for example `return e;`.
- `result.BindAsync(async v => ...)` on a `Result<T>` when the lambda returns `Result<T>`: the `Result<TOut>` and `Result` handlers both fit.

#### Migrating to the async matrix

Recompiling against this version changes what some existing call sites bind to:

- `TapAsync` on a `Task<Result>`, `Task<Result<T>>`, `ValueTask<Result>` or `ValueTask<Result<T>>` source with an `async` lambda now awaits the handler. It used to bind `Action`/`Action<T>`, which ran it as `async void`: the chain did not wait for it and its exceptions were lost. Exceptions from the handler now propagate.
- `MapAsync` on a `ValueTask<Result<T>>` source with an `async` lambda now returns `ValueTask<Result<U>>`. It used to return `ValueTask<Result<Task<U>>>`.
- `TapAsync` on a `Task<Result>` or `Task<Result<T>>` source with a non-`async` lambda that returns a `Task`, such as `taskResult.TapAsync(x => SaveAsync(x))`, now awaits that task. It used to bind `Action<T>` and discard it.
- On .NET 9+, a lambda that returns `ValueTask` binds the new `ValueTask` handlers instead of a sync or `Action` overload.
- 6.5.0, on .NET 9+: `SwitchFirstAsync`/`SwitchLastAsync` on a `ValueTask` source with two non-`async` lambdas that return a `ValueTask` now await them. They used to bind the `Action` pair and discard them.
- 6.5.0, on .NET 9+: `FinallyAsync` on a `ValueTask` source with a lambda that returns `ValueTask`, `ValueTask<T>` or `Task<T>`, or an `async` lambda that returns a value, now awaits the handler. The result is `ValueTask<Result..>` for `ValueTask` and `Task<T>` handlers and `ValueTask<T>` otherwise; it used to be the nested `ValueTask<ValueTask..>` or `ValueTask<Task<T>>` from the `Func<.., TOut>` overload.
- 6.5.0, every target: the conditional `TapAsync(condition, action)` on a `Task`/`ValueTask` source, with a `bool` or `Func<bool>` condition, now awaits an `async` lambda, such as `taskResult.TapAsync(isNew, async v => await SaveAsync(v))`. It used to bind `Action`/`Action<T>` and run as `async void`, so exceptions from the handler were lost; they now propagate. A non-`async` lambda that returns a `Task` on a `Task` source, or a `ValueTask` on a `ValueTask` source, is now awaited too; it used to be discarded.

Unchanged:

- A non-`async` lambda that returns a `Task` on a `ValueTask` source still binds `Action<T>` for `TapAsync` and the conditional `TapAsync`, so the task is not awaited. Use an `async` lambda or a `ValueTask` handler.
- A non-`async` lambda that returns a `ValueTask` on a `Task` source, such as `taskResult.TapAsync(x => SaveValueTaskAsync(x))` or `taskResult.TapAsync(isNew, x => SaveValueTaskAsync(x))`, still binds `Action<T>`, so the `ValueTask` is discarded. Use an `async` lambda or a `Task` handler.
- A non-`async` lambda that returns `ValueTask<T>` on a `ValueTask` source still binds `Action<T>` for `TapAsync` and the conditional `TapAsync`, so the `ValueTask<T>` is discarded: unlike `Task<T>` on a `Task` source, `ValueTask<T>` does not convert to `ValueTask`. Use an `async` lambda.
- The sync `Tap(condition, action)` on a `Result`/`Result<T>` instance takes only `Action`, so `result.Tap(isNew, async () => await SaveAsync())` still runs as `async void`. Check the condition inside an awaited handler instead: `result.TapAsync(() => isNew ? SaveAsync() : Task.CompletedTask)`.

Source compatibility: a project that targets .NET 9+ and pins `LangVersion` 12 gets CS0121 for untyped `async` lambdas on `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `EnsureAsync`, `TapIfAsync`, `ThenAsync`, `ThenDoAsync` and `TraverseAsync`, which all compiled before, and since 6.5.0 on `ElseAsync`, `FailIfAsync`, `SwitchFirstAsync`, `SwitchLastAsync`, `TapErrorAsync`, `TapErrorFirstAsync`, `ElseDoAsync`, `ElseDoFirstAsync`, `CompensateAsync`, `CompensateFirstAsync`, `ThenEnsureAsync`, `FinallyAsync` (`ValueTask` sources) and `Result.TryAsync`. Use C# 13 or later, or type the handler.

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
