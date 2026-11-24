# CSharpEssentials.Results

Functional Result pattern for explicit success/failure handling in .NET, without exceptions for control flow.

`Result` and `Result<T>` are value types with implicit conversions from values and errors. `Result<T, E1..E3>` puts the error types in the signature for exhaustive matching with C# 15 ([Typed errors](#typed-errors-resultt-e1-resultt-e1-e2-resultt-e1-e2-e3)). Operations chain with `Then`, `Bind`, `Map` and `Ensure`, a failure can carry several errors, every operation has async overloads, and `Result.Try`, `Result.TryAsync` and `TryCatch` turn exceptions into errors.

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Errors` and `CSharpEssentials.Json`.

Benchmarks against CSharpFunctionalExtensions, ErrorOr and FluentResults are in [BENCHMARKS.md](./BENCHMARKS.md). On net10, Map, Bind, Tap and Match allocate 0 B, like CSharpFunctionalExtensions and ErrorOr (FluentResults 88-344 B). A failure allocates 56 B (ErrorOr 88 B; CSharpFunctionalExtensions 0 B with its string error), and the async pipeline's failure path allocates 536 B vs 480 B for CSharpFunctionalExtensions. Error aggregation takes 99.09 ns / 336 B vs 42.79 ns / 184 B for ErrorOr's manual-list idiom. The package sets `IsAotCompatible` on its .NET 9+ targets and brings CSharpEssentials.Errors, Json, Core and Enums (CSharpFunctionalExtensions has no dependencies).

## Installation

```bash
dotnet add package CSharpEssentials.Results
```

## Usage

### Creating results

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

### Returning failures

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

`Result<T>` does not accept `null`: `Result<string> r = (string?)null;` throws `ArgumentNullException`. A `bool` converts to `Result` too: `true` is a success and `false` is the `Error.False` failure. `default(Result)` and `default(Result<T>)` are successes; the `Value` of `default(Result<T>)` is `default(TValue)` (`null` for reference types) even though the nullability annotations say it is set, so call `EnsureNotNull` on results that may come from an uninitialized field or array slot. A failure must hold at least one error: `Result.From` and `Result.Failure` throw `ArgumentException` for an empty collection. Results serialize with `System.Text.Json` (an `errors` array and, for `Result<T>`, a `Value`) and deserialize back to the same state.

Use the explicit factories when the compiler cannot infer the target type, for example with `var`, in a lambda whose return type is inferred, or as a generic argument:

```csharp
var conflict = Result.Failure<string>(Error.Conflict("User.Duplicate", "User already exists."));
Task<Result<int>> pending = Task.FromResult(Result.Success(42));
```

### Inspecting a result

```csharp
Result<int> result = GetUser(10);

if (result.IsSuccess)
{
    int value = result.Value;
}
else
{
    Error first = result.FirstError;
    Error last = result.LastError;
    Error[] all = result.Errors;
}

var (isSuccess, value2, errors) = result;   // Result<T>; Result deconstructs to (isSuccess, errors)
```

`Value` is `default` on a failure. `Errors`, `FirstError` and `LastError` on a success return the sentinel errors `Error.NoErrors`, `Error.NoFirstError` and `Error.NoLastError`; `ErrorsOrEmptyArray` returns an empty array instead. `TryGet(out value, out errors)` returns `true` for a success. `Result<T>.ToResult()` drops the value, and `Result<T>` values support `==`, `Equals`, LINQ query syntax (`from a in r1 from b in r2 select a + b`) and `r1 + r2` (a `Result<T[]>` that collects every value or every error).

`ToString()` is for logs. A success prints `Success: ` plus the value's own `ToString()`, formatted with `CultureInfo.InvariantCulture` when the value is `IFormattable`, so a class without a `ToString` override prints its type name. Earlier versions printed JSON; for JSON, call `result.Value.ConvertToJson()` from `CSharpEssentials.Json`.

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

`Then` takes either a plain function (`Func<T, U>`) or a function that returns a `Result<U>`. `Bind` takes a `Result`-returning function and `Map` a plain one (a `Result`-returning `Map` overload exists too). `ThenDo` and `Tap` run a side effect and return the same result. On a failure these operations skip the handler and pass the errors through.

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

GetUser(10).Switch(
    onSuccess: user => Console.WriteLine($"User: {user}"),
    onError: errors => Console.WriteLine($"Failed: {errors.Length} error(s)"));
```

`MatchFirst` and `MatchLast` (and `SwitchFirst`, `SwitchLast`) hand the handler a single `Error` instead of the array. On a non-generic `Result` the handlers take no value and the failure handler parameter is named `onFailure`.

### Combining results

```csharp
var r1 = Result.Success();
var r2 = Result.Success();
Result r3 = Error.NotFound();

Result combined = Result.And(r1, r2, r3); // Failure: returns r3's errors; Result.And stops at the first failure
Result all = new[] { r1, r2, r3 }.CombineAll(); // Failure: collects the errors of every failing result
```

### Safe execution

```csharp
Result<int> result = Result.Try(() => int.Parse("not a number"), ex => Error.Exception(ex));
```

`Result.TryAsync` converts exceptions to errors the same way, but when the `CancellationToken` you pass is cancelled it lets the `OperationCanceledException` propagate instead of turning it into an `Error`. An `OperationCanceledException` that did not come from your token (for example an `HttpClient` timeout) is still passed to the error handler.

### Mapping errors

```csharp
// Per-error mapper: runs once for every error, in order. Never called on success.
Result<User> result = GetUser(id)
    .MapError(e => Error.Failure($"Users.{e.Code}", e.Description));

// Array mapper: replaces the whole error array.
Result<User> single = GetUser(id)
    .MapError(errors => [Error.Failure("Users.Failed", $"{errors.Length} error(s)")]);
```

`MapError(Func<Error, Error>)` used to map only `FirstError` and drop the rest; it now maps every error.

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

### Operations reference

Handlers run only on the side named in the table. The `Async` counterparts are under [Async](#async).

| Operation | Runs on | Behaviour |
|---|---|---|
| `Then`, `Bind`, `Map` | success | Transform or chain the value; on a non-generic `Result` the handlers take no argument. `Select` and `SelectMany` exist on `Result<T>` only. |
| `BindIf(condition, func)` | success | Bind only when the `bool`, `Func<bool>` or `Func<T, bool>` condition holds. |
| `ThenDo`, `Tap`, `TapIf` | success | Side effect; `Tap(condition, action)` takes a `bool` or `Func<bool>`, `TapIf` (`Result<T>` only) a `Func<T, bool>` predicate. The result is returned unchanged. |
| `Ensure(predicate, error)` | success | `Result<T>` only. Fail with the error, or with `errorFactory(value)`, when the predicate is `false`. `EnsureNotNull` fails on a `null` value. |
| `FailIf(predicate, error)` | success | `Result<T>` only. The opposite of `Ensure`: fail when the predicate is `true`. |
| `ThenEnsure(validator)` | success | `Result<T>` only. Run a validator that returns a `Result` or a `Result<T>`; its failure becomes the result. |
| `MapError` | failure | Rewrite the errors; see [Mapping errors](#mapping-errors). |
| `Else(...)` | failure | Replace the failure with an `Error`, or with what a function of the `Error[]` returns: an `Error`, an `Error[]` (`IEnumerable<Error>` on `Result`) or, on `Result<T>`, a `TValue`. `Result<T>` also takes a fallback `TValue` directly. |
| `ElseDo`, `ElseDoFirst`, `TapError`, `TapErrorFirst`, `TapErrorIf` | failure | Side effect with the `Error[]` or the first `Error`; the failure is returned unchanged. |
| `Compensate`, `CompensateFirst` | failure | Replace the failure with the result of a function of the errors or the first error. |
| `Recover(errorType, recovery)`, `Recover(predicate, recovery)`, `RecoverFirst` | failure | `Result<T>` only. Replace the failure with a value or `Result<T>` when an error has the given `ErrorType` or matches the predicate. |
| `Match`, `MatchFirst`, `MatchLast`, `Switch`, `SwitchFirst`, `SwitchLast` | both | Consume the result. |
| `Finally(func)` | both | Receive the whole result and return a value (or run an action and return the result). |
| `GetValueOrDefault()`, `GetValueOrDefault(value)`, `GetValueOrDefault(factory)`, `UnwrapOrDefault(value)` | both | `Result<T>` only. The value, or a fallback on failure. |
| `GetValueOrThrow(message)`, `GetValueOrThrow(exception)`, `Unwrap()` | both | `Result<T>` only. The value, or throw: `InvalidOperationException`, your exception, or `ResultUnwrapException` with the `Errors`. |
| `TryCatch(func, error)` | success | Run `func`; an exception becomes `error`, or `Error.Exception(ex)` when none is given. |

#### Compensating a failure

`Else` swaps the failure for an error or a value. `Compensate` runs a function that returns a new `Result`, so the fallback can fail too. `CompensateFirst` passes only the first error, which is enough to choose a fallback by `ErrorType`:

```csharp
Result<Config> config = LoadConfig(primaryPath)
    .Compensate(errors => LoadConfig(fallbackPath));

Result<User> user = await GetCachedUserAsync(id, ct)
    .CompensateFirstAsync(error => error.Type == ErrorType.NotFound
        ? LoadUserAsync(id, ct)
        : Task.FromResult<Result<User>>(error), ct);
```

A successful result skips the function. `CompensateAsync` and `CompensateFirstAsync` take `Task` or `ValueTask` sources and functions; see the [overload matrix](#async-overload-matrix).

Static helpers on `Result`:

```csharp
Result a = Result.SuccessIf(age >= 18, Error.Validation("Age.Minor", "Must be 18 or older."));
Result b = Result.FailureIf(() => items.Count == 0, Error.Validation("Items.Empty", "No items."));
Result<int> c = Result.SuccessIf(age >= 18, age, Error.Validation("Age.Minor", "Must be 18 or older."));
Result<(int, string)> pair = Result<int>.Combine(Result.Success(1), Result.Success("a"));   // up to 8 results, collects every error
Result either = Result.Or(r1, r2);   // success as soon as one succeeds, otherwise every error
```

`Result.From` and `Result.Failure` build failures from one or more errors; an empty error list throws. `ToResult()` extension methods turn an `Error`, an `Error[]`, an `IEnumerable<Error>` or a plain value into a `Result` or `Result<T>`. On .NET 10 and later `result.ValueOrDefault` and `left | right` (the left result when it succeeded, otherwise the right) are available as extension members. `ResultComparer`, `ResultComparer<T>` and `ResultBaseComparer` in `CSharpEssentials.ResultPattern.Comparers` are `IEqualityComparer` implementations, and `TrimStart(prefix)`/`TrimEnd(suffix)` on `string` return a `Result<string>` that fails when the input or the prefix/suffix is empty.

### Async

```csharp
Result<UserDto> result = await FetchUserAsync(id)
    .ThenAsync(user => EnrichAsync(user))
    .EnsureAsync(dto => IsValidAsync(dto), Error.Validation("Invalid", "Not valid."));
```

Every member that returns `Task` or `ValueTask` ends in `Async`: `BindAsync`, `ElseAsync`, `FailIfAsync`, `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `SwitchFirstAsync`, `SwitchLastAsync`, `ThenAsync` and `ThenDoAsync`. The old unsuffixed names on `Task`/`ValueTask` sources (`task.Match(...)`, `task.Then(...)`) and the instance `Bind` overloads that take a `Task`/`ValueTask` function still compile as `[Obsolete]` forwarders, as do the `Bind` members on `IResult`/`IResult<T>`; see [the async naming migration](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v6-async-naming.md). Use `BindAsync` for async lambdas: `result.Bind(async v => ...)` is ambiguous (CS0121). The `CancellationToken` is optional on every `Task` source.

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

`ThenAsync` is an alias of `BindAsync`. A `Task<Result>` source has no `ValueTask` handler.

`Task` and `ValueTask` sources also have `BindIfAsync`, `SelectAsync`, `TryCatchAsync` and `GetValueOrDefaultAsync`, which are not part of the matrix.

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

#### Lambda binding and C# 12

On .NET 9+, a lambda that returns `ValueTask` binds the `ValueTask` handler, and an untyped `async` lambda binds the `Task` handler through `OverloadResolutionPriority`, which needs C# 13 or later. With `LangVersion` 12 on a .NET 9+ target, an untyped `async` lambda is ambiguous (CS0121) on every operation marked ¹, `TraverseAsync` included. Move to C# 13, or type the handler: `(Func<int, Task<string>>)(async v => ...)` or a local function. Two more shapes need a typed handler on C# 12, on every target including `netstandard2.1`:

- `result.MapErrorAsync(async e => ...)`: the per-error `Func<Error, Task<Error>>` and the array `Func<Error[], Task<Error[]>>` mappers both fit.
- `result.BindAsync(async v => ...)` on a `Result<T>`.

On C# 13 or later both compile. On a `Result<T>`, `MapErrorAsync` matches an untyped `async` lambda on what its body returns: a body that returns an `Error`, including `return e;`, binds the per-error mapper, so `async errors => errors` receives a single `Error`, and a body that returns a new `Error[]` binds the array mapper. Cast to `Func<Error[], Task<Error[]>>` when the array mapper must be explicit.

#### Handlers that are not awaited

Upgrading from 6.x changes what some `TapAsync`, `MapAsync`, `SwitchFirstAsync`/`SwitchLastAsync` and `FinallyAsync` call sites bind to, mostly so that `async` lambdas are now awaited; the list is in [the async naming migration](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v6-async-naming.md). These shapes still bind a sync `Action` and do not await the handler:

- A non-`async` lambda that returns a `Task` on a `ValueTask` source still binds `Action<T>` for `TapAsync` and the conditional `TapAsync`, so the task is not awaited. Use an `async` lambda or a `ValueTask` handler.
- A non-`async` lambda that returns a `ValueTask` on a `Task` source, such as `taskResult.TapAsync(x => SaveValueTaskAsync(x))` or `taskResult.TapAsync(isNew, x => SaveValueTaskAsync(x))`, still binds `Action<T>`, so the `ValueTask` is discarded. Use an `async` lambda or a `Task` handler.
- A non-`async` lambda that returns `ValueTask<T>` on a `ValueTask` source still binds `Action<T>` for `TapAsync` and the conditional `TapAsync`, so the `ValueTask<T>` is discarded: unlike `Task<T>` on a `Task` source, `ValueTask<T>` does not convert to `ValueTask`. Use an `async` lambda.
- The sync `Tap(condition, action)` on a `Result`/`Result<T>` instance takes only `Action`, so `result.Tap(isNew, async () => await SaveAsync())` still runs as `async void`. Check the condition inside an awaited handler instead: `result.TapAsync(() => isNew ? SaveAsync() : Task.CompletedTask)`.

### Collection extensions

```csharp
// Collect ALL errors from multiple results
Result validation = new[] { ValidateName(input), ValidateEmail(input), ValidateAge(input) }
    .CombineAll();

// Map each item and collect: success array or all errors
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
| `SequenceAsync(ct)` | Await already-started tasks, collect all | `Task<Result<T[]>>` or `ValueTask<Result<T[]>>` |

#### TraverseAsync and SequenceAsync

`TraverseAsync` runs an async selector over a sequence sequentially: each item is awaited before the next one starts, so side effects keep their order and a non-thread-safe resource such as a `DbContext` is safe to use. Like `Traverse`, it never short-circuits: every failure is collected and the errors come back in input order.

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
- Overloads: `Func<T, Task<Result<U>>>` and `Func<T, CancellationToken, Task<Result<U>>>` on all targets; the `ValueTask` selector twins are available on .NET 9+. `SequenceAsync` accepts `IEnumerable<Task<Result<T>>>` and `IEnumerable<ValueTask<Result<T>>>`.

### TapIf: conditional side effects

```csharp
Result<int> result = 42;

// Runs only on success when the predicate is true
result.TapIf(v => v > 10, v => logger.LogInformation("Large value: {Value}", v));

// Async variants
await result.TapIfAsync(v => v > 10, async v => await AuditAsync(v));
await result.TapIfAsync(isEnabled, async v => await TrackAsync(v));

// Works on Task<Result<T>> and ValueTask<Result<T>>
await GetResultAsync().TapIfAsync(v => v > 0, v => Enqueue(v));
```

## Typed errors: `Result<T, E1>`, `Result<T, E1, E2>`, `Result<T, E1, E2, E3>`

Matching on these types needs the C# 15 compiler. A net11.0 project gets C# 15 by default with the .NET 11 SDK. A project that targets net10.0, net9.0, net8.0 or netstandard2.1 must build with the .NET 11 SDK and set `<LangVersion>15</LangVersion>`; without it a type pattern over the result fails with CS9327 (C# 14), CS9260 (C# 13) or CS8400 (C# 8). Implicit returns, `Match`, `Switch`, `TryGetValue` and `ToResult` work at any language version.

A typed result holds either the success value or exactly one error, and the error type is part of the signature. It is a `readonly struct` with one value field, one error field and a tag, so `Result<int, NotFoundError, ConflictError>` is 16 bytes and the success path allocates nothing. Up to three error types are supported; use `Result<T>` when a handler can fail in more ways.

```csharp
public static class UserErrors
{
    public static class Codes
    {
        public const string NotFound = "User.NotFound";
        public const string Deleted = "User.Deleted";
        public const string Locked = "User.Locked";
    }

    public static readonly NotFoundError NotFound = new(Codes.NotFound, "User not found.");
    public static readonly NotFoundError Deleted = new(Codes.Deleted, "User was deleted.");

    // An error that callers must see as its own case gets its own type.
    public sealed record Locked(Guid Id) : ConflictError(Codes.Locked, "User is locked.");
}

public Result<UserDto, NotFoundError, UserErrors.Locked> Get(Guid id)
{
    if (deleted) return UserErrors.Deleted;
    if (!exists) return UserErrors.NotFound;
    if (locked) return new UserErrors.Locked(id);
    return dto;
}
```

Values and errors convert implicitly, a predefined `static readonly` error keeps its instance, and a derived error (`UserErrors.Locked`) goes to the slot of its base kind when there is no slot of its own. An error type that is not in the set does not compile (CS0029), and two slots of the same type make every conversion ambiguous (CS0457). The error types come from [CSharpEssentials.Errors](../CSharpEssentials.Errors/Readme.MD#typed-error-kinds).

### Matching

Use a switch expression. Once every case has an arm, the compiler needs no discard and no `null` arm; drop one case and it reports CS8509 naming the missing case.

```csharp
IResult response = Get(id) switch
{
    UserDto dto => Results.Ok(dto),
    NotFoundError { Code: UserErrors.Codes.Deleted } => Results.StatusCode(410),
    NotFoundError => Results.NotFound(),
    UserErrors.Locked locked => Results.Conflict(locked.Id),
};
```

- A `static readonly` field is not a pattern (CS0150), so `case UserErrors.NotFound` does not compile. Tell errors of the same type apart with a constant `Code` property pattern, and keep a bare kind arm after it for exhaustiveness.
- A switch statement gets no exhaustiveness check. One that returns from every case still reports CS0161 until you add `case null:`. In statement code prefer `Match` or `Switch`:

```csharp
int status = Get(id).Match(
    onSuccess: _ => 200,
    onError1: _ => 404,
    onError2: _ => 409);
```

`Match` and `Switch` take one delegate per case, in slot order. `TryGetValue(out T)` and `TryGetValue(out Ei)` read a case without throwing.

### Widening and reordering

A result converts implicitly into a result whose error set contains its own, in any order, so a handler can return the result of a narrower call:

```csharp
Result<UserDto, NotFoundError> Find(Guid id) => ...;

Result<UserDto, NotFoundError, UserErrors.Locked> Get(Guid id) => Find(id);
Result<UserDto, UserErrors.Locked, NotFoundError> reordered = Get(id);
```

### `default`

`default(Result<T, E1, E2>)` holds no case. The compiler cannot catch it, so the members behave as follows:

| Member | On `default` |
|---|---|
| `HasValue` | `false` |
| `TryGetValue(...)` | `false` for every case |
| `Value` | `null` |
| `IsSuccess` / `IsFailure` | `false` / `true` |
| switch expression without a `null` arm | throws `SwitchExpressionException` |
| `Match`, `Switch`, `ToResult` | throw `InvalidOperationException` |
| `IResultBase.Errors`, `FirstError`, `LastError` | an `Unexpected` error with code `Result.Default` |

Create a typed result from a value or an error. Uninitialized fields, `new Result<T, E1>()` and array elements are all `default`.

### Struct error types

Error types may be structs. A struct error is boxed when it is stored, because all slots share one `object` field. `default` of a struct error type is a valid error with whatever its fields default to, often a `null` code; the bridge below turns an `IError` without a code into an `Unexpected` error instead of throwing.

### Converting to `Result<T>`

`ToResult()` returns the classic `Result<T>`, and the typed result also implements `IResultBase`, so Mediator behaviors, `ResultEndpointFilter` and other code written for `Result<T>` keep working. Each error becomes one `Error`:

- An `Error` stays as it is.
- Any other `IError` (every `TypedError` kind) keeps its code, description, type and the same `ErrorMetadata` instance.
- An `Exception` becomes `Error.Exception(exception)`.
- Anything else becomes an `Unexpected` error whose code is the name of the slot type.

`IResultBase.Errors` and the related members never throw, for any slot type. The typed results do not plug into the Mediator failure conversion: when a handler that returns a typed result throws, `ExceptionHandlingBehavior` lets the original exception through unchanged.

## Upgrading to 8.0

### Deserializing a failure without errors

A failed result must carry at least one error, and deserialization now enforces that. JSON with `"IsFailure": true` (for `Result`) or `"IsSuccess": false` (for `Result<T>`) and a missing, `null` or empty `errors` array throws `JsonException`. Earlier versions read `{"IsFailure":true}` as a success and `"errors":[]` as a failure whose `FirstError` and `ToString()` threw.

Payloads written by this library already include the flag and the errors. Check hand-written or third-party payloads: a `Result<T>` success must include `"IsSuccess": true`, because a missing flag reads as `false`, and every failure needs at least one error.

### Cancellation in async operations

Async operations that take a `CancellationToken` and wait on a task source (`MapAsync`, `BindAsync`, `TapAsync`, `MatchAsync`, `EnsureAsync`, `ThenAsync`, `TryAsync` and the rest) wait through Core `WithCancellation`, which is `Task.WaitAsync` on the .NET 9, 10 and 11 targets. When the token is cancelled while they wait, they throw `TaskCanceledException`, a subclass of `OperationCanceledException`, instead of `OperationCanceledException` itself. A `catch (OperationCanceledException)` still catches it; an exact type check no longer matches. The `netstandard2.1` asset is unchanged.
